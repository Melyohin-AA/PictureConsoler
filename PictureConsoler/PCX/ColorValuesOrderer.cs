using System;
using System.Drawing;

namespace PictureConsoler.PCX
{
    class ColorValuesOrderer
    {
        private readonly Color[] prevCVs, thisCVs;
        private int[,] d2Matrix;// [prevCVs, thisCVs]
        private int[] rowDec, columnDec, mins;
        private byte[] markedIs, links;
        private bool[] visited;

        public ColorValuesOrderer(Color[] prevColorValues, Color[] thisColorValues)
        {
            if (prevColorValues.Length != thisColorValues.Length) throw new ArgumentException();
            prevCVs = prevColorValues;
            thisCVs = thisColorValues;
            Init();
        }
        private void Init()
        {
            d2Matrix = new int[prevCVs.Length, thisCVs.Length];
            for (byte i = 0; i < prevCVs.Length; i++)
                for (byte j = 0; j < thisCVs.Length; j++)
                    d2Matrix[i, j] = prevCVs[i].CalcDistance2(thisCVs[j]);
            rowDec = new int[prevCVs.Length];
            columnDec = new int[thisCVs.Length];
            markedIs = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Repeat((byte)255, thisCVs.Length));
            links = new byte[thisCVs.Length];
            mins = new int[thisCVs.Length];
            visited = new bool[thisCVs.Length];
        }

        public Color[] Order()
        {
            for (byte i = 0; i < prevCVs.Length; i++) ProcessRow(i);
            return FormOrdered();
        }
        private void ProcessRow(byte i)
        {
            byte markedI = i, markedJ = 255, j;
            for (j = 0; j < thisCVs.Length; j++)
            {
                links[j] = 255;
                mins[j] = int.MaxValue;
                visited[j] = false;
            }
            //
            while (markedI != 255)
            {
                j = 255;
                for (byte j1 = 0; j1 < thisCVs.Length; j1++)
                {
                    if (visited[j1]) continue;
                    int dist2 = d2Matrix[markedI, j1] - rowDec[markedI] - columnDec[j1];
                    if (dist2 < mins[j1])
                    {
                        mins[j1] = dist2;
                        links[j1] = markedJ;
                    }
                    if ((j == 255) || (mins[j1] < mins[j])) j = j1;
                }
                //
                int delta = mins[j];
                for (byte j1 = 0; j1 < thisCVs.Length; j1++)
                {
                    if (visited[j1])
                    {
                        rowDec[markedIs[j1]] += delta;
                        columnDec[j1] -= delta;
                    }
                    else mins[j1] -= delta;
                }
                rowDec[i] += delta;
                //
                visited[j] = true;
                markedJ = j;
                markedI = markedIs[j];
            }
            //
            while (links[j] != 255)
            {
                byte nextJ = links[j];
                markedIs[j] = markedIs[nextJ];
                j = nextJ;
            }
            markedIs[j] = i;
        }
        private Color[] FormOrdered()
        {
            Color[] ordered = new Color[thisCVs.Length];
            for (byte j = 0; j < thisCVs.Length; j++)
                ordered[markedIs[j]] = thisCVs[j];
            return ordered;
        }
    }
}
