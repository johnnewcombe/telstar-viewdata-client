using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelstarClient.Models;

public class Row {
    
    public Row(bool readOnly) {
        Chars = new List<Char>();
        ReadOnly = readOnly;

        for (var i = 0; i < Display.COLS; i++) {
            var cell = new Char((char)0xe276, "white,", "black");
            Chars.Add(cell);
        }
    }

    public List<Char> Chars { get; set; }
    public bool ReadOnly { get; }
}
