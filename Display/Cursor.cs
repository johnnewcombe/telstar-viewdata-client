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

using TelstarClient.Models;

namespace TelstarClient.Display;

public class Cursor {
    private int _col;
    private int _row;
    private bool _visible = true;

    public Cursor() {
        _col = 0;
        _row = 0;
    }
    public Cursor(int col, int row) {
        _col = 0;
        _row = 0;   
    }
    public Cursor(int col, int row, bool visible) {
        _col = 0;
        _row = 0;   
        Visible = visible;
    }

    public Cursor(int startIndex) {
        _row = startIndex/Models.Display.COLS;
        _col = startIndex%Models.Display.COLS;
    }
    /// <summary>
    /// Returns the current cursor row position.
    /// </summary>
    public int Row {
        set { _row = value; }
        get { return _row; }
    }

    /// <summary>
    /// Returns the current cursor column position.
    /// </summary>
    public int Col {
        set { _col = value; }
        get { return _col; }
    }

    /// <summary>
    /// This property determines whether the cursor
    /// should be visible or not.
    /// </summary>
    public bool Visible {
        get { return _visible; }
        set { _visible = value; }
    }

    public int GetCursorIndex() {
       return Col + Row * Models.Display.COLS;
    }
    
    #region Cursor Movement

    /// <summary>
    /// Increments the cursor. The cursor wraps at the end of the row, and
    /// wraps from the bottom back to the top.
    /// </summary>
    public void HorizontalTab(int count) {
        do {
            HorizontalTab();
            count--;
        }while (count > 0);
    }

    public void HorizontalTab() {
        _col++;
        if (_col < Models.Display.COLS) return;
        _col = 0;
        _row++;
        if (_row == Models.Display.ROWS)
            _row = 0;
    }

    /// <summary>
    /// Backspace.
    /// </summary>
    public void Backspace() {
        _col--;
        if (_col >= 0) return;
        _col = Models.Display.COLS - 1;
        _row--;
        if (_row < 0)
            _row = Models.Display.ROWS - 1;
    }

    /// <summary>
    /// Vertical tab.
    /// </summary>
    public void VerticalTab() {
        _row--;
        if (_row >= 0) return;
        _row = Models.Display.ROWS - 1;
    }

    /// <summary>
    /// Linefeed.
    /// </summary>
    public void LineFeed() {
        _row++;
        if (_row < Models.Display.ROWS) return;
        _row = 0;
    }

    /// <summary>
    /// Carriage return.
    /// </summary>
    public void CarriageReturn() {
        _col = 0;
    }

    /// <summary>
    /// Home the cursor.
    /// </summary>
    public void Home() {
        _row = 0;
        _col = 0;
    }

    #endregion

}