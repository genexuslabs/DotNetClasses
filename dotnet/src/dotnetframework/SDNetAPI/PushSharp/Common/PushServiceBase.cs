using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PushSharp.Common
{
	public abstract class PushServiceBase<TChannelSettings> : IDisposable where TChannelSettings : PushChannelSettings
	{
		public ChannelEvents Events = new ChannelEvents();

		public abstract PlatformType Platform { get; }
        public int PendingNotificationsToProcess { get; set; }
		public PushServiceSettings ServiceSettings { get; private set; }
		public TChannelSettings ChannelSettings { get; private set; }
		public bool IsStopping { get; private set; }

		List<PushChannelBase> channels = new List<PushChannelBase>();
		Queue<Notification> queuedNotifications = new Queue<Notification>();
		//CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private bool CancellationRequested;
		List<double> measurements = new List<double>();

		protected abstract PushChannelBase CreateChannel(PushChannelSettings channelSettings);
        Thread distributerThread;

        public PushServiceBase(TChannelSettings channelSettings): this(channelSettings,null)
        {
        }
		public PushServiceBase(TChannelSettings channelSettings, PushServiceSettings serviceSettings)
		{
			this.ServiceSettings = serviceSettings ?? new PushServiceSettings();
			this.ChannelSettings = channelSettings;

			this.queuedNotifications = new Queue<Notification>();

			/*timerCheckScale = new Timer(new TimerCallback((state) =>
			{
				CheckScale();
			}), null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));*/
			
			CheckScale();            
            distributerThread = new Thread(new ThreadStart(Distributer));
			//distributerTask = new Task(Distributer, TaskCreationOptions.LongRunning);
			//distributerTask.ContinueWith((ft) =>
			//{
			//	var ex = ft.Exception;

	//		}, TaskContinuationOptions.OnlyOnFaulted);
		//	distributerTask.Start();

            distributerThread.Start();
			IsStopping = false;
            
		}

		public void QueueNotification(Notification notification)
		{
            lock (queuedNotifications)
            {
                PendingNotificationsToProcess++;
                queuedNotifications.Enqueue(notification);
            }
		}

		public void Stop(bool waitForQueueToFinish)
		{
			IsStopping = true;

            while (PendingNotificationsToProcess > 0)
            {
                Thread.Sleep(1000);
            }
#if !NETCOREAPP1_1
			distributerThread.Abort();
#endif
            
			//Stop all channels
            lock (channels)
            {
                foreach (PushChannelBase channel in channels)
                {
                    channel.Stop(waitForQueueToFinish);
                    channel.Events.UnRegisterProxyHandler(this.Events);   
                }
			    this.channels.Clear();
            }
			
			//Sleep a bit to avoid race conditions
			Thread.Sleep(2000);

			//this.cancelTokenSource.Cancel();
            this.CancellationRequested = true;
		}

		public void Dispose()
		{
			if (!IsStopping)
				Stop(false);
		}

		void Distributer()
		{
			while (!this.CancellationRequested)
			{
				if (channels == null || channels.Count <= 0)
				{
					Thread.Sleep(150);
					continue;
				}
                
                Notification notification = null;
                lock (this.queuedNotifications)
                {
                    if (this.queuedNotifications.Count > 0)
                        notification = queuedNotifications.Dequeue();

                }
                if (notification == null)
				{
					//No notifications in queue, sleep a bit!
					Thread.Sleep(150);
					continue;
				}

				PushChannelBase channelOn = null;

				//Get the channel with the smallest queue                             
                if (channels.Count == 1)
					channelOn = channels[0];
				else
					channelOn = (from c in channels
								 orderby c.QueuedNotificationCount
								 select c).FirstOrDefault();
                
				if (channelOn != null)
				{
					//Measure when the message entered the queue
					notification.EnqueuedTimestamp = DateTime.UtcNow;

					channelOn.QueueNotification(notification);
                    PendingNotificationsToProcess--;
				}
			}
		}

		void CheckScale()
		{
			if (ServiceSettings.AutoScaleChannels)
			{
				if (channels.Count <= 0)
				{
					SpinupChannel();
					return;
				}

				var avgTime = GetAverageQueueWait();                
				if (avgTime < 1 && channels.Count > 1)
				{
					TeardownChannel();
				}
				else if (avgTime > 5 && channels.Count < this.ServiceSettings.MaxAutoScaleChannels)
				{
					var numChannelsToSpinUp = 1;

					//Depending on the wait time, let's spin up more than 1 channel at a time
					if (avgTime > 500)
						numChannelsToSpinUp = 19;
					else if (avgTime > 250)
						numChannelsToSpinUp = 10;
					else if (avgTime > 100)
						numChannelsToSpinUp = 5;

					for (int i = 0; i < numChannelsToSpinUp; i++)
						if (channels.Count < this.ServiceSettings.MaxAutoScaleChannels)
							SpinupChannel();
				}
			}
			else
			{
				while (channels.Count > ServiceSettings.Channels)
					TeardownChannel();

				while (channels.Count < ServiceSettings.Channels)
					SpinupChannel();
			}
		}

		void newChannel_OnQueueTimed(double queueTimeMilliseconds)
		{
			//We got a measurement for how long a message waited in the queue
			lock (measurements)
			{
				measurements.Add(queueTimeMilliseconds);

				while (measurements.Count > 1000)
					measurements.RemoveAt(0);
			}				
		}

		double GetAverageQueueWait()
		{
			if (measurements == null)
				return 0;

			lock (measurements)
			{
				if (measurements.Count > 0)
					return measurements.Average();
				else
					return 0;
			}
		}

		void SpinupChannel()
		{
			lock (channels)
			{
				var newChannel = this.CreateChannel(this.ChannelSettings);

				newChannel.Events.RegisterProxyHandler(this.Events);

				newChannel.OnQueueTimed += new Action<double>(newChannel_OnQueueTimed);

				channels.Add(newChannel);
			}
		}

		void TeardownChannel()
		{
			lock (channels)
			{
				var channelOn = channels[0];
				channels.RemoveAt(0);

				channelOn.Events.UnRegisterProxyHandler(this.Events);
			}
		}

	}
}
