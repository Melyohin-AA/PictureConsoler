using System;
using System.Drawing;
using System.IO;

namespace PictureConsoler.Commons.PCX
{
	public sealed class MassLogger
	{
		public const string mark = "PCLM";
		public const byte version = 1;
		public const byte batchSize = 16;// MassColorsDeterminor.massColorCount;

		private readonly string dest;
		private long frameCountAddress;
		private ushort frameCount;
		private FileStream stream;
		private BinaryWriter writer;
		private long batchCountAddress;
		private ulong batchCount;

		public MassLogger(string dest)
		{
			this.dest = dest ?? throw new ArgumentNullException(nameof(dest));
		}

		public void StartFrame(bool useReducedColors, bool ignoreColorCount, ulong minDelta, Color[] massColors)
		{
			if (massColors.Length != batchSize)
				throw new ArgumentOutOfRangeException(nameof(massColors));
			if (stream != null)
				FinishFrame();
			else Init();
			WriteFlags(useReducedColors, ignoreColorCount);
			writer.Write(minDelta);
			WriteMassColors(massColors);
			batchCountAddress = stream.Position;
			writer.Write(0L); // space for batch count
			frameCount++;
		}
		private void Init()
		{
			stream = new FileStream(dest, FileMode.OpenOrCreate);
			stream.SetLength(0L);
			writer = new BinaryWriter(stream);
			writer.Write(mark.ToCharArray());
			writer.Write(version);
			frameCountAddress = stream.Position;
			writer.Write((ushort)0); // space for frame count
		}
		private void FinishFrame()
		{
			long pos = stream.Position;
			stream.Position = batchCountAddress;
			writer.Write(batchCount);
			stream.Position = pos;
			batchCount = 0L;
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
				throw new InvalidOperationException("Stream not initiallized");
			FinishFrame();
			stream.Position = frameCountAddress;
			writer.Write(frameCount);
			writer.Close();
			writer = null;
			stream.Close();
			stream = null;
		}
	}
}
