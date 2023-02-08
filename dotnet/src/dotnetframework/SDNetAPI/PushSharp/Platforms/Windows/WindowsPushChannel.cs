using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using PushSharp.Common;

namespace PushSharp.Windows
{
	public class WindowsPushChannel : PushChannelBase
	{
		WindowsPushChannelSettings windowsSettings = null;
		WindowsMessageTransportAsync transport;
		long waitCounter = 0;

        public WindowsPushChannel(WindowsPushChannelSettings channelSettings) : this(channelSettings, null)
        {
        }

		public WindowsPushChannel(WindowsPushChannelSettings channelSettings, PushServiceSettings serviceSettings)
			: base(channelSettings, serviceSettings) 
		{
			windowsSettings = channelSettings;

			transport = new WindowsMessageTransportAsync();

			transport.MessageResponseReceived += new Action<WindowsNotificationStatus>(transport_MessageResponseReceived);

			transport.UnhandledException += new Action<WindowsNotification, Exception>(transport_UnhandledException);
		}

		void transport_UnhandledException(WindowsNotification notification, Exception exception)
		{
			this.Events.RaiseNotificationSendFailure(notification,exception);
			this.Events.RaiseChannelException(exception, notification);
			Interlocked.Decrement(ref waitCounter);
		}

		void transport_MessageResponseReceived(WindowsNotificationStatus status)
		{
			if (status.HttpStatus == HttpStatusCode.OK && status.NotificationStatus == WindowsNotificationSendStatus.Received)
			{
				//It worked! Raise success
				this.Events.RaiseNotificationSent(status.Notification); 
			}
			else if (status.NotificationStatus == WindowsNotificationSendStatus.TokenExpired)
			{
				// If token expired sendind a notification, requeue it again
				this.QueueNotification(status.Notification);
			}
			else
			{
				// Something happened
				this.Events.RaiseNotificationSendFailure(status.Notification, new Exception(status.ErrorDescription));
			}
            Interlocked.Decrement(ref PendingNotificationsResult);
			Interlocked.Decrement(ref waitCounter);
		}

		protected override void SendNotification(Notification notification)
		{
			Interlocked.Increment(ref waitCounter);

			transport.Send(notification as WindowsNotification, windowsSettings.PackageName, windowsSettings.PackageSecurityIdentifier, windowsSettings.ClientSecret);
		}

		public override void Stop(bool waitForQueueToDrain)
		{
			base.Stop(waitForQueueToDrain);

			var slept = 0;
			while (Interlocked.Read(ref waitCounter) > 0 && slept <= 30000)
			{
				slept += 100;
				Thread.Sleep(100);
			}
		}
	}
}
