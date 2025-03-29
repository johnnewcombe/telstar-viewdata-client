using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelstarClient.Models;

public class Display 
{
    public const int COLS = 40;
    public const int ROWS = 24;
    
    public Display()
    {
        Rows = new List<Row>();
        for (var i = 0; i < ROWS; i++)
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
        Cells = new List<Col>();
        Visible = visible;
            
        for (var i = 0; i < Display.COLS; i++)
        {
            var cell = new Col(0xe276, "white,", "black");
            Cells.Add(cell);
        }
    }
    public List<Col> Cells { get; set; }
    public bool Visible { get; set; }

}

public class Col
{
    public Col(int characters, string foreground, string background)
    {
        Character = characters;
        Foreground = foreground;
        Background = background;
    }
    public int Character { get; set; }
    public string Foreground { get; set; }
    public string Background { get; set; }
}