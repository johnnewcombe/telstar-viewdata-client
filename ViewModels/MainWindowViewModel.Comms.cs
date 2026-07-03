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
using System.Threading;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using TelstarClient.Comms;
using TelstarClient.Display;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel {

    #region Comms Client Control and Events

    private Lock _lock = new();
    private bool _connectStatus;

    /// <summary>
    /// Thread safe property to allow the connected status to be read
    /// from multiple threads
    /// </summary>
    public bool ConnectStatus {
        get {
            lock (_lock)
                return _connectStatus;
        }
        set {
            lock (_lock)
                _connectStatus = value;
        }
    }

    /// <summary>
    /// Note that this could be called for serial and tcp connections
    /// </summary>
    /// <param name="arg1">This will be the ip address or hostname
    /// for TCP connections and serial device for serial connections.</param>
    /// <param name="arg2">This will be the tcp port number for TCP
    /// connections or baud rate for serial connections</param>
    /// <param name="serial"></param>
    public void Connect(string arg1, int arg2, bool serial) {
        try {

            // open the comms client
            if (serial)
            {          
                _logger.LogInformation("Connecting to {arg1} at {arg2} baud", arg1,arg2);
                _comms = new SerialClient();
            }
            else
            {
                _logger.LogInformation("Connecting to {arg1} at {arg2} baud", arg1,arg2);
                _comms = new TcpClient();
            }

            _comms.OnConnectEvent += OnConnect;
            _comms.OnDataReceivedEvent += OnReceived;
            _comms.Connect(arg1, arg2);

            _cyclicBuffer.Clear();

            Dispatcher.UIThread.Post(UpdateMainDisplay);
            
        }
        catch (Exception ex) {
            // Catch errors in Connection and receive Callbacks
            _logger.LogError(ex, "Failed to connect to {Ip}:{Port}", arg1,arg2);
        }
    }

    public void Disconnect() {

        _logger.LogInformation("Disconnecting TCP connection");

        if (_comms is not null) {
            _comms.Disconnect();
            
            // set the thread safe property
            ConnectStatus = false;

            // switch to UI thread
            _displayManagerMain.Display.Clear();
            Dispatcher.UIThread.Post(UpdateMainDisplay);
        }
    }

    // Connection Status Listener
    private void OnConnect(bool status) {

        _logger.LogInformation("Connected:{Status}", status);

        // set the thread safe property
        ConnectStatus = status;
        
        // switch to UI thread
        Dispatcher.UIThread.Post(UpdateConnectStatus);
    }


    // Data Received Listener
    private void OnReceived(string data) {

        // add data to the cyclic buffer, this is thread safe
        foreach (var c in data) {
            _cyclicBuffer.Add(c);
        }

        // at this point we are not on the UI thread but one created by the TCPClient
        // this is a fire and forget call, the TCP Client will not wait for a result
        // Dispatcher.UIThread.Post(ProcessReceiveBuffer);
        Dispatcher.UIThread.Post(ProcessReceiveBuffer);
    }

    #endregion

}