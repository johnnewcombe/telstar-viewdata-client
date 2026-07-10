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
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Microsoft.Extensions.Logging;
using TelstarClient.Configuration;
using ViewdataDisplay;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel
{
    #region Public Methods

    public void TextHandler(TextInputEventArgs args)
    {
        // e.Text is already fully composed by the OS — Shift+8 gives "*", etc.
        if (args.Text is { Length: > 0 })
        {
            var c = args.Text[0];
            if (c >= 0 && c <= 127)
            {
                ProcessKey((byte)c);
                args.Handled = false;
            }
        }
    }

    public void KeyHandler(KeyEventArgs args)
    {
        // Only handle keys that OnTextInput will NEVER fire for
        var ascii = GetAsciiKey(args);
        if (ascii is not null)
        {
            ProcessKey(ascii.Value);
            args.Handled = true;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Handles keyboard activity passed from the View.
    /// </summary>
    /// <param name="asciiValue"></param>
    private void ProcessKey(byte asciiValue)
    {
        // NOTE That this function does not run on the UI thread
        _logger.LogDebug("ASCII Value:{Hex:X2}h,{Decimal}d", asciiValue, asciiValue);

        // generic operations only, Form specific operations are below
        switch (asciiValue)
        {
            case Constants.ALT_F: // full screen
                ToggleKioskMode();
                return;
            case Constants.ALT_X: // alt+x disconnect
                Disconnect();
                SetDisplay(DisplayType.Directory);
                return;
            case Constants.ALT_Q: // alt+q
                Disconnect();
                Shutdown();
                return;
        }

        var result = false;

        switch (_displayType)
        {
            case DisplayType.Terminal:
                result = HandleTerminalKey(asciiValue);
                break;
            case DisplayType.Welcome:
                result = HandleWelcomeKey(asciiValue);
                break;
            case DisplayType.Directory:
                result = HandleDirectoryKey(asciiValue);
                break;
            case DisplayType.ConnectTcp:
                result = HandleConnectTcpKey(asciiValue);
                break;
            case DisplayType.ConnectSerial:
                result = HandleConnectSerialKey(asciiValue);
                break;
            case DisplayType.EditConnection:
                result = HandleEditConnectionKey(asciiValue);
                break;
            case DisplayType.Help:
                result = HandleHelpKey(asciiValue);
                break;
        }

        if (!result)
        {
            UpdateConnectStatus();
        }

        // update the appropriate display man
        DisplayData = _displayType == DisplayType.Terminal
            ? _displayManagerMain.Display.Chars
            : _displayManagerAlt.Display.Chars;
    }

    /// <summary>
    /// Key handler returns true if the status has been updated.
    /// </summary>
    /// <param name="asciiValue"></param>
    /// <returns></returns>
    private bool HandleTerminalKey(byte asciiValue)
    {
        // looking for alt key combinations (same as ctrl codes but with high bit set)
        switch (asciiValue)
        {
            case Constants.ALT_C: // conceal
                _displayManagerMain.Display.Conceal();
                break;
            case Constants.ALT_R: // reveal
                _displayManagerMain.Display.Reveal();
                break;
            case Constants.CR: // return
                asciiValue = Constants.HASH;
                break;
            case Constants.ALT_M: // alt+D sends CR for modems etc.
                asciiValue = Constants.CR;
                break;
            case Constants.ALT_H: // alt+h show help menus
                SetDisplay(DisplayType.Help);
                return false;
            case (byte)ViewdataDisplay.Constants.CurOn:
                return true;
            case (byte)ViewdataDisplay.Constants.CurOff:
                return true;
        }

        // send value to remote end
        if (!_commsClient.Write(asciiValue))
        {
            _logger.LogError("Failed to send character to server:{Hex:X2}h,{Decimal}d", asciiValue, asciiValue);
        }
        else
        {
            _logger.LogInformation("Character sent to server:{Hex:X2}h,{Decimal}d", asciiValue, asciiValue);
        }

        return false;
    }

    /// <summary>
    /// Key handler, returns true the status has been updated.
    /// </summary>
    /// <param name="asciiValue"></param>
    /// <returns></returns>
    private bool HandleWelcomeKey(byte asciiValue)
    {
        // pointless switch statement for future use and to remove ReSharper warnings
        switch (asciiValue)
        {
            case > 0:
                break;
        }

        // if we get a key press of any kind whilst looking at the welcome page
        // then load the menu
        SetDisplay(DisplayType.Directory);
        return false;
    }

    /// <summary>
    /// Key handler, returns true the status has been updated.
    /// </summary>
    /// <param name="asciiValue"></param>
    /// <returns></returns>
    private bool HandleDirectoryKey(byte asciiValue)
    {
        IConnection con;
        int index;

        switch (asciiValue)
        {
            case Constants.ALT_H: // alt+h show help menus
                SetDisplay(DisplayType.Help);
                break;
            case (byte)'S' or (byte)'s': // serial connection (there's only one serial connection)
                con = _settings.Config.SerialConnection;
                SetDisplay(DisplayType.ConnectSerial, con);
                break;

            // validate number Connect)
            case >= (byte)'0' and <= (byte)'9':

                // convert to index
                index = asciiValue - (byte)'0';

                // manual dialling?
                if (index == 0)
                {
                    SetDisplay(DisplayType.ConnectTcp);
                }
                else if (index - 1 < _settings.Config.Connections.Count)
                {
                    // user has selected a connection
                    // index-1 as menu is '1' based and collection is '0' based
                    con = _settings.Config.Connections[index - 1];

                    if (con is TcpConnection tcp)
                    {
                        if (!string.IsNullOrEmpty(tcp.Name))
                        {
                            Connect(tcp.Host, tcp.Port, false, false);
                            SetDisplay(DisplayType.Terminal);
                        }
                    }
                }

                break;

            // validate alt+number (Edit)
            case >= Constants.ALT_0 and <= Constants.ALT_9:
                index = asciiValue - Constants.ALT_0;

                if (index - 1 < _settings.Config.Connections.Count)
                {
                    // user has selected a connection
                    // index-1 as menu is '1' based and collection is '0' based
                    con = _settings.Config.Connections[index - 1];
                    SetDisplay(DisplayType.EditConnection, con);
                }

                break;
        }

        return false;
    }

    /// <summary>
    /// Key handler, returns true the status has been updated.
    /// </summary>
    /// <param name="asciiValue"></param>
    /// <returns></returns>
    private bool HandleConnectTcpKey(byte asciiValue)
    {
        var port = 0;
        var host = string.Empty;
        var parity = false;

        switch (asciiValue)
        {
            case Constants.ESC:
                SetDisplay(_previousDisplayType);
                return false;
        }

        // connect or ignore
        if (!_currentForm.ProcessFormKey(asciiValue) || asciiValue == Constants.ALT_C)
        {
            if (_currentForm is not null)
            {
                if (_currentForm.IsValid())
                {
                    // get the values
                    var field = _currentForm.GetFieldById("host");
                    if (field is not null)
                        host = field.Value;
                    field = _currentForm.GetFieldById("port");
                    if (field is not null)
                        int.TryParse(field.Value, out port);
                    field = _currentForm.GetFieldById("parity");
                    if (field is not null)
                        parity = field.Value is "y" or "Y";

                    Connect(host, port, parity, false);
                    SetDisplay(DisplayType.Terminal);
                }
                else
                {
                    _logger.LogError("Form is invalid: {Host}, {Port}", host, port);
                    // update the status
                    DisplayStatusMessage("INVALID DATA", ViewdataDisplay.Constants.Red);
                    return true;
                }
            }
        }
        else
        {
            DisplayData = _displayManagerAlt.Display.Chars;
        }

        return false;
    }

    /// <summary>
    /// Key handler, returns true the status has been updated.
    /// </summary>
    /// <param name="asciiValue"></param>
    /// <returns></returns>
    private bool HandleConnectSerialKey(byte asciiValue)
    {
        var baud = 0;
        var device = string.Empty;
        var parity = false;

        switch (asciiValue)
        {
            case Constants.ESC:
                SetDisplay(_previousDisplayType);
                return false;
        }

        if (!_currentForm.ProcessFormKey(asciiValue) || asciiValue == Constants.ALT_C)
        {
            if (_currentForm is not null)
            {
                if (_currentForm.IsValid())
                {
                    // get the values

                    var field = _currentForm.GetFieldById("device");
                    if (field is not null)
                        device = field.Value;
                    field = _currentForm.GetFieldById("baud");
                    if (field is not null)
                        int.TryParse(field.Value, out baud);
                    field = _currentForm.GetFieldById("parity");
                    if (field is not null)
                        parity = field.Value is "y" or "Y";
                    
                    // update settings directly
                    _settings.Config.SerialConnection.Device = device;
                    _settings.Config.SerialConnection.BaudRate = baud;
                    _settings.Config.SerialConnection.Parity = parity ;
                    _settings.Save();

                    // connect
                    Connect(device, baud, parity, true);
                    SetDisplay(DisplayType.Terminal);
                }
                else
                {
                    _logger.LogError("Form is invalid: {Device}, {Baud}", device, baud);

                    // display error message on the status bar
                    DisplayStatusMessage("INVALID DATA", ViewdataDisplay.Constants.Red);
                    return true;
                }
            }
        }
        else
        {
            DisplayData = _displayManagerAlt.Display.Chars;
        }

        return false;
    }

    /// <summary>
    /// Key handler, returns true the status has been updated.
    /// </summary>
    /// <param name="asciiValue"></param>
    /// <returns></returns>
    private bool HandleEditConnectionKey(byte asciiValue)
    {
        var port = 0;
        var host = string.Empty;
        var name = string.Empty;
        var parity = false;

        switch (asciiValue)
        {
            case Constants.ESC:
                SetDisplay(_previousDisplayType);
                return false;
            case Constants.ALT_D: // delete entry

                if (_currentForm.Connection is not null)
                {
                    if (_currentForm.Connection is TcpConnection tcp)
                    {
                        tcp.Name = string.Empty;
                        tcp.Host = string.Empty;
                        tcp.Port = 0;
                        tcp.Parity = false;
                    }

                    _logger.LogInformation("Saving connection:{Name}, Host:{IP}, Port:{Port}, Parity:{parity}", name, host, port, parity);
                    _settings.Save();

                    //DisplayEditor returns false when complete or canceled
                    SetDisplay(_previousDisplayType);
                }

                break;
        }

        // process the key, the ProcessFormKey returns false when complete so if
        // false or save has been selected then save the current connection
        if (!_currentForm.ProcessFormKey(asciiValue) || asciiValue == Constants.ALT_S)
        {
            // save connection
            if (_currentForm is not null)
            {
                // get the values from the fields

                var field = _currentForm.GetFieldById("dirName");
                if (field is not null)
                    name = field.Value;
                field = _currentForm.GetFieldById("host");
                if (field is not null)
                    host = field.Value;
                field = _currentForm.GetFieldById("port");
                if (field is not null)
                    int.TryParse(field.Value, out port);
                field = _currentForm.GetFieldById("parity");
                if (field is not null)
                    parity = field.Value is "y" or "Y";

                // if the form is valid get the current connection from the form and update and save it
                if (_currentForm.IsValid() && _currentForm.Connection is not null)
                {
                    if (_currentForm.Connection is TcpConnection tcp)
                    {
                        tcp.Name = name;
                        tcp.Host = host;
                        tcp.Port = port;
                        tcp.Parity = parity;

                        // save, the form holds the current connection within settings
                        _logger.LogInformation("Saving connection, Name:{Name}, Host:{IP}, Port:{Port}, Parity:{parity}", name, host, port, parity);
                        _settings.Save();
                        UpdateConnectStatus();
                        DisplayData = _displayManagerAlt.Display.Chars;
                    }
                }
                else // connection is either null or invalid
                {
                    if (_currentForm.Connection != null)
                    {
                        _logger.LogError("Connection invalid and not saved, Name:{Name}, Host:{IP}, Port:{Port}", name, host, port);

                        // display error message on the status bar
                        DisplayStatusMessage("INVALID DATA", ViewdataDisplay.Constants.Red);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Connection invalid and not saved, the forms connection is null");
                    }
                }
            }

            //DisplayEditor returns false when complete or canceled
            SetDisplay(_previousDisplayType);
        }


        return false;
    }

    /// <summary>
    /// Key handler, returns true the status has been updated.
    /// </summary>
    /// <param name="asciiValue"></param>
    /// <returns></returns>
    private bool HandleHelpKey(byte asciiValue)
    {
        switch (asciiValue)
        {
            case Constants.ESC:
                SetDisplay(_previousDisplayType);
                break;
        }

        return false;
    }


    private static byte? GetAsciiKey(KeyEventArgs e)
    {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (alt)
        {
            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                return (byte)(e.Key - Key.D1 + Constants.ALT_1);
            }

            return e.Key switch
            {
                Key.C => Constants.ALT_C,
                Key.D => Constants.ALT_D,
                Key.F => Constants.ALT_F,
                Key.H => Constants.ALT_H,
                Key.M => Constants.ALT_M,
                Key.Q => Constants.ALT_Q,
                Key.R => Constants.ALT_R,
                Key.S => Constants.ALT_S,
                Key.X => Constants.ALT_X,
                _ => null
            };
        }

        if (ctrl)
        {
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                return e.Key == Key.T ? (byte)10 : (byte)(e.Key - Key.A + 1);
            }

            return null;
        }

        return e.Key switch
        {
            // Navigation / editing keys
            Key.Enter => Constants.CR,
            Key.Escape => Constants.ESC,
            Key.Back => Constants.LEFT,
            Key.Tab => shift ? (byte)Constants.SHIFT_TAB : (byte)Constants.RIGHT,
            Key.Delete => Constants.DEL,

            // Arrow keys (standard ASCII codes)
            Key.Left => Constants.LEFT,
            Key.Right => Constants.RIGHT,
            Key.Down => Constants.DOWN,
            Key.Up => Constants.UP,

            // Modifier keys that shouldn't produce a key code
            Key.LeftShift or Key.RightShift or
                Key.LeftCtrl or Key.RightCtrl or
                Key.LeftAlt or Key.RightAlt or
                Key.Capital or Key.NumLock or
                Key.Scroll => null,

            _ => null
        };
    }

    private void Shutdown()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
        else
        {
            Environment.Exit(0);
        }
    }

    #endregion
}