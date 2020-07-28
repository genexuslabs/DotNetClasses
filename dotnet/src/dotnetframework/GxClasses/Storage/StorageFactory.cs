using GeneXus.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.Storage
{
	public class StorageFactory
	{
		const char QUESTION_MARK = '?';
		public static ExternalProvider GetExternalProviderFromUrl(string url, out string objectName)
		{
			objectName = null;
			ExternalProvider provider = ServiceFactory.GetExternalProvider();
			if (provider != null)
			{
				if (provider.GetObjectNameFromURL(url, out objectName))
				{
					var questionMarkIndex = objectName.IndexOf(QUESTION_MARK);
					objectName = questionMarkIndex >= 0 ? objectName.Substring(0, questionMarkIndex): objectName.Substring(0);
					return provider;
				}
			}
			return null;
		}
	}
}
