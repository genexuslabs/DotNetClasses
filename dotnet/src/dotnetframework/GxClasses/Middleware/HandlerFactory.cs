using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Http;
using GeneXus.Http.HttpModules;
using GeneXus.Metadata;
using GeneXus.Utils;

namespace GeneXus.HttpHandlerFactory
{

	internal class ErrorRequestHandler : IHttpHandler
	{
		string message;
		HttpStatusCode httpCode;

		internal ErrorRequestHandler(string message, HttpStatusCode httpCode)
		{
			this.message = message;
			this.httpCode = httpCode;
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.StatusCode = (int)httpCode;
			context.Response.StatusDescription = message;
			if (context.Request.AcceptTypes.Contains("application/json")) 
			{
				context.Response.ContentType = "application/json";
				HttpHelper.SetError(context, "0", "Method not Allowed");
			}
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}


	internal class OptionsApiObjectRequestHandler : IHttpHandler
	{
		string actualPath;
		string regexpPath;
		string objectName;
		internal OptionsApiObjectRequestHandler(string path, string name, string regexp)
		{
			actualPath = path;
			objectName = name;
			regexpPath = regexp;
		}

		public void ProcessRequest(HttpContext context)
		{
			// OPTIONS VERB
			List<string> mthheaders = new List<string>() { $"{HttpMethod.Options.Method},{HttpMethod.Head.Method}" };
			bool found = false;
			foreach (Tuple<string, string> t in GXAPIModule.servicesMapData[actualPath].Keys)
			{
				if (t.Item1.Equals(objectName.ToLower()) || (GxRegex.IsMatch( t.Item1,regexpPath)))
				{
					mthheaders.Add(t.Item2);
					found = true;
				}
			}
			if (found)
			{
				HttpHelper.CorsHeaders(context);
				HttpHelper.AllowHeader(context, mthheaders);
			}
			else
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			}
		}		

		public bool IsReusable
		{
			get { return false; }
		}
	}

	class HandlerFactory  : IHttpHandlerFactory
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<HandlerFactory>();
		private static List<string> GxNamespaces;

		public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
		{
			IHttpHandler handlerToReturn;

			string relativeURL = context.Request.AppRelativeCurrentExecutionFilePath;
			string fname = relativeURL.Substring(relativeURL.LastIndexOf('~') + 2);
			string cname1 = (fname.Contains(".")) ? fname.Substring(0, fname.LastIndexOf('.')) : fname;
			string cname0 = cname1.ToLower();
			string mainNamespace;
			string assemblyName, cname;
			string actualPath;
			if (cname0 == "gxoauthlogout")
			{
				return new GXOAuthLogout();
			}
			else if (cname0 == "gxoauthuserinfo")
			{
				return new GXOAuthUserInfo();
			}
			else if (cname0 == "gxoauthaccesstoken")
			{
				return new GXOAuthAccessToken();
			}
			else if (cname0 == "gxmulticall")
			{
				return new GXMultiCall();
			}
			else if (HttpHelper.GamServicesInternalName.Contains(cname0))
			{
				mainNamespace = HttpHelper.GAM_NSPACE;
			}
			else
			{
				if (!Config.GetValueOf("AppMainNamespace", out mainNamespace))
					mainNamespace = "GeneXus.Programs.";

				if (GXAPIModule.serviceInPath(pathTranslated, actualPath: out actualPath))
				{
					string nspace;
					bool methodMismatch = false;
					Config.GetValueOf("AppMainNamespace", out nspace);
					string objClass = GXAPIModule.servicesBase[actualPath];
					//
					string objectName = GetObjFromPath(cname0, actualPath);
					string objectNameUp = GetObjFromPath(cname1, actualPath);
					//
					Dictionary<string, object> routeParms;
					if (GXAPIModule.servicesMapData.ContainsKey(actualPath))
					{
						bool IsServiceCall = GetSMap(actualPath, objectName, objectNameUp, requestType, out string mapName, out string mapRegExp, out routeParms, out methodMismatch);
						if (IsServiceCall)
						{
							if (!string.IsNullOrEmpty(mapName) && GXAPIModule.servicesMap[actualPath].TryGetValue(mapName, out SingleMap value))
							{
								string tmpController = objClass;
								string asssemblycontroller = tmpController;
								if (objClass.Contains("\\"))
								{
									tmpController = objClass.Substring(objClass.LastIndexOf("\\") + 1);
									string addNspace = objClass.Substring(0, objClass.LastIndexOf("\\")).Replace("\\", ".");
									asssemblycontroller = addNspace + "." + tmpController;
									nspace += "." + addNspace;
								}
								GxContext gxContext = GxContext.CreateDefaultInstance();
								object handler = ClassLoader.FindInstance(asssemblycontroller, nspace, tmpController, new Object[] { gxContext }, null);

								gxContext.HttpContext = context;
								GxRestWrapper restWrapper = new Application.GxRestWrapper(handler as GXBaseObject, context, gxContext, value.ServiceMethod, value.VariableAlias, routeParms);
								return restWrapper;
							}
						}
						else
						{
							if (requestType.Equals(HttpMethod.Options.Method) && !string.IsNullOrEmpty(actualPath) && GXAPIModule.servicesMapData.ContainsKey(actualPath))
							{
								return new OptionsApiObjectRequestHandler(actualPath, objectName, mapRegExp);
							}
							else
							{
								if (methodMismatch)
								{
									return new ErrorRequestHandler("Method not allowed", HttpStatusCode.MethodNotAllowed);
								}

							}
						}
					}
					return null;
				}
			}
			assemblyName = cname0;
			cname = cname0;
			if (cname.EndsWith("_bc_ws"))
			{
				cname = cname.Substring(0, cname.Length - 3);
				assemblyName = cname;
			}
			string className = mainNamespace + "." + cname;

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
				if (! typeof(IHttpHandler).IsAssignableFrom(objType))
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
					handlerToReturn = null;
					GXLogging.Error(log, objType.FullName + " is not an Http Service");
				}
				else
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
			}
			else
            {
                handlerToReturn = System.Web.UI.PageParser.GetCompiledPageInstance(url, pathTranslated, context);
            }
			return handlerToReturn;
		}

		public string GetObjFromPath(string cname, string apath)
		{
			if (cname.LastIndexOf("/") == (cname.Length - 1))
				cname = cname.Substring(0, cname.Length - 1);
			string objectName = cname.Remove(0, apath.Length);
			return objectName;
		}

		public bool  GetSMap(string actualPath, string objectName, string objectNameUp, string requestType, out string mapName, out string mapRegexp, out Dictionary<string, object> routeParms, out bool methodMismatch)
		{
			routeParms = null;
			methodMismatch = false;
			if (GXAPIModule.servicesMapData[actualPath].TryGetValue(Tuple.Create(objectName, requestType), out mapName))
			{
				// Url exact match
				mapRegexp = mapName;
				methodMismatch = false;
				return true;
			}
			else
			{
				bool pathFound = false;
				mapRegexp = mapName;
				foreach (SingleMap m in GXAPIModule.servicesMap[actualPath].Values)
				{					
					if (!m.Path.Equals(m.PathRegexp) && GxRegex.IsMatch(objectName, m.PathRegexp))
					{
						pathFound = true;
						methodMismatch = false;
						// regexp URL match
						mapName = m.Name;
						mapRegexp = m.PathRegexp;
						if (m.Verb.Equals(requestType))
						{							
							routeParms = new Dictionary<string, object>();
							int i = 0;
							foreach (string smatch in ((GxRegexMatch)GxRegex.Matches(objectNameUp, m.PathRegexp, RegexOptions.Multiline | RegexOptions.IgnoreCase)[0]).Groups)
							{
								string var = ((GxRegexMatch)GxRegex.Matches(m.Path, m.PathRegexp)[0]).Groups[i];
								var = var.Substring(1, var.Length - 2);
								routeParms.Add(var, smatch);
								i++;
							}
							methodMismatch = false;
							return true;
						}						
					}
				}
				if (pathFound && !requestType.Equals( HttpMethod.Options.Method))
				{					
					mapName = null;
					mapRegexp = null;
					routeParms = null;
					methodMismatch = true;
					return false;
				}
			}
			return false;
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
				
                objType = ClassLoader.FindType(assemblyName, className, null);
				if (objType == null)					
					objType = Assembly.Load(assemblyName).GetType(className);
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
