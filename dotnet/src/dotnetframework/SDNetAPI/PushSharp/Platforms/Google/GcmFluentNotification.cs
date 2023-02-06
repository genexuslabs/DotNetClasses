﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushSharp.Android;
using Artech.Genexus.SDAPI;

namespace PushSharp
{
	public static class GcmFluentNotification
	{
		public static GcmNotification ForDeviceRegistrationId(this GcmNotification n, string deviceRegistrationId)
		{
			n.RegistrationIds.Add(deviceRegistrationId);
			return n;
		}

		public static GcmNotification WithCollapseKey(this GcmNotification n, string collapseKey)
		{
			n.CollapseKey = collapseKey;
			return n;
		}

        public static GcmNotification WithDelayWhileIdle(this GcmNotification n)
        {
            return WithDelayWhileIdle(n,false);
        }

		public static GcmNotification WithDelayWhileIdle(this GcmNotification n, bool delayWhileIdle)
		{
			n.DelayWhileIdle = delayWhileIdle;
			return n;
		}

		public static GcmNotification WithJson(this GcmNotification n, string json)
		{
			try { var nobj = Utils.FromJSonString(json); }
			catch { throw new InvalidCastException("Invalid JSON detected!"); }

			n.JsonData = json;
			return n;
		}

		public static GcmNotification WithTag(this GcmNotification n, object tag)
        {
            n.Tag = tag;
            return n;
        }
	}
}
