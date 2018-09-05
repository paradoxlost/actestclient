using System;
using System.IO;

namespace TestClient.Net.Packets
{
	public static class PacketWriter
	{
		public static void WriteString16(this BinaryWriter writer, string value)
		{
			writer.Write((short)value.Length);
			writer.Write(value.ToCharArray());
			int pad = (value.Length + 2) & 3;
			if (pad > 0)
				writer.Seek(4 - pad, SeekOrigin.Current);
		}
	}
}
