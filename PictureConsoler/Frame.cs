using System;
using System.Drawing;
using ConsoleGraphicsLib;
using System.IO;
using System.Collections.Generic;

namespace PictureConsoler
{
    abstract class Frame
    {
        public const char symbol = '\x00DC';//'\x2584';

        public FrameDeck Deck { get; }
        protected byte[,] colors;
        public byte this[int x, int y]
        {
            get { return colors[x, y]; }
        }
        
        protected Frame(FrameDeck deck)
        {
            Deck = deck;
            colors = new byte[deck.FrameW, deck.FrameH];
        }
        protected Frame(FrameDeck deck, BinaryReader stream)
        {
            Deck = deck;
            colors = new byte[deck.FrameW, deck.FrameH];
            for (ushort ix = 0; ix < deck.FrameW; ix++)
                for (ushort iy = 0; iy < deck.FrameH; iy++)
                    colors[ix, iy] = stream.ReadByte();
        }

        public abstract void FillColors(Bitmap bitmap);

        public virtual void Draw(ConsoleGraphics graph)
        {
            for (ushort iy = 0; iy < Deck.FrameH; iy++)
                for (ushort ix = 0; ix < Deck.FrameW; ix++)
                    graph.Screen[ix, iy].Color = colors[ix, iy];
        }
        public virtual void Draw()
        {
            for (ushort iy = 0; iy < Deck.FrameH; iy++)
            {
                for (ushort ix = 0; ix < Deck.FrameW; ix++)
                {
                    Console.ForegroundColor = (ConsoleColor)(colors[ix, iy] & 0x0F);
                    Console.BackgroundColor = (ConsoleColor)(colors[ix, iy] >> 4);
                    Console.Write(symbol);
                }
                Console.CursorLeft -= Deck.FrameW;
                Console.CursorTop++;
            }
        }

        public virtual void Save(BinaryWriter stream)
        {
            for (ushort ix = 0; ix < Deck.FrameW; ix++)
                for (ushort iy = 0; iy < Deck.FrameH; iy++)
                    stream.Write(colors[ix, iy]);
        }

        public virtual Bitmap ToBitmap()
        {
            Bitmap bitmap = new Bitmap(Deck.SymbolW * Deck.FrameW, Deck.SymbolH * Deck.FrameH);
            var g = Graphics.FromImage(bitmap);
            ushort x = 0, y = 0;
            for (int ix = 0; ix < bitmap.Width; ix += Deck.SymbolW)
            {
                for (int iy = 0; iy < bitmap.Height; iy += Deck.SymbolH)
                {
                    GetBrushesOfSector(colors[x, y++], out Brush upperBrush, out Brush lowerBrush);
                    g.FillRectangle(upperBrush, ix, iy, Deck.SymbolW, Deck.HalfH);
                    g.FillRectangle(lowerBrush, ix, iy + Deck.HalfH, Deck.SymbolW, Deck.HalfH << 1);
                    if (Deck.OddSymbolH) g.FillRectangle(upperBrush, ix, iy + Deck.SymbolH - 1, Deck.SymbolW, 1);
                }
                x++;
                y = 0;
            }
            g.Flush();
            return bitmap;
        }
        private void GetBrushesOfSector(byte doubleColor, out Brush upperBrush, out Brush lowerBrush)
        {
            Color upperColor = ConsoleColorsLib.ConsoleColors.ConsoleColorValues[doubleColor >> 4];
            Color lowerColor = ConsoleColorsLib.ConsoleColors.ConsoleColorValues[doubleColor & 0x0F];
            upperBrush = new SolidBrush(upperColor);
            lowerBrush = new SolidBrush(lowerColor);
        }
    }
}
