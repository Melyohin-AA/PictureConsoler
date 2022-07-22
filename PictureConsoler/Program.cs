﻿using System;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using ConsoleGraphicsLib;
using ConsoleColorsLib;

namespace PictureConsoler
{
    class Program
    {
        public const string Ext = ".pcuf", OldClassicExt = ".pcff", OldXExt = ".pcxf";
        public const string Caption = "Picture Consoler";
        public const short Buff = 0x7FFF;

        public static FrameDeck Deck { get; private set; }
        private static bool auto = false, shiftBuffer = false;
        private static ushort interval = 0;

        private static Filter filter = Filter.None;
        private static Palette palette;
        private static double ohlCompressFactor, ohlDivisionFactor;
        private static bool savePreconsoledResult, saveConsoledResult;

        private static ConsoleGraphics graph;

        private static void Init()
        {
            Console.Title = Caption;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.InputEncoding = Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();
            Console.CursorSize = 50;
            ConsoleColors.TryApplyColorValues();
            for (ushort i = 0; i < 256; i++)
            {
                Console.ForegroundColor = (ConsoleColor)(i & 0x0F);
                Console.BackgroundColor = (ConsoleColor)(i >> 4);
                if ((i & 0x0F) == 0x0F) Console.WriteLine();
                else Console.Write(Frame.symbol);
            }
            PrintHeader();
        }
        private static void PrintHeader()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            string version = $"v[{System.Windows.Forms.Application.ProductVersion}]";
            Console.CursorLeft = 16 - version.Length;
            Console.WriteLine(version);
            Console.Write("\n     ");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  N");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("t  ");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n");
        }

        private static void ReadData(string[] args)
        {
            Console.CursorVisible = true;
            Console.Write("File path: ");
            string path;
            if (args.Length == 0) path = ReadBMP_Path();
            else
            {
                path = args[0];
                Console.WriteLine(path);
            }
            byte pw, ph, fontIndex = 255;
            FileInfo info = new FileInfo(path);
            Console.Title = $"{info.Name} - {Console.Title}";
            string fileExt = info.Extension.ToLower();
            if (IsExtSpecial(fileExt))
            {
                Console.Write("Loading  . . . ");
                if (fileExt == OldClassicExt) Deck = Classic.ClassicFrameDeck.LoadAsPCFF(path);
                else if (fileExt == OldXExt) Deck = PCX.XFrameDeck.LoadAsPCXF(path);
                else Deck = FrameDeck.LoadAsPCUF(path);
                palette = Deck.GetPalette();
                Console.WriteLine();
                pw = Deck.SymbolW;
                ph = Deck.SymbolH;
                foreach (ConsoleFont font in FontSelector.ConsoleFonts_)
                {
                    if ((font.SizeX != pw) || (font.SizeY != ph)) continue;
                    fontIndex = (byte)font.Index;
                    break;
                }
                if (fontIndex == 255) ReadSymbolSize(out pw, out ph, out fontIndex);
                else Console.WriteLine("Font #{0}: {1}x{2}", fontIndex, pw, ph);
            }
            else
            {
                ReadSymbolSize(out pw, out ph, out fontIndex);
                ReadFilterSelection();
                ReadPaletteSelection();
                ReadFlagSaveConsoledResult();
                Console.Write("Loading and fragmentation . . . ");
                LoadFrames(path, pw, ph);
                Console.WriteLine();
                TrySaveAsPCUFOption(path);
            }
            Console.WriteLine("Frame count : " + Deck.Frames.Length);
            if (IsExtSpecial(fileExt))
            {
                TrySaveConsoledOption(path);
                if (fileExt != Ext) TrySaveAsPCUFOption(path);
            }
            if (Deck.Frames.Length > 1)
            {
                Console.Write("Inerval (0-65535) : ");
                interval = ReadDB_Interval();
                ShiftBuffer.CalcColumnRow();
                if (ShiftBuffer.ModeOpportunity)
                {
                    Console.Write("Shift Buffer mode? {Y/N} ");
                    shiftBuffer = ReadDiscretAnswer();
                    if (shiftBuffer && !ShiftBuffer.IsWinSizeValid())
                    {
                        Console.WriteLine("Screen is too small to see all picture in Shift Buffer mode!");
                        Console.ReadKey(true);
                    }
                }
            }
            Console.CursorVisible = false;
            FontSelector.SetConsoleFont(fontIndex);
        }
        private static bool IsExtSpecial(string fileExt)
        {
            return (fileExt == Ext) || (fileExt == OldClassicExt) || (fileExt == OldXExt);
        }
        private static void ReadSymbolSize(out byte width, out byte height, out byte index)
        {
            Console.WriteLine("   Fonts: ");
            var fonts = FontSelector.ConsoleFonts_;
            foreach (ConsoleFont font in fonts)
                Console.WriteLine("#{0} - {1}x{2}", font.Index, font.SizeX, font.SizeY);
            Console.Write("Select font (index): ");
            while (true)
            {
                string strFont = Console.ReadLine();
                if (TryParseAsFontByForce(strFont, out width, out height, out index)) return;
                if (byte.TryParse(strFont, out index) && (index < fonts.Length)) break;
                Console.Write("Parsing failed. Correct: ");
            }
            width = (byte)fonts[index].SizeX;
            height = (byte)fonts[index].SizeY;
        }
        private static bool TryParseAsFontByForce(string strFont, out byte width, out byte height, out byte index)
        {
            width = height = index = 0;
            string[] args = strFont.Split('~');
            if (args.Length != 3) return false;
            if (!byte.TryParse(args[2], out height)) return false;
            height <<= 1;
            return byte.TryParse(args[0], out index) && byte.TryParse(args[1], out width);
        }
        private static void TrySaveConsoledOption(string path)
        {
            ReadFlagSaveConsoledResult();
            if (saveConsoledResult)
            {
                Console.Write("Saving . . . ");
                string consoledSavePath = GetConsoledSavePath(path);
                Deck.SaveConsoled(consoledSavePath);
                Console.WriteLine();
            }
        }
        private static void TrySaveAsPCUFOption(string path)
        {
            Console.Write("Save PCUF file? {Y/N} ");
            if (ReadDiscretAnswer())
            {
                Console.Write("Saving . . . ");
                string filename = path + Ext;
                Deck.SaveAsPCUF(filename);
                Console.WriteLine();
            }
        }

        private static void ReadPaletteSelection()
        {
            Console.Write("Select palette: {F2 - Classic/F4 - PCX/F6 - PCM} ");
            switch (ReadKeyPress(new HashSet<ConsoleKey>(3) { ConsoleKey.F2, ConsoleKey.F4, ConsoleKey.F6 }))
            {
                case ConsoleKey.F2: palette = Palette.Classic; break;
                case ConsoleKey.F4: palette = Palette.PCX; break;
                case ConsoleKey.F6: palette = Palette.PCM; break;
            }
            if ((palette == Palette.PCX) || palette == Palette.PCM)
            {
                Console.Write("Use 6 threads instead of 1 for calculations? {Y/N} ");
                PCX.MassColorsDeterminor.Use6Threads = ReadDiscretAnswer();
                Console.Write("Use reduced 21-bit colors for calculations? {Y/N} ");
                PCX.MassColorsDeterminor.UseReducedColors = ReadDiscretAnswer();
            }
        }

        private static void ReadFilterSelection()
        {
            Console.Write("Select filter: {F1 - None/F3 - Sobel/F5 - Outline Highlighting} ");
            switch (ReadKeyPress(new HashSet<ConsoleKey>(3) { ConsoleKey.F1, ConsoleKey.F3, ConsoleKey.F5 }))
            {
                case ConsoleKey.F3:
                    filter = Filter.Sobel;
                    ReadSobelCap();
                    ReadFlagSavePreconsoledResult();
                    break;
                case ConsoleKey.F5:
                    filter = Filter.OutlineHighlighting;
                    ReadSobelCap();
                    ReadOHL_SobelCompressFactor();
                    ReadOHL_SobelDivisionFactor();
                    ReadOHL_ReverseFlag();
                    ReadFlagSavePreconsoledResult();
                    break;
            }
        }
        private static void ReadOHL_SobelCompressFactor()
        {
            ohlCompressFactor = ReadDoubleParameter("Sobel-compress factor", 0.3, 0.0, 1.0);
        }
        private static void ReadOHL_SobelDivisionFactor()
        {
            ohlDivisionFactor = ReadDoubleParameter("Sobel-division factor", 0.2, 0.0, 1.0);
        }
        private static void ReadFlagSavePreconsoledResult()
        {
            Console.Write("Save preconsoled result? {Y/N} ");
            savePreconsoledResult = ReadDiscretAnswer();
        }
        private static void ReadFlagSaveConsoledResult()
        {
            Console.Write("Save consoled result? {Y/N} ");
            saveConsoledResult = ReadDiscretAnswer();
        }
        private static void ReadOHL_ReverseFlag()
        {
            Console.Write("Do reverse highlighting? {Y/N} ");
            OutlineHighlighter.Revesre = ReadDiscretAnswer();
        }
        private static void ReadSobelCap()
        {
            SobelFilter.SobelCap = ReadDoubleParameter("Cap of sobel-value", 1.0, 0.5, 4.0);
        }
        private static double ReadDoubleParameter(string message, double default_value,
            double lower_limit, double upper_limit)
        {
            message = $"{message} {{{lower_limit.SmartToString()} - {upper_limit.SmartToString()}/default {default_value.SmartToString()}}} : ";
            Console.Write(message);
            double result;
            while (true)
            {
                string input = Console.ReadLine();
                if (input.Length == 0) return default_value;
                if (!double.TryParse(input.Replace('.', ','), out result))
                {
                    Console.Write("Parsing failed. Correct : ");
                    continue;
                }
                if ((result >= lower_limit) && (result <= upper_limit)) return result;
                Console.Write("Value is out of range. Correct : ");
            }
        }

        private static ConsoleKey ReadKeyPress(HashSet<ConsoleKey> available_keys)
        {
            ConsoleKey key = ConsoleKey.Separator;
            do key = Console.ReadKey(true).Key; while (!available_keys.Contains(key));
            Console.WriteLine(key);
            return key;
        }
        private static bool ReadDiscretAnswer()
        {
            ConsoleKey key = ConsoleKey.Separator;
            do key = Console.ReadKey(true).Key; while ((key != ConsoleKey.Y) && (key != ConsoleKey.N));
            Console.WriteLine((key == ConsoleKey.Y) ? 'Y' : 'N');
            return key == ConsoleKey.Y;
        }
        private static string ReadBMP_Path()
        {
            string path;
            while (true)
            {
                path = Console.ReadLine();
                if (!File.Exists(path))
                {
                    Console.Write("No such file. Correct: ");
                    continue;
                }
                return path;
            }
        }
        private static ushort ReadDB_Interval()
        {
            ushort result;
            while (!ushort.TryParse(Console.ReadLine(), out result)) Console.Write("Parsing failed. Correct: ");
            return result;
        }

        private static void Main(string[] args)
        {
            if (args.Length > 2) throw new ArgumentException();
            Init();
            ReadData(args);
            Console.SetWindowSize(1, 1);
            Console.SetCursorPosition(0, 0);
            if (shiftBuffer) ShiftBuffer.SetBufferSize();
            else
            {
                graph = new ConsoleGraphics(Deck.FrameW, Deck.FrameH, new ScreenCell(Frame.symbol, 0, false));
                Console.SetBufferSize(Deck.FrameW + 1, Deck.FrameH + 1);
            }
            Console.WindowWidth = Math.Min(Deck.FrameW, (ushort)20);
            Console.Clear();
            CleanUpKeyBuffer();
            if (shiftBuffer)
            {
                ShiftBuffer.Prepare();
                if (interval == 0) ShiftBuffer.LoopDisplayImmediately();
                else ShiftBuffer.LoopDisplay();
            }
            else if ((interval == 0) || (Deck.Frames.Length == 1)) LoopDisplayImmediately();
            else LoopDisplay();
        }

        private static void LoopDisplay()
        {
            Stopwatch stopwatch = new Stopwatch();
            bool update = false;
            while (true)
            {
                int delta = 0;
                if (auto) stopwatch.Restart();
                if (update) update = false;
                else Deck.Cons.Draw(graph);
                Deck.NextFrame();
                graph.Redraw();
                if (auto)
                {
                    delta = (int)(interval - stopwatch.ElapsedMilliseconds);
                    Console.Title = Caption + ' ' + delta;
                    if (delta > 0)
                    {
                        update = true;
                        Deck.Cons.Draw(graph);
                        Deck.ApplyColorValues();
                        while (stopwatch.ElapsedMilliseconds < interval) Thread.Sleep(1);
                    }
                    stopwatch.Stop();
                }
                ReadKey();
            }
        }
        private static void LoopDisplayImmediately()
        {
            while (true)
            {
                Deck.Cons.Draw(graph);
                Deck.ApplyColorValues();
                graph.Redraw();
                ReadKey();
                Deck.NextFrame();
            }
        }

        private static void CheckKeyCMD(ref bool change_auto)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.Tab: change_auto = true; break;
                case ConsoleKey.R:
                    Deck.CurrentFrame = shiftBuffer ? (ushort)0 : (ushort)(Deck.Frames.Length - 1);
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.LeftArrow:
                    if (interval > 0) interval--;
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.RightArrow:
                    if (interval < 0xFFFE) interval++;
                    break;
            }
        }
        private static void ReadKey()
        {
            bool change_auto = false;
            if (Console.KeyAvailable) CheckKeyCMD(ref change_auto);
            do_change_auto:
            if (change_auto)
            {
                if (auto && (interval != 0)) Console.Title = Caption;
                auto = !auto;
                change_auto = false;
            }
            if (!auto)
            {
                while (Console.KeyAvailable) CleanUpKeyBuffer();
                CheckKeyCMD(ref change_auto);
                if (change_auto) goto do_change_auto;
            }
            CleanUpKeyBuffer();
        }

        private static void CleanUpKeyBuffer()
        {
            while (Console.KeyAvailable) Console.ReadKey(true);
        }

        private static void LoadFrames(string path, byte symbolW, byte symbolH)
        {
            var stopwatch = Stopwatch.StartNew();
            Image image = Image.FromFile(path);
            Deck = FrameDeck.FromImage(symbolW, symbolH, image,
                GetFrameDeckConstructor(), GetFrameFilter(),
                GetPreconsoledSavePath(path), GetConsoledSavePath(path));
            stopwatch.Stop();
            Console.Write("\nTime elapsed = {0}ms", stopwatch.ElapsedMilliseconds);
        }
        private static FrameDeck.Constructor GetFrameDeckConstructor()
        {
            FrameDeck.Constructor constructor;
            switch (palette)
            {
                case Palette.Classic:
                    constructor = (sw, sh, fw, fh, fc) => new Classic.ClassicFrameDeck(sw, sh, fw, fh, fc);
                    break;
                case Palette.PCX:
                    constructor = (sw, sh, fw, fh, fc) => new PCX.XFrameDeck(sw, sh, fw, fh, fc);
                    break;
                case Palette.PCM:
                    constructor = (sw, sh, fw, fh, fc) => new PCM.MFrameDeck(sw, sh, fw, fh, fc);
                    break;
                default: throw new Exception();
            }
            return constructor;
        }
        private static Func<Bitmap, Bitmap> GetFrameFilter()
        {
            Func<Bitmap, Bitmap> filterF = null;
            switch (filter)
            {
                case Filter.Sobel: filterF = (frameBmp) => SobelFilter.Filter(frameBmp); break;
                case Filter.OutlineHighlighting:
                    filterF = (frameBmp) =>
                    {
                        double[,] sobelGrayscale = SobelFilter.GetGrayscaleSobel(frameBmp);
                        OutlineHighlighter.Highlight(frameBmp, sobelGrayscale, ohlCompressFactor, ohlDivisionFactor);
                        return frameBmp;
                    }; break;
            }
            return filterF;
        }
        private static string GetPreconsoledSavePath(string path)
        {
            if (!savePreconsoledResult) return null;
            string preconsoledImagePath = path + "~" + ((filter == Filter.Sobel) ? "sobel" : "ohl");
            if ((filter == Filter.OutlineHighlighting) && OutlineHighlighter.Revesre) preconsoledImagePath += "r";
            return preconsoledImagePath;
        }
        private static string GetConsoledSavePath(string path)
        {
            if (!saveConsoledResult) return null;
            string consoledImagePath = $"{path}~{palette}";
            if (filter == Filter.Sobel) consoledImagePath += "-sobel";
            else if (filter == Filter.OutlineHighlighting) consoledImagePath += "-ohl";
            if ((filter == Filter.OutlineHighlighting) && OutlineHighlighter.Revesre) consoledImagePath += "r";
            return consoledImagePath;
        }

        public static partial class ShiftBuffer
        {
            public static bool ModeOpportunity { get; private set; }
            public static ushort MaxColumnCount { get; private set; }
            public static ushort MaxRowCount { get; private set; }
            public static ushort ColumnCount { get; private set; }
            public static ushort RowCount { get; private set; }

            private static ushort frameX, frameY, frameRow = 0;
            private static void Scroll()
            {
                Console.SetWindowPosition(frameX, frameY);
            }

            private static void ResetCursorTop()
            {
                frameX = frameY = frameRow = 0;
                Scroll();
            }
            private static void NextColumn()
            {
                frameX += Deck.FrameW;
                frameRow = frameY = 0;
                Scroll();
            }
            private static void Shift()
            {
                if (Deck.CurrentFrame > 0)
                {
                    frameRow++;
                    if (frameRow < RowCount)
                    {
                        frameY += Deck.FrameH;
                        Scroll();
                    }
                    else NextColumn();
                    return;
                }
                ResetCursorTop();
            }

            private static void NormalizeWinSize_ExceptionIllegal()
            {
                Console.WindowWidth = (ColumnCount == 1) ? Deck.FrameW + 1 : Deck.FrameW;
                Console.WindowHeight = Deck.FrameH;
            }
            private static void NormalizeWinSize()
            {
                try { Console.WindowWidth = (ColumnCount == 1) ? Deck.FrameW + 1 : Deck.FrameW; }
                catch (ArgumentOutOfRangeException) { }
                try { Console.WindowHeight = Deck.FrameH; }
                catch (ArgumentOutOfRangeException) { }
            }

            public static void Prepare()
            {
                int i = 0;
                foreach (Frame frame in Deck.Frames)
                {
                    if (i == RowCount)
                    {
                        Console.CursorLeft += Deck.FrameW;
                        Console.CursorTop = i = 0;
                    }
                    frame.Draw();
                    i++;
                }
                NormalizeWinSize();
                ResetCursorTop();
                Deck.ApplyColorValues();
            }
            public static void LoopDisplay()
            {
                Stopwatch stopwatch = new Stopwatch();
                while (true)
                {
                    int delta = 0;
                    if (auto) stopwatch.Restart();
                    stopwatch.Stop();
                    Deck.NextFrame();
                    delta = (int)(interval - stopwatch.ElapsedMilliseconds);
                    if (delta > 0) Thread.Sleep(delta);
                    NormalizeWinSize();
                    ReadKey();
                    Shift();
                    Deck.ApplyColorValues();
                }
            }
            public static void LoopDisplayImmediately()
            {
                while (true)
                {
                    Deck.NextFrame();
                    NormalizeWinSize();
                    ReadKey();
                    Shift();
                    Deck.ApplyColorValues();
                }
            }

            public static bool IsWinSizeValid()
            {
                int width = Console.WindowWidth, height = Console.WindowHeight;
                try
                {
                    NormalizeWinSize_ExceptionIllegal();
                    Console.WindowWidth = width;
                    Console.WindowHeight = height;
                    return true;
                }
                catch (Exception)
                {
                    Console.WindowWidth = width;
                    Console.WindowHeight = height;
                    return false;
                }
            }
            public static void CalcColumnRow()
            {
                ModeOpportunity = true;
                MaxRowCount = (ushort)(Buff / Deck.FrameH);
                MaxColumnCount = (ushort)(Buff / Deck.FrameW);
                if (Deck.Frames.Length <= MaxRowCount)
                {
                    RowCount = (ushort)Deck.Frames.Length;
                    ColumnCount = 1;
                    return;
                }
                RowCount = MaxRowCount;
                ColumnCount = (ushort)Math.Ceiling(Deck.Frames.Length / (float)MaxRowCount);
                if (ColumnCount > MaxColumnCount) ModeOpportunity = false;
            }
            public static void SetBufferSize()
            {
                Console.SetBufferSize(Deck.FrameW * ColumnCount + 1, Deck.FrameH * RowCount + 1);
            }
        }
    }

    enum Filter
    {
        None,
        Sobel,
        OutlineHighlighting,
    }

    enum Palette
    {
        Classic,
        PCX,
        PCM,
    }
}