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
    private bool _doubleHeight;
    private Display _display;
    private Cursor _cursor;
    private bool _holdGraphics;

    public ViewdataUtils(Display display, Cursor cursor) {
        _display = display;
        _cursor = cursor;
    }

    public char ConvertChar(char character) {
        // process control codes
        // null character will be returned if a control
        if (ProcessControls(character))
            return NullChar; // character CHAR_NULL not null

        // if current row is lower line of a Double Height row then
        // it will be readonly
        if (_display.Rows[_cursor.Row].ReadOnly) {
            return NullChar;
        }

        // if we get here then the current char is not a control code
        // i.e. char >= 0x20, therefore the _escapMode flag, if set,
        // has been set by the previous character.
        if (_escapedMode) {
            
            // reset the escapeMode flag
            _escapedMode = false;

            // check for hold/release graphics
            _holdGraphics = character == HoldGraphics;
            if (character == ReleaseGraphics) {
                _holdGraphics = false;
            }

            Debug.Print("{0}",(int)character);
            // sort out graphics by selecting the appropriate character in the font
            if (character >= 0x20 && character <= 0x3f) {
                character += (char)(0xe200 - 0x20);
            }
            if (character >= 0x60 && character <= 0x7F) {
                character += (char)(0xe220 - 0x60);
            }
            
            
            
            
            
            
        }
        else {
        }

        return character;
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