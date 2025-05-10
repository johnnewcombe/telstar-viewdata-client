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
using System.Text;
using Avalonia.Input;

namespace TelstarClient.ViewModels;

public class KeyMapper {
    
    private Dictionary<char,char> _map = new Dictionary<char,char>();

    public KeyMapper() {
        //load dictionary
        _map.Add('\x0d','\x5f');
    }
    
    public string Map(string keySymbol) {

        var result = new StringBuilder();

        foreach (var key in keySymbol) {
            char value;
            if (_map.TryGetValue(key,out value)) {
                // key exists so add the mapped value to the result
                result.Append(value);
            }
            else {
                // key not in dictionary so no mapping exists
                // therefore return presented character unaltered
                result.Append(key);
            }
        }
        
        return result.ToString();
    }
}