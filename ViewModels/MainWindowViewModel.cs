using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Threading;
using TelstarClient.Comms;
using TelstarClient.Configuration;
using TelstarClient.Extensions;
using TelstarClient.Models;

namespace TelstarClient.ViewModels;

public class MainWindowViewModel : ViewModelBase {

    private const string connectedStatus = "CONNECTED";
    private const string disconnectedStatus = "DISCONNECTED";
    private const string errorStatus = "UNABLE TO CONNECT";
    private const string connectingStatus = "CONNECTING";


    private string appSupportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                         Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName +
                                         Path.DirectorySeparatorChar;
    private string configFile;
    private string _status;
    private bool _menu;

    private readonly Display.DisplayManager _displayManager = new Display.DisplayManager();

    //private List<Models.Char> _displayManagerData = new List<Models.Char>();
    private readonly CyclicBuffer _cyclicBuffer = new CyclicBuffer(2048);
    private KeyMapper _keyMapper = new KeyMapper();
    private TCPClient _tcp = new TCPClient();

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel() {
        DisplayWelcomeMessage();
        
        configFile = appSupportDirectory + "config.json";
        
        // create the app suport directory if it doesn't exist
        if (!Directory.Exists(appSupportDirectory)) {
            // create directory
            Directory.CreateDirectory(appSupportDirectory);
        }
    }

    public async Task DisplayWelcomeMessage() {
        await Task.Delay(100);

        _displayManager.Write(Display.MainMenu.GetLogo());
        _displayManager.Display.SetStatusText(disconnectedStatus);

        OnPropertyChanged(nameof(DisplayData));
    }

    #region TCP Client Control and Events

    public void Connect(string ip, int port) {
        try {

            // open the tcp client

            _tcp.OnConnectEvent += OnConnect;
            _tcp.OnDataReceivedEvent += OnReceived;
            _tcp.Connect(ip, port);

            _displayManager.Display.SetStatusText(connectingStatus);

            OnPropertyChanged(nameof(DisplayData));
        }
        catch (Exception ex) {
            // Catch errors in Connection and receive Callbacks
            Debug.WriteLine($"Error : {ex}");
        }
    }

    public void Disconnect() {
        if (_tcp is not null) {
            _tcp.Disconnect();
        }

        _displayManager.Display.SetStatusText(disconnectedStatus);
        OnPropertyChanged(nameof(DisplayData));
    }

    public void KeyHandler(string data) {

        if (_tcp.IsConnected()) {
            // key mapper
            if (data == null || _tcp == null) {
                return;
            }

            data = _keyMapper.Map(data);

            if (_tcp.Write(data)) {
                //Trace.Print("Sent=>{0}", data);
            }
        }
        else if (!_menu) {
            _menu = true;
            _displayManager.Display.Clear();
            _displayManager.SetCursorPosition(0, 0);
            _displayManager.Write(Display.MainMenu.GetMenu());

            // TODO: update the menu
            var settings = new Settings(configFile);
            foreach (var connection in settings.config.Connections) {
                Trace.WriteLine($"{connection.Name} {connection.Address}:{connection.Port}\r\n");
            }


            OnPropertyChanged(nameof(DisplayData));
        }
        else {


            //var iconfig = new Configuration.JsonConfig(configFile);
            //var config = iconfig.GetConnection(data);

            //Trace.WriteLine(config.Address);
            //Trace.WriteLine(config.Port);
            //Connect(config.Address, config.Port);

        }
    }

    // Connection Status Listener
    private void OnConnect(bool status) {

        if (status) {
            _displayManager.Display.SetStatusText(connectedStatus);
        }
        else {
            _displayManager.Display.SetStatusText(errorStatus);
            // delay
            Thread.Sleep(750);
            _displayManager.Display.SetStatusText(disconnectedStatus);
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

        //Debug.Print($"Buffer Size: {_cyclicBuffer.Count}");

        // get data from buffer and process for viewdata 
        while (_cyclicBuffer.Count > 0) {

            if (_displayManager.WriteChar(_cyclicBuffer.Remove())) {

                // updating this property will invoke the OnPropertyChanged event
                // to update the view
                DisplayData = _displayManager.Display.Chars;
            }
        }

        //Debug.Print("ProcessReceiveBuffer Exit");
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