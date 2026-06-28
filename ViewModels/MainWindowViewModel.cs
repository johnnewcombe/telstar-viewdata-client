/*
    Copyright (c) 2025 John Newcombe

    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar. If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using TelstarClient.Comms;
using TelstarClient.Configuration;
using TelstarClient.Display;
using TelstarClient.Extensions;
using TelstarClient.Forms;
using TelstarClient.Logging;
using Char = TelstarClient.Models.Char;
using Directory = TelstarClient.Forms.Directory;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {

    private const string ConnectedStatus = "CONNECTED";
    private const string DisconnectedStatus = "DISCONNECTED";
    private const string ErrorStatus = "UNABLE TO CONNECT";
    private const string ConnectingStatus = "CONNECTING";
    private const string ConfigFile = "config.json";


    // TODO consider simplifying this field stuff or moving into a FormObject with the edit screen text?
    private class Field() {
        public int Col;
        public int Row;
    }

    // Used to store the index (screen pos) of any fields within the display.
    // Fields are marked with a colon.
    //private List<int> _fields;

    // this is the item in the list not the index of the display
    //private int _currentField;

    // used to keep track of what is being displayed to the user
    // the order is unimportant EXCEPT that 'Welcome' must be the
    // first entry.
    private enum DisplayType {
        Welcome,
        Directory,
        Terminal,
        Help,
        EditConnection,
    }

    // this is used to access the appSettings.json file
    // this is separate from the user config.json
    private IConfiguration _appSettings;

    private readonly string _appSupportDirectory =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
        Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName +
        Path.DirectorySeparatorChar;

    private Forms.IForm _currentForm;
    private DisplayType _displayType;
    private DisplayType _previousDisplayType;
    private bool _keyCtrl;
    private List<Char> _displayData;
    private Display.Cursor _cursor;
    private readonly Settings _settings;
    private readonly DisplayManager _displayManagerMain;
    private readonly DisplayManager _displayManagerAlt;
    private readonly CyclicBuffer _cyclicBuffer;
    private readonly KeyMapper _keyMapper;
    private readonly TCPClient _tcp;

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel() {

        _appSettings = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // set the log level
        Log.LogLevel(_appSettings.GetSection("Logging:LogLevel:System").Value);

        var configFile = _appSupportDirectory + ConfigFile;

        // create the app suport directory if it doesn't exist
        if (!System.IO.Directory.Exists(_appSupportDirectory)) {
            // create directory
            System.IO.Directory.CreateDirectory(_appSupportDirectory);
        }

        // set up the alt display and show the welcome message
        // this will set the 'displayType' to 'Welcome'
        _displayManagerAlt = new DisplayManager();

        // note that this method is asynchronous and includes a delay such
        // that it completes AFTER the constructor has completed
        DisplayWelcomeMessage();

        _displayManagerMain = new DisplayManager(true);
        _displayManagerMain.OnDisplayDataChangedEvent += DisplayDataChanged;
        _displayManagerMain.Display.SetStatusText(DisconnectedStatus);

        _settings = new Settings(configFile);
        _keyMapper = new KeyMapper();
        _cyclicBuffer = new CyclicBuffer(2048);

        _tcp = new TCPClient();
        _tcp.OnConnectEvent += OnConnect;
        _tcp.OnDataReceivedEvent += OnReceived;

    }

    private async Task DisplayWelcomeMessage() {
        await Task.Delay(100);
        UpdateConnectStatus();
        SetDisplay(DisplayType.Welcome);
    }

    public string[]? Args { set; get; }

    #region Data Processing and Notification

    private void DisplayDataChanged() {
        // this method is called if the DiplayManager has updated the display internally
        // e.g. when flashing text (this is handled within the display manager itself)
        // this allows us to update the Display property of this view model

        // however we must only do this if we are displaying Viewdata screen
        if (_displayType == DisplayType.Terminal) {
            Dispatcher.UIThread.Post(UpdateMainDisplay);
        }
    }

    private void UpdateMainDisplay() {
        DisplayData = _displayManagerMain.Display.Chars;
        Cursor = _displayManagerMain.Cursor;
        
    }

    private void UpdateAltDisplay() {
        DisplayData = _displayManagerAlt.Display.Chars;
        Cursor = _displayManagerAlt.Cursor;
    }

    /// <summary>
    /// This called on the UI Tread via the dispatcher (see OnConnect event).
    /// </summary>
    private void UpdateConnectStatus() {

        string statusText;

        try {
            // this function cannot have parameters so read from thread safe property
            // to get the current status.
            if (ConnectStatus) {
                statusText = ConnectedStatus;
            }
            else {
                statusText = DisconnectedStatus;
            }

            // update both displays
            _displayManagerMain.Display.SetStatusText(statusText);
            _displayManagerAlt.Display.SetStatusText(statusText);

            if (_displayType == DisplayType.Terminal) {
                DisplayData = _displayManagerMain.Display.Chars;
                Cursor = _displayManagerMain.Cursor;
            }
            else {
                DisplayData = _displayManagerAlt.Display.Chars;
                Cursor = _displayManagerAlt.Cursor;
            }
        }
        catch (Exception ex) {
            // ensures that all exceptions are handled within the async body
            // not handling them with void async methods can cause the process
            // to crash
            Logging.Log.Error(ex.Message, ex);
        }
    }

    /// <summary>
    /// Method to process the receive buffer. This is called from
    /// the OnDataReceived event but on the UI Thread as a separate
    /// Task.
    /// </summary>
    private void ProcessReceiveBuffer() {

        // get data from buffer and process for viewdata 
        while (_tcp.IsConnected() && _cyclicBuffer.Count > 0) {

            if (_displayManagerMain.Write(_cyclicBuffer.Remove())) {

                // if we have displayed the help page, don't update the view
                // just yet
                if (_displayType == DisplayType.Terminal) {
                    DisplayData = _displayManagerMain.Display.Chars;
                }
            }
        }

    }

    #endregion

    #region Public Properties and methods

    /// <summary>
    /// Display data to be displayed by the View.
    /// </summary>
    public List<Char> DisplayData {
        get { return _displayData; }
        set {
            _displayData = value;
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// The cursor position.
    /// </summary>
    public Display.Cursor Cursor {
        get { return _cursor; }
        set {
            _cursor = value; 
            OnPropertyChanged();
        }
    }   
    #endregion

    #region Private Methods

    /// <summary>
    /// Sets up the display based on the passed displayType. Optionally can receive
    /// a Configuration.Connection for displayTypes that are tailored based on
    /// a user selected connection, e.g. EditConnection.
    /// </summary>
    /// <param name="displayType"></param>
    /// <param name="conection"></param>
    private void SetDisplay(DisplayType displayType, Connection  connection = null) {

        // if we are using the alt display then clear it etc
        if (displayType > 0) {
            _displayManagerAlt.Display.Clear();
            _displayManagerAlt.SetCursorPosition(0, 0);
        }

        // The screen can be in any one of these states. The client could
        // be online or offline when in any of these states.
        switch (displayType) {

            case DisplayType.Terminal:
                // nothing to do
                break;
            case DisplayType.Welcome:
                _displayManagerAlt.Write(new Welcome().ToString());
                break;
            case DisplayType.Directory:
                // pop the menu into the placeholder
                _displayManagerAlt.Write(new Directory().ToString().Replace(Constants.PlaceHolder, GetDirectoryFromConfig()));
                break;
            case DisplayType.EditConnection:
                _currentForm = new EditConnection(connection);
                _displayManagerAlt.Write(_currentForm.ToString());
                _displayManagerAlt.SetCursorPosition(_currentForm.GetCurrentField().StartIndex);
                // TODO need to read the currently selected connection so as to populate the fields
                break;
            case DisplayType.Help:
                _currentForm = new Help();
                _displayManagerAlt.Write(_currentForm.ToString());
                break;
            default:
                return; // important
        }

        //this only happens if we have changed the display
        _previousDisplayType = _displayType;
        _displayType = displayType;

        if (_displayType == DisplayType.Terminal) {
            Dispatcher.UIThread.Post(UpdateMainDisplay);
        }
        else {
            Dispatcher.UIThread.Post(UpdateAltDisplay);
        }
    }
    
    private string GetDirectoryFromConfig() {

        // get the menu details from the config file
        var item = 0;
        var menuSb = new StringBuilder();
        foreach (var connection in _settings.Config.Connections) {
            //if (connection.Name is not null) {
                item++;
                menuSb.Append($"   \e{Constants.AlphaCyan}{item} \e{Constants.AlphaWhite}{connection.Name}\r\n");
            //}
        }

        return menuSb.ToString();
    }
    
    #endregion

}