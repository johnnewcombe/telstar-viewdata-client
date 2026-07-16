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

        // frameBuffer.RowBytes may not exactly equal width*4 (stride
        // padding) - copy row by row rather than assuming the buffer is
        // contiguous, so this stays correct even if Avalonia ever pads
        // rows on some platform/format combination.
        int sourceStride = width * 4;
        unsafe
        {
            byte* destination = (byte*)frameBuffer.Address;
            for (int y = 0; y < height; y++)
            {
                int sourceOffset = y * sourceStride;
                int destOffset = y * frameBuffer.RowBytes;
                Marshal_Copy(rgba, sourceOffset, destination + destOffset, sourceStride);
            }
        }

        return bitmap;
    }

    private static unsafe void Marshal_Copy(byte[] source, int sourceOffset, byte* destination, int length)
    {
        fixed (byte* sourcePtr = &source[sourceOffset])
        {
            Buffer.MemoryCopy(sourcePtr, destination, length, length);
        }
    }
}
