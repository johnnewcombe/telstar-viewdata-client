using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using TelstarClient.Comms;
using TelstarClient.Models;
using Char = TelstarClient.Models.Char;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {
    private DisplayManager.DisplayManager _displayManager;
    private List<Char> _displayManagerData;

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
            _tcp.OnDataRecievedEvent += OnRecieved;
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
    private void OnRecieved(string data) {
        //Debug.Print("Recv<={0}", data);

        // add data to the display
        foreach (var c in data) {
            
            // print char returns a Tuple which is used to bind to a cell
            // in the UI
            var dData = _displayManager.PrintChar(c);
            
            if (dData is not null) {

                        // if we get a NULL character e.g. character 0, then do nothing
                        // this happens if we have just sent a control code e.g. 00 to 1F
                        // to be printed.

                        // updating this property will invoke the OnPropertyChanged event
                        // to update the view
                        DisplayManagerData = dData;

            }
        }
    }

    public void Disconnect() {
        _tcp.Disconnect();
    }

    public void Send(string data) {
        if (_tcp.Write(data)) {
            //Debug.Print("Sent=>{0}", data);
        }
    }

    public List<Char> DisplayManagerData {
        set {
            _displayManagerData = value;
            OnPropertyChanged(nameof(DisplayManagerData));
        }

        get { return _displayManagerData; }
    }

// implmentation of INotify for properties
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyDisplayData) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyDisplayData));
    }
}