using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

using TestClient.Crypto;

namespace TestClient.Net.Packets
{
	public class Fragment
	{
		public FragmentHeader Header;

		public uint Type { get; set; }

		public Fragment()
		{
		}

		public Fragment(uint type)
			: this()
		{
			Type = type;
		}

		public Fragment(uint type, ushort group)
			: this(type)
		{
			Header.Group = group;
		}

		public Fragment(uint type, FragmentGroup group)
			: this(type, (ushort)group)
		{
		}

		public uint Hash(Span<byte> buffer)
		{
			Span<FragmentHeader> pHeader = MemoryMarshal.Cast<byte, FragmentHeader>(buffer);
			ref FragmentHeader header = ref MemoryMarshal.GetReference(pHeader);

			int len = header.Size - FragmentHeader.SizeOf;

			uint result = Checksum.GetMagicNumber(buffer, FragmentHeader.SizeOf, true);
			result += Checksum.GetMagicNumber(
				buffer.Slice(FragmentHeader.SizeOf, len), len, true);

			return result;
		}

		public int Serialize(Stream stream)
		{
			int start = (int)stream.Position;

			stream.Seek(FragmentHeader.SizeOf, SeekOrigin.Current);

			// write extended data to stream
			using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, true))
			{
				writer.Write(Type);

				OnSerialize(writer);
			}

			int resultLength = (int)stream.Position - start;

			Header.Size = (ushort)(resultLength);
			stream.Seek(start, SeekOrigin.Begin);

			// write header to stream
			ReadOnlySpan<FragmentHeader> pHeader = MemoryMarshal.CreateReadOnlySpan(ref Header, 1);
			ReadOnlySpan<byte> pBytes = MemoryMarshal.AsBytes(pHeader);
			stream.Write(pBytes);

			stream.Seek(resultLength - FragmentHeader.SizeOf, SeekOrigin.Current);

			return resultLength;
		}

		protected virtual void OnSerialize(BinaryWriter writer)
		{
		}

	}
}
