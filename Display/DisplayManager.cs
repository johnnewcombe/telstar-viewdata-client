using System.Collections.Generic;
using System.Diagnostics;
using TelstarClient.Models;
using TelstarClient.Extensions;

namespace TelstarClient.Display;

public class DisplayManager {

    #region Private Variables

    private bool _escapedMode;

    //private bool _graphicsMode;
    private Models.Display _display;
    private Cursor _cursor;
    private char _holdGraphicsCharacter = ' ';
    private FontMapper _fontMapper;
    private ColourMapper _colourMapper;

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

        // process control codes and any attributes changed
        if (ProcessC0Controls(character)) {
            // nothing further to do so exit
            return false;
        }

        // if current row is read only e.g. lower line of a Double Height row then
        // it will be readonly
        if (_display.IsRowReadOnly(_cursor.Row)) {
            return false;
        }

        // get the character from the current cursor position
        var chr = _display.GetChar(_cursor.Row, _cursor.Col);

        // first get the attributes from the previous cell (or defaults if col 0)
        var prevChr = GetPreviousCharacter();

        // get current attributes based on the previous Char
        if (prevChr == null) {
            chr.SetDefaultAttributes();
        }
        else {
            prevChr.CloneAttributes(ref chr);
        }

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
            if (chr.Value == Constants.DoubleHeight && character != Constants.DoubleHeight) {
                _display.RowReferences[_cursor.Row]--;
            }
            else if (chr.Value != Constants.DoubleHeight && character == Constants.DoubleHeight) {
                _display.RowReferences[_cursor.Row]++;

            }

            // update the value
            chr.Value = character;

            // apply new attributes from this control code and collect
            // any other cells that need updating e.g. to the end of
            // the row etc.
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

                /*
                 * Normal graphics the base numbers are
                 * e200 for 20-3f
                 * e220 for 60-7f
                 *
                 * Seperated
                 * e2c0 for 20-3f
                 * e2e0 for 60-7f
                 */
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

        // could be a DH control, DH alpha or DH graphic
        if (chr.IsDoubleHeight) {
            SetDoubleHeight(ref chr);
        }

        // Move cursor for next character, irrespective of
        // how many are being updated or whether this is a parallel control
        // or a normal character.
        // Note that we will not reach this code for any C0 controls.
        _cursor.HorizontalTab();

        // update the char appropriately
        // we return a list as it may be necessary to update the rest of a row.

        // substitute viewdata characters for suitable font characters as required
        chr.Value = _fontMapper.Map(chr.Value);

        // TODO: This cannot be used if chr is a reference to the chr in the DisplayGrid
        //  only if it is a ne instance of a Char. The Display object must contain the
        //  control value.
        if (chr.IsControl) {
            if (chr.IsGraphicsHold && _holdGraphicsCharacter != ' ') {
                chr.Value = _holdGraphicsCharacter;
            }
            else {
                chr.Value = '\xe200';
            }
        }

        //result.Insert(0, chr.DeepClone());
        return true;
    }

    public void SetCursorPosition(int column, int row) {

        _cursor.Row = row;
        _cursor.Col = column;
    }

    public void SetStatusOnline() {
        Display.SetStatusText(1, "CONNECTED".PadRight(Constants.StatusPadding), Constants.Green);
    }

    public void SetStatusOffline() {
        Display.SetStatusText(1, "DISCONNECTED".PadRight(Constants.StatusPadding), Constants.Green);
    }

    public void SetStatusConnecting() {
        Display.SetStatusText(1, "CONNECTING".PadRight(Constants.StatusPadding), Constants.Green);
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

    /// <summary>
    /// Returns the previous character unless the current cursor is at
    /// the start of the row in which case null is returned.
    /// </summary>
    /// <returns></returns>
    private Char GetPreviousCharacter() {

        if (_cursor.Col == 0) {
            return null;
        }

        return _display.GetChar(_cursor.Row, _cursor.Col - 1);
    }

    /// <summary>
    /// Adds any new attributes to the character that this control character
    /// might require.
    ///
    /// If a character on the screen is updated, this may very well affect
    /// the following characters up until the end of the row. There are
    /// some basic rules to be followed i.e.
    ///
    /// Rules:
    /// 
    /// * A foreground colour change affects all following characters in the row up
    ///   until another colour change is found.
    /// * A new background control character affects all following characters in the
    ///   row until either another new background is found or if s a black
    ///   background control character is found.
    /// * A new background control character is applied to the cell containing the NB
    ///   control code.
    /// * A black background control character affects all following characters in the
    ///   row until either another black background is found or if a new
    ///   background control character is found.
    /// * A black background control character is applied to the cell containing the NB
    ///   control code.
    /// * A double height control character affects all following characters in the row
    ///   until a normal height control character is found.
    /// * Any row containing a double height character will cause the row below to be
    ///   read only.
    /// * Flash affects all following characters in the row until a steady control
    ///   character is found.
    /// * Separated graphics control character affects all following characters in the row until a
    ///   Contiguous graphics control character is found.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    private void ApplyNewAttributes(ref Char chr) {

        if (!_escapedMode || chr.Value <= 0x40 || chr.Value > 0x5f) {
            return;
        }

        chr.IsControl = true;

        var prevChr = GetPreviousCharacter();

        // check to see if we have a foreground change
        if (chr.IsForegroundColourChange()) {
            SetForeground(ref chr, _colourMapper.Map(chr.Value));
            chr.IsGraphic = chr.IsGraphicColourChange();
        }

        switch (chr.Value) {

            case Constants.Flash: //TODO 
                break;
            case Constants.Steady: //TODO 
                break;
            case Constants.NormalHeight:
                chr.IsDoubleHeight = false;
                break;
            case Constants.DoubleHeight:
                chr.IsDoubleHeight = true;
                break;
            case Constants.Conceal: //TODO 
                break;
            case Constants.Contiguous:
                chr.IsSeparated = false;
                break;
            case Constants.Separated:
                chr.IsSeparated = true;
                break;
            case Constants.NewBackground:
                SetBackground(ref chr, prevChr is null ? Constants.White : prevChr.Foreground);
                break;
            case Constants.BlackBackground:
                SetBackground(ref chr, Constants.Black);
                break;
            case Constants.HoldGraphics:
                chr.IsGraphicsHold = true;
                break;
            case Constants.ReleaseGraphics:
                chr.IsGraphicsHold = false;
                break;
            default:
                break;
        }

        return;
    }

    private void SetForeground(ref Char chr, string colour) {

        // set the character's foreground
        chr.Foreground = colour;

        // set the background of the rest of the row
        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);

        foreach (var c in row) {
            // if next char is a foreground colour change then all done
            if (c.IsControl && c.IsForegroundColourChange()) {
                break;
            }

            c.Foreground = colour;
        }
    }

    private void SetBackground(ref Char chr, string colour) {

        // set the character's background
        chr.Background = colour;

        // set the background of the rest of the row
        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);

        foreach (var c in row) {
            
            // if next char is a foreground colour change then all done
            if (c.IsControl && c.IsBackgroundColourChange()) {
                break;
            }
            c.Background = colour;

            // if next char is a Black Background or New Background, then all done
            if (c.IsControl && (c.Value == Constants.BlackBackground || c.Value == Constants.NewBackground)) {
                break;
            }
            
        }
    }

    /// <summary>
    /// This routine simply sets the lower part of double height text or graphic characters.
    /// </summary>
    /// <param name="chr"></param>
    private void SetDoubleHeight(ref Char chr) {

        // set the chr below but only if we are not on the last row
        // the DH control code doesn't need changing.
        if (_cursor.Row < Models.Display.ROWS - 1) {

            // copy all the attributes from the upper row to the lower one
            // TODO This needs to be done whenever writing to the top row when
            //  there is a DH in the row.
            //  It may even be possible to just apply the background/foreground
            //  attribute for the chars before the DH control and all attributes for the
            //  control and following chars.
            //  New/Kill background applies to both rows before and after a DH

            // get the character below
            var chrBelow = _display.Chars[(_cursor.Row + 1) * Models.Display.COLS + _cursor.Col];

            //_display.CloneAttributesToRowBelow(_cursor.Row);
            for (var i = 0; i < Models.Display.COLS; i++) {
                var chrUpper = _display.Chars[_cursor.Row * Models.Display.COLS + i];
                var chrLower = _display.Chars[(_cursor.Row + 1) * Models.Display.COLS + i];
                chrLower.Foreground = chrUpper.Foreground;
                chrLower.Background = chrUpper.Background;
            }

            // controls dont get modified
            if (chr.IsControl) {
                return;
            }

            // attributes already updated so just update the value
            var val = chr.Value;

            // modify value to set the upper and lower font values
            // Capitals when in graphics mode are displayed as there normal alpha characters
            // these are referred to as Blast Through characters.
            if (chr.IsGraphic && !chr.IsBlastThrough()) {
                chr.Value = (char)(val + 0x40); // graphics font char already set, DH (upper) is 0x40 above
                chrBelow.Value = (char)(val + 0x80); // graphics font char already set, DH (lower) is 0x48 above
            }
            else {
                chr.Value = (char)(val + 0xe020 - 0x20); // chars start from 0x20
                chrBelow.Value = (char)(val + 0xe120 - 0x20); // chars start from 0x20
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

}