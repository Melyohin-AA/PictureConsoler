using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace PictureConsoler.PCM
{
    class MFrameDeck : FrameDeck
    {
        public const byte sign = 2;
        
        private bool colorValuesApplied;
        public Color[] ColorValues { get; } = new Color[16];

        public MFrameDeck(byte symbolW, byte symbolH, ushort frameW, ushort frameH, ushort frameCount) :
            base(symbolW, symbolH, frameW, frameH, frameCount) { }
        public MFrameDeck(BinaryReader stream) : base(stream)
        {
            for (byte i = 0; i < 16; i++)
            {
                byte r = stream.ReadByte(), g = stream.ReadByte(), b = stream.ReadByte();
                ColorValues[i] = Color.FromArgb(r, g, b);
            }
        }

        public override void ApplyColorValues()
        {
            if (colorValuesApplied) return;
            ColorValues.CopyTo(ConsoleColorsLib.ConsoleColors.ConsoleColorValues, 0);
            ConsoleColorsLib.ConsoleColors.ApplyColorValues();
            colorValuesApplied = true;
        }

        protected override byte GetSign()
        {
            return sign;
        }
        protected override Frame ReadFrame(BinaryReader stream, ushort frameIndex)
        {
            return new MFrame(this, stream);
        }

        protected override void SaveExtraData(BinaryWriter stream)
        {
            foreach (Color value in ColorValues)
            {
                stream.Write(value.R);
                stream.Write(value.G);
                stream.Write(value.B);
            }
        }

        protected override void InitFramesFromPreconsoled(Bitmap[] precFrames)
        {
            var mcd = new PCX.MassColorsDeterminor(EnumerateSectors(precFrames));
            mcd.Determine().CopyTo(ColorValues, 0);
            for (ushort i = 0; i < precFrames.Length; i++)
            {
                Frames[i] = new MFrame(this);
                Frames[i].FillColors(precFrames[i]);
            }
        }
        private IEnumerable<Color> EnumerateSectors(Bitmap[] precFrames)
        {
            foreach (Bitmap precFrame in precFrames)
                foreach (Color sector in BitmapToSectors(precFrame))
                    yield return sector;
        }
    }
}
