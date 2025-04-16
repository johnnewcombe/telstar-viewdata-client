using System.Collections.Generic;

namespace TelstarClient.Display;

public class ColourMapper {
    
    private Dictionary<char, char> _map = new Dictionary<char, char>();

    public ColourMapper() {
        //load dictionary
        _map.Add('\x20', '\xe200');
        _map.Add('\x5f', '\x23');
    }
}