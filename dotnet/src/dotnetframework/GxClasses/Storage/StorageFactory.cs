using GeneXus.Services;
using GeneXus.Utils;

namespace GeneXus.Storage
{
	public class StorageFactory
	{
		const char QUESTION_MARK = '?';
		public static bool TryGetProviderObjectName(ExternalProvider provider, string objectNameOrUrl, out string providerObjectName)
		{
			providerObjectName = null;
			if (provider != null && provider.TryGetObjectNameFromURL(objectNameOrUrl, out providerObjectName))
			{
				int idx = providerObjectName.IndexOf(QUESTION_MARK);
				if (idx > 0)
				{
					providerObjectName = providerObjectName.Substring(0, idx);
				}

				// We store in DB, Path Encoded Urls. If the parameter is an absolute URL, we need to decode the ObjectName to get the real Object Name.
				if (providerObjectName != null && PathUtil.IsAbsoluteUrl(objectNameOrUrl))
					providerObjectName = StorageUtils.DecodeUrl(providerObjectName);
				return true;
			}
			return false;
		}

		public static string GetProviderObjectAbsoluteUriSafe(ExternalProvider provider, string rawObjectUri)
		{
			string providerObjectName;
			if (TryGetProviderObjectName(provider, rawObjectUri, out providerObjectName))
			{
				rawObjectUri = providerObjectName;
			}
			return rawObjectUri;
		}
	}
}
