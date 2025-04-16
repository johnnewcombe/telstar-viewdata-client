using System.Collections.Generic;

namespace TelstarClient.Display;

/// <summary>
/// Maps viewdata characters to font characters.
/// </summary>
public class FontMapper {
    
    private Dictionary<char, char> _map = new Dictionary<char, char>();

    public FontMapper() {
        //load dictionary
        _map.Add('\x20', '\xe200');
        _map.Add('\x5f', '\x23');
    }

    /// <summary>
    /// Maps viewdata characters to font characters.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public char Map(char key) {

        char value;

        if (_map.TryGetValue(key, out value)) {
            // key exists so add the mapped value to the result
            return value;
        }

        // key not in dictionary so no mapping exists
        // therefore return presented character unaltered
        return key;

    }
}