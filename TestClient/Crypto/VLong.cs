using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient.Crypto
{
	public partial class VLong
	{
		#region Private stuff

		private static byte[] BitTab = new byte[] {
			0,1,2,2,3,3,3,3,4,4,4,4,4,4,4,4,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
			6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,
			7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
			7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,
			8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
			8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
			8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,
			8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8 };

		private static int BitsPerUnit = 8 * System.Runtime.InteropServices.Marshal.SizeOf(typeof(uint));
		private static int BitsPerHalfUnit = BitsPerUnit / 2;

		private Value data;
		private int negative;

		#endregion

		#region Properties

		public int UnitsUsed
		{
			get { return this.data.UnitsUsed; }
			protected set { this.data.UnitsUsed = value; }
		}

		public int Size
		{
			get { return this.data.Length; }
			set { this.data.Length = value; }
		}

		public Value InnerValue { get { return this.data; } }

		#endregion

		#region Ctor

		public VLong()
		{
			this.data = new Value();
		}

		public VLong(VLong copy)
			: this()
		{
			Copy(copy);
		}

		public VLong(uint value)
			: this()
		{
			Init(value);
		}

		public static VLong Unpack(uint[] values)
		{
			VLong tmp = new VLong();

			uint len = values[0];
			uint[] values2 = new uint[values.Length - 1];
			Array.Copy(values, 1, values2, 0, len);
			tmp.Load(values2);

			return tmp;
		}

		#endregion

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = UnitsUsed - 1; i >= 0; i--)
			{
				sb.Append(this.data[i].ToString("x8"));
			}

			return sb.ToString();
		}

		public void Load(uint[] values)
		{
			this.data = new Value();
			for (int i = 0; i < values.Length; i++)
			{
				this.data.Set(i, values[i]);
			}
		}

		#region VLong Operations

		public bool IsZero { get { return this.UnitsUsed == 0; } }

		public bool GetBit(int index)
		{
			return ((this.data.Get(index / BitsPerUnit) & (1u << (index % BitsPerUnit))) != 0);
		}

		public void SetBit(int index)
		{
			int bitIndex = index / BitsPerUnit;
			this.data.Set(bitIndex, this.data.Get(bitIndex) | (1u << index % BitsPerUnit));
		}

		public void ClearBit(int index)
		{
			int bitIndex = index / BitsPerUnit;
			this.data.Set(bitIndex, this.data.Get(bitIndex) & ~(1u << index % BitsPerUnit));
		}

		public int GetBits()
		{
			// I didn't bother to recode this beyond the obvious error corrections
			int x = UnitsUsed;
			if (x > 0)
			{
				uint msw = this.data.Get(x - 1);
				x = (x - 1) * BitsPerUnit;
				int w = BitsPerUnit;
				do
				{
					w >>= 1;
					if (msw >= (1u << w))
					{
						x += w;
						msw >>= w;
					}
				} while (w > 8);
				x += BitTab[msw];
			}
			return x;
		}

		// cf
		public int Compare(VLong other)
		{
			bool neg = this.negative != 0 && !IsZero;
			if (!object.Equals(other, null) && neg == (other.negative != 0 && !other.IsZero))
			{
				if (UnitsUsed > other.UnitsUsed) return 1;
				if (UnitsUsed < other.UnitsUsed) return -1;
				int i = UnitsUsed;
				while (i > 0)
				{
					i--;
					if (this.data.Get(i) > other.data.Get(i)) return 1;
					if (this.data.Get(i) < other.data.Get(i)) return -1;
				}
				return 0;
			}
			else if (neg)
				return -1;
			else
				return 1;
		}

		public int Product(VLong other)
		{
			VLong right = new VLong(other);

			int max = Math.Min(UnitsUsed, right.UnitsUsed);
			uint tmp = 0;

			for (int i = 0; i < max; i += 1)
				tmp ^= this.data.Get(i) & right.data.Get(i);

			uint count = 0;
			while (tmp > 0)
			{
				if ((tmp & 1) != 0)
					count++;
				tmp >>= 1;
			}

			return (int)(count & 1);
		}

		public void Shl()
		{
			uint carry = 0;
			int N = UnitsUsed;
			for (int i = 0; i <= N; i += 1)
			{
				uint u = this.data.Get(i);
				this.data.Set(i, (u << 1) + carry);
				carry = u >> (BitsPerUnit - 1);
			}
		}

		public bool Shr()
		{
			uint carry = 0;
			int i = UnitsUsed;
			while (i > 0)
			{
				i--;
				uint u = this.data.Get(i);
				this.data.Set(i, (u >> 1) + carry);
				carry = u << (BitsPerUnit - 1);
			}
			return carry != 0;
		}

		public void Shr(int factor)
		{
			int delta = factor / BitsPerUnit;
			factor %= BitsPerUnit;

			for (int i = 0; i < UnitsUsed; i += 1)
			{
				uint u = this.data.Get(i + delta);
				if (factor > 0)
				{
					u >>= factor;
					u += this.data.Get(i + delta + 1) << (BitsPerUnit - factor);
				}
				this.data.Set(i, u);
			}
		}

		public void Add(VLong other)
		{
			VLong right = new VLong(other);

			uint carry = 0;
			int max = Math.Max(UnitsUsed, right.UnitsUsed);
			this.data.Reserve(max);

			for (int i = 0; i < max + 1; i += 1)
			{
				uint u = this.data.Get(i);
				u += carry;
				carry = (u < carry) ? 1u : 0;

				uint ux = right.data.Get(i);
				u += ux;
				carry += (u < ux) ? 1u : 0;
				this.data.Set(i, u);
			}
		}

		public void Xor(VLong other)
		{
			VLong right = new VLong(other);
			int max = Math.Max(UnitsUsed, right.UnitsUsed);
			this.data.Reserve(max);

			for (int i = 0; i < max; i += 1)
			{
				this.data.Set(i, this.data.Get(i) ^ right.data.Get(i));
			}
		}

		public void And(VLong other)
		{
			VLong right = new VLong(other);
			int max = Math.Max(UnitsUsed, right.UnitsUsed);
			this.data.Reserve(max);
			for (int i = 0; i < max; i += 1)
			{
				this.data.Set(i, this.data.Get(i) & right.data.Get(i));
			}
		}

		public void Subtract(VLong other)
		{
			VLong right = new VLong(other);
			uint carry = 0;
			int N = UnitsUsed;
			for (int i = 0; i < N; i += 1)
			{
				uint ux = right.data.Get(i);
				ux += carry;
				if (ux >= carry)
				{
					uint u = this.data.Get(i);
					uint nu = u - ux;
					carry = (nu > u) ? 1u : 0;
					this.data.Set(i, nu);
				}
			}
		}

		public void Init(uint x)
		{
			this.data.Clear();
			this.data.Set(0, x);
		}

		public void Copy(VLong other)
		{
			this.negative = other.negative;
			this.data.Clear();
			this.data = other.data.Copy(false);
		}

		public void Mul(VLong left, VLong right)
		{
			this.data.FastMul(left.data, right.data, left.GetBits() + right.GetBits());
		}

		public void Divide(VLong left, VLong right, VLong remainder)
		{
			Init(0);
			remainder.Copy(left);
			VLong m = new VLong();
			VLong s = new VLong();

			m.Copy(right);
			s.Init(1);

			while (remainder.Compare(m) > 0)
			{
				m.Shl();
				s.Shl();
			}
			while (remainder.Compare(right) >= 0)
			{
				while (remainder.Compare(m) < 0)
				{
					m.Shr();
					s.Shr();
				}
				remainder.Subtract(m);
				Add(s);
			}
		}

		#endregion
	}
}
