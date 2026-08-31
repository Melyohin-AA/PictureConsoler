using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;
using System.IO;

namespace PictureConsoler
{
	class GifBuilder
	{
		private static readonly byte[] netscapeLoopingAppExtBlock = new byte[] {
			33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0,
		};

		public GifBitmapEncoder Encoder { get; private set; } = new GifBitmapEncoder();

		public void AddFrame(Bitmap frame)
		{
			IntPtr hbmp = frame.GetHbitmap();
			BitmapSource bmpSource = Imaging.CreateBitmapSourceFromHBitmap(
				hbmp, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
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
			using (var output = new FileStream(filepath, FileMode.OpenOrCreate))
			{
				output.SetLength(0L); // removing existing file content
				output.Write(fileBytes, 0, 13);
				output.Write(netscapeLoopingAppExtBlock, 0, netscapeLoopingAppExtBlock.Length);
				output.Write(fileBytes, 13, fileBytes.Length - 13);
			}
		}

		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);
	}
}
