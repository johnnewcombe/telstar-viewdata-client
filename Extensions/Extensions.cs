using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TelstarClient.Models;

namespace TelstarClient.Extensions;

public static class Extensions {

    private const string DefaultForeground = "White";
    private const string DefaultBackground = "Black";

    public static void Clear(this Models.Display display) {

        foreach (var c in display.Chars) {
            c.Foreground = DefaultForeground;
            c.Background = DefaultBackground;
            c.Value = Models.Display.SPC;
            c.IsControl = false;
            c.IsConcealed = false;
            c.IsSeparated = false;
            c.IsGraphic = false;
            c.IsGraphicsHold = false;
        }
    }

    public static bool IsRowReadOnly(this Models.Display display, int row) {
        return false;
    }

    public static Char GetChar(this Models.Display display, int row, int col) {
        return display.Chars[(row * Models.Display.COLS) + col];
    }
}