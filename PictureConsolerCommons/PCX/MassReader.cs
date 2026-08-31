using System;
using System.Drawing;
using System.IO;

namespace PictureConsoler.Commons.PCX
{
	public sealed class MassReader
	{
		private readonly FileStream stream;
		private readonly BinaryReader reader;

		public ushort FrameCount { get; private set; }
		public ulong BatchCount { get; private set; }
		public bool UseReducedColors { get; private set; }
		public bool IgnoreColorCount { get; private set; }

		public ulong MinDelta { get; private set; }
		public Color[] MassColors { get; private set; } = new Color[MassLogger.batchSize];
		public ushort FrameIndex { get; private set; }
		public ulong BatchIndex { get; private set; }

		public MassReader(string source)
		{
			stream = new FileStream(source, FileMode.Open);
			reader = new BinaryReader(stream);
			if (new string(reader.ReadChars(4)) != MassLogger.mark)
				throw new IOException($"'{MassLogger.mark}' mark missing");
			byte version = reader.ReadByte();
			if (version != MassLogger.version)
				throw new IOException($"Version {version} unsupported");
			FrameCount = reader.ReadUInt16();
		}

		public void NextFrame()
		{
			if (BatchIndex < BatchCount)
				throw new IndexOutOfRangeException($"{BatchCount - BatchIndex} batches unread");
			if (FrameIndex >= FrameCount)
				throw new IndexOutOfRangeException("The last frame is read");
			ReadFlags();
			MinDelta = reader.ReadUInt64();
			ReadMassColors();
			BatchCount = reader.ReadUInt64();
			BatchIndex = 0L;
			FrameIndex++;
		}
		private void ReadFlags()
		{
			byte flags = reader.ReadByte();
			UseReducedColors = (flags & 0b10) != 0;
			IgnoreColorCount = (flags & 0b01) != 0;
		}
		private void ReadMassColors()
		{
			for (byte i = 0; i < MassLogger.batchSize; i++)
				MassColors[i] = Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
		}

		public void NextBatch()
		{
			if (BatchIndex >= BatchCount)
				throw new IndexOutOfRangeException("The last batch is read");
			MinDelta = reader.ReadUInt64();
			byte[] dirs = new byte[MassLogger.batchSize];
			for (byte i = 0; i < MassLogger.batchSize / 2; i++)
			{
				byte buffer = reader.ReadByte();
				dirs[i * 2] = (byte)(buffer >> 4);
				dirs[i * 2 + 1] = (byte)(buffer & 0x0F);
			}
			for (int i = 0; i < MassLogger.batchSize; i++)
			{
				if (dirs[i] == 6) continue;
				MassColors[i] = Mass.ModifyMassColor(MassColors[i], dirs[i]);
				if (MassColors[i].IsEmpty)
					throw new IndexOutOfRangeException();
			}
			BatchIndex++;
		}
	}
}
