using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Brushes = Avalonia.Media.Brushes;


namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    
    const int ROWS = 24;
    const int COLS = 40;

    public MainWindow()
    {
        InitializeComponent();

        // remove title bar etc.
        //this.ExtendClientAreaToDecorationsHint = true;

        var thicknessZero = Thickness.Parse("0");
        
        // Loop through attaching font characters from the
        // display data array i.e. (960 bytes of virtual screen memory)
        // Last row is blank.
        for (var i = 0; i < (ROWS * COLS); i++)
        {
            
            /* Mode 7 font
             * Graphics start at e201
             * Non-contiguous graphics start at e2c1
             * Upper part of Alpha double height e021
             * Lower part of Alpha double height e121
             *
             */
            var g = GetCharacterLabel(0xe276);
            
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