using System;
using System.IO;
using System.Drawing;

namespace PictureConsoler.PCM
{
    class MFrame : Frame
    {
        public MFrameDeck MDeck { get { return (MFrameDeck)Deck; } }

        public MFrame(MFrameDeck deck) : base(deck) { }
        public MFrame(MFrameDeck deck, BinaryReader stream) : base(deck, stream) { }

        public override void FillColors(Bitmap bitmap)
        {
            int x = 0, y = 0;
            for (int ix = Deck.SymbolW; ix < bitmap.Width; ix += Deck.SymbolW)
            {
                for (int iy = Deck.SymbolH; iy < bitmap.Height; iy += Deck.SymbolH)
                    colors[x, y++] = MDeck.FillCons(ix - Deck.SymbolW, iy - Deck.SymbolH, bitmap, MDeck.ColorValues);
                x++;
                y = 0;
            }
        }

        public override Bitmap ToBitmap()
        {
            MDeck.ColorValues.CopyTo(ConsoleColorsLib.ConsoleColors.ConsoleColorValues, 0);
            return base.ToBitmap();
        }
    }
}
