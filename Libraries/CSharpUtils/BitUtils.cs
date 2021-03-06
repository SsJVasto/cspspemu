﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace CSharpUtils
{
	static public class BitUtils
	{
#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public uint CreateMask(int Size)
		{
			return (uint)((1 << Size) - 1);
		}

#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public void Insert(ref uint Value, int Offset, int Count, uint ValueToInsert)
		{
			Value = Insert(Value, Offset, Count, ValueToInsert);
		}

#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public uint Insert(uint InitialValue, int Offset, int Count, uint ValueToInsert)
		{
			uint Mask = CreateMask(Count);
			InitialValue &= ~(Mask << Offset);
			InitialValue |= (ValueToInsert & Mask) << Offset;
			return InitialValue;
		}

#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public uint Extract(uint InitialValue, int Offset, int Count)
		{
			if (Count == 0) return 0;
			return (uint)((InitialValue >> Offset) & CreateMask(Count));
		}

#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public uint ExtractScaled(uint InitialValue, int Offset, int Count, int Scale)
		{
			if (Count == 0) return 0;
			return (uint)((Extract(InitialValue, Offset, Count) * Scale) / CreateMask(Count));
		}

#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public bool ExtractBool(uint InitialValue, int Offset)
		{
			return Extract(InitialValue, Offset, 1) != 0;
		}

#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public int ExtractSigned(uint InitialValue, int Offset, int Count)
		{
			var Mask = CreateMask(Count);
			uint SignBit = (uint)(1 << (Offset + (Count - 1)));
			uint _Value = (uint)((InitialValue >> Offset) & Mask);
			if ((_Value & SignBit) != 0)
			{
				_Value |= ~Mask;
			}
			return (int)_Value;
		}

#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public float ExtractUnsignedScaled(uint Value, int Offset, int Count, float Scale = 1.0f)
		{
			return ((float)Extract(Value, Offset, Count) / (float)CreateMask(Count)) * Scale;
			throw new NotImplementedException();
		}

#if NET_4_5
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		static public byte[] XorBytes(params byte[][] Arrays)
		{
			int Length = Arrays[0].Length;
			foreach (var Array in Arrays) if (Array.Length != Length) throw(new InvalidOperationException("Arrays sizes must match"));
			var Bytes = new byte[Length];
			foreach (var Array in Arrays)
			{
				for (int n = 0; n < Length; n++) Bytes[n] ^= Array[n];
			}
			return Bytes;
		}
	}
}
