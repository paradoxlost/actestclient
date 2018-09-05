using System;
//using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using TestClient.Crypto;

namespace TestClient.Net.Packets
{
	public class Packet
	{
		public TransitHeader Header;

		// server connect info ( switch / referral )
		// retransmit info
		// reject info
		public uint SequenceAck { get; private set; }
		// login (custom packet)
		// world login (token?)
		// connect response (login handshake p.3)
		public long ConnectToken { get; private set; }
		// command
		public long TimeStamp { get; private set; }

		public uint EchoRequest { get; private set; }
		public uint EchoResponse0 { get; private set; }
		public uint EchoResponse1 { get; private set; }

		public uint Flow0 { get; private set; }
		public ushort Flow1 { get; private set; }

		public int OptionSize { get; private set; }

		public Fragment Fragment { get; private set; }

		public Packet()
		{
		}

		public Packet(PacketFlags flags)
			: this()
		{
			Header.Flags = flags;
		}

		public void AddFragment(Fragment frag)
		{
			Fragment = frag;
			Header.Flags |= PacketFlags.Fragmented;
		}

		public void SetAck(uint sequence)
		{
			SequenceAck = sequence;
			OptionSize += 4;
			Header.Flags |= PacketFlags.AckSequence;
		}

		public void SetToken(long token, bool world = false)
		{
			ConnectToken = token;
			OptionSize += 8;
			Header.Flags |= world ? PacketFlags.LoginWorld : PacketFlags.ConnectResponse;
		}

		public void SetTimeStamp(long timeStamp)
		{
			TimeStamp = timeStamp;
			OptionSize += 8;
			Header.Flags |= PacketFlags.TimeSync;
		}

		public uint Hash(uint seed, Span<byte> buffer)
		{
			Span<TransitHeader> pHeader = MemoryMarshal.Cast<byte, TransitHeader>(buffer);
			ref TransitHeader header = ref MemoryMarshal.GetReference(pHeader);

			uint orig = header.Checksum;
			uint result = 0;

			if (Header.Size > 0)
			{
				if (OptionSize > 0)
					result += Checksum.GetMagicNumber(
						buffer.Slice(TransitHeader.SizeOf, OptionSize), OptionSize, true);

				int remaining = Header.Size - OptionSize;
				int position = TransitHeader.SizeOf + OptionSize;
				if (remaining > 0)
				{
					if (Fragment != null)
					{
						uint frag = Fragment.Hash(buffer.Slice(position));
						result += frag;
					}
					else
					{
						result += Checksum.GetMagicNumber(
							buffer.Slice(position, remaining), remaining, true);
					}
				}

				if (header.Flags.HasFlag(PacketFlags.Checksum))
					result ^= seed;
			}

			header.Checksum = 0xbadd70dd;
			result += Checksum.GetMagicNumber(buffer, TransitHeader.SizeOf, true);

			header.Checksum = orig;
			return result;
		}

		public int Serialize(Stream stream)
		{
			stream.Seek(TransitHeader.SizeOf, SeekOrigin.Begin);

			// write extended data to stream
			using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true))
			{
				if (OptionSize > 0)
				{
					// resend
					// reject

					if (Header.Flags.HasFlag(PacketFlags.AckSequence))
					{
						writer.Write(SequenceAck);
					}

					if (Header.Flags.HasFlag(PacketFlags.LoginWorld))
					{
						writer.Write(ConnectToken);
					}

					if (Header.Flags.HasFlag(PacketFlags.ConnectResponse))
					{
						writer.Write(ConnectToken);
					}

					// command

					if (Header.Flags.HasFlag(PacketFlags.TimeSync))
					{
						writer.Write(TimeStamp);
					}

					if (Header.Flags.HasFlag(PacketFlags.EchoRequest))
					{
						writer.Write(EchoRequest);
					}

					if (Header.Flags.HasFlag(PacketFlags.EchoResponse))
					{
						writer.Write(EchoResponse0);
						writer.Write(EchoResponse1);
					}

					if (Header.Flags.HasFlag(PacketFlags.FlowUpdate))
					{
						writer.Write(Flow0);
						writer.Write(Flow1);
					}
				}

				OnSerialize(writer);

				if (Fragment != null)
					Fragment.Serialize(stream);
			}

			int resultLength = (int)stream.Position;

			Header.Size = (ushort)(resultLength - TransitHeader.SizeOf);
			stream.Seek(0, SeekOrigin.Begin);

			// write header to stream
			ReadOnlySpan<TransitHeader> pHeader = MemoryMarshal.CreateReadOnlySpan(ref Header, 1);
			ReadOnlySpan<byte> pBytes = MemoryMarshal.AsBytes(pHeader);
			stream.Write(pBytes);

			return resultLength;
		}

		protected virtual void OnSerialize(BinaryWriter writer)
		{
		}

	}

	public class TimeSyncPacket : Packet
	{
		public TimeSyncPacket(long timeStamp)
			: base()
			{
				SetTimeStamp(timeStamp);
			}
	}

	// public class Packet<T> : Packet where T : struct
	// {
	// 	T field;
	// 	public Packet(PacketFlags flags, T field)
	// 		: base(flags)
	// 	{
	// 		this.field = field;
	// 	}

	// 	protected override void OnSerialize(BinaryWriter writer)
	// 	{
	// 		Type tt = typeof(T);
	// 		switch (Type.GetTypeCode(tt))
	// 		{
	// 			case TypeCode.Int16:
	// 				writer.Write((short)Convert.ChangeType(field, tt));
	// 				break;

	// 			case TypeCode.Int32:
	// 				writer.Write((int)Convert.ChangeType(field, tt));
	// 				break;

	// 			case TypeCode.Int64:
	// 				writer.Write((long)Convert.ChangeType(field, tt));
	// 				break;
	// 		}
	// 	}
	// }
}
