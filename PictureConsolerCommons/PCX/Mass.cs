using System;
using System.Drawing;

namespace PictureConsoler.Commons.PCX
{
	public static class Mass
	{
		public static Color ModifyMassColor(Color oldMC, byte j)
		{
			switch (j)
			{
				case 0:
					if (oldMC.R == 0)
						return Color.Empty;
					return Color.FromArgb(oldMC.R - 1, oldMC.G, oldMC.B);
				case 1:
					if (oldMC.R == 255)
						return Color.Empty;
					return Color.FromArgb(oldMC.R + 1, oldMC.G, oldMC.B);
				case 2:
					if (oldMC.G == 0)
						return Color.Empty;
					return Color.FromArgb(oldMC.R, oldMC.G - 1, oldMC.B);
				case 3:
					if (oldMC.G == 255)
						return Color.Empty;
					return Color.FromArgb(oldMC.R, oldMC.G + 1, oldMC.B);
				case 4:
					if (oldMC.B == 0)
						return Color.Empty;
					return Color.FromArgb(oldMC.R, oldMC.G, oldMC.B - 1);
				case 5:
					if (oldMC.B == 255)
						return Color.Empty;
					return Color.FromArgb(oldMC.R, oldMC.G, oldMC.B + 1);
			}
			throw new Exception();
		}
	}
}
