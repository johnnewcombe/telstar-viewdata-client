using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using TelstarClient.Comms;
using TelstarClient.Extensions;
using TelstarClient.Models;

namespace TelstarClient.ViewModels;

public class MainWindowViewModel : ViewModelBase{
    
    private string _status;
    private readonly Display.DisplayManager _displayManager = new Display.DisplayManager();
    //private List<Models.Char> _displayManagerData = new List<Models.Char>();
    private readonly CyclicBuffer _cyclicBuffer = new CyclicBuffer(2048);
    private Models.KeyMapper _keyMapper = new KeyMapper();
    private TCPClient _tcp;

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel() {
    }

    #region TCP Client Control and Events

    public void Connect() {
        try {

            // open the tcp client
            _tcp = new TCPClient("glasstty.com", 6502);
            _tcp.OnConnectEvent += OnConnect;
            _tcp.OnDataReceivedEvent += OnReceived;
            _tcp.Connect();
            
            _displayManager.SetStatusConnecting();
            OnPropertyChanged(nameof(DisplayData));
        }
        catch (Exception ex) {
            // Catch errors in Connection and receive Callbacks
            Debug.Print($"Error : {ex}");
        }
    }

    public void Disconnect() {
        if (_tcp is not null) {
            _tcp.Disconnect();
        }
        _displayManager.SetStatusOffline();
        OnPropertyChanged(nameof(DisplayData));
    }

    public void Send(string data) {

        // key mapper
        if (data == null || _tcp == null) {
            return;
        }
        
        data = _keyMapper.Map(data);

        if (_tcp.Write(data)) {
            Debug.Print("Sent=>{0}", data);
        }
    }

    // Connection Status Listener
    private void OnConnect(bool status) {

        if (status) {
            _displayManager.SetStatusOnline();        }
        else {
            _displayManager.SetStatusOffline();
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
                DisplayData = _displayManager.Display.Chars;
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

    public List<Models.Char> DisplayData {
        get { return _displayManager.Display.Chars; }
        set {
            _displayManager.Display.Chars = value;
            OnPropertyChanged();
        }
    }

    #endregion

}