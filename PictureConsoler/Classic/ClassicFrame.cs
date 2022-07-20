using System;
using System.IO;
using System.Drawing;

namespace PictureConsoler.Classic
{
    class ClassicFrame : Frame
    {
        public ClassicFrame(ClassicFrameDeck deck) : base(deck) { }
        public ClassicFrame(ClassicFrameDeck deck, BinaryReader stream) : base(deck, stream) { }

        public override void FillColors(Bitmap bitmap)
        {
            int limX = bitmap.Width - Deck.SymbolW, limY = bitmap.Height - Deck.SymbolH;
            ushort x = 0, y = 0;
            for (int ix = 0; ix <= limX; ix += Deck.SymbolW)
            {
                for (int iy = 0; iy <= limY; iy += Deck.SymbolH)
                    colors[x, y++] = Deck.FillCons(ix, iy, bitmap,
                        ClassicFrameDeck.colorValues);
                x++;
                y = 0;
            }
        }
    }
}
