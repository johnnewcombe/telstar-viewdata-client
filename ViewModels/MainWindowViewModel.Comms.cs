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
using TelstarClient.Display;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel {

    #region TCP Client Control and Events

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

    public void Connect(string ip, int port) {
        try {

            // open the tcp client
            _cyclicBuffer.Clear();
            _logger.LogInformation("Connecting to {Ip}:{Port}", ip,port);
            _comms.Connect(ip, port);
            Dispatcher.UIThread.Post(UpdateMainDisplay);
            
        }
        catch (Exception ex) {
            // Catch errors in Connection and receive Callbacks
            _logger.LogError(ex, "Failed to connect to {Ip}:{Port}", ip,port);
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