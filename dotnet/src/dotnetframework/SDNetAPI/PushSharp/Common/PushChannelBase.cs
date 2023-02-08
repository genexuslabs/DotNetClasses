using System;
using System.Collections.Generic;

using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Threading;

using System.Net;
using System.ComponentModel;
using log4net;

namespace PushSharp.Common
{
	public abstract class PushChannelBase : IDisposable
	{
        public static readonly ILog log = log4net.LogManager.GetLogger(typeof(PushChannelBase));

		public ChannelEvents Events = new ChannelEvents();
		
		public PushChannelSettings ChannelSettings { get; private set; }
		public PushServiceSettings ServiceSettings { get; private set; }

		internal event Action<double> OnQueueTimed;

		Queue<Notification> queuedNotifications;
		
		protected bool stopping;
        protected Thread taskSender;
		//protected CancellationTokenSource CancelTokenSource;
		//protected CancellationToken CancelToken;
        protected bool CancellationRequested;
		protected abstract void SendNotification(Notification notification);

        public PushChannelBase(PushChannelSettings channelSettings): this(channelSettings,null)
        {
        }
		public PushChannelBase(PushChannelSettings channelSettings, PushServiceSettings serviceSettings)
		{
			this.stopping = false;
			//this.CancelTokenSource = new CancellationTokenSource();
			//this.CancelToken = CancelTokenSource.Token;

			this.queuedNotifications = new Queue<Notification>();
		    
			this.ChannelSettings = channelSettings;
			this.ServiceSettings = serviceSettings ?? new PushServiceSettings();

			//Start our sending task
            taskSender = new Thread(new ThreadStart(Sender));
            taskSender.Start();
            
            
		}

		public virtual void Stop(bool waitForQueueToDrain)
		{
			stopping = true;

           
			//See if we want to wait for the queue to drain before stopping
			if (waitForQueueToDrain)
			{
                int slept = 0;
                int maxSleep = 10000; //10 seconds

                while (QueuedNotificationCount > 0 && slept <= maxSleep)
                {
                    Thread.Sleep(100);
                    slept += 100;
                }
                while (QueuedNotificationCount > 0 || PendingNotificationsResult > 0)
					Thread.Sleep(50);
			}

			//Sleep a bit to prevent any race conditions
			Thread.Sleep(2000);

		//	if (!CancelTokenSource.IsCancellationRequested)
			//	CancelTokenSource.Cancel();

			//Wait on our tasks for a maximum of 30 seconds

            taskSender.Join(15000);
#if !NETCOREAPP1_1
			taskSender.Abort();            
#endif
			//sk.WaitAll(new Task[] { taskSender }, 30000);
		}

		public virtual void Dispose()
		{
			//Stop without waiting
			if (!stopping)
				Stop(false);
		}

		public int QueuedNotificationCount
		{
            get { lock (queuedNotifications) { return queuedNotifications.Count; } }
		}
        public int PendingNotificationsResult;

        public void QueueNotification(Notification notification)
        {
            QueueNotification( notification, true);
        }
		public void QueueNotification(Notification notification, bool countsAsRequeue)
		{
			//if (this.CancelToken.IsCancellationRequested)
			//	throw new ObjectDisposedException("Channel", "Channel has already been signaled to stop");

			//If the count is -1, it can be queued infinitely, otherwise check that it's less than the max
			if (this.ServiceSettings.MaxNotificationRequeues < 0 || notification.QueuedCount <= this.ServiceSettings.MaxNotificationRequeues)
			{
				//Increase the queue counter
				if (countsAsRequeue)
					notification.QueuedCount++;
                lock (queuedNotifications)
                {
				    queuedNotifications.Enqueue(notification);
                    Interlocked.Increment(ref PendingNotificationsResult);
                }
			}
			else
				Events.RaiseNotificationSendFailure(notification, new MaxSendAttemptsReachedException());
		}

		void Sender()
		{
			while (!this.CancellationRequested || QueuedNotificationCount > 0)
			{
                Notification notification = null;
                lock (this.queuedNotifications)
                {
                    if (this.queuedNotifications.Count > 0)
                        notification = queuedNotifications.Dequeue();

                }
                if (notification == null)
                {
                    //No notifications in queue, sleep a bit!
                    Thread.Sleep(250);
                    continue;
                }

				//Report back the time in queue
				var timeInQueue = DateTime.UtcNow - notification.EnqueuedTimestamp;
				if (OnQueueTimed != null)
					OnQueueTimed(timeInQueue.TotalMilliseconds);

				//Send it
				this.SendNotification(notification);
			}
           
		}

	}
}
