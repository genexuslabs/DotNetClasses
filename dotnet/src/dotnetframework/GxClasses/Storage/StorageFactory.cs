using GeneXus.Services;

namespace GeneXus.Storage
{
	public class StorageFactory
	{
		const char QUESTION_MARK = '?';
		/*public static ExternalProvider GetExternalProviderFromUrl(string url, out string objectName)
		{
			objectName = null;
			ExternalProvider provider = ServiceFactory.GetExternalProvider();
			if (provider != null)
			{
				if (provider.TryGetObjectNameFromURL(url, out objectName))
				{
					var questionMarkIndex = objectName.IndexOf(QUESTION_MARK);
					objectName = questionMarkIndex >= 0 ? objectName.Substring(0, questionMarkIndex): objectName.Substring(0);
					return provider;
				}
			}
			return null;
		}
		*/
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
