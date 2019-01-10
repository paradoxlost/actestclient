using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient.Net
{
	[Flags]
	public enum ServerFlags
	{
		None = 0,
		Connecting = 1,
		Connected = 2,
		ChecksumSeeds = 4,
		Sync = 8,
		Wakeup = 0x00080000
	}
}
