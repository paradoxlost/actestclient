using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient.Crypto
{
	partial class VLong
	{
		public class Value
		{
			private uint[] data;

			public int UnitsUsed { get; set; }

			public int Length
			{
				get { return this.data.Length; }
				set { EnsureCapacity(value); }
			}

			public uint this[int index]
			{
				get { return this.data[index]; }
				set { this.data[index] = value; }
			}

			internal uint[] Items { get { return this.data; } }

			public Value()
			{
				this.data = new uint[1];
			}

			private void EnsureCapacity(int size)
			{
				if (this.data.Length < size)
				{
					uint[] newData = new uint[size];

					Array.Copy(this.data, newData, this.data.Length);
					Clear();

					this.data = newData;
				}
			}

			#region "Copy" Operators

			public Value Copy()
			{
				return Copy(true);
			}

			public Value Copy(bool shallow)
			{
				//if (shallow)
				//	return this;

				Value v = new Value();
				v.data = new uint[this.data.Length];
				Array.Copy(this.data, v.data, this.data.Length);
				v.UnitsUsed = UnitsUsed;

				return v;
			}

			#endregion

			#region Flex Unit

			public void Clear()
			{
				Array.Clear(this.data, 0, this.data.Length);
			}

			public uint Get(int index)
			{
				return (index >= UnitsUsed) ? 0 : this.data[index];
			}

			public void Set(int index, uint value)
			{
				if (index < UnitsUsed)
				{
					this.data[index] = value;
					if (value == 0)
						while (UnitsUsed > 0 && this.data[UnitsUsed - 1] == 0)
							UnitsUsed--;
				}
				else
				{
					if (value != 0)
					{
						Reserve(index + 1);
						for (int i = UnitsUsed; i < index; i++)
							this.data[i] = 0;

						this.data[index] = value;
						UnitsUsed = index + 1;
					}
				}
			}

			public void Reserve(int size)
			{
				EnsureCapacity(size);
			}

			private static uint lo(uint val)
			{
				return (val & ((1 << 16) - 1));
			}

			private static uint hi(uint val)
			{
				return val >> 16;
			}

			private static uint lh(uint val)
			{
				return val << 16;
			}

			private unsafe uint DoInner(int n, uint m, int offset, uint[] ya)
			{
				uint c = 0;
				uint[] items = this.data;
				fixed (uint* pa = items, pya = ya)
				{
					uint* pai = pa + offset, pyai = pya;

					while (n-- > 0)
					{
						uint v = *pai, p = *pyai++;
						v += c;
						c = (v < c) ? 1u : 0;
						uint w;

						//w = lo(p) * lo(m); v += w; c += (v < w);
						w = lo(p) * lo(m);
						v += w;
						c += (v < w) ? 1u : 0;

						//w = lo(p) * hi(m); c += hi(w); w = lh(w); v += w; c += (v < w);
						w = lo(p) * hi(m);
						c += hi(w);
						w = lh(w);
						v += w;
						c += (v < w) ? 1u : 0;

						//w = hi(p) * lo(m); c += hi(w); w = lh(w); v += w; c += (v < w);
						w = hi(p) * lo(m);
						c += hi(w);
						w = lh(w);
						v += w;
						c += (v < w) ? 1u : 0;

						//c += hi(p) * hi(m);
						c += hi(p) * hi(m);

						*pai++ = v;
					}
				}
				return c;
			}

			public void FastMul(Value x, Value y, int keep)
			{
				int limit = (keep + BitsPerUnit - 1) / BitsPerUnit;
				Reserve(limit);
				Clear();

				int min = Math.Min(x.UnitsUsed, limit);

				uint[] items = x.data;
				for (int i = 0; i < min; i++)
				{
					uint m = items[i];
					int min2 = Math.Min(i + y.UnitsUsed, limit);
					uint c = DoInner(min2 - i, m, i, y.data);
					int j = min2;
					while (c > 0 && j < limit)
					{
						this.data[j] += c;
						c = (this.data[j] < c) ? 1u : 0;
						j += 1;
					}
				}

				keep %= BitsPerUnit;
				if (keep > 0)
					this.data[limit - 1] &= (1u << keep) - 1u;

				while (limit > 0 && this.data[limit - 1] == 0)
					limit--;

				UnitsUsed = limit;
			}

			#endregion
		}
	}
}
