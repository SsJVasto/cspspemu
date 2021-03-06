﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace CSharpUtils
{
	sealed public class Logger
	{
		public enum Level
		{
			Notice,
			Info,
			Warning,
			Unimplemented,
			Error,
			Fatal,
		}

		public string Name { get; private set; }
		public bool Enabled;
		static Dictionary<string, Logger> Loggers = new Dictionary<string, Logger>();

		internal Logger()
		{
		}

		static public Logger GetLogger(string Name)
		{
			lock (Loggers)
			{
				if (!Loggers.ContainsKey(Name))
				{
					Loggers[Name] = new Logger()
					{
						Name = Name,
					};
				}

				return Loggers[Name];
			}
		}

		static public event Action<string, Level, string, StackFrame> OnGlobalLog;

		public event Action<Level, string, StackFrame> OnLog;

		private Logger Log(Level Level, object Format, params object[] Params)
		{
			if (Enabled || OnGlobalLog != null)
			{
				StackTrace stackTrace = new StackTrace();
				StackFrame StackFrame = null;
				foreach (var Frame in stackTrace.GetFrames())
				{
					if (Frame.GetMethod().DeclaringType != typeof(Logger))
					{
						StackFrame = Frame;
						break;
					}
				}

				if (Enabled)
				{
					if (OnLog != null) OnLog(Level, String.Format(Format.ToString(), Params), StackFrame);
				}

				if (OnGlobalLog != null)
				{
					OnGlobalLog(Name, Level, String.Format(Format.ToString(), Params), StackFrame);
				}
			}

			return this;
		}

		public Logger Notice(object Format, params object[] Params) { return Log(Level.Notice, Format, Params); }
		public Logger Info(object Format, params object[] Params) { return Log(Level.Info, Format, Params); }
		public Logger Warning(object Format, params object[] Params) { return Log(Level.Warning, Format, Params); }
		public Logger Unimplemented(object Format, params object[] Params) { return Log(Level.Unimplemented, Format, Params); }
		public Logger Error(object Format, params object[] Params) { return Log(Level.Error, Format, Params); }
		public Logger Fatal(object Format, params object[] Params) { return Log(Level.Fatal, Format, Params); }

		static public TimeSpan Measure(Action Action)
		{
			var Start = DateTime.UtcNow;
			Action();
			var End = DateTime.UtcNow;
			return End - Start;
		}

		public class Stopwatch
		{
			List<DateTime> DateTimeList = new List<DateTime>();

			public void Tick()
			{
				DateTimeList.Add(DateTime.UtcNow);
			}

			public override string ToString()
			{
				var TimeSpans = new List<TimeSpan>();
				for (int n = 1; n < DateTimeList.Count; n++)
				{
					TimeSpans.Add(DateTimeList[n] - DateTimeList[n - 1]);
				}

				return String.Format(
					"Logger.Stopwatch({0})",
					String.Join(",", TimeSpans.Select(Item => String.Format(
						"{0} ms",
						(int)Item.TotalMilliseconds
					)))
				);
			}
		}
	}
}
