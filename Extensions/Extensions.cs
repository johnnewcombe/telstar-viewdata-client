using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Char = TelstarClient.Models.Char;

namespace TelstarClient.Extensions;

public static class Extensions {

    private const string DefaultForeground = "White";
    private const string DefaultBackground = "Black";

    /// <summary>
    /// Clears the display by setting each character back to its default (blank) values.
    /// </summary>
    /// <param name="display"></param>
    public static void Clear(this Models.Display display) {

        foreach (var c in display.Chars) {
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
}