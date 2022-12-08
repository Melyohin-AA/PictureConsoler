using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PictureConsoler
{
    static class FontSelector
    {
        private enum StdHandle { OutputHandle = -11 }
        [DllImport("kernel32")]
        private static extern IntPtr GetStdHandle(StdHandle index);

        [DllImport("kernel32")]
        private extern static bool SetConsoleFont(IntPtr hOutput, uint index);
        public static bool SetConsoleFont(uint index)
        {
            return SetConsoleFont(GetStdHandle(StdHandle.OutputHandle), index);
        }

        [DllImport("kernel32")]
        private static extern uint GetNumberOfConsoleFonts();
        public static uint ConsoleFontsCount { get { return GetNumberOfConsoleFonts(); } }

        [DllImport("kernel32")]
        private static extern bool GetConsoleFontInfo(IntPtr hOutput, [MarshalAs(UnmanagedType.Bool)]bool bMaximize,
            uint count, [MarshalAs(UnmanagedType.LPArray), Out] ConsoleFont[] fonts);
        public static ConsoleFont[] ConsoleFonts
        {
            get
            {
                ConsoleFont[] fonts = new ConsoleFont[GetNumberOfConsoleFonts()];
                if (fonts.Length > 0) GetConsoleFontInfo(GetStdHandle(StdHandle.OutputHandle), false,
                    (uint)fonts.Length, fonts);
                return fonts;
            }
        }
        
        public static readonly ConsoleFont[] ConsoleFonts_ = new ConsoleFont[]
        {
            new ConsoleFont() { Index = 0, SizeX = 4, SizeY = 6 },
            new ConsoleFont() { Index = 1, SizeX = 16, SizeY = 8 },
            new ConsoleFont() { Index = 2, SizeX = 6, SizeY = 9 },
            new ConsoleFont() { Index = 3, SizeX = 8, SizeY = 9 },
            new ConsoleFont() { Index = 4, SizeX = 5, SizeY = 12 },
            new ConsoleFont() { Index = 5, SizeX = 7, SizeY = 12 },
            new ConsoleFont() { Index = 6, SizeX = 8, SizeY = 12 },
            new ConsoleFont() { Index = 7, SizeX = 16, SizeY = 12 },
            new ConsoleFont() { Index = 8, SizeX = 12, SizeY = 16 },
            new ConsoleFont() { Index = 9, SizeX = 10, SizeY = 18 },
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ConsoleFont
    {
        public uint Index;
        public short SizeX, SizeY;
    }
}
