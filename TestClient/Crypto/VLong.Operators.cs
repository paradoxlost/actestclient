using System;
using System.Collections.Generic;
using System.Text;

namespace TestClient.Crypto
{
	partial class VLong
	{
		#region Cast Operators

		public static implicit operator VLong(uint value)
		{
			VLong result = new VLong();
			result.Init(value);
			return result;
		}

		#endregion

		#region Arithmatic Operators

		public override bool Equals(object obj)
		{
			VLong val = obj as VLong;
			return val != null && this.Compare(val) == 0;
		}

		public override int GetHashCode()
		{
			return this.data.GetHashCode();
		}

		public static VLong operator +(VLong left, VLong right)
		{
			VLong result = null;

			if (left.negative == right.negative)
			{
				result = new VLong(left);
				result.Add(right);
			}
			else if (left.Compare(right) >= 0)
			{
				result = new VLong(left);
				result.Subtract(right);
			}
			else
			{
				//result = new VLong(right);
				//result.Add(left);
				result = right + left;
			}

			return result;
		}

		public static VLong operator -(VLong left, VLong right)
		{
			VLong result = null;

			if (left.negative != right.negative)
			{
				result = new VLong(left);
				result.Add(right);
			}
			else if (left.Compare(right) >= 0)
			{
				result = new VLong(left);
				result.Subtract(right);
			}
			else
			{
				result = new VLong(right);
				result.Subtract(left);
				result.negative = 1 - result.negative;
			}

			return result;
		}

		public static VLong operator *(VLong left, VLong right)
		{
			VLong result = new VLong();

			result.Mul(left, right);
			result.negative = left.negative ^ right.negative;

			return result;
		}

		public static VLong operator /(VLong left, VLong right)
		{
			VLong result = new VLong();
			VLong remainder = new VLong();

			result.Divide(left, right, remainder);
			result.negative = left.negative ^ right.negative;

			return result;
		}

		public static VLong operator %(VLong left, VLong right)
		{
			VLong result = new VLong();
			VLong remainder = new VLong();

			result.Divide(left, right, remainder);
			remainder.negative = left.negative;

			return remainder;
		}

		public static VLong operator ^(VLong left, VLong right)
		{
			VLong result = new VLong();

			result.Copy(left);
			result.Xor(right);

			return result;
		}

		public static VLong operator &(VLong left, VLong right)
		{
			VLong result = new VLong();

			result.Copy(left);
			result.And(right);

			return result;
		}

		public static VLong operator <<(VLong left, int factor)
		{
			VLong result = new VLong();

			result.Copy(left);

			while (factor > 0)
			{
				factor--;
				result += result;
			}

			return result;
		}

		public static VLong operator >>(VLong left, int factor)
		{
			VLong result = new VLong();

			result.Copy(left);
			result.Shr(factor);

			return result;
		}

		#endregion

		//vlong pow2( unsigned n );

		#region Comparison Operators

		public static bool operator !=(VLong left, VLong right)
		{
			return !object.Equals(left, null) && left.Compare(right) != 0;
		}

		public static bool operator ==(VLong left, VLong right)
		{
			return !object.Equals(left, null) && left.Compare(right) == 0;
		}

		public static bool operator >=(VLong left, VLong right)
		{
			return !object.Equals(left, null) && left.Compare(right) >= 0;
		}

		public static bool operator <=(VLong left, VLong right)
		{
			return !object.Equals(left, null) && left.Compare(right) <= 0;
		}

		public static bool operator >(VLong left, VLong right)
		{
			return !object.Equals(left, null) && left.Compare(right) > 0;
		}

		public static bool operator <(VLong left, VLong right)
		{
			return !object.Equals(left, null) && left.Compare(right) < 0;
		}

		#endregion
	}
}
