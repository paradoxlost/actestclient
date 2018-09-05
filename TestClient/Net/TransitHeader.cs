using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TestClient.Net
{
	// 4 byes were trimmed out of this
	[StructLayout(LayoutKind.Sequential)]
	public struct TransitHeader
	{
		public uint Sequence { get; set; }
		public Packets.PacketFlags Flags { get; set; }
		public uint Checksum { get; set; }
		public ushort Id { get; set; }
		public ushort Time { get; set; }
		public ushort Size { get; set; }
		public ushort Table { get; set; }

		public static readonly int SizeOf = Marshal.SizeOf<TransitHeader>();
	}
}
