using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneXus.CloudServices
{
	public class Services
	{
		static Services()
		{
			string serviceFile = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CloudServices.config");
		}

	}

	public enum ServiceType
	{
		Cache,
		Storage,
		Notifications
	}
}
