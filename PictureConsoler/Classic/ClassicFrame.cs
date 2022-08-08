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
            for (ushort i = 0; i < Deck.FrameH; i++)
            {
                for (ushort j = 0; j < Deck.FrameW; j++)
                {
                    int x = j * Deck.SymbolW, y = i * Deck.SymbolH;
                    colors[j, i] = Deck.FillCons(x, y, bitmap, ClassicFrameDeck.colorValues);
                }
            }
        }
    }
}
