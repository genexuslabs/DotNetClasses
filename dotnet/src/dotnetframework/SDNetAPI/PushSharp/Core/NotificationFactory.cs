using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PushSharp
{
	public class NotificationFactory
	{
		public static Common.Notification Create(Common.PlatformType platform)
		{
#if !NETCOREAPP1_1
			switch (platform)
			{
				case Common.PlatformType.Apple:
					return Apple();
                case Common.PlatformType.AndroidGcm:
                    return Google();
                case Common.PlatformType.Blackberry:
                    return BlackBerry();
				case Common.PlatformType.Windows:
					return Windows();

				default:
					return null;
			}
#else
			return null;
#endif
		}
#if !NETCOREAPP1_1
		public static Apple.AppleNotification Apple()
		{            
			return new Apple.AppleNotification();
		}

        public static Android.GcmNotification Google()
        {
            return new PushSharp.Android.GcmNotification();
        }

        public static Blackberry.BlackberryNotification BlackBerry()
        {
            return new PushSharp.Blackberry.BlackberryNotification();
        }

		public static Windows.WindowsNotification Windows()
		{
			return new PushSharp.Windows.WindowsNotification();
		}
#endif
	}
}
