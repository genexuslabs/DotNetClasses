using GeneXus.Application;
using GeneXus.Utils;
using log4net;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;

namespace GeneXus.Helpers
{
	public sealed class ThemeHelper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Helpers.ThemeHelper));
		private static ConcurrentDictionary<string, ThemeData> m_themes = new ConcurrentDictionary<string, ThemeData>();
		private static ThemeData CreateDefaultThemeData(string themeName)
		{
			ThemeData themeData = new ThemeData
			{
				name = themeName,
				baseLibraryCssReferences = Array.Empty<string>()
			};

			return themeData;
		}
		private static ThemeData GetThemeData(string themeName)
		{
			if (!m_themes.ContainsKey(themeName))
			{
				string path = Path.Combine(GxContext.StaticPhysicalPath(), "themes", $"{themeName}.json");
				ThemeData themeData;
				try
				{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					string json = File.ReadAllText(path);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					themeData = JSONHelper.Deserialize<ThemeData>(json);
				}
				catch (Exception ex)
				{
					GXLogging.Warn(log, $"Unable to load theme metadata ({themeName}). Using an empty default one.", ex);
					themeData = CreateDefaultThemeData(themeName);
				}
				m_themes[themeName] = themeData;
			}

			return m_themes[themeName];
		}
		public static string[] GetThemeCssReferencedFiles(string themeName)
		{
			ThemeData themeData = GetThemeData(themeName);
			return themeData.baseLibraryCssReferences;
		}
	}

	[DataContract]
	//Is must be public for medium trust environments.
	public class ThemeData
	{
		[DataMember]
		public string name;

		[DataMember]
		public string[] baseLibraryCssReferences;
	}
}
