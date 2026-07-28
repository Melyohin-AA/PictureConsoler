using System;
using System.Drawing;
using System.IO;

namespace PictureConsoler.Commons.PCX
{
	public sealed class MassReader
	{
		private readonly FileStream stream;
		private readonly BinaryReader reader;

		public byte BatchSize { get; private set; }
		public ulong BatchCount { get; private set; }
		public bool UseReducedColors { get; private set; }
		public bool IgnoreColorCount { get; private set; }

		public ulong MinDelta { get; private set; }
		public Color[] MassColors { get; private set; }
		public ulong Index { get; private set; }

		public MassReader(string source)
		{
			stream = new FileStream(source, FileMode.Open);
			reader = new BinaryReader(stream);
			if (new string(reader.ReadChars(4)) != MassLogger.mark)
				throw new InvalidDataException();
			ReadFlags();
			BatchSize = reader.ReadByte();
			MassColors = new Color[BatchSize];
			MinDelta = reader.ReadUInt64();
			ReadMassColors();
			BatchCount = reader.ReadUInt64();
		}
		private void ReadFlags()
		{
			byte flags = reader.ReadByte();
			UseReducedColors = (flags & 0b10) != 0;
			IgnoreColorCount = (flags & 0b01) != 0;
		}
		private void ReadMassColors()
		{
			for (byte i = 0; i < BatchSize; i++)
				MassColors[i] = Color.FromArgb(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
		}

		public void Next()
		{
			MinDelta = reader.ReadUInt64();
			byte[] dirs = new byte[BatchSize];
			for (byte i = 0; i < BatchSize / 2; i++)
			{
				byte buffer = reader.ReadByte();
				dirs[i * 2] = (byte)(buffer >> 4);
				dirs[i * 2 + 1] = (byte)(buffer & 0x0F);
			}
			for (int i = 0; i < BatchSize; i++)
			{
				if (dirs[i] == 6) continue;
				MassColors[i] = Mass.ModifyMassColor(MassColors[i], dirs[i]);
				if (MassColors[i].IsEmpty)
					throw new IndexOutOfRangeException();
			}
			Index++;
		}
	}
}
