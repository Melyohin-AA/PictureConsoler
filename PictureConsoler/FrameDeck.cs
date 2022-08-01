using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace PictureConsoler
{
    abstract class FrameDeck
    {
        public const string mark = "PCUF";

        public Frame[] Frames { get; protected set; }
        public ushort CurrentFrame { get; set; }
        public byte SymbolW { get; }
        public byte SymbolH { get; }
        public ushort FrameW { get; }
        public ushort FrameH { get; }
        public uint UpperHalfs { get; private set; }
        public uint LowerHalfs { get; private set; }
        public byte HalfH { get; private set; }
        public bool OddSymbolH { get; private set; }

        public Frame Cons
        {
            get { return Frames[CurrentFrame]; }
            private set { Frames[CurrentFrame] = value; }
        }

        public void NextFrame()
        {
            CurrentFrame++;
            if (CurrentFrame == Frames.Length) CurrentFrame = 0;
        }

        public FrameDeck(byte symbolW, byte symbolH, ushort frameW, ushort frameH, ushort frameCount)
        {
            if ((symbolW == 0) || (symbolH == 0) || (frameW == 0) || (frameH == 0) || (frameCount == 0))
                throw new ArgumentOutOfRangeException();
            SymbolW = symbolW;
            SymbolH = symbolH;
            FrameW = frameW;
            FrameH = frameH;
            InitSecondaryProperties();
            Frames = new Frame[frameCount];
        }

        protected FrameDeck(BinaryReader stream)
        {
            SymbolW = stream.ReadByte();
            SymbolH = stream.ReadByte();
            FrameW = stream.ReadUInt16();
            FrameH = stream.ReadUInt16();
            InitSecondaryProperties();
            ushort frameCount = stream.ReadUInt16();
            Frames = new Frame[frameCount];
            for (ushort i = 0; i < frameCount; i++)
                Frames[i] = ReadFrame(stream, i);
        }
        protected abstract Frame ReadFrame(BinaryReader stream, ushort frameIndex);

        private void InitSecondaryProperties()
        {
            OddSymbolH = (SymbolH & 1) == 1;
            LowerHalfs = (uint)(SymbolW * (SymbolH >> 1));
            UpperHalfs = OddSymbolH ? LowerHalfs + SymbolW : LowerHalfs;
            HalfH = (byte)(SymbolH >> 1);
        }

        public Color[,] BitmapToSectors(Bitmap bitmap)
        {
            Color[,] sectors = new Color[FrameW, FrameH << 1];
            int x = 0, y = 0;
            for (int ix = SymbolW; ix < bitmap.Width; ix += SymbolW)
            {
                for (int iy = SymbolH; iy < bitmap.Height; iy += SymbolH)
                {
                    sectors[x, y++] = GetAverageUpperSectorColor(ix - SymbolW, iy - SymbolH, bitmap);
                    sectors[x, y++] = GetAverageLowerSectorColor(ix - SymbolW, iy - SymbolH, bitmap);
                }
                x++;
                y = 0;
            }
            return sectors;
        }

        public Palette GetPalette()
        {
            if (this is Classic.ClassicFrameDeck) return Palette.Classic;
            if (this is PCX.XFrameDeck) return Palette.PCX;
            if (this is PCM.MFrameDeck) return Palette.PCM;
            throw new Exception();
        }

        public void SaveConsoled(string savePath)
        {
            if (Frames.Length > 1)
            {
                GIF_Builder cGifBuilder = new GIF_Builder();
                foreach (Frame cFrame in Frames)
                    cGifBuilder.AddFrame(cFrame.ToBitmap());
                cGifBuilder.Save(savePath + ".gif");
            }
            else Frames[0].ToBitmap().Save(savePath + ".png", ImageFormat.Png);
        }

        public abstract void ApplyColorValues();

        public void SaveAsPCUF(string path)
        {
            if (File.Exists(path)) File.WriteAllBytes(path, new byte[0]);
            BinaryWriter stream = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate),
                System.Text.Encoding.UTF8);
            stream.Write(mark.ToCharArray());
            stream.Write(GetSign());
            stream.Write(SymbolW);
            stream.Write(SymbolH);
            stream.Write(FrameW);
            stream.Write(FrameH);
            stream.Write((ushort)Frames.Length);
            foreach (Frame frame in Frames) frame.Save(stream);
            SaveExtraData(stream);
            stream.Close();
        }
        protected abstract byte GetSign();
        protected virtual void SaveExtraData(BinaryWriter stream) { }

        protected abstract void InitFramesFromPreconsoled(Bitmap[] precFrames);

        public static FrameDeck LoadAsPCUF(string path)
        {
            BinaryReader stream = new BinaryReader(new FileStream(path, FileMode.Open), System.Text.Encoding.UTF8);
            if (string.Join("", stream.ReadChars(mark.Length)) != mark)
                throw new IOException("Trying to load unsupported format file!");
            byte sign = stream.ReadByte();
            FrameDeck deck;
            switch (sign)
            {
                case Classic.ClassicFrameDeck.sign: deck = new Classic.ClassicFrameDeck(stream); break;
                case PCX.XFrameDeck.sign: deck = new PCX.XFrameDeck(stream); break;
                case PCM.MFrameDeck.sign: deck = new PCM.MFrameDeck(stream); break;
                default: throw new IOException("Uknown PCUF-sign!");
            }
            stream.Close();
            return deck;
        }

        public byte FillCons(int x, int y, Bitmap bitmap, Color[] colorValues)
        {
            Color upper = GetAverageUpperSectorColor(x, y, bitmap);
            Color lower = GetAverageLowerSectorColor(x, y, bitmap);
            byte up = (byte)GetNearestConsoleColor(upper, colorValues);
            byte low = (byte)GetNearestConsoleColor(lower, colorValues);
            return (byte)((up << 4) | low);
        }
        public static ConsoleColor GetNearestConsoleColor(Color color, Color[] colorValues)
        {
            int minDelta = color.CalcDistance2(colorValues[0]);
            byte bestInd = 0;
            for (byte i = 1; i < colorValues.Length; i++)
            {
                int delta = color.CalcDistance2(colorValues[i]);
                if (delta >= minDelta) continue;
                minDelta = delta;
                bestInd = i;
            }
            return (ConsoleColor)bestInd;
        }
        protected Color GetAverageUpperSectorColor(int x, int y, Bitmap bitmap)
        {
            uint r = 0, g = 0, b = 0;
            int yu = y + HalfH;
            for (int iy = y; iy < yu; iy++)
                AddRGBFromSectorLine(x, iy, bitmap, ref r, ref g, ref b);
            if (OddSymbolH) AddRGBFromSectorLine(x, y + SymbolH - 1, bitmap, ref r, ref g, ref b);
            r /= UpperHalfs;
            g /= UpperHalfs;
            b /= UpperHalfs;
            return Color.FromArgb((int)r, (int)g, (int)b);
        }
        protected Color GetAverageLowerSectorColor(int x, int y, Bitmap bitmap)
        {
            uint r = 0, g = 0, b = 0;
            int yu = y + (HalfH << 1);
            for (int iy = y + HalfH; iy < yu; iy++)
                AddRGBFromSectorLine(x, iy, bitmap, ref r, ref g, ref b);
            r /= LowerHalfs;
            g /= LowerHalfs;
            b /= LowerHalfs;
            return Color.FromArgb((int)r, (int)g, (int)b);
        }
        private void AddRGBFromSectorLine(int x, int iy, Bitmap bitmap, ref uint r, ref uint g, ref uint b)
        {
            int xu = x + SymbolW;
            for (int ix = x; ix < xu; ix++)
            {
                r += bitmap.GetPixel(ix, iy).R;
                g += bitmap.GetPixel(ix, iy).G;
                b += bitmap.GetPixel(ix, iy).B;
            }
        }

        public abstract class FromImageFactory
        {
            protected readonly byte symbolW, symbolH;
            protected readonly Image source;
            protected readonly Func<Bitmap, Bitmap> filter;

            public ushort FrameCount { get; private set; }
            public event Action<Bitmap> PreconsoledFrameProcessedEvent;
            public event Action<Frame> ConsoledFrameProcessedEvent;

            public FromImageFactory(byte symbolW, byte symbolH, Image source, Func<Bitmap, Bitmap> filter)
            {
                if ((symbolW == 0) || (symbolH == 0)) throw new ArgumentOutOfRangeException();
                this.source = source ?? throw new ArgumentNullException();
                this.symbolW = symbolW;
                this.symbolH = symbolH;
                this.filter = filter;
            }

            public FrameDeck Create()
            {
                Bitmap[] precFrames = GetPreconsoledFrames();
                return CollectFrameDeck(precFrames);
            }
            private Bitmap[] GetPreconsoledFrames()
            {
                FrameDimension dim = new FrameDimension(source.FrameDimensionsList[0]);
                checked { FrameCount = (ushort)source.GetFrameCount(dim); }
                var precFrames = new Bitmap[FrameCount];
                for (ushort i = 0; i < FrameCount; i++)
                {
                    source.SelectActiveFrame(dim, i);
                    var precFrame = new Bitmap(source);
                    if (filter != null) precFrame = filter(precFrame);
                    PreconsoledFrameProcessedEvent?.Invoke(precFrame);
                    precFrames[i] = precFrame;
                }
                return precFrames;
            }
            private FrameDeck CollectFrameDeck(Bitmap[] precFrames)
            {
                ushort frameW = (ushort)(precFrames[0].Width / symbolW);
                ushort frameH = (ushort)(precFrames[0].Height / symbolH);
                FrameDeck deck = GetNewDeckObject(frameW, frameH, (ushort)precFrames.Length);
                deck.InitFramesFromPreconsoled(precFrames);
                if (ConsoledFrameProcessedEvent != null)
                {
                    foreach (Frame cFrame in deck.Frames)
                        ConsoledFrameProcessedEvent(cFrame);
                }
                return deck;
            }

            protected abstract FrameDeck GetNewDeckObject(ushort frameW, ushort frameH, ushort frameCount);
        }
    }
}
