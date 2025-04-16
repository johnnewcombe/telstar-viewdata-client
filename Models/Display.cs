using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelstarClient.Models;

public class Display {
    
    public const int COLS = 40;
    public const int ROWS = 24;
    public const char SPC = '\xe200';
    public int[] RowReferences = new int[25];
    public List<Char> Chars { get; set; }
}