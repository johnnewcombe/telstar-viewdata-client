using System.Collections.Generic;
using TelstarClient.Models;

namespace TelstarClient.DisplayManager;

/// <summary>
/// This class manages the Display model. Methods here update the Display model
/// and provide all the Viewdata decoding etc., 
/// </summary>
public class DisplayManager {
    private Display _display;
    private readonly Cursor _cursor;
    private ViewdataUtils _viewdataUtils;

    /// <summary>
    /// Contructor which creates a new Display Model.
    /// </summary>
    public DisplayManager() {
        _display = CreateDisplay();
        _cursor = new();
        _viewdataUtils = new(_display, _cursor);
    }

    /// <summary>
    /// Accepts a single Unicode 16 character to update the cursor or Display model.
    /// Returns a List of Char objects that need updating in the UI.
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public List<Char> PrintChar(char character) {
        // process for viewdata
        character = _viewdataUtils.ConvertChar(character);
        
        // special case, clear screen
        if (character == 0x0c) {

            // clear the model data
            _display = CreateDisplay();

            // list of new Chars to be returned these
            // will be used to update the UI
            var clearScreen = new List<Char>();
            foreach (var r in _display.Rows) {
                foreach (var c in r.Chars) {
                    clearScreen.Add(c);
                }
            }
            return clearScreen;
        }
        
        if (character < 0x20) {
            return null;
        }

        // get the position index e.g. 0-959
        var chr = _display.Rows[_cursor.Row].Chars[_cursor.Col];

        // update the char
        chr.Value = character;
        chr.Foreground = "White";
        chr.Background = "Black";


        // move cursor for next character
        _cursor.HorizontalTab();

        // return position index and character as a tuple
        return new List<Char> { chr };
    }

    /// <summary>
    /// Clear the screen by creating a new one.
    /// </summary>
    private void ClearScreen() {
        // TODO: How does this propagate to the display! Event?
        _display = new Display();
    }
    
    private Display CreateDisplay() {
        var index = 0;

        var display = new Display();
        display.Rows = new List<Row>();

        for (var i = 0; i < Display.ROWS; i++) {
            var row = new Row(false);
            row.Chars = new List<Char>();

            for (var j = 0; j < Display.COLS; j++) {
                var chr = new Char(' ', "white,", "black");
                chr.Index = index++;
                row.Chars.Add(chr);
            }

            display.Rows.Add(row);
        }

        return display;
    }
}