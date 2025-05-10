using Avalonia.Input;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel {

        /// <summary>
    /// Handles keyboard activity passsed from the View.
    /// </summary>
    /// <param name="e"></param>
    public void KeyHandler(KeyEventArgs e) {

        // if connected then help is available also
        if (_tcp.IsConnected()) {

            // control char ?
            if (e.Key == Key.LeftCtrl) {
                _keyCtrl = true;
                return;
            }

            if (e.Key == Key.Escape) {
                // if we press the escape whilst connected it cancels
                // any help screens or dialogs etc.
                //if (_help) {
                    _help = false;
                    // switch back to main display
                    DisplayData = _displayManager.Display.Chars;
                //}
            }

            
            if (_keyCtrl) {
                // previous char was a ctrl            
                _keyCtrl = false;
                switch (e.KeySymbol.ToLower()) {
                    case "a":
                        DisplayAbout();
                        break;
                    case "q":
                    case "x":
                    case "z":
                        Disconnect();
                        DisplayMenu();
                        break;
                    case "h":
                        if (_help) {
                            // hang-up or something
                        }
                        else {
                            // TODO save current screen and put it back
                            // maybe a second cache buffer in the Display object?
                            DisplayHelp();

                        }
                        break;
                    case "r":
                    case "c":
                        break;
                }
            }

            if (e.KeySymbol != null) {
                var keySymbol = _keyMapper.Map(e.KeySymbol);

                if (_tcp.Write(keySymbol)) {
                    //Trace.Print("Sent=>{0}", data);
                }
            }
        }
        // if not connected and not the menu/help then display the menu
        else if (!_menu) {
            DisplayMenu();
            _menu = true;
        }
        else {
            
            // not Connected so connect
            // key press is a string, so convert to int, get the appropriate
            // connection details and connect
            if (int.TryParse(e.KeySymbol, out var index)) {

                if (index >= 0 && index < _settings.config.Connections.Count) {

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