using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Fonts;
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

        //this.ExtendClientAreaToDecorationsHint = true;

        var thicknessZero = Thickness.Parse("0");
        //display.Margin = Thickness.Parse("10");
        //display.FirstColumn = 0;


        // Loop through attaching font characters from the
        // display data array i.e. (960 bytes of virtual screen memory)
        // Last row is blank.
        for (var i = 0; i < (24 * 40); i++)
        {
            var g = GetSixelGraphic(0);
            
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

    private static Label GetSixelGraphic(int charNumber)
    {
        var thicknessZero = Thickness.Parse("0");

        /* Mode 7 font
         * Graphics start at e201
         * Non-contiguous graphics start at e2c1
         * Upper part of Alpha double height e021
         * Lower part of Alpha double height e121
         *
         */
        
        // create a sixel
        var label = new Label()
        {
            FontSize = 22,
            Background = Brushes.Black,
            Foreground = Brushes.White,
            Content = "\xe276",
            Padding = thicknessZero,
            Margin = thicknessZero,
            FontStretch = FontStretch.UltraExpanded
            
        };

        // set the style i.e. Mode 7 font.
        label.Classes.Add("mode7");

        return label;
    }
}