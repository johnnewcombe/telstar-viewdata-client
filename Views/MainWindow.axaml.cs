using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Mime;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
        

        
        // Uniform grid
        // Loop through attaching font characters from the
        // display data array i.e. (960 bytes of virtual screen memory)
        for (var i = 0; i < 24 * 40; i++)
        {
            var panel = new Panel();
            var background = new Rectangle
            {
                Fill = Brushes.White
            };
            
            var foreground = new Image()
            {
                // just adding a single character for now
                //Source= new Bitmap("/Users/john/RiderProjects/telstar-client-avalonia/telstar-client-avalonia/Assets/Fonts/bitmap_40.png"),
                Source= new Bitmap("/Users/john/Library/CloudStorage/Dropbox/Repositories/telstar-client-avalonia/telstar-client-avalonia/Assets/MODE7GX3_EDITED/37_numbersign_copy.png"),

            };
            panel.Children.Add(background);
            panel.Children.Add(foreground);
            
            display.Children.Add(panel);
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
    

}

