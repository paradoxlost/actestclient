using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient.Net
{
	[Flags]
	public enum ServerFlags
	{
		Awake = 0,
		Shook = 1,
		ChecksumSeeds = 2,
		Sync = 4,
		Wakeup = 0x00080000
	}
}
