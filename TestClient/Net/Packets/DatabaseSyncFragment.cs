using System;
using System.IO;

namespace TestClient.Net.Packets
{
	public class DatabaseSyncFragment : Fragment
	{
		public DatabaseSyncFragment()
			: base(0xf7e6, FragmentGroup.Event)
		{
		}

		protected override void OnSerialize(BinaryWriter writer)
		{
			// 0000   e6 f7 00 00 01 00 00 00 03 00 00 00 00 00 00 00
			// 0010   01 00 00 00 18 08 00 00 e8 f7 ff ff 01 00 00 00
			// 0020   01 00 00 00 03 00 00 00 e2 03 00 00 1e fc ff ff
			// 0030   01 00 00 00 01 00 00 00 02 00 00 00 d6 03 00 00
			// 0040   2a fc ff ff 01 00 00 00 00 00 00 00 00 00 00 00

			// msg
			// 1
			// count
			// dat[0x14c], dat[0x150], dat_trans_id, 1
			// 0 pad to 80 bytes (2 words)

			writer.Write(1);
			writer.Write(3);

			// portal
			writer.Write(0);
			writer.Write(1);
			writer.Write(0x00000818u);
			writer.Write(0xfffff7e8u);
			writer.Write(1);

			// client_local_english
			writer.Write(1);
			writer.Write(3);
			writer.Write(0x000003e2u);
			writer.Write(0xfffffc1eu);
			writer.Write(1);

			// cell
			writer.Write(1);
			writer.Write(2);
			writer.Write(0x000003d6u);
			writer.Write(0xfffffc2au);
			writer.Write(1);

			writer.Write(0ul);
		}
	}
}
