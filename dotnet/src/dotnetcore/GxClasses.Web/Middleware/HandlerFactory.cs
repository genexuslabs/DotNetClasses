using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Http;
using GeneXus.Mime;
using GeneXus.Utils;
using Microsoft.AspNetCore.Http;

namespace GeneXus.HttpHandlerFactory
{
	public class AppSettings
	{
		public BaseUrls BaseUrls { get; set; }
		public bool AnalyticsEnabled { get; set; }
		public int SessionTimeout { get; set; }
		public int MaxFileUploadSize { get; set; }
	}

	public class BaseUrls
	{
		public string Api { get; set; }
		public string Auth { get; set; }
		public string Web { get; set; }
	}

	public class HandlerFactory
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<HandlerFactory>();
		private string _basePath;
		static Dictionary<string, Type> _aspxObjects = new Dictionary<string, Type>(){
												{"gxoauthlogout",typeof(GXOAuthLogout)},
												{"gxoauthuserinfo",typeof(GXOAuthUserInfo)},
												{"gxoauthaccesstoken",typeof(GXOAuthAccessToken)},
												{"gxmulticall",typeof(GXMultiCall)}};
		private static List<string> GxNamespaces;

		public HandlerFactory()
		{
		}
		public HandlerFactory(RequestDelegate next)
		{
		
		}
		public HandlerFactory(RequestDelegate next, String basePath)
		{
			_basePath = basePath;
		}


		public async Task Invoke(HttpContext context)
		{
			IHttpHandler handler=null;
			string url = string.Empty;
			try
			{				
				context.NewSessionCheck();
				url = context.Request.Path.Value;

				handler = GetHandler(context, context.Request.Method, ObjectUrl(url, _basePath), string.Empty);
				if (handler != null)
				{
					context.Response.OnStarting(() =>
					{
						if (context.Response.StatusCode == (int)HttpStatusCode.OK && url.EndsWith(HttpHelper.ASPX) && string.IsNullOrEmpty(context.Response.ContentType))
						{
							context.Response.ContentType = MediaTypesNames.TextHtml;
							//If no ContentType is specified, the default is text/HTML.
						}
						handler.sendAdditionalHeaders();
						return Task.CompletedTask;
					});
					handler.ProcessRequest(context);
					await Task.CompletedTask;
					handler.ControlOutputWriter?.Flush();
				}
				else
				{
					await Task.FromException(new PageNotFoundException(url));
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Handler Factory failed creating {url}", ex);
				await Task.FromException(ex);
			}
		}
		public static bool IsAspxHandler(string path, string basePath)
		{
			string name = ObjectUrl(path, basePath);
			return name.EndsWith(HttpHelper.ASPX, StringComparison.OrdinalIgnoreCase) || _aspxObjects.ContainsKey(name);
		}
		private static string ObjectUrl(string requestPath, string basePath) 
		{
			string lastSegment = requestPath;
			if (!string.IsNullOrEmpty(basePath) && lastSegment.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
			{
				lastSegment = lastSegment.Remove(0, basePath.Length);
			}
			lastSegment = CleanUploadUrlSuffix(lastSegment.TrimStart('/')).ToLower();
			GXLogging.Debug(log, "ObjectUrl:", lastSegment);
			if (HttpHelper.GAMServices.ContainsKey(lastSegment))
			{
				return HttpHelper.GAMServices[lastSegment];
			}
			return lastSegment;
		}
		private static string CleanUploadUrlSuffix(string url)
		{
			if (url.EndsWith($"{HttpHelper.ASPX}{HttpHelper.GXOBJECT}", StringComparison.OrdinalIgnoreCase))
			{
				return url.Substring(0, url.Length - (HttpHelper.GXOBJECT.Length));
			}
			else
				return url;
		}
		public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
		{
			GXLogging.Debug(log, "GetHandler url:", url);

			IHttpHandler handlerToReturn =null;

			int idx = url.LastIndexOf('.');
			string cname0;
			if (idx >= 0)
				cname0 = url.Substring(0, url.LastIndexOf('.')).ToLower();
			else
				cname0 = url.Substring(0).ToLower();

			if (_aspxObjects.ContainsKey(cname0))
			{
				Type t = _aspxObjects[cname0];
				return Activator.CreateInstance(t) as IHttpHandler;
			}
			string assemblyName = cname0;
			string cname = cname0;
            if (cname.EndsWith( "_bc_ws"))
            {
                // BC web service can be called with _ws for compatibility with 9.0. Remove "_ws" to resolve class
                cname = cname.Substring(0, cname.Length - 3);
                assemblyName = cname;
            }
			string mainNamespace = null;
			string className;
			if (cname.StartsWith("agxpl_", StringComparison.OrdinalIgnoreCase) || cname.Equals("gxqueryviewerforsd", StringComparison.OrdinalIgnoreCase))
			{
				className = $"{HttpHelper.QUERYVIEWER_NAMESPACE}.{cname}";
			}
			else if (Preferences.GxpmEnabled && (cname.StartsWith("awf", StringComparison.OrdinalIgnoreCase) || cname.StartsWith("wf", StringComparison.OrdinalIgnoreCase) || cname.StartsWith("apwf", StringComparison.OrdinalIgnoreCase)))
			{
				className = $"{HttpHelper.GXFLOW_NSPACE}.{cname}";
			}
			else if (HttpHelper.GamServicesInternalName.Contains(cname))
			{
				className = $"{HttpHelper.GAM_NSPACE}.{cname}";
			}
			else
			{
				if (Config.GetValueOf("AppMainNamespace", out mainNamespace))
					className = mainNamespace + "." + cname;
				else
					className = "GeneXus.Programs." + cname;
			}

			Type objType = GetHandlerType(assemblyName, className);
			if (objType == null)
			{
				if (mainNamespace == null)
					mainNamespace = "";
				List<string> namespaces = GetGxNamespaces(context, mainNamespace);
				foreach (string gxNamespace in namespaces)
				{
					className = gxNamespace + "." + cname;
					objType = GetHandlerType(assemblyName, className);
					if (objType != null)
					{
						break;
					}
				}
			}
            if (objType != null)
            {
				try
				{
					handlerToReturn = (IHttpHandler)Activator.CreateInstance(objType, null);
				}
				catch (Exception e)
				{
					GXLogging.Error(log, "GeneXus HttpHandlerFactory error: Could not create " + className + " (assembly: " + assemblyName + ").", e);
					GXLogging.Error(log, "Inner Exception", e.InnerException);
					throw e;
				}
            }
			return handlerToReturn;
		}
		internal static List<string> GetGxNamespaces(HttpContext context, string mainNamespace)
		{
			if (GxNamespaces == null)
			{
				GxNamespaces = new List<string>();
				try
				{
                    string binPath = GxContext.StaticPhysicalPath();
					GXLogging.Debug(log, $"binPath at GetGXNamespaces {binPath}");
					if (context != null && !binPath.EndsWith("bin") && !GxContext.IsAzureContext){
                        binPath = Path.Combine(binPath, "bin"); 
                    }
					string[] files = Directory.GetFiles(binPath, "*.Common.dll", SearchOption.TopDirectoryOnly);
                    if ((files == null || files.Length == 0) && (!GxContext.IsAzureContext))
                    {
                        binPath = Path.Combine(binPath, "bin"); 
                        files = Directory.GetFiles(binPath, "*.Common.dll", SearchOption.TopDirectoryOnly);
                    }
					foreach (string file in files)
					{
						try
						{
							Assembly commonAsmb = Assembly.LoadFile(file);
							if (IsGxCommonAssembly(commonAsmb))
							{
								Type[] types = commonAsmb.GetTypes();
								foreach (Type type in types)
								{
									if (type.Name == "GxModelInfoProvider" && type.Namespace != mainNamespace)
									{
										GxNamespaces.Add(type.Namespace);
										break;
									}
									if (type.Name == "GxModelInfoProvider" && type.Namespace == mainNamespace)
									{
										break;
									}
								}
							}
						}
						catch (Exception e) 
						{
							GXLogging.Error(log, "GxNamespaces Load Exception #2", e);
						}
					}
				}
				catch (Exception e)
				{
					GXLogging.Error(log, "GxNamespaces Load Exception #1", e);
				}
			}
			return GxNamespaces;
		}
		private static bool IsGxCommonAssembly(Assembly asmb)
		{
			object[] atts = asmb.GetCustomAttributes(typeof(GeneXusCommonAssemblyAttribute), false);
			return (atts.Length > 0);
		}
		internal static Type GetHandlerType(string assemblyName, string className)
		{
			Type objType = null;
			try
			{
				
                objType = GeneXus.Metadata.ClassLoader.FindType(assemblyName, className, null);
				if (objType == null)
					
					objType = Assembly.Load(new AssemblyName(assemblyName)).GetType(className);
			}
			catch
			{
			}
			return objType;
		}
		public void ReleaseHandler(IHttpHandler handler)
		{
		}
		
	}
	internal class PageNotFoundException:Exception {
		internal PageNotFoundException(string message):base(message)
		{

		}
	}

}
