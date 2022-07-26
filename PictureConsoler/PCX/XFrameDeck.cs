﻿using System;
using System.IO;
using System.Drawing;

namespace PictureConsoler.PCX
{
    class XFrameDeck : FrameDeck
    {
        public const byte sign = 1;

        public XFrame PrevProcessedFrame { get; private set; }

        public XFrameDeck(byte symbolW, byte symbolH, ushort frameW, ushort frameH, ushort frameCount) :
            base(symbolW, symbolH, frameW, frameH, frameCount) { }
        public XFrameDeck(BinaryReader stream) : base(stream) { }

        public override void ApplyColorValues()
        {
            ((XFrame)Cons).ApplyColorValues();
        }

        protected override byte GetSign()
        {
            return sign;
        }
        protected override Frame ReadFrame(BinaryReader stream, ushort frameIndex)
        {
            return new XFrame(this, stream);
        }

        protected override void InitFramesFromPreconsoled(Bitmap[] precFrames)
        {
            PrevProcessedFrame = null;
            for (ushort i = 0; i < precFrames.Length; i++)
            {
                XFrame frame = new XFrame(this);
                Frames[i] = frame;
                frame.FillColors(precFrames[i]);
                PrevProcessedFrame = frame;
            }
            PrevProcessedFrame = null;
        }

        public static FrameDeck LoadAsPCXF(string path)
        {
            BinaryReader stream = new BinaryReader(new FileStream(path, FileMode.Open), System.Text.Encoding.UTF8);
            byte symbolW = stream.ReadByte(), symbolH = stream.ReadByte();
            ushort frameW = stream.ReadUInt16(), frameH = stream.ReadUInt16();
            long bytesLeft = stream.BaseStream.Length - stream.BaseStream.Position;
            ushort frameCount;
            checked { frameCount = (ushort)(bytesLeft / (frameW * frameH + 16 * 3)); }
            XFrameDeck deck = new XFrameDeck(symbolW, symbolH, frameW, frameH, frameCount);
            for (ushort i = 0; i < frameCount; i++)
                deck.Frames[i] = new XFrame(deck, stream);
            stream.Close();
            return deck;
        }

        public class XFromImageFactory : FromImageFactory
        {
            public XFromImageFactory(byte symbolW, byte symbolH, Image source, Func<Bitmap, Bitmap> filter) :
                base(symbolW, symbolH, source, filter) { }

            protected override FrameDeck GetNewDeckObject(ushort frameW, ushort frameH, ushort frameCount)
            {
                return new XFrameDeck(symbolW, symbolH, frameW, frameH, frameCount);
            }
        }
    }
}
