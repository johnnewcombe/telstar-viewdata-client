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

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace TelstarClient.Display;

public static class Constants {
    
    #region Primary Controls C0

    public const char NullChar = '\x00';
    public const char STX = '\x02';
    public const char ETX = '\x03';
    public const char ENQ = '\x03';
    public const char ACK = '\x03';
    public const char BS = '\x08';
    public const char HT = '\x09';
    public const char LF = '\x0a';
    public const char VT = '\x0b';
    public const char HomeClear = '\x0c';
    public const char CR = '\x0d';
    public const char SO = '\x0e';
    public const char SI = '\x0f';
    public const char CurOn = '\x11';
    public const char DC2 = '\x12';
    public const char DC3 = '\x13';
    public const char CurOff = '\x14';
    public const char Esc = '\x1b';
    public const char SS2 = '\x1c';
    public const char SS3 = '\x1d';
    public const char Home = '\x1e';

    #endregion

    # region Parallel Controls C1

    public const char AlphaRed = '\x41';
    public const char AlphaGreen = '\x42';
    public const char AlphaYellow = '\x43';
    public const char AlphaBlue = '\x44';
    public const char AlphaMagenta = '\x45';
    public const char AlphaCyan = '\x46';
    public const char AlphaWhite = '\x47';

    public const char Flash = '\x48';
    public const char Steady = '\x49';
    public const char NormalHeight = '\x4c';
    public const char DoubleHeight = '\x4d';

    public const char GraphicRed = '\x51';
    public const char GraphicGreen = '\x52';
    public const char GraphicYellow = '\x53';
    public const char GraphicBlue = '\x54';
    public const char GraphicMagenta = '\x55';
    public const char GraphicCyan = '\x56';
    public const char GraphicWhite = '\x57';

    public const char Conceal = '\x58';
    public const char Contiguous = '\x59';
    public const char Separated = '\x5a';
    public const char BlackBackground = '\x5c';
    public const char NewBackground = '\x5d';
    public const char HoldGraphics = '\x5e';
    public const char ReleaseGraphics = '\x5f';

    #endregion

    #region Avalonia Colours

    public const string Red = "Red";
    public const string Green = "Chartreuse";
    public const string Yellow = "Yellow";
    public const string Blue = "Blue";
    public const string Magenta = "Magenta";
    public const string Cyan = "Cyan";
    public const string White = "White";
    public const string Black = "Black";
    
    public const string DefaultForeground = "White";
    public const string DefaultBackground = "Black";
    
    #endregion

    public const string PlaceHolder = "[PLACEHOLDER]";
}