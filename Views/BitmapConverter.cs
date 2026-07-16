using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace TelstarClient.Views;

/// <summary>
/// Converts the raw RGBA8888 byte buffer produced by
/// ViewdataDisplay.DisplayManager.Bitmap into an Avalonia WriteableBitmap,
/// ready to display via an Image control's Source.
/// </summary>
internal static class BitmapConverter
{
    /// <summary>
    /// Converts a raw RGBA8888 buffer (row-major, top-to-bottom,
    /// left-to-right, 4 bytes per pixel - exactly what
    /// DisplayManager.Bitmap produces) into a WriteableBitmap.
    ///
    /// Uses Marshal.Copy rather than unsafe pointer arithmetic - no
    /// AllowUnsafeBlocks project setting required. Given this runs once
    /// per frame regenerate (not once per pixel), the performance
    /// difference against the unsafe version should be negligible.
    /// </summary>
    /// <param name="rgba">The raw pixel buffer.</param>
    /// <param name="width">Bitmap width in pixels (480 at the standard Viewdata grid).</param>
    /// <param name="height">Bitmap height in pixels (500 at the standard Viewdata grid).</param>
    internal static WriteableBitmap ToWriteableBitmap(byte[] rgba, int width, int height)
    {
        // NOTE: there's a documented (old - 2020, Avalonia 0.9.11/0.10.0-preview1)
        // Linux-specific bug where Rgba8888 renders as if it were Bgra8888
        // (red/blue channels swapped), while Windows renders it correctly.
        // Almost certainly long fixed given the age, but worth knowing given
        // you're on Fedora: if the rendered image comes out with red and
        // blue swapped, try PixelFormat.Bgra8888 here instead (and swap the
        // R/B byte order when writing pixels in BitmapGenerator.RenderCell).
        var bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96), // standard DPI - this is a pixel-art bitmap, DPI is nominal
            PixelFormat.Rgba8888,
            AlphaFormat.Opaque); // every pixel in DisplayManager.Bitmap has A=255 (fully opaque) - if you see odd
                                  // rendering, AlphaFormat.Unpremul is the safe fallback (community reports of
                                  // Opaque behaving oddly in some cases), though it should be visually identical here

        using ILockedFramebuffer frameBuffer = bitmap.Lock();

        int sourceStride = width * 4;

        // frameBuffer.RowBytes may not exactly equal width*4 (stride
        // padding on some platform/format combinations) - copy row by
        // row via Marshal.Copy rather than assuming the buffer is fully
        // contiguous, so this stays correct either way.
        if (frameBuffer.RowBytes == sourceStride)
        {
            // Common case: no padding, the whole buffer is contiguous -
            // one single copy rather than looping per row.
            System.Runtime.InteropServices.Marshal.Copy(rgba, 0, frameBuffer.Address, rgba.Length);
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                int sourceOffset = y * sourceStride;
                IntPtr destinationRow = frameBuffer.Address + (y * frameBuffer.RowBytes);
                System.Runtime.InteropServices.Marshal.Copy(rgba, sourceOffset, destinationRow, sourceStride);
            }
        }

        return bitmap;
    }
}
