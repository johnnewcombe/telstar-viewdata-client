using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Logging;
using TelstarClient.Models;
using TelstarClient.Extensions;

namespace TelstarClient.Display;

public partial class DisplayManager {

    #region Private Variables

    private bool _escapedMode;

    //private bool _graphicsMode;
    private Models.Display _display;
    private readonly Cursor _cursor;
    private char _holdGraphicsCharacter;
    private readonly FontMapper _fontMapper;
    private readonly ColourMapper _colourMapper;
    
    #endregion

    public DisplayManager() {
        _display = CreateDisplay();
        _cursor = new Cursor();
        _fontMapper = new FontMapper();
        _colourMapper = new ColourMapper();
    }

    public Models.Display Display {
        set { _display = value; }
        get { return _display; }
    }

    public void Write(string text) {
        foreach (var c in text) {
            WriteChar(c);
        }
    }

    public bool WriteChar(char character) {
        
        Trace.WriteLine($"WriteChar - Row: {_cursor.Row}, Col: {_cursor.Col}, Value: {(int)character:X2}");

        // process control codes and any attributes changed
        if (ProcessC0Controls(character)) {
            // nothing further to do so exit
            return false;
        }

        // if current row is read only e.g. lower line of a Double Height row then
        // it will be readonly
        if (_display.IsRowReadOnly(_cursor.Row)) {
            // despite the row being Read-Only, we still have to move the cursor on.
            _cursor.HorizontalTab();
            return false;
        }

        // get the character from the current cursor position
        var chr = _display.GetChar(_cursor.Row, _cursor.Col);

        // if we get here then the current char is not a C0 control code
        // i.e. char < 0x20.

        // if the _escapeMode flag is set then this has been done
        // by the previous character and the current character is
        // a viewdata control code. Note that the actual control
        // code is stored in the display model, however, a space
        // (or held graphic) will be displayed in the UI.
        if (_escapedMode) {

            // If the current char is a DH and the new one is not we need
            // to reduce the number of DH chars in the row, however, if
            // the current char is not a DH and the new one is then increment
            // the row reference
            // TODO what about overwriting a control e.g. when all DHs have been
            //  overwritten and RowReference is back at zero, surely the row below
            //  will need to be restored or cleared etc.
            if (chr.Value == Constants.DoubleHeight && character != Constants.DoubleHeight) {
                _display.RowReferences[_cursor.Row]--;
            }
            else if (chr.Value != Constants.DoubleHeight && character == Constants.DoubleHeight) {
                _display.RowReferences[_cursor.Row]++;

            }

            // set the value (this may get modified later based on the font being used)
            chr.Value = character;

            // apply new attributes based this control to the rest of the row etc.
            ApplyNewAttributes(ref chr);

            // reset the escapeMode flag
            _escapedMode = false;

        }
        else {
            // not a control code
            chr.IsControl = false;

            // update the value
            chr.Value = character;

            if (chr.IsGraphic) {

                var graphicsBase = chr.IsSeparated ? 0xe2c0 : 0xe200;

                // sort out graphics by selecting the appropriate character in the font
                if (chr.Value >= 0x20 && chr.Value <= 0x3f) {
                    chr.Value += (char)(graphicsBase - 0x20);
                }

                if (chr.Value >= 0x60 && chr.Value <= 0x7f) {
                    chr.Value += (char)(graphicsBase - 0x40);
                }

                // store the graphics value for any future hold graphics requirements
                _holdGraphicsCharacter = chr.Value;
            }
        }
        // process a DH control, DH alpha, DH graphic, NB etc.
        // or a Normal height control
        // note that a NH will not have a double height property set
        // if KB appears after a NH then it may still need to kill the
        // background in the lower row
        // TODO Simplify or move to ProcessDoubleHeight()
        if (chr.IsDoubleHeight ||
            (chr.IsControl && chr.Value == Constants.BlackBackground && _display.RowHasDoubleHeight(_cursor.Row)) ||
            (chr.IsControl && chr.IsForegroundColourChange() && _display.RowHasDoubleHeight(_cursor.Row))) {
            ProcessDoubleHeight(ref chr);
        }
        
        SetSpecialDisplayValues(ref chr);
        
        // Move cursor for next character, note that we will not reach
        // this code for any C0 controls.
        _cursor.HorizontalTab();
        
        return true;
    }

    public void SetCursorPosition(int column, int row) {

        _cursor.Row = row;
        _cursor.Col = column;
    }


    /// <summary>
    /// This routine simply sets the lower part of double height text or graphic characters.
    /// </summary>
    /// <param name="chr"></param>
    private void ProcessDoubleHeight(ref Char chr) {

        // if we are on the last row or the char is not marked as DH then do nothing
        if (_cursor.Row >= Models.Display.ROWS - 1) {
            return;
        }

        // get the character below
        var chrBelow = _display.Chars[(_cursor.Row + 1) * Models.Display.COLS + _cursor.Col];

        // DH graphic char
        if (!chr.IsControl && chr.IsGraphic && !chr.IsBlastThrough()) {
            // convert to upper and lower font values
            var val = chr.Value;
            chr.Value = (char)(val + 0x40); // graphics font char already set, DH (upper) is 0x40 above
            chrBelow.Value = (char)(val + 0x80); // graphics font char already set, DH (lower) is 0x80 above

        }

        // DH alpha char
        else if (!chr.IsControl && !chr.IsGraphic) {
            // convert to upper and lower font values
            var val = chr.Value;
            chr.Value = (char)(val + 0xe020 - 0x20); // chars start from 0x20
            chrBelow.Value = (char)(val + 0xe120 - 0x20); // chars start from 0x20
        }

        // NB or KB control
        else if (chr.IsControl && (chr.Value == Constants.NewBackground ||
                                   chr.Value == Constants.BlackBackground)) {
            // set background to end of lower row or until KB or another NB.
            // upper row will already have been done so copy to lower row including this char

            // copy the background colour to the immediate char in the row below
            _display.GetCharBelow(chr).Background = chr.Background;

            // copy the remainer of the row
            var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);
            foreach (var c in row) {

                if (c.IsControl && (c.Value == Constants.NewBackground ||
                                    c.Value == Constants.BlackBackground)) {
                    break;
                }

                // copy the background colour to all chars in the row below
                //_display.Chars[c.Index + Models.Display.COLS].Background = c.Background;
                _display.GetCharBelow(c).Background = c.Background;
            }
        }
        // FG control
        else if (chr.IsControl && chr.IsForegroundColourChange()) {
            var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);
            foreach (var c in row) {
                if (c.IsForegroundColourChange()) {
                    break;
                }

                // copy the background colour to all chars in the row below
                _display.GetCharBelow(c).Foreground = c.Foreground;
            }
        }

    }

    private Models.Display CreateDisplay() {

        var display = new Models.Display();
        display.Chars = new List<Char>();

        // note that we are creating a 40*25 screen not a 40*24,
        // the last lie will be used for status info
        for (var i = 0; i < (Models.Display.ROWS + 1) * Models.Display.COLS; i++) {
            var chr = new Char(Models.Display.SPC, Constants.White, Constants.Black);
            chr.Index = i;
            display.Chars.Add(chr);
        }

        return display;
    }

    private bool ProcessC0Controls(char character) {

        // is this the character passed from the comms link
        if (character >= 0x20) return false;

        // if any of these get detected then CHAR_NULL character is returned otherwise
        // the passed character is returned unaltered.
        switch (character) {
            case Constants.BS:
                _cursor.Backspace();
                break;
            case Constants.HT:
                _cursor.HorizontalTab();
                break;
            case Constants.LF:
                _cursor.LineFeed();
                break;
            case Constants.VT:
                _cursor.VerticalTab();
                break;
            case Constants.HomeClear:
                // update display model
                // the UI will be updated
                _display.Clear();
                _cursor.Home();
                break;
            case Constants.Home:
                _cursor.Home();
                break;
            case Constants.CR:
                _cursor.CarriageReturn();
                break;
            case Constants.CurOn:
                _cursor.Visible = true;
                break;
            case Constants.CurOff:
                _cursor.Visible = false;
                break;
            case Constants.Esc:
                _escapedMode = true;
                break;
        }

        return true;
    }

}