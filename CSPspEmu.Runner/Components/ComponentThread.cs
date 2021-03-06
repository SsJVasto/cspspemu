﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpUtils.Threading;
using CSPspEmu.Core;
using CSharpUtils;

namespace CSPspEmu.Runner.Components
{
	abstract public class ComponentThread : PspEmulatorComponent, IRunnableComponent
	{
		static Logger Logger = Logger.GetLogger("ComponentThread");

		protected AutoResetEvent RunningUpdatedEvent = new AutoResetEvent(false);
		public bool Running = true;

		protected Thread ComponentThreadThread;
		protected AutoResetEvent StopCompleteEvent = new AutoResetEvent(false);
		protected AutoResetEvent PauseEvent = new AutoResetEvent(false);
		protected AutoResetEvent ResumeEvent = new AutoResetEvent(false);

		readonly public TaskQueue ThreadTaskQueue = new TaskQueue();
		abstract protected String ThreadName { get; }

		public void StartSynchronized()
		{
			Logger.Notice("Component {0} StartSynchronized!", this);
			var ElapsedTime = Logger.Measure(() =>
			{
				ComponentThreadThread = new Thread(() =>
				{
					ComponentThreadThread.Name = this.ThreadName;
					Thread.CurrentThread.CurrentCulture = new CultureInfo(PspConfig.CultureName);
					try
					{
						Main();
					}
					finally
					{
						Running = false;
						RunningUpdatedEvent.Set();
						StopCompleteEvent.Set();
						Logger.Notice("Component {0} Stopped!", this);
					}
				});
				ComponentThreadThread.Start();
				ThreadTaskQueue.EnqueueAndWaitCompleted(() =>
				{
				});
			});
			Logger.Notice("Component {0} Started! StartedTime({1})", this, ElapsedTime.TotalSeconds);
		}

		public void StopSynchronized()
		{
			Logger.Notice("Component {0} StopSynchronized...", this);
			var ElapsedTime = Logger.Measure(() =>
			{
				if (Running)
				{
					StopCompleteEvent.Reset();
					{
						Running = false;
						RunningUpdatedEvent.Set();
					}
					if (!StopCompleteEvent.WaitOne(1000))
					{
						Logger.Error("Error stopping {0}", this);
						ComponentThreadThread.Abort();
					}
				}
			});
			Logger.Notice("Stopped! {0}", ElapsedTime);
		}

		public void PauseSynchronized()
		{
			Logger.Notice("Component {0} PauseSynchronized!", this);

			ThreadTaskQueue.EnqueueAndWaitStarted(() =>
			{
				while (!PauseEvent.WaitOne(TimeSpan.FromMilliseconds(10)))
				{
					if (!Running) break;
				}
			});
		}

		public void ResumeSynchronized()
		{
			Logger.Notice("Component {0} ResumeSynchronized!", this);

			PauseEvent.Set();
		}

		abstract protected void Main();
	}
}
