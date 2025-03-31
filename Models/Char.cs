using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TelstarClient.Models;

public class Char {
    public Char(char value, string foreground, string background) {
        Value = value;
        Foreground = foreground;
        Background = background;
    }

    public char Value { get; set; }
    public int Index { get; set; }
    public string Foreground { get; set; }
    public string Background { get; set; }
}