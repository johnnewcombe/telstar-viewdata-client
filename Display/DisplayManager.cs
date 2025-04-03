using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging.Effects;
using System.Dynamic;
using System.IO.Pipelines;
using Avalonia.Styling;
using TelstarClient.Models;

namespace TelstarClient.DisplayManager;

public class ViewdataUtils {
    // character constants

    #region Primary Controls C0

    private const char NullChar = '\x00';
    private const char STX = '\x02';
    private const char ETX = '\x03';
    private const char ENQ = '\x03';
    private const char ACK = '\x03';
    private const char BS = '\x08';
    private const char HT = '\x09';
    private const char LF = '\x0a';
    private const char VT = '\x0b';
    private const char HomeClear = '\x0c';
    private const char CR = '\x0d';
    private const char SO = '\x0e';
    private const char SI = '\x0f';
    private const char CurOn = '\x11';
    private const char DC2 = '\x12';
    private const char DC3 = '\x13';
    private const char CurOff = '\x14';
    private const char Esc = '\x1b';
    private const char SS2 = '\x1c';
    private const char SS3 = '\x1d';
    private const char Home = '\x1e';

    #endregion

    # region Parallel Controls C1

    private const char AlphaRed = '\x41';
    private const char AlphaGreen = '\x42';
    private const char AlphaYellow = '\x43';
    private const char AlphaBlue = '\x44';
    private const char AlphaMagenta = '\x45';
    private const char AlphaCyan = '\x46';
    private const char AlphaWhite = '\x47';
    private const char Flash = '\x48';
    private const char Steady = '\x49';
    private const char NormalHeight = '\x4c';
    private const char DoubleHeight = '\x4d';
    private const char GraphicRed = '\x51';
    private const char GraphicGreen = '\x52';
    private const char GraphicYellow = '\x53';
    private const char GraphicBlue = '\x54';
    private const char GraphicMagenta = '\x55';
    private const char GraphicCyan = '\x56';
    private const char GraphicWhite = '\x57';
    private const char Conceal = '\x58';
    private const char Contiguous = '\x59';
    private const char Separated = '\x5a';
    private const char BlackBackground = '\x5c';
    private const char NewBackground = '\x5d';
    private const char HoldGraphics = '\x5e';
    private const char ReleaseGraphics = '\x5f';

    #endregion

    #region Avalonia Colours
    
    private const string Red = "Red";
    private const string Green = "Chartreuse";
    private const string Yellow = "Yellow";
    private const string Blue = "Blue";
    private const string Magenta = "Magenta";
    private const string Cyan = "Cyan";
    private const string White = "White";
    private const string Black = "Black";
    
    #endregion

    #region Private Variables
    
    private bool _escapedMode;
    private bool _graphicsMode;
    private bool _doubleHeight;
    private Display _display;
    private Cursor _cursor;
    private bool _holdGraphics;
    private char _holdGraphicsCharacter;

    #endregion
    
    public ViewdataUtils() {
        _display = CreateDisplay();
        _cursor = new Cursor();
    }

    public List<Char> ProcessChar(char character) {
        var result = new List<Char>();

        // process control codes and any attributes changed
        if (ProcessC0Controls(character)) {
            if (character == HomeClear) {
                // return a full screen full of blank characters
                result.AddRange(ClearScreen());
            }

            // nothing to update so return empty list of chars.
            return result;
        }

        // if current row is read only e.g. lower line of a Double Height row then
        // it will be readonly
        if (_display.Rows[_cursor.Row].ReadOnly) {
            return result;
        }

        // get the current character from the position index e.g. 0-959
        var chr = _display.Rows[_cursor.Row].Chars[_cursor.Col];

        // first get the attributes from the previous cell (or defaults if col 0)
        ApplyCurrentAttributes(ref chr);

        // update the value, this could be a control code or an alpha mosaic
        // and could get changed if we needed to point to a graphic or double
        // height character etc. within the font.
        chr.Value = character;

        // if we get here then the current char is not a C0 control code
        // (char < 0x20)
        if (_escapedMode) {
            // if the _escapeMode flag is set then this has been done
            // by the previous character and the current character is
            // a viewdata control code so set the flag. Note that the
            // actual control code is stored in the display model,
            // however, a space (or held graphic) will be displayed
            // in the UI.

            // reset the escapeMode flag
            _escapedMode = false;

            // check for hold/release graphics code and set the flag accordingly
            _holdGraphics = character == HoldGraphics;
            if (character == ReleaseGraphics) {
                _holdGraphics = false;
            }

            // apply new attributes from this control code
            result.AddRange(ApplyNewAttributes(ref chr));

        }
        else {

            // not a control code
            chr.IsControl = false;

            if (chr.IsGraphic) {
                // sort out graphics by selecting the appropriate character in the font
                if (chr.Value >= 0x20 && chr.Value <= 0x3f) {
                    chr.Value += (char)(0xe200 - 0x20);
                }

                if (chr.Value >= 0x60 && chr.Value <= 0x7f) {
                    chr.Value += (char)(0xe220 - 0x60);
                }
            }
        }

        // Move cursor for next character, irrespective of
        // how many are being updated or whether this is a parallel control
        // or a normal character.
        // Note that we will not reach this code for any C0 controls.
        _cursor.HorizontalTab();
        
        // update the char appropriately
        // we return a list as it may be necessary to update the rest of a row.
        result.Insert(0, chr);
        return result;
    }

    private bool ProcessC0Controls(char character) {
        // is this a Control code
        var result = character < 0x20;

        // if any of these get detected then CHAR_NULL character is returned otherwise
        // the passed character is returned unaltered.
        switch (character) {
            case BS:
                _cursor.Backspace();
                break;
            case HT:
                _cursor.HorizontalTab();
                break;
            case LF:
                _cursor.LineFeed();
                break;
            case VT:
                _cursor.VerticalTab();
                break;
            case HomeClear:
                // update display
                _display = CreateDisplay();
                _cursor.Home();
                break;
            case Home:
                _cursor.Home();
                break;
            case CR:
                _cursor.CarriageReturn();
                break;
            case '\x11':
                _cursor.Visible = true;
                break;
            case '\x14':
                _cursor.Visible = false;
                break;
            case '\x1b':
                _escapedMode = true;
                break;
        }

        return result;
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

        return _display.Rows[_cursor.Row].Chars[_cursor.Col - 1];
    }

    /// <summary>
    /// Returns all Char positions from current cursor position
    /// to the end of the row.
    /// </summary>
    /// <returns></returns>
    private List<Char> GetToEndOfRow() {

        var results = new List<Char>();

        for (var i = _cursor.Col; i < Display.ROWS; i++) {
            results.Add(_display.Rows[_cursor.Row].Chars[i]);
        }

        return results;
    }

    /// <summary>
    /// Adds new attributes based on the control character passed.
    /// Returns a bool indicating whether the remainder of the row
    /// needs updating.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    private void ApplyCurrentAttributes(ref Char chr) {

        var updateds = new List<Char>();
        var prevChr = GetPreviousCharacter();

        // get current attributes based on the previous Char
        if (prevChr == null) {
            // must be at the start of a row so use default settings
            chr.Background = "White";
            chr.Background = "Black";
            chr.IsGraphic = false;
            chr.IsControl = false;
        }
        else {
            // get previous char
            chr.Foreground = prevChr.Foreground;
            chr.Background = prevChr.Background;
            chr.IsGraphic = prevChr.IsGraphic;

        }
    }

    private List<Char> ApplyNewAttributes(ref Char chr) {

        // TODO: Refactor this! to simplyfy it

        // apply any new attributes on top
        var result = new List<Char>();
        var prevChr = GetPreviousCharacter();

        switch (chr.Value) {

            case AlphaRed:
                chr.Foreground = Red;
                chr.IsGraphic = false;
                chr.IsControl = true;
                break;
            case AlphaGreen:
                chr.Foreground = Green;
                chr.IsGraphic = false;
                chr.IsControl = true;
                break;
            case AlphaYellow:
                chr.Foreground = Yellow;
                chr.IsGraphic = false;
                chr.IsControl = true;
                break;
            case AlphaBlue:
                chr.Foreground = Blue;
                chr.IsGraphic = false;
                chr.IsControl = true;
                break;
            case AlphaMagenta:
                chr.Foreground = Magenta;
                chr.IsGraphic = false;
                chr.IsControl = true;
                break;
            case AlphaCyan:
                chr.Foreground = Cyan;
                chr.IsGraphic = false;
                chr.IsControl = true;
                break;
            case AlphaWhite:
                chr.Foreground = White;
                chr.IsGraphic = false;
                chr.IsControl = true;
                break;

            case GraphicRed:
                chr.Foreground = Red;
                chr.IsGraphic = true;
                chr.IsControl = true;
                break;
            case GraphicGreen:
                chr.Foreground = Green;
                chr.IsGraphic = true;
                chr.IsControl = true;
                break;
            case GraphicYellow:
                chr.Foreground = Yellow;
                chr.IsGraphic = true;
                chr.IsControl = true;
                break;
            case GraphicBlue:
                chr.Foreground = Blue;
                chr.IsGraphic = true;
                chr.IsControl = true;
                break;
            case GraphicMagenta:
                chr.Foreground = Magenta;
                chr.IsGraphic = true;
                chr.IsControl = true;
                break;
            case GraphicCyan:
                chr.Foreground = Cyan;
                chr.IsGraphic = true;
                chr.IsControl = true;
                break;
            case GraphicWhite:
                chr.Foreground = White;
                chr.IsGraphic = true;
                chr.IsControl = true;
                break;
            case NewBackground:
                chr.IsControl = true;

                var colour = prevChr is null ? White : prevChr.Foreground;
                var row = GetToEndOfRow();
                foreach (var r in row) {
                    r.Background = colour;
                }

                result.AddRange(row);
                break;

            case BlackBackground:
                colour = Black;
                row = GetToEndOfRow();
                foreach (var r in row) {
                    r.Background = colour;
                }

                result.AddRange(row);

                break;

        }

        return result;
    }

    private Display CreateDisplay() {
        var index = 0;

        var display = new Display();
        display.Rows = new List<Row>();

        for (var i = 0; i < Display.ROWS; i++) {
            var row = new Row(false);
            row.Chars = new List<Char>();

            for (var j = 0; j < Display.COLS; j++) {
                var chr = new Char(' ', "White", "Black");
                chr.Index = index++;
                row.Chars.Add(chr);
            }

            display.Rows.Add(row);
        }

        return display;
    }

    /// <summary>
    /// Clear the screen by creating a new one.
    /// </summary>
    private List<Char> ClearScreen() {
        // clear the model data

        // list of Chars to be returned these
        // will be used to update the UI
        var clearScreen = new List<Char>();
        for (var i = 0; i < Display.COLS * Display.ROWS; i++) {
            var c= new Char(' ', "Black", "White");
            c.Index = i;
            clearScreen.Add(c);
        }

        return clearScreen;
    }
}