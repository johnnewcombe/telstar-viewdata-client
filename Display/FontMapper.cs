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

using System.Collections.Generic;
using log4net.Config;

namespace TelstarClient.Display;

/// <summary>
/// Maps viewdata characters to font characters.
/// </summary>
public class FontMapper {
    
    private Dictionary<char, char> _map = new Dictionary<char, char>();
    
    public FontMapper() {

        //load dictionary
        // e.g. _map.Add(oldchar, replacement);

        _map.Add('\x5f', '\x23');
        // this is a special case with Avalonia in that a space value of 0x20
        // does not render the background so the blank graphic is used instead
        _map.Add('\x20', '\xe200');
        _map.Add('\x7e','\xf7');
        _map.Add('\x7f', '\xb6');
        _map.Add('\x7b', '\xbc');
        _map.Add('\x5c', '\xbd');
        _map.Add('\x7d', '\xbe');
        _map.Add('\x23', '\xa3');
        
    }

    /// <summary>
    /// Maps viewdata characters to font characters.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public char Map(char key) {
        return _map.TryGetValue(key, out var value) ? value : key;
    }
}