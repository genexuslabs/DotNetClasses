using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Http;
using GeneXus.HttpHandlerFactory;
using GeneXus.Services;
using GeneXus.Utils;
using GxClasses.Web.Middleware;
using log4net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GeneXus.Application
{
	public class Program
	{
		const string DEFAULT_PORT = "80";
		public static void Main(string[] args)
		{
			try
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
			catch (Exception e)
			{
				Console.Error.WriteLine("ERROR:");
				Console.Error.WriteLine("Web Host terminated unexpectedly: {0}", e.Message);
				Console.Read();
			}
		}
		public static IWebHost BuildWebHost(string[] args) =>
		   WebHost.CreateDefaultBuilder(args)
			.ConfigureLogging(logging => logging.AddConsole())
			.UseStartup<Startup>()
			.Build();

		public static IWebHost BuildWebHostPort(string[] args, string port)
		{
			return WebHost.CreateDefaultBuilder(args)
				 .ConfigureLogging(logging => logging.AddConsole())
				 .UseUrls(Preferences.HttpProtocolSecure() ? $"{Uri.UriSchemeHttps}://*:{port}" : $"{Uri.UriSchemeHttp}://*:{port}")
				.UseStartup<Startup>()
				.Build();
		}
	}

	public static class GXHandlerExtensions
	{
		public static IApplicationBuilder UseGXHandlerFactory(this IApplicationBuilder builder, string basePath)
		{
			return builder.UseMiddleware<HandlerFactory>(basePath);
		}
		public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app, string basePath)
		{
			return app
					.Map($"{basePath}/gxwebsocket"    , (_app) => _app.UseMiddleware<Notifications.WebSocket.WebSocketManagerMiddleware>())
					.Map($"{basePath}/gxwebsocket.svc", (_app) => _app.UseMiddleware<Notifications.WebSocket.WebSocketManagerMiddleware>()); //Compatibility reasons. Remove in the future.
		}
	}
  
	public class Startup
	{ 

		static readonly ILog log = log4net.LogManager.GetLogger(typeof(Startup));
		const int DEFAULT_SESSION_TIMEOUT_MINUTES = 20;
		const long DEFAULT_MAX_FILE_UPLOAD_SIZE_BYTES = 528000000;
		public static string VirtualPath = string.Empty;
		public static string LocalPath = Directory.GetCurrentDirectory();

		const string UrlTemplateControllerWithParms = "controllerWithParms";
		const string RESOURCES_FOLDER = "Resources";
		const string TRACE_FOLDER = "logs";
		const string TRACE_PATTERN = "trace.axd";
		const string REST_BASE_URL = "rest/";
		const string DATA_PROTECTION_KEYS = "DataProtection-Keys";
		const string REWRITE_FILE = "rewrite.config";
		const string SWAGGER_DEFAULT_YAML = "default.yaml";
		const string DEVELOPER_MENU = "developermenu.html";
		const string SWAGGER_SUFFIX = "swagger";

		public List<string> servicesBase = new List<string>();		

		private GXRouting gxRouting;

		public Startup(Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			GXRouting.ContentRootPath = env.ContentRootPath;
			GXRouting.UrlTemplateControllerWithParms = "controllerWithParms";
			GxContext.IsHttpContext = true;
			gxRouting = new GXRouting(REST_BASE_URL);
		}
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc(option => option.EnableEndpointRouting = false);
			services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
				options.Limits.MaxRequestBodySize = null;
			});
			services.Configure<IISServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
			services.AddDistributedMemoryCache();

			services.Configure<FormOptions>(options =>
			{
				if (Config.GetValueOf("MaxFileUploadSize", out string MaxFileUploadSizeStr) && long.TryParse(MaxFileUploadSizeStr, out long MaxFileUploadSize))
				{
					GXLogging.Info(log, $"MaxFileUploadSize:{MaxFileUploadSize}");
					options.MultipartBodyLengthLimit = MaxFileUploadSize;
				}
				else
				{
					GXLogging.Info(log, $"MaxFileUploadSize DefaultValue:{DEFAULT_MAX_FILE_UPLOAD_SIZE_BYTES}");
					options.MultipartBodyLengthLimit = DEFAULT_MAX_FILE_UPLOAD_SIZE_BYTES;
				}
			});
			ISessionService sessionService = GXSessionServiceFactory.GetProvider();

			if (sessionService != null)
				ConfigureSessionService(services, sessionService);
			services.AddHttpContextAccessor();
			services.AddSession(options =>
			{
				if (Config.GetValueOf("SessionTimeout", out string SessionTimeoutStr) && int.TryParse(SessionTimeoutStr, out int SessionTimeout))
					options.IdleTimeout = TimeSpan.FromMinutes(SessionTimeout);
				else
					options.IdleTimeout = TimeSpan.FromMinutes(DEFAULT_SESSION_TIMEOUT_MINUTES); 
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
				options.Cookie.IsEssential = true;
				string sameSite;
				SameSiteMode sameSiteMode = SameSiteMode.Unspecified;
				if (Config.GetValueOf("SAMESITE_COOKIE", out sameSite) && Enum.TryParse<SameSiteMode>(sameSite, out sameSiteMode))
				{
					options.Cookie.SameSite = sameSiteMode;
				}
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

		private void ConfigureSessionService(IServiceCollection services, ISessionService sessionService)
		{
			if (sessionService is GxRedisSession)
			{
				services.AddStackExchangeRedisCache(options =>
				{
					options.Configuration = sessionService.ConnectionString;
					options.InstanceName = sessionService.InstanceName;
				});
				services.AddDataProtection().PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(sessionService.ConnectionString), DATA_PROTECTION_KEYS).SetApplicationName(sessionService.InstanceName);
			}
			else if (sessionService is GxDatabaseSession)
			{
				services.AddDistributedSqlServerCache(options =>
				{
					options.ConnectionString = sessionService.ConnectionString;
					options.SchemaName = sessionService.Schema;
					options.TableName = sessionService.TableName;
				});
			}
		}
		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			string baseVirtualPath = string.IsNullOrEmpty(VirtualPath) ? VirtualPath : $"/{VirtualPath}";
			
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
			ConfigureSwaggerUI(app, baseVirtualPath);

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
			string rewriteFile = Path.Combine(LocalPath, REWRITE_FILE);
			if (File.Exists(rewriteFile))
				AddRewrite(app, rewriteFile, baseVirtualPath);

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
			
			foreach( string p in gxRouting.servicesPathUrl.Keys)
			{
				 servicesBase.Add( string.IsNullOrEmpty(VirtualPath) ? p : $"{VirtualPath}/{p}");
			}
			app.UseExceptionHandler(new ExceptionHandlerOptions
			{
				ExceptionHandler = new CustomExceptionHandlerMiddleware().Invoke,
				AllowStatusCode404Response = true
			});

			string restBasePath = string.IsNullOrEmpty(VirtualPath) ? REST_BASE_URL : $"{VirtualPath}/{REST_BASE_URL}";
			string apiBasePath = string.IsNullOrEmpty(VirtualPath) ? string.Empty : $"{VirtualPath}/";
			app.UseMvc(routes =>
			{
				foreach (string serviceBasePath in servicesBase)
				{			
					string tmpPath = string.IsNullOrEmpty(apiBasePath) ? serviceBasePath : serviceBasePath.Replace(apiBasePath, string.Empty);
					foreach (string sPath in gxRouting.servicesValidPath[tmpPath])
					{
						string s = serviceBasePath + sPath;
						routes.MapRoute($"{s}", new RequestDelegate(gxRouting.ProcessRestRequest));
					}
				}
				routes.MapRoute($"{restBasePath}{{*{UrlTemplateControllerWithParms}}}", new RequestDelegate(gxRouting.ProcessRestRequest));
				routes.MapRoute("Default", VirtualPath, new { controller = "Home", action = "Index" });
			});

			app.UseWebSockets();
			string basePath = string.IsNullOrEmpty(VirtualPath) ? string.Empty : $"/{VirtualPath}";
			Config.ScriptPath = basePath;
			app.MapWebSocketManager(basePath);

			app.MapWhen(
				context => IsAspx(context, basePath),
						appBranch =>
						{
							appBranch.UseGXHandlerFactory(basePath);
						});
			app.Run(async context => 
			{
				await Task.FromException(new PageNotFoundException(context.Request.Path.Value));
			});
			app.UseEnableRequestRewind();
		}

		private void ConfigureSwaggerUI(IApplicationBuilder app, string baseVirtualPath)
		{
			try
			{
				string baseVirtualPathWithSep = string.IsNullOrEmpty(baseVirtualPath) ? string.Empty: $"{baseVirtualPath.TrimStart('/')}/";
				foreach (string yaml in Directory.GetFiles(LocalPath, "*.yaml")) {
					FileInfo finfo = new FileInfo(yaml);

					app.UseSwaggerUI(options =>
					{
						options.SwaggerEndpoint($"../../{finfo.Name}", finfo.Name);
						options.RoutePrefix =$"{baseVirtualPathWithSep}{finfo.Name}/{SWAGGER_SUFFIX}";
					});
					if (finfo.Name.Equals(SWAGGER_DEFAULT_YAML, StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(LocalPath, DEVELOPER_MENU)))
						app.UseSwaggerUI(options =>
						{
							options.SwaggerEndpoint($"../../{SWAGGER_DEFAULT_YAML}", SWAGGER_DEFAULT_YAML);
							options.RoutePrefix =$"{baseVirtualPathWithSep}{DEVELOPER_MENU}/{SWAGGER_SUFFIX}";
						});

				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Errpr loading SwaggerUI " + ex.Message);
			}
		}

		private void AddRewrite(IApplicationBuilder app, string rewriteFile, string baseURL)
		{
			string rules = File.ReadAllText(rewriteFile);
			rules = rules.Replace("{BASEURL}", baseURL);
			
			using (var apacheModRewriteStreamReader = new StringReader(rules))
			{
				var options = new RewriteOptions().AddApacheModRewrite(apacheModRewriteStreamReader);
				app.UseRewriter(options);
			}
		}

		bool IsAspx(HttpContext context, string basePath)
		{
			return HandlerFactory.IsAspxHandler(context.Request.Path.Value, basePath);
		}		
	}
	public class CustomExceptionHandlerMiddleware
	{
		public async Task Invoke(HttpContext httpContext)
		{
			Exception ex = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
			HttpStatusCode httpStatusCode = (HttpStatusCode)httpContext.Response.StatusCode;
			if (ex!=null)
			{
				if (ex is PageNotFoundException)
				{
					httpStatusCode = HttpStatusCode.NotFound;
				}
				else
				{
					httpStatusCode = HttpStatusCode.InternalServerError;
				}
			}
			if (httpStatusCode!= HttpStatusCode.OK)
			{
				string redirectPage = Config.MapCustomError(httpStatusCode.ToString(HttpHelper.INT_FORMAT));
				if (!string.IsNullOrEmpty(redirectPage))
{
					httpContext.Response.Redirect($"{httpContext.Request.GetApplicationPath()}/{redirectPage}");
				}
				else
				{
					httpContext.Response.StatusCode = (int)httpStatusCode;
				}
			}
			await Task.CompletedTask;
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
