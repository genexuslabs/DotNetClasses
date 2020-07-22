using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using GeneXus.Utils;

namespace GeneXus.Application
{
	internal class URLRouter
	{

		static ConcurrentDictionary<string, string> routerList;
		const string RESOURCE_NAME = "urlrouter.txt";

		public static string GetURLRoute(string key, string[] parms, string[] parmsName, bool useNamedParameters=true)
		{
			if (PathUtil.IsAbsoluteUrl(key) || key.StartsWith("/"))
				return key;

			if (routerList == null)
			{
				routerList = new ConcurrentDictionary<string, string>();
				Load(Path.Combine(GxContext.StaticPhysicalPath(),RESOURCE_NAME));
			}

			string[] urlQueryString = key.Split('?');
			string query = urlQueryString.Length > 1 ? urlQueryString[1] : string.Empty;
			string path = urlQueryString[0];

			string[] parameterValues= string.IsNullOrEmpty(query) ? parms : GetParameterValues(query);

			if (routerList.ContainsKey(path))
				path = string.Format(routerList[path], parameterValues);

			if (string.IsNullOrEmpty(query))
				return $"{path}{ConvertParmsToQueryString(useNamedParameters, parms, parmsName, path)}";
			else
				return $"{path}?{query}";
		}

		static bool NamedParametersQuery(string query)
		{
			return query.Contains("=");
		}
		private static string[] GetParameterValues(string query)
		{
			if (NamedParametersQuery(query))
			{
				NameValueCollection names = HttpUtility.ParseQueryString(query);
				string[] values = new string[names.Count];
				for (int i = 0; i < names.Count; i++)
					values[i]=names[i];

				return values;
			}
			else
			{
				return query.Replace("%2C", ",").Split(',');
			}
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

		private static void Load(string resourceName)
		{
			if (File.Exists(resourceName))
			{
				foreach (string line in File.ReadLines(resourceName, Encoding.UTF8))
				{
					string[] map = line.Split(new char[] { '=' }, 2);
					if (map.Length == 2)
					{
						string objectName = map[0];
						string url = map[1];
						url = url.Replace(@"\=", "=").Replace(@"\\", @"\");
						routerList[objectName] = url;
					}
				}
			}
		}
	}
}
