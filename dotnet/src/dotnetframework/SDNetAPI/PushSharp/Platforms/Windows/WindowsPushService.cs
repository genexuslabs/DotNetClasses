using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushSharp.Common;

namespace PushSharp.Windows
{
	public class WindowsPushService : PushServiceBase<WindowsPushChannelSettings>
	{
        public WindowsPushService(WindowsPushChannelSettings channelSettings)
            : this(channelSettings, null)
        {
        }
		public WindowsPushService(WindowsPushChannelSettings channelSettings, PushServiceSettings serviceSettings)
			: base(channelSettings, serviceSettings)
		{
		}

		protected override PushChannelBase CreateChannel(PushChannelSettings channelSettings)
		{
			return new WindowsPushChannel(channelSettings as WindowsPushChannelSettings);
		}

		public override PlatformType Platform
		{
			get { return PlatformType.Windows; }
		}
	}
}
