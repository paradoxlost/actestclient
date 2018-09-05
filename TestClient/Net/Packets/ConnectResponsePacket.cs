using System;
using System.IO;

namespace TestClient.Net.Packets
{
	public class ConnectResponsePacket : Packet
	{
		public long Cookie { get; set; }

		public ConnectResponsePacket(long cookie)
			: base (PacketFlags.ConnectResponse)
		{
			Cookie = cookie;
		}

		protected override void OnSerialize(BinaryWriter writer)
		{
			writer.Write(Cookie);
		}
	}
}
