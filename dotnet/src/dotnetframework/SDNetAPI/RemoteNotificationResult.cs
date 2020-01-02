using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
#if !NETCOREAPP1_1
using PushSharp.Windows;
using PushSharp.Apple;
using PushSharp.Android;
#endif
namespace Artech.Genexus.SDAPI
{
	public class RemoteNotificationResult
	{
		public short DeviceType { get; private set; }
		public string DeviceToken { get; private set; }
		public int ErrorCode { get; set; }
		public string ErrorDescription { get; set; }

		public static RemoteNotificationResult ForDevice(short deviceType, string deviceToken)
		{
			return new RemoteNotificationResult() { DeviceType = deviceType, DeviceToken = deviceToken };
		}

		public static RemoteNotificationResult FromNotification(PushSharp.Common.Notification notification)
		{
#if !NETCOREAPP1_1
            if (notification == null)
                return new RemoteNotificationResult();
			if (notification is AppleNotification)
				return FromAppleNotification(notification as AppleNotification);
			if (notification is GcmNotification)
				return FromGoogleNotification(notification as GcmNotification);
			if (notification is WindowsNotification)
				return FromWindowsNotification(notification as WindowsNotification);

			throw new Exception("Invalid device type");
#else
			return new RemoteNotificationResult();
#endif
		}
#if !NETCOREAPP1_1
		private static RemoteNotificationResult FromAppleNotification(AppleNotification appleNotification)
		{
			RemoteNotificationResult result = new RemoteNotificationResult();
			result.DeviceType = Notifications.IOS;
			result.DeviceToken = appleNotification.DeviceToken;
			return result;
		}

		private static RemoteNotificationResult FromGoogleNotification(GcmNotification googleNotification)
		{
			RemoteNotificationResult result = new RemoteNotificationResult();
			result.DeviceType = Notifications.ANDROID;
			result.DeviceToken = googleNotification.RegistrationIds[0];
			return result;
		}

		private static RemoteNotificationResult FromWindowsNotification(WindowsNotification notification)
		{
			RemoteNotificationResult result = new RemoteNotificationResult();
			result.DeviceType = Notifications.WPHONE;
			result.DeviceToken = notification.ChannelUri;
			return result;
		}
#endif
	}
}
