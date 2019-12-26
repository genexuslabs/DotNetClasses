using System;
using System.Collections;
using System.Reflection;
using System.Threading;

namespace GeneXus.Mail.Util
{
	public class AsyncCallback
	{
		private object callbackObject;
		private string methodName;
		private bool asynch;

		public AsyncCallback(object callbackObject, string methodName) : this(callbackObject, methodName, true) {}

		public AsyncCallback(object callbackObject, string methodName, bool asynch)
		{
			this.callbackObject = callbackObject;
			this.methodName = methodName;
			this.asynch = asynch;
		}

		public object CallbackObject
		{
			get
			{
				return callbackObject;
			}
		}

		public string MethodName
		{
			get
			{
				return methodName;
			}
		}

		public bool Asynch
		{
			get
			{
				return asynch;
			}
		}
	}

	public class AsyncRunner
	{
		private object obj;
		private string methodName;
		private object[] parms;
		private AsyncCallback callback;
		private int timeout;
		private Thread thread;
		private Timer timeoutTimer;
		private bool executingCallback;

		public AsyncRunner(object obj, string methodName)
		{
			this.obj = obj;
			this.methodName = methodName;
			this.parms = Array.Empty<object>();
			this.callback = null;
			this.timeout = 0;
			this.thread = null;
			this.timeoutTimer = null;
			this.executingCallback = false;
		}

		public AsyncRunner(object obj, string methodName, object[] parms) : this(obj, methodName)
		{
			this.parms = parms;
		}

		public AsyncRunner(object obj, string methodName, object[] parms, AsyncCallback callback) : this(obj, methodName, parms)
		{
			this.callback = callback;
		}

		public AsyncRunner(object obj, string methodName, object[] parms, AsyncCallback callback, int timeout) : this(obj, methodName, parms, callback)
		{
			this.timeout = timeout;
		}

		public void Run()
		{
			thread = new Thread(new ThreadStart(Execute));
			thread.Name = "GeneXus Asynch Mail Reader";
			thread.Priority = ThreadPriority.Highest;
			thread.Start();
		}

		public void Abort()
		{
			try
			{
				thread.Abort();
			}
			catch {}
		}

		private void Execute()
		{
			try
			{
				MethodInfo[] methods = obj.GetType().GetMethods();
				for(int i=0; i<methods.Length; i++)
				{
					MethodInfo method = methods[i];
					if(method.Name.Equals(methodName))
					{
						if (!this.executingCallback)
						{
							StartTimer();
						}
						else
						{
							StopTimer();
						}
						method.Invoke(obj, parms);
						if (!this.executingCallback)
						{
							StopTimer();
						}
						DoCallback(null);
						break;
					}
				}
			}
			catch(Exception ex)
			{
				AbortAndCallback(ex);
			}
		}

		private void StartTimer()
		{
			if (timeout > 0)
			{
				timeoutTimer = new Timer(new TimerCallback(AbortAndCallback), null, timeout, timeout);
			}
		}

		public void ResetTimer()
		{
			if (timeout > 0 && timeoutTimer != null)
			{
				timeoutTimer.Change(timeout, timeout);
			}
		}

		private void StopTimer()
		{
			if (timeoutTimer != null)
			{
				timeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
				timeoutTimer.Dispose();
				timeoutTimer = null;
			}
		}

		private void AbortAndCallback(object parm)
		{
			Abort();
			Exception inner = null;
			if (parm is Exception)
			{
				inner = (Exception)parm;
				if (inner.InnerException != null)
					inner = inner.InnerException;
			}
			DoCallback(inner);
		}

		private void DoCallback(object retValue)
		{
			if(callback != null)
			{
				if (callback.Asynch)
				{
					new AsyncRunner(callback.CallbackObject, callback.MethodName, new object[] { retValue }).Run();
				}
				else
				{
					this.obj = callback.CallbackObject;
					this.methodName = callback.MethodName;
					this.parms = new object[] { retValue };
					this.callback = null;
					this.executingCallback = true;
					Execute();
					this.executingCallback = false;
				}
			}
		}
	}
}
