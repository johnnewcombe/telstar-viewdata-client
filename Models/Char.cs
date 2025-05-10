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
    along with Foobar. If not, see <https://www.gnu.org/licenses/>.

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
    public bool IsInvalid { get; set; }
    public bool IsControl { get; set; }
    public bool IsConcealed { get; set; }
    public bool IsSeparated { get; set; }
    public bool IsGraphic { get; set; }
    public bool IsGraphicsHold { get; set; }
    public bool IsDoubleHeight { get; set; }
    public int Index { get; set; }
    public string Foreground { get; set; }
    public string Background { get; set; }

}