using System;
using System.IO;
using System.Drawing;

namespace PictureConsoler.Classic
{
    class ClassicFrameDeck : FrameDeck
    {
        public const byte sign = 0;
        public static readonly Color[] colorValues = new Color[16];

        private bool colorValuesApplied;

        public ClassicFrameDeck(byte symbolW, byte symbolH, ushort frameW, ushort frameH, ushort frameCount) :
            base(symbolW, symbolH, frameW, frameH, frameCount) { }
        public ClassicFrameDeck(BinaryReader stream) : base(stream) { }

        public override void ApplyColorValues()
        {
            if (colorValuesApplied) return;
            colorValues.CopyTo(ConsoleColorsLib.ConsoleColors.ConsoleColorValues, 0);
            ConsoleColorsLib.ConsoleColors.ApplyColorValues();
            colorValuesApplied = true;
        }

        protected override byte GetSign()
        {
            return sign;
        }
        protected override Frame ReadFrame(BinaryReader stream, ushort frameIndex)
        {
            return new ClassicFrame(this, stream);
        }

        static ClassicFrameDeck()
        {
            const byte a = 128, b = 192, c = 255;
            for (byte i = 0; i < 7; i++) colorValues[i] = GetColorBin(i, a);
            colorValues[7] = Color.FromArgb(b, b, b);
            colorValues[8] = Color.FromArgb(a, a, a);
            for (byte i = 1; i < 8; i++) colorValues[i + 8] = GetColorBin(i, c);
        }

        private static Color GetColorBin(byte bin, int one)
        {
            int r = ((bin & 4) != 0) ? one : 0;
            int g = ((bin & 2) != 0) ? one : 0;
            int b = ((bin & 1) != 0) ? one : 0;
            return Color.FromArgb(r, g, b);
        }

        protected override void InitFramesFromPreconsoled(Bitmap[] precFrames)
        {
            for (ushort i = 0; i < precFrames.Length; i++)
            {
                Frames[i] = new ClassicFrame(this);
                Frames[i].FillColors(precFrames[i]);
            }
        }

        public static FrameDeck LoadAsPCFF(string path)
        {
            BinaryReader stream = new BinaryReader(new FileStream(path, FileMode.Open), System.Text.Encoding.UTF8);
            byte symbolW = stream.ReadByte(), symbolH = stream.ReadByte();
            ushort frameW = stream.ReadUInt16(), frameH = stream.ReadUInt16();
            long bytesLeft = stream.BaseStream.Length - stream.BaseStream.Position;
            ushort frameCount;
            checked { frameCount = (ushort)(bytesLeft / (frameW * frameH)); }
            ClassicFrameDeck deck = new ClassicFrameDeck(symbolW, symbolH, frameW, frameH, frameCount);
            for (ushort i = 0; i < frameCount; i++)
                deck.Frames[i] = new ClassicFrame(deck, stream);
            stream.Close();
            return deck;
        }
    }
}
