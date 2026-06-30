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
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelstarClient.ViewModels;
using Brushes = Avalonia.Media.Brushes;

namespace TelstarClient.Views;

public partial class MainWindow : Window
{
    private ILogger<MainWindow> logger =
        App.Host.Services.GetRequiredService<ILogger<MainWindow>>();

    public MainWindowViewModel ViewModel { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainWindowViewModel();
        ViewModel.PropertyChanged += this.PropertyChangedEventHandler;

        // TODO implement this somehow
        // remove title bar and chrome etc. e.g. if Kiosk mode
        //ExtendClientAreaToDecorationsHint = true;
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints. NoChrome;
        //WindowState = WindowState.FullScreen;
        //this.Topmost = true;

        //initialise the display
        display.Children.Clear();

        // note that we create an extra row of labels for the status line
        for (int i = 0; i < Models.Display.COLS * (Models.Display.ROWS + 1); i++)
        {
            var g = InitCharacterLabel(Models.Display.SPC);
            display.Children.Add(g);
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
                    logger.LogError(ex, "Failed to update the display");
                }

                break;
        }
    }

    private void Window_FullScreen(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }


    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        logger.LogDebug("Keypress:{Key}, Physical Key:{PhysicalKey}, Modifiers:{Modifiers}", e.Key,
            e.PhysicalKey, e.KeyModifiers);
        try
        {
            ViewModel.KeyHandler(e);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,"{Message}", ex.Message);
        }
    }

    private void Window_TextInput(object sender, TextInputEventArgs e)
    {
        logger.LogDebug("TextInput:{Text}",e.Text);
        try
        {
            ViewModel.TextHandler(e);
        }

        catch (Exception ex)
        {
            logger.LogError(ex,"{Message}", ex.Message);
            throw;
        }
    }

    private void ConnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        //ViewModel.Connect();
    }

    private void DisconnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        //ViewModel.Disconnect();
    }

    private void RevealButton_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    private void ConcealButton_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    private void UpdateCursor()
    {
        var cursor = ViewModel.Cursor;
        if (cursor is not null &&
            cursor.Visible) // TODO change to use .Visible property once cursor positioning is working
        {
            var label = (Label)((Viewbox)display.Children[cursor.GetCursorIndex()]).Child;
            label.Content = "_";
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
            var label = (Label)((Viewbox)display.Children[c.Index]).Child;

            if (!c.InVisible)
            {
                label.Content = c.Value;
                label.Foreground = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Foreground);
                label.Background = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Background);
            }
            else
            {
                label.Content = "";
                label.Background = (IImmutableSolidColorBrush)new ImmutableSolidColorBrush(Colors.Black);
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

    private void InputElement_OnTextInput(object sender, TextInputEventArgs e)
    {
        throw new NotImplementedException();
    }
}