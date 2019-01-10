using System;
using System.IO;

namespace TestClient.Net.Packets
{
	public class PingEventFragment : EventFragment
	{
		public PingEventFragment()
			: base(0x01e9)
		{
		}
	}
}
