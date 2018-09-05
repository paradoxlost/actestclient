using System;
using System.Runtime.InteropServices;

namespace TestClient.Crypto
{
	internal static class Checksum
	{
		public static uint GetMagicNumber<T>(ref T packet, int size, bool includeSize) where T : struct
		{
			Span<T> packetSpan = MemoryMarshal.CreateSpan(ref packet, 1);
			Span<byte> bytes = MemoryMarshal.AsBytes(packetSpan);
			return GetMagicNumber(bytes, size, includeSize);
		}

		public static uint GetMagicNumber(in Span<byte> pBuffer, int size, bool includeSize)
		{
			uint magic = 0;

			if (includeSize)
				magic += (uint)size << 16;

			int i = 0;

			//uint* pInts = (uint*)pBuffer;
			Span<uint> pInts = MemoryMarshal.Cast<byte, uint>(pBuffer);
			for (i = 0; i < size / 4; i++)
				magic += pInts[i];

			int shift = 3;
			for (i = (i * 4); i < size; i++)
			{
				magic += (uint)pBuffer[i] << (shift * 8);
				shift--;
			}

			return magic;
		}

		public static unsafe uint Calculate(byte[] buffer)
		{
			uint checksum1 = 0;
			uint checksum2 = 0;

			Span<byte> pBuffer = buffer;
			Span<Net.TransitHeader> pHeader = MemoryMarshal.Cast<byte, Net.TransitHeader>(pBuffer);
			ref Net.TransitHeader header = ref MemoryMarshal.GetReference(pHeader);

			header.Checksum = 0xbadd70dd;
			checksum1 = GetMagicNumber(pBuffer, Net.TransitHeader.SizeOf, true);
			checksum2 = GetMagicNumber(pBuffer.Slice(Net.TransitHeader.SizeOf), header.Size, true);
			header.Checksum = checksum1 + checksum2;

			return checksum1 + checksum2;
		}

		public static unsafe uint Calculate200(byte[] buffer)
		{
			uint checksum = 0;
			Span<byte> pBuffer = buffer;
			Span<Net.TransitHeader> pHeader = MemoryMarshal.Cast<byte, Net.TransitHeader>(pBuffer);
			ref Net.TransitHeader header = ref MemoryMarshal.GetReference(pHeader);

			int pos = Net.TransitHeader.SizeOf;
			int end = Net.TransitHeader.SizeOf + header.Size;

			// TODO: optional header data. fragments may not start here

			while (pos < end)
			{
				Span<byte> pFrag = pBuffer.Slice(pos, Net.FragmentHeader.SizeOf);
				Span<Net.FragmentHeader> pFragHeader = MemoryMarshal.Cast<byte, Net.FragmentHeader>(pFrag);
				ref Net.FragmentHeader fragHeader = ref MemoryMarshal.GetReference(pFragHeader);

				int len = fragHeader.Size;
				checksum += GetMagicNumber(pFrag, Net.FragmentHeader.SizeOf, true);
				checksum += GetMagicNumber(pBuffer.Slice(pos + Net.FragmentHeader.SizeOf), len - Net.FragmentHeader.SizeOf, true);

				pos += len;
			}

			// fixed (byte* pBuffer = buffer)
			// {
			// 	byte* pEnd = pBuffer + ((Net.TransitHeader*)pBuffer)->Size + HeaderSize;

			// 	for (byte* pFrag = pBuffer + HeaderSize; pFrag < pEnd; )
			// 	{
			// 		int len = ((Net.FragmentHeader*)pFrag)->Size;
			// 		checksum += GetMagicNumber(pFrag, FragmentHeaderSize, true);
			// 		checksum += GetMagicNumber(pFrag + FragmentHeaderSize, len - FragmentHeaderSize, true);
			// 		pFrag += len;
			// 	}
			// }

			return checksum;
		}

		public static uint CalculateTransport(byte[] buffer)
		{
			uint checksum = 0;
			Span<byte> pBuffer = buffer;
			Span<Net.TransitHeader> headerSpan = MemoryMarshal.Cast<byte, Net.TransitHeader>(pBuffer);
			ref Net.TransitHeader header = ref MemoryMarshal.GetReference(headerSpan);

			uint orig = header.Checksum;
			header.Checksum = 0xbadd70dd;
			checksum += GetMagicNumber(pBuffer, Net.TransitHeader.SizeOf, true);
			header.Checksum = orig;

			return checksum;
		}
	}
}
