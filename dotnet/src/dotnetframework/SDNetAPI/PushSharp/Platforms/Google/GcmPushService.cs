﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushSharp.Common;

namespace PushSharp.Android
{
	public class GcmPushService : PushServiceBase<GcmPushChannelSettings>
	{
        public GcmPushService(GcmPushChannelSettings channelSettings)
            : this(channelSettings, null)
        {
        }
		public GcmPushService(GcmPushChannelSettings channelSettings, PushServiceSettings serviceSettings)
			: base(channelSettings, serviceSettings)
		{
		}

		protected override PushChannelBase CreateChannel(PushChannelSettings channelSettings)
		{
			return new GcmPushChannel(channelSettings as GcmPushChannelSettings);
		}

		public override PlatformType Platform
		{
			get { return PlatformType.AndroidGcm; }
		}
	}
}
