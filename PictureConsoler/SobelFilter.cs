using System;
using System.Drawing;

namespace PictureConsoler
{
    static class SobelFilter
    {
        private const byte grayscaleGradientPhaseCount = 7;
        private static double[,] SX = new double[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        public static bool GrayscaleAsHue;
        public static double SobelCap = 1.0;

        private static Color[] grayscaleGradient;
        
        static SobelFilter()
        {
            FillGrayscaleGradient();
        }
        private static void FillGrayscaleGradient()
        {
            grayscaleGradient = new Color[255 * grayscaleGradientPhaseCount + 1];
            for (byte phase = 0; phase < grayscaleGradientPhaseCount; phase++)
            {
                for (byte value = 0; value < 255; value++)
                {
                    byte red = 0, green = 0, blue = 0;
                    switch (phase)
                    {
                        case 0:// black -> red
                            red = value;
                            green = blue = 0;
                            break;
                        case 1:// red -> yellow
                            red = 255;
                            green = value;
                            blue = 0;
                            break;
                        case 2:// yellow -> green
                            red = (byte)~value;
                            green = 255;
                            blue = 0;
                            break;
                        case 3:// green -> cyan
                            red = 0;
                            green = 255;
                            blue = value;
                            break;
                        case 4:// cyan -> blue
                            red = 0;
                            green = (byte)~value;
                            blue = 255;
                            break;
                        case 5:// blue -> magenta
                            red = value;
                            green = 0;
                            blue = 255;
                            break;
                        case 6:// magenta -> white
                            red = blue = 255;
                            green = value;
                            break;
                    }
                    short hue = (short)(255 * phase + value);
                    grayscaleGradient[hue] = Color.FromArgb(red, green, blue);
                }
            }
            grayscaleGradient[255 * grayscaleGradientPhaseCount] = Color.FromArgb(255, 255, 255);
        }
        private static Color GetColorFromGrayscale(double grayscale)
        {
            if (GrayscaleAsHue)
            {
                short hue = (short)(grayscale * 255 * grayscaleGradientPhaseCount);
                return grayscaleGradient[hue];
            }
            int value = (int)(grayscale * 255);
            return Color.FromArgb(value, value, value);
        }

        public static Bitmap Filter(Bitmap original)
        {
            double[,] grayscale = BitmapToGrayscale(original);
            double[,] sobel = Filter(grayscale, SX);
            return GrayscaleToBitmap(sobel);
        }
        public static double[,] GetGrayscaleSobel(Bitmap original)
        {
            double[,] grayscale = BitmapToGrayscale(original);
            return Filter(grayscale, SX);
        }

        private static double[,] BitmapToGrayscale(Bitmap original)
        {
            double[,] grayscaled = new double[original.Width, original.Height];
            for (int ix = 0; ix < original.Width; ix++)
            {
                for (int iy = 0; iy < original.Height; iy++)
                {
                    Color pixel = original.GetPixel(ix, iy);
                    grayscaled[ix, iy] = (pixel.R + pixel.G + pixel.B) / (255.0 * 3);
                }
            }
            return grayscaled;
        }
        private static Bitmap GrayscaleToBitmap(double[,] g)
        {
            Bitmap bmp = new Bitmap(g.GetLength(0), g.GetLength(1));
            for (int ix = 0; ix < bmp.Width; ix++)
                for (int iy = 0; iy < bmp.Height; iy++)
                    bmp.SetPixel(ix, iy, GetColorFromGrayscale(g[ix, iy]));
            return bmp;
        }

        private static double[,] Filter(double[,] g, double[,] sx)
        {
            int width = g.GetLength(0), height = g.GetLength(1);
            int ssh = sx.GetLength(0) / 2;
            double[,] result = new double[width, height];
            double gx, gy;
            for (int x = ssh; x < width - ssh; x++)
            {
                for (int y = ssh; y < height - ssh; y++)
                {
                    gx = gy = 0.0;
                    for (int ix = -ssh; ix <= ssh; ix++)
                    {
                        for (int iy = -ssh; iy <= ssh; iy++)
                        {
                            gx += g[x + ix, y + iy] * sx[ix + ssh, iy + ssh];
                            gy += g[x + ix, y + iy] * sx[iy + ssh, ix + ssh];
                        }
                    }
                    double rawSobel = Math.Sqrt(gx * gx + gy * gy);
                    result[x, y] = Math.Min(rawSobel / SobelCap, 1.0);
                }
            }
            return result;
        }
    }
}
