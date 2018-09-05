using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TestClient.Net
{
	public enum FragmentGroup
	{
		Event = 5,
		Private = 9,
		Object = 10
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FragmentHeader
	{
		public uint Sequence { get; set; }
		public uint Id { get; set; }
		public ushort Count { get; set; }
		public ushort Size { get; set; }
		public ushort Index { get; set; }
		public ushort Group { get; set; }

		public static readonly int SizeOf = Marshal.SizeOf<FragmentHeader>();
		public const int MaxChunk = 448;
	}
}
