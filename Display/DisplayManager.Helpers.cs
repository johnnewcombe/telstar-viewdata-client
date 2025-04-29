using System.Diagnostics;
using TelstarClient.Extensions;
using TelstarClient.Models;

namespace TelstarClient.Display;

public partial class DisplayManager {
    
    /// Sets the Graphics Hold status, specify true to hold graphics, false to release.
    /// Adds any new attributes to the character that this control character
    /// might require.
    ///
    ///
    /// Extract from Prestel Terminal Specification:
    /// --------------------------------------------
    /// Generally all control characters are displayed as spaces, implying at least
    /// one space between rectangles with different display colours in the same row.
    /// The hold mosaic mode allows a limited range of abrupt display colour changes
    /// by calling for the display of a held mosaic character in the rectangle
    /// corresponding to any control character occurring during the mosaic mode.
    /// This held character is displayed in the modes obtaining for the rectangle
    /// in which it is displayed, except for the contiguous/separated mode which
    /// forms part of the structure of the held mosaic character.
    /// 
    /// The held mosaic character is only defined during the mosaic mode. It is then
    /// the most recent character in Columns 2a, 3a, 6a or 7a providing that there
    /// has been no intervening change in either the alphanumeric/mosaic or the
    /// normal/double height modes. This character is to be displayed in the
    /// contiguous or separated mode as when it was first displayed. In the absence
    /// of such a character the held mosaic character is taken to be a space. The
    /// hold mosaic mode is released by RELEASE MOSAIC (5b/15).
    /// </summary>
    /// <param name="chr"></param>
    /// <param name="held"></param>
    private void SetGraphicsHold(ref Char chr, bool held) {

        // the graphics hold control itself is set to display the held char if it exists
        if (chr.Value == Constants.HoldGraphics) {
            chr.IsGraphicsHold = held;
        }
        
        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);
 
        foreach (var c in row) {
 
            c.IsGraphicsHold = held;

            // stop?
            if (c.IsControl && (c.Value == Constants.ReleaseGraphics)|| 
                c.Value == Constants.HoldGraphics) {
                break;
            }
        }
    }
    
    /// <summary>
    /// Adds any new attributes to the character that this control character
    /// might require.
    /// </summary>
    /// <param name="chr"></param>
    private void SetSeparatedMode(ref Char chr, bool separated) {

        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);

        foreach (var c in row) {

            c.IsSeparated = separated;

            // stop?
            if (c.IsControl && (c.Value == Constants.Separated ||
                                c.Value == Constants.Contiguous)) {
                break;
            }
        }
    }


// TODO combine with SetBackground??
    /// <summary>
    /// Helper function to set a colour change.
    /// </summary>
    /// <param name="chr"></param>
    /// <param name="colour"></param>
    private void SetForeground(ref Char chr, string colour, bool isGraphic) {

        // set the character's foreground
        //chr.Foreground = colour;

        // the colour change control only affects the chars following
        // the control itself holds the previous colour. This matters
        // where the graphic hold character is used instead of a blank

        // set the colour of the rest of the row
        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);
        
        foreach (var c in row) {

            c.Foreground = colour;
            c.IsGraphic = isGraphic;

            // stop?
            if (c.IsControl && c.IsForegroundColourChange()) {
                break;
            }
        }
    }
    
    /// <summary>
    /// Helper function to set a colour change.
    /// </summary>
    /// <param name="chr"></param>
    /// <param name="colour"></param>
    private void SetBackground(ref Char chr, string colour) {

        // set the character's background
        chr.Background = colour;

        // set the background of the rest of the row
        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);

        foreach (var c in row) {

            // stop?
            if (c.IsControl && c.IsBackgroundColourChange()) {
                break;
            }

            c.Background = colour;

        }
    }

    /// <summary>
    /// Helper function to process a DH control code.
    /// </summary>
    /// <param name="chr"></param>
    private void SetDoubleHeight(ref Char chr) {

        chr.IsDoubleHeight = true;

        // set DH to all chars until EOL or another DH or NH
        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);
        foreach (var c in row) {
            
            // stop?
            if (c.IsControl && (c.Value == Constants.DoubleHeight ||
                                c.Value == Constants.NormalHeight)) {
                break;
            }

            // set DH to all chars until EOL or another DH or NH
            c.IsDoubleHeight = true;

        }

        //get whole row
        row = _display.GetRemainderOfRow(_cursor.Row, 0);
        foreach (var c in row) {
            // copy the colours to all chars in the row below
            var index = c.Index + Models.Display.COLS;
            _display.Chars[index].Background = c.Background;
            _display.Chars[index].Foreground = c.Foreground;
        }
    }

    /// <summary>
    /// Helper function to process a NH control.
    /// </summary>
    /// <param name="chr"></param>
    private void SetNormalHeight(ref Char chr) {

        chr.IsDoubleHeight = false;

        // reset DH to all chars until EOL or another DH or NH
        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);
        foreach (var c in row) {

            // stop?
            if (c.IsControl && (c.Value == Constants.DoubleHeight ||
                                c.Value == Constants.NormalHeight)) {
                break;
            }

            c.IsDoubleHeight = false;
        }
    }
}