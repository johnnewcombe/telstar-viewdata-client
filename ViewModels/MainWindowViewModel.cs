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
    along with the product. If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using TelstarClient.Comms;
using TelstarClient.Configuration;
using ViewdataDisplay;
using TelstarClient.Forms;
using Directory = TelstarClient.Forms.Directory;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const string CONNECTED_STATUS = "CONNECTED";
    private const string DISCONNECTED_STATUS = "DISCONNECTED";
    private const string CONFIG_FILE = "config.json";

    // Used to store the index (screen pos) of any fields within the display.
    // Fields are marked with a colon.
    //private List<int> _fields;

    // this is the item in the list not the index of the display
    //private int _currentField;

    // used to keep track of what is being displayed to the user
    // the order is unimportant EXCEPT that 'Welcome' must be the
    // first entry.
    /// <summary>
    /// Defines the types of screens that can be displayed to the user.
    /// </summary>
    private enum DisplayType
    {
        Welcome,
        Directory,
        ConnectTcp,
        ConnectSerial,
        Terminal,
        Help,
        EditConnection,
    }
    

    private ILogger<MainWindowViewModel> _logger;
    private ICommsClient _commsClient;
    private readonly CommsClientFactory _commsClientFactory;

    private readonly string _appSupportDirectory =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
        Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName +
        Path.DirectorySeparatorChar;

    private IForm _currentForm;
    private DisplayType _displayType;
    private DisplayType _previousDisplayType;
    private List<ViewdataDisplay.Char> _displayData;
    private Cursor _cursor;
    private readonly Settings _settings;
    private readonly DisplayManager _displayManagerMain;
    private readonly DisplayManager _displayManagerAlt;
    private readonly CyclicBuffer _cyclicBuffer;


    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel(CommsClientFactory commsClientFactory, ILogger<MainWindowViewModel> logger)
    {
        _logger = logger;

        _commsClientFactory = commsClientFactory;
        _commsClient = _commsClientFactory.Create(CommsClientType.Tcp); // default

        var configFile = _appSupportDirectory + CONFIG_FILE;

        _logger.LogInformation("Logging pipeline initialised");
        _logger.LogInformation("LogFile:{LogFile}", App.LogPath);
        _logger.LogInformation("Config File:{Directory}{Config}", _appSupportDirectory, CONFIG_FILE);
        _logger.LogDebug("Checking AppSupport directory:{Directory}", _appSupportDirectory);

        // create the app suport directory if it doesn't exist
        if (!System.IO.Directory.Exists(_appSupportDirectory))
        {
            _logger.LogDebug("Creating AppSupport directory:{Directory}", _appSupportDirectory);
            // create directory
            System.IO.Directory.CreateDirectory(_appSupportDirectory);
        }

        // check that the directory was created
        if (!System.IO.Directory.Exists(_appSupportDirectory))
        {
            _logger.LogError("Failed to create AppSupport directory:{Directory}", _appSupportDirectory);
        }
        else
        {
            _logger.LogDebug("AppSupport directory created:{Directory}", _appSupportDirectory);
        }

        // set up the alt display and show the welcome message
        // this will set the 'displayType' to 'Welcome'
        _displayManagerAlt = new DisplayManager();
        _displayManagerAlt.OnDisplayDataChangedEvent += DisplayDataChangedAlt;
        _displayManagerAlt.OnCurosrPositionChangedEvent += CursorPositionChangedAlt;

        // note that this method is asynchronous and includes a delay such
        // that it completes AFTER the constructor has completed
        // we ignore the result of this method to avoid the compiler warning
        _ = DisplayWelcomeMessage();

        _displayManagerMain = new DisplayManager(true);
        _displayManagerMain.OnDisplayDataChangedEvent += DisplayDataChangedMain;
        _displayManagerMain.OnCurosrPositionChangedEvent += CursorPositionChangedMain;
        _displayManagerMain.Display.SetStatusText(DISCONNECTED_STATUS);

        _settings = new Settings(configFile);
        _cyclicBuffer = new CyclicBuffer(2048);
        
    }
    /// <summary>
    /// Disposes of the view model, ensuring resources like the communication client are properly cleaned up.
    /// </summary>
    public void Dispose()
    {
        _logger.LogInformation("Disposing MainWindowViewModel");
        _commsClient.Dispose();
        // dispose anything else owned here
    }
    /// <summary>
    /// Display the Welcome message and update the connected status.
    /// note this is an asynchronous method with a delay such that
    /// it completes AFTER the constructor has completed.
    /// </summary>
    private async Task DisplayWelcomeMessage()
    {
        await Task.Delay(100);
        UpdateConnectStatus();
        SetDisplay(DisplayType.Welcome);
    }

    public string[] Args { get; set; }

    #region Data Processing and Notification

    /// <summary>
    /// Toggles Full Screen (Kiosk) mode. 
    /// </summary>
    private void ToggleKioskMode()
    {
        // get MainWindow
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger.LogDebug("MainWindow type is {Type}", desktop.MainWindow?.GetType().FullName);
            (desktop.MainWindow as Views.MainWindow)?.ToggleKioskMode();
        }
    }
    
    /// <summary>
    /// This method is called by the _displayManagerMain.OnDisplayDataChangedEvent if the
    /// Display Manager has updated the display internally
    /// </summary>
    private void DisplayDataChangedMain()
    {
        // this method is called by the _displayManagerMain.OnDisplayDataChangedEvent
        // if the Display Manager has updated the display internally
        // this allows us to update the viewDisplay property of this view model

        // however we must only do this if we are displaying Viewdata screen
        if (_displayType == DisplayType.Terminal)
        {
            Dispatcher.UIThread.Post(UpdateMainDisplay);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void CursorPositionChangedMain()
    {
        if (_displayType == DisplayType.Terminal)
        {
            Dispatcher.UIThread.Post(UpdateMainCursor);
        }
                
    }

    /// <summary>
    /// 
    /// </summary>
    private void DisplayDataChangedAlt()
    {
        // this method is called by the _displayManagerMain.OnDisplayDataChangedEvent
        // if the Display Manager has updated the display internally
        // this allows us to update the DisplayData property of this view model
        if (_displayType != DisplayType.Terminal)
        {
            Dispatcher.UIThread.Post(UpdateAltDisplay);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CursorPositionChangedAlt()
    {
        if (_displayType != DisplayType.Terminal)
        {
            Dispatcher.UIThread.Post(UpdateAltCursor);
        }
    }
    
    /// <summary>
    /// Updates the main display, this is the display that handles terminal data.
    /// </summary>
    private void UpdateMainDisplay()
    {
        //DisplayData = _displayManagerMain.Display.Chars;
        Bitmap = _displayManagerMain.Bitmap;

    }
    private void UpdateMainCursor()
    {
        Cursor = _displayManagerMain.Cursor;
        Bitmap = _displayManagerMain.Bitmap;
        
    }

    /// <summary>
    /// Updates the alt display, this is the display that handles connection, help
    /// and other internal screens.
    /// </summary>
    private void UpdateAltDisplay()
    {
        //DisplayData = _displayManagerAlt.Display.Chars;
        Bitmap = _displayManagerAlt.Bitmap;
    }

    private void UpdateAltCursor()
    {
        Cursor = _displayManagerAlt.Cursor;
        Bitmap = _displayManagerAlt.Bitmap;
    }
    /// <summary>
    /// Helper method to update the status display. It handles both main and alt displays
    /// and updates the cursor etc.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="foregroundColour"></param>
    /// <param name="backgroundColour"></param>
    private void DisplayStatusMessage(string message,string foregroundColour = ViewdataDisplay.Constants.Green, 
        string backgroundColour = ViewdataDisplay.Constants.Black)
    {
        try
        {
            // update both displays
            _displayManagerMain.Display.SetStatusText(message, foregroundColour, backgroundColour);
            _displayManagerAlt.Display.SetStatusText(message,foregroundColour, backgroundColour);

            // but only display one
            if (_displayType == DisplayType.Terminal)
            {
                _logger.LogInformation("Displaying status message to main display:{message}", message);
                //DisplayData = _displayManagerMain.Display.Chars;
                UpdateMainDisplay();
            }
            else
            {
                _logger.LogInformation("Displaying status message to alternate display:{message}", message);
                //DisplayData = _displayManagerAlt.Display.Chars;
                UpdateAltDisplay();
            }
        }
        catch (Exception ex)
        {
            // ensures that all exceptions are handled within the async body
            // not handling them with void async methods can cause the process
            // to crash
            _logger.LogError(ex, "Failed to update the Connection Status");
        }
    }

    /// <summary>
    /// This should be called on the UI Thread or via the dispatcher (see OnConnect event).
    /// </summary>
    private void UpdateConnectStatus()
    {

        // this function cannot have parameters so read from thread safe property
        // to get the current status.
        if (ConnectStatus)
        {
            DisplayStatusMessage(CONNECTED_STATUS);
        }
        else
        {
            DisplayStatusMessage(DISCONNECTED_STATUS);
        }
    }

    /// <summary>
    /// Method to process the receive buffer. This is called from
    /// the OnDataReceived event but on the UI Thread as a separate
    /// Task.
    /// </summary>
    private void ProcessReceiveBuffer()
    {
        // get data from buffer and process for viewdata 
        while (_commsClient.IsConnected() && _cyclicBuffer.Count > 0)
        {
            if (_displayManagerMain.Write(_cyclicBuffer.Remove()))
            {
                // only update the view for terminal display
                if (_displayType == DisplayType.Terminal)
                {
                    // TODO consider raising the OnDataChanged event within ViewdataDisplay.
                    //DisplayData = _displayManagerMain.Display.Chars;
                    UpdateMainDisplay();

                }
            }
        }
    }

    #endregion

    #region Public Properties and methods

    /// <summary>
    /// Display data to be displayed by the View. Setting this property causes
    /// MainWindow to collect the data and display it via the OnPropertyChanged
    /// event. Setting DsplaData will also cause MainWindow to read the Cursor
    /// property.
    /// </summary>
    //public List<ViewdataDisplay.Char> DisplayData
    //{
    //    get { return _displayData; }
    //    set
    //    {
    //        _displayData = value;
    //        OnPropertyChanged();
    //    }
    //}
    
    private byte[] _bitmap = Array.Empty<byte>();
    public byte[] Bitmap
    {
        get => _bitmap;
        set
        {
            _bitmap = value;
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// The cursor position, this is collected by MainWindow when the DisplayData property is updated.
    /// is fired when the main data is updated. Any Cursor setting can be placed here 
    /// </summary>
    public Cursor Cursor
    {
        get { return _cursor; }
        set
        {
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
    /// <param name="connection"></param>
    private void SetDisplay(DisplayType displayType, IConnection connection = null)
    {
        // if we are using the alt display then clear it etc
        if (displayType > 0)
        {
            _displayManagerAlt.Display.Clear();
            _displayManagerAlt.SetCursorPosition(0, 0);
        }

        // The screen can be in any one of these states. The client could
        // be online or offline when in any of these states.
        switch (displayType)
        {
            case DisplayType.Terminal:
                // nothing to do
                break;
            case DisplayType.Welcome:
                _displayManagerAlt.Write(new Welcome(_displayManagerAlt, connection).ToString());
                break;
            case DisplayType.Directory:
                // pop the menu into the placeholder
                _displayManagerAlt.Write(new Directory(_displayManagerAlt, connection).ToString()
                    .Replace(ViewdataDisplay.Constants.PlaceHolder, GetDirectoryFromConfig()));
                break;
            case DisplayType.ConnectTcp:
                _currentForm = new ConnectTcp(_displayManagerAlt, connection);
                _displayManagerAlt.Write(_currentForm.ToString());
                _displayManagerAlt.SetCursorPosition(_currentForm.GetCursor());
                //DisplayData = _displayManagerAlt.Display.Chars;
                UpdateAltDisplay();

                break;
            case DisplayType.ConnectSerial:
                _currentForm = new ConnectSerial(_displayManagerAlt, connection);
                _displayManagerAlt.Write(_currentForm.ToString());
                _displayManagerAlt.SetCursorPosition(_currentForm.GetCursor());
                //DisplayData = _displayManagerAlt.Display.Chars;
                UpdateAltDisplay();
                break;
            case DisplayType.EditConnection:
                _currentForm = new EditConnection(_displayManagerAlt, connection);
                _displayManagerAlt.Write(_currentForm.ToString());
                _displayManagerAlt.SetCursorPosition(_currentForm.GetCursor());
                //DisplayData = _displayManagerAlt.Display.Chars;
                UpdateAltDisplay();
                break;
            case DisplayType.Help:
                _currentForm = new Help(_displayManagerAlt, connection);
                _displayManagerAlt.Write(_currentForm.ToString());
                break;
            default:
                return; // important
        }

        //this only happens if we have changed the display
        _previousDisplayType = _displayType;
        _displayType = displayType;

        //if (_displayType == DisplayType.Terminal)
        //{
        //    Dispatcher.UIThread.Post(UpdateMainDisplay);
        //}
        //else
        //{
        //    Dispatcher.UIThread.Post(UpdateAltDisplay);
        //}
    }

    /// <summary>
    /// Generates the directory menu display string from the configuration settings.
    /// </summary>
    /// <returns>A formatted string representing the menu.</returns>
    private string GetDirectoryFromConfig()
    {
        // get the menu details from the config file
        var item = 0;
        var menuSb = new StringBuilder();
        foreach (var connection in _settings.Config.Connections)
        {
            //if (connection.Name is not null) {
            item++;
            menuSb.Append(
                $"   \e{ViewdataDisplay.Constants.AlphaWhite}{item} \e{ViewdataDisplay.Constants.AlphaCyan}{connection.Name}\r\n");
            //}
        }

        return menuSb.ToString();
    }

    #endregion
    
    
}