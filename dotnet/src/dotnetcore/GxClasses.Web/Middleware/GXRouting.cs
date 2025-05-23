using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeneXus;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Http;
using GeneXus.HttpHandlerFactory;
using GeneXus.Metadata;
using GeneXus.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GxClasses.Web.Middleware
{
	internal class GXRouting : IGXRouting
	{

		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<IGXRouting>();

		public static string VirtualPath = string.Empty;
		public static string LocalPath = Directory.GetCurrentDirectory();
		public static string ContentRootPath = ".";
		static char[] urlSeparator = { '/', '\\' };
		const char QUESTIONMARK = '?';
		const string oauthRoute = "/oauth";
		const string SvcExtension = ".svc";
		const string SvcExtensionPattern = "*.svc";
		public static string UrlTemplateControllerWithParms;

		//Azure Functions
		public static bool AzureRuntime;
		public AzureDeployFeature AzureDeploy = new AzureDeployFeature();
		public static string AzureFunctionName;

		static Regex SDSVC_PATTERN = new Regex("([^/]+/)*(sdsvc_[^/]+/[^/]+)(\\?.*)*");

		internal const string PRIVATE_DIR = "private";
		public Dictionary<string, string> servicesPathUrl = new Dictionary<string, string>();
		public Dictionary<String, Dictionary<string, SingleMap>> servicesMap = new Dictionary<String, Dictionary<string, SingleMap>>();
		public Dictionary<String, Dictionary<Tuple<string, string>, String>> servicesMapData = new Dictionary<String, Dictionary<Tuple<string, string>, string>>();
		public Dictionary<string, List<string>> servicesValidPath = new Dictionary<string, List<string>>();
		public HashSet<string> svcFiles;
		public string restBaseURL;

		public GXRouting(string baseURL)
		{
			restBaseURL = baseURL;
			ServicesGroupSetting();
			ServicesFunctionsMetadata();
			GetAzureDeploy();
			SvcFiles();
		}

		static public List<ControllerInfo> GetRouteController(Dictionary<string, string> apiPaths,
											Dictionary<string, List<string>> sValid,
											Dictionary<string, Dictionary<string, SingleMap>> sMap,
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
								string mth = sMap[basePath][value].ServiceMethod;
								Dictionary<string, string> vMap = sMap[basePath][value].VariableAlias;
								if (questionMarkIdx > 0 && path.Length > questionMarkIdx + 1)
									parms = path.Substring(questionMarkIdx + 1);
								result.Add(new ControllerInfo() { Name = apiPaths[basePath], Parameters = parms, MethodName = mth, Verb = verb, VariableAlias = vMap });
								GXLogging.Debug(log, $"Controller found Name:{apiPaths[basePath]} Parameters:{parms} MethodName:{mth} Verb={verb}");
							}
							else
							{
								// Method not allowed
								result.Add(new ControllerInfo() { Name = apiPaths[basePath], Parameters = string.Empty, MethodName = string.Empty, Verb = string.Empty });
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
								method = $"{controllerWithMth.Substring(idx + 1)}";
								result.Add(new ControllerInfo() { Name = controller, Parameters = parms, MethodPattern = method });
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
		internal async Task RouteHttpService(HttpContext context)
		{
			try
			{
				HandlerFactory handlerFactory = new HandlerFactory();
				await handlerFactory.Invoke(context);
			}
			catch (Exception ex)
			{
				await Task.FromException(ex);
			}
		}
		public Task ProcessRestRequest(HttpContext context)
		{
			try
			{
				if (!AzureRuntime && context.GetType() == typeof(DefaultHttpContext))
				{
					IHttpContextAccessor contextAccessor = context.RequestServices.GetService<IHttpContextAccessor>();
					context = new GxHttpContextAccesor(contextAccessor);
				}
				Task result = Task.CompletedTask;
				string path = context.Request.Path.ToString();
				string actualPath = string.Empty;
				bool isServiceInPath = ServiceInPath(path, out actualPath);
				if (path.Contains($"/{restBaseURL}") || isServiceInPath || (AzureRuntime && path.Contains(oauthRoute)))
				{
					string controllerWithParms = string.Empty;
					if (!AzureRuntime)
					{
						controllerWithParms = context.GetRouteValue(UrlTemplateControllerWithParms) as string;
						if (String.IsNullOrEmpty(controllerWithParms) && !String.IsNullOrEmpty(actualPath))
						{
							if (context.Request.RouteValues.Count > 0)
							{
								foreach (IRouter r in context.GetRouteData().Routers)
								{
									Route current_r = r as Route;
									if (current_r != null)
									{
										path = current_r.ParsedTemplate.TemplateText;
										break;
									}
								}
							}
							string controllerPath = path.ToLower().Split(actualPath).Last<string>();
							controllerWithParms = controllerPath.Split(QUESTIONMARK).First<string>();
					
						}
					}
					else
					{
						if (path.Contains(oauthRoute) && (AzureDeploy.GAM == "true")) 
							return (RouteHttpService(context));
						controllerWithParms = GetGxRouteValue(path);
						GXLogging.Debug(log, $"Running Azure functions. ControllerWithParms :{controllerWithParms} path:{path}");
					}
				
					List<ControllerInfo> controllers = GetRouteController(servicesPathUrl, servicesValidPath, servicesMap, servicesMapData, actualPath, context.Request.Method, controllerWithParms);
					GxRestWrapper controller = null;
					ControllerInfo controllerInfo = controllers.FirstOrDefault(c => (controller = GetController(context, c)) != null);
					

					if (controller != null)
					{
						if (HttpMethods.IsGet(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsGet(controllerInfo.Verb)))
						{
							result = controller.Get(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsPost(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsPost(controllerInfo.Verb)))
						{
							result = controller.Post();
						}
						else if (HttpMethods.IsDelete(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsDelete(controllerInfo.Verb)))
						{
							result = controller.Delete(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsPut(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsPut(controllerInfo.Verb)))
						{
							result = controller.Put(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsPatch(context.Request.Method) && (controllerInfo.Verb == null || HttpMethods.IsPatch(controllerInfo.Verb)))
						{
							result = controller.Patch(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsOptions(context.Request.Method))
						{
							List<string> mthheaders = new List<string>() { $"{HttpMethod.Options.Method},{HttpMethod.Head.Method}" };
							if (!String.IsNullOrEmpty(actualPath) && servicesMapData.ContainsKey(actualPath) && !string.IsNullOrEmpty(controllerWithParms))
							{
								foreach (Tuple<string, string> t in servicesMapData[actualPath].Keys)
								{
									if (t.Item1.Equals(controllerWithParms.ToLower()))
									{
										mthheaders.Add(t.Item2);
									}
								}
							}
							else
							{
								mthheaders.Add(HttpMethod.Get.Method);
								mthheaders.Add(HttpMethod.Post.Method);
							}
							HttpHelper.CorsHeaders(context);
							HttpHelper.AllowHeader(context, mthheaders);
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
						context.Response.Headers.Clear();
						result = Task.FromException(new PageNotFoundException(path));
					}
				}
				context.CommitSession();

				return result;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "ProcessRestRequest", ex);
				HttpHelper.SetUnexpectedError(context, HttpStatusCode.InternalServerError, ex);
				return Task.FromException(ex);
			}
		}

		internal string GetGxRouteValue(string path)
		{
			//Not API Objects
			string basePath = "";
			if (AzureDeploy.Deploy == AzureFeature.AzureServerless && !string.IsNullOrEmpty(AzureDeploy.Route))
				basePath = AzureDeploy.Route;
			else
				basePath = restBaseURL;

			string pathWithNoBase = string.IsNullOrEmpty(basePath) ? path.Substring(1) : path.Substring(basePath.Length + 2);

			//API Objects
			string AzureFunctionShortName = AzureFunctionName.Substring(AzureFunctionName.LastIndexOf("_") + 1);
			string controllerWithParms = "";
			foreach (var map in servicesMap)
			{
				if (path.ToLower().Contains(map.Key.ToLower()))
				{
					foreach (var mlist in map.Value)
					{
						if (mlist.Key.ToLower() == AzureFunctionShortName.ToLower())
						{

							controllerWithParms = mlist.Value.Path;
							Regex rx = new Regex(mlist.Value.PathRegexp, RegexOptions.IgnoreCase);
							MatchCollection matches = rx.Matches(pathWithNoBase.ToLower());
							if (matches != null)
							{
								string uriPath = "";
								foreach (Match match in matches)
								{
									uriPath = match.Value;
									break;
								}

								if (pathWithNoBase.Remove(pathWithNoBase.Length - uriPath.Length).ToLower() == map.Key.ToLower())
									return controllerWithParms;
							}

						}
					}
				}
			}

			controllerWithParms = path.Remove(0, basePath.Length + 1);
			controllerWithParms = controllerWithParms.StartsWith("/") ? controllerWithParms.Remove(0, 1) : controllerWithParms;
			return controllerWithParms;


		}
		public bool ServiceInPath(String path, out String actualPath)
		{
			actualPath = null;
			string tmppath = path.Substring(0, 1).Equals("/") ? path.Substring(1) : path;
			int pos = tmppath.IndexOf("/");
			if (pos > 0)
			{
				string innerPath = tmppath.Substring(pos, tmppath.Length - pos);
				actualPath = FindPath(innerPath, servicesPathUrl, true);
			}
			if (String.IsNullOrEmpty(actualPath))
			{
				// fallback
				actualPath = FindPath(path, servicesPathUrl, false);
				if (String.IsNullOrEmpty(actualPath))
				{
					actualPath = String.Empty;
					GXLogging.Debug(log, $"ServiceInPath path:{path} not found");
					return false;
				}
				else
				{
					return true;
				}
			}
			else {
				return true;
			}						
		}

		private String FindPath(string innerPath, Dictionary<string,string > servicesPathUrl, bool startTxt)
		{
			string actualPath = String.Empty;
			foreach (string subPath in from String subPath in servicesPathUrl.Keys									
									select subPath)
			{
				bool match = false;
				innerPath = innerPath.ToLower();
				match = (startTxt)? innerPath.StartsWith($"/{subPath.ToLower()}"): innerPath.Contains($"/{subPath.ToLower()}");
				if (match)
				{
					actualPath = subPath.ToLower();
					GXLogging.Debug(log, $"ServiceInPath actualPath:{actualPath}");
					return subPath;
				}
			}
			return null;
		}

		public GxRestWrapper GetController(HttpContext context, string controller, string methodName, Dictionary<string, string> variableAlias)
		{
			return GetController(context, new ControllerInfo() { Name = controller, MethodName = methodName, VariableAlias = variableAlias });
		}
		public GxRestWrapper GetController(HttpContext context, ControllerInfo controllerInfo)
		{
			string controller = controllerInfo.Name;
			string methodName = controllerInfo.MethodName;
			string methodPattern = controllerInfo.MethodPattern;
			Dictionary<string, string> variableAlias = controllerInfo.VariableAlias;
			GXLogging.Debug(log, $"GetController:{controller} method:{methodName} methodPattern:{methodPattern}");
			GxContext gxContext = GxContext.CreateDefaultInstance();
			gxContext.HttpContext = context;
			context.NewSessionCheck();
			string nspace = Preferences.AppMainNamespace;

			String tmpController = controller;
			String addNspace = string.Empty;
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
			string svcFile=null;
			if (privateDirExists && File.Exists(Path.Combine(privateDir, $"{asssemblycontroller.ToLower()}.grp.json")))
			{
				controller = tmpController;
				GXLogging.Debug(log, $"FindController:{controller} namespace:{nspace} assembly:{asssemblycontroller}");
				object controllerInstance = ClassLoader.FindInstance(asssemblycontroller, nspace, controller, new Object[] { gxContext }, Assembly.GetEntryAssembly());
				GXBaseObject proc = controllerInstance as GXBaseObject;
				if (proc != null)
					return new GxRestWrapper(proc, context, gxContext, methodName, variableAlias, null);
				else
					GXLogging.Warn(log, $"Controller not found controllerAssemblyName:{asssemblycontroller} nspace:{nspace} controller:{controller}");
			}
			else
			{
				string controllerLower = controller.ToLower();
				svcFile = SvcFile($"{controller}{SvcExtension}");
				if (File.Exists(svcFile))
				{
					string[] controllerAssemblyQualifiedName = new string(File.ReadLines(svcFile).First().SkipWhile(c => c != '"')
					   .Skip(1)
					   .TakeWhile(c => c != '"')
					   .ToArray()).Trim().Split(',');
					string controllerAssemblyName = controllerAssemblyQualifiedName.Last();
					string controllerClassName = controllerAssemblyQualifiedName.First();
					if (!string.IsNullOrEmpty(nspace) && controllerClassName.StartsWith(nspace))
						controllerClassName = controllerClassName.Substring(nspace.Length + 1);
					else
						nspace = string.Empty;
					object controllerInstance = ClassLoader.FindInstance(controllerAssemblyName, nspace, controllerClassName, new Object[] { gxContext }, Assembly.GetEntryAssembly());
					GXBaseObject proc = controllerInstance as GXBaseObject;
					if (proc != null)
						return new GxRestWrapper(proc, context, gxContext, methodName, methodPattern);
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

		private void SvcFiles()
		{
			svcFiles = new HashSet<string>(new CaseInsensitiveStringEqualityComparer());
			foreach (string file in Directory.GetFiles(ContentRootPath, SvcExtensionPattern, SearchOption.AllDirectories))
			{
				svcFiles.Add(file);
			}
		}
		string SvcFile(string controller)
		{
			string fileName;
			string controllerFullName = new FileInfo(Path.Combine(ContentRootPath, controller)).FullName;
			if (svcFiles.TryGetValue(controllerFullName, out fileName))
				return fileName;
			else
			{
				GXLogging.Warn(log, "Service file not found:" + controllerFullName);
				return null;
			}
			
		}
		public void ServicesGroupSetting()
		{
			try
			{
				if (Directory.Exists(Path.Combine(ContentRootPath, PRIVATE_DIR)))
				{
					string[] grpFiles = Directory.GetFiles(Path.Combine(ContentRootPath, PRIVATE_DIR), "*.grp.json");
					foreach (String grp in grpFiles)
					{
						string content = File.ReadAllText(grp);
						if (!string.IsNullOrEmpty(content))
						{
							object p = JSONHelper.Deserialize<MapGroup>(content);
							MapGroup m = p as MapGroup;
							if (m != null && m.Name != null && m.Mappings != null)
							{

								if (String.IsNullOrEmpty(m.BasePath))
								{
									m.BasePath = restBaseURL;
								}
								string mapPath = (m.BasePath.EndsWith("/")) ? m.BasePath : m.BasePath + "/";
								string mapPathLower = mapPath.ToLower();
								string mNameLower = m.Name.ToLower();
								servicesPathUrl[mapPathLower]= mNameLower;
								if (!RestAPIHelpers.ServiceAsController())
								{
									GXLogging.Debug(log, $"addServicesPathUrl key:{mapPathLower} value:{mNameLower}");
									foreach (SingleMap sm in m.Mappings)
									{
										if (sm.Verb == null)
											sm.Verb = "GET";
										if (String.IsNullOrEmpty(sm.Path))
											sm.Path = sm.Name;
										else
										{
											sm.Path = Regex.Replace(sm.Path, "^/|/$", "");
										}
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
												servicesValidPath[mapPathLower].Add(sm.Path.ToLower());

												servicesMapData[mapPathLower].Add(Tuple.Create(sm.Path.ToLower(), sm.Verb.ToUpper()), sm.Name.ToLower());
												servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm);
											}
										}
										else
										{
											servicesValidPath.Add(mapPathLower, new List<string>());
											servicesValidPath[mapPathLower].Add(sm.Path.ToLower());

											servicesMapData.Add(mapPathLower, new Dictionary<Tuple<string, string>, string>());
											servicesMapData[mapPathLower].Add(Tuple.Create(sm.Path.ToLower(), sm.Verb.ToUpper()), sm.Name.ToLower());
											servicesMap.Add(mapPathLower, new Dictionary<string, SingleMap>());
											servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm);
										}
									}
								}
							}
						}
					}
				}
			}catch (Exception ex)
			{
				GXLogging.Error(log, $"Error Loading Services Group Settings", ex);
				throw;
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
		public void GetAzureDeploy()
		{
			string azureDeployFlagFile = Path.Combine(ContentRootPath, "azureflag.json");

			if (File.Exists(azureDeployFlagFile))
			{
				string azureFlag = File.ReadAllText(azureDeployFlagFile);
				AzureDeploy = JSONHelper.Deserialize<AzureDeployFeature>(azureFlag);
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

	public class AzureDeployFeature
	{
		[DataMember()]
		public string Deploy
		{ get; set; }

		[DataMember()]
		public string Route
		{ get; set; }

		[DataMember()]
		public string GAM
		{ get; set; }


	}
	internal class CaseInsensitiveStringEqualityComparer : IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
		}

		public int GetHashCode(string obj)
		{
			return obj.ToLower().GetHashCode();
		}
	}
	public static class AzureFeature
	{
		public const string AzureServerless = "AzureServerless";
		public const string AzureHttpFunctions = "AzureHttpFunctions";
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
	public class MapGroup
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
	public class SingleMap
	{
		string verb = "GET";
		string name = string.Empty;
		string path = string.Empty;
		string pathregexp = string.Empty;
		string implementation = string.Empty;
		string methodName = string.Empty;
		Dictionary<string, string> variableAlias = new Dictionary<string, string>();

		[DataMember()]
		public string Name { get => name; set => name = value; }

		[DataMember()]
		public string Path { get => path; set => path = value; }

		[DataMember()]
		public string PathRegexp { get => pathregexp; set => pathregexp = value; }

		[DataMember()]
		public string ServiceMethod { get => methodName; set => methodName = value; }

		[DataMember()]
		public string Implementation { get => implementation; set => implementation = value; }

		[DataMember()]
		public string Verb { get => verb; set => verb = value; }

		[DataMember()]
		public Dictionary<string, string> VariableAlias { get => variableAlias; set => variableAlias = value; }
	}
}
