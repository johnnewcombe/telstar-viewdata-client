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
using System.Threading;
using Avalonia.Controls.Converters;
using Avalonia.Logging;
using TelstarClient.Extensions;
using TelstarClient.Forms;
using Char = TelstarClient.Models.Char;

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
    private string _name;

    private Timer _stateTimer; // TODO use _stateTimer.Dispose() when exiting the display manager.

    #endregion

    #region Public Variabled

    public delegate void OnDisplayDataChangedEventHandler();

    public event OnDisplayDataChangedEventHandler OnDisplayDataChangedEvent;
    
    #endregion
    
    #region Constructor

    public DisplayManager(bool enableFlash = false) {

        _display = CreateDisplay();
        _cursor = new Cursor();
        _fontMapper = new FontMapper();
        _colourMapper = new ColourMapper();

        if (enableFlash) {
            _stateTimer = new Timer(Flash, null, 1000, 1000);
        }
    }

    #endregion

    private void Flash(Object state) {

        // Update the display
        Display.Flash();

        // Raise an event to the mainViewModel to indicate that the display property has changed.
        OnDisplayDataChangedEvent();
    }

    #region Public Properties

    public Models.Display Display {
        set { _display = value; }
        get { return _display; }
    }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Writes a string to the display.
    /// </summary>
    /// <param name="text"></param>
    public void Write(string text) {
        foreach (var c in text) {
            Write(c);
        }
    }
    
    public bool Write(char character) {

        //Logging.Log.Information($"Write - Row: {_cursor.Row}, Col: {_cursor.Col}, Value: {(int)character:X2}");

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
            chr.Control = false;

            // update the value
            chr.Value = character;

            if (chr.Graphic) {

                var graphicsBase = chr.Separated ? 0xe2c0 : 0xe200;

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
            else {
                _holdGraphicsCharacter = Models.Display.SPC;
            }
        }

        ProcessDoubleHeight(ref chr);

        if (chr.Control) {

            if (chr.GraphicsHold && chr.Graphic) {
                chr.Value = _holdGraphicsCharacter;
            }
            else {
                chr.Value = Models.Display.SPC;
            }
        }

        // substitute viewdata characters for suitable font characters as required
        chr.Value = _fontMapper.Map(chr.Value);

        // Move cursor for next character, note that we will not reach
        // this code for any C0 controls.
        _cursor.HorizontalTab();

        return true;
    }

    /// <summary>
    /// Sets the cursor position.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="row"></param>
    public void SetCursorPosition(int column, int row) {
        _cursor.Row = row;
        _cursor.Col = column;
    }

    public void SetCursorPosition(int index) {
        
        if (index < Models.Display.COLS) {
            _cursor.Row = 0;
            _cursor.Col = index;
        }
        else {
            _cursor.Row = index / Models.Display.COLS;
            _cursor.Col = index % Models.Display.COLS;
        }
    }
    
    #endregion

    #region Private Methods

    /// <summary>
    /// This routine simply sets the lower part of double height text or graphic characters.
    /// </summary>
    /// <param name="chr"></param>
    private void ProcessDoubleHeight(ref Char chr) {

        // check to see if we need to do anything
        if (!_display.RowHasDoubleHeight(_cursor.Row)) {
            return;
        }

        // TODO what if it was DH but isn't now? do we need to blank the lower row?

        // At this point we know the row is double height (i.e. taking up two display rows)
        // even if there is no double height chars actually displayed.
        // Under this scenario any New Background (NB) controls on the row need to be updated
        // as any new background that appears before a DH control will have only updated the
        // top row, now they need to update top and bottom rows.

        // TODO note that a NH will not have a double height property set
        //  if KB appears after a NH then it may still need to kill the
        //  background in the lower row

        // get the whole row (passing -1 for col will retrieve cols 0-39)
        // and fix any background issues
        var chrs = _display.GetRemainderOfRow(_cursor.Row, -1);
        foreach (var c in chrs) {
            _display.GetCharBelow(c).Background = c.Background;
            // TODO refactor as this is duplicated below for NB etc. and in SetDoubleHeight()
        }

        if (!chr.DoubleHeight) {
            return;
        }

        // if we are on the last row or the char is not marked as DH then do nothing
        if (_cursor.Row >= Models.Display.ROWS - 1) {
            return;
        }

        // get the character below
        // TODO refactor this with chr.GetCharBelow()
        var chrBelow = _display.Chars[(_cursor.Row + 1) * Models.Display.COLS + _cursor.Col];

        // DH graphic char
        if (!chr.Control && chr.Graphic && !chr.IsBlastThrough()) {
            // convert to upper and lower font values
            var val = chr.Value;
            chr.Value = (char)(val + 0x40); // graphics font char already set, DH (upper) is 0x40 above
            chrBelow.Value = (char)(val + 0x80); // graphics font char already set, DH (lower) is 0x80 above

        }

        // DH alpha char
        else if (!chr.Control && (!chr.Graphic || chr.IsBlastThrough())) {
            // convert to upper and lower font values
            var val = chr.Value;
            chr.Value = (char)(val + 0xe020 - 0x20); // chars start from 0x20
            chrBelow.Value = (char)(val + 0xe120 - 0x20); // chars start from 0x20
        }

        // NB or KB control
        else if (chr.Control && (chr.Value == Constants.NewBackground ||
                                 chr.Value == Constants.BlackBackground)) {
            // set background to end of lower row or until KB or another NB.
            // upper row will already have been done so copy to lower row including this char

            // copy the background colour to the immediate char in the row below
            _display.GetCharBelow(chr).Background = chr.Background;

            // copy the remainder of the row
            var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);
            foreach (var c in row) {

                if (c.Control && (c.Value == Constants.NewBackground ||
                                  c.Value == Constants.BlackBackground)) {
                    break;
                }

                // copy the background colour to all chars in the row below
                //_display.Chars[c.Index + Models.Display.COLS].Background = c.Background;
                _display.GetCharBelow(c).Background = c.Background;
            }
        }
        // FG control
        else if (chr.Control && chr.IsForegroundColourChange()) {
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
    /// Adds any new attributes to the character that this control character
    /// might require.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    private void ApplyNewAttributes(ref Char chr) {

        chr.Control = true;

        if (chr.IsForegroundColourChange()) {

            var colour = _colourMapper.Map(chr.Value);
            SetForeground(ref chr, colour, chr.IsGraphicColourChange());
            return;
        }

        switch (chr.Value) {

            case Constants.Flash:
                SetFlash(ref chr, true);
                break;
            case Constants.Steady:
                SetFlash(ref chr, false);
                break;
            case Constants.NormalHeight:
                SetNormalHeight(ref chr);
                break;
            case Constants.DoubleHeight:
                SetDoubleHeight(ref chr);
                break;
            case Constants.Conceal:
                // there is no reveal code, reveal is executed by the user
                // normally a control code affects the following cells, however conceal
                // is unusual in that the position where the control code is, is also affected.
                // This wouldn't normally matter as control codes are blank. However if Hold 
                // Graphics is set, it would matter.
                SetConceal(ref chr, true);
                break;
            case Constants.Contiguous:
                SetSeparatedMode(ref chr, false);
                chr.Separated = false;
                break;
            case Constants.Separated:
                SetSeparatedMode(ref chr, true);
                break;
            case Constants.NewBackground:
                SetBackground(ref chr, chr.Foreground);
                break;
            case Constants.BlackBackground:
                SetBackground(ref chr, Constants.Black);
                break;
            case Constants.HoldGraphics:
                SetGraphicsHold(ref chr, true);
                break;
            case Constants.ReleaseGraphics:
                SetGraphicsHold(ref chr, false);
                break;
            default:
                // this is an invalid code for Prestel but we need attributes to be passed to next char
                //Logging.Log.Warning(
                //    $"ApplyNewAttributes -  Row: {_cursor.Row}, Col:{_cursor.Col}, Value: {(int)chr.Value:X2}");
                chr.Invalid = true;
                break;
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

    #endregion

}