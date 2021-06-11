using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeneXus.Services;

namespace GxClasses.Services.Storage
{
	public static class ExternalProviderCommon
	{
		public static String getProviderObjectName(ExternalProvider provider, String objectNameOrUrl)
		{
			String providerObjectName = null;
			if (provider != null)
			{
				//providerObjectName = provider.GetObjectNameFromURL(objectNameOrUrl);
				if (providerObjectName != null && providerObjectName.IndexOf("?") > 0)
				{
					providerObjectName = providerObjectName.Substring(0, providerObjectName.IndexOf("?"));
				}
			}
			return providerObjectName;
		}

		public static String getProviderObjectAbsoluteUriSafe(ExternalProvider provider, String rawObjectUri)
		{
			String providerObjectName = getProviderObjectName(provider, rawObjectUri);
			if (providerObjectName != null && rawObjectUri.IndexOf("?") > 0)
			{
				rawObjectUri = rawObjectUri.Substring(0, rawObjectUri.IndexOf("?"));
			}
			return rawObjectUri;
		}

	}
}
