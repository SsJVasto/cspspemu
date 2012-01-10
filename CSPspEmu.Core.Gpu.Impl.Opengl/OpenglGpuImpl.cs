﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpUtils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.State;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System.Globalization;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Utils;
//using Cloo;
//using Cloo.Bindings;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public partial class OpenglGpuImpl : GpuImpl
	{
		/// <summary>
		/// 
		/// </summary>
		private PspConfig Config;

		/// <summary>
		/// 
		/// </summary>
		private PspMemory Memory;

		/// <summary>
		/// 
		/// </summary>
		private VertexReader VertexReader;

		/// <summary>
		/// 
		/// </summary>
		private TextureCache TextureCache;

		/// <summary>
		/// 
		/// </summary>
		private Texture CurrentTexture;

		/// <summary>
		/// 
		/// </summary>
		public override void InitializeComponent()
		{
			this.Config = PspEmulatorContext.PspConfig;
			this.Memory = PspEmulatorContext.GetInstance<PspMemory>();
			this.TextureCache = PspEmulatorContext.GetInstance<TextureCache>();
			this.VertexReader = new VertexReader();
		}

		//static public object GpuLock = new object();

		/// <summary>
		/// 
		/// </summary>
		void Initialize()
		{
			///GL.Enable(EnableCap.Blend);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			/*
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int)TextureEnvModeCombine.Modulate);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int)TextureEnvModeSource.Texture);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Rgb, (int)TextureEnvModeOperandRgb.SrcColor);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb, (int)TextureEnvModeSource.Previous);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand1Rgb, (int)TextureEnvModeOperandRgb.SrcColor);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineAlpha, (int)TextureEnvModeCombine.Replace);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src0Alpha, (int)TextureEnvModeSource.Previous);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Alpha, (int)TextureEnvModeOperandAlpha.SrcAlpha);
			*/
			//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Combine);

			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, Color.FromArgb(1, 0, 0, 0));
			//glAlphaFunc(GL_GREATER,0.000000)


			//glTexEnvi(GL_TEXTURE_ENV,GL_TEXTURE_ENV_MODE,GL_MODULATE)


			//glTexEnvfv(GL_TEXTURE_ENV,GL_TEXTURE_ENV_COLOR, { 0.000000, 0.000000, 0.000000, 1.000000 } )

			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

			//drawBeginClear(GpuState);
			/*
			GL.ClearColor(0, 0, 0, 1);
			GL.ClearDepth(0);
			GL.ClearStencil(0);
			GL.ClearAccum(0, 0, 0, 0);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.AccumBufferBit);
			*/
		}

		GpuStateStruct* GpuState;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="VertexInfo"></param>
		/// <param name="VertexType"></param>
		private void PutVertex(ref VertexInfo VertexInfo, ref VertexTypeStruct VertexType)
		{
			/*
			if (GpuState->ClearingMode)
			{
				Console.WriteLine(VertexInfo);
			}
			*/
			//Console.WriteLine(VertexInfo);

			//Console.WriteLine(VertexInfo);
			//Console.WriteLine(VertexInfo);
			if (VertexType.Color != VertexTypeStruct.ColorEnum.Void)
			{
				GL.Color4((float)VertexInfo.R, (float)VertexInfo.G, (float)VertexInfo.B, (float)VertexInfo.A);
			}
			if (VertexType.Texture != VertexTypeStruct.NumericEnum.Void)
			{
				/*
				if (VertexType.Texture == VertexTypeStruct.NumericEnum.Short)
				{
					var Texture = CurrentTexture;

					Console.WriteLine(
						"U={0}, V={1}, OFFSET({2}, {3}), SCALE({4}, {5}) : SIZE({6},{7}) : {8}",
						VertexInfo.U, VertexInfo.V,
						GpuState->TextureMappingState.TextureState.OffsetU, GpuState->TextureMappingState.TextureState.OffsetV,
						GpuState->TextureMappingState.TextureState.ScaleU, GpuState->TextureMappingState.TextureState.ScaleV,
						Texture.Width, Texture.Height,
						GpuState->TextureMappingState.TextureState
					);
					
					//return;
				}
				*/

				//Console.WriteLine("{0}, {1}", VertexInfo.U, VertexInfo.V);
				GL.TexCoord2(VertexInfo.U, VertexInfo.V);
			}
			if (VertexType.Normal != VertexTypeStruct.NumericEnum.Void)
			{
				GL.Normal3(VertexInfo.NX, VertexInfo.NY, VertexInfo.NZ);
			}
			if (VertexType.Position != VertexTypeStruct.NumericEnum.Void)
			{
				GL.Vertex3(VertexInfo.PX, VertexInfo.PY, VertexInfo.PZ);
			}
		}

		VertexTypeStruct VertexType;
		byte[] IndexListByte = new byte[ushort.MaxValue];
		short[] IndexListShort = new short[ushort.MaxValue];
		VertexInfo[] Vertices = new VertexInfo[ushort.MaxValue];

		void ReadVertex(int Index, VertexInfo* VertexInfo)
		{
			if (VertexType.Index == VertexTypeStruct.IndexEnum.Byte)
			{
				*VertexInfo = Vertices[IndexListByte[Index]];
			}
			else if (VertexType.Index == VertexTypeStruct.IndexEnum.Short)
			{
				*VertexInfo = Vertices[IndexListShort[Index]];
			}
			else *VertexInfo = Vertices[Index];
		}

		void drawBeginClear(GpuStateStruct* GpuState)
		{
			bool ccolorMask = false, calphaMask = false;

			//return;

			GL.Disable(EnableCap.Blend);
			GL.Disable(EnableCap.Lighting);
			GL.Disable(EnableCap.Texture2D);
			GL.Disable(EnableCap.AlphaTest);
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Fog);
			GL.Disable(EnableCap.ColorLogicOp);
			GL.Disable(EnableCap.CullFace);
			GL.DepthMask(false);

			if (GpuState->ClearFlags.HasFlag(ClearBufferSet.ColorBuffer))
			{
				ccolorMask = true;
			}

			if (GlEnableDisable(EnableCap.StencilTest, GpuState->ClearFlags.HasFlag(ClearBufferSet.StencilBuffer)))
			{
				calphaMask = true;
				// Sets to 0x00 the stencil.
				// @TODO @FIXME! : Color should be extracted from the color! (as alpha component)
				GL.StencilFunc(StencilFunction.Always, 0, 0xFF);
				GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
			}

			//int i; glGetIntegerv(GL_STENCIL_BITS, &i); writefln("GL_STENCIL_BITS: %d", i);

			if (GpuState->ClearFlags.HasFlag(ClearBufferSet.DepthBuffer))
			{
				GL.Enable(EnableCap.DepthTest);
				GL.DepthFunc(DepthFunction.Always);
				GL.DepthMask(true);
				GL.DepthRange(0, 0);

				//glDepthRange(0.0, 1.0); // Original value
			}

			GL.ColorMask(ccolorMask, ccolorMask, ccolorMask, calphaMask);

			//glClearDepth(0.0); glClear(GL_COLOR_BUFFER_BIT);

			//if (state.clearFlags & ClearBufferMask.GU_COLOR_BUFFER_BIT) glClear(GL_DEPTH_BUFFER_BIT);
		}

		public bool IsCurrentWindow = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GpuState"></param>
		override public unsafe void Prim(GpuStateStruct* GpuState, PrimitiveType PrimitiveType, ushort VertexCount)
		{
			//Console.WriteLine("Prim: {0}, {1}", PrimitiveType, VertexCount);
			this.GpuState = GpuState;

			if (!IsCurrentWindow)
			{
				IsCurrentWindow = true;
				GraphicsContext.MakeCurrent(NativeWindow.WindowInfo);
			}

			//Console.WriteLine("--------------------------------------------------------");
			VertexType = GpuState->VertexState.Type;

			VertexReader.SetVertexTypeStruct(VertexType, (byte*)Memory.PspAddressToPointerSafe(GpuState->VertexAddress));
			//IndexReader.SetVertexTypeStruct(VertexType, VertexCount, (byte*)Memory.PspAddressToPointerSafe(GpuState->IndexAddress));


			int TotalVerticesWithoutMorphing = VertexCount;

			void* IndexAddress = Memory.PspAddressToPointerSafe(GpuState->IndexAddress);

			switch (VertexType.Index)
			{
				case VertexTypeStruct.IndexEnum.Void:
					break;
				case VertexTypeStruct.IndexEnum.Byte:
					Marshal.Copy(new IntPtr(IndexAddress), IndexListByte, 0, VertexCount);
					TotalVerticesWithoutMorphing = IndexListByte.Take(VertexCount).Max() + 1;
					break;
				case VertexTypeStruct.IndexEnum.Short:
					Marshal.Copy(new IntPtr(IndexAddress), IndexListShort, 0, VertexCount);
					TotalVerticesWithoutMorphing = IndexListShort.Take(VertexCount).Max() + 1;
					break;
				default:
					throw (new NotImplementedException());
			}

			//Console.WriteLine(TotalVerticesWithoutMorphing);

			int MorpingVertexCount = (int)VertexType.MorphingVertexCount + 1;
			int z = 0;
			VertexInfo TempVertexInfo;

			float* Morphs = &GpuState->MorphingState.MorphWeight0;

			//for (int n = 0; n < MorpingVertexCount; n++) Console.Write("{0}, ", Morphs[n]); Console.WriteLine("");

			fixed (VertexInfo* VerticesPtr = Vertices)
			{
				for (int n = 0; n < TotalVerticesWithoutMorphing; n++)
				{
					//Console.WriteLine(MorpingVertexCount);
					if (MorpingVertexCount == 1)
					{
						VertexReader.ReadVertex(z++, &TempVertexInfo);
						//Console.WriteLine(TempVertexInfo);
						VerticesPtr[n] = TempVertexInfo;
					}
					else
					{
						var ComponentsIn = (float*)&TempVertexInfo;
						var ComponentsOut = (float*)&VerticesPtr[n];
						for (int cc = 0; cc < 20; cc++) ComponentsOut[cc] = 0;
						for (int m = 0; m < MorpingVertexCount; m++)
						{
							VertexReader.ReadVertex(z++, &TempVertexInfo);
							for (int cc = 0; cc < 20; cc++) ComponentsOut[cc] += ComponentsIn[cc] * Morphs[m];
						}
					}
				}
			}

			//VertexType.Texture == VertexTypeStruct.TextureEnum.Byte
			//return;
			//PrepareRead(GpuState);

			PrepareStateCommon(GpuState);

			if (GpuState->ClearingMode)
			{
				//return;
				//GL.ClearColor(1, 1, 0, 0);

				// @TODO: Fake
				/*
				*/
				//PrepareState(GpuState);
				//return;
				//Console.WriteLine(VertexCount);
				drawBeginClear(GpuState);
				/*
				GL.ClearColor(0, 0, 0, 1);
				GL.ClearDepth(0);
				GL.ClearStencil(0);
				GL.ClearAccum(0, 0, 0, 0);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.AccumBufferBit);
				return;
				*/
			}
			else
			{
				PrepareState(GpuState);
			}

			// DRAW BEGIN COMMON
			{
				if (GpuState->VertexState.Type.Transform2D)
				//if (true)
				{
					GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
					//GL.Ortho(0, 512, 272, 0, -0x7FFF, +0x7FFF);
					//GL.Ortho(0, 480, 272, 0, -0x7FFF, +0x7FFF);
					GL.Ortho(0, 480, 272, 0, 0, -0xFFFF);
					
					GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();
				}
				else
				{
					GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
					GL.MultMatrix(GpuState->VertexState.ProjectionMatrix.Values);

					GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();
					GL.MultMatrix(GpuState->VertexState.ViewMatrix.Values);
					GL.MultMatrix(GpuState->VertexState.WorldMatrix.Values);

					if (GpuState->VertexState.WorldMatrix.Values[0] == float.NaN)
					{
						throw (new Exception("Invalid WorldMatrix"));
					}

					//GpuState->VertexState.ViewMatrix.Dump();
					//GpuState->VertexState.WorldMatrix.Dump();

					//Console.WriteLine("NO Transform2D");
				}
			}

			//GL.Enable(EnableCap.Blend);

			/*
			if (CurrentTexture != null)
			{
				if (CurrentTexture.TextureHash == 0x2202293873)
				{
					Console.Error.WriteLine(CurrentTexture);
					Console.Error.WriteLine(VertexCount);
				}
			}
			*/

			// DRAW ACTUALLY
			{
				uint VertexSize = GpuState->VertexState.Type.GetVertexSize();

				byte* VertexPtr = (byte*)Memory.PspAddressToPointerSafe(GpuState->VertexAddress);

				//Console.WriteLine(VertexSize);

				var BeginMode = default(BeginMode);

				switch (PrimitiveType)
				{
					case PrimitiveType.Lines: BeginMode = BeginMode.Lines; break;
					case PrimitiveType.LineStrip: BeginMode = BeginMode.LineStrip; break;
					case PrimitiveType.Triangles: BeginMode = BeginMode.Triangles; break;
					case PrimitiveType.Points: BeginMode = BeginMode.Points; break;
					case PrimitiveType.TriangleFan: BeginMode = BeginMode.TriangleFan; break;
					case PrimitiveType.TriangleStrip: BeginMode = BeginMode.TriangleStrip; break;
					case PrimitiveType.Sprites: BeginMode = BeginMode.Quads; break;
					default: throw (new NotImplementedException("Not implemented PrimitiveType:'" + PrimitiveType + "'"));
				}

				//Console.WriteLine(BeginMode);

				//lock (GpuLock)
				{
					//Console.Error.WriteLine("GL.Begin : Thread : {0}", Thread.CurrentThread.ManagedThreadId);
					GL.Begin(BeginMode);
					try
					{
						if (PrimitiveType == PrimitiveType.Sprites)
						{
							GL.Disable(EnableCap.CullFace);
							for (int n = 0; n < VertexCount; n += 2)
							{
								VertexInfo VertexInfoTopLeft;
								VertexInfo VertexInfoTopRight;
								VertexInfo VertexInfoBottomRight;
								VertexInfo VertexInfoBottomLeft;
								ReadVertex(n + 0, &VertexInfoTopLeft);
								ReadVertex(n + 1, &VertexInfoBottomRight);

								//if (GpuState->ClearingMode) Console.WriteLine("{0} - {1}", VertexInfoTopLeft, VertexInfoBottomRight);

								float R = VertexInfoBottomRight.R, G = VertexInfoBottomRight.G, B = VertexInfoBottomRight.B, A = VertexInfoBottomRight.A;
								float PZ = VertexInfoTopLeft.PZ;
								float NZ = VertexInfoTopLeft.NZ;

								VertexInfoTopRight = new VertexInfo()
								{
									U = VertexInfoBottomRight.U,
									V = VertexInfoTopLeft.V,
									PX = VertexInfoBottomRight.PX,
									PY = VertexInfoTopLeft.PY,
									NX = VertexInfoBottomRight.NX,
									NY = VertexInfoTopLeft.NY,
								};

								VertexInfoBottomLeft = new VertexInfo()
								{
									U = VertexInfoTopLeft.U,
									V = VertexInfoBottomRight.V,
									PX = VertexInfoTopLeft.PX,
									PY = VertexInfoBottomRight.PY,
									NX = VertexInfoTopLeft.NX,
									NY = VertexInfoBottomRight.NY,
								};

								VertexInfoBottomLeft.R = VertexInfoBottomRight.R = VertexInfoTopRight.R = VertexInfoTopLeft.R = R;
								VertexInfoBottomLeft.G = VertexInfoBottomRight.G = VertexInfoTopRight.G = VertexInfoTopLeft.G = G;
								VertexInfoBottomLeft.B = VertexInfoBottomRight.B = VertexInfoTopRight.B = VertexInfoTopLeft.B = B;
								VertexInfoBottomLeft.A = VertexInfoBottomRight.A = VertexInfoTopRight.A = VertexInfoTopLeft.A = R;
								VertexInfoBottomLeft.PZ = VertexInfoBottomRight.PZ = VertexInfoTopRight.PZ = VertexInfoTopLeft.PZ = PZ;
								VertexInfoBottomLeft.NZ = VertexInfoBottomRight.NZ = VertexInfoTopRight.NZ = VertexInfoTopLeft.NZ = NZ;

								PutVertex(ref VertexInfoTopLeft, ref VertexType);
								PutVertex(ref VertexInfoTopRight, ref VertexType);
								PutVertex(ref VertexInfoBottomRight, ref VertexType);
								PutVertex(ref VertexInfoBottomLeft, ref VertexType);
							}
						}
						else
						{
							VertexInfo VertexInfo;
							for (int n = 0; n < VertexCount; n++)
							{
								ReadVertex(n, &VertexInfo);
								PutVertex(ref VertexInfo, ref VertexType);

							}
						}
					}
					finally
					{
						GL.End();
						GL.Flush();
					}
				}
			}
			//Console.WriteLine(VertexCount);

			//PrepareWrite(GpuState);
		}

		static private bool GlEnableDisable(EnableCap EnableCap, bool EnableDisable)
		{
			if (EnableDisable)
			{
				GL.Enable(EnableCap);
			}
			else
			{
				GL.Disable(EnableCap);
			}
			return EnableDisable;
		}

		readonly byte[] TempBuffer = new byte[512 * 512 * 4];

		/*
		 * 
		struct GlPixelFormat {
			PixelFormats pspFormat;
			float size;
			uint  internal;
			uint  external;
			uint  opengl;
			uint  isize() { return cast(uint)size; }
		}

		static const auto GlPixelFormats = [
			GlPixelFormat(PixelFormats.GU_PSM_5650,   2, 3, GL_RGB,  GL_UNSIGNED_SHORT_5_6_5_REV),
			GlPixelFormat(PixelFormats.GU_PSM_5551,   2, 4, GL_RGBA, GL_UNSIGNED_SHORT_1_5_5_5_REV),
			GlPixelFormat(PixelFormats.GU_PSM_4444,   2, 4, GL_RGBA, GL_UNSIGNED_SHORT_4_4_4_4_REV),
			GlPixelFormat(PixelFormats.GU_PSM_8888,   4, 4, GL_RGBA, GL_UNSIGNED_INT_8_8_8_8_REV),
			GlPixelFormat(PixelFormats.GU_PSM_T4  , 0.5, 1, GL_COLOR_INDEX, GL_COLOR_INDEX4_EXT),
			GlPixelFormat(PixelFormats.GU_PSM_T8  ,   1, 1, GL_COLOR_INDEX, GL_COLOR_INDEX8_EXT),
			GlPixelFormat(PixelFormats.GU_PSM_T16 ,   2, 4, GL_COLOR_INDEX, GL_COLOR_INDEX16_EXT),
			GlPixelFormat(PixelFormats.GU_PSM_T32 ,   4, 4, GL_RGBA, GL_UNSIGNED_INT ), // COLOR_INDEX, GL_COLOR_INDEX32_EXT Not defined.
			GlPixelFormat(PixelFormats.GU_PSM_DXT1,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT1_EXT),
			GlPixelFormat(PixelFormats.GU_PSM_DXT3,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT3_EXT),
			GlPixelFormat(PixelFormats.GU_PSM_DXT5,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT5_EXT),
		];
		*/

		public struct GlPixelFormat
		{
			public GuPixelFormats GuPixelFormat;
			public PixelType OpenglPixelType;
		};

		static readonly public GlPixelFormat[] GlPixelFormatList = new GlPixelFormat[]
		{
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_5650, OpenglPixelType = PixelType.UnsignedShort565Reversed },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_5551, OpenglPixelType = PixelType.UnsignedShort1555Reversed },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_4444, OpenglPixelType = PixelType.UnsignedShort4444Reversed },
			new GlPixelFormat() { GuPixelFormat = GuPixelFormats.RGBA_8888, OpenglPixelType = PixelType.UnsignedInt8888Reversed },
		};

		[HandleProcessCorruptedStateExceptions()]
		private void PrepareRead(GpuStateStruct* GpuState)
		{
#if true
//#else
			var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
			int Width = (int)GpuState->DrawBufferState.Width;
			int Height = 272;
			int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
			int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
			//GpuState->DrawBufferState.Format
			var Address = (void*)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address);
			GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);
			//Console.WriteLine("PrepareRead: {0:X}", Address);

			try
			{
				GL.WindowPos2(0, 272);
				GL.PixelZoom(1, -1);

				GL.DrawPixels(512, 272, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(Address));
				//GL.DrawPixels(512, 272, PixelFormat.AbgrExt, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));

				//GL.WindowPos2(0, 0);
				//GL.PixelZoom(1, 1);
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
#endif
		}

		[HandleProcessCorruptedStateExceptions()]
		private void PrepareWrite(GpuStateStruct* GpuState)
		{
			//Console.WriteLine("PrepareWrite");
			try
			{
				var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
				int Width = (int)GpuState->DrawBufferState.Width;
				int Height = 272;
				int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
				int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
				//GpuState->DrawBufferState.Format
				var Address = (void *)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address);

				//Console.WriteLine("{0}", GlPixelFormat.GuPixelFormat);

				//Console.WriteLine("{0:X}", GpuState->DrawBufferState.Address);
				GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);

#if false
				//GL.WindowPos2(0, 272);
				//GL.PixelZoom(1, -1);

				GL.WindowPos2(0, 0);
				GL.PixelZoom(1, 1);

				GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(Address));
#else
				GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, TempBuffer);
				fixed (void* _TempBufferPtr = &TempBuffer[0])
				{
					var Input = (byte*)_TempBufferPtr;
					var Output = (byte*)Address;

					for (int Row = 0; Row < Height; Row++)
					{
						var ScanIn = (byte *)&Input[ScanWidth * Row];
						var ScanOut = (byte*)&Output[ScanWidth * (Height - Row - 1)];
						//Console.WriteLine("{0}:{1},{2},{3}", Row, PixelSize, Width, ScanWidth);
						PointerUtils.Memcpy(ScanOut, ScanIn, ScanWidth);
					}
				}
#endif
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
		}

		[HandleProcessCorruptedStateExceptions()]
		public override void Finish(GpuStateStruct* GpuState)
		{
			//return;
			/*
			if (GpuState->DrawBufferState.LowAddress != 0)
			{
				//var Address = PspMemory.FrameBufferOffset | GpuState->DrawBufferState.LowAddress;
				var Address = GpuState->DrawBufferState.Address;
				try
				{
					Console.WriteLine("{0:X}", Address);
					Memory.CheckAndEnforceAddressValid(Address);
					GL.ReadPixels(0, 0, 512, 272, PixelFormat.Rgba, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));
				}
				catch (Exception Exception)
				{
					// 0x04000000
					Console.WriteLine("Address: {0:X}", Address);
					Console.WriteLine(Exception);
					//throw(Exception);
				}
			}
			*/
		}

		public override void End(GpuStateStruct* GpuState)
		{
			PrepareWrite(GpuState);
			//throw new NotImplementedException();
		}

		public override void TextureFlush(GpuStateStruct* GpuState)
		{
			TextureCache.RecheckAll();
			//Console.WriteLine("TextureFlush!");
			//base.TextureFlush(GpuState);
		}

		public override void TextureSync(GpuStateStruct* GpuState)
		{
			//Console.WriteLine("TextureSync!");
			//base.TextureSync(GpuState);
		}

		public override void AddedDisplayList()
		{
			//TextureCache.RecheckAll();
			//throw new NotImplementedException();
		}

		private void TransferToFrameBuffer(GpuStateStruct* GpuState)
		{
			var TextureTransferState = GpuState->TextureTransferState;

			var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];

			GL.PixelZoom(1, -1);
			GL.WindowPos2(TextureTransferState.DestinationX, 272 - TextureTransferState.DestinationY);
			//GL.PixelZoom(1, -1);
			//GL.PixelZoom(1, 1);
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, TextureTransferState.BytesPerPixel);
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, TextureTransferState.SourceLineWidth);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, TextureTransferState.SourceX);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, TextureTransferState.SourceY);

			GL.DrawPixels(
				TextureTransferState.Width,
				TextureTransferState.Height,
				PixelFormat.Rgba,
				GlPixelFormat.OpenglPixelType,
				new IntPtr(Memory.PspAddressToPointerSafe(TextureTransferState.SourceAddress))
			);

			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
			GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
			GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
		}

		private void TransferGeneric(GpuStateStruct* GpuState)
		{
			var TextureTransferState = GpuState->TextureTransferState;

			var SourcePointer = (byte*)Memory.PspAddressToPointer(TextureTransferState.SourceAddress.Address);
			var DestinationPointer = (byte*)Memory.PspAddressToPointer(TextureTransferState.DestinationAddress.Address);
			var SourceX = TextureTransferState.SourceX;
			var SourceY = TextureTransferState.SourceY;
			var DestinationX = TextureTransferState.DestinationX;
			var DestinationY = TextureTransferState.DestinationY;
			var BytesPerPixel = TextureTransferState.BytesPerPixel;

			for (uint y = 0; y < TextureTransferState.Height; y++)
			{
				var RowSourceOffset = (uint)(
					(TextureTransferState.SourceLineWidth * (y + SourceY)) + SourceX
				);
				var RowDestinationOffset = (uint)(
					(TextureTransferState.DestinationLineWidth * (y + DestinationY)) + DestinationX
				);
				PointerUtils.Memcpy(
					DestinationPointer + RowDestinationOffset * BytesPerPixel,
					SourcePointer + RowSourceOffset * BytesPerPixel,
					TextureTransferState.Width * BytesPerPixel
				);
			}

			/*
			// Generic implementation.
			with (gpu.state.textureTransfer) {
				auto srcAddressHost = cast(ubyte*)gpu.memory.getPointer(srcAddress);
				auto dstAddressHost = cast(ubyte*)gpu.memory.getPointer(dstAddress);

				if (gpu.state.drawBuffer.isAnyAddressInBuffer([srcAddress, dstAddress])) {
					gpu.performBufferOp(BufferOperation.STORE, BufferType.COLOR);
				}

				for (int n = 0; n < height; n++) {
					int srcOffset = ((n + srcY) * srcLineWidth + srcX) * bpp;
					int dstOffset = ((n + dstY) * dstLineWidth + dstX) * bpp;
					(dstAddressHost + dstOffset)[0.. width * bpp] = (srcAddressHost + srcOffset)[0.. width * bpp];
					//writefln("%08X <- %08X :: [%d]", dstOffset, srcOffset, width * bpp);
				}
				//std.file.write("buffer", dstAddressHost[0..512 * 272 * 4]);
			
				if (gpu.state.drawBuffer.isAnyAddressInBuffer([dstAddress])) {
					//gpu.impl.test();
					//gpu.impl.test("trxkick");
					gpu.markBufferOp(BufferOperation.LOAD, BufferType.COLOR);
				}
				//gpu.impl.test();
			}
			*/
		}

		public override void Transfer(GpuStateStruct* GpuState)
		{
			//return;
			var TextureTransferState = GpuState->TextureTransferState;

			if (
				(TextureTransferState.DestinationAddress.Address == GpuState->DrawBufferState.Address) &&
				(TextureTransferState.DestinationLineWidth == GpuState->DrawBufferState.Width) &&
				(TextureTransferState.BytesPerPixel == GpuState->DrawBufferState.BytesPerPixel)
			)
			{
				//Console.Error.WriteLine("Writting to DrawBuffer");
				TransferToFrameBuffer(GpuState);
			}
			else
			{
				Console.Error.WriteLine("NOT Writting to DrawBuffer");
				TransferGeneric(GpuState);
				/*
				base.Transfer(GpuStateStruct);
				PrepareWrite(GpuStateStruct);
				{

				}
				PrepareRead(GpuStateStruct);
				*/
			}
			Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!! : {0}", GpuState->TextureTransferState.ToStringDefault());
		}
	}
}
