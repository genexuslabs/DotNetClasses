using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using GeneXus.Http;
using GeneXus.Utils;

namespace GeneXus.Application
{
	internal class URLRouter
	{

		static ConcurrentDictionary<string, string> routerList;
		const string RESOURCE_PATTERN = "*.rewrite";

		internal static string GetURLRoute(string key, string[] parms, string[] parmsName, bool useNamedParameters=true)
		{
			if (PathUtil.IsAbsoluteUrl(key) || key.StartsWith("/"))
				return key;

			if (routerList == null)
			{
				routerList = new ConcurrentDictionary<string, string>();
				Load(RESOURCE_PATTERN);
			}

			string[] urlQueryString = key.Split('?');
			string query = urlQueryString.Length > 1 ? urlQueryString[1] : string.Empty;
			string path = urlQueryString[0].ToLower();

			string[] parameterValues= string.IsNullOrEmpty(query) ? parms : HttpHelper.GetParameterValues(query);

			if (routerList.ContainsKey(path))
				path = string.Format(routerList[path], parameterValues);

			if (string.IsNullOrEmpty(query))
				return $"{path}{ConvertParmsToQueryString(useNamedParameters, parms, parmsName, path)}";
			else
				return $"{path}?{query}";
		}
		private static string ConvertParmsToQueryString(bool useNamedParameters, string[] parms, string[] parmsName, string routerRule)
		{
			if (routerRule.Contains("%1") || (parms.Length == 0))
				return string.Empty;

			StringBuilder queryString = new StringBuilder("?");
			string parameterSeparator = useNamedParameters ? "&" : ",";

			for (int i = 0; i < parms.Length; i++)
			{
				bool lastParameter = (i == parms.Length - 1);

				if (!useNamedParameters || parms.Length != parmsName.Length)
					queryString.Append(parms[i]);
				else
					queryString.Append($"{parmsName[i]}={parms[i]}");

				if (!lastParameter)
					queryString.Append(parameterSeparator);
			}
			return queryString.ToString();
		}

		private static void Load(string resourcePattern)
		{
			string[] files = Directory.GetFiles(GxContext.StaticPhysicalPath(), resourcePattern);
			foreach (string resourceName in files)
			{
				foreach (string line in File.ReadLines(resourceName, Encoding.UTF8))
				{
					string[] map = line.Split(new char[] { '=' }, 2);
					if (map.Length == 2)
					{
						string objectName = map[0];
						string url = map[1];
						url = url.Replace(@"\=", "=").Replace(@"\\", @"\");
						routerList[objectName.ToLower()] = url;
					}
				}
			}
		}
	}
}
