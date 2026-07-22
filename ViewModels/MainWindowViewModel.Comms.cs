/*
    Copyright (c) 2025 John Newcombe

    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with the product. If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using TelstarClient.Comms;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel
{
    #region Comms Client Control and Events

    //private Lock _lock = new();
    //private bool _connectStatus;

    /*
    /// <summary>
    /// Thread safe property to allow the connected status to be read
    /// from multiple threads
    /// </summary>
    private bool ConnectStatus
    {
        get
        {
            lock (_lock)
                return _connectStatus;
        }
        set
        {
            lock (_lock)
                _connectStatus = value;
        }
    }
*/
    /// <summary>
    /// Establishes a connection to the specified target, either TCP or Serial.
    /// </summary>
    /// <param name="arg1">The IP address/hostname or serial device name.</param>
    /// <param name="arg2">The TCP port or baud rate.</param>
    /// <param name="parity">A flag indicating if parity is enabled.</param>
    /// <param name="serial">True for a serial connection, false for TCP.</param>
    private void Connect(string arg1, int arg2, bool parity, string initString, bool serial)
    {
        try
        {
            // open the comms client
            if (serial)
            {
                _logger.LogInformation("Connecting to device:{arg1}, baud rate:{arg2} parity:{parity}", arg1, arg2, parity);
                // removing the event stops the Disconnect fiddling with the screen and
                _commsClient.OnConnectEvent -= OnConnectionChange;
                _commsClient.Disconnect();
                _commsClient = _commsClientFactory.Create(CommsClientType.Serial);
            }
            else
            {
                _logger.LogInformation("Connecting to host:{arg1} :port{arg2}, parity:{parity}", arg1, arg2, parity);
                // removing the event stops the Disconnect fiddling with the screen and
                _commsClient.OnConnectEvent -= OnConnectionChange;                _commsClient.Disconnect();
                _commsClient.Disconnect();
                _commsClient = _commsClientFactory.Create(CommsClientType.Tcp);
            }

            _commsClient.OnConnectEvent += OnConnectionChange;
            _commsClient.OnDataReceivedEvent += OnReceived;
            _commsClient.Connect(arg1, arg2, parity);
            _cyclicBuffer.Clear();
            
            // send init
            _commsClient.Write(initString.ToUpper().Replace("^M","\r\n"));

        }
        catch (Exception ex)
        {
            // Catch errors in Connection and receive Callbacks
            _logger.LogError(ex, "Failed to connect to {Ip}:{Port}", arg1, arg2);
        }
    }

    /// <summary>
    /// Disconnects from the current connection.
    /// </summary>
    private void Disconnect()
    {
        _logger.LogInformation("Disconnecting");

        if (_commsClient is not null)
        {
            _commsClient.Disconnect();
        }

        _logger.LogInformation("Disconnected");
    }

    /// <summary>
    /// Listener for connection events. Note that this is 
    /// </summary>
    /// <param name="connected">The new connection status.</param>
    /// <param name="error"></param>
    private void OnConnectionChange(bool connected, string? error)
    {
        
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("Error:{Error}", error);
        }
        else
        {
            _logger.LogInformation("Connected:{Connected}", connected);
        }
        
        // set the thread safe property
        //ConnectStatus = connected;
        
        // switch to UI thread
        Dispatcher.UIThread.Post(() => ConnectionChange(connected, error));   

    }

/// <summary>
/// We only get here 
/// </summary>
    private async void ConnectionChange(bool connected, string error)
    {
        _logger.LogInformation("Connection change, connected:{connected}", connected);

        // TODO create a SetErrorStatus method or combine with SetStatusText
        // display the error if the error text is populated
        if (!string.IsNullOrEmpty(error))
        {
            // we have to do both as we could be on a connect screen or a terminal screen
            _displayManagerMain.SetStatusText(error,"Red");
            _displayManagerAlt.SetStatusText(error,"Red");
            await Task.Delay(2000);
            SetDisplay(DisplayType.Directory);
        }
        
        // we are on the UI thread so update the display
        UpdateConnectStatus();

        if (!connected)
        {
            // we have been disconnected but we don't know if this
            // was an error or due to a user action.
            SetDisplay(DisplayType.Directory);
            
            // clear the display so that all old data is removed
            // for next time we switch to Terminal
            _displayManagerMain.ClearDisplay();
        }
        else
        {
            SetDisplay(DisplayType.Terminal);
        }
    }
    /// <summary>
    /// Listener for data received events.
    /// </summary>
    /// <param name="data">The received data.</param>
    private void OnReceived(string data)
    {
        // add data to the cyclic buffer, this is thread safe
        foreach (var c in data)
        {
            _cyclicBuffer.Add(c);
            _logger.LogDebug("Character received from server:{Hex:X2}h,{Decimal}d", (byte)c, (byte)c);
        }

        // at this point we are not on the UI thread but one created by the TCPClient
        // this is a fire and forget call, the TCP Client will not wait for a result
        // Dispatcher.UIThread.Post(ProcessReceiveBuffer);
        Dispatcher.UIThread.Post(ProcessReceiveBuffer);
    }

    #endregion
}