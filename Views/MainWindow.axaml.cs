using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Net.Http.Headers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using TelstarClient.Display;
using TelstarClient.Models;
using TelstarClient.ViewModels;
using Brushes = Avalonia.Media.Brushes;

namespace TelstarClient.Views;

public partial class MainWindow : Window {
    public MainWindowViewModel ViewModel { get; set; }

    public MainWindow() {
        InitializeComponent();

        ViewModel = new MainWindowViewModel();
        ViewModel.PropertyChanged += this.PropertyChangedEventHandler;

        // remove title bar etc.
        //this.ExtendClientAreaToDecorationsHint = true;

        //initialise the display
        display.Children.Clear();
        for (int i = 0; i < Models.Display.COLS * Models.Display.ROWS; i++) {
            var g = InitCharacterLabel(0x20);
            display.Children.Add(g);
        }
    }

    private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
            case nameof(ViewModel.DisplayManagerData):
                try {
                    // execute on the main thread
                    Dispatcher.UIThread.Post(UpdateDisplay);
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
                }

                break;
            case nameof(ViewModel.Status):
                Dispatcher.UIThread.Post(UpdateStatus);
                break;
        }
    }

    private void ConnectButton_OnClick(object? sender, RoutedEventArgs e) {
        ViewModel.Connect();
    }

    private void DisconnectButton_OnClick(object? sender, RoutedEventArgs e) {
        ViewModel.Disconnect();
    }

    private void Keypad_OnClick(object? sender, RoutedEventArgs e) {
        var button = (Button)sender;
        ViewModel.Send((string)button.Tag);
    }

    private void RevealButton_OnClick(object? sender, RoutedEventArgs e) {
    }

    private void ConcealButton_OnClick(object? sender, RoutedEventArgs e) {
    }

    // TODO: Use this to display stored pages from the view model
    private void UpdateDisplay() {
        var chars = ViewModel.DisplayManagerData;

        if (chars is null) {
            return;
        }

        if (chars.Count > 1) {
            Debug.Print($"Chars to Update: {chars.Count}");
        }

        foreach (var c in chars) {
            
            if (c is null) continue;
            var label = (Label)((Viewbox)display.Children[c.Index]).Child;
            label.Content = c.IsControl ? "\xe200" : $"{c.Value}";
            label.Foreground = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Foreground);
            label.Background = (IImmutableSolidColorBrush)new BrushConverter().ConvertFromString(c.Background);
        }
    }

    private void UpdateStatus() {
        status.Content=ViewModel.Status;
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