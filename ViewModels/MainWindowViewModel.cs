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
using TelstarClient.Logging;
using Char = TelstarClient.Models.Char;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase {
    
    private const string ConnectedStatus = "CONNECTED";
    private const string DisconnectedStatus = "DISCONNECTED";
    private const string ErrorStatus = "UNABLE TO CONNECT";
    private const string ConnectingStatus = "CONNECTING";

    private IConfiguration _appSettings;
    private readonly string _appSupportDirectory =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
        Path.DirectorySeparatorChar + AppDomain.CurrentDomain.FriendlyName +
        Path.DirectorySeparatorChar;

    private const string ConfigFile = "config.json";

    private string _status;
    private bool _menu;
    private bool _altFrameDisplayed;
    private bool _keyCtrl;
    private List<Char> _displayData;
    private readonly Settings _settings;
    private readonly DisplayManager _displayManager;
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
        
        DisplayWelcomeMessage();
        
        var configFile = _appSupportDirectory + ConfigFile;

        // create the app suport directory if it doesn't exist
        if (!Directory.Exists(_appSupportDirectory)) {
            // create directory
            Directory.CreateDirectory(_appSupportDirectory);
        }

        _displayManager = new DisplayManager(true);
        _displayManager.OnDisplayDataChangedEvent += DisplayDataChanged;
        
        _displayManagerAlt = new DisplayManager();
        _settings = new Settings(configFile);
        _keyMapper = new KeyMapper();
        _cyclicBuffer = new CyclicBuffer(2048);

        _tcp = new TCPClient();
        _tcp.OnConnectEvent += OnConnect;
        _tcp.OnDataReceivedEvent += OnReceived;

    }
    
    #region Data Processing and Notification
    
    private void DisplayDataChanged() {
        // this method is called if the DiplayManager has updated the display internally
        // e.g. when flashing text (this is handled within the display manager itself)
        // this allows us to update the Display property of this view model
        
        // however we must only do this if we are NOT displaying the altDisplay e.g.Menu/Help etc
        if (!_altFrameDisplayed) {
            Dispatcher.UIThread.Post(UpdateDisplay);
        }
    }

    private void UpdateDisplay() {
        DisplayData = _displayManager.Display.Chars;
    }
    
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
            Logging.Log.Error(e.Message);
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

        // get data from buffer and process for viewdata 
        while (_tcp.IsConnected() && _cyclicBuffer.Count > 0) {

            if (_displayManager.WriteChar(_cyclicBuffer.Remove())) {

                // if we have displayed the help page, don't update the view
                // just yet
                if (!_altFrameDisplayed) {
                    DisplayData = _displayManager.Display.Chars;
                }
            }
        }

    }

    #endregion

    #region Public Properties and methods

    /// <summary>
    /// Diplay data to be displayed by the View.
    /// </summary>
    public List<Char> DisplayData {
        get { return _displayData; }
        set {
            _displayData = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Private Methods

    private async Task DisplayWelcomeMessage() {

        await Task.Delay(100);

        _displayManager.Write(Display.Menus.GetLogo());
        _displayManager.Display.SetStatusText(DisconnectedStatus);

        DisplayData = _displayManager.Display.Chars;
    }

    /// <summary>
    /// Displays the specified text (e.g. a Viewdata Frame) on the alternate
    /// display. The main display is no longer shown but can still be written
    /// to e.g. from a tcp stream.
    /// </summary>
    private void DisplayAltFrame(string frame) {
        
        // for the help we use an alternative display so that any
        // incoming data can be handled as normal
        _displayManagerAlt.Display.Clear();
        _displayManagerAlt.SetCursorPosition(0, 0);
        _displayManagerAlt.Write(frame);
        _altFrameDisplayed = true;

        DisplayData = _displayManagerAlt.Display.Chars;

    }

    /// <summary>
    /// 
    /// </summary>
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
        DisplayData = _displayManager.Display.Chars;
        _menu = true;
    }

    #endregion

}