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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using TelstarClient.ViewModels;
using Brushes = Avalonia.Media.Brushes;

namespace TelstarClient.Views;

public partial class MainWindow : Window {

    public MainWindowViewModel ViewModel { get; set; }

    public MainWindow() {
        InitializeComponent();

        Trace.Listeners.Add(new ConsoleTraceListener());
        
        //ViewModel = new MainWindowViewModel();
        ViewModel = DataContext as MainWindowViewModel;
        ViewModel.PropertyChanged += this.PropertyChangedEventHandler;

        // remove title bar and chrome etc.
        // if Kiosk mode
        //this.ExtendClientAreaToDecorationsHint = true;
        //this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints. NoChrome;
        //this.WindowState = WindowState.FullScreen;
        //this.Topmost = true;
        
        //initialise the display
        display.Children.Clear();

        // note that we create an extra row of labels for the status line
        for (int i = 0; i < Models.Display.COLS * (Models.Display.ROWS + 1); i++) {
            var g = InitCharacterLabel(Models.Display.SPC);
            display.Children.Add(g);
        }
    }

    private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
            case nameof(ViewModel.DisplayData):
                try {
                    // execute on the main thread
                    UpdateDisplay();
                }
                catch (Exception ex) {
                    Trace.WriteLine(ex.Message);
                }

                break;
        }
    }

    private void Window_FullScreen(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {

    }

    private void Window_KeyDown(object sender, KeyEventArgs e) {
        if (e.Key == Key.LeftCtrl) {
        }

        try {
            ViewModel.KeyHandler(e);
        }
        catch (Exception ex) {
            Trace.WriteLine(ex.Message);
        }
    }

    private void ConnectButton_OnClick(object? sender, RoutedEventArgs e) {
        //ViewModel.Connect();
    }

    private void DisconnectButton_OnClick(object? sender, RoutedEventArgs e) {
        //ViewModel.Disconnect();
    }

    private void RevealButton_OnClick(object? sender, RoutedEventArgs e) {
    }

    private void ConcealButton_OnClick(object? sender, RoutedEventArgs e) {
    }

    private void UpdateDisplay() {

        var data = ViewModel.DisplayData;

        if (data is null) {
            return;
        }

        foreach (var c in data) {

            var label = (Label)((Viewbox)display.Children[c.Index]).Child;

            if (!c.InVisible) {
                label.Content = c.Value;
                label.Foreground = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Foreground);
                label.Background = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Background);
            }
            else {
                label.Content = "";
                label.Background = (IImmutableSolidColorBrush)new ImmutableSolidColorBrush(Colors.Black);
            }
        }
    }

    private static Viewbox InitCharacterLabel(int charNumber) {
        var thicknessZero = Thickness.Parse("0");

        // create a sixel
        var label = new Label() {
            Background = Brushes.Black,
            Foreground = Brushes.White,
            Content = (char)charNumber,
            Padding = thicknessZero,
            Margin = thicknessZero,
        };

        // set the style i.e. Mode 7 font.
        label.Classes.Add("mode7");

        var viewBox = new Viewbox() {
            Child = label,
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        return viewBox;
    }
}