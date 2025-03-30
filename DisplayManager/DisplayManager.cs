using TelstarClient.Models;

namespace TelstarClient.DisplayManager;

/// <summary>
/// This class manages the Display model. Methods here update the Display model
/// and provide all the Viewdata decoding etc., 
/// </summary>
public class DisplayManager
{
    private Display _display;
    private int _col;
    private int _row;

    /// <summary>
    /// Contructor which creates a new Display Model.
    /// </summary>
    public DisplayManager()
    {
        _display = new Display();
    }

    /// <summary>
    /// Returns the last character sent to the Print method.
    /// </summary>
    public char LastCharacter { private set; get; }

    /// <summary>
    /// Places a single Unicode 16 character in the Display model
    /// the current cursor position.
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public (int, char) PrintChar(char character)
    {
        _display.Rows[_row].Cells[_col].Character = character;
        LastCharacter = character;
        horozontalTab();

        // return position index and character as a tuple
        return (_row * _col + _col, character);
    }

    /// <summary>
    /// Returns the current cursor row position.
    /// </summary>
    public int CurrentRow
    {
        get { return _row; }
    }

    /// <summary>
    /// Returns the current cursor column position.
    /// </summary>
    public int CurrentCol
    {
        get { return _col; }
    }

    #region Cursor Movement
    
    /// <summary>
    /// Increments the cursor. The cursor wraps at the end of the row, and
    /// wraps from the bottom back to the top.
    /// </summary>
    private void horozontalTab()
    {
        _col++;
        if (_col < Display.COLS) return;
        _col = 0;
        _row++;
        if (_row == Display.ROWS)
            _row = 0;
    }

    private void backSpace()
    {
        _col--;
        if (_col >= 0) return;
        _col = Display.COLS - 1;
        _row--;
        if (_row < 0)
            _row = Display.ROWS - 1;
    }

    private void verticalTab()
    {
        _row--;
        if (_row >= 0) return;
        _row = Display.ROWS - 1;
    }
    private void lineFeed()
    {
        _row++;
        if (_row < Display.ROWS) return;
        _row = 0;
    }
    private void carraigeReturn()
    {
        _col = 0;
    }
    
    #endregion

}