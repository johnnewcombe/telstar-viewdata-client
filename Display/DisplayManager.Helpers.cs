using System.Diagnostics;
using TelstarClient.Extensions;
using TelstarClient.Models;

namespace TelstarClient.Display;

public partial class DisplayManager {

    private void SetSpecialDisplayValues(ref Char chr) {

        /// Transform Section
        // display blank for controls or hold graphics character if appropriate
        // TODO refactor this
        //  note that any foreground colour changes when HG is active should take on the
        //  old colour not the new one. i.e. _holdGraphics need to store the graphic and colour
        //  and maybe other stuff
        if (chr.IsControl) {

            // sometimes invalid Prestel controls appear in pages that have been imported from
            // elsewhere, the first test page has some of these. These are simply set to a
            // blank character
            if (chr.IsInvalid) {
                chr.Value = Models.Display.SPC;
            }

            else if (chr.IsGraphicsHold) {
                chr.Value = _holdGraphicsCharacter;
                //Trace.WriteLine($" Row: {_cursor.Row}, Col:{_cursor.Col}, GH Value: {(int)chr.Value}");

            }
            else {
                chr.Value = Models.Display.SPC;
            }
        }

        // substitute viewdata characters for suitable font characters as required
        chr.Value = _fontMapper.Map(chr.Value);
    }
/*
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
*/
    /// <summary>
    /// Adds any new attributes to the character that this control character
    /// might require.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    private void ApplyNewAttributes(ref Char chr) {

        //if (!_escapedMode || chr.Value <= 0x40 || chr.Value > 0x5f) {
        //    return;
        //}

        chr.IsControl = true;

        // check to see if we have a foreground change
        if (chr.IsForegroundColourChange()) {
            var colour = _colourMapper.Map(chr.Value);
            SetForeground(ref chr, colour, chr.IsGraphicColourChange());
            return;
        }

        switch (chr.Value) {

            case Constants.Flash: //TODO 
                break;
            case Constants.Steady: //TODO 
                break;
            case Constants.NormalHeight:
                SetNormalHeight(ref chr);
                break;
            case Constants.DoubleHeight:
                SetDoubleHeight(ref chr);
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
                Trace.WriteLine($"ApplyNewAttributes -  Row: {_cursor.Row}, Col:{_cursor.Col}, Value: {(int)chr.Value:X2}");
                chr.IsInvalid = true;
                break;
        }

    }

    /// <summary>
    /// Sets the Graphics Hold status, specify true to hold graphics, false to release.
    /// </summary>
    /// <param name="chr"></param>
    /// <param name="held"></param>
    private void SetGraphicsHold(ref Char chr, bool held) {
        var row = _display.GetRemainderOfRow(_cursor.Row, _cursor.Col);

        chr.IsGraphicsHold = held;

        foreach (var c in row) {

            c.IsGraphicsHold = held;

            // if next char is a colour change then all done
            if (c.IsControl && c.Value == Constants.ReleaseGraphics) {
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

            // if next char is a colour change then all done
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

            // if next char is a colour change then all done
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
            if (c.IsControl && (c.Value == Constants.DoubleHeight ||
                                c.Value == Constants.NormalHeight)) {
                break;
            }

            c.IsDoubleHeight = false;
        }
    }

}