using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient.Net.Packets
{
	[Flags]
	public enum PacketFlags
	{
		Default			= 0x00000000,
		Resent			= 0x00000001,
		Checksum		= 0x00000002,
		Fragmented		= 0x00000004,
		SwitchServer	= 0x00000100,
		LogonServerAddr = 0x00000200,
		EmptyHeader		= 0x00000400,
		Referral		= 0x00000800,
		Resend			= 0x00001000,
		RejectResend	= 0x00002000,
		AckSequence		= 0x00004000,
		Disconnect		= 0x00008000,
		LoginRequest	= 0x00010000,
		LoginWorld		= 0x00020000,
		ConnectRequest  = 0x00040000,
		ConnectResponse = 0x00080000,
		NetError		= 0x00100000,
		NetDisconnect   = 0x00200000,
		Command			= 0x00400000,
		TimeSync		= 0x01000000,
		EchoRequest		= 0x02000000,
		EchoResponse	= 0x04000000,
		FlowUpdate		= 0x08000000
	}
}
