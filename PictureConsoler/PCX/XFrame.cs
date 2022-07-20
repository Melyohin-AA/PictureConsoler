using System;
using System.IO;
using System.Drawing;

namespace PictureConsoler.PCX
{
    class XFrame : Frame
    {
        private Color[] colorValues;

        public XFrame(XFrameDeck deck) : base(deck) { }
        public XFrame(XFrameDeck deck, BinaryReader stream) : base(deck, stream)
        {
            colorValues = new Color[16];
            for (byte i = 0; i < 16; i++)
            {
                byte r = stream.ReadByte(), g = stream.ReadByte(), b = stream.ReadByte();
                colorValues[i] = Color.FromArgb(r, g, b);
            }
        }

        public override void Save(BinaryWriter stream)
        {
            base.Save(stream);
            foreach (Color value in colorValues)
            {
                stream.Write(value.R);
                stream.Write(value.G);
                stream.Write(value.B);
            }
        }

        public void ApplyColorValues()
        {
            colorValues.CopyTo(ConsoleColorsLib.ConsoleColors.ConsoleColorValues, 0);
            ConsoleColorsLib.ConsoleColors.ApplyColorValues();
        }

        public override void FillColors(Bitmap bitmap)
        {
            Color[,] sectors = Deck.BitmapToSectors(bitmap);
            DetColourValues(sectors);
            FillColorsX(sectors);
        }
        private void DetColourValues(Color[,] sectors)
        {
            var mcd = new MassColorsDeterminor(System.Linq.Enumerable.Cast<Color>(sectors));
            colorValues = mcd.Determine();
        }
        private void FillColorsX(Color[,] sectors)
        {
            for (int i = 0; i < Deck.FrameH; i++)
            {
                for (int j = 0; j < Deck.FrameW; j++)
                {
                    byte ui = (byte)FrameDeck.GetNearestConsoleColor(sectors[j, i << 1], colorValues);
                    byte li = (byte)FrameDeck.GetNearestConsoleColor(sectors[j, (i << 1) + 1], colorValues);
                    colors[j, i] = (byte)((ui << 4) | li);
                }
            }
        }

        public override Bitmap ToBitmap()
        {
            colorValues.CopyTo(ConsoleColorsLib.ConsoleColors.ConsoleColorValues, 0);
            return base.ToBitmap();
        }
    }
}
