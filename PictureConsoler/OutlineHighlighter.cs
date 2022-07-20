using System;
using System.Drawing;

namespace PictureConsoler
{
    static class OutlineHighlighter
    {
        public static bool Revesre = true;

        public static void Highlight(Bitmap bitmap, double[,] sobel, double compress_factor, double division_factor)
        {
            double shift = 1.0 - compress_factor * division_factor;
            for (int ix = 0; ix < bitmap.Width; ix++)
            {
                for (int iy = 0; iy < bitmap.Height; iy++)
                {
                    Color pixel = bitmap.GetPixel(ix, iy);
                    double sobelFactor = sobel[ix, iy] * compress_factor + shift;
                    bitmap.SetPixel(ix, iy, ProcessPixel(pixel, sobelFactor));
                }
            }
        }

        private static Color ProcessPixel(Color pixel, double sobel_factor)
        {
            if (Revesre) sobel_factor = 1 / sobel_factor;
            double newRedRaw = pixel.R * sobel_factor;
            double newGreenRaw = pixel.G * sobel_factor;
            double newBlueRaw = pixel.B * sobel_factor;
            byte newRed = (byte)Math.Min((short)newRedRaw, (short)255);
            byte newGreen = (byte)Math.Min((short)newGreenRaw, (short)255);
            byte newBlue = (byte)Math.Min((short)newBlueRaw, (short)255);
            return Color.FromArgb(newRed, newGreen, newBlue);
        }
    }
}
