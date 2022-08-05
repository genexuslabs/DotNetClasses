using System;
using System.IO;

namespace GeneXus.CloudServices
{
	public static class Services
	{
		static Services()
		{
			string _ = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "CloudServices.config");
		}

	}

	public enum ServiceType
	{
		Cache,
		Storage,
		Notifications
	}
}
