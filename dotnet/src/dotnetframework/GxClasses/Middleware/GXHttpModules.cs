using System;
using System.Collections.Generic;
using GeneXus.Configuration;
using System.Web;
using System.IO;
using GeneXus.Utils;
using GeneXus.Application;
using ManagedFusion.Rewriter;
using System.Web.Hosting;
using ManagedFusion.Rewriter.Rules;

namespace GeneXus.Http.HttpModules
{
	public class SingleMap
	{
		String name;
		String implementation;
		String methodName;
		String verb;

		public string Name { get => name; set => name = value; }
		public string ServiceMethod { get => methodName; set => methodName = value; }
		public string Implementation { get => implementation; set => implementation = value; }
		public string Verb { get => verb; set => verb = value; }


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
		public static Dictionary<String, Dictionary<String, String>> servicesMap;
		public static Dictionary<String, Dictionary<String, String>> servicesVerbs;

		const string REST_BASE_URL = "rest/";
		private static bool moduleStarted;

		void IHttpModule.Init(HttpApplication context)
		{
			if (!GXAPIModule.moduleStarted)
			{
				// Load API Map			
				ServicesGroupSetting(GxContext.StaticPhysicalPath());
				GXAPIModule.moduleStarted = true;
			}
			context.PostMapRequestHandler += new EventHandler(context_PostMapRequestHandler);
		}

		void IHttpModule.Dispose()
		{
		}
		
		private void context_PostMapRequestHandler(object sender, EventArgs e)
		{
			HttpApplication httpApp = sender as HttpApplication;
			HttpContext context = httpApp.Context;
			String actualPath = "";			
			if (GXAPIModule.serviceInPath(context.Request.FilePath, actualPath: out actualPath))
			{
				IHttpHandler myHandler = new GeneXus.HttpHandlerFactory.HandlerFactory().GetHandler(context, context.Request.RequestType, context.Request.Url.AbsoluteUri, context.Request.FilePath);
				if (myHandler !=null)
					context.Handler = myHandler;
			}
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
		
		public void ServicesGroupSetting(String webPath)
		{
			if (!String.IsNullOrEmpty(webPath) && servicesMap == null)
			{
				servicesPathUrl = new List<String>();
				servicesBase = new Dictionary<string, string>();				
				servicesMap = new Dictionary<String, Dictionary<string, string>>();
				servicesVerbs = new Dictionary<String, Dictionary<string, string>>();
				servicesClass = new Dictionary<String, String>();

				String[] grpFiles = Directory.GetFiles(webPath, "*.grp.json");
				foreach (String grp in grpFiles)
				{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					object p = JSONHelper.Deserialize<MapGroup>(File.ReadAllText(grp));
#pragma warning restore SCS0018
					MapGroup m = p as MapGroup;
					if (m != null)
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
							if (servicesMap.ContainsKey(mapPathLower))
							{
								if (!servicesMap[mapPathLower].ContainsKey(sm.Name.ToLower()))
								{
									servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm.ServiceMethod);
									servicesVerbs[mapPathLower].Add(sm.Name.ToLower(), (sm.Verb!=null)?sm.Verb:"GET");
								}
							}
							else
							{
								servicesMap.Add(mapPathLower, new Dictionary<string, string>());
								servicesVerbs.Add(mapPathLower, new Dictionary<string, string>());
								servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm.ServiceMethod);
								servicesVerbs[mapPathLower].Add(sm.Name.ToLower(), (sm.Verb != null) ? sm.Verb : "GET");
							}
						}
					}

				}
			}
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
		private static RewriterModule rewriter;
		private static bool moduleStarted;
		private static bool enabled;
		public void Dispose()
		{

		}
		public void Init(HttpApplication context)
		{
			if (!moduleStarted)
			{
				string physicalApplicationPath = null;
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
					Manager.Configuration.Rewriter.AllowIis7TransferRequest = false; //Avoid TOO Many Redirects with inverse urles.
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
