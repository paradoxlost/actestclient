using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient.Crypto
{
	partial class VLong
	{
		#region Statics

		public static VLong ModInv(VLong a, VLong m)
		{
			VLong j = new VLong(1u);
			VLong i = new VLong(0);
			VLong b = new VLong(m);
			VLong c = new VLong(a);
			VLong x = new VLong();
			VLong y = new VLong();

			while (!c.IsZero)
			{
				x = b / c;
				y = b - (x * c);
				b = c;
				c = y;
				y = j;
				j = i - (j * x);
				i = y;
			}
			if (i < 0)
				i += m;
			return i;
		}

		public static VLong ModExp(VLong x, VLong e, VLong m)
		{
			Monty me = new Monty(m);
			return me.Exp(x, e);
		}

		#endregion

		private class Monty
		{
			private VLong m;
			private VLong n1;

			private VLong t;
			private VLong k;

			private int rBits;
			public VLong R { get; private set; }
			public VLong R1 { get; private set; }

			public Monty(VLong mod)
			{
				this.m = mod;

				this.rBits = 0;

				R = new VLong(1u);

				while (R < this.m)
				{
					R += R;
					//R.Add(R);
					this.rBits++;
				}

				R1 = ModInv(R - m, m);
				n1 = R - ModInv(this.m, R);
			}

			private void Mul(ref VLong x, VLong y)
			{
				this.t.data.FastMul(x.data, y.data, this.rBits * 2);

				this.k.data.FastMul(this.t.data, this.n1.data, this.rBits);

				x.data.FastMul(this.k.data, this.m.data, this.rBits * 2);
				x += this.t;
				x.Shr(this.rBits);

				if (x >= this.m)
					x -= this.m;
			}

			public VLong MontyExp(VLong x, VLong e)
			{
				VLong result = R - this.m;
				VLong t = new VLong(x);

				this.t = new VLong();
				this.k = new VLong();

				int bits = e.GetBits();
				int i = 0;

				while (true)
				{
					if (e.GetBit(i))
						Mul(ref result, t);
					i++;
					if (i == bits)
						break;
					Mul(ref t, t);
				}
				return result;
			}

			public VLong Exp(VLong x, VLong e)
			{
				return (MontyExp((x * R) % this.m, e) * R1) % this.m;
			}
		}
	}
}
