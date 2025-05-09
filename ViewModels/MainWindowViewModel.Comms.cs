using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Threading;
using TelstarClient.Extensions;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel {
    
    #region TCP Client Control and Events
    
    private Lock locker = new ();
    private bool _connectStatus;

    public bool ConnectStatus
    {
        get
        {
            lock (locker)
                return _connectStatus;
        }
        set
        {
            lock (locker)
                _connectStatus = value;
        }
    }
    
    public void Connect(string ip, int port) {
        try {

            // open the tcp client
            _cyclicBuffer.Clear();
            _tcp.Connect(ip, port);

            _displayManager.Display.SetStatusText(ConnectingStatus);
            DisplayData = _displayManager.Display.Chars;
        }
        catch (Exception ex) {
            // Catch errors in Connection and receive Callbacks
            Trace.WriteLine($"Error : {ex}");
        }
    }

    public void Disconnect() {

        if (_tcp is not null) {
            _tcp.Disconnect();
        }
    }

    // Connection Status Listener
    private void OnConnect(bool status) {
            ConnectStatus = status;
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