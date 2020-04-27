using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Data.NTier;
using GeneXus.Http;
using GeneXus.HttpHandlerFactory;
using GeneXus.Metadata;
using GeneXus.Procedure;
using GeneXus.Utils;
using log4net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace GeneXus.Application
{
	public class Program
	{
		const string DEFAULT_PORT = "80";
		public static void Main(string[] args)
		{

			string port = DEFAULT_PORT;
			if (args.Length > 2)
			{
				Startup.VirtualPath = args[0];
				Startup.LocalPath = args[1];
				port = args[2];
			}
			if (port == DEFAULT_PORT)
			{
				BuildWebHost(null).Run();
			}
			else
			{
				BuildWebHostPort(null, port).Run();
			}
		}
		public static IWebHost BuildWebHost(string[] args) =>
		   WebHost.CreateDefaultBuilder(args)
			.ConfigureLogging(logging => logging.AddConsole())
			.UseStartup<Startup>()
			.Build();

		public static IWebHost BuildWebHostPort(string[] args, string port) =>
		   WebHost.CreateDefaultBuilder(args)
				.ConfigureLogging(logging => logging.AddConsole())
				.UseUrls($"http://*:{port}")
			   .UseStartup<Startup>()
			   .Build();
	}

	public class SingleMap
	{
		String name;
		String implementation;
		String methodName;

		public string Name { get => name; set => name = value; }
		public string ServiceMethod { get => methodName; set => methodName = value; }
		public string Implementation { get => implementation; set => implementation = value; }
	
	}

	public class MapGroup
	{

		String _objectType;
		String _name;
		String _basePath;
		SingleMap[] _mappings;

		public string ObjectType { get => _objectType; set => _objectType = value; }
		public string Name { get => _name; set => _name = value; }
		public string BasePath { get => _basePath; set => _basePath = value; }
		public SingleMap[] Mappings { get => _mappings; set => _mappings = value; }
	}

	public static class GXHandlerExtensions
	{
		public static IApplicationBuilder UseGXHandlerFactory(this IApplicationBuilder builder, string basePath)
		{
			return builder.UseMiddleware<HandlerFactory>(basePath);
		}
		public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app,
															  PathString path)
		{
			return app.Map(path, (_app) => _app.UseMiddleware<Notifications.WebSocket.WebSocketManagerMiddleware>());
		}
	}
	public class Startup
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(Startup));
		const int DEFAULT_SESSION_TIMEOUT_MINUTES = 20;
		public static string VirtualPath = string.Empty;
		public static string LocalPath = Directory.GetCurrentDirectory();
		static string ContentRootPath;
		static char[] urlSeparator = {'/','\\'};
		const char QUESTIONMARK = '?';
		const string UrlTemplateControllerWithParms = "controllerWithParms";
		const string RESOURCES_FOLDER = "Resources";
		const string TRACE_FOLDER = "logs";
		const string TRACE_PATTERN = "trace.axd";
		const string REST_BASE_URL = "rest/";

		public List<String> servicesPathUrl = new List<String>();
		public List<String> servicesBase = new List<String>();
		public Dictionary<String, Dictionary<String, String>> servicesMap = new Dictionary<String, Dictionary<string, string>>();

		public Startup(IHostingEnvironment env)
        {
			var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
			ContentRootPath = env.ContentRootPath;
			Config.ConfigRoot = builder.Build();
			GxContext.IsHttpContext = true;
			ServicesGroupSetting();
		}

		public void ServicesGroupSetting()
		{
			string[] grpFiles = Directory.GetFiles(ContentRootPath, "*.grp.json");
			foreach (String grp in grpFiles)
			{
				object p = JSONHelper.Deserialize<MapGroup>(File.ReadAllText(grp));
				MapGroup m = p as MapGroup;
				if (m != null)
				{
					
					if (String.IsNullOrEmpty(m.BasePath))
					{
						m.BasePath = REST_BASE_URL;
					}
					String mapPath = (m.BasePath.EndsWith("/")) ? m.BasePath : m.BasePath + "/";
					String mapPathLower = mapPath.ToLower();
					servicesPathUrl.Add(mapPath);
					foreach (SingleMap sm in m.Mappings)
					{
						if (servicesMap.ContainsKey(mapPathLower))
						{
							if (!servicesMap[mapPathLower].ContainsKey(sm.Name.ToLower()))
							{
								servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm.ServiceMethod);
							}
						}
						else {
							servicesMap.Add(mapPathLower, new Dictionary<string, string>());
							servicesMap[mapPathLower].Add(sm.Name.ToLower(), sm.ServiceMethod);
						}
					}
				}

			}
		}

		Boolean serviceInPath(String path, out String actualPath)
		{
			actualPath = "";
			foreach (String subPath in servicesPathUrl)
			{
				if (path.ToLower().Contains($"/{subPath.ToLower()}"))
				{
					actualPath = subPath.ToLower();
					return true;
				}
			}
			return false;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc(option => option.EnableEndpointRouting = false);
			services.Configure<AppSettings>(Config.ConfigRoot.GetSection("AppSettings"));
			services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
			services.Configure<IISServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
			services.AddDistributedMemoryCache();
			AppSettings settings = new AppSettings();
			Config.ConfigRoot.GetSection("AppSettings").Bind(settings);
			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(settings.SessionTimeout==0 ? DEFAULT_SESSION_TIMEOUT_MINUTES : settings.SessionTimeout); 
				options.Cookie.HttpOnly = true;
			});


			services.AddDirectoryBrowser();
			if (GXUtil.CompressResponse())
			{
				services.AddResponseCompression(options =>
				{
					options.MimeTypes = new[]
					{
							// Default
							"text/plain",
							"text/css",
							"application/javascript",
							"text/html",
							"application/xml",
							"text/xml",
							"application/json",
							"text/json",
							// Custom
							"application/json",
							"application/pdf"
							};
					options.EnableForHttps = true;
				});
			}
			services.AddMvc();
		}
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			var baseVirtualPath = string.IsNullOrEmpty(VirtualPath) ? VirtualPath : $"/{VirtualPath}";
			
			var provider = new FileExtensionContentTypeProvider();
			//mappings
			provider.Mappings[".json"] = "application/json";
			provider.Mappings[".woff"] = "font/woff";
			provider.Mappings[".woff2"] = "font/woff2";
			provider.Mappings[".tmp"] = "image/jpeg";
			provider.Mappings[".cod"] = "application/vnd.rim.cod";
			provider.Mappings[".jad"] = "text/vnd.sun.j2me.app-descriptor";
			provider.Mappings[".apk"] = "application/vnd.android.package-archive";
			provider.Mappings[".gxsd"] = "application/zip";
			provider.Mappings[".caf"] = "audio/x-caf";
			provider.Mappings[".yaml"] = "text/yaml";
			provider.Mappings[".otf"] = "font/opentype";
			provider.Mappings[".pdf"] = "application/pdf";
			provider.Mappings[".log"] = "text/plain";
			provider.Mappings[".usdz"] = "model/vnd.pixar.usd";
			provider.Mappings[".sfb"] = "model/sfb";
			provider.Mappings[".gltf"] = "model/gltf+json";
			if (GXUtil.CompressResponse())
			{
				app.UseResponseCompression();
			}
			app.UseCookiePolicy();
			app.UseSession();
			app.UseStaticFiles();

			if (Directory.Exists(Path.Combine(LocalPath, RESOURCES_FOLDER)))
			{
				app.UseStaticFiles(new StaticFileOptions()
				{
					FileProvider = new PhysicalFileProvider(Path.Combine(LocalPath, RESOURCES_FOLDER)),
					RequestPath = new PathString($"{baseVirtualPath}/{RESOURCES_FOLDER}"),
					ContentTypeProvider = provider
				});
			}
			string traceFolder = Path.Combine(LocalPath, TRACE_FOLDER);
			Config.LoadConfiguration();
			if (Preferences.HttpProtocolSecure())
			{
				app.UseHttpsRedirection();
				app.UseHsts();
			}
			if (log.IsDebugEnabled)
			{
				try
				{
					if (!Directory.Exists(traceFolder))
						Directory.CreateDirectory(traceFolder);
				}
				catch { }
				if (Directory.Exists(Path.Combine(LocalPath, TRACE_FOLDER)))
				{
					app.UseDirectoryBrowser(new DirectoryBrowserOptions
					{
						FileProvider = new PhysicalFileProvider(traceFolder),
						RequestPath = $"/{TRACE_PATTERN}"
					});
					app.UseStaticFiles(new StaticFileOptions()
					{
						FileProvider = new PhysicalFileProvider(traceFolder),
						RequestPath = new PathString($"{baseVirtualPath}/{TRACE_PATTERN}"),
						ContentTypeProvider = provider
					});
				}
			}

			app.UseStaticFiles(new StaticFileOptions()
			{
				FileProvider = new PhysicalFileProvider(LocalPath),
				RequestPath = new PathString($"{baseVirtualPath}"),
				OnPrepareResponse = s =>
				{
					var path = s.Context.Request.Path;
					if (path.HasValue &&  path.Value.IndexOf("/appsettings.json", StringComparison.OrdinalIgnoreCase)>=0)
					{
						s.Context.Response.StatusCode = 401;
						s.Context.Response.Body = Stream.Null;
						s.Context.Response.ContentLength = 0;
					}
				},
				ContentTypeProvider = provider
			});
			
			foreach( String p in servicesPathUrl)
			{
				 servicesBase.Add( string.IsNullOrEmpty(VirtualPath) ? p : $"{VirtualPath}/{p}");
			}

			var restBasePath = string.IsNullOrEmpty(VirtualPath) ? REST_BASE_URL : $"{VirtualPath}/{REST_BASE_URL}";

			app.UseMvc(routes =>
			{
				foreach (String serviceBasePath in servicesBase)
				{
					routes.MapRoute($"{serviceBasePath}{{*{UrlTemplateControllerWithParms}}}", new RequestDelegate(ProcessRestRequest));
				}
				routes.MapRoute($"{restBasePath}{{*{UrlTemplateControllerWithParms}}}", new RequestDelegate(ProcessRestRequest));
				routes.MapRoute("Default", VirtualPath, new { controller = "Home", action = "Index" });
			});

			app.UseWebSockets();
			var basePath = string.IsNullOrEmpty(VirtualPath) ? string.Empty : $"/{VirtualPath}";
			app.MapWebSocketManager($"{basePath}/gxwebsocket.svc");

			app.MapWhen(
				context => IsAspx(context, basePath),
						appBranch => {
							appBranch.UseGXHandlerFactory(basePath);
						});
			app.UseEnableRequestRewind();
		}
		bool IsAspx(HttpContext context, string basePath)
		{
			return HandlerFactory.IsAspxHandler(context.Request.Path.Value, basePath);
		}
		static public List<ControllerInfo> GetRouteController(string path)
		{
			List<ControllerInfo> result = new List<ControllerInfo>();
			string parms = string.Empty;
			GXLogging.Debug(log, "GetRouteController path:", path);
			try {
				if (!string.IsNullOrEmpty(path))
				{
					int questionMarkIdx = path.IndexOf(QUESTIONMARK);
					string controller;
					if (questionMarkIdx >= 0)
					{
						// rest/module1/module2/service?paramaters
						controller = path.Substring(0, questionMarkIdx).TrimEnd(urlSeparator);
						if (path.Length > questionMarkIdx + 1)
							parms = path.Substring(questionMarkIdx + 1);

						result.Add(new ControllerInfo() { Name = controller, Parameters = parms });
					}
					else
					{
						// rest/module1/module2/service
						controller = path.TrimEnd(urlSeparator);
						result.Add(new ControllerInfo() { Name = controller, Parameters = parms });

						// rest/module1/module2/service/paramaters
						int idx = path.LastIndexOfAny(urlSeparator);
						if (idx > 0 && idx < path.Length - 1)
						{
							controller = path.Substring(0, idx);
							parms = path.Substring(idx + 1);
							result.Add(new ControllerInfo() { Name = controller, Parameters = parms });
						}
					}
				}
			}catch(Exception ex)
			{
				GXLogging.Error(log, ex, "Controller match URL failed ", path);
			}
			return result;
		}
		Task ProcessRestRequest(HttpContext context)
		{
			try
			{
				string path = context.Request.Path.ToString();
				string actualPath = "";
				if (path.Contains($"/{REST_BASE_URL}") || serviceInPath(path, out actualPath))
				{
					string controllerWihtParms = context.GetRouteValue(UrlTemplateControllerWithParms) as string;
					List<ControllerInfo> controllers = GetRouteController(controllerWihtParms);
					GxRestWrapper controller = null;
					ControllerInfo controllerInfo = controllers.FirstOrDefault(c => (controller = GetController(context, c.Name)) != null);

					if (controller != null)
					{
						if (servicesMap.ContainsKey(actualPath) && (servicesMap[actualPath].TryGetValue(controllerInfo.Name.ToLower(), out String value)))
						{
							controller.ServiceMethod = value;
						}
						else
						{
							controller.ServiceMethod = null;
						}
						if (HttpMethods.IsGet(context.Request.Method))
						{
							return controller.Get(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsPost(context.Request.Method))
						{
							return controller.Post();
						}
						else if (HttpMethods.IsDelete(context.Request.Method))
						{
							return controller.Delete(controllerInfo.Parameters);
						}
						else if (HttpMethods.IsPut(context.Request.Method))
						{
							return controller.Put(controllerInfo.Parameters);
						}
					}
					else
					{
						context.Response.StatusCode = (int)HttpStatusCode.NotFound;
						context.Response.Headers.Clear();
					}
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException(ex);
			}
		}
		private GxRestWrapper GetController(HttpContext context, string controller)
		{

			GxContext gxContext = new GxContext
			{
				HttpContext = context
			};
			DataStoreUtil.LoadDataStores(gxContext);
			context.NewSessionCheck();
			string nspace;
			Config.GetValueOf("AppMainNamespace", out nspace);
			if (File.Exists(Path.Combine(ContentRootPath, controller + "_bc.svc")))
			{
				var sdtInstance = ClassLoader.FindInstance(Config.CommonAssemblyName, nspace,  GxSilentTrnSdt.GxSdtNameToCsharpName(controller), new Object[] { gxContext }, Assembly.GetEntryAssembly()) as GxSilentTrnSdt;
				if (sdtInstance != null)
					return new GXBCRestService(sdtInstance, context, gxContext);
			}
			else
			{
				string svcFile = Path.Combine(ContentRootPath, $"{controller.ToLower()}.svc");
				if (File.Exists(svcFile))
				{
					controller = new string(File.ReadLines(svcFile).First().SkipWhile(c => c != ',')
						   .Skip(1)
						   .TakeWhile(c => c != '"')
						   .ToArray()).Trim();
					var controllerInstance = ClassLoader.FindInstance(controller, nspace, controller, new Object[] { gxContext }, Assembly.GetEntryAssembly());
					GXProcedure proc = controllerInstance as GXProcedure;
					if (proc != null)
						return new GxRestWrapper(proc, context, gxContext);
				}
			}
			return null;
		}
	}
	public class EnableRequestRewindMiddleware
	{
		private readonly RequestDelegate _next;

		public EnableRequestRewindMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			context.Request.EnableBuffering();
			await _next(context);
		}
	}

	public static class EnableRequestRewindExtension
	{
		public static IApplicationBuilder UseEnableRequestRewind(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<EnableRequestRewindMiddleware>();
		}
	}
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			string[] defaultFiles = { "default.htm", "default.html", "index.htm", "index.html" };
			foreach (string file in defaultFiles) {
				if (System.IO.File.Exists(Path.Combine(Startup.LocalPath, file))){
					return Redirect(file);
				}
			}
			return Redirect(defaultFiles[0]);
		}
	}
}
