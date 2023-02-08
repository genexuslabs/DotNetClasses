using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushSharp.Common;


namespace PushSharp
{
#if !NETCOREAPP1_1
	public class PushService : IDisposable
	{
		public ChannelEvents Events;

		public bool WaitForQueuesToFinish { get; set; }
				
		Apple.ApplePushService appleService = null;
        Android.GcmPushService googleService = null;
        Blackberry.BlackberryPushService blackBerryService = null;
		Windows.WindowsPushService windowsService = null;

		static PushService instance = null;
		public static PushService Instance
		{
			get
			{
				if (instance == null)
					instance = new PushService();

				return instance;
			}
		}

		public PushService()
		{
			this.Events = new ChannelEvents();
		}

		public PushService(bool waitForQueuesToFinish) : this()
		{
			this.WaitForQueuesToFinish = waitForQueuesToFinish;
        }

        # region GooglePush

        public void StartGooglePushService(Android.GcmPushChannelSettings channelSettings)
        {
            StartGooglePushService(channelSettings, null);
        }
        public void StartGooglePushService(Android.GcmPushChannelSettings channelSettings, PushServiceSettings serviceSettings)
		{
            googleService = new PushSharp.Android.GcmPushService(channelSettings, serviceSettings);
            googleService.Events.RegisterProxyHandler(this.Events);
		}

        public void StopGooglePushService()
        {
            StopGooglePushService(true);
        }
        public void StopGooglePushService(bool waitForQueueToFinish)
		{
			if (googleService != null)
                googleService.Stop(waitForQueueToFinish);
        }

        #endregion

        # region ApplePush

        public void StartApplePushService(Apple.ApplePushChannelSettings channelSettings)
        {
            StartApplePushService(channelSettings, null);
        }
        public void StartApplePushService(Apple.ApplePushChannelSettings channelSettings, PushServiceSettings serviceSettings)
        {
            appleService = new Apple.ApplePushService(channelSettings, serviceSettings);
            appleService.Events.RegisterProxyHandler(this.Events);
        }

        public void StopApplePushService()
        {
            StopApplePushService(true);
        }
        public void StopApplePushService(bool waitForQueueToFinish)
        {
            if (appleService != null)
                appleService.Stop(waitForQueueToFinish);
        }

        #endregion


        # region BlackBerryPush

        public void StartBlackBerryPushService(Blackberry.BlackberryPushChannelSettings channelSettings)
        {
            StartBlackBerryPushService(channelSettings, null);
        }
        public void StartBlackBerryPushService(Blackberry.BlackberryPushChannelSettings channelSettings, PushServiceSettings serviceSettings)
        {
            blackBerryService = new PushSharp.Blackberry.BlackberryPushService(channelSettings, serviceSettings);
            googleService.Events.RegisterProxyHandler(this.Events);
        }

        public void StopBlackBerryPushService()
        {
            StopBlackBerryPushService(true);
        }
        public void StopBlackBerryPushService(bool waitForQueueToFinish)
        {
            if (blackBerryService != null)
                blackBerryService.Stop(waitForQueueToFinish);
        }

        #endregion

		# region WindowsPush

		public void StartWindowsPushService(Windows.WindowsPushChannelSettings channelSettings)
		{
			StartWindowsPushService(channelSettings, null);
		}
		public void StartWindowsPushService(Windows.WindowsPushChannelSettings channelSettings, PushServiceSettings serviceSettings)
		{
			windowsService = new PushSharp.Windows.WindowsPushService(channelSettings, serviceSettings);
			windowsService.Events.RegisterProxyHandler(this.Events);
		}

		public void StopWindowsPushService()
		{
			StopWindowsPushService(true);
		}
		public void StopWindowsPushService(bool waitForQueueToFinish)
		{
			if (windowsService != null)
				windowsService.Stop(waitForQueueToFinish);
		}

		#endregion


        public void QueueNotification(Notification notification)
		{
			switch (notification.Platform)
			{
				case PlatformType.Apple:
					appleService.QueueNotification(notification);
					break;
                case PlatformType.AndroidGcm:
                    googleService.QueueNotification(notification);
                    break;
                case PlatformType.Blackberry:
                    blackBerryService.QueueNotification(notification);
                    break;
				case PlatformType.Windows:
					windowsService.QueueNotification(notification);
					break;
			}
		}   

        public void StopAllServices()
        {
            StopAllServices(true);
        }

		public void StopAllServices(bool waitForQueuesToFinish)
		{
            StopApplePushService(waitForQueuesToFinish);
            StopBlackBerryPushService(waitForQueuesToFinish);
            StopGooglePushService(waitForQueuesToFinish);
			StopWindowsPushService(waitForQueuesToFinish);
		}

		void IDisposable.Dispose()
		{
			StopAllServices(this.WaitForQueuesToFinish);
		}

}	
#else
	public class PushService 
	{ }
#endif

	}
