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