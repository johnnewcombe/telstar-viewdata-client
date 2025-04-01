using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging.Effects;
using System.IO.Pipelines;
using TelstarClient.Models;

namespace TelstarClient.DisplayManager;

public class ViewdataUtils {
    // character constants
    private const char NullChar = '\x00';
    private const char BS = '\x08';
    private const char HT = '\x09';
    private const char LF = '\x0a';
    private const char VT = '\x0b';
    private const char Home = '\x0c';
    private const char HomeClear = '\x1e';
    private const char CR = '\x0d';
    private const char Esc = '\x1b';

    /* 41-49
     * r g y b m c w f s
     *
     * 4c-4d
     * n d
     *
     * 51-5a
     * r g y b m c w conceal, contig, sep,
     *
     * 5c-5f
     * black back, new back, hold, release


     */
    private const char DoubleHeight = '\x4c';
    private const char NormalHeight = '\x4d';

    private const char HoldGraphics = '\x5e';
    private const char ReleaseGraphics = '\x5f';

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

            // TODO: not sure this is needed if we are to be scanning the
            //  current row each time
            # region Not Needed?
            
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

            // the control viewdata code is replaced by a space or a hold graphic
            character = _holdGraphics ? _holdGraphicsCharacter : ' ';
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
        chr.Foreground = "yellow";
        chr.Background = "Black";

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

        // reset graphics mode and escapeMode if column == 0
        if (_cursor.Col == 0) {
            //_graphicsMode = false;
            //_escapedMode = false;
        }

        return result;
    }
}