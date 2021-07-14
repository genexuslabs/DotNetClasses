using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeneXus;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Http;
using GeneXus.Metadata;
using GeneXus.Procedure;
using GeneXus.Utils;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GxClasses.Web.Middleware
{
	public class GXRouting : IGXRouting
	{

		static readonly ILog log = log4net.LogManager.GetLogger(typeof(IGXRouting));

		public static string VirtualPath = string.Empty;
		public static string LocalPath = Directory.GetCurrentDirectory();
		public static string ContentRootPath;
		static char[] urlSeparator = { '/', '\\' };
		const char QUESTIONMARK = '?';
		public static string UrlTemplateControllerWithParms;

		//Azure Functions
		public bool AzureRuntime;
		public static string AzureFunctionName;

		const string SDSVC_METHO_SUFFIX = "DL";
		static Regex SDSVC_PATTERN = new Regex("([^/]+/)*(sdsvc_[^/]+/[^/]+)(\\?.*)*");

		const string PRIVATE_DIR = "private";
		public Dictionary<string, string> servicesPathUrl = new Dictionary<string, string>();
		public Dictionary<String, Dictionary<string, string>> servicesMap = new Dictionary<String, Dictionary<string, string>>();
		public Dictionary<String, Dictionary<Tuple<string, string>, String>> servicesMapData = new Dictionary<String, Dictionary<Tuple<string, string>, string>>();
		public Dictionary<string, List<string>> servicesValidPath = new Dictionary<string, List<string>>();
		public string restBaseURL;

		public GXRouting(string baseURL)
		{
			restBaseURL = baseURL;
			ServicesGroupSetting();
			ServicesFunctionsMetadata();
		}

		static public List<ControllerInfo> GetRouteController(Dictionary<string, string> apiPaths,
											Dictionary<string, List<string>> sValid,
											Dictionary<string, Dictionary<string, string>> sMap,
											Dictionary<string, Dictionary<Tuple<string, string>, string>> sMapData,
											string basePath, string verb, string path)
		{
			List<ControllerInfo> result = new List<ControllerInfo>();
			string parms = string.Empty;
			string method = string.Empty;
			GXLogging.Debug(log, $"GetRouteController basePath:{basePath} verb:{verb} path:{path}");
			try
			{
				if (!string.IsNullOrEmpty(path))
				{
					int questionMarkIdx = path.IndexOf(QUESTIONMARK);
					string controller;
					if (apiPaths.ContainsKey(basePath)
						&& sValid.ContainsKey(basePath)
						&& sMap.ContainsKey(basePath)
						&& sMapData.ContainsKey(basePath)
						)
					{
						if (sValid[basePath].Contains(path.ToLower()))
						{
							if (sMapData[basePath].TryGetValue(Tuple.Create(path.ToLower(), verb), out string value))
							{
								//string httpverb = "";
								//if (sVerb.ContainsKey(basePath))
								//	sVerb[basePath].TryGetValue(path.ToLower(), out httpverb);
								//else
								//	httpverb = "";
								string mth = sMap[basePath][value];
								if (questionMarkIdx > 0 && path.Length > questionMarkIdx + 1)
									parms = path.Substring(questionMarkIdx + 1);
								result.Add(new ControllerInfo() { Name = apiPaths[basePath], Parameters = parms, MethodName = mth, Verb = verb });
								GXLogging.Debug(log, $"Controller found Name:{apiPaths[basePath]} Parameters:{parms} MethodName:{mth} Verb={verb}");
							}
							else
							{
								// Method not allowed
								result.Add(new ControllerInfo() { Name = apiPaths[basePath], Parameters = "", MethodName = "", Verb = "" });
								GXLogging.Debug(log, $"Controller found (Method not allowed) Name:{apiPaths[basePath]}");
								return result;
							}
						}
						else
							return result; // Not found
					}
					else
					{
						if (SDSVC_PATTERN.IsMatch(path))
						{
							string controllerWithMth = path;
							if (questionMarkIdx >= 0)
							{
								// rest/module1/module2/sdsvc_service/method?parameters
								controllerWithMth = path.Substring(0, questionMarkIdx).TrimEnd(urlSeparator);
								if (path.Length > questionMarkIdx + 1)
									parms = path.Substring(questionMarkIdx + 1);
							}

							int idx = controllerWithMth.LastIndexOfAny(urlSeparator);
							if (idx > 0 && idx < controllerWithMth.Length - 1)
							{
								controller = controllerWithMth.Substring(0, idx);
								method = $"{controllerWithMth.Substring(idx + 1)}{SDSVC_METHO_SUFFIX}";
								result.Add(new ControllerInfo() { Name = controller, Parameters = parms, MethodName = method });
							}
						}
						else if (questionMarkIdx >= 0)
						{
							// rest/module1/module2/service?paramaters
							controller = path.Substring(0, questionMarkIdx).TrimEnd(urlSeparator);
							if (path.Length > questionMarkIdx + 1)
								parms = path.Substring(questionMarkIdx + 1);
							GXLogging.Debug(log, $"Controller found (with question mark) Name:{controller} Parameters:{parms}");
							result.Add(new ControllerInfo() { Name = controller, Parameters = parms });
						}
						else if (path.EndsWith(HttpHelper.GXOBJECT, StringComparison.OrdinalIgnoreCase))
						{
							controller = path.Substring(0, path.Length - HttpHelper.GXOBJECT.Length);
							GXLogging.Debug(log, $"Controller found Name:{controller}/{HttpHelper.GXOBJECT}");
							result.Add(new ControllerInfo() { Name = controller, Parameters = parms });
						}
						else
						{
							// rest/module1/module2/service
							controller = path.TrimEnd(urlSeparator);
							GXLogging.Debug(log, $"Controller found (without parameters) Name:{controller} Parameters:{parms}");
							result.Add(new ControllerInfo() { Name = controller, Parameters = parms });

							// rest/module1/module2/service/parameters
							int idx = path.LastIndexOfAny(urlSeparator);
							if (idx > 0 && idx < path.Length - 1)
							{
								controller = path.Substring(0, idx);
								parms = path.Substring(idx + 1);
								GXLogging.Debug(log, $"Controller found (without url parameters) Name:{controller} Parameters:{parms}");
								result.Add(new ControllerInfo() { Name = controller, Parameters = parms });
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, ex, "Controller match URL failed ", path);
			}
			return result;
		}
		public Task ProcessRestRequest(HttpContext context)
		{
			try
			{
				string path = context.Request.Path.ToString();
				string actualPath = "";
				if (path.Contains($"/{restBaseURL}") | ServiceInPath(path, out actualPath))
				{
					string controllerWithParms = "";
					if (!AzureRuntime)
						controllerWithParms = context.GetRouteValue(UrlTemplateControllerWithParms) as string;
					else
					{
						controllerWithParms = GetGxRouteValue(path);
						GXLogging.Debug(log, $"Running Azure functions. ControllerWithParms :{controllerWithParms} path:{path}");
					}
					
					List<ControllerInfo> controllers = GetRouteController(servicesPathUrl, servicesValidPath, servicesMap, servicesMapData, actualPath, context.Request.Method, controllerWithParms);
					GxRestWrapper controller = null;
					ControllerInfo controllerInfo = controllers.FirstOrDefault(c => (controller = GetController(context, c.Name, c.MethodName)) != null);

					if (controller != null)
					{
						if (HttpMethods.IsGet(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsGet(controllerInfo.Verb)))
						{
							return controller.Get(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsPost(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsPost(controllerInfo.Verb)))
						{
							return controller.Post();
						}
						else if (HttpMethods.IsDelete(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsDelete(controllerInfo.Verb)))
						{
							return controller.Delete(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsPut(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsPut(controllerInfo.Verb)))
						{
							return controller.Put(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsPatch(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsPatch(controllerInfo.Verb)))
						{
							return controller.Patch(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsOptions(context.Request.Method))
						{
							string mthheaders = "OPTIONS,HEAD";
							if (!String.IsNullOrEmpty(actualPath) && servicesMapData.ContainsKey(actualPath))
							{
								foreach (Tuple<string, string> t in servicesMapData[actualPath].Keys)
								{
									if (t.Item1.Equals(controllerWithParms.ToLower()))
									{
										mthheaders += "," + t.Item2;
									}
								}
							}
							else
							{
								mthheaders += ", GET, POST";
							}
							context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)context.Request.Headers["Origin"] });
							context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept" });
							context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { mthheaders });
							context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
							context.Response.Headers.Add("Allow", mthheaders);
							context.Response.StatusCode = (int)HttpStatusCode.OK;
						}
						else
						{
							context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
							context.Response.Headers.Clear();
						}
					}
					else
					{
						GXLogging.Error(log, $"ProcessRestRequest controller not found path:{path} controllerWithParms:{controllerWithParms}");
						context.Response.StatusCode = (int)HttpStatusCode.NotFound;
						context.Response.Headers.Clear();
					}
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "ProcessRestRequest", ex);
				GxRestWrapper.SetError(context, Convert.ToInt32(HttpStatusCode.InternalServerError).ToString(), ex.Message);
				return Task.CompletedTask;
			}
		}

		internal string GetGxRouteValue(string path)
		{
			string controllerWithParms = "";
			//Not API Objects
			string basePath = restBaseURL;

			//API Objects
			foreach (var map in servicesMap)
			{
				foreach (var mlist in map.Value)
				{
					if (mlist.Key == AzureFunctionName)
					{
						basePath = $"{restBaseURL}/{map.Key}";
					}
				}			
			}

			controllerWithParms = path.Remove(0, basePath.Length + 1);
			controllerWithParms = controllerWithParms.StartsWith("/") ? controllerWithParms.Remove(0, 1) : controllerWithParms;
			return controllerWithParms;

		}
		public bool ServiceInPath(String path, out String actualPath)
		{
			actualPath = "";
			foreach (String subPath in servicesPathUrl.Keys)
			{
				if (path.ToLower().Contains($"/{subPath.ToLower()}"))
				{
					actualPath = subPath.ToLower();
					GXLogging.Debug(log, $"ServiceInPath actualPath:{actualPath}");
					return true;
				}
			}
			GXLogging.Debug(log, $"ServiceInPath path:{path} not found");
			return false;
		}
		public GxRestWrapper GetController(HttpContext context, string controller, string methodName)
		{
			GXLogging.Debug(log, $"GetController:{controller} method:{methodName}");
			GxContext gxContext = GxContext.CreateDefaultInstance();
			//	IHttpContextAccessor contextAccessor = context.RequestServices.GetService<IHttpContextAccessor>();
			//	gxContext.HttpContext = new GxHttpContextAccesor(contextAccessor);
			gxContext.HttpContext = context;
			context.NewSessionCheck();
			string nspace;
			Config.GetValueOf("AppMainNamespace", out nspace);

			String tmpController = controller;
			String addNspace = "";
			String asssemblycontroller = tmpController;


			if (controller.Contains("\\"))
			{
				tmpController = controller.Substring(controller.LastIndexOf("\\") + 1);
				addNspace = controller.Substring(0, controller.LastIndexOf("\\")).Replace("\\", ".");
				asssemblycontroller = addNspace + "." + tmpController;
				nspace += "." + addNspace;
			}

			string privateDir = Path.Combine(ContentRootPath, PRIVATE_DIR);
			bool privateDirExists = Directory.Exists(privateDir);

			GXLogging.Debug(log, $"PrivateDir:{privateDir} asssemblycontroller:{asssemblycontroller}");

			if (privateDirExists && File.Exists(Path.Combine(privateDir, $"{asssemblycontroller.ToLower()}.grp.json")))
			{
				controller = tmpController;
				GXLogging.Debug(log, $"FindController:{controller} namespace:{nspace} assembly:{asssemblycontroller}");
				var controllerInstance = ClassLoader.FindInstance(asssemblycontroller, nspace, controller, new Object[] { gxContext }, Assembly.GetEntryAssembly());
				GXProcedure proc = controllerInstance as GXProcedure;
				if (proc != null)
					return new GxRestWrapper(proc, context, gxContext, methodName);
				else
					GXLogging.Warn(log, $"Controller not found controllerAssemblyName:{asssemblycontroller} nspace:{nspace} controller:{controller}");
			}
			else
			{
				string controllerLower = controller.ToLower();
				string svcFile = Path.Combine(ContentRootPath, $"{controllerLower}.svc");
				if (File.Exists(svcFile))
				{
					var controllerAssemblyQualifiedName = new string(File.ReadLines(svcFile).First().SkipWhile(c => c != '"')
					   .Skip(1)
					   .TakeWhile(c => c != '"')
					   .ToArray()).Trim().Split(',');
					var controllerAssemblyName = controllerAssemblyQualifiedName.Last();
					var controllerClassName = controllerAssemblyQualifiedName.First();
					if (!string.IsNullOrEmpty(nspace) && controllerClassName.StartsWith(nspace))
						controllerClassName = controllerClassName.Substring(nspace.Length + 1);
					else
						nspace = String.Empty;
					var controllerInstance = ClassLoader.FindInstance(controllerAssemblyName, nspace, controllerClassName, new Object[] { gxContext }, Assembly.GetEntryAssembly());
					GXProcedure proc = controllerInstance as GXProcedure;
					if (proc != null)
						return new GxRestWrapper(proc, context, gxContext, methodName);
					else
						GXLogging.Warn(log, $"Controller not found controllerAssemblyName:{controllerAssemblyName} nspace:{nspace} controller:{controllerClassName}");
				}
				else if (File.Exists(Path.Combine(ContentRootPath, controllerLower + "_bc.svc")))
				{
					var sdtInstance = ClassLoader.FindInstance(Config.CommonAssemblyName, nspace, GxSilentTrnSdt.GxSdtNameToCsharpName(controllerLower), new Object[] { gxContext }, Assembly.GetEntryAssembly(), true) as GxSilentTrnSdt;
					if (sdtInstance != null)
						return new GXBCRestService(sdtInstance, context, gxContext);
					else
						GXLogging.Warn(log, $"Controller not found controllerAssemblyName:{Config.CommonAssemblyName} nspace:{nspace} controller:{GxSilentTrnSdt.GxSdtNameToCsharpName(controller)}");
				}
			}
			GXLogging.Warn(log, $"Controller was not found");
			return null;
		}
		public void ServicesGroupSetting()
		{
			if (Directory.Exists(Path.Combine(ContentRootPath, PRIVATE_DIR)))
			{
				string[] grpFiles = Directory.GetFiles(Path.Combine(ContentRootPath, PRIVATE_DIR), "*.grp.json");
				foreach (String grp in grpFiles)
				{
					object p = JSONHelper.Deserialize<MapGroup>(File.ReadAllText(grp));
					MapGroup m = p as MapGroup;
					if (m != null)
					{

						if (String.IsNullOrEmpty(m.BasePath))
						{
							m.BasePath = restBaseURL;
						}
						String mapPath = (m.BasePath.EndsWith("/")) ? m.BasePath : m.BasePath + "/";
						String mapPathLower = mapPath.ToLower();
						String mNameLower = m.Name.ToLower();
						servicesPathUrl.Add(mapPathLower, mNameLower);
						GXLogging.Debug(log, $"addServicesPathUrl key:{mapPathLower} value:{mNameLower}");
						foreach (SingleMap sm in m.Mappings)
						{
							if (sm.Verb == null)
								sm.Verb = "GET";
							if (String.IsNullOrEmpty(sm.Path))
								sm.Path = sm.Name;
							if (servicesMap.ContainsKey(mapPathLower))
							{
								if (!servicesMap[mapPathLower].ContainsKey(sm.Name.ToLower()))
								{
									servicesValidPath[mapPathLower].Add(sm.Path.ToLower());
									servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm.ServiceMethod);
									servicesMapData[mapPathLower].Add(Tuple.Create(sm.Path.ToLower(), sm.Verb.ToUpper()), sm.Name.ToLower());
								}
							}
							else
							{
								servicesValidPath.Add(mapPathLower, new List<string>());
								servicesValidPath[mapPathLower].Add(sm.Path.ToLower());
								servicesMap.Add(mapPathLower, new Dictionary<string, string>());
								servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm.ServiceMethod);
								servicesMapData.Add(mapPathLower, new Dictionary<Tuple<string, string>, string>());
								servicesMapData[mapPathLower].Add(Tuple.Create(sm.Path.ToLower(), sm.Verb.ToUpper()), sm.Name.ToLower());
							}
						}
					}
				}
			}
		}

		public void ServicesFunctionsMetadata()
		{
			//Used for Azure functions
			
			string functionMetadataFile = "functions.metadata";
			string metadataFilePath = Path.Combine(ContentRootPath, functionMetadataFile);
			
			if (File.Exists(metadataFilePath))
			{
				AzureRuntime = true;
			}
		}
	}


	public class FunctionMetadata
	{
		public string name { get; set; }

		public string scriptFile { get; set; }

		public string entryPoint { get; set; }

		public string language { get; set; }

		[JsonPropertyName("properties")]
		public PropertyList properties { get; set; }

		[JsonPropertyName("bindings")]
		public List<Binding> bindings { get; set; }

	}
		public class PropertyList
		{
			public bool IsCodeless { get; set; }
			public string configurationSource { get; set; }
		}

	public class Binding
	{
		public string name { get; set; }
		public string direction { get; set; }
		public string authLevel { get; set; }
		public string route { get; set; }
		public string type { get; set; }
		public List<string> methods { get; set; }

	}

	[DataContract()]
	internal class MapGroup
	{	
		String _objectType;
		String _name;
		String _basePath;
		SingleMap[] _mappings;

		[DataMember()]
		public string ObjectType { get => _objectType; set => _objectType = value; }

		[DataMember()]
		public string Name { get => _name; set => _name = value; }

		[DataMember()]
		public string BasePath { get => _basePath; set => _basePath = value; }

		[DataMember()]
		public SingleMap[] Mappings { get => _mappings; set => _mappings = value; }
	}

	[DataContract()]
	internal class SingleMap
	{
		String verb = "GET";
		String name = "";
		String path = "";
		String implementation = "";
		String methodName = "";

		[DataMember()]
		public string Name { get => name; set => name = value; }

		[DataMember()]
		public string Path { get => path; set => path = value; }

		[DataMember()]
		public string ServiceMethod { get => methodName; set => methodName = value; }

		[DataMember()]
		public string Implementation { get => implementation; set => implementation = value; }

		[DataMember()]
		public string Verb { get => verb; set => verb = value; }

	}
}
