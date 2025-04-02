using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using TelstarClient.Comms;
using TelstarClient.Models;
using Char = TelstarClient.Models.Char;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {
    private DisplayManager.DisplayManager _displayManager;
    private List<Char> _displayManagerData;
    private CyclicBuffer _cyclicBuffer = new CyclicBuffer();
    private CancellationTokenSource _cancellationTokenSource;

    TCPClient _tcp;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel() {
        _displayManager = new DisplayManager.DisplayManager();

    }

    #region TCP Client Control and Events

    public void Connect() {
        try {
            // start a new task to process inclomming data
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(ProcessReceiveBuffer, _cancellationTokenSource.Token);

            // open the tcp client
            _tcp = new TCPClient("glasstty.com", 6502);
            _tcp.OnConnectEvent += OnConnect;
            _tcp.OnDataReceivedEvent += OnReceived;
            _tcp.Connect();
        }
        catch (Exception ex) {
            // Catch errors in Connection and Recieve Callbacks
            Debug.Print("Error : " + ex.ToString());
        }
    }

    public void Disconnect() {
        _tcp.Disconnect();
        //  cancel the data processing task
        _cancellationTokenSource.Cancel();
    }

    public void Send(string data) {
        if (_tcp.Write(data)) {
            //Debug.Print("Sent=>{0}", data);
        }
    }

    // Connection Status Listner
    private void OnConnect(bool status) {
        Debug.Print("Connected! : " + status.ToString());
    }

    // Data Received Listner
    private void OnReceived(string data) {

        foreach (var c in data) {
            _cyclicBuffer.Add(c);
        }

    }

    #endregion

    #region Asynchronous Tasks

    private void ProcessReceiveBuffer() {
        // get data from buffer and process for viewdata  
        while (true) {

            if (_cyclicBuffer.Count > 0) {
                var c = _cyclicBuffer.Remove();
                ViewdataProcess(c.ToString());
            }

            if (_cancellationTokenSource.Token.IsCancellationRequested) {
                Debug.Print("Task Ended!");
                return;
            }
        }
    }

    #endregion

    #region Data Processing and Data Property Notification Events

    private void ViewdataProcess(string data) {
        // add data to the display
        foreach (var c in data) {
            // print char returns a Tuple which is used to bind to a cell
            // in the UI
            var dData = _displayManager.PrintChar(c);

            if (dData.Count > 0) {
                // updating this property will invoke the OnPropertyChanged event
                // to update the view
                DisplayManagerData = dData;
            }
        }
    }

    public List<Char> DisplayManagerData {
        set {
            _displayManagerData = value;
            OnPropertyChanged(nameof(DisplayManagerData));
        }

        get { return _displayManagerData; }
    }

    // implementation of INotify for properties
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyDisplayData) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyDisplayData));
    }

    #endregion
    
}