using System;
using System.IO;

namespace TestClient.Net.Packets
{
	public class EventFragment : Fragment
	{
		public uint Sequence { get; set; }
		public uint Action { get; set; }

		public EventFragment(uint action)
			: base(0xf7b1, 10)
		{
			Action = action;
		}

		protected override void OnSerialize(BinaryWriter writer)
		{
			writer.Write(Sequence);
			writer.Write(Action);
		}
	}
}
