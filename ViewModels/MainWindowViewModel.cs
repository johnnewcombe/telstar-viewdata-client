using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using TelstarClient.Comms;
using TelstarClient.Models;
using Char = TelstarClient.Models.Char;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel {

    private string _status;
    private Display.DisplayManager _displayManager;
    private Models.Display _displayManagerData;
    private CyclicBuffer _cyclicBuffer = new CyclicBuffer(2048);
    private CancellationTokenSource _cancellationTokenSource;
    //private static Lock iLock = new Lock();

    private TCPClient _tcp;
    //private TcpClientNew _tcp;

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel() {
        _displayManager = new Display.DisplayManager();
        Status = "Offline";
    }

    #region TCP Client Control and Events

    public async void Connect() {
        try {

            // start a new task to process incoming data
            //_cancellationTokenSource = new CancellationTokenSource();
            //Action process = ProcessReceiveBuffer;
            //var t = Task.Run(process, _cancellationTokenSource.Token);
            //if (t.Exception != null) {
            //    throw t.Exception;
            //}

            // open the tcp client
            _tcp = new TCPClient("glasstty.com", 6502);
            _tcp.OnConnectEvent += OnConnect;
            _tcp.OnDataReceivedEvent += OnReceived;
            _tcp.Connect();

            //open the new client
            //_tcp = new TcpClientNew(_cyclicBuffer);
            //_tcp.Connect();

            Status = "Online";

        }
        catch (Exception ex) {
            // Catch errors in Connection and receive Callbacks
            Debug.Print("Error : " + ex.ToString());
        }
    }

    public void Disconnect() {

        _tcp.Disconnect();

        //  cancel the data processing task
        _cancellationTokenSource.Cancel();

        Status = "Offline";
    }

    public void Send(string data) {

        if (_tcp == null) {
            return;
        }

        //foreach (char c in data) {
        //    _tcp.Send(c);
        //}
        if (_tcp.Write(data)) {
            Debug.Print("Sent=>{0}", data);
        }
    }

    // Connection Status Listener
    private void OnConnect(bool status) {
        Debug.Print("Connected! : " + status.ToString());
    }

    // Data Received Listner
    private void OnReceived(string data) {

        // add data to the cyclic buffer
        foreach (var c in data) {
            _cyclicBuffer.Add(c);
        }

        // at this point we are not on the UI thread but one created by the TCPClient
        // this is a fire and forget call, the TCP Client will not wait for a result
        Dispatcher.UIThread.Post(ProcessReceiveBuffer);

    }

    #endregion

    #region Data Processing and Notification

    /// <summary>
    /// Method to process the receive buffer. This is called from
    /// the OnDataReceived event but on the UI Thread as a seperate
    /// Task.
    /// </summary>
    private void ProcessReceiveBuffer() {

        // get data from buffer and process for viewdata 
        if (_cyclicBuffer.Count > 0) {
            if (_displayManager.ProcessChar(_cyclicBuffer.Remove())) {
                // updating this property will invoke the OnPropertyChanged event
                // to update the view
                DisplayManagerData = _displayManager.Display;
            }
        }
    }


    public string Status {
        get { return _status; }
        set {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public Models.Display DisplayManagerData {
        get {
            return _displayManagerData;
        }
        set {
            _displayManagerData = value;
            OnPropertyChanged(nameof(DisplayManagerData));
        }
    }

    // implementation of INotify for properties
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyDisplayData) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyDisplayData));
    }

    #endregion

}