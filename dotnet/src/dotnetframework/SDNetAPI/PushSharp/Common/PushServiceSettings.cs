﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PushSharp.Common
{
	public class PushServiceSettings
	{
		public PushServiceSettings()
		{
			this.AutoScaleChannels = false;
			this.MaxAutoScaleChannels = 100;
			this.Channels = 2;
			this.MaxNotificationRequeues = 3;
		}

		public bool AutoScaleChannels { get; set; }
		public int MaxAutoScaleChannels { get; set; }
		public int Channels { get; set; }

		public int MaxNotificationRequeues { get; set; }
	}
}
