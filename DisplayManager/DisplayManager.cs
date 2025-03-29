using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using Avalonia.Controls.ApplicationLifetimes;
using TelstarClient.Models;

namespace TelstarClient.DisplayManager;

public class DisplayManager
{
    private Models.Display _display;
    private int _col;
    private int _row;

    public DisplayManager()
    {
        _display = new Display();
    }

    public char LastCharacter { private set; get; }
    
    public List<int> GetDisplay()
    {
        var results = new List<int>();

        foreach (var row in _display.Rows)
        {
            foreach (var cell in row.Cells)
            {
                results.Add(cell.Character);
            }
        }

        return results;
    }

    public void Print(string text)
    {
        foreach (char c in text)
        {
            //TODO: sort this out... 
            _display.Rows[_row].Cells[_col].Character = c;
            LastCharacter = c;
        }
    }

    public int CurrentRow
    {
        get { return _row; }
    }

    public int CurrentCol
    {
        get { return _col; }
    }

    /// <summary>
    /// Increments cursor. The cursor wrapps at the end of the rom, and
    /// the wraps from the bottom back to the top.
    /// </summary>
    private void incrementCursor()
    {
        _col++;
        if (_col == Display.COLS)
        {
            _col = 0;
            _row++;
            if (_row == Display.ROWS)
            {
                _row = 0;
            }
        }
    }
}