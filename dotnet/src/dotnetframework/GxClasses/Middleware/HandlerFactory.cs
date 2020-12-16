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
			string cname0 = (fname.Contains("."))? fname.Substring(0, fname.LastIndexOf('.')).ToLower():fname.ToLower();
			string actualPath = "";
			if (cname0 == "gxresourceprovider")
			{
				return new GeneXus.Http.GXResourceProvider();
			}
			else if (cname0 == "gxreor")
			{
				return new GeneXus.Http.GXReorServices();
			}
			else if (cname0 == "gxobject")
			{
				return new GeneXus.Http.GXObjectUploadServices();
			}
			else if (cname0 == "gxoauthlogout")
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
			else if (cname0 == "gx_valid_service")
			{
				return new GeneXus.Http.GXValidService();
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
				if (cname0.LastIndexOf("/") == (cname0.Length - 1))
					cname0 = cname0.Substring(0, cname0.Length - 1);
				String objectName = cname0.Substring(cname0.LastIndexOf("/") + 1);				
				
				if (GXAPIModule.servicesMapData.ContainsKey(actualPath) &&
					GXAPIModule.servicesMapData[actualPath].TryGetValue(Tuple.Create(objectName, requestType), out String mapName))
				{
					if (!String.IsNullOrEmpty(mapName) && GXAPIModule.servicesMap[actualPath].TryGetValue(mapName, out String value))
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
						var handler = ClassLoader.FindInstance(asssemblycontroller, nspace, tmpController, null, null);
						var gxContext = GxContext.CreateDefaultInstance();
						gxContext.HttpContext = context;
						GxRestWrapper restWrapper = new Application.GxRestWrapper(handler as GXProcedure, context, gxContext, value);
						return restWrapper;
					}
				}
				else
				{					
					if ( requestType.Equals("OPTIONS") && !String.IsNullOrEmpty(actualPath) && GXAPIModule.servicesMapData.ContainsKey(actualPath))
					{
						// OPTIONS VERB
						string mthheaders = "OPTIONS,HEAD";
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
							context.Response.Headers.Add("Allow", mthheaders);
							context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
							context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
							context.Response.Headers.Add("Access-Control-Allow-Methods", mthheaders);
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
            else
            {
                
                handlerToReturn = (IHttpHandler)System.Web.UI.PageParser.GetCompiledPageInstance(url, pathTranslated, context);
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
