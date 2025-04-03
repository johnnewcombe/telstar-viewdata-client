using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelstarClient.Models;

public class Display {
    public const int COLS = 40;
    public const int ROWS = 24;
    public List<Row> Rows { get; set; }
}


