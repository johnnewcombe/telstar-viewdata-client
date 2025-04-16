using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelstarClient.Models;

public class Char {
    
    public Char(char value, string foreground, string background) {
        Value = value;
        Foreground = foreground;
        Background = background;
    }

    // NOTE, changing anything here MUST be reflected in the Extensions Class also.
    public bool IsControl  { get; set; }
    public bool IsConcealed{get;set;}
    public bool IsSeparated{get;set;}
    public bool IsGraphic{ get; set; }
    public bool IsGraphicsHold{ get; set; }
    public bool IsDoubleHeightTop{ get; set; }
    public bool IsDoubleHeightBottom{ get; set; }
    public char Value { get; set; }
    public int Index { get; set; }
    public string Foreground { get; set; }
    public string Background { get; set; }
}
