using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GeneXus.Configuration;
using GeneXus.Http;
using GeneXus.Utils;

namespace GeneXus.Application
{
	internal class URLRouter
	{

		static ConcurrentDictionary<string, string> routerList;
		const string RESOURCE_PATTERN = "*.rewrite";
		const string schemeRegEx = @"^([a-z][a-z0-9+\-.]*):";
		static Regex scheme = new Regex(schemeRegEx, RegexOptions.IgnoreCase);
		internal static string GetURLRoute(string key, object[] objectParms, string[] parmsName, string scriptPath)
		{
			string[] parms = objectParms.Select(p => StringizeParm(p)).ToArray() ;
			if (PathUtil.IsAbsoluteUrl(key) || key.StartsWith("/") || string.IsNullOrEmpty(key) || scheme.IsMatch(key))
			{
				if (parms.Length > 0)
				{
					string[] noNameParms = Array.Empty<string>();
					return $"{key}{ConvertParmsToQueryString(parms, noNameParms)}";
				}
				else
				{
					return key;
				}
			}

			if (routerList == null)
			{
				routerList = new ConcurrentDictionary<string, string>();
				Load(RESOURCE_PATTERN);
			}

			string[] urlQueryString = key.Split('?');
			string query = urlQueryString.Length > 1 ? urlQueryString[1] : string.Empty;
			string path = urlQueryString[0];

			string[] parameterValues= string.IsNullOrEmpty(query) ? parms : HttpHelper.GetParameterValues(query);

			string routerKey;
			bool rewriteMatch = routerList.TryGetValue(NormalizedUrlObjectName(path), out routerKey);
			if (rewriteMatch)
			{
				path = string.Format(routerKey, parameterValues);
			}
			string basePath = Preferences.RewriteEnabled ? scriptPath : string.Empty;
			string result;

			if (rewriteMatch)
			{
				if (PatternHasParameters(routerKey))
					result = $"{basePath}{path}";
				else
					result = $"{basePath}{path}{ConvertParmsToQueryString(parms, parmsName)}";
			}
			else if (parms.Length > 0)
			{
				result = $"{basePath}{path}{ConvertParmsToQueryString(parms, parmsName)}";
			}
			else if (!string.IsNullOrEmpty(query))
			{
				result = $"{basePath}{path}?{query}";
			}
			else if (!string.IsNullOrEmpty(path))
				result = $"{basePath}{path}";
			else
				result = path;

			return result;
		}

		private static string StringizeParm(object objectParm)
		{
			if (objectParm == null)
				return string.Empty;
			else if (objectParm is string)
				return objectParm as string;
			else
				return objectParm.ToString();
		}

		private static bool PatternHasParameters(string routerKey)
		{
			return !string.IsNullOrEmpty(routerKey) && routerKey.Contains("{0}");
		}
		private static string ConvertParmsToQueryString(string[] parms, string[] parmsName)
		{
			if (parms.Length == 0)
				return string.Empty;
			bool useNamedParameters = Preferences.UseNamedParameters && (parms.Length == parmsName.Length);

			StringBuilder queryString = new StringBuilder("?");
			string parameterSeparator = useNamedParameters ? "&" : ",";

			for (int i = 0; i < parms.Length; i++)
			{
				bool lastParameter = (i == parms.Length - 1);

				if (!useNamedParameters)
					queryString.Append(parms[i]);
				else
					queryString.Append($"{parmsName[i]}={parms[i]}");

				if (!lastParameter)
					queryString.Append(parameterSeparator);
			}
			return queryString.ToString();
		}
		private static string NormalizedUrlObjectName(string objectName)
		{
			return objectName.ToLower();
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
						routerList[$"{NormalizedUrlObjectName(objectName)}.aspx"] = url;
					}
				}
			}
		}
	}
}
