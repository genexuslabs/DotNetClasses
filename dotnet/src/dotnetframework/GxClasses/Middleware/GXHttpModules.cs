using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Utils;
using ManagedFusion.Rewriter;
using ManagedFusion.Rewriter.Engines;
using ManagedFusion.Rewriter.Rules;

namespace GeneXus.Http.HttpModules
{
	public class SingleMap
	{
		string name;
		string implementation;
		string methodName;
		string verb;
		string path;
		string pathregexp;
		Dictionary<string, string> variableAlias;
		public string Name { get => name; set => name = value; }
		public string ServiceMethod { get => methodName; set => methodName = value; }
		public string Implementation { get => implementation; set => implementation = value; }
		public string Verb { get => verb; set => verb = value; }
		public string Path { get => path; set => path = value; } 
		public string PathRegexp { get => pathregexp; set => pathregexp = value; }
		public Dictionary<string, string> VariableAlias { get => variableAlias; set => variableAlias = value; }

	}

	public class MapGroup
	{

		String _objectType;
		String _name;
		String _basePath;
		List<SingleMap> _mappings;

		public string ObjectType { get => _objectType; set => _objectType = value; }
		public string Name { get => _name; set => _name = value; }
		public string BasePath { get => _basePath; set => _basePath = value; }
		public List<SingleMap> Mappings { get => _mappings; set => _mappings = value; }
		
	}

	public class GXAPIModule : IHttpModule
	{

		public static List<String> servicesPathUrl;
		public static Dictionary<String, String> servicesBase;
		public static Dictionary<String, String> servicesClass;
		public static Dictionary<String, Dictionary<string, SingleMap>> servicesMap;

		//public static Dictionary<String, Dictionary<String, String>> servicesVerbs;
		public static Dictionary<String, Dictionary<Tuple<string, string>, String>> servicesMapData = new Dictionary<String, Dictionary<Tuple<string, string>, string>>();

		const string REST_BASE_URL = "rest/";
		const string PRIVATE_DIR = "private";
		private static bool moduleStarted;

		void IHttpModule.Init(HttpApplication context)
		{
			if (!GXAPIModule.moduleStarted)
			{
				// Load API Map			
				ServicesGroupSetting(GxContext.StaticPhysicalPath());
				GXAPIModule.moduleStarted = true;
			}
			context.PostMapRequestHandler += context_PostMapRequestHandler;
			context.PostResolveRequestCache += onPostResolveRequestCache;
		}
		private void onPostResolveRequestCache(object sender, EventArgs eventArgs)
		{
			if (string.Equals(HttpContext.Current.Request.HttpMethod, HttpMethod.Options.Method, StringComparison.OrdinalIgnoreCase))
			{
				IHttpHandler apiHandler = MapHandler(sender, eventArgs);
				if (apiHandler != null)
					HttpContext.Current.RemapHandler(apiHandler);
			}
			else if (string.Equals(HttpContext.Current.Request.HttpMethod, HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase) &&
				HttpContext.Current.Request.Path.EndsWith("/" + REST_BASE_URL, StringComparison.OrdinalIgnoreCase))
			{
				CSRFHelper.ValidateAntiforgery(HttpContext.Current);
			}
		}
		void IHttpModule.Dispose()
		{
		}

		private void context_PostMapRequestHandler(object sender, EventArgs eventArgs)
		{
			HttpApplication httpApp = sender as HttpApplication;
			IHttpHandler apiHandler = MapHandler(sender, eventArgs);
			if (apiHandler != null)
				httpApp.Context.Handler = apiHandler;

		}
		private IHttpHandler MapHandler(object sender, EventArgs e)
		{
			HttpApplication httpApp = sender as HttpApplication;
			HttpContext context = httpApp.Context;
			if (GXAPIModule.serviceInPath(context.Request.FilePath, actualPath: out _))
			{
				return new GeneXus.HttpHandlerFactory.HandlerFactory().GetHandler(context, context.Request.RequestType, context.Request.Url.AbsoluteUri, context.Request.FilePath);
			}
			else
				return null;
		}

		public static Boolean serviceInPath(String path, out String actualPath)
		{
			actualPath = "";
			if (servicesPathUrl != null)
			{
				foreach (String subPath in servicesPathUrl)
				{
					if (path.ToLower().Contains($"/{subPath.ToLower()}"))
					{
						actualPath = subPath.ToLower();
						return true;
					}
				}
			}
			return false;
		}
		
		public void ServicesGroupSetting(string webPath)
		{
			if (!String.IsNullOrEmpty(webPath) && servicesMap == null)
			{				
				servicesPathUrl = new List<string>();
				servicesBase = new Dictionary<string, string>();				
				servicesMap = new Dictionary<string, Dictionary<string, SingleMap>>();
				//servicesVerbs = new Dictionary<string, Dictionary<string, string>>();
				servicesMapData = new Dictionary<string, Dictionary<Tuple<string, string>, string>>();
				servicesClass = new Dictionary<String, String>();

				if (Directory.Exists(Path.Combine(webPath, PRIVATE_DIR))) 
				{
					String[] grpFiles = Directory.GetFiles(Path.Combine(webPath, PRIVATE_DIR), "*.grp.json");
					foreach (String grp in grpFiles)
					{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
						object p = JSONHelper.Deserialize<MapGroup>(File.ReadAllText(grp));
#pragma warning restore SCS0018
						MapGroup m = p as MapGroup;
						if (m != null && m.Name != null && m.Mappings != null )
						{

							if (String.IsNullOrEmpty(m.BasePath))
							{
								m.BasePath = REST_BASE_URL;
							}
							String mapPath = (m.BasePath.EndsWith("/")) ? m.BasePath : m.BasePath + "/";
							String mapPathLower = mapPath.ToLower();
							servicesPathUrl.Add(mapPathLower);
							servicesBase.Add(mapPathLower, m.Name.ToLower());
							servicesClass.Add(mapPathLower, m.Name.ToLower() + "_services");
							foreach (SingleMap sm in m.Mappings)
							{
								if (String.IsNullOrEmpty(sm.Verb))
									sm.Verb = "GET";
								if (sm.VariableAlias == null)
									sm.VariableAlias = new Dictionary<string, string>();
								else
								{
									Dictionary<string, string> vMap = new Dictionary<string, string>();
									foreach (KeyValuePair<string, string> v in sm.VariableAlias)
									{
										vMap.Add(v.Key.ToLower(), v.Value.ToLower());
									}
									sm.VariableAlias = vMap;
								}							
								if (servicesMap.ContainsKey(mapPathLower))
								{
									if (!servicesMap[mapPathLower].ContainsKey(sm.Name.ToLower()))
									{										
										servicesMapData[mapPathLower].Add(Tuple.Create(sm.Path.ToLower(), sm.Verb), sm.Name.ToLower());
										servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm);
									}
								}
								else
								{
									servicesMapData.Add(mapPathLower, new Dictionary<Tuple<string,string>, string>());
									servicesMapData[mapPathLower].Add(Tuple.Create(sm.Path.ToLower(), sm.Verb), sm.Name.ToLower());
									servicesMap.Add(mapPathLower, new Dictionary<string, SingleMap>());
									servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm);
								}							
							
							}
						}

					}
				}
			}
		}
	}
	public class GXSessionModule : IHttpModule
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXSessionModule>();
		HttpApplication App;
		const string ASPNETSESSION_COOKIE = "ASP.NET_SessionId";
		string cookieName= ASPNETSESSION_COOKIE;

		public void Init(HttpApplication app)
		{
			App = app; 
			try
			{
				SessionStateSection sessionStateSection = (SessionStateSection)System.Configuration.ConfigurationManager.GetSection("system.web/sessionState");
				if (sessionStateSection != null)
					cookieName = sessionStateSection.CookieName;
				IHttpModule module = app.Modules["Session"];
				if (module.GetType() == typeof(SessionStateModule))
				{
					SessionStateModule stateModule = (SessionStateModule)module;
					stateModule.Start += (Session_Start);
				}
			}catch(SecurityException ex)
			{
				GXLogging.Info(log, ".NET trust level is lower than full", ex.Message);
				app.EndRequest += Session_Start;
			}
		}

		private void Session_Start(object sender, EventArgs e)
		{
			if (App.Request.GetIsSecureFrontEnd() || App.Request.GetIsSecureConnection() == 1)
			{
				HttpCookie sessionCookie = RetrieveResponseCookie(App.Response, cookieName);

				if (sessionCookie != null && !sessionCookie.Secure)
				{
					sessionCookie.Secure = true;
					App.Response.SetCookie(sessionCookie);
				}
			}
		}
		private HttpCookie RetrieveResponseCookie(HttpResponse currentResponse, string cookieName)
		{
			foreach (string key in App.Response.Cookies.Keys)
			{
				if (key.Equals(cookieName, StringComparison.OrdinalIgnoreCase))
				{
					return App.Response.Cookies[key];
				}
			}
			return null;
		}

		public void Dispose()
		{
			App = null;
		}
	}
	public class GXStaticCacheModule : IHttpModule
    {
        #region IHttpModule Members
        private static string[] cachingTypes = { ".jpg", ".jpeg", ".bmp", ".gif", ".js", ".css" ,".png"};
        private static int cacheExpirationHours;
        private static bool moduleStarted;  

        void IHttpModule.Init(HttpApplication context)
        {
            if (!GXStaticCacheModule.moduleStarted)
            {
                string cacheExpirationHoursS = string.Empty;
                if (Config.GetValueOf("CACHE_CONTENT_EXPIRATION", out cacheExpirationHoursS))
                {
                    Int32.TryParse(cacheExpirationHoursS, out GXStaticCacheModule.cacheExpirationHours);
                }
                GXStaticCacheModule.moduleStarted = true;
            }
            context.EndRequest += new EventHandler(context_EndRequest);
        }

        void context_EndRequest(object sender, EventArgs e)
        {            
            HttpContext context = ((HttpApplication)sender).Context;

			if (context.Request != null && context.Request.RequestType == "GET" && context.Response.StatusCode == 200)
			{
				string filePath = context.Request.FilePath;
				if (filePath.IndexOf(".svc") < 0 && filePath.IndexOf(".aspx") < 0 && isCacheableMimeType(filePath))
				{
					context.Response.Cache.SetCacheability(HttpCacheability.Public);
					context.Response.Cache.SetMaxAge(new TimeSpan(GXStaticCacheModule.cacheExpirationHours, 0, 0));
				}
			}
        }

        private bool isCacheableMimeType(string path)
        {
            string ext = System.IO.Path.GetExtension(path);
            return Array.Exists(GXStaticCacheModule.cachingTypes, element => element.Equals(ext));
        }

        void IHttpModule.Dispose()
        {
        }
        #endregion
    }
	public class GXRewriter : IHttpModule
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXRewriter>();
		private static RewriterModule rewriter;
		private static bool moduleStarted;
		private static bool enabled;
		internal static string physicalApplicationPath;
		public void Dispose()
		{

		}
		public void Init(HttpApplication context)
		{
			if (!moduleStarted)
			{
				try
				{
					physicalApplicationPath = HostingEnvironment.ApplicationPhysicalPath;
				}
			finally
				{
					if (String.IsNullOrEmpty(physicalApplicationPath))
						physicalApplicationPath = GxContext.StaticPhysicalPath();
				}

				if (File.Exists(Path.Combine(physicalApplicationPath, Preferences.DefaultRewriteFile)))
				{
					ChangeApacheDefaultEngine();
					Manager.Configuration.Rewriter.AllowIis7TransferRequest = false; //Avoid Too Many Redirects with inverse urles.
					enabled = true;
				}
				moduleStarted = true;
			}
			if (enabled)
			{
				rewriter = new RewriterModule();
				rewriter.Init(context);
			}
		}
		private void ChangeApacheDefaultEngine()
		{
			try
			{
				GxApacheEngine engine = new GxApacheEngine();
				typeof(Manager).GetField("_rewriterEngine", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, engine);
				engine.Init();
			}catch(Exception ex)
			{
				GXLogging.Error(log, "Error changing ChangeApacheDefaultEngine", ex);
			}
		}

	}
	public class GxApacheEngine : ApacheEngine
	{
		public GxApacheEngine() : base()
		{
		}
		public override void Init()
		{
			Paths.Clear();
			DirectoryInfo refreshDir = new DirectoryInfo(GXRewriter.physicalApplicationPath);
			FileInfo file = new FileInfo(Path.Combine(refreshDir.FullName, Preferences.DefaultRewriteFile));
			Add(HttpContext.Current.Request.ApplicationPath, file);
			RefreshRules();
		}
	}
	public class GxInverseRuleAction: DefaultRuleAction
	{
		public override void Execute(RuleContext context)
		{
			base.Execute(context);
			context.SubstitutedUrl = AddBase(context.RuleSet.VirtualBase, context.SubstitutedUrl);
		}
		public override bool IsMatch(RuleContext context)
		{
			return base.IsMatch(context);
		}
		private Uri AddBase(string baseFrom, Uri url)
		{
			if (!String.IsNullOrEmpty(baseFrom) && baseFrom != "/")
			{
				string urlPath = url.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped);

				if (!urlPath.StartsWith(baseFrom))
					urlPath = baseFrom + urlPath;

				while (urlPath.Contains("//"))
					urlPath = urlPath.Replace("//", "/");

				return new Uri(url, urlPath);
			}
			return url;
		}
	}
}
