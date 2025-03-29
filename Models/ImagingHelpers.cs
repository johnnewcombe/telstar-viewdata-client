using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
//using System.Drawing;

using Avalonia.Platform;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace TelstarClient.Models
{
    public static class ImageHelper
    {

    }
}




//The following code example demonstrates the use of the TextRenderingHint
// and TextContrast properties and the TextRenderingHint enumeration.
// 
// This example is designed to be used with Windows Forms. Paste the
// code into a form and call the ChangeTextRenderingHintAndTextContrast
// method when handling the form's Paint event, passing e as PaintEventArgs.
/*
    private void ChangeTextRenderingHintAndTextContrast(PaintEventArgs e){        

        // Retrieve the graphics object.
        Graphics formGraphics = e.Graphics;
        
        // Declare a new font.
        Font myFont = new Font(FontFamily.GenericSansSerif, 20, 
            FontStyle.Regular);

        // Set the TextRenderingHint property.
        formGraphics.TextRenderingHint = 
            System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
        
        // Draw the string.
        formGraphics.DrawString("H", myFont, 
            Brushes.Firebrick, 20.0F, 20.0F);
        
        // Change the TextRenderingHint property.
        formGraphics.TextRenderingHint = 
            System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        
        // Draw the string again.
        formGraphics.DrawString("Hello World", myFont, 
            Brushes.Firebrick, 20.0F, 60.0F);

        // Set the text contrast to a high-contrast setting.
        formGraphics.TextContrast = 0;

        // Draw the string.
        formGraphics.DrawString("Hello World", myFont, 
            Brushes.DodgerBlue, 20.0F, 100.0F);

        // Set the text contrast to a low-contrast setting.
        formGraphics.TextContrast = 12;

        // Draw the string again.
        formGraphics.DrawString("Hello World", myFont, 
            Brushes.DodgerBlue, 20.0F, 140.0F);

        // Dispose of the font object.
        myFont.Dispose();
    }
    */
