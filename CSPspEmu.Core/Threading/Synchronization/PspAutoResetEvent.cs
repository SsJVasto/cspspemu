﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Threading.Synchronization
{
	public class PspAutoResetEvent : PspResetEvent
	{
		public PspAutoResetEvent(bool InitialValue)
			: base(InitialValue, AutoReset: true)
		{
		}
	}
}
