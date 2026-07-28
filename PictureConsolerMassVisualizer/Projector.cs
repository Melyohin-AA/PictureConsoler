using System;
using System.Drawing;

namespace PictureConsoler.MassVisualizer
{
	internal class Projector
	{
		public Bitmap Bitmap { get; }

		public Projector()
		{
			Bitmap = new Bitmap(512, 512);
			//var g = Graphics.FromImage(Bitmap);
			//g.Clear(Color.Black);
			//g.FillRectangle(Brushes.White, 256, 256, 256, 256);
			//g.DrawLine(Pens.Red, 0, 0, 255, 0);
			//g.DrawLine(Pens.Green, 0, 0, 0, 255);
			//g.DrawLine(Pens.Blue, 256, 0, 511, 0);
			//g.DrawLine(Pens.Green, 256, 0, 256, 255);
			//g.DrawLine(Pens.Red, 0, 256, 255, 256);
			//g.DrawLine(Pens.Blue, 0, 256, 0, 511);
			//g.Flush();
			for (int x = 0; x < 256; x++)
			{
				for (int y = 0; y < 256; y++)
				{
					int brightness = (300 - x - y) / 2;
					if (brightness < 0)
						brightness = 0;
					Color px = Color.FromArgb(brightness, brightness, brightness);
					Bitmap.SetPixel(x, y, px);
					Bitmap.SetPixel(x + 256, y, px);
					Bitmap.SetPixel(x, y + 256, px);
					Bitmap.SetPixel(x + 256, y + 256, px);
				}
			}
		}

		public void ProjectPixel(byte x, byte y, byte z, Color color)
		{
			//color = Color.FromArgb(64, color);
			Bitmap.SetPixel(x, y, color);
			Bitmap.SetPixel(x, z + 256, color);
			Bitmap.SetPixel(256 + z, y, color);
		}

		public void ProjectCircle(byte x, byte y, byte z, Color color)
		{
			//color = Color.FromArgb(128, color);
			var b = new SolidBrush(color);
			var g = Graphics.FromImage(Bitmap);
			g.FillEllipse(b, x - 2, y - 2, 5, 5);
			g.FillEllipse(b, x - 2, z + 254, 5, 5);
			g.FillEllipse(b, z + 254, y - 2, 5, 5);
			g.Flush();
		}
	}
}
