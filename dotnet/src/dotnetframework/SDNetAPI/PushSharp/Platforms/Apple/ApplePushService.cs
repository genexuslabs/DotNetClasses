﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PushSharp.Common;

namespace PushSharp.Apple
{
	public class ApplePushService : Common.PushServiceBase<ApplePushChannelSettings>, IDisposable
	{
		FeedbackService feedbackService;
		//CancellationTokenSource cancelTokenSource;
		Timer timerFeedback;

        public ApplePushService(ApplePushChannelSettings channelSettings) : this(channelSettings,null)
        {
            
        }

		public ApplePushService(ApplePushChannelSettings channelSettings, PushServiceSettings serviceSettings)
			: base(channelSettings, serviceSettings)
		{
			var appleChannelSettings = channelSettings as ApplePushChannelSettings;
			//cancelTokenSource = new CancellationTokenSource();
			feedbackService = new FeedbackService();
			feedbackService.OnFeedbackReceived += new FeedbackService.FeedbackReceivedDelegate(feedbackService_OnFeedbackReceived);

			//allow control over feedback call interval, if set to zero, don't make feedback calls automatically
			if (appleChannelSettings.FeedbackIntervalMinutes > 0)
			{
				timerFeedback = new Timer(new TimerCallback((state) =>
				{
					try { feedbackService.Run(channelSettings as ApplePushChannelSettings/*, this.cancelTokenSource.Token*/); }
					catch (Exception ex) { this.Events.RaiseChannelException(ex); }

					//Timer will run first after 10 seconds, then every 10 minutes to get feedback!
				}), null, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(appleChannelSettings.FeedbackIntervalMinutes));

			}
		}

		void feedbackService_OnFeedbackReceived(string deviceToken, DateTime timestamp)
		{
			this.Events.RaiseDeviceSubscriptionExpired(PlatformType.Apple, deviceToken);
		}

		protected override Common.PushChannelBase CreateChannel(Common.PushChannelSettings channelSettings)
		{
			return new ApplePushChannel(channelSettings as ApplePushChannelSettings);
		}

		public override PlatformType Platform
		{
			get { return PlatformType.Apple; }
		}
	}
}
