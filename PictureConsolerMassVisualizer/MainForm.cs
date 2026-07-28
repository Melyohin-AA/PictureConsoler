using System;
using System.Drawing;
using System.Windows.Forms;

namespace PictureConsoler.MassVisualizer
{
	public partial class MainForm : Form
	{
		private const string CaptionBase = "PictureConsoler::MassVisualizer";

		//private const byte xa = 255, xb = 128, xc = 128, xd = 64;
		//private static readonly Color[] colors = new Color[] {
		//	Color.FromArgb(xa, 0, 0), Color.FromArgb(xa, xa, 0), Color.FromArgb(0, xa, 0),
		//	Color.FromArgb(0, xa, xa), Color.FromArgb(0, 0, xa), Color.FromArgb(xa, 0, xa),
		//	Color.FromArgb(xb, 0, 0), Color.FromArgb(xb, xb, 0), Color.FromArgb(0, xb, 0),
		//	Color.FromArgb(0, xb, xb), Color.FromArgb(0, 0, xb), Color.FromArgb(xb, 0, xb),
		//	Color.FromArgb(xa, xc, 0), Color.FromArgb(xa, xa, xa),
		//	Color.FromArgb(xb, xd, 0), Color.FromArgb(xb, xb, xb),
		//};

		private readonly Projector projector = new Projector();
		private readonly Commons.PCX.MassReader massReader;

		public MainForm()
		{
			InitializeComponent();
			var dialog = new OpenFileDialog { Filter = "PC Mass Logs (*.pcml)|*.pcml" };
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				Close();
				return;
			}
			massReader = new Commons.PCX.MassReader(dialog.FileName);
			ShowInfo();
			DrawMassColors(true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);
			e.Graphics.DrawImage(projector.Bitmap, 0, 0);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (massReader.Index < massReader.BatchCount)
			{
				massReader.Next();
				ShowInfo();
				DrawMassColors(massReader.Index < massReader.BatchCount - 1);
				Invalidate();
			}
		}

		private void DrawMassColors(bool asPixels)
		{
			for (int i = 0; i < massReader.BatchSize; i++)
			{
				Color mc = massReader.MassColors[i];
				if (asPixels)
					projector.ProjectPixel(mc.R, mc.G, mc.B, mc);//colors[i]
				else projector.ProjectCircle(mc.R, mc.G, mc.B, mc);//colors[i]
			}
		}

		private void ShowInfo()
		{
			Text = $"{CaptionBase} index={massReader.Index}/{massReader.BatchCount} delta={massReader.MinDelta}";
		}
	}
}
