using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaApplication1.Models;

public class Display 
{
    public Display()
    {
        Rows = new List<Row>();
        for (var i = 0; i < Globals.ROWS; i++)
        {
            var row = new Row(true);
            Rows.Add(row);
        }
    }
    public List<Row> Rows { get; set; }
}

public class Row 
{
    public Row(bool visible)
    {
        Cells = new List<Cell>();
        Visible = visible;
            
        for (var i = 0; i < Globals.COLS; i++)
        {
            var cell = new Cell(0xe276, "white,", "black");
            Cells.Add(cell);
        }
    }
    public List<Cell> Cells { get; set; }
    public bool Visible { get; set; }

}

public class Cell
{
    public Cell(int characters, string foreground, string background)
    {
        Character = characters;
        Foreground = foreground;
        Background = background;
    }
    public int Character { get; set; }
    public string Foreground { get; set; }
    public string Background { get; set; }
}