using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.SDAPI;

namespace PushSharp.Common
{
	public enum PlatformType
	{
		Apple,
		AndroidC2dm,
		AndroidGcm,
		Windows,
		WindowsPhone,
		Blackberry,
        None
	}

	public class PlatformConverter
	{
		public static PlatformType FromDeviceType(int gxDeviceType)
		{
			switch (gxDeviceType)
			{ 
				case Notifications.IOS:
					return PlatformType.Apple;
				case Notifications.ANDROID:
					return PlatformType.AndroidGcm;
				case Notifications.BB:
					return PlatformType.Blackberry;
				case Notifications.WPHONE:
					return PlatformType.WindowsPhone;
				default:
					return PlatformType.None;
			}
			
		}
	}
}
