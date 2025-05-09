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

public partial class MainWindowViewModel : ViewModelBase {

    private const string ConnectedStatus = "CONNECTED";
    private const string DisconnectedStatus = "DISCONNECTED";
    private const string ErrorStatus = "UNABLE TO CONNECT";
    private const string ConnectingStatus = "CONNECTING";

    private readonly string _appSupportDirectory =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
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

    #region Data Processing and Notification

    private async void UpdateConnectStatus() {

        try {
            var status = ConnectStatus;
            if (status) {
                _displayManager.Display.SetStatusText(ConnectedStatus);
            }
            else {

                _displayManager.Display.SetStatusText(ErrorStatus);
                DisplayData = _displayManager.Display.Chars;

                // delay, can't just use Thread.Sleep(2000) as this causes UI thread
                // to stop and prevents display of above message
                await Task.Delay(2000);
                _displayManager.Display.SetStatusText(DisconnectedStatus);
            }
        }
        catch (Exception e) {
            // ensures that all exceptions are handled within the async body
            // not handling them with void async methods can cause the process
            // to crash
            Trace.WriteLine(e);
        }
        finally {
            DisplayData = _displayManager.Display.Chars;
        }
    }

    /// <summary>
    /// Method to process the receive buffer. This is called from
    /// the OnDataReceived event but on the UI Thread as a separate
    /// Task.
    /// </summary>
    private void ProcessReceiveBuffer() {

        //Debug.Print($"Buffer Size: {_cyclicBuffer.Count}");

        // get data from buffer and process for viewdata 
        while (_tcp.IsConnected() && _cyclicBuffer.Count > 0) {

            if (_menu) {
                //continue;
            }

            if (_displayManager.WriteChar(_cyclicBuffer.Remove())) {

                // updating this property will invoke the OnPropertyChanged event
                // to update the view
                DisplayData = _displayManager.Display.Chars;
            }
        }

    }

    #endregion

    #region Public Properties and methods

    /// <summary>
    /// Diplay data to be displayed by the View.
    /// </summary>
    public List<Models.Char> DisplayData {
        get { return _displayManager.Display.Chars; }
        set {
            _displayManager.Display.Chars = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Private Methods

    private async Task DisplayWelcomeMessage() {

        await Task.Delay(100);

        _displayManager.Write(Display.Menus.GetLogo());
        _displayManager.Display.SetStatusText(DisconnectedStatus);

        OnPropertyChanged(nameof(DisplayData));
    }

    private void DisplayHelp() {

        Menus.GetHelp();
        _displayManager.Display.Clear();
        _displayManager.SetCursorPosition(0, 0);
        _displayManager.Write(Display.Menus.GetHelp());

        OnPropertyChanged(nameof(DisplayData));
    }

    private void DisplayMenu() {

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
        _displayManager.Write(Display.Menus.GetMenu().Replace(Constants.PlaceHolder, menuSb.ToString()));

        OnPropertyChanged(nameof(DisplayData));

    }

    #endregion

}