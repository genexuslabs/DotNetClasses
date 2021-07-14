using GeneXus.Services;

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
