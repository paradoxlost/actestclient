using System;
using System.IO;

namespace TestClient.Net.Packets
{
	public class LoginRequestPacket : Packet
	{
		public string ProtocolVersion { get; set; }
		public string AccountName { get; set; }
		public string Password { get; set; }

		public LoginRequestPacket()
			: base(PacketFlags.LoginRequest)
		{
			ProtocolVersion = "1802";//Net.NetworkManager.ProtocolVersion;
		}

		public LoginRequestPacket(string account, string password)
			: this()
		{
			AccountName = account.ToLower();
			Password = password;
		}

		protected override void OnSerialize(BinaryWriter writer)
		{
			// strings are 4byte aligned including length
			writer.Write((short)ProtocolVersion.Length);
			writer.Write(ProtocolVersion.ToCharArray());
			writer.Seek(4 - ((ProtocolVersion.Length + 2) % 4), SeekOrigin.Current);

			int userNamePad = 0;
			int passwordPad = 0;
			int packetLen = 20;
			uint loginType = 0;

			userNamePad = (AccountName.Length + 2) % 4;
			if (userNamePad > 0) userNamePad = 4 - userNamePad;

			packetLen += AccountName.Length + 2 + userNamePad;

			if (string.IsNullOrEmpty(Password))
			{
				loginType = 0x0000001u;
			}
			else
			{
				loginType = 0x0000002u;
				passwordPad = (Password.Length + 5) % 4;
				if (passwordPad > 0) passwordPad = 4 - passwordPad;

				packetLen += Password.Length + 5 + passwordPad;
			}

			// length??
			writer.Write(packetLen);

			// login type
			// 00000001 - account
			// 00000002 - account/password
			// 40000002 - account/ticket
			writer.Write(loginType);

			// unknown
			writer.Write(0u);

			// timestamp
			writer.Write((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

			// account name
			writer.Write((short)AccountName.Length);
			writer.Write(AccountName.ToCharArray());
			if (userNamePad > 0)
				writer.Seek(userNamePad, SeekOrigin.Current);

			// empty string
			writer.Write(0u);

			// ticket
			if (!string.IsNullOrEmpty(Password))
			{
				// int length of pshort and data
				writer.Write(Password.Length + 1);
				// packed short data length
				writer.Write((byte)Password.Length);
				// data
				writer.Write(Password.ToCharArray());
				// align-pad
				if (passwordPad > 0)
					writer.Seek(passwordPad, SeekOrigin.Current);
			}
			else
				writer.Write(0u);

			// dat versions
			// int length
			// writer.Write(0x0000001Cu);
			// // engine
			// writer.Write(0x00000016u);
			// // game
			// writer.Write(0x00000000u);

			// // major
			// writer.Write(0x4C46722F34A7D7D2u);
			// writer.Write(0xFD6F854F51EFB48Au);

			// // minor
			// writer.Write(0x00001A01u);
		}
	}
}
