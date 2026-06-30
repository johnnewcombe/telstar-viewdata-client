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

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Microsoft.Extensions.Logging;
using TelstarClient.Extensions;
using TelstarClient.Forms;

namespace TelstarClient.ViewModels;

public partial class MainWindowViewModel
{
    /// <summary>
    /// Handles keyboard activity passed from the View.
    /// </summary>
    /// <param name="asciiValue"></param>
    public void ProcessKey(byte asciiValue)
    {
        // NOTE That this function does not run on the UI thread
        logger.LogDebug("ASCII Value:{Hex:X2}h,{Decimal}d", asciiValue, asciiValue);
        Configuration.Connection con = null;

        // The screen can be in any one of 'DisplayType' states. Also,
        // the client could be online or offline when in any of these states.

        switch (_displayType)
        {
            case DisplayType.Terminal:

                // looking for alt key combinations (same as ctrl codes but with high bit set)
                switch (asciiValue)
                {
                    case 0x98: // ' alt+x disconnect
                        Disconnect();
                        SetDisplay(DisplayType.Directory);
                        break;
                    case 0x88: // alt+h show help menus
                        SetDisplay(DisplayType.Help);
                        break;
                    case 0x83: // conceal
                        _displayManagerMain.Display.ToggleConceal();
                        break;
                }

                // send to remote end
                if (!_tcp.Write(asciiValue))
                {
                    logger.LogError("Failed to send character to server:{Hex:X2}h,{Decimal}d", asciiValue, asciiValue);
                }
                else
                {
                    logger.LogInformation("Character sent to server:{Hex:X2}h,{Decimal}d", asciiValue, asciiValue);
                }

                break;

            case DisplayType.Welcome:

                // if we get a key press of any kind whilst looking at the welcome page
                // then load the menu
                SetDisplay(DisplayType.Directory);
                break;

            case DisplayType.Directory:

                int index;
                
                switch (asciiValue)
                {
                    case 0x98: // alt+x
                        Disconnect();
                        SetDisplay(DisplayType.Directory);
                        break;

                    // validate number Connect)
                    case >= 0x30 and <= 0x39:

                        // convert to index
                        index = asciiValue - 0x30;

                        // manual dialling?
                        if (index == 0)
                        {
                            SetDisplay(DisplayType.Connect);
                        }
                        else if (index - 1 < _settings.Config.Connections.Count)
                        {
                            // user has selected a connection
                            // index-1 as menu is '1' based and collection is '0' based
                            con = _settings.Config.Connections[index - 1];

                            if (!string.IsNullOrEmpty(con.Name))
                            {
                                Connect(con.Host, con.Port);
                                SetDisplay(DisplayType.Terminal);
                            }
                        }

                        break;

                    // validate alt+number (Edit)
                    case >= 0xB0 and <= 0xB9:
                        index = asciiValue - 0xB0;

                        if (index - 1 < _settings.Config.Connections.Count)
                        {
                            // user has selected a connection
                            // index-1 as menu is '1' based and collection is '0' based
                            con = _settings.Config.Connections[index - 1];
                            SetDisplay(DisplayType.EditConnection, con);
                        }

                        break;

                    case 0x88: // ' alt+x disconnect
                        SetDisplay(DisplayType.Help);
                        break;
                }

                break;
            
            case DisplayType.Connect:
                
                // connect or ignore
                if (!_currentForm.ProcessFormKey(asciiValue))
                {
                    if (_currentForm is not null)
                    {
                        if (_currentForm.IsValid())
                        {
                            // get the values
                            Forms.Field field;
                            var port = 0;
                            var host = string.Empty;

                            field = _currentForm.GetFieldById("host");
                            if (field is not null)
                                host = field.Value;
                            field = _currentForm.GetFieldById("port");
                            if (field is not null)
                                int.TryParse(field.Value, out port);

                            Connect(host, port);
                            SetDisplay(DisplayType.Terminal);
                        }
                    }
                }

                break;

            case DisplayType.EditConnection:

                if (!_currentForm.ProcessFormKey(asciiValue))
                {
                    // save connection
                    if (_currentForm is not null)
                    {
                        // get the values
                        Forms.Field field;
                        var port = 0;
                        var name = string.Empty;
                        var host = string.Empty;

                        field = _currentForm.GetFieldById("dirName");
                        if (field is not null)
                            name = field.Value;
                        //field = _currentForm.GetFieldById("dirEntry");
                        //if (field is not null)
                        //    int.TryParse(field.Value, out memory);
                        field = _currentForm.GetFieldById("host");
                        if (field is not null)
                            host = field.Value;
                        field = _currentForm.GetFieldById("port");
                        if (field is not null)
                            int.TryParse(field.Value, out port);

                        // if the form is valid
                        if (_currentForm.IsValid() && _currentForm.Connection is not null)
                        {
                            // save
                            _currentForm.Connection.Name = name;
                            _currentForm.Connection.Host = host;
                            _currentForm.Connection.Port = port;
                            _settings.Save();
                        }
                        else
                        {
                            // TODO work out a way to display errors
                            // error, not saved
                            logger.LogError("Connection invalid and not saved:{Name}, {IP}, {Port}", 
                                _currentForm.Connection.Name,
                                _currentForm.Connection.Host,
                                _currentForm.Connection.Port);
                        }
                    }

                    //DisplayEditor returns false when complete or canceled
                    SetDisplay(_previousDisplayType);
                }
                else
                {
                    // updte the display as something was processed by _currentForm.ProcessFormKey
                    DisplayData = _displayManagerAlt.Display.Chars;
                }


                break;

            case DisplayType.Help:

                switch (asciiValue)
                {
                    case 0x98: // alt+x
                        Disconnect();
                        SetDisplay(DisplayType.Directory);
                        break;
                    case 0x90: // alt+q
                        Disconnect();
                        Shutdown();
                        break;
                    default:
                        break;
                }

                SetDisplay(_previousDisplayType);

                break;
        }
    }

    public void TextHandler(TextInputEventArgs args)
    {
        // e.Text is already fully composed by the OS — Shift+8 gives "*", etc.
        if (args.Text is { Length: > 0 })
        {
            char c = args.Text[0];
            if (c >= 0 && c <= 127)
            {
                ProcessKey((byte)c);
                args.Handled = true;
            }
        }
    }

    public void KeyHandler(KeyEventArgs args)
    {
        // Only handle keys that OnTextInput will NEVER fire for
        byte? ascii = GetAsciiKey(args);
        if (ascii is not null)
        {
            ProcessKey(ascii.Value);
            args.Handled = true;
        }
    }


    private static byte? GetAsciiKey(KeyEventArgs e)
    {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        return e.Key switch
        {
            // Navigation / editing keys
            Key.Enter or Key.Return => 13,
            Key.Escape => 27,
            Key.Back => 8,
            Key.Tab when shift => 0x89, // TAB with msb set.
            Key.Tab => 9,
            Key.Delete => 127,

            // Arrow keys
            Key.Left => 0x08,
            Key.Right => 0x09,
            Key.Down => 0x0A,
            Key.Up => 0x0B,

            // platform and keyboard agnostic (hopefully!)
            Key.A when ctrl => 1,
            Key.B when ctrl => 2,
            Key.C when ctrl => 3,
            Key.D when ctrl => 4,
            Key.E when ctrl => 5,
            Key.F when ctrl => 6,
            Key.G when ctrl => 7,
            Key.H when ctrl => 8,
            Key.J when ctrl => 9,
            Key.K when ctrl => 10,
            Key.L when ctrl => 11,
            Key.M when ctrl => 12,
            Key.N when ctrl => 13,
            Key.O when ctrl => 14,
            Key.P when ctrl => 15,
            Key.Q when ctrl => 16,
            Key.R when ctrl => 17,
            Key.S when ctrl => 18,
            Key.T when ctrl => 19,
            Key.U when ctrl => 20,
            Key.V when ctrl => 21,
            Key.W when ctrl => 22,
            Key.X when ctrl => 24,
            Key.Y when ctrl => 25,
            Key.Z when ctrl => 26,

            // Alt+letter combinations used within the app
            // same as normal codes but with high bit set
            Key.C when alt => 0x83, // conceal
            Key.H when alt => 0x88, // help/menus
            Key.Q when alt => 0x90, // quit
            Key.R when alt => 0x92, // reveal
            Key.X when alt => 0x98, // escape/exit
            Key.D1 when alt => 0xb1, // Alt+1...
            Key.D2 when alt => 0xb2, // ... etc
            Key.D3 when alt => 0xb3,
            Key.D4 when alt => 0xb4,
            Key.D5 when alt => 0xb5,
            Key.D6 when alt => 0xb6,
            Key.D7 when alt => 0xb7,
            Key.D8 when alt => 0xb8,
            Key.D9 when alt => 0xb9,

            // Pure modifier keys — nothing to send
            Key.LeftShift or Key.RightShift or
                Key.LeftCtrl or Key.RightCtrl or
                Key.LeftAlt or Key.RightAlt or
                Key.Capital or Key.NumLock or
                Key.Scroll => null,

            _ => null
        };
    }

    private void Shutdown()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
        else
        {
            Environment.Exit(0);
        }
    }
}

// TODO Create an external Keymap file that is read in on startup
/* Keyboard Key Table

None 0 Represents no key.
Backquote 1 `~ on a US keyboard. This is the 半角/全角/漢字 (hankaku/zenkaku/kanji) key on Japanese keyboards.
Backslash 2 Used for both the US | (on the 101-key layout) and also for the key located between the " and Enter keys on row C of the 102-, 104- and 106-key layouts. #~ on a UK (102) keyboard.
BracketLeft 3 [{ on a US keyboard.
BracketRight 4 ]} on a US keyboard.
Comma 5 ,< on a US keyboard.
Digit0 6 0) on a US keyboard.
Digit1 7 1! on a US keyboard.
Digit2 8 2@ on a US keyboard.
Digit3 9 3# on a US keyboard.
Digit4 10 4$ on a US keyboard.
Digit5 11 5% on a US keyboard.
Digit6 12 6^ on a US keyboard.
Digit7 13 7& on a US keyboard.
Digit8 14 8* on a US keyboard.
Digit9 15 9( on a US keyboard.
Equal 16 =+ on a US keyboard.
IntlBackslash 17 Located between the left Shift and Z keys. | on a UK keyboard.
IntlRo 18 Located between the / and right Shift keys. \ろ (ro) on a Japanese keyboard.
IntlYen 19 Located between the = and Backspace keys. ¥ (yen) on a Japanese keyboard. / on a Russian keyboard.
A 20 a on a US keyboard. q on an AZERTY (e.g., French) keyboard.
B 21 b on a US keyboard.
C 22 c on a US keyboard.
D 23 d on a US keyboard.
E 24 e on a US keyboard.
F 25 f on a US keyboard.
G 26 g on a US keyboard.
H 27 h on a US keyboard.
I 28 i on a US keyboard.
J 29 j on a US keyboard.
K 30 k on a US keyboard.
L 31 l on a US keyboard.
M 32 m on a US keyboard.
N 33 n on a US keyboard.
O 34 o on a US keyboard.
P 35 p on a US keyboard.
Q 36 q on a US keyboard. a on an AZERTY (e.g., French) keyboard.
R 37 r on a US keyboard.
S 38 s on a US keyboard.
T 39 t on a US keyboard.
U 40 u on a US keyboard.
V 41 v on a US keyboard.
W 42 w on a US keyboard. z on an AZERTY (e.g., French) keyboard.
X 43 x on a US keyboard.
Y 44 y on a US keyboard. z on a QWERTZ (e.g., German) keyboard.
Z 45 z on a US keyboard. w on an AZERTY (e.g., French) keyboard. y on a QWERTZ (e.g., German) keyboard.
Minus 46 -_ on a US keyboard.
Period 47 .> on a US keyboard.
Quote 48 '" on a US keyboard.
Semicolon 49 ;: on a US keyboard.
Slash 50 /? on a US keyboard.
AltLeft 51 Alt, Option or ⌥.
AltRight 52 Alt, Option or ⌥. This is labelled AltGr key on many keyboard layouts.
Backspace 53 Backspace or ⌫. Labelled Delete on Apple keyboards.
CapsLock 54 CapsLock or ⇪.
ContextMenu 55 The application context menu key, which is typically found between the right Meta key and the right Control key.
ControlLeft 56 Control or ⌃.
ControlRight 57 Control or ⌃.
Enter 58 Enter or ↵. Labelled Return on Apple keyboards.
MetaLeft 59 The ⊞ (Windows), ⌘, Command or other OS symbol key.
MetaRight 60 The ⊞ (Windows), ⌘, Command or other OS symbol key.
ShiftLeft 61 Shift or ⇧.
ShiftRight 62 Shift or ⇧.
Space 63 (space).
Tab 64 Tab or ⇥.
Convert 65 Japanese: 変換 (henkan).
KanaMode 66 Japanese: カタカナ/ひらがな/ローマ字 (katakana/hiragana/romaji).
Lang1 67 Korean: HangulMode 한/영 (han/yeong). Japanese (Mac keyboard): かな (kana).
Lang2 68 Korean: Hanja 한자 (hanja). Japanese (Mac keyboard): 英数 (eisu).
Lang3 69 Japanese (word-processing keyboard): Katakana.
Lang4 70 Japanese (word-processing keyboard): Hiragana.
Lang5 71 Japanese (word-processing keyboard): Zenkaku/Hankaku.
NonConvert 72 Japanese: 無変換 (muhenkan).
Delete 73 ⌦. The forward delete key. Note that on Apple keyboards, the key labelled Delete on the main part of the keyboard is Backspace.
End 74 End or ↘.
Help 75 Help. Not present on standard PC keyboards.
Home 76 Home or ↖.
Insert 77 Insert or Ins. Not present on Apple keyboards.
PageDown 78 Page Down, PgDn or ⇟.
PageUp 79 Page Up, PgUp or ⇞.
ArrowDown 80 ↓.
ArrowLeft 81 ←.
ArrowRight 82 →.
ArrowUp 83 ↑.
NumLock 84 Numeric keypad Num Lock. On the Mac, this is used for the numpad Clear key.
NumPad0 85 Numeric keypad 0 Ins on a keyboard. 0 on a phone or remote control.
NumPad1 86 Numeric keypad 1 End on a keyboard. 1 or 1 QZ on a phone or remote control.
NumPad2 87 Numeric keypad 2 ↓ on a keyboard. 2 ABC on a phone or remote control.
NumPad3 88 Numeric keypad 3 PgDn on a keyboard. 3 DEF on a phone or remote control.
NumPad4 89 Numeric keypad 4 ← on a keyboard. 4 GHI on a phone or remote control.
NumPad5 90 Numeric keypad 5 on a keyboard. 5 JKL on a phone or remote control.
NumPad6 91 Numeric keypad 6 → on a keyboard. 6 MNO on a phone or remote control.
NumPad7 92 Numeric keypad 7 Home on a keyboard. 7 PQRS or 7 PRS on a phone or remote control.
NumPad8 93 Numeric keypad 8 ↑ on a keyboard. 8 TUV on a phone or remote control.
NumPad9 94 Numeric keypad 9 PgUp on a keyboard. 9 WXYZ or 9 WXY on a phone or remote control.
NumPadAdd 95 Numeric keypad +.
NumPadClear 96 Numeric keypad C or AC (All Clear). Also for use with numpads that have a Clear key that is separate from the NumLock key. On the Mac, the numpad Clear key is NumLock.
NumPadComma 97 Numeric keypad , (thousands separator). For locales where the thousands separator is a "." (e.g., Brazil), this key may generate a ..
NumPadDecimal 98 Numeric keypad . Del. For locales where the decimal separator is "," (e.g., Brazil), this key may generate a,
NumPadDivide 99 Numeric keypad /.
NumPadEnter 100 Numeric keypad Enter.
NumPadEqual 101 Numeric keypad =.
NumPadMultiply 102 Numeric keypad * on a keyboard. For use with numpads that provide mathematical operations (+, -, * and /).
NumPadParenLeft 103 Numeric keypad (. Found on the Microsoft Natural Keyboard.
NumPadParenRight 104 Numeric keypad ). Found on the Microsoft Natural Keyboard.
NumPadSubtract 105 Numeric keypad -.
Escape 106 Esc or ⎋.
F1 107 F1.
F2 108 F2.
F3 109 F3.
F4 110 F4.
F5 111 F5.
F6 112 F6.
F7 113 F7.
F8 114 F8.
F9 115 F9.
F10 116 F10.
F11 117 F11.
F12 118 F12.
F13 119 F13.
F14 120 F14.
F15 121 F15.
F16 122 F16.
F17 123 F17.
F18 124 F18.
F19 125 F19.
F20 126 F20.
F21 127 F21.
F22 128 F22.
F23 129 F23.
F24 130 F24.
PrintScreen 131 PrtScr SysRq or Print Screen.
ScrollLock 132 Scroll Lock.
Pause 133 Pause Break.
BrowserBack 134 Browser Back. Some laptops place this key to the left of the ↑ key.
BrowserFavorites 135 Browser Favorites.
BrowserForward 136 Browser Forward. Some laptops place this key to the right of the ↑ key.
BrowserHome 137 Browser Home.
BrowserRefresh 138 Browser Refresh.
BrowserSearch 139 Browser Search.
BrowserStop 140 Browser Stop.
Eject 141 Eject or ⏏. This key is placed in the function section on some Apple keyboards.
LaunchApp1 142 App 1. Sometimes labelled My Computer on the keyboard.
LaunchApp2 143 App 2. Sometimes labelled Calculator on the keyboard.
LaunchMail 144 Mail.
MediaPlayPause 145 Media Play/Pause or ⏵⏸.
MediaSelect 146 Media Select.
MediaStop 147 Media Stop or ⏹.
MediaTrackNext 148 Media Next or ⏭.
MediaTrackPrevious 149 Media Previous or ⏮.
Power 150 Power.
Sleep 151 Sleep.
AudioVolumeDown 152 Volume Down.
AudioVolumeMute 153 Mute.
AudioVolumeUp 154 Volume Up.
WakeUp 155 Wake Up.
Again 156 Again. Legacy. Found on Sun’s USB keyboard.
Copy 157 Copy. Legacy. Found on Sun’s USB keyboard.
Cut 158 Cut. Legacy. Found on Sun’s USB keyboard.
Find 159 Find. Legacy. Found on Sun’s USB keyboard.
Open 160 Open. Legacy. Found on Sun’s USB keyboard.
Paste 161 Paste. Legacy. Found on Sun’s USB keyboard.
Props 162 Props. Legacy. Found on Sun’s USB keyboard.
Select 163 Select. Legacy. Found on Sun’s USB keyboard.
Undo 164 Undo. Legacy. Found on Sun’s USB keyboard.

*/