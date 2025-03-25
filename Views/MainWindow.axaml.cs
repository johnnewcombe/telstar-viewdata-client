using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Tmds.DBus.Protocol;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Brushes = Avalonia.Media.Brushes;
using Color = Avalonia.Media.Color;
using Image = Avalonia.Controls.Image;
using Rectangle = Avalonia.Controls.Shapes.Rectangle;

namespace AvaloniaApplication1.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.ExtendClientAreaToDecorationsHint = true;

        var thicknessZero = Thickness.Parse("0");
        display.Margin = Thickness.Parse("10");
        display.FirstColumn = 0;


        // Uniform grid
        // Loop through attaching font characters from the
        // display data array i.e. (960 bytes of virtual screen memory)
        for (var i = 0; i < 24 * 40; i++)
        {
            //var panel = new Panel();
            //panel.Margin = thicknessZero;


            var background = new Rectangle
            {
                Margin = thicknessZero,
                Fill = Brushes.Yellow
            };

            var g = GetSixelGraphic(0);

            //var foreground = new Image()
            //{
            //  Margin = thicknessZero,
            //    Height = background.Height,
            //    Width = background.Width,

            //    // just adding a single character for now
            //    Source= new Bitmap("/Users/john/Library/CloudStorage/Dropbox/Repositories/telstar-client-avalonia/telstar-client-avalonia/Assets/bitmap_40.png"),
            //Source= new Bitmap("/Users/john/Library/CloudStorage/Dropbox/Repositories/telstar-client-avalonia/telstar-client-avalonia/Assets/untitled.png"),

            //panel.Children.Add(background);
            //panel.Children.Add(foreground);
            //var rune = new PathIcon
            //{
            //    Foreground = Brushes.Yellow,
            //    Background = Brushes.Blue,
            //    Margin = thicknessZero,
            //    Padding =  thicknessZero,
            //    FontStretch = FontStretch.UltraExpanded,
            //    BackgroundSizing = BackgroundSizing.OuterBorderEdge,
            //    BorderThickness = thicknessZero,
            //};

            display.Children.Add(g);
            var panel = display.Children[i];
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

    private static UniformGrid GetSixelGraphic(int charNumber)
    {
        var thicknessZero = Thickness.Parse("0");

        // create a sixel
        var sixel = new UniformGrid()
        {
            Columns = 2,
            Rows = 3,
            Background = Brushes.Blue // blue will highlight any width and height issues.
        };

        // what about iterating cols and rows... etc
        for (var i = 0; i < (2 * 3); i++)
        {
            // create a background pixel
            var pixel = new Rectangle
            {
                Margin = thicknessZero,
                Fill = Brushes.Black,
            };

            // create a foreground pixel where appropriate

            if (i == 0 || i == 1 || i == 4 || i == 5)
            {
                pixel.Fill = Brushes.Yellow;
            }

            sixel.Children.Add(pixel);
        }


        var cell = sixel.Children[0];

        var background = new Rectangle
        {
            Margin = thicknessZero,
            Fill = Brushes.Yellow
        };

        return sixel;
    }
}