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
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelstarClient.ViewModels;
using Brushes = Avalonia.Media.Brushes;

namespace TelstarClient.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger =
        App.Host.Services.GetRequiredService<ILogger<MainWindow>>();

    private MainWindowViewModel ViewModel { get; }

    private bool KioskMode { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainWindowViewModel();
        ViewModel.PropertyChanged += PropertyChangedEventHandler;

        //initialise the display
        Display.Children.Clear();

        // note that we create an extra row of labels for the status line
        for (int i = 0; i < ViewdataDisplay.Display.Cols * (ViewdataDisplay.Display.Rows + 1); i++)
        {
            var g = InitCharacterLabel(ViewdataDisplay.Display.Spc);
            Display.Children.Add(g);
        }
    }

    private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.DisplayData):
                try
                {
                    // execute on the main thread
                    UpdateDisplay();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update the display");
                }

                break;
        }
    }

    public void ToggleKioskMode()
    {
        if (KioskMode)
        {
            // turn kiosk mode off
            ExtendClientAreaToDecorationsHint = false;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
            WindowState = WindowState.Normal;
            Topmost = false;
        }
        else
        {
            // turn kiosk mode on
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            WindowState = WindowState.FullScreen;
            Topmost = true;
        }

        // toggle the flag
        KioskMode = !KioskMode;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        _logger.LogDebug("Keypress:{Key}, Physical Key:{PhysicalKey}, Modifiers:{Modifiers}", e.Key,
            e.PhysicalKey, e.KeyModifiers);
        try
        {
            ViewModel.KeyHandler(e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
        }
    }

    private void Window_TextInput(object sender, TextInputEventArgs e)
    {
        _logger.LogDebug("TextInput:{Text}", e.Text);
        try
        {
            ViewModel.TextHandler(e);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            throw;
        }
    }

    private void UpdateCursor()
    {
        var cursor = ViewModel.Cursor;
        if (cursor is not null &&
            cursor.Visible)
        {
            var label = (Label)((Viewbox)Display.Children[cursor.GetCursorIndex()]).Child;
            if (label != null) label.Content = "_";
        }
    }

    private void UpdateDisplay()
    {
        var data = ViewModel.DisplayData;

        if (data is null)
        {
            return;
        }

        foreach (var c in data)
        {
            var label = (Label)((Viewbox)Display.Children[c.Index]).Child;
            
            if (label == null) continue;
            
            
            
            
            /*
             * Avalonia's default Label template wraps its content in a ContentPresenter inside
             * a Border (the Border.Background is what you're setting). The Border sizes itself
             * based on the measured size of its content. Most text-rendering engines — Avalonia's
             * included — collapse or zero-out the measured advance width for a string that's
             * whitespace-only, because trailing/leading whitespace gets trimmed during text
             * shaping. So the Label ends up with an effective width (and sometimes height)
             * of zero, even though Background is correctly applied — there's just nothing left
             * to paint it on.
             
             * The pattern used below is that in order to set a character to be invisible the
             * empty graphic character is specified. Another approach would be to use any printable
             * character and set the forground colour to equl the background colour.
             */
            if (c.InVisible)
            {
                // see note above
                label.Content = '\xE200';
                label.Background =  new ImmutableSolidColorBrush(Colors.Black);
            }
            else if (c.InvisibleForeground)
            {
                // see note above
                label.Content = '\xE200'; // full graphic character
                label.Background = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Background);
            }
            else
            {
                // display the character
                label.Content = c.Value;
                label.Foreground = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Foreground);
                label.Background = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Background);
                
            }
        }

        // once rendered, add the cursor
        UpdateCursor();
    }

    private static Viewbox InitCharacterLabel(int charNumber)
    {
        var thicknessZero = Thickness.Parse("0");

        // create a sixel
        var label = new Label()
        {
            Background = Brushes.Black,
            Foreground = Brushes.White,
            Content = (char)charNumber,
            Padding = thicknessZero,
            Margin = thicknessZero,
        };

        // set the style i.e. Mode 7 font.
        label.Classes.Add("mode7");

        var viewBox = new Viewbox()
        {
            Child = label,
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        return viewBox;
    }

}