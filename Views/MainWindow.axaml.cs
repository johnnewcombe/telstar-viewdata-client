using System.Diagnostics;
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
    
    public MainWindow()
    {
        InitializeComponent();

        MainWindowViewModel viewModel = new MainWindowViewModel();

        // remove title bar etc.
        //this.ExtendClientAreaToDecorationsHint = true;

        var data = viewModel.DisplayData;
        
        // TODO: This is just an initialisation, we need to update this if a value changes
        // Loop through attaching font characters from the
        // display data array i.e. (960 bytes of virtual screen memory)
        for (var i = 0; i < (Globals.ROWS * Globals.COLS); i++)
        {
            var g = GetCharacterLabel(data[i]);
            // Add to Uniform grid 40 x 24
            display.Children.Add(g);
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Button Click!");
    }

    private void MenuFileOpen_OnClick(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Menu Click!");
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