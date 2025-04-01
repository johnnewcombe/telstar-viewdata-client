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
    /// Constructor which creates a new Display Model.
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
        
        // clear screen is a special case as the UI screen needs a full update
        // to clear each char position
        if (character == 0x0c) {
            return ClearScreen();
        }
        
        // process the character, this will update cursor and other stuff etc
        // and will return a list of characters to be updated. Ech character
        // includes the display index so can be passed directly back to the
        // view controller as a list.
        var chrs = _viewdataUtils.ProcessChar(character);

        if (chrs != null && chrs.Count == 0) {
            return chrs;
        }
        
        // move cursor for next character
        _cursor.HorizontalTab();

        // generally we will be updating a single character but sometimes
        // it could be a whole row or in the case of a clear screen, a whole screen.
        // return all characters as a list
        return chrs;
    }

    /// <summary>
    /// Clear the screen by creating a new one.
    /// </summary>
    private List<Char> ClearScreen() {
        // clear the model data
        _display = CreateDisplay();
        _cursor.Home();

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