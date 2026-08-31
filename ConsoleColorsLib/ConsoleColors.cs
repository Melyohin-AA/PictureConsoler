using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ConsoleColorsLib
{
	public static class ConsoleColors
	{
		public static Color[] GetClassicColorValues()
		{
			const byte a = 128, b = 192, c = 255;
			var colorValues = new Color[16];
			for (byte i = 0; i < 7; i++)
				colorValues[i] = GetColorBin(i, a);
			colorValues[7] = Color.FromArgb(b, b, b);
			colorValues[8] = Color.FromArgb(a, a, a);
			for (byte i = 1; i < 8; i++)
				colorValues[i + 8] = GetColorBin(i, c);
			return colorValues;
		}
		private static Color GetColorBin(byte bin, int one)
		{
			int r = ((bin & 4) != 0) ? one : 0;
			int g = ((bin & 2) != 0) ? one : 0;
			int b = ((bin & 1) != 0) ? one : 0;
			return Color.FromArgb(r, g, b);
		}

		public static void Recolor(Color[] colorValues)
		{
			const int stdOutputHandle = -11;
			if (colorValues.Length != 16)
				throw new ArgumentOutOfRangeException(nameof(colorValues));
			IntPtr hConsoleOutput = GetStdHandle(stdOutputHandle);
			ConsoleScreeenBufferInfoEx csbe = GetBufferInfo(hConsoleOutput);
			for (byte i = 0; i < 16; i++)
				csbe.colors[i] = ColorRef(colorValues[i]);
			SetBufferInfo(hConsoleOutput, csbe);
		}

		private static int ColorRef(Color color)
		{
			return (color.B << 16) | (color.G << 8) | color.R;
		}

		private static ConsoleScreeenBufferInfoEx GetBufferInfo(IntPtr hConsoleOutput)
		{
			var csbe = new ConsoleScreeenBufferInfoEx();
			csbe.cbSize = Marshal.SizeOf(csbe);
			if ((int)hConsoleOutput == -1)
				throw new Win32Exception(Marshal.GetLastWin32Error());
			if (!GetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe))
				throw new Win32Exception(Marshal.GetLastWin32Error());
			return csbe;
		}

		private static void SetBufferInfo(IntPtr hConsoleOutput, ConsoleScreeenBufferInfoEx csbe)
		{
			csbe.window.bottom++;
			csbe.window.right++;
			if (!SetConsoleScreenBufferInfoEx(hConsoleOutput, ref csbe))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool GetConsoleScreenBufferInfoEx(
			IntPtr hConsoleOutput, ref ConsoleScreeenBufferInfoEx csbe);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetConsoleScreenBufferInfoEx(
			IntPtr hConsoleOutput, ref ConsoleScreeenBufferInfoEx csbe);

		[StructLayout(LayoutKind.Sequential)]
		private struct ConsoleScreeenBufferInfoEx
		{
			public int cbSize;
			public Coord size;
			public Coord cursorPosition;
			public ushort attributes;
			public SmallRect window;
			public Coord maxWindowSize;
			public ushort popupAttributes;
			public bool fullscreenSupported;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public int[] colors;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct Coord
		{
			public short x, y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SmallRect
		{
			public short left, top, right, bottom;
		}
	}
}
