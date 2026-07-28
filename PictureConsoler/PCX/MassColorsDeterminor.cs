using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace PictureConsoler.PCX
{
    class MassColorsDeterminor
    {
        public const byte massColorCount = 16, six = 6;

        public static bool Use6Threads { get; set; }
        public static bool UseReducedColors { get; set; }
        public static bool IgnoreColorCount { get; set; }
        public static Commons.PCX.MassLogger Logger { get; set; }

        private readonly IEnumerable<Color> sectors;
        private readonly HashSet<Color> colorSet = new HashSet<Color>();
        private readonly Dictionary<Color, int> colors = new Dictionary<Color, int>();
        private readonly Color[] massColors = new Color[massColorCount];
        private readonly byte[] dirs = new byte[massColorCount];
        private ulong delta, minDelta;

        public MassColorsDeterminor(IEnumerable<Color> sectors)
        {
            this.sectors = sectors ?? throw new ArgumentNullException();
        }

        public Color[] Determine()
        {
            minDelta = ulong.MaxValue;
            FillColors();
            if (colorSet.Count <= massColorCount)
                SetMassColorsAsEnough();
            else
            {
                DoMassDeal();
                Logger?.Start(UseReducedColors, IgnoreColorCount, minDelta, massColors);
                if (Use6Threads) DoMassDescent6T();
                else DoMassDescent();
                Logger?.Stop();
            }
            //return System.Linq.Enumerable.ToArray(System.Linq.Enumerable.OrderBy(massColors, c => c.ToArgb));
            return (Color[])massColors.Clone();
        }
        private void FillColors()
        {
            colorSet.Clear();
            colors.Clear();
            foreach (Color pixel in sectors)
            {
                Color color = UseReducedColors ? Color.FromArgb(pixel.R & 0xFE, pixel.G & 0xFE, pixel.B & 0xFE) : pixel;
                colorSet.Add(color);
                if (colors.TryGetValue(color, out int count))
                    colors[color] = count + 1;
                else colors.Add(color, 1);
            }
        }
        private void SetMassColorsAsEnough()
        {
            byte i = 0;
            foreach (Color color in colorSet)
            {
                massColors[i] = color;
                i++;
            }
        }

        private void DoMassDeal()
        {
            // I hate mass deal. It's so bad. At least it is relatively fast
            var dealingDict = CollectDealingDict();
            for (byte i = 0; i < massColorCount; i++)
            {
                Color massColor = CalcAverageColor(dealingDict);
                if (i < massColorCount - 1) CorrectDealingDict(dealingDict, massColor);
                massColors[i] = massColor;
            }
        }
        private Dictionary<Color, double> CollectDealingDict()
        {
            var dealingDict = new Dictionary<Color, double>(colors.Count);
            foreach (var colorPair in colors)
                dealingDict.Add(colorPair.Key, IgnoreColorCount ? 1.0 : colorPair.Value);
            return dealingDict;
        }
        private Color CalcAverageColor(Dictionary<Color, double> dealingDict)
        {
            double totalR = 0.0, totalG = 0.0, totalB = 0.0;
            double totalCount = 0.0;
            foreach (var colorPair in dealingDict)
            {
                totalR += colorPair.Key.R * colorPair.Value;
                totalG += colorPair.Key.G * colorPair.Value;
                totalB += colorPair.Key.B * colorPair.Value;
                totalCount += colorPair.Value;
            }
            byte r, g, b;
            checked
            {
                r = (byte)(totalR / totalCount);
                g = (byte)(totalG / totalCount);
                b = (byte)(totalB / totalCount);
            }
            return Color.FromArgb(r, g, b);
        }
        private void CorrectDealingDict(Dictionary<Color, double> dealingDict, Color massColor)
        {
            const uint normDist2 = 3 * 64 * 64;
            foreach (var colorPair in new HashSet<KeyValuePair<Color, double>>(dealingDict))
            {
                uint dist2 = CalcDist2(colorPair.Key, massColor);
                dealingDict[colorPair.Key] = colorPair.Value * dist2 / normDist2;
            }
        }

        private void DoMassDescent()
        {
            bool haveChanges;
            minDelta = CalcDelta();
            do
            {
                DoMassDescentIteration(out haveChanges);
                Logger?.Log(minDelta, dirs);
            } while (haveChanges);
        }
        private void DoMassDescentIteration(out bool haveChanges)
        {
            haveChanges = false;
            for (byte i = 0; i < massColorCount; i++)
            {
                Color oldMC = massColors[i], minMC = oldMC, newMC;
                byte minMCJ = 6; // no dir
                for (byte j = 0; j < 6; j++)
                {
                    newMC = Commons.PCX.Mass.ModifyMassColor(oldMC, j);
                    if (newMC == Color.Empty) continue;
                    massColors[i] = newMC;
                    ulong newDeltaOfIteration = CalcDelta();
                    if (newDeltaOfIteration < minDelta)
                    {
                        minDelta = newDeltaOfIteration;
                        minMC = newMC;
                        minMCJ = j;
                        haveChanges = true;
                    }
                }
                massColors[i] = minMC;
                dirs[i] = minMCJ;
            }
            delta = minDelta;
        }

        private void DoMassDescent6T()
        {
            var deltas = new ulong[six];
            var newMCs = new Color[six];
            var threads = new Thread[six];
            bool haveChanges;
            minDelta = CalcDelta();
            do
            {
                DoMassDescentIteration6T(deltas, newMCs, threads, out haveChanges);
                Logger?.Log(minDelta, dirs);
            } while (haveChanges);
        }
        private void DoMassDescentIteration6T(ulong[] deltas, Color[] newMCs, Thread[] threads, out bool haveChanges)
        {
            haveChanges = false;
            for (byte i = 0; i < massColorCount; i++)
            {
                Color oldMC = massColors[i], minMC = oldMC;
                byte minMCJ = 6; // no dir
                for (byte j = 0; j < six; j++)
                {
                    deltas[j] = ulong.MaxValue;
                    newMCs[j] = Commons.PCX.Mass.ModifyMassColor(oldMC, j);
                    if (newMCs[j] == Color.Empty) continue;
                    // TODO: Stop creating threads!
                    threads[j] = new Thread((j_) =>
                    {
                        var modifiedMassColors = (Color[])massColors.Clone();
                        modifiedMassColors[i] = newMCs[(byte)j_];
                        deltas[(byte)j_] = CalcDelta(modifiedMassColors);
                    });
                    threads[j].Start(j);
                }
                for (byte j = 0; j < six; j++)
                {
                    if (newMCs[j] == Color.Empty) continue;
                    threads[j].Join();
                    if (deltas[j] < minDelta)
                    {
                        minDelta = deltas[j];
                        minMC = newMCs[j];
                        minMCJ = j;
                        haveChanges = true;
                    }
                }
                massColors[i] = minMC;
                dirs[i] = minMCJ;
            }
            delta = minDelta;
        }

        private ulong CalcDelta(Color[] massColors)
        {
            ulong delta = 0;
            foreach (var colorPair in colors)
            {
                uint minLocalDelta = uint.MaxValue;
                foreach (Color massColor in massColors)
                {
                    uint dist2 = CalcDist2(colorPair.Key, massColor);
                    uint localDelta = IgnoreColorCount ? dist2 : (uint)(dist2 * colorPair.Value);
                    if (localDelta < minLocalDelta) minLocalDelta = localDelta;
                }
                delta += minLocalDelta;
            }
            return delta;
        }
        private ulong CalcDelta()
        {
            return CalcDelta(massColors);
        }
        private static uint CalcDist2(Color a, Color b)
        {
            short rd = (short)(a.R - b.R);
            short rg = (short)(a.G - b.G);
            short rb = (short)(a.B - b.B);
            return (uint)(rd * rd + rg * rg + rb * rb);
        }
    }
}
