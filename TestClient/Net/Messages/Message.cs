using System;
using System.Buffers;

using TestClient.Net.Packets;

namespace TestClient.Net.Messages
{
	public class MessageEventArgs : EventArgs
	{
		public MessageDirection Direction { get; private set; }
		public Message Message { get; private set; }

		public MessageEventArgs(Message msg, MessageDirection dir)
		{
			Direction = dir;
			Message = msg;
		}
	}

	public class Message : MessageStruct, IDisposable
	{
		private FragmentedPacket packet;
		public uint Type { get; set; }

		#region Ctor/Dispose

		protected Message()
			: base()
		{
		}

		public Message(uint type, FragmentedPacket packet, MemberParser parser)
			: base(packet.Buffer.Memory.Slice(4), 0, parser)
		{
			Type = type;
			this.packet = packet;
			//Parse();
		}

		~Message()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				packet.Dispose();
			}
		}

		#endregion
	}
}
