using System;
using System.Reflection;
using GeneXus.Configuration;
using log4net;
using GeneXus.Utils;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net;
using GeneXus.Mime;
using GeneXus.Http;
using System.Collections.Generic;
using GeneXus.Application;
using System.IO;

namespace GeneXus.HttpHandlerFactory
{
	public class AppSettings
	{
		public BaseUrls BaseUrls { get; set; }
		public bool AnalyticsEnabled { get; set; }
		public int SessionTimeout { get; set; }
	}

	public class BaseUrls
	{
		public string Api { get; set; }
		public string Auth { get; set; }
		public string Web { get; set; }
	}

	public class HandlerFactory
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.HttpHandlerFactory.HandlerFactory));
		private readonly AppSettings _appSettings;
		private string _basePath;
		static Dictionary<string, Type> _aspxObjects = new Dictionary<string, Type>(){
												{"gxresourceprovider",typeof(GXResourceProvider)},
												{"gxobject",typeof(GXObjectUploadServices)},
												{"gxoauthlogout",typeof(GXOAuthLogout)},
												{"gxoauthuserinfo",typeof(GXOAuthUserInfo)},
												{"gxoauthaccesstoken",typeof(GXOAuthAccessToken)},
												{"gx_valid_service",typeof(GXValidService)},
												{"gxmulticall",typeof(GXMultiCall)}};
		static Dictionary<string, string> _aspxRewrite = new Dictionary<string, string>(){
												{"oauth/access_token","gxoauthaccesstoken"},
												{"oauth/logout","gxoauthlogout"},
												{"oauth/userinfo","gxoauthuserinfo"},
												{"oauth/gam/signin","agamextauthinput"},
												{"oauth/gam/callback","agamextauthinput"},
												{"oauth/gam/access_token","agamoauth20getaccesstoken"},
												{"oauth/gam/userinfo","agamoauth20getuserinfo"},
												{"oauth/gam/signout","agamextauthinput"},
												{"saml/gam/signin","Saml2/SignIn"},
												{"saml/gam/callback","gamexternalauthenticationinputsaml20_ws"},
												{"saml/gam/signout","Saml2/Logout"},
												{"oauth/requesttokenservice","agamstsauthappgetaccesstoken"},
												{"oauth/queryaccesstoken","agamstsauthappvalidaccesstoken"},
												{"oauth/gam/v2.0/access_token","agamoauth20getaccesstoken_v20"},
												{"oauth/gam/v2.0/userinfo","agamoauth20getuserinfo_v20"},
												{"oauth/gam/v2.0/RequestTokenAndUserinfo","aGAMSSORestRequestTokenAndUserInfo_v20"}};
		private const string QUERYVIEWER_NAMESPACE = "QueryViewer.Services";
		private static List<string> GxNamespaces;
		public HandlerFactory(RequestDelegate next, IOptions<AppSettings> appSettings)
		{
			_appSettings = appSettings.Value; 
		}
		public HandlerFactory(RequestDelegate next, IOptions<AppSettings> appSettings, String basePath)
		{
			_basePath = basePath;
			_appSettings = appSettings.Value;
		}


		public async Task Invoke(HttpContext context)
		{
			try
			{
				context.NewSessionCheck();
				var url = context.Request.Path.Value;

				IHttpHandler handler = GetHandler(context, context.Request.Method, ObjectUrl(context.Request.Path.Value, _basePath), string.Empty);
				context.Response.OnStarting(() =>
				{
					if (context.Response.StatusCode == (int)HttpStatusCode.OK && url.EndsWith(".aspx") && string.IsNullOrEmpty(context.Response.ContentType))
					{
						context.Response.ContentType = MediaTypesNames.TextHtml;
						//If no ContentType is specified, the default is text/HTML.
					}
					handler.sendAdditionalHeaders();
					return Task.CompletedTask;
				});

				handler.ProcessRequest(context);
				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Task.FromException(ex);
			}
		}
		public static bool IsAspxHandler(string path, string basePath)
		{
			var name = ObjectUrl(path, basePath);
			return name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase) || _aspxObjects.ContainsKey(name);
		}
		private static string ObjectUrl(string requestPath, string basePath) 
		{
			var lastSegment = requestPath;
			if (!string.IsNullOrEmpty(basePath) && lastSegment.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
			{
				lastSegment = lastSegment.Remove(0, basePath.Length);
			}
			lastSegment = lastSegment.TrimStart('/');
			GXLogging.Debug(log, "ObjectUrl:", lastSegment);
			if (_aspxRewrite.ContainsKey(lastSegment))
			{
				return _aspxRewrite[lastSegment];
			}
			return lastSegment;
		}
		public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
		{
			GXLogging.Debug(log, "GetHandler url:", url);

			IHttpHandler handlerToReturn =null;

			var idx = url.LastIndexOf('.');
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
				className = $"{QUERYVIEWER_NAMESPACE}.{cname}";
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
			if (objType ==null)
				throw new Exception("GeneXus HttpHandlerFactory error: Could not create " + className + " (assembly: " + assemblyName + ").");

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
                    if (context != null && !binPath.EndsWith("bin")){
                        binPath = Path.Combine(binPath, "bin"); 
                    }
					string[] files = Directory.GetFiles(binPath, "*.Common.dll", SearchOption.TopDirectoryOnly);
                    if (files == null || files.Length == 0)
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
}
