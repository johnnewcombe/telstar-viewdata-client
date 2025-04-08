using System;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Threading;
using TelstarClient.Comms;


namespace TelstarClient.ViewModels;

public class MainWindowViewModel {

    private string _status;
    private readonly Display.DisplayManager _displayManager;
    private Models.Display _displayManagerData;
    private readonly CyclicBuffer _cyclicBuffer = new CyclicBuffer(2048);

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

    public void Connect() {
        try {

            // open the tcp client
            _tcp = new TCPClient("glasstty.com", 6502);
            _tcp.OnConnectEvent += OnConnect;
            _tcp.OnDataReceivedEvent += OnReceived;
            _tcp.Connect();

            Status = "Online";
        }
        catch (Exception ex) {
            // Catch errors in Connection and receive Callbacks
            Debug.Print($"Error : {ex}");
        }
    }

    public void Disconnect() {

        _tcp.Disconnect();
        Status = "Offline";
    }

    public void Send(string data) {

        if (_tcp == null) {
            return;
        }

        if (_tcp.Write(data)) {
            Debug.Print("Sent=>{0}", data);
        }
    }

    // Connection Status Listener
    private void OnConnect(bool status) {

        if (status) {

            Debug.Print("Connected! : " + status.ToString());
        }
    }

    // Data Received Listener
    private void OnReceived(string data) {

        // add data to the cyclic buffer
        foreach (var c in data) {
            _cyclicBuffer.Add(c);
        }
        // at this point we are not on the UI thread but one created by the TCPClient
        // this is a fire and forget call, the TCP Client will not wait for a result
        // Dispatcher.UIThread.Post(ProcessReceiveBuffer);
        Dispatcher.UIThread.Post(ProcessReceiveBuffer);
    }

    #endregion

    #region Data Processing and Notification

    /// <summary>
    /// Method to process the receive buffer. This is called from
    /// the OnDataReceived event but on the UI Thread as a separate
    /// Task.
    /// </summary>
    private void ProcessReceiveBuffer() {

        Debug.Print($"Buffer Size: {_cyclicBuffer.Count}");

        // get data from buffer and process for viewdata 
        while (_cyclicBuffer.Count > 0) {
            if (_displayManager.ProcessChar(_cyclicBuffer.Remove())) {
                // updating this property will invoke the OnPropertyChanged event
                // to update the view
                DisplayManagerData = _displayManager.Display;
            }
        }

        Debug.Print("ProcessReceiveBuffer Exit");
    }

    public string Status {
        get { return _status; }
        set {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public Models.Display DisplayManagerData {
        get { return _displayManagerData; }
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