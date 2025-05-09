using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Threading;
using TelstarClient.Extensions;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel {
    
    #region TCP Client Control and Events

    public void Connect(string ip, int port) {
        try {

            // open the tcp client
            _cyclicBuffer.Clear();
            _tcp.Connect(ip, port);

            _displayManager.Display.SetStatusText(ConnectingStatus);

            OnPropertyChanged(nameof(DisplayData));
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

        if (status) {
            _displayManager.Display.SetStatusText(ConnectedStatus);
        }
        else {
            _displayManager.Display.SetStatusText(ErrorStatus);
            DisplayData = _displayManager.Display.Chars;

            // delay
            Thread.Sleep(750);
            _displayManager.Display.SetStatusText(DisconnectedStatus);
        }

        DisplayData = _displayManager.Display.Chars;
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