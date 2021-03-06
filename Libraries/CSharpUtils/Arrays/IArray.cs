﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils.Arrays
{
	public interface IArray<TType> : IEnumerable<TType>
	{
		TType this[int Index] { get; set; }
		int Length { get; }
		TType[] GetArray();
	}
}
