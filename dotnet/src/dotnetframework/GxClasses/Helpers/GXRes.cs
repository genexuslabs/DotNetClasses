namespace GeneXus.Resources
{
	using System;
	using System.Resources;
	using System.Globalization;
	using System.Reflection;
	using GeneXus.Configuration;
	using GeneXus.Utils;
	using System.IO;
	using log4net;
	using GeneXus.Application;
	using System.Collections;
	using System.Text;
	using System.Collections.Concurrent;
#if NETCORE
	using GxClasses.Helpers;
	using System.Collections.ObjectModel;
	using System.Collections.Generic;
#endif

	public class GXResourceManager
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Resources.GXResourceManager));
        static ConcurrentDictionary<string, ResourceSet> _rm = new ConcurrentDictionary<string, ResourceSet>();
		static ConcurrentDictionary<string, LocalUtil> _localUtils = new ConcurrentDictionary<string,LocalUtil>();
		static string _defaultLangName;
		static string DefaultLangName
		{
			get
			{
				if (_defaultLangName == null)
				{
					Config.GetValueOf("LANG_NAME", out _defaultLangName);
				}
				return _defaultLangName;
			}
		}
		public static LocalUtil GetLocalUtil(string langName)
		{
			if (!_localUtils.ContainsKey(langName))
			{
				_localUtils.TryAdd(langName, new LocalUtil(langName));
			}
			return (LocalUtil)_localUtils[langName];
		}
		static ResourceSet GetRManager(string langName)
		{
			if (String.IsNullOrEmpty(langName) || String.IsNullOrEmpty(langName.Trim()))
				langName = DefaultLangName;

			if (langName == null)
				throw new Exception("Undefined language, check client.exe.config/web.config for a LANG_NAME entry. Verify your KB has at least one language defined");

			if (!_rm.ContainsKey(langName))
			{
                string langCode = Config.GetLanguageProperty(langName, "code");
                LoadResources(langName, langCode != null ? langCode : langName);
			}
			if (_rm.ContainsKey(langName))
				return (ResourceSet)_rm[langName];
			else
				return null;
		}
#if NETCORE
		static void LoadResources(string langName, string langid)
		{
			try
			{
				string resourcesFile = Path.Combine(FileUtil.GetStartupDirectory(), "messages." + langid.ToLower() + ".resources");
				if (File.Exists(resourcesFile))
				{
					ResourceSet rs = new ResourceSet(resourcesFile);
					_rm[langName] = rs;
				}
			}
			catch (System.IO.FileNotFoundException fex)
			{
				GXLogging.Error(log, "LoadResources Error ", fex);
			}
		}
#else
		static void LoadResources(string langName, string langid)
		{
			try
			{
				Assembly msg = Assembly.LoadFrom(FindResources("messages." + langid.ToLower()+ ".dll"));
                if (msg != null)
                {
                    ResourceManager rm = new ResourceManager("messages." + langid.ToLower(), msg);
                    _rm[langName] = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
                }
			}
			catch (System.IO.FileNotFoundException fex)
			{
				GXLogging.Error(log, "LoadResources Error ", fex);
			}
		}
		public static Stream FindResources(string resourcesFileName, string resourcesName)
        {
            string file = FindResources(resourcesFileName);
            return Assembly.LoadFrom(file).GetManifestResourceStream(resourcesName);
        }
        public static String FindResources(string resourcesFile)
        {
			ArrayList file = new ArrayList();
			string fileName = null;
			
			file.Add(Path.Combine(GxContext.StaticPhysicalPath(), resourcesFile));
			file.Add(Path.Combine(Path.Combine(GxContext.StaticPhysicalPath(), "bin"), resourcesFile));
			try
			{
				file.Add(Path.Combine(FileUtil.UriToPath(FileUtil.GetStartupDirectory()), resourcesFile));
			}
			catch{}
			file.Add(Path.Combine(FileUtil.GetStartupDirectory(), resourcesFile));

			foreach (string f in file)
			{
				if (File.Exists(f))
				{
					GXLogging.Debug(log, "FindResources "+ f);
                    fileName = f;
					break;
				}
			}
			if (fileName!=null)
				return fileName;
			else 
				throw new FileNotFoundException("File " + resourcesFile + " not found");
		}

#endif
		static public string GetMessage(string strId)
		{
			return GetMessage(DefaultLangName, strId);
		}
		static public string GetMessage(string langId, string strId)
		{
			return GetMessage(langId, strId, null);
		}
		static public string GetMessage(string strId, Object[] obj)
		{
			return GetMessage(DefaultLangName, strId, obj);
		}
		static public string GetMessage(string langId, string strId1, Object[] obj)
		{
			bool trimSpaces = strId1 != null && strId1.Length > 0 && (strId1[0] == ' ' || strId1[strId1.Length - 1] == ' ');
			string strId = trimSpaces ? strId1.Trim() : strId1;

            ResourceSet rs = GetRManager(langId);
            string strFormat = null;
            if (rs != null)
            {
                strFormat = rs.GetString(strId);
            }
			if (strFormat == null)
			{
                strFormat = strId1;
			}
			else
			{
				try
				{
					if (obj != null)
					{
						strFormat = string.Format(strFormat, obj);
					}
				}
				catch 
				{
					GXLogging.Warn(log, "Cannot format message '" + strFormat + "'");
				}
                if (trimSpaces)
                    strFormat = CopySpaces(strId1, strFormat);
			}
			return strFormat;

		}

		static string CopySpaces(string strId1, string strFormat)
		{
			StringBuilder str = new StringBuilder();
			for (int i = 0; i < strId1.Length; i++)
			{
				if (strId1[i] == ' ')
				{
					str.Append(' ');
				}
				else
				{
					str.Append(strFormat);
					break;
				}
			}
			for (int i = strId1.Length-1; i >=0; i--)
			{
				if (strId1[i] == ' ')
				{
					str.Append(' ');
				}
				else
				{
					break;
				}
			}
			return str.ToString();
		}

	}
}
