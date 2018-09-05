using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient.Crypto
{
	partial class VLong
	{
		#region Parse

		public static VLong Parse(string data)
		{
			return Parse(data, false);
		}

		public static VLong Parse(string data, bool isHex)
		{
			//int segSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(uint)) * 2;
			System.Globalization.NumberStyles style = isHex ?
				System.Globalization.NumberStyles.AllowHexSpecifier :
				System.Globalization.NumberStyles.None;

			int pos = 0;

			VLong ret = new VLong();

			while (pos < data.Length)
			{
				ret *= 0x100;

				uint piece = uint.Parse(
					data.Substring(pos, 2),
					style);

				ret.Add(piece);

				pos += 2;
			}

			return ret;
		}

		#endregion

		#region Operations

		public static VLong Add(VLong left, VLong right)
		{
			VLong result = new VLong();

			uint carry = 0;
			int max = Math.Max(left.UnitsUsed, right.UnitsUsed);
			result.data.Reserve(max);

			for (int i = 0; i < max + 1; i += 1)
			{
				uint u = left.data.Get(i);
				u += carry;
				carry = (u < carry) ? 1u : 0;

				uint ux = right.data.Get(i);
				u += ux;
				carry += (u < ux) ? 1u : 0;
				result.data.Set(i, u);
			}

			return result;
		}

		public static VLong Subtract(VLong left, VLong right)
		{
			VLong result = new VLong();
			uint carry = 0;
			int used = left.UnitsUsed;
			for (int i = 0; i < used; i += 1)
			{
				uint ux = right.data.Get(i);
				ux += carry;
				if (ux >= carry)
				{
					uint u = left.data.Get(i);
					uint nu = u - ux;
					carry = (nu > u) ? 1u : 0;
					result.data.Set(i, nu);
				}
			}
			return result;
		}

		public static int Product(VLong left, VLong right)
		{
			int max = Math.Min(left.UnitsUsed, right.UnitsUsed);
			uint tmp = 0;

			for (int i = 0; i < max; i += 1)
				tmp ^= left.data.Get(i) & right.data.Get(i);

			uint count = 0;
			while (tmp > 0)
			{
				if ((tmp & 1) != 0)
					count++;
				tmp >>= 1;
			}

			return (int)(count & 1);
		}


		#endregion
	}
}
