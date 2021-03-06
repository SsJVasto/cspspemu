﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct GpuMatrix4x4Struct
	{
		readonly static public int[] Indexes = new int[] {
			0, 1, 2, 3,
			4, 5, 6, 7,
			8, 9, 10, 11,
			12, 13, 14, 15
		};

		/// <summary>
		/// 
		/// </summary>
		public fixed float Values[4 * 4];

		/// <summary>
		/// 
		/// </summary>
		public uint Index;

		/// <summary>
		/// 
		/// </summary>
		internal void Reset(uint Index = 0)
		{
			this.Index = Index;
		}

		public void Dump()
		{
			fixed (float* ValuesPtr = Values)
			{
				Console.WriteLine("----------------------");
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						Console.Write("{0}, ", ValuesPtr[y * 4 + x]);
					}
					Console.WriteLine("");
				}
				Console.WriteLine("----------------------");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		internal void Write(float Value)
		{
			//if (Index < Indexes.Length)
			{
				fixed (float* ValuesPtr = Values)
				{
					ValuesPtr[Indexes[Index++ % Indexes.Length]] = Value;
				}
			}
		}

		public void SetIdentity()
		{
			fixed (float* ValuesPtr = Values)
			{
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						ValuesPtr[x + y * 4] = (x == y) ? 1 : 0;
					}
				}
			}
		}

		public Matrix4 Matrix4
		{
			get
			{
				fixed (float* ValuesPtr = Values)
				{
					var Matrix4 = new Matrix4(
						ValuesPtr[0], ValuesPtr[1], ValuesPtr[2], ValuesPtr[3],
						ValuesPtr[4], ValuesPtr[5], ValuesPtr[6], ValuesPtr[7],
						ValuesPtr[8], ValuesPtr[9], ValuesPtr[10], ValuesPtr[11],
						ValuesPtr[12], ValuesPtr[13], ValuesPtr[14], ValuesPtr[15]
					);
					//Matrix4.Transpose();
					return Matrix4;
				}
			}
		}

	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	unsafe public struct GpuMatrix4x3Struct
	{
		readonly static public int[] Indexes = new int[] {
			0, 1, 2,
			4, 5, 6,
			8, 9, 10,
			12, 13, 14
		};

		/// <summary>
		/// 
		/// </summary>
		public fixed float Values[4 * 4];

		/// <summary>
		/// 
		/// </summary>
		public uint Index;

		/// <summary>
		/// 
		/// </summary>
		internal void Reset(uint Index = 0)
		{
			fixed (float* ValuesPtr = Values)
			{
				//ValuesPtr[15] = 1.0f;
			}
			this.Index = Index;
		}

		public void Dump()
		{
			fixed (float* ValuesPtr = Values)
			{
				Console.WriteLine("----------------------");
				for (int y = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++)
					{
						Console.Write("{0}, ", ValuesPtr[y * 4 + x]);
					}
					Console.WriteLine("");
				}
				Console.WriteLine("----------------------");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Value"></param>
		internal void Write(float Value)
		{
			//if (Index < Indexes.Length)
			{
				fixed (float* ValuesPtr = Values)
				{
					ValuesPtr[Indexes[Index++ % Indexes.Length]] = Value;
				}
			}
		}

		internal void WriteAt(int Index, float Value)
		{
			//if (Index < Indexes.Length)
			{
				fixed (float* ValuesPtr = Values)
				{
					ValuesPtr[Indexes[Index]] = Value;
				}
			}
		}

		public Matrix4 Matrix4
		{
			get
			{
				fixed (float* ValuesPtr = Values)
				{
					var Matrix4 = new Matrix4(
#if false
						ValuesPtr[0], ValuesPtr[1], ValuesPtr[2], ValuesPtr[3],
						ValuesPtr[4], ValuesPtr[5], ValuesPtr[6], ValuesPtr[7],
						ValuesPtr[8], ValuesPtr[9], ValuesPtr[10], ValuesPtr[11],
						ValuesPtr[12], ValuesPtr[13], ValuesPtr[14], ValuesPtr[15]
#else
						ValuesPtr[0], ValuesPtr[1], ValuesPtr[2], 0,
						ValuesPtr[4], ValuesPtr[5], ValuesPtr[6], 0,
						ValuesPtr[8], ValuesPtr[9], ValuesPtr[10], 0,
						ValuesPtr[12], ValuesPtr[13], ValuesPtr[14], 1.0f
#endif
					);
					//Matrix4.Transpose();
					return Matrix4;
				}
			}
		}

		public void SetLastColumn()
		{
			fixed (float* ValuesPtr = Values)
			{
				ValuesPtr[3] = 0.0f;
				ValuesPtr[7] = 0.0f;
				ValuesPtr[11] = 0.0f;
				ValuesPtr[15] = 1.0f;
			}
		}
	}
}
