using System.Collections.Generic;

namespace TelstarClient.Display;

/// <summary>
/// Maps viewdata characters to font characters.
/// </summary>
public class FontMapper {
    
    private Dictionary<char, char> _map = new Dictionary<char, char>();

    public FontMapper() {

        //load dictionary

        _map.Add('\x5f', '\x23');
        // this is a special case with Avalonia in that a space value of 0x20
        // does not render the background so the blank graphic is used instead
        _map.Add('\x20', '\xe200');

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