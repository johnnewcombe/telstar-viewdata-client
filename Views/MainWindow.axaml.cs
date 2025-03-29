using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using TelstarClient.Models;
using TelstarClient.ViewModels;
using Brushes = Avalonia.Media.Brushes;

namespace TelstarClient.Views;

public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        ViewModel = new MainWindowViewModel();
        ViewModel.PropertyChanged += this.PropertyChangedEventHandler;

        // remove title bar etc.
        //this.ExtendClientAreaToDecorationsHint = true;

        //initialise the display
        display.Children.Clear();
        for (int i = 0; i < Display.COLS * Display.ROWS; i++)
        {
            var g = GetCharacterLabel(0x20);
            display.Children.Add(g);
        }
    }

    private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ViewModel.DisplayManagerData):
                try
                {
                    Dispatcher.UIThread.Post(updateDisplay);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                break;
        }
    }

    private void ConnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Connect();
    }

    private void DisconnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Disconnect();
    }

    private void Keypad_OnClick(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        ViewModel.Send((string)button.Tag);
    }


    private void RevealButton_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    private void ConcealButton_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    // TODO: Use this to display stored pages from the view model
    private void updateDisplay()
    {
        var data = ViewModel.DisplayManagerData;

        // Loop through attaching font characters from the
        // display data array i.e. (960 bytes of virtual screen memory)
        // TODO this updates all characters, is this the best approach?
        //   consider a tuple with the char and display index
        // TODO Can we use a custom binding?
        for (var i=0;i<Display.COLS*Display.ROWS;i++)
        {
            var cell = (Viewbox)display.Children[i];
            ((Label)cell.Child).Content = $"&{data[i]};";
        }
    }

    private static Viewbox GetCharacterLabel(int charNumber)
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