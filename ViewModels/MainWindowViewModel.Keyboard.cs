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

            if (_keyCtrl) {
                // previous char was a ctrl            
                _keyCtrl = false;
                switch (e.KeySymbol.ToLower()) {
                    case "q":
                    case "x":
                    case "z":
                        Disconnect();
                        DisplayMenu();
                        _menu = true;
                        break;
                    case "h":
                        // TODO save current screen and put it back
                        // maybe a second cache buffer in the Display object?
                        DisplayHelp();
                        _menu = true;
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
        else if (!_menu) {
            // menu is second screen, so any key press on first screen shows menu
            DisplayMenu();
            _menu = true;
        }
        else {
            // Connect

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

            //var iconfig = new Configuration.JsonConfig(configFile);
            //var config = iconfig.GetConnection(data);

            //Trace.WriteLine(config.Address);
            //Trace.WriteLine(config.Port);
            //Connect(config.Address, config.Port);

        }
    }
}