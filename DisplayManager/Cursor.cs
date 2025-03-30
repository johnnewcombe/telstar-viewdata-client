using TelstarClient.Models;

namespace TelstarClient.DisplayManager;

public class Cursor
{
    private int _col;
    private int _row;
    private bool _visible = true;

    /// <summary>
    /// Returns the current cursor row position.
    /// </summary>
    public int Row
    {
        get { return _row; }
    }

    /// <summary>
    /// Returns the current cursor column position.
    /// </summary>
    public int Col
    {
        get { return _col; }
    }

    /// <summary>
    /// This property determines whether the cursor
    /// should be visible or not.
    /// </summary>
    public bool Visible
    {
        get { return _visible; }
        set { _visible = value; }
    }

    #region Cursor Movement

    /// <summary>
    /// Increments the cursor. The cursor wraps at the end of the row, and
    /// wraps from the bottom back to the top.
    /// </summary>
    public void HorizontalTab()
    {
        _col++;
        if (_col < Display.COLS) return;
        _col = 0;
        _row++;
        if (_row == Display.ROWS)
            _row = 0;
    }

    /// <summary>
    /// Backspace.
    /// </summary>
    public void Backspace()
    {
        _col--;
        if (_col >= 0) return;
        _col = Display.COLS - 1;
        _row--;
        if (_row < 0)
            _row = Display.ROWS - 1;
    }

    /// <summary>
    /// Vertical tab.
    /// </summary>
    public void VerticalTab()
    {
        _row--;
        if (_row >= 0) return;
        _row = Display.ROWS - 1;
    }

    /// <summary>
    /// Linefeed.
    /// </summary>
    public void LineFeed()
    {
        _row++;
        if (_row < Display.ROWS) return;
        _row = 0;
    }

    /// <summary>
    /// Carriage return.
    /// </summary>
    public void CarriageReturn()
    {
        _col = 0;
    }

    /// <summary>
    /// Home the cursor.
    /// </summary>
    public void Home()
    {
        _row = 0;
        _col = 0;
    }

    #endregion
}