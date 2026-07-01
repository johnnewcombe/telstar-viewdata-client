using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
/*
    Copyright (c) 2025 John Newcombe

    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with the product. If not, see <https://www.gnu.org/licenses/>.

*/

using TelstarClient.Display;

namespace TelstarClient.Models;

public class Char {

    public Char(char value, string foreground = Constants.White, string background = Constants.Black) {
        Value = value;
        Foreground = foreground;
        Background = background;
    }

    // NOTE, changing anything here MUST be reflected in the Extensions Class also.
    public char Value { get; set; }
    public bool InVisible { get; set; }
    public bool Invalid { get; set; }
    public bool Control { get; set; }
    public bool Concealed { get; set; }
    public bool Flash { get; set; }
    public bool Separated { get; set; }
    public bool Graphic { get; set; }
    public bool GraphicsHold { get; set; }
    public bool DoubleHeight { get; set; }
    public int Index { get; set; }
    public string Foreground { get; set; }
    public string Background { get; set; }

}