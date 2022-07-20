using System;
using System.Drawing;

namespace PictureConsoler
{
    static class DoubleExtension
    {
        public static string SmartToString(this double value)
        {
            string str = value.ToString();
            if (value - (int)value == 0.0) str += ".0";
            else str = str.Replace(',', '.');
            return str;
        }
    }

    static class ColorExtension
    {
        public static int CalcDistance2(this Color color, Color opp)
        {
            short rDelta = (short)(color.R - opp.R);
            short gDelta = (short)(color.G - opp.G);
            short bDelta = (short)(color.B - opp.B);
            return rDelta * rDelta + gDelta * gDelta + bDelta * bDelta;
        }

        public static double CalcDistance(this Color color, Color opp)
        {
            return Math.Sqrt(color.CalcDistance2(opp));
        }
    }
}