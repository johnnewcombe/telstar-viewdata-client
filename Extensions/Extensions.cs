using System;
using System.Collections.Generic;
using Char = TelstarClient.Models.Char;

namespace TelstarClient.Extensions;

public static class Extensions {

    private const string DefaultForeground = "White";
    private const string DefaultBackground = "Black";

    #region Char Extensions
    
    /// <summary>
    /// All the properties of this object, except the Value, are copied to the
    /// destination object
    /// </summary>
    /// <param name="chr"></param>
    /// <param name="dest"></param>
    public static void CloneAttributes(this Char chr, ref Char dest) {

        ArgumentNullException.ThrowIfNull(dest);

        dest.Foreground = chr.Foreground;
        dest.Background = chr.Background;
        dest.IsGraphic = chr.IsGraphic;
        dest.IsControl = chr.IsControl;
        dest.IsSeparated = chr.IsSeparated;
        dest.IsConcealed = chr.IsConcealed;
        dest.IsGraphicsHold = chr.IsGraphicsHold;
        dest.IsDoubleHeight = chr.IsDoubleHeight;
        //dest.IsDoubleHeightLower = chr.IsDoubleHeightLower;
        
    }

    /// <summary>
    /// Sets the object properties, except the Value, to default values.
    /// </summary>
    /// <param name="chr"></param>
    public static void SetDefaultAttributes(this Char chr) {
        chr.Background = "White";
        chr.Background = "Black";
        chr.IsGraphic = false;
        chr.IsControl = false;
        chr.IsSeparated = false;
        chr.IsConcealed = false;
        chr.IsGraphicsHold = false;
        chr.IsDoubleHeight = false;
        //chr.IsDoubleHeightLower = false;
    }

    /// <summary>
    /// Returns true if the character is 'blast through', i.e. should be displayed
    /// as an alpha character when in graphic mode.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsBlastThrough(this Char chr) {

        if (chr.IsGraphic && chr.Value >= 0x40 && chr.Value <= 0x5A) {
            return true;
        }
        return false;
    } 

    /// <summary>
    /// Returns true if the character is an lpha colour change control.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsAlphaColourChange(this Char chr) {
        return chr.IsControl && (chr.Value > 0x40 && chr.Value <= 0x47);
    }

    /// <summary>
    /// Returns true if the character is a graphic colour change control.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsGraphicColourChange(this Char chr) {
        return chr.IsControl && (chr.Value > 0x50 && chr.Value <= 0x57);
    }

    /// <summary>
    /// Returns true if the character is either an Alpha colour change control or a
    /// graphic colour change control..
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsForegroundColourChange(this Char chr) {
        return IsAlphaColourChange(chr) || IsGraphicColourChange(chr);
    }

    #endregion

    #region Display Extensions

    /// <summary>
    /// Clears the display by setting each character back to its default (blank) values.
    /// </summary>
    /// <param name="display"></param>
    public static void Clear(this Models.Display display) {

        // display is 25 rows (0-24) but we don't clear the last (status) row
        for (var i = 0; i < Models.Display.ROWS * Models.Display.COLS; i++) {
            var c = display.Chars[i];
            c.Foreground = DefaultForeground;
            c.Background = DefaultBackground;
            c.Value = Models.Display.SPC;
            c.IsControl = false;
            c.IsConcealed = false;
            c.IsSeparated = false;
            c.IsGraphic = false;
            c.IsGraphicsHold = false;
        }
    }

    /// <summary>
    /// Determines if the specified row is read only. This is typically used to protect the lower row
    /// of a double height row from being overwritten.
    /// </summary>
    /// <param name="display"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public static bool IsRowReadOnly(this Models.Display display, int row) {

        // the row will be read only if the row above has a double height reference
        // TODO: This is not true as a DH followed by a NH would give a reference of 0 i.e.
        //  not readonly but it should be as any DH height on the row requires the row below to be blank.
        if (row > 0 && display.RowReferences[row - 1] > 1) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a reference to the character at position specified by row and column.
    /// </summary>
    /// <param name="display"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Char GetChar(this Models.Display display, int row, int col) {

        if (row < 0 || row >= Models.Display.ROWS || col < 0 || col >= Models.Display.COLS) {
            throw new ArgumentOutOfRangeException();
        }

        return display.Chars[(row * Models.Display.COLS) + col];
    }

    /// <summary>
    /// Returns all Char positions from after current cursor position
    /// to the end of the row, i.e. NOT including the character at the
    /// current position. 
    /// </summary>
    /// <returns></returns>
    public static List<Char> GetRemainderOfRow(this Models.Display display, int row, int col) {

        var result = new List<Char>();

        for (var i = col + 1; i < Models.Display.COLS; i++) {
            //results.Add(_display.Rows[_cursor.Row].Chars[i]);
            result.Add(display.GetChar(row, i));
        }

        return result;
    }

    /// <summary>
    /// Displays a status message on the 25th row (row 24) of the display starting at the specified column.
    /// The colour values are those defined by Avalonia.
    /// </summary>
    /// <param name="display"></param>
    /// <param name="col"></param>
    /// <param name="status"></param>
    /// <param name="foregroundColour"></param>
    /// <param name="backgroundColour"></param>
    public static void SetStatusText(this Models.Display display, int col, string status,
        string foregroundColour = "Yellow", string backgroundColour = "Black") {

        foreach (var c in status) {

            var cell = display.Chars[24 * Models.Display.COLS + col];
            cell.Value = c;
            cell.Foreground = foregroundColour;
            cell.Background = DefaultBackground;
            col++;

            if (col >= Models.Display.COLS) {
                break;
            }

        }

    }
}

# endregion