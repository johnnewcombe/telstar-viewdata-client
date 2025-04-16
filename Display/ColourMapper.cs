using System.Collections.Generic;

namespace TelstarClient.Display;

/// <summary>
/// Maps a viewdata foreground colour code to an Avalonia colour.
/// </summary>
public class ColourMapper {

    private Dictionary<char, string> _map = new Dictionary<char, string>();

    public ColourMapper() {
        //load dictionary
        _map.Add(Constants.AlphaRed, Constants.Red);
        _map.Add(Constants.AlphaGreen, Constants.Green);
        _map.Add(Constants.AlphaYellow, Constants.Yellow);
        _map.Add(Constants.AlphaBlue, Constants.Blue);
        _map.Add(Constants.AlphaMagenta, Constants.Magenta);
        _map.Add(Constants.AlphaCyan, Constants.Cyan);
        _map.Add(Constants.AlphaWhite, Constants.White);
        _map.Add(Constants.GraphicRed, Constants.Red);
        _map.Add(Constants.GraphicGreen, Constants.Green);
        _map.Add(Constants.GraphicYellow, Constants.Yellow);
        _map.Add(Constants.GraphicBlue, Constants.Blue);
        _map.Add(Constants.GraphicMagenta, Constants.Magenta);
        _map.Add(Constants.GraphicCyan, Constants.Cyan);
        _map.Add(Constants.GraphicWhite, Constants.White);

    }

    public string Map(char key) {
        return _map.GetValueOrDefault(key);
    }
}