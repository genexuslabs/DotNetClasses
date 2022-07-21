using System;
using System.Web;
using System.Reflection;
using GeneXus.Configuration;
using System.Collections.Generic;
using log4net;
using System.IO;
using GeneXus.Application;
using GeneXus.Utils;
using GeneXus.Http.HttpModules;
using GeneXus.Metadata;
using GeneXus.Procedure;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Net.Http.Headers;
using System.Net.Http;

namespace GeneXus.HttpHandlerFactory
{
	class HandlerFactory : IHttpHandlerFactory
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static List<string> GxNamespaces;

		public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
		{
			IHttpHandler handlerToReturn;			

			string relativeURL = context.Request.AppRelativeCurrentExecutionFilePath;
			string fname = relativeURL.Substring(relativeURL.LastIndexOf('~') + 2);
			String cname1 = (fname.Contains(".")) ? fname.Substring(0, fname.LastIndexOf('.')) : fname;
			string cname0 = cname1.ToLower();
			string actualPath = "";
			
			if (cname0 == "gxoauthlogout")
			{
				return new GeneXus.Http.GXOAuthLogout();
			}
			else if (cname0 == "gxoauthuserinfo")
			{
				return new GeneXus.Http.GXOAuthUserInfo();
			}
			else if (cname0 == "gxoauthaccesstoken")
			{
				return new GeneXus.Http.GXOAuthAccessToken();
			}
			else if (cname0 == "gxmulticall")
			{
				return new GeneXus.Http.GXMultiCall();
			}
            string assemblyName, cname;
			if (GXAPIModule.serviceInPath(pathTranslated, actualPath: out actualPath))
			{
				string nspace;
				Config.GetValueOf("AppMainNamespace", out nspace);
				String objClass = GXAPIModule.servicesBase[actualPath];
				//
				String objectName = GetObjFromPath(cname0, actualPath);
				String objectNameUp = GetObjFromPath(cname1, actualPath);
				//
				Dictionary<string, object> routeParms;
				if (GXAPIModule.servicesMapData.ContainsKey(actualPath) && GetSMap(actualPath, objectName, objectNameUp, requestType, out string mapName, out routeParms))					
				{
					if (!String.IsNullOrEmpty(mapName) && GXAPIModule.servicesMap[actualPath].TryGetValue(mapName, out SingleMap value))
					{
						String tmpController = objClass;
						String asssemblycontroller = tmpController;
						if (objClass.Contains("\\"))
						{
							tmpController = objClass.Substring(objClass.LastIndexOf("\\") + 1);
							String addNspace = objClass.Substring(0, objClass.LastIndexOf("\\")).Replace("\\", ".");
							asssemblycontroller = addNspace + "." + tmpController;
							nspace += "." + addNspace;
						}
						var gxContext = GxContext.CreateDefaultInstance();
						var handler = ClassLoader.FindInstance(asssemblycontroller, nspace, tmpController, new Object[] { gxContext }, null);

						gxContext.HttpContext = context;						
						GxRestWrapper restWrapper = new Application.GxRestWrapper(handler as GXBaseObject, context, gxContext, value.ServiceMethod, value.VariableAlias, routeParms);
						return restWrapper;
					}
				}
				else
				{					
					if ( requestType.Equals(HttpMethod.Options.Method) && !String.IsNullOrEmpty(actualPath) && GXAPIModule.servicesMapData.ContainsKey(actualPath))
					{
						// OPTIONS VERB
						string mthheaders = $"{HttpMethod.Options.Method},{HttpMethod.Head.Method}";
						bool found = false;
						foreach (Tuple<string, string> t in GXAPIModule.servicesMapData[actualPath].Keys)
						{
							if (t.Item1.Equals(objectName.ToLower()))
							{
								mthheaders += "," + t.Item2;
								found = true;
							}
						}
						if (found)
						{
							context.Response.Headers.Add(HeaderNames.Allow, mthheaders);
							context.Response.Headers.Add(HeaderNames.AccessControlAllowHeaders, HeaderNames.ContentType);
							context.Response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "*");
							context.Response.Headers.Add(HeaderNames.AccessControlAllowMethods, mthheaders);
							context.Response.End();
						}
						else
						{
							context.Response.StatusCode = (int)HttpStatusCode.NotFound;
							context.Response.End();
						}
						return null;
					}
				}
				return null;
			}
			else
			{
				{
					assemblyName = cname0;
					cname = cname0;
				}
				if (cname.EndsWith("_bc_ws"))
				{
					cname = cname.Substring(0, cname.Length - 3);
					assemblyName = cname;
				}
			}
			string mainNamespace, className;
			if (Config.GetValueOf("AppMainNamespace", out mainNamespace))
				className = mainNamespace + "." + cname;
			else
				className = "GeneXus.Programs." + cname;

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
                handlerToReturn = (IHttpHandler)System.Web.UI.PageParser.GetCompiledPageInstance(url, pathTranslated, context);
            }
			return handlerToReturn;
		}

		public string GetObjFromPath(string cname, string apath)
		{
			if (cname.LastIndexOf("/") == (cname.Length - 1))
				cname = cname.Substring(0, cname.Length - 1);
			String objectName = cname.Remove(0, apath.Length);
			return objectName;
		}

		public bool  GetSMap(string actualPath, string objectName, string objectNameUp, string requestType, out string mapName, out Dictionary<string, object> routeParms)
		{
			routeParms = null;
			if (GXAPIModule.servicesMapData[actualPath].TryGetValue(Tuple.Create(objectName, requestType), out mapName))
			{				
				return true;
			}
			else
			{
				foreach (SingleMap m in GXAPIModule.servicesMap[actualPath].Values)
				{
					if (!m.Path.Equals(m.PathRegexp) && GxRegex.IsMatch(objectName, m.PathRegexp))
					{
						mapName = m.Name;						
						routeParms = new Dictionary<string, object>();
						int i=0;
						foreach (string smatch in ((GxRegexMatch)GxRegex.Matches(objectNameUp, m.PathRegexp, RegexOptions.Multiline | RegexOptions.IgnoreCase)[0]).Groups)
						{
							string var  = ((GxRegexMatch)GxRegex.Matches(m.Path, m.PathRegexp)[0]).Groups[i];
							var = var.Substring(1, var.Length -2);
							routeParms.Add(var, smatch);
							i++;
						}
						return true;
					}
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
				
                objType = GeneXus.Metadata.ClassLoader.FindType(assemblyName, className, null);
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
