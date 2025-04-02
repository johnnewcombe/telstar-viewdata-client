using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Threading;
using TelstarClient.Comms;
using TelstarClient.Models;
using Char = TelstarClient.Models.Char;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {
    private DisplayManager.DisplayManager _displayManager;
    private List<Char> _displayManagerData;
    private CyclicBuffer _cyclicBuffer = new CyclicBuffer();

    TCPClient _tcp;

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel() {
        _displayManager = new DisplayManager.DisplayManager();
    }

    public void Connect() {
        try {
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

    // Connection Status Listner
    private void OnConnect(bool status) {
        Debug.Print("Connected! : " + status.ToString());
    }

    // Data Received Listner
    private void OnReceived(string data) {

        foreach (var c in data) {
            _cyclicBuffer.Add(c);
            //TODO: Remove this once buffer is in place.
            var cOut = _cyclicBuffer.Remove(); // added here temporarily to prevent the buffer filling
        }

        // TODO: can we call this on the main UI thread or a separate thread maybe
        //  otherwise there is a risk that incoming data will be missed when doing 
        //  large screen updates e.g. CLS etc.
        ViewdataProcess(data);
    }
    
    private void ViewdataProcess(string data) {
        // add data to the display
        foreach (var c in data) {
            // print char returns a Tuple which is used to bind to a cell
            // in the UI
            var dData = _displayManager.PrintChar(c);

            if (dData.Count>0) {
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

    public void Disconnect() {
        _tcp.Disconnect();
    }

    public void Send(string data) {
        if (_tcp.Write(data)) {
            //Debug.Print("Sent=>{0}", data);
        }
    }
}