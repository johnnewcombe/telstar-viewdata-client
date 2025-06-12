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
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Input;
using TelstarClient.Display;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel {

    /// <summary>
    /// Handles keyboard activity passed from the View.
    /// </summary>
    /// <param name="e"></param>
    public async Task KeyHandler(KeyEventArgs e) {

        Logging.Log.Information($"Key:{e.Key.ToString()}, Symbol:{e.KeySymbol}, Physical Key:{e.PhysicalKey.ToString()} Modifiers: {e.KeyModifiers}");
        
        // if connected then help is available also
        if (_tcp.IsConnected()) {
            
            if (e.KeyModifiers == KeyModifiers.Control) {

                // previous char was a ctrl            
                switch (e.KeySymbol.ToLower()) {
                    case "a":
                        DisplayAltFrame(Menus.GetAbout());
                        break;
                    case "q":
                    case "x":
                    case "z":
                        Disconnect();
                        DisplayMenu();
                        break;
                    case "h":
                        DisplayAltFrame(Menus.GetHelp());
                        break;
                    case "r":
                    case "c":
                        break;
                }
            }
            else {
                if (e.KeySymbol != null) {
                    // if we press the escape whilst connected it cancels
                    // any help screens or dialogs etc.
                    if (_altFrameDisplayed) {
                        _altFrameDisplayed = false;
                        // switch back to main display*
                        DisplayData = _displayManager.Display.Chars;
                    }
                    else {
                        var keySymbol = _keyMapper.Map(e.KeySymbol);

                        if (_tcp.Write(keySymbol)) {
                            //Trace.Print("Sent=>{0}", data);
                        }
                    }
                }
            }
        }
        // if not connected and not the menu/help then display the menu
        else if (!_menu) {
            DisplayMenu();
            _menu = true;
        }
        else {
            // TODO Help available in disconnected state
            //
            //

            // not Connected so connect
            // key press is a string, so convert to int, get the appropriate
            // connection details and connect
            if (int.TryParse(e.KeySymbol, out var index)) {

                if (index == 0) {

                    // Manual dialing
                    //
                    // TODO Implement Manual connections

                }
                else if (index > 0 && index < _settings.config.Connections.Count) {

                    // index-1 as menu is '1' based and collection is '0' based
                    var con = _settings.config.Connections[index - 1];
                    if (con.Name is not null) {
                        Connect(con.Address, con.Port);
                    }

                }
            }
        }
    }
}