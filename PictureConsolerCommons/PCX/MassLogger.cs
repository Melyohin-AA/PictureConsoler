using System;
using System.Drawing;
using System.IO;

namespace PictureConsoler.Commons.PCX
{
	public sealed class MassLogger
	{
		public const string mark = "PCML";
		public const byte batchSize = 16;// MassColorsDeterminor.massColorCount;

		private readonly string dest;
		private FileStream stream;
		private BinaryWriter writer;
		private long batchCountAddress;
		private ulong batchCount;

		public MassLogger(string dest)
		{
			this.dest = dest ?? throw new ArgumentNullException(nameof(dest));
		}

		public void Start(bool useReducedColors, bool ignoreColorCount, ulong minDelta, Color[] massColors)
		{
			if (stream != null)
				throw new InvalidOperationException();
			if (massColors.Length != batchSize)
				throw new ArgumentOutOfRangeException(nameof(massColors));
			stream = new FileStream(dest, FileMode.OpenOrCreate);
			stream.SetLength(0L);
			writer = new BinaryWriter(stream);
			writer.Write(mark.ToCharArray());
			WriteFlags(useReducedColors, ignoreColorCount);
			writer.Write(batchSize);
			writer.Write(minDelta);
			WriteMassColors(massColors);
			batchCountAddress = stream.Position;
			writer.Write(0L); // batch count
		}
		private void WriteFlags(bool useReducedColors, bool ignoreColorCount)
		{
			byte flags = 0;
			if (useReducedColors)
				flags |= 0b10;
			if (ignoreColorCount)
				flags |= 0b01;
			writer.Write(flags);
		}
		private void WriteMassColors(Color[] massColors)
		{
			for (byte i = 0; i < batchSize; i++)
			{
				Color mc = massColors[i];
				writer.Write(mc.R);
				writer.Write(mc.G);
				writer.Write(mc.B);
			}
		}

		public void Log(ulong minDelta, byte[] dirs)
		{
			if (dirs.Length != batchSize)
				throw new ArgumentOutOfRangeException(nameof(dirs));
			writer.Write(minDelta);
			byte buffer = 0;
			for (byte i = 0; i < batchSize; i++)
			{
				if (i % 2 == 0)
					buffer = dirs[i];
				else
				{
					buffer = (byte)((buffer << 4) | dirs[i]);
					writer.Write(buffer);
				}
			}
			batchCount++;
		}

		public void Stop()
		{
			if (stream == null)
				throw new InvalidOperationException();
			stream.Position = batchCountAddress;
			writer.Write(batchCount);
			writer.Close();
			writer = null;
			stream.Close();
			stream = null;
		}
	}
}
