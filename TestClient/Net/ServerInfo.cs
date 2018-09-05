using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using TestClient.Crypto;
using TestClient.Net.Packets;

namespace TestClient.Net
{
	public class ServerInfo
	{
		public IPEndPoint WriteAddress { get; set; }
		public IPEndPoint ReadAddress { get; set; }
		public ServerFlags Flags { get; set; }

		public ushort ServerId { get; set; }
		public ushort ClientId { get; set; }
		public ushort Table { get; set; }
		public uint SendCount { get; set; }
		public uint RecvCount { get; set; }
		public uint ConnectCount { get; set; }
		public double ServerTime { get; set; }
		public long LastConnect { get; set; }
		public long LastPacketSent { get; set; }
		public long LastPing { get; set; }
		public long LastSyncSent { get; set; }
		public long LastSyncRecv { get; set; }
		public int LastAck { get; set; }
		public ushort LastEventAck { get; set; }
		public long StartTime { get; private set; }
		public long ConnectTime { get; private set; }

		public CryptoSystem SendGenerator { get; private set; }
		public CryptoSystem RecvGenerator { get; private set; }

		public ServerInfo()
		{
		}

		public ServerInfo(string host, int port)
			: this()
		{
			Debug.Print($"Saving Server {host}:{port}");

			if (IPAddress.TryParse(host, out IPAddress ipAddr))
			{
				WriteAddress = new IPEndPoint(ipAddr, port);
				ReadAddress = new IPEndPoint(ipAddr, port + 1);
			}
			else
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(host);
				if (hostEntry == null)
					throw new ArgumentException("Invalid Server Address");

				WriteAddress = new IPEndPoint(hostEntry.AddressList[0], port);
				ReadAddress = new IPEndPoint(hostEntry.AddressList[0], port + 1);
			}
		}

		public bool IsFrom(IPEndPoint endPoint)
		{
			if (ReadAddress.Address.Equals(endPoint.Address))
			{
				return ReadAddress.Port <= endPoint.Port && endPoint.Port <= ReadAddress.Port + 1;
			}
			return false;
		}

		public void Reset()
		{

		}

		public void SetSeeds(uint recv, uint send)
		{
			Flags |= ServerFlags.ChecksumSeeds;
			RecvGenerator = new CryptoSystem(recv);
			SendGenerator = new CryptoSystem(send);
		}
	}
}
