namespace GeneXus.Configuration
{
	using System;
	using System.Globalization;
	using System.IO;
#if NETCORE
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
#else
	using System.Web;
#endif
	using System.Configuration;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Xml;
	using log4net.Config;
	using log4net;
	using GeneXus.Utils;
	using GeneXus.Application;
	using GeneXus.Encryption;
	using System.Security;
	using Services;
	using System.Collections.Concurrent;
	using System.Reflection;
	using System.Runtime.Serialization.Json;
	using System.Collections.Generic;
	using GxClasses.Helpers;
	using System.Text;

	public class Config
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Configuration.Config));
		public const string GX_LANG_SPA = "spa";
		public const string GX_LANG_POR = "por";
		public const string GX_LANG_ITA = "ita";
		public const string GX_LANG_ENG = "eng";
		public const string GX_LANG_CHS = "chs";
		public const string GX_LANG_CHT = "cht";
		public const string GX_LANG_JAP = "jap";
		public const string DATASTORE_SECTION = "datastores/";

		private static string configFileName;
		public static string loadedConfigFile;
		private static bool configLog = true;
		private static bool configLoaded;
		static NameValueCollection _config;
		static ConcurrentDictionary<string, string> s_confMapping;
		const string CONFMAPPING_FILE = "confmapping.json";
		static Hashtable languages;
#if NETCORE
		static ConcurrentDictionary<string,string> customErrors;
#endif
		private static ConcurrentDictionary<string, string> connectionProperties = new ConcurrentDictionary<string, string>();

		public static bool ConfigLog
		{
			get { return configLog; }
			set { configLog = value; }
		}
		public static string ConfigFileName
		{
			get { return configFileName; }
			set { configFileName = value; configLoaded = false; connectionProperties.Clear(); }
		}

		public static void ParseArgs(ref string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg.StartsWith("\\config:"))
				{
					ConfigFileName = arg.Substring(8);
					RemoveArg(ref args, ref i);
				}
				else if (arg.StartsWith("/gxperf:"))
				{
					Diagnostics.GXDebugManager.Config(arg.Substring(8));
					RemoveArg(ref args, ref i);
				}

			}
		}

		private static void RemoveArg(ref string[] args, ref int i)
		{
			string[] nArgs = new string[args.Length - 1];
			if (i != 0)
				Array.Copy(args, 0, nArgs, 0, i);
			Array.Copy(args, i + 1, nArgs, i, nArgs.Length - i);
			args = nArgs;
			i--;
		}

#if !NETCORE
		public static int GetCount(string sectionName)
		{
			if (sectionName == null)
			{
				return ConfigurationSettings.AppSettings.Count;
			}
			else
			{
				if (ConfigurationSettings.GetConfig(sectionName) != null)
				{
					return ((CollectionBase)ConfigurationSettings.GetConfig(sectionName)).Count;
				}
				else
				{
					return 0;
				}
			}
		}

		public static bool SectionExists(string section)
		{
			return (ConfigurationSettings.GetConfig(section) != null);
		}
#endif
		internal const string CoreAssemblyName = "GeneXus";
		public static String CommonAssemblyName
		{
			get
			{
				string commonAssembly = "Common";
				string nspace;
				if (Config.GetValueOf("AppMainNamespace", out nspace) && !string.IsNullOrEmpty(nspace))
					commonAssembly = string.Format("{0}.{1}", nspace, commonAssembly);
				return commonAssembly;
			}
		}

		public static string GetValueOf(string sId, string defaultValue)
		{
			string result = null;
			if (!GetValueOf(sId, out result))
			{
				result = defaultValue;
			}
			return result;
		}

		public static bool GetValueOf(string sId, out string sString)
		{
			try
			{
				sString = config.Get(sId);
				if (String.IsNullOrEmpty(sString))
					return false;
				return true;
			}
			catch
			{
				sString = string.Empty;
				return false;
			}
		}

		public static bool GetValueOf(string sId)
		{
			string sString;
			return GetValueOf(sId, out sString);
		}

		static string GetMappedProperty(string original)
		{
			if (ConfMapping != null && ConfMapping.ContainsKey(original))
				return ConfMapping[original];
			else
				return $"{EnvVarReader.ENVVAR_PREFIX}{original}";
		}

		static ConcurrentDictionary<string, string> ConfMapping
		{
			get
			{
				if (s_confMapping == null)
				{
					try
					{
						string filePath = Path.Combine(GxContext.StaticPhysicalPath(), CONFMAPPING_FILE);
						if (File.Exists(filePath))
						{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
							using (FileStream file = File.Open(filePath, FileMode.Open))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
							{
								DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings
								{
									UseSimpleDictionaryFormat = true
								};

								DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ConcurrentDictionary<string, string>), settings);

#pragma warning disable SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
								s_confMapping = serializer.ReadObject(file) as ConcurrentDictionary<string, string>;
#pragma warning restore SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
							}
						}
						else
							s_confMapping = new ConcurrentDictionary<string, string>();
					}
					catch
					{
						s_confMapping = null;
					}
				}

				return s_confMapping;
			}
		}

		[SecuritySafeCritical]
		public static bool LoadConfiguration()
		{
			return config != null;
		}

		internal static bool GetEncryptedDataStoreProperty(string id, string name, out string ret)
		{
			string key = "Connection-" + id + name;
			string ds = DATASTORE_SECTION + id;
			ret = "";
			string cfgBuf = string.Empty;
			bool found = false;
			string envPath;

			if (!connectionProperties.ContainsKey(key))
			{
				try
				{
					if (Config.GetValueOf(ds, key, out cfgBuf))
					{
						found = true;
					}
					else if ((envPath = Environment.GetEnvironmentVariable("GXCFG")) != null)
					{
						if (IniGetValueOf(envPath + "\\gxcfg.ini", "Connection-" + id + name, out cfgBuf))
							found = true;
					}
#if !NETCORE
					else if (IniGetValueOf(Environment.SystemDirectory + "\\gxcfg.ini", "Connection-" + id + name, out cfgBuf))
						found = true;
#endif
				}
				catch (SecurityException ex)
				{
					GXLogging.Warn(log, "Error in GetEncryptedDataStoreProperty", ex);
				}
				if (found)
				{
					if (!CryptoImpl.Decrypt(ref ret, cfgBuf))
					{
						ret = cfgBuf;
					}
					if (!string.IsNullOrEmpty(ret))
						connectionProperties[key] = ret;
				}
			}
			else
			{
				ret = connectionProperties[key];
				found = true;
			}

			return found;
		}

		public static bool GetValueOf(string sectionName, string sId, out string sString)
		{
			return GetValueOf(sId, out sString);
		}
#if NETCORE
		public static string MapCustomError(string code)
		{
			if (customErrors!=null && customErrors.ContainsKey(code))
				return customErrors[code];
			else
				return null;
		}
#endif
		public static string GetLanguageProperty(string language, string property)
		{
			string sString = null;
			if (languages != null)
				sString = languages.Contains(language) ? (string)((Hashtable)languages[language])[property] : null;
			else
			{
#if !NETCORE
#pragma warning disable CS0618 // Type or member is obsolete
				Hashtable appsettings = (Hashtable)ConfigurationSettings.GetConfig("languages/" + language);
#pragma warning restore CS0618 // Type or member is obsolete
				if (appsettings != null)
					sString = (string)appsettings[property];
				else if (property == "code")
				{
					switch (language.ToLower())
					{
						case "spanish":
							sString = "spa";
							break;
						case "english":
							sString = "eng";
							break;
						case "portuguese":
							sString = "por";
							break;
						case "italian":
							sString = "ita";
							break;
						case "simplifiedchinese":
							sString = "chs";
							break;
						case "traditionalchinese":
							sString = "cht";
							break;
						case "japanese":
							sString = "jap";
							break;
						default:
							sString = null;
							break;
					}

				}
#endif
			}
			return sString;
		}

		public static bool IniGetValueOf(string fileName, string sId, out string sString)
		{
			sString = "";
			bool found = false;
			try
			{
				if (File.Exists(fileName))
				{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					using (FileStream _fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					{

						using (StreamReader _sr = new StreamReader(_fs))
						{
							string line;
							while ((line = _sr.ReadLine()) != null)
								if (line.Trim().StartsWith(sId.Trim()))
								{
									found = true;
									sString = line.Substring(line.IndexOf("=") + 1).Trim();
									break;
								}
						}
					}
				}
				return found;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error in IniGetValueOf", ex);
				return found;
			}
		}
		public static CultureInfo GetCultureForLang(string lang)
		{
			if (string.IsNullOrEmpty(StringUtil.Trim(lang)) && Config.GetValueOf("LANG_NAME", out string kbLanguage))
				lang = kbLanguage;

			string culture = lang;
			switch (culture.ToLower())
			{
				case GX_LANG_SPA:
					culture = "es-UY";
					break;
				case GX_LANG_POR:
					culture = "pt-BR";
					break;
				case GX_LANG_ITA:
					culture = "it-IT";
					break;
				case GX_LANG_ENG:
					culture = "en-US";
					break;
				case GX_LANG_CHS:
					culture = "zh-CN";
					break;
				case GX_LANG_CHT:
					culture = "zh-HK";
					break;
				case GX_LANG_JAP:
					culture = "ja-JP";
					break;
				default:
					culture = Config.GetLanguageProperty(lang, "culture");
					break;
			}
			try
			{
				CultureInfo ci = new CultureInfo(culture);
				if (!CalendarUtilities.IsGregorian(ci.DateTimeFormat.Calendar))
				{
					CalendarUtilities.ChangeCalendar(ci, new GregorianCalendar());
				}
				return ci;
			}
			catch (ArgumentNullException ex)
			{
				GXLogging.Error(log, "Invalid language", lang, ex);
				return new CultureInfo(CultureInfo.CurrentCulture.Name);
			}
		}

#if NETCORE
		public static IConfigurationRoot ConfigRoot { get; set; }
		const string Log4NetShortName = "log4net";
		static Version Log4NetVersion = new Version(2, 0, 11);

		const string ConfigurationManagerBak = "System.Configuration.ConfigurationManager, Version=4.0.3.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
		const string ConfigurationManagerFileName = "System.Configuration.ConfigurationManager.dll";

		const string ITextSharpFileName = "iTextSharp.dll";
		const string ITextSharpAssemblyName = "itextsharp";
		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var requestedAssembly = new AssemblyName(args.Name);
			if (requestedAssembly.Name == Log4NetShortName){
				requestedAssembly.Version = Log4NetVersion;
				return Assembly.Load(requestedAssembly);
			}

			if (args.Name.StartsWith(ConfigurationManagerBak))
			{
				string fileName = Path.Combine(FileUtil.GetStartupDirectory(), ConfigurationManagerFileName);
				if (File.Exists(fileName))
				{
					return Assembly.LoadFrom(fileName);
				}
			}
			else if (args.Name.StartsWith(ITextSharpAssemblyName))
			{
				string fileName = Path.Combine(FileUtil.GetStartupDirectory(), ITextSharpFileName);
				if (File.Exists(fileName))
				{
					return Assembly.LoadFrom(fileName);
				}
			}
			else
			{
				AssemblyName assName = new AssemblyName(args.Name);
				bool strongNamedAssembly = assName.GetPublicKeyToken().Length > 0;
				string fileName = Path.Combine(FileUtil.GetStartupDirectory(), $"{assName.Name}.dll");
				if (!strongNamedAssembly && File.Exists(fileName))
				{
					return Assembly.LoadFrom(fileName);
				}
			}
			return null;
		}
#else
		static Dictionary<string, Version> AssemblyRedirect = new Dictionary<string, Version>
		{
			{"log4net", new Version(2, 0, 11) },
			{ "System.Threading.Tasks.Extensions", new Version(4, 2, 0, 1) },
			{ "System.Runtime.CompilerServices.Unsafe", new Version(4, 0, 4, 1) },
			{ "System.Buffers", new Version(4, 0, 3, 0)},
			{ "System.Memory",new Version(4, 0, 1, 1) }
		};

		static string GeoTypesAssembly = "Microsoft.SqlServer.Types";
		[SecurityCritical]
		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var requestedAssembly = new AssemblyName(args.Name);
			if (AssemblyRedirect.ContainsKey(requestedAssembly.Name) && requestedAssembly.Version != AssemblyRedirect[requestedAssembly.Name])
			{
				requestedAssembly.Version = AssemblyRedirect[requestedAssembly.Name];
				return Assembly.Load(requestedAssembly);
			}
			else if (requestedAssembly.Name == GeoTypesAssembly && requestedAssembly.Version!=null)
			{
				return SQLGeographyWrapper.GeoAssembly;
			}
			else return null;
		}
#endif
		
		static NameValueCollection config
		{
			[SecuritySafeCritical] 
			get
			{
				if (!configLoaded || _config == null)
				{
					try
					{
						AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
					}
					catch (Exception ex)
					{
						GXLogging.Info(log, ".NET trust level is lower than full", ex.Message);
					}
					string logConfigSource;
					configLoaded = true;
					if (configFileName != null)
					{
						if (log.IsDebugEnabled) loadedConfigFile = configFileName;
						_config = loadConfig(configFileName, out logConfigSource);
						if (!string.IsNullOrEmpty(logConfigSource))
							logConfig(logConfigSource);
						else
							logConfig(configFileName);
						return _config;
					}
#if !NETCORE
					if (GxContext.IsHttpContext &&
						File.Exists(GxContext.StaticPhysicalPath() + "web.config"))
					{
						logConfig(null);
						if (log.IsDebugEnabled) loadedConfigFile = GxContext.StaticPhysicalPath() + "web.config";
						_config = ConfigurationSettings.AppSettings;
						foreach (string key in _config.Keys)
						{
							string value = MappedValue(key, _config[key]);
							if (value!=_config[key])
								_config[key] = value;
						}
						languages = null;
						return _config;
					}
					if (GxContext.IsHttpContext &&
						File.Exists(GxContext.StaticPhysicalPath() + "bin/client.exe.config"))
					
					{

						logConfig("bin/log.config");
						if (log.IsDebugEnabled)
							loadedConfigFile = GxContext.StaticPhysicalPath() + "bin/client.exe.config";
						_config = loadConfig("bin/client.exe.config");
						return _config;
					}
					if (File.Exists("client.exe.config"))
					{
						logConfig("log.console.config");
						if (log.IsDebugEnabled) loadedConfigFile = Path.GetFullPath("client.exe.config");
						_config = loadConfig("client.exe.config");
					}
					else
					{
						string file = FileUtil.GetStartupDirectory() + "/client.exe.config";
						string logFile = FileUtil.GetStartupDirectory() + "/log.console.config";
						logConfig(logFile);
						if (log.IsDebugEnabled) loadedConfigFile = Path.GetFullPath(file);
						_config = loadConfig(file);

					}

#else
					string basePath = FileUtil.GetBasePath();
					string currentDir = Directory.GetCurrentDirectory();
					string startupDir = FileUtil.GetStartupDirectory();
					string appSettings = "appsettings.json";
					string clientConfig = "client.exe.config";
					string logConfigFile = GxContext.IsHttpContext ? "log.config" : "log.console.config";

					if (File.Exists(Path.Combine(basePath, appSettings)))
					{
						_config = loadConfigJson(basePath, appSettings);
						logConfig(Path.Combine(basePath, logConfigFile));
					}
					else if (File.Exists(appSettings))
					{
						_config = loadConfigJson(currentDir, appSettings);
						logConfig(logConfigFile);
					}
					else if (File.Exists(Path.Combine(startupDir, appSettings)))
					{
						_config = loadConfigJson(startupDir, appSettings);
						logConfig(Path.Combine(startupDir, logConfigFile));
					}
					else if (File.Exists(clientConfig))
					{
						_config = loadConfig(clientConfig, out logConfigSource);
						if (!string.IsNullOrEmpty(logConfigSource))
							logConfig(logConfigSource);
						else
							logConfig(logConfigFile);
					}
					try
					{
						Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
					}
					catch (Exception ex)
					{
						GXLogging.Info(log, "Could not register encoding provider", ex.Message);
					}
#endif
				}
				return _config;
			}
		}

#if NETCORE
		public static string ScriptPath { get; set; }
		static NameValueCollection loadConfigJson(string baseDir, string appSettings)
		{
			if (ConfigRoot == null)
			{
				var builder = new ConfigurationBuilder()
					.SetBasePath(baseDir)
					.AddJsonFile(appSettings, optional: false, reloadOnChange: true)

					.AddEnvironmentVariables();
				ConfigRoot = builder.Build();
			}
			languages = new Hashtable(StringComparer.OrdinalIgnoreCase);
			customErrors = new ConcurrentDictionary<string, string>();
			NameValueCollection cfg = new NameValueCollection(StringComparer.Ordinal); //Case sensitive
			foreach (var c in ConfigRoot.GetSection("appSettings").GetChildren())
			{
				string key = c.Key;
				cfg.Add(key, MappedValue(key, c.Value));
			}

			foreach (var c in ConfigRoot.GetSection("languages").GetChildren())
			{
				languages[c.Key] = new Hashtable(StringComparer.OrdinalIgnoreCase);
				Hashtable language = (Hashtable)languages[c.Key];
				foreach (var prop in c.GetChildren())
				{
					language[prop.Key] = prop.Value;
				}
			}
			foreach (var c in ConfigRoot.GetSection("customErrors").GetChildren())
			{
				customErrors[c.Key] = c.Value;
			}
			return cfg;
		}
#endif
		private static void logConfig(string filename)
		{
			if (configLog)
			{
				try
				{
#if NETCORE
					if (filename != null && File.Exists(filename))
					{
						var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
						XmlConfigurator.ConfigureAndWatch(logRepository, new FileInfo(filename));
						GXLogging.Debug(log, "DOMConfigurator log4net configured with ", filename);
					}
#else
					if (filename != null)
					{
						XmlConfigurator.ConfigureAndWatch(new FileInfo(filename));
						GXLogging.Debug(log, "DOMConfigurator log4net configured with ", filename);
					}
					else
					{
						XmlConfigurator.Configure();
						GXLogging.Debug(log, "DOMConfigurator log4net configured with web.config");
					}
#endif
					GXLogging.Debug(log, "GxClasses version:", GxContext.StdClassesVersion());
				}
				catch (Exception ex)
				{
					Console.WriteLine("Could not load log4net configuration: " + ex.Message);
				}
				configLog = false;
			}
		}

		static NameValueCollection loadConfig(string filename)
		{
			string logConfigSource;
			return loadConfig(filename, out logConfigSource);
		}
		static NameValueCollection loadConfig(string filename, out string logConfigSource)
		{
			GXLogging.Debug(log, "Start loadConfig, filename '", filename, "'");
			NameValueCollection cfg = new NameValueCollection(StringComparer.Ordinal); //Case sensitive
			logConfigSource = null;
			if (!File.Exists(filename))
				return cfg;
#pragma warning disable CA3075 // Insecure DTD processing in XML
			using (XmlReader rdr = XmlReader.Create(filename))
#pragma warning restore CA3075 // Insecure DTD processing in XML
			{
				languages = new Hashtable(StringComparer.OrdinalIgnoreCase);

				while (rdr.Read())
				{
					if (rdr.IsStartElement())
					{
						if (rdr.Name.Equals("datastores"))
						{
							while (rdr.Read() && !(!rdr.IsStartElement() && rdr.Name.Equals("datastores")))
								if (!(rdr.IsStartElement() && rdr["key"] == null) && (rdr.IsStartElement()))
								{
									string key = rdr["key"];
									cfg.Add(key, MappedValue(key, rdr["value"]));
								}
						}
						else if (rdr.Name.Equals("appSettings"))
						{
							while (rdr.Read() && rdr.IsStartElement())
							{
								string key = rdr["key"];
								cfg.Add(key, MappedValue(key, rdr["value"]));
							}
						}
						else if (rdr.Name.Equals("log4net") && rdr.IsStartElement())
							logConfigSource = rdr["configSource"];
						else if (rdr.Name.Equals("languages"))
							while (rdr.Read() && rdr.IsStartElement())
							{
								languages[rdr.Name] = new Hashtable(StringComparer.OrdinalIgnoreCase);
								Hashtable language = (Hashtable)languages[rdr.Name];
								language["code"] = rdr["code"];
								language["time_fmt"] = rdr["time_fmt"];
								language["decimal_point"] = rdr["decimal_point"];
								language["date_fmt"] = rdr["date_fmt"];
								language["culture"] = rdr["culture"];
								language["thousand_sep"] = rdr["thousand_sep"];
							}
					}
				}
			}
			GXLogging.Debug(log, "Return loadConfig");
			return cfg;
		}
		private static string MappedValue(string key, string value)
		{
			string mappedValue = value;
			string mappedKey = GetMappedProperty(key);
			if (EnvVarReader.GetEnvironmentValue(mappedKey, out string envVarValue))
				mappedValue = envVarValue;
			return mappedValue;

		}
	}
	public class Preferences
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Configuration.Preferences));
		static object syncRoot = new Object();
		static Hashtable cachingTtl;
		static string _applicationPath = "";
		static int maximumOpenCursors;
		static int compatibleEmptyString = -1;
		static int blankStringAsEmpty = -1;
		static int useBase64ViewState = -1;
		static int oldSTR = -1;
		static int instrumented = -1;
		static string mediaPath;
		static string blobPath;
		static string blobPathFolderName;
		static int blankEmptyDates = -1;
		static int ignoreAddOnEmptyDate = -1;
		static int setupDB = -1;
		static HTMLDocType docType = HTMLDocType.UNDEFINED;
		static int docTypeDTD = -1;
		static string remoteLocation;
		static int remote = -1;
		static int storageTimezone = -1;
		static int gxpmEnabled = -1;
		static int rewriteEnabled = -1;
		private static int exposeMetadata = -1;
		public static string DefaultRewriteFile = "rewrite.config";
		const string USE_NAMED_PARAMETERS = "UseNamedParameters";
		const string REST_DATES_WITH_MILLIS = "REST_DATES_WITH_MILLIS";
		const string YES = "1";
		const string NO = "0";
		static string defaultDatastore;
		const string DEFAULT_DS = "Default";
		static int httpclient_max_per_route = -1;
		internal static string DefaultDatastore
		{
			get
			{
				if (defaultDatastore == null)
				{
					if (Config.GetValueOf("DataStore-Default", out string strDefaultDS))
					{
						defaultDatastore = strDefaultDS;
					}
					else
					{
						defaultDatastore = DEFAULT_DS;
					}
				}
				return defaultDatastore;
			}
			set
			{
				defaultDatastore = value;
			}
		}
		public static string RemoteLocation
		{
			get
			{
				if (Remote) return remoteLocation;
				else return null;
			}
		}
		public static bool DocTypeDTD
		{
			get
			{
				string value;
				if (docTypeDTD == -1)
				{
					if (Config.GetValueOf("DocumentTypeDTD", out value) && value == "0")
						docTypeDTD = 0;
					else
						docTypeDTD = 1;
				}
				return (docTypeDTD == 1);
			}
		}
		public static bool GxpmEnabled
		{
			get
			{
				string value;
				if (gxpmEnabled == -1)
				{
					if (Config.GetValueOf("GX_FLOW_ENABLED", out value) && value == "1")
						gxpmEnabled = 1;
					else
						gxpmEnabled = 0;
				}
				return (gxpmEnabled == 1);
			}
		}
		public static bool RewriteEnabled
		{
			get
			{
				if (rewriteEnabled == -1)
				{
#if NETCORE
					var basePath = FileUtil.GetBasePath();
#else
					var basePath = Directory.GetParent(FileUtil.GetStartupDirectory()).FullName;
#endif
					string rewriteFile = Path.Combine(basePath, DefaultRewriteFile);
					rewriteEnabled = File.Exists(rewriteFile)?1:0;
				}
				return (rewriteEnabled == 1);
			}
		}
		public static bool UseNamedParameters
		{
			get
			{
				return Config.GetValueOf(USE_NAMED_PARAMETERS, YES) == YES;
			}
		}
		public static bool WFCDateTimeMillis
		{
			get
			{
				return Config.GetValueOf(REST_DATES_WITH_MILLIS, NO) == YES;
			}
		}
		public static bool MustSetupDB()
		{
			if (setupDB == -1)
			{
				string val;
				if (Config.GetValueOf("SETUP_DB", out val) && val == "1")
				{
					setupDB = 1;
					return true;
				}
				else
				{
					setupDB = 0;
					return false;
				}
			}
			else return (setupDB == 1);
		}
		
		public static HTMLDocType DocType
		{
			get
			{
				if (docType == HTMLDocType.UNDEFINED)
				{
					string value;
					docType = HTMLDocType.NONE;
					if (Config.GetValueOf("DocumentType", out value))
					{
						if (value.StartsWith("XHTML"))
							docType = HTMLDocType.XHTML1;
						else if (value.StartsWith("HTML4S"))
							docType = HTMLDocType.HTML4S;
						else if (value.StartsWith("HTML4"))
							docType = HTMLDocType.HTML4;
						else if (value.StartsWith("HTML5"))
							docType = HTMLDocType.HTML5;
					}
				}
				return docType;
			}
		}

		public static bool Remote
		{
			get
			{
				if (remote == -1)
				{
					if (Config.GetValueOf("GXDB_LOCATION", out remoteLocation))
					{
						remote = 1;
						return true;
					}
					else
					{
						remote = 0;
						return false;
					}
				}
				else return (remote == 1);
			}

		}
		public static bool IgnoreAddOnEmptyDates
		{
			get
			{
				if (ignoreAddOnEmptyDate == -1)
				{
					string val;
					if (Config.GetValueOf("IGNORE_ADD_ON_EMPTY_DATE", out val) && val == "1")
					{
						ignoreAddOnEmptyDate = 1;
						return true;
					}
					else
					{
						ignoreAddOnEmptyDate = 0;
						return false;
					}
				}
				else return (ignoreAddOnEmptyDate == 1);
			}

		}
		
		public static bool BlankEmptyDates
		{
			get
			{
				if (blankEmptyDates == -1)
				{
					string val;
					if (Config.GetValueOf("BLANK_EMPTY_DATE", out val) && val == "1")
					{
						blankEmptyDates = 1;
						return true;
					}
					else
					{
						blankEmptyDates = 0;
						return false;
					}
				}
				else return (blankEmptyDates == 1);
			}

		}
		public static bool ExposeMetadata
		{
			get
			{
				if (exposeMetadata == -1)
				{
					string val;
					if (Config.GetValueOf("EXPOSE_METADATA", out val) && val == "1")
					{
						exposeMetadata = 1;
						return true;
					}
					else
					{
						exposeMetadata = 0;
						return false;
					}
				}
				else return (exposeMetadata == 1);
			}

		}
		public static bool Instrumented
		{
			get
			{
				if (instrumented == -1)
				{

					string instr;
					if (!GxContext.isReorganization && Config.GetValueOf("ENABLE_MANAGEMENT", out instr) && instr.Equals("1"))
						instrumented = 1;
					else instrumented = 0;
				}
				return (instrumented == 1);
			}
		}
		public static bool CompatibleEmptyStringAsNull()
		{
			string data;
			if (compatibleEmptyString == -1)
			{
				if (Config.GetValueOf("CompatibleEmptyStringAsNull", out data) && data.Trim().Equals("1"))
				{
					compatibleEmptyString = 1;
				}
				else
				{
					compatibleEmptyString = 0;
				}
			}
			return (compatibleEmptyString == 1);
		}

		public static bool BlankStringAsEmpty()
		{
			string data;
			if (blankStringAsEmpty == -1)
			{
				if (Config.GetValueOf("BlankStringAsEmpty", out data) && data.Trim().Equals("1"))
				{
					blankStringAsEmpty = 1;
				}
				else
				{
					blankStringAsEmpty = 0;
				}
			}
			return (blankStringAsEmpty == 1);
		}
		public static bool UseBase64ViewState()
		{
			string data;
			if (useBase64ViewState == -1)
			{
				if (Config.GetValueOf("UseBase64ViewState", out data) && data.Trim().Equals("y"))
				{
					useBase64ViewState = 1;
				}
				else
				{
					useBase64ViewState = 0;
				}
			}
			return (useBase64ViewState == 1);
		}
		public static bool HttpProtocolSecure()
		{
			string val;
			if (Config.GetValueOf("HTTP_PROTOCOL", out val))
				if (val == "Secure")
					return true;
			return false;
		}
		public static bool OldNtoC()
		{
			string data;
			if (oldSTR == -1)
			{
				if (Config.GetValueOf("OLD_STR", out data) && data.Trim().Equals("1"))
				{
					oldSTR = 1;
				}
				else
				{
					oldSTR = 0;
				}
			}
			return (oldSTR == 1);
		}
		public static string getTMP_MEDIA_PATH()
		{
			bool defaultPath = true;
			if (mediaPath == null)
			{
				lock (syncRoot)
				{
					if (mediaPath == null)
					{
						if (Config.GetValueOf("TMPMEDIA_DIR", out mediaPath))
						{
							mediaPath = mediaPath.Trim();

							if (!String.IsNullOrEmpty(mediaPath) && !mediaPath.EndsWith("\\") && !mediaPath.EndsWith("/"))
							{
								mediaPath += Path.DirectorySeparatorChar;
							}
							if (mediaPath.StartsWith("http"))
							{
								return mediaPath;
							}
						}
						else
						{
							mediaPath = "";
						}
						if (!String.IsNullOrEmpty(mediaPath))
							defaultPath = false;
						if (GXServices.Instance == null || GXServices.Instance.Get(GXServices.STORAGE_SERVICE) == null)
						{
							if (defaultPath || !Path.IsPathRooted(mediaPath))
								mediaPath = Path.Combine(GxContext.StaticPhysicalPath(), mediaPath) + Path.DirectorySeparatorChar;
						}
						else
							mediaPath = mediaPath.Replace("\\", "/");
						try
						{
							GxDirectory directory = new GxDirectory(GxContext.StaticPhysicalPath(), mediaPath);

							if (!defaultPath)
								GXFileWatcher.Instance.AsyncDeleteFiles(directory);

							if (!directory.Exists())
								directory.Create();
						}
						catch (Exception ex)
						{
							GXLogging.Error(log, "Error creating TMPMEDIA_DIR " + mediaPath, ex);
						}
						GXLogging.Debug(log, "TMP_MEDIA_PATH:", mediaPath);
					}
				}
			}
			return mediaPath;
		}
		public static string getPRINT_LAYOUT_METADATA_DIR()
		{
			if (mediaPath == null)
			{
				lock (syncRoot)
				{
					if (mediaPath == null)
					{
						if (Config.GetValueOf("PRINT_LAYOUT_METADATA_DIR", out mediaPath))
						{
							mediaPath = mediaPath.Trim();

							if (!String.IsNullOrEmpty(mediaPath) && !mediaPath.EndsWith("\\") && !mediaPath.EndsWith("/"))
							{
								mediaPath += Path.DirectorySeparatorChar;
							}
						}
						else
						{
							mediaPath = "";
						}
						GXLogging.Debug(log, "PRINT_LAYOUT_METADATA_DIR:", mediaPath);
					}
				}
			}
			return mediaPath;
		}
		public enum StorageTimeZonePty { Undefined = 0, Utc = 1, Local = 2 };

		public static Boolean useTimezoneFix()
		{
			return getStorageTimezonePty() != StorageTimeZonePty.Undefined;
		}

		public static StorageTimeZonePty getStorageTimezonePty()
		{
			if (storageTimezone == -1)
			{
				string sValue;
				storageTimezone = (int)StorageTimeZonePty.Undefined;
				if (Config.GetValueOf("StorageTimeZone", out sValue))
					int.TryParse(sValue, out storageTimezone);

			}
			return (StorageTimeZonePty)storageTimezone;
		}

		public static void setStorageTimezonePty(StorageTimeZonePty value)
		{
			storageTimezone = (int)value;
		}

		public static string getBLOB_PATH_SHORT_NAME()
		{
			return blobPathFolderName;
		}
		public static string getBLOB_PATH()
		{
			bool defaultPath = true;
			if (blobPath == null)
			{
				lock (syncRoot)
				{
					if (blobPath == null)
					{
						if (Config.GetValueOf("CS_BLOB_PATH", out blobPath))
						{
							blobPath = blobPath.Trim();
							if (!String.IsNullOrEmpty(blobPath) && !blobPath.EndsWith("\\") && !blobPath.EndsWith("/"))
							{
								blobPath += Path.DirectorySeparatorChar;
							}

							if (blobPath.StartsWith("http"))
							{
								return blobPath;
							}
						}
						else
						{
							blobPath = "";
						}
						if (!String.IsNullOrEmpty(blobPath))
							defaultPath = false;
						if (String.IsNullOrEmpty(blobPath) || !Path.IsPathRooted(blobPath))
						{
							blobPathFolderName = String.IsNullOrEmpty(blobPath) ? blobPath : blobPath.TrimEnd('/', '\\').TrimStart('/', '\\');
							if (GXServices.Instance == null || GXServices.Instance.Get(GXServices.STORAGE_SERVICE) == null)
							{
								blobPath = Path.Combine(GxContext.StaticPhysicalPath(), blobPath);
								if (blobPath[blobPath.Length - 1] != Path.DirectorySeparatorChar)
									blobPath += Path.DirectorySeparatorChar;
							}
							else
								blobPath = blobPath.Replace("\\", "/");
						}
						try
						{

							GxDirectory directory = new GxDirectory(GxContext.StaticPhysicalPath(), blobPath);

							if (!defaultPath)
								GXFileWatcher.Instance.AsyncDeleteFiles(directory);

							if (!directory.Exists())
								directory.Create();

							string multimediaPath = Path.Combine(blobPath, GXDbFile.MultimediaDirectory);
							GxDirectory multimediaDirectory = new GxDirectory(GxContext.StaticPhysicalPath(), multimediaPath);
							if (!multimediaDirectory.Exists())
								multimediaDirectory.Create();
						}
						catch (Exception ex)
						{
							GXLogging.Error(log, "Error creating CS_BLOB_PATH " + blobPath, ex);
						}

						GXLogging.Debug(log, "BLOB_PATH:", blobPath);
					}
				}
			}
			return blobPath;
		}

		public static int CachingTTLs(int category)
		{
			if (cachingTtl == null)
			{
				lock (syncRoot)
				{
					if (cachingTtl == null)
					{
						cachingTtl = new Hashtable();
						bool exist = true;
						string cache_ttl;
						for (int i = 0; exist; i++)
						{
							exist = Config.GetValueOf("CACHE_TTL_" + i, out cache_ttl);
							if (exist)
								cachingTtl[i] = Convert.ToInt32(cache_ttl);
						}
					}
				}
			}
			if (cachingTtl.Contains(category))
				return (int)cachingTtl[category];
			else
				return DefaultCacheValue(category);
		}
		private static int DefaultCacheValue(int category)
		{
			switch (category)
			{
				case 0: return 0;
				case 1: return 60;
				case 2: return 600;
				case 3: return -1;
				default: return -1;
			}
		}
		public static Hashtable CachingTTLs()
		{
			return cachingTtl;
		}

		public static string ApplicationPath
		{
			get { return _applicationPath; }
			set { _applicationPath = value; }
		}

		internal static bool CorsEnabled {
			get {
				return !string.IsNullOrEmpty(CorsAllowedOrigins());
			}
		}

		internal static string CorsAllowedOrigins()
		{
			if (Config.GetValueOf("CORS_ALLOW_ORIGIN", out string corsOrigin))
				return corsOrigin;
			else
				return string.Empty;
		}
		public static int GetMaximumOpenCursors()
		{
			if (maximumOpenCursors == 0)
			{
				try
				{
					string strmax;
					if (Config.GetValueOf("MAX_CURSOR", out strmax))
					{
						maximumOpenCursors = Convert.ToInt32(strmax);
					}
					else
					{
						maximumOpenCursors = 100;
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "MaximumOpenCursors error", ex);
					maximumOpenCursors = 100;
				}
			}
			return maximumOpenCursors;

		}

		public static string GetDefaultTheme()
		{
			string theme = "";
			if (Config.GetValueOf("Theme", out theme))
				return theme;
			else
				return "";
		}

		public static int GetHttpClientMaxConnectionPerRoute()
		{
			if (httpclient_max_per_route == -1)
			{
				try
				{
					string strmax;
					if (Config.GetValueOf("HTTPCLIENT_MAX_PER_ROUTE", out strmax))
					{
						httpclient_max_per_route = Convert.ToInt32(strmax);
					}
					else
					{
						httpclient_max_per_route = 1000;
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "HttpClientMaxPerRoute error", ex);
					httpclient_max_per_route = 1000;
				}
			}
			return httpclient_max_per_route;

		}

	}
}
