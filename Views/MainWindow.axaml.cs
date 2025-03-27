using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.ViewModels;
using Brushes = Avalonia.Media.Brushes;


namespace AvaloniaApplication1.Views;

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
    }

    private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
    {
        updateDisplay();
        switch (e.PropertyName)
        {
            case nameof(ViewModel.DisplayData):
                updateDisplay();
                break;
            case nameof(ViewModel.CharacterReceived):
                addCharacter();
                break;
        }
    }

    private void ConnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.Test();
    }

    private void DisconnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ClearDisplay();
    }

    private void RevealButton_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    private void ConcealButton_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    private void addCharacter()
    {
        var cursor = ViewModel.Cursor;
        
        // TODO: calculate screen position from cursor
        var pos = 0
        display.Children[pos] = GetCharacterLabel(ViewModel.CharacterReceived);
    }

    private void updateDisplay()
    {
        var data = ViewModel.DisplayData;

        // Loop through attaching font characters from the
        // display data array i.e. (960 bytes of virtual screen memory)
        // TODO this updates all characters, is this the best approach?
        // TODO Can we use Bindings?
        for (var i = 0; i < (Globals.ROWS * Globals.COLS); i++)
        {
            var g = GetCharacterLabel(data[i]);

            display.Children.Clear();
            display.Children.Add(g);
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