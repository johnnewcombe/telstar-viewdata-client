using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using TelstarClient.Comms;
using TelstarClient.Configuration;
using TelstarClient.Display;
using TelstarClient.Extensions;

namespace TelstarClient.ViewModels;

public class MainWindowViewModel : ViewModelBase {

    private const string ConnectedStatus = "CONNECTED";
    private const string DisconnectedStatus = "DISCONNECTED";
    private const string ErrorStatus = "UNABLE TO CONNECT";
    private const string ConnectingStatus = "CONNECTING";
    private readonly string _appSupportDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                         Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName +
                                         Path.DirectorySeparatorChar;
    private const string ConfigFile = "config.json";

    private string _status;
    private bool _menu;
    private bool _keyCtrl;
    private readonly Settings _settings;
    private readonly DisplayManager _displayManager;
    private readonly CyclicBuffer _cyclicBuffer;
    private readonly KeyMapper _keyMapper;
    private readonly TCPClient _tcp;

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel() {
        DisplayWelcomeMessage();
        
        var configFile = _appSupportDirectory + ConfigFile;

        // create the app suport directory if it doesn't exist
        if (!Directory.Exists(_appSupportDirectory)) {
            // create directory
            Directory.CreateDirectory(_appSupportDirectory);
        }

        _displayManager = new DisplayManager();
        _settings = new Settings(configFile);
        _keyMapper = new KeyMapper();
        _cyclicBuffer = new CyclicBuffer(2048);
        
        _tcp = new TCPClient();
        _tcp.OnConnectEvent += OnConnect;
        _tcp.OnDataReceivedEvent += OnReceived;
        
    }

    public async Task DisplayWelcomeMessage() {
        await Task.Delay(100);

        _displayManager.Write(Display.MainMenu.GetLogo());
        _displayManager.Display.SetStatusText(DisconnectedStatus);

        OnPropertyChanged(nameof(DisplayData));
    }

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
            Debug.WriteLine($"Error : {ex}");
        }
    }

    public void Disconnect() {
        if (_tcp is not null) {
            _tcp.Disconnect();
        }

        _displayManager.Display.SetStatusText(DisconnectedStatus);
        OnPropertyChanged(nameof(DisplayData));
    }

    public void KeyHandler(KeyEventArgs e) {

        // if connected then help is available also
        if (_tcp.IsConnected()) {
            
            // control char ?
            if (e.Key == Key.LeftCtrl) {
                _keyCtrl = true;
                return;
            }

            if (_keyCtrl) {
                // previous char was a ctrl            
                _keyCtrl = false;
                switch (e.KeySymbol.ToLower()) {
                    case "c":
                        _tcp.Disconnect();
                        DisplayMenu();
                        break;
                    case "h":
                        // TODO save current screen and put it back
                        // maybe a second cache buffer in the Display object?
                        DisplayHelp();
                        break;
                }

            }
            
            if (e.KeySymbol == null) {
                return;
            }
            
            var keySymbol = _keyMapper.Map(e.KeySymbol);

            if (_tcp.Write(keySymbol)) {
                //Trace.Print("Sent=>{0}", data);
            }
        }
        else if (!_menu) { // i.e. any key presses when menu not shown
            DisplayMenu();
        }
        else {

            // key press is a string, so convert to int, get the appropriate
            // connection details and connect
            if(int.TryParse(e.KeySymbol, out var index)) {
                
                if (index >= 0 && index < _settings.config.Connections.Count) {
                    
                    // index-1 as menu is '1' based and collection is '0' based
                    var con = _settings.config.Connections[index-1];
                    if (con.Name is not null) {
                        Connect(con.Address, con.Port);
                    }
                    
                }
            }

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
            _displayManager.Display.SetStatusText(ConnectedStatus);
        }
        else {
            _displayManager.Display.SetStatusText(ErrorStatus);
            // delay
            Thread.Sleep(750);
            _displayManager.Display.SetStatusText(DisconnectedStatus);
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

    private void DisplayHelp() {

        MainMenu.GetHelp();
        _displayManager.Display.Clear();
        _displayManager.SetCursorPosition(0, 0);
        _displayManager.Write(Display.MainMenu.GetHelp());

        OnPropertyChanged(nameof(DisplayData)); 
    }

    private void DisplayMenu() {
        
        _menu = true;
        _displayManager.Display.Clear();
        _displayManager.SetCursorPosition(0, 0);
        //_displayManager.Write(Display.MainMenu.GetMenu());

        // update the menu diisplay
        var item = 0;
        var menuSb = new StringBuilder();
        foreach (var connection in _settings.config.Connections) {
            if (connection.Name is not null) {
                item++;
                menuSb.Append($"   \e{Constants.AlphaCyan}{item} \e{Constants.AlphaWhite}{connection.Name}\r\n\n");
            }
        }
        // pop the menu into the placeholder
        _displayManager.Write(Display.MainMenu.GetMenu().Replace(Constants.PlaceHolder,menuSb.ToString()));

        OnPropertyChanged(nameof(DisplayData));

    }

    #endregion

}