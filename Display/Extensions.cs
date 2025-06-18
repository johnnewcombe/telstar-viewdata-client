/*
    Copyright (c) 2025 John Newcombe
   
    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar. If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TelstarClient.Display;
using Char = TelstarClient.Models.Char;

namespace TelstarClient.Extensions;

public static class Extensions {
    
    #region Char Extensions
    
    /// <summary>
    /// Returns true if the character is 'blast through', i.e. should be displayed
    /// as an alpha character when in graphic mode.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsBlastThrough(this Char chr) {

        if (chr.Graphic && chr.Value >= 0x40 && chr.Value <= 0x5A) {
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
        return chr.Control && (chr.Value >= Constants.AlphaRed && chr.Value <= Constants.AlphaWhite);
    }

    /// <summary>
    /// Returns true if the character is a graphic colour change control.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsGraphicColourChange(this Char chr) {
        return chr.Control && (chr.Value >= Constants.GraphicRed && chr.Value <= Constants.GraphicWhite);
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

    /// <summary>
    /// Returns true if the character represents a new or black (kill) backgroung control.
    /// </summary>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static bool IsBackgroundColourChange(this Char chr) {
        return chr.Control && (chr.Value == Constants.NewBackground || chr.Value == Constants.BlackBackground);
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
            c.Foreground = Constants.DefaultForeground;
            c.Background = Constants.DefaultBackground;
            c.Value = Models.Display.SPC;
            c.InVisible = false;
            c.Control = false;
            c.Flash = false;
            c.Concealed = false;
            c.Separated = false;
            c.Graphic = false;
            c.GraphicsHold = false;
            c.DoubleHeight = false;
        }

        // clear the row references
        for (var i = 0; i < Models.Display.ROWS-1; i++) {
            display.RowReferences[i] = 0;
        }
    }

    public static void Flash(this Models.Display display) {

        // TODO access c.Flash in a threadsafe way as this method is called from
        //   a thread pool thread
        for (var i = 0; i < Models.Display.ROWS * Models.Display.COLS; i++) {
            var c = display.Chars[i];
            if (c.Flash) {
                // TODO lock ??
                c.InVisible = !c.InVisible;
            }
        }
    }

    public static void ToggleConceal(this Models.Display display) {
        for (var i = 0; i < Models.Display.ROWS * Models.Display.COLS; i++) {
            var c = display.Chars[i];
            if (c.Concealed) {
                // TODO lock ??
                c.InVisible = !c.InVisible;
            }
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
        if (row > 0 && display.RowReferences[row - 1] > 0) {
            return true;
        }
        return false;
    }

    public static bool RowHasDoubleHeight(this Models.Display display, int row) {
        if (display.RowReferences[row] > 0) {
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
            throw new ArgumentOutOfRangeException("The row and columm specified do not correspond to a character within the display.");
        }

        return display.Chars[(row * Models.Display.COLS) + col];
    }

    /// <summary>
    /// Returns the character below the specified character from the display.
    /// </summary>
    /// <param name="display"></param>
    /// <param name="chr"></param>
    /// <returns></returns>
    public static Char GetCharBelow(this Models.Display display, Char chr) {
        var index = chr.Index + Models.Display.COLS;
        if (index < Models.Display.ROWS * Models.Display.COLS) {
            return display.Chars[chr.Index + Models.Display.COLS];
        }

        throw new IndexOutOfRangeException("The character requested is beyond the bounds of the display.");
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
    public static void SetStatusText(this Models.Display display, string status,
        string foregroundColour = Constants.Green, string backgroundColour = Constants.Black) {
        
        // clear the row
        for (var i = 0; i < Models.Display.COLS; i++) {
            var cell = display.Chars[24 * Models.Display.COLS + i];
            cell.Value = Models.Display.SPC;
        }
        
        // work out the starting column so that the text is centred
        var col = (Models.Display.COLS / 2) - (status.Length / 2);

        // add the text and attributes to the status row
        foreach (var c in status) {

            var cell = display.Chars[24 * Models.Display.COLS + col];
            cell.Value = c;
            cell.Foreground = foregroundColour;
            cell.Background = backgroundColour;
            col++;

            // belts and braces
            if (col >= Models.Display.COLS) {
                break;
            }
        }
    }
}

# endregion