﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core
{
	public class PspGlobalConfiguration
	{
		static private string _CurrentVersion;

		static public string CurrentVersion
		{
			get
			{
				if (_CurrentVersion == null)
				{
					_CurrentVersion = Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.version_current.txt").ReadAllContentsAsString();
				}
				return _CurrentVersion;
			}
		}

		static private string _GitRevision;

		static public string GitRevision
		{
			get
			{
				if (_GitRevision == null)
				{
					_GitRevision = Assembly.GetEntryAssembly().GetManifestResourceStream("CSPspEmu.git_revision.txt").ReadAllContentsAsString();
				}
				return _GitRevision;
			}
		}
	}
}
