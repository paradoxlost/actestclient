using System;
using System.Buffers;
using System.Collections.Concurrent;

namespace TestClient.Net.Packets
{
	public class FragmentedPacketEventArgs : EventArgs
	{
		public FragmentedPacket Packet { get; private set; }

		public FragmentedPacketEventArgs()
		{
		}

		public FragmentedPacketEventArgs(FragmentedPacket packet)
		{
			Packet = packet;
		}
	}

	public class FragmentedPacket : IDisposable
	{
		public IMemoryOwner<byte> Buffer;
		public int Count;
		public int Received;
		public int Length;

		public uint Sequence;

		private bool[] chunks;

		#region Ctor/Dispose

		public FragmentedPacket(uint sequence, int count)
		{
			Sequence = sequence;
			Buffer = MemoryPool<byte>.Shared.Rent(count * FragmentHeader.MaxChunk);
			Count = count;

			chunks = new bool[Count];
		}

		~FragmentedPacket()
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
				Buffer.Dispose();
			}
		}

		#endregion

		public void AddChunk(in Span<byte> data, int index, int size)
		{
			if (!chunks[index])
			{
				Memory<byte> chunk = Buffer.Memory.Slice(index * FragmentHeader.MaxChunk, size);
				data.CopyTo(chunk.Span);
				Received++;
				Length += size;
				chunks[index] = true;
			}
			else
			{
			}
		}
	}
}
