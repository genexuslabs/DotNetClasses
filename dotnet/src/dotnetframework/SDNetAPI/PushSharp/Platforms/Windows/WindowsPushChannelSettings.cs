﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushSharp.Common;

namespace PushSharp.Windows
{
	public class WindowsPushChannelSettings : PushChannelSettings
	{
		public WindowsPushChannelSettings(string packageName, string packageSecurityIdentifier, string clientSecret)
		{
			this.PackageName = packageName;
			this.PackageSecurityIdentifier = packageSecurityIdentifier;
			this.ClientSecret = clientSecret;
		}

		public string PackageName { get; private set; }
		public string PackageSecurityIdentifier { get; private set; }
		public string ClientSecret { get; private set; }
	}
}
