using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging.Effects;
using System.Dynamic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Avalonia.Styling;
using Avalonia.Utilities;
using TelstarClient.Models;
using TelstarClient.Extensions;

namespace TelstarClient.Display;

public class DisplayManager {
    // character constants

    // this is a speacial case with Avalonia in that a space value of 0x20
    // does not render the background so the blank graphic is used instead


    #region Primary Controls C0

    public const char NullChar = '\x00';
    public const char STX = '\x02';
    public const char ETX = '\x03';
    public const char ENQ = '\x03';
    public const char ACK = '\x03';
    public const char BS = '\x08';
    public const char HT = '\x09';
    public const char LF = '\x0a';
    public const char VT = '\x0b';
    public const char HomeClear = '\x0c';
    public const char CR = '\x0d';
    public const char SO = '\x0e';
    public const char SI = '\x0f';
    public const char CurOn = '\x11';
    public const char DC2 = '\x12';
    public const char DC3 = '\x13';
    public const char CurOff = '\x14';
    public const char Esc = '\x1b';
    public const char SS2 = '\x1c';
    public const char SS3 = '\x1d';
    public const char Home = '\x1e';

    #endregion

    # region Parallel Controls C1

    public const char AlphaRed = '\x41';
    public const char AlphaGreen = '\x42';
    public const char AlphaYellow = '\x43';
    public const char AlphaBlue = '\x44';
    public const char AlphaMagenta = '\x45';
    public const char AlphaCyan = '\x46';
    public const char AlphaWhite = '\x47';

    public const char Flash = '\x48';
    public const char Steady = '\x49';
    public const char NormalHeight = '\x4c';
    public const char DoubleHeight = '\x4d';

    public const char GraphicRed = '\x51';
    public const char GraphicGreen = '\x52';
    public const char GraphicYellow = '\x53';
    public const char GraphicBlue = '\x54';
    public const char GraphicMagenta = '\x55';
    public const char GraphicCyan = '\x56';
    public const char GraphicWhite = '\x57';

    public const char Conceal = '\x58';
    public const char Contiguous = '\x59';
    public const char Separated = '\x5a';
    public const char BlackBackground = '\x5c';
    public const char NewBackground = '\x5d';
    public const char HoldGraphics = '\x5e';
    public const char ReleaseGraphics = '\x5f';

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
    
    private const string Offline = "Brown";
    private const string Online = "Green";
    private const string Connecting = "Yellow";
    #endregion

    # region Misc Constants
    
    private const int StatusPadding = 10;

    #endregion

    #region Private Variables

    private bool _escapedMode;
    private bool _graphicsMode;
    private Models.Display _display;
    private Cursor _cursor;
    private char _holdGraphicsCharacter = ' ';
    private FontMapper _fontMapper;

    #endregion

    public DisplayManager() {
        _display = CreateDisplay();
        _cursor = new Cursor();
        _fontMapper = new FontMapper();
    }

    public Models.Display Display {
        set { _display = value; }
        get { return _display; }
    }

    public void  Write(string text) {
        foreach (var c in text) {
            WriteChar(c);
        }
    }
    public bool WriteChar(char character) {
        //var result = new List<Char>();
        var result = false;

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

            if (chr.IsGraphic) {

                int graphicsBase;

                graphicsBase = chr.IsSeparated ? 0xe2c0 : 0xe200;
                /*
                 * Normal graphics the base numbers are
                 * e200 for 20-3f
                 * e220 for 60-7f
                 *
                 * Seperated
                 * e2c0 for 20-3f
                 * e2e0 for 60-7f
                 */

                // sort out graphics by selecting the appropriate character in the font
                if (chr.Value >= 0x20 && chr.Value <= 0x3f) {
                    //chr.Value += (char)(0xe200 - 0x20);
                    chr.Value += (char)(graphicsBase - 0x20);
                }

                if (chr.Value >= 0x60 && chr.Value <= 0x7f) {
                    //chr.Value += (char)(0xe220 - 0x60);
                    chr.Value += (char)(graphicsBase - 0x40);
                }

                _holdGraphicsCharacter = chr.Value;
            }
        }

        // Move cursor for next character, irrespective of
        // how many are being updated or whether this is a parallel control
        // or a normal character.
        // Note that we will not reach this code for any C0 controls.
        _cursor.HorizontalTab();

        // update the char appropriately
        // we return a list as it may be necessary to update the rest of a row.

        // substitute viewdata characters for suitable font characters as required
        chr.Value=_fontMapper.Map(chr.Value);

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
        Display.SetStatusText(1,"Online".PadRight(StatusPadding),Online);
    }

    public void SetStatusOffline() {
        Display.SetStatusText(1,"Offline".PadRight(StatusPadding),Offline);
    }
    public void SetStatusConnecting() {
        Display.SetStatusText(1,"Connecting".PadRight(StatusPadding),Connecting);
    }
    
    private bool ProcessC0Controls(char character) {

        // is this a Control code
        if (character >= 0x20) return false;

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
                // update display model
                // the UI will be updated
                _display.Clear();
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
    /// Adds new attributes based on the control character passed.
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
            chr.IsSeparated = false;
            chr.IsConcealed = false;
        }
        else {
            // get previous char
            chr.Foreground = prevChr.Foreground;
            chr.Background = prevChr.Background;
            chr.IsGraphic = prevChr.IsGraphic;
            chr.IsControl = prevChr.IsControl;
            chr.IsSeparated = prevChr.IsSeparated;
            chr.IsConcealed = prevChr.IsConcealed;
            chr.IsGraphicsHold = prevChr.IsGraphicsHold;
        }
    }

    /// <summary>
    /// Adds any new attributes that this control character might require.
    /// Returns a list of chars in addition to the argument passed, that
    /// need the UI display updating e.g. a New Background might require
    /// the rest of the row to be updated.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    private bool ApplyNewAttributes(ref Char chr) {

        var result = true;

        if (!_escapedMode || chr.Value <= 0x40 || chr.Value > 0x5f) {
            return result;
        }

        chr.IsControl = true;

        var prevChr = GetPreviousCharacter();

        // TODO: We need to consider the following.
        //  * Control characters can be placed anywhere
        //    on the screen and often affect the remainder
        //    of the row up until the next control char.
        //  * A colour change will need to update all
        //    following the control up until the next
        //    colour change.
        //  * A new background needs to update the
        //    rest of the row up until the next
        //    NewBackground or BlackBackground.
        //  * We only need to update display chars
        //    that are different.

        switch (chr.Value) {

            case AlphaRed:
                chr.Foreground = Red;
                chr.IsGraphic = false;
                break;
            case AlphaGreen:
                chr.Foreground = Green;
                chr.IsGraphic = false;
                break;
            case AlphaYellow:
                chr.Foreground = Yellow;
                chr.IsGraphic = false;
                break;
            case AlphaBlue:
                chr.Foreground = Blue;
                chr.IsGraphic = false;
                break;
            case AlphaMagenta:
                chr.Foreground = Magenta;
                chr.IsGraphic = false;
                break;
            case AlphaCyan:
                chr.Foreground = Cyan;
                chr.IsGraphic = false;
                break;
            case AlphaWhite:
                chr.Foreground = White;
                chr.IsGraphic = false;
                break;

            case Flash:
                break;
            case Steady:
                break;

            case NormalHeight:
                break;
            case DoubleHeight:
                break;

            case GraphicRed:
                chr.Foreground = Red;
                chr.IsGraphic = true;
                break;
            case GraphicGreen:
                chr.Foreground = Green;
                chr.IsGraphic = true;
                break;
            case GraphicYellow:
                chr.Foreground = Yellow;
                chr.IsGraphic = true;
                break;
            case GraphicBlue:
                chr.Foreground = Blue;
                chr.IsGraphic = true;
                break;
            case GraphicMagenta:
                chr.Foreground = Magenta;
                chr.IsGraphic = true;
                break;
            case GraphicCyan:
                chr.Foreground = Cyan;
                chr.IsGraphic = true;
                break;
            case GraphicWhite:
                chr.Foreground = White;
                chr.IsGraphic = true;
                break;

            case Conceal:
                break;
            case Contiguous:
                chr.IsSeparated = false;
                break;
            case Separated:
                chr.IsSeparated = true;
                break;

            case NewBackground:

                var colour = prevChr is null ? White : prevChr.Foreground;

                // set the character's background
                chr.Background = colour;

                // set the background of the rest of the row
                var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);
                
                foreach (var r in row) {
                    // TODO: check for a control that would cancel this e.g. another
                    //  NewBackground or an BlackBackground
                    r.Background = colour;
                }

                // the Chars in the row are deep clones of those in the display
                //result.AddRange(row);
                break;

            case BlackBackground:

                // set the character's background
                chr.Background = Black;

                // set the background of the rest of the row
                row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);

                foreach (var r in row) {
                    // TODO: check for a control that would cancel this e.g. NewBackground
                    r.Background = Black;
                }

                //result.AddRange(row);
                break;

            case HoldGraphics:
                chr.IsGraphicsHold = true;
                break;

            case ReleaseGraphics:
                chr.IsGraphicsHold = false;
                break;
            default:
                result = false;
                break;
        }

        return result;
    }

    private Models.Display CreateDisplay() {

        var display = new Models.Display();
        display.Chars = new List<Char>();

        // note that we are creating a 40*25 screen not a 40*24,
        // the last lie will be used for status info
        for (var i = 0; i < (Models.Display.ROWS+1) * Models.Display.COLS; i++) {
            var chr = new Char(Models.Display.SPC, White, Black);
            chr.Index = i;
            display.Chars.Add(chr);
        }

        return display;
    }
    
}