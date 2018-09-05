using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TestClient.Util
{
	public static class SpanExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(this Span<byte> buffer, ref int position) where T : struct
		{
			T result = default(T);
			//System.Diagnostics.Debug.WriteLine($"SizeOf<{typeof(T).ToString()}> = {Marshal.SizeOf(typeof(T))}");

			result = MemoryMarshal.Read<T>(buffer.Slice(position));
			position += Marshal.SizeOf(typeof(T));

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(this Span<byte> buffer, int position) where T : struct
		{
			T result = default(T);
			result = MemoryMarshal.Read<T>(buffer.Slice(position));
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(this Memory<byte> buffer, ref int position) where T : struct
		{
			return buffer.Span.Read<T>(ref position);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Read<T>(this Memory<byte> buffer, int position) where T : struct
		{
			return buffer.Span.Read<T>(position);
		}
	}
}
