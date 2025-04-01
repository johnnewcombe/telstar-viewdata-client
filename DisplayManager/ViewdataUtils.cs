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

            /*
            # region Not Needed?
            // TODO: not sure this is needed if we are to be scanning the
            //  previos chars in the row each time we update
            
            
            // set the grahics/alpha mode
            if (!_graphicsMode && character >= 0x51 && character <= 0x57) {
                //switch to graphics mode
                _graphicsMode = true;
            }

            if (_graphicsMode && character >= 0x41 && character <= 0x47) {
                //switch to alpha mode
                _graphicsMode = false;
            }

            #endregion
            */
            
            // get current attributes based on the previous Char
            if (_cursor.Col == 0) {
                // use default settings if we are populating col 0
                chr.Background = "White";
                chr.Background = "Black";
            }
            else {
                // get previous char
                var prevChr = _display.Rows[_cursor.Row].Chars[_cursor.Col - 1];
                chr.Background = prevChr.Background;
                chr.Foreground = prevChr.Foreground;
            }
            
            switch (character) {
                case AlphaRed:
                    chr.Foreground = "Red";
                    break;
                case AlphaGreen:
                    chr.Foreground = "Green";
                    break;
                case AlphaYellow:
                    chr.Foreground = "Yellow";
                    break;
                case AlphaBlue:
                    chr.Foreground = "Blue";
                    break;
                case AlphaMagenta:
                    chr.Foreground = "Magenta";
                    break;
                case AlphaCyan:
                    chr.Foreground = "Cyan";
                    break;
                case AlphaWhite:
                    chr.Foreground = "White";
                    break;
            }
            
            
            

        }
        else {
            // not a control code
            chr.IsControl = false;

            if (_graphicsMode) {
                // sort out graphics by selecting the appropriate character in the font
                if (character >= 0x20 && character <= 0x3f) {
                    character += (char)(0xe200 - 0x20);
                }

                if (character >= 0x60 && character <= 0x7f) {
                    character += (char)(0xe220 - 0x60);
                }
            }
            else {
            }
        }

        var row = _display.Rows[_cursor.Row];
        foreach (var r in row.Chars) {
            // TODO: in order to determine the colour and other attributes of the character
            //  we need to look at everything that went before on that row.
        }

        // update the char appropriately
        chr.Value = character;

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
}