using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging.Effects;
using System.IO.Pipelines;
using TelstarClient.Models;

namespace TelstarClient.DisplayManager;

public class ViewdataUtils {
    // character constants

    #region Primary Controls C0

    private const char NullChar = '\x00';
    private const char BS = '\x08';
    private const char HT = '\x09';
    private const char LF = '\x0a';
    private const char VT = '\x0b';
    private const char Home = '\x0c';
    private const char HomeClear = '\x1e';
    private const char CR = '\x0d';
    private const char Esc = '\x1b';

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

    private bool _escapedMode;
    private bool _graphicsMode;
    private bool _doubleHeight;
    private Display _display;
    private Cursor _cursor;
    private bool _holdGraphics;
    private char _holdGraphicsCharacter;

    public ViewdataUtils(Display display, Cursor cursor) {
        _display = display;
        _cursor = cursor;
    }

    public List<Char> ProcessChar(char character) {
        var result = new List<Char>();

        // process control codes
        // null character will be returned if a control
        // TODO: we need to returm more than the character e.g. is it a control code
        // and the attributes changed
        if (ProcessControls(character)) {
            // nothing to update so return empty list of chars.
            return result;
        }

        // if current row is lower line of a Double Height row then
        // it will be readonly
        if (_display.Rows[_cursor.Row].ReadOnly) {
            return result;
        }

        // get the current character from the position index e.g. 0-959
        var chr = _display.Rows[_cursor.Row].Chars[_cursor.Col];
        
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
            chr.IsControl = true;


            // reset the escapeMode flag
            _escapedMode = false;

            // check for hold/release graphics code and set the flag accordingly
            _holdGraphics = character == HoldGraphics;
            if (character == ReleaseGraphics) {
                _holdGraphics = false;
            }

            // first get the attributes from the previos cell (or defaults if col 0)
            chr = ApplyCurrentAttributes(chr);
            chr = ApplyNewAttributes(chr);

        }
        else {
            // not a control code
            chr.IsControl = false;
            chr = ApplyCurrentAttributes(chr);
            
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

        // update the char appropriately
        // we return a list as it may be necessary to update the rest of a row.
        return new List<Char>() { chr };
    }

    private bool ProcessControls(char character) {
        // is this a Control code
        var result = character < 0x20;

        // if any of these get detected then CHAR_NULL character is returned otherwise
        // the passed character is returned unaltered.
        switch (character) {
            case '\x08':
                _cursor.Backspace();
                break;
            case '\x09':
                _cursor.HorizontalTab();
                break;
            case '\x0a':
                _cursor.LineFeed();
                break;
            case '\x0b':
                _cursor.VerticalTab();
                break;
            case '\x0c':
            case '\x1e':
                _cursor.Home();
                break;
            case '\x0d':
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

    private Char ApplyCurrentAttributes(Char chr) {
        // get current attributes based on the previous Char
        if (_cursor.Col == 0) {
            // use default settings if we are populating col 0
            chr.Background = "White";
            chr.Background = "Black";
            chr.IsGraphic = false;
        }
        else {
            // get previous char
            var prevChr = _display.Rows[_cursor.Row].Chars[_cursor.Col - 1];
            chr.Foreground = prevChr.Foreground;
            chr.Background = prevChr.Background;
            chr.IsGraphic = prevChr.IsGraphic;
        }

        return chr;
    }

    private Char ApplyNewAttributes(Char chr) {
        
                    // apply any new attributes on top
            switch (chr.Value) {
                case AlphaRed:
                    chr.Foreground = "Red";
                    chr.IsGraphic = false;
                    break;
                case AlphaGreen:
                    chr.Foreground = "Green";
                    chr.IsGraphic = false;
                    break;
                case AlphaYellow:
                    chr.Foreground = "Yellow";
                    chr.IsGraphic = false;
                    break;
                case AlphaBlue:
                    chr.Foreground = "Blue";
                    chr.IsGraphic = false;
                    break;
                case AlphaMagenta:
                    chr.Foreground = "Magenta";
                    chr.IsGraphic = false;
                    break;
                case AlphaCyan:
                    chr.Foreground = "Cyan";
                    chr.IsGraphic = false;
                    break;
                case AlphaWhite:
                    chr.Foreground = "White";
                    chr.IsGraphic = false;
                    break;
                
                case GraphicRed:
                    chr.Foreground = "Red";
                    chr.IsGraphic = true;
                    break;
                case GraphicGreen:
                    chr.Foreground = "Green";
                    chr.IsGraphic = true;
                    break;
                case GraphicYellow:
                    chr.Foreground = "Yellow";
                    chr.IsGraphic = true;
                    break;
                case GraphicBlue:
                    chr.Foreground = "Blue";
                    chr.IsGraphic = true;
                    break;
                case GraphicMagenta:
                    chr.Foreground = "Magenta";
                    chr.IsGraphic = true;
                    break;
                case GraphicCyan:
                    chr.Foreground = "Cyan";
                    chr.IsGraphic = true;
                    break;
                case GraphicWhite:
                    chr.Foreground = "White";
                    chr.IsGraphic = true;
                    break;
            }

            return chr;
    }
}