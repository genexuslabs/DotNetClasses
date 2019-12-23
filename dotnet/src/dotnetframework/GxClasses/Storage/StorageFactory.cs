using GeneXus.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.Storage
{
	public class StorageFactory
	{

		public static ExternalProvider GetExternalProviderFromUrl(string url, out string objectName)
		{
			objectName = null;
			ExternalProvider provider = ServiceFactory.GetExternalProvider();
			if (provider != null)
			{
				if (provider.GetObjectNameFromURL(url, out objectName))
				{
					objectName = objectName.Substring(0, objectName.IndexOf("?"));
					return provider;
				}
			}
			return null;
		}
	}
}
