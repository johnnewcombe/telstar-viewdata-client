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
