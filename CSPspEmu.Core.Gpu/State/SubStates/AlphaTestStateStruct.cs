﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Gpu.State.SubStates
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AlphaTestStateStruct
	{
		/// <summary>
		/// Alpha Test Enable (GL_ALPHA_TEST) glAlphaFunc(GL_GREATER, 0.03f);
		/// </summary>
		public bool Enabled;
		
		/// <summary>
		/// TestFunction.GU_ALWAYS
		/// </summary>
		public TestFunctionEnum Function;

		/// <summary>
		/// 
		/// </summary>
		public float Value;

		/// <summary>
		/// 0xFF
		/// </summary>
		public byte Mask;
	}
}
