using System;
using System.Collections.Concurrent;
using System.Threading;
using GeneXus.Application;
using GeneXus.Procedure;
using GeneXus.Utils;
using Xunit;

namespace UnitTesting
{
	public class SubmitTest
	{
		internal static ConcurrentDictionary<Guid, int> numbers = new ConcurrentDictionary<Guid, int>();
		[Fact]
		public void SubmitAll()
		{
			MainProc proc = new MainProc();
			proc.execute();
		}
	}
	public class MainProc : GXProcedure
	{
		const int THREAD_NUMBER = 2000;
		public MainProc()
		{
			context = new GxContext();
			IsMain = true;
		}
		public void execute()
		{
			initialize();
			executePrivate();
		}

		void executePrivate()
		{
			for (int i = 0; i < THREAD_NUMBER; i++)
			{
				SubmitProc proc = new SubmitProc();
				proc.executeSubmit();
			}
			cleanup();
			ThreadUtil.WaitForEnd();//Force WaitForEnd since tests run in web context when running in parallel with Middleware tests
			Assert.Equal(THREAD_NUMBER, SubmitTest.numbers.Count);
		}
		public override void cleanup()
		{
			ExitApp();
		}

		public override void initialize()
		{

		}
	}
	public class SubmitProc : GXProcedure
	{
		public SubmitProc()
		{
			context = new GxContext();
		}
		public void executeSubmit()
		{
			SubmitProc submitProc;
			submitProc = new SubmitProc();
			submitProc.context.SetSubmitInitialConfig(context);
			submitProc.initialize();
			Submit(executePrivateCatch, submitProc);
		}
		void executePrivateCatch(object stateInfo)
		{
			((SubmitProc)stateInfo).executePrivate();
		}
		void executePrivate()
		{
			int millisecondsToWait = (int)ThreadSafeRandom.NextDouble()*10;
			Thread.Sleep(millisecondsToWait);
			SubmitTest.numbers[Guid.NewGuid()]=Thread.CurrentThread.ManagedThreadId;
		}

		public override void initialize()
		{

		}
	}
}
