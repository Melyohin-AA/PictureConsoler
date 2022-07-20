using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PictureConsoler
{
    class GIF_Builder
    {
        public GifBitmapEncoder Encoder { get; private set; } = new GifBitmapEncoder();

        public void AddFrame(Bitmap frame)
        {
            IntPtr hbmp = frame.GetHbitmap();
            BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            BitmapFrame bmpFrame = BitmapFrame.Create(bmpSource);
            Encoder.Frames.Add(bmpFrame);
            DeleteObject(hbmp);
        }

        public void Save(string filepath)
        {
            MemoryStream stream = new MemoryStream();
            Encoder.Save(stream);
            byte[] fileBytes = stream.ToArray();
            stream.Close();
            // fix of gif's header
            byte[] applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };
            List<byte> newBytes = new List<byte>();
            newBytes.AddRange(fileBytes.Take(13));
            newBytes.AddRange(applicationExtension);
            newBytes.AddRange(fileBytes.Skip(13));
            File.WriteAllBytes(filepath, newBytes.ToArray());
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}