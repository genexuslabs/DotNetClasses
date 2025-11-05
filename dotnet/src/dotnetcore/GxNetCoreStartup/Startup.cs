using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Http;
using GeneXus.HttpHandlerFactory;
using GeneXus.Services;
using GeneXus.Services.OpenTelemetry;
using GeneXus.Utils;
using GxClasses.Web.Middleware;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace GeneXus.Application
{
	public class Program
	{
		const string DEFAULT_PORT = "80";
		const int GRACEFUL_SHUTDOWN_DELAY_SECONDS = 30;
		static string DEFAULT_SCHEMA = Uri.UriSchemeHttp;

		public static void Main(string[] args)
		{
			try
			{
				string port = DEFAULT_PORT;
				string schema = DEFAULT_SCHEMA;
				if (args.Length > 2)
				{
					Startup.VirtualPath = args[0];
					Startup.LocalPath = args[1];
					port = args[2];
					if (args.Length > 3 && Uri.UriSchemeHttps.Equals(args[3], StringComparison.OrdinalIgnoreCase))
						schema = Uri.UriSchemeHttps;
					if (args.Length > 4)
						Startup.IsMcp = args[4].Equals("mcp", StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					LocatePhysicalLocalPath();

				}

				if (port == DEFAULT_PORT)
				{
					BuildWebHost(null).Run();
				}
				else
				{
					BuildWebHostPort(null, port, schema).Run();
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("ERROR:");
				Console.Error.WriteLine("Web Host terminated unexpectedly: {0}", e.Message);
			}
		}

		public static IWebHost BuildWebHost(string[] args) =>
		   WebHost.CreateDefaultBuilder(args)
		   .UseStartup<Startup>()
		   .UseContentRoot(Startup.LocalPath)
		   .UseShutdownTimeout(TimeSpan.FromSeconds(GRACEFUL_SHUTDOWN_DELAY_SECONDS))
		   .Build();

		public static IWebHost BuildWebHostPort(string[] args, string port)
		{
			return BuildWebHostPort(args, port, DEFAULT_SCHEMA);
		}
		static IWebHost BuildWebHostPort(string[] args, string port, string schema)
		{
			return WebHost.CreateDefaultBuilder(args)
					 .UseUrls($"{schema}://*:{port}")
					.UseStartup<Startup>()
					.UseWebRoot(Startup.LocalPath)
					.UseContentRoot(Startup.LocalPath)
					.UseShutdownTimeout(TimeSpan.FromSeconds(GRACEFUL_SHUTDOWN_DELAY_SECONDS))
					.Build();
		}

		private static void LocatePhysicalLocalPath()
		{
			string startup = FileUtil.GetStartupDirectory();
			string startupParent = Directory.GetParent(startup).FullName;
			if (startup == Startup.LocalPath && !File.Exists(Path.Combine(startup, Startup.APP_SETTINGS)) && File.Exists(Path.Combine(startupParent, Startup.APP_SETTINGS)))
				Startup.LocalPath = startupParent;
		}
	}

	public static class GXHandlerExtensions
	{
		public static IApplicationBuilder UseAntiforgeryTokens(this IApplicationBuilder app, string basePath)
		{
			return app.UseMiddleware<ValidateAntiForgeryTokenMiddleware>(basePath);
		}
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
	public class CustomBadRequestObjectResult : ObjectResult
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(CustomBadRequestObjectResult).FullName);
		public CustomBadRequestObjectResult(ActionContext context)
			: base(HttpHelper.GetJsonError(StatusCodes.Status400BadRequest.ToString(), HttpHelper.StatusCodeToTitle(HttpStatusCode.BadRequest)))
		{
			LogErrorResponse(context);
			StatusCode = StatusCodes.Status400BadRequest;
		}
		static void LogErrorResponse(ActionContext context)
		{
			if (log.IsErrorEnabled)
			{
				foreach (KeyValuePair<string, ModelStateEntry> entry in context.ModelState)
				{
					if (entry.Value.Errors.Count > 0)
					{
						foreach (ModelError error in entry.Value.Errors)
						{
							GXLogging.Error(log, "Field ", entry.Key, "Errors:", error.ErrorMessage);
						}
					}
				}
			}
		}
	}

	public class Startup
	{
		static IGXLogger log;
		internal static string APPLICATIONINSIGHTS_CONNECTION_STRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";

		const long DEFAULT_MAX_FILE_UPLOAD_SIZE_BYTES = 528000000;
		public static string VirtualPath = string.Empty;
		public static string LocalPath = Directory.GetCurrentDirectory();
		public static bool IsMcp = false;
		internal static string APP_SETTINGS = "appsettings.json";

		const string UrlTemplateControllerWithParms = "controllerWithParms";
		const string RESOURCES_FOLDER = "Resources";
		const string TRACE_FOLDER = "logs";
		const string TRACE_PATTERN = "trace.axd";
		internal const string REST_BASE_URL = "rest/";
		const string DATA_PROTECTION_KEYS = "DataProtection-Keys";
		const string REWRITE_FILE = "rewrite.config";
		const string SWAGGER_DEFAULT_YAML = "default.yaml";
		const string DEVELOPER_MENU = "developermenu.html";
		const string SWAGGER_SUFFIX = "swagger";
		const string CORS_POLICY_NAME = "AllowSpecificOriginsPolicy";
		const string CORS_ANY_ORIGIN = "*";
		const double CORS_MAX_AGE_SECONDS = 86400;
		internal const string GX_CONTROLLERS = "gxcontrollers";
		internal static string DefaultFileName { get; set; }

		public List<string> servicesBase = new List<string>();		

		private GXRouting gxRouting;
		public Startup(IConfiguration configuration, IHostingEnvironment env)
		{
			Config.ConfigRoot = configuration;
			GxContext.IsHttpContext = true;
			Config.LoadConfiguration();
			GXRouting.ContentRootPath = env.ContentRootPath;
			GXRouting.UrlTemplateControllerWithParms = "controllerWithParms";
			gxRouting = new GXRouting(REST_BASE_URL);
			log = GXLoggerFactory.GetLogger<Startup>();
		}
		public void ConfigureServices(IServiceCollection services)
		{
			OpenTelemetryService.Setup(services);

			services.AddHealthChecks()
					.AddCheck("liveness", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
					.AddCheck("readiness", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });

			services.Configure<MimeMappingsOptions>(Config.ConfigRoot.GetSection("MimeMappings"));

			IMvcBuilder builder = services.AddMvc(option =>
			{
				option.EnableEndpointRouting = false;
				option.Conventions.Add(new HomeControllerConvention());
			});

			RegisterControllerAssemblies(builder);

			services.Configure<KestrelServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
				options.Limits.MaxRequestBodySize = null;
				if (Config.GetValueOrEnvironmentVarOf("MinRequestBodyDataRate", out string MinRequestBodyDataRateStr) && double.TryParse(MinRequestBodyDataRateStr, out double MinRequestBodyDataRate))
				{
					GXLogging.Info(log, $"MinRequestBodyDataRate:{MinRequestBodyDataRate}");
					options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: MinRequestBodyDataRate, gracePeriod: TimeSpan.FromSeconds(10));
				}
			});
			services.Configure<IISServerOptions>(options =>
			{
				options.AllowSynchronousIO = true;
			});
			services.AddLogging(builder => builder.AddConsole());
			services.Configure<FormOptions>(Config.ConfigRoot.GetSection("FormOptions"));
			services.PostConfigure<FormOptions>(options =>
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

			services.AddHttpContextAccessor();
			if (sessionService != null)
				ConfigureSessionService(services, sessionService);

			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(Preferences.SessionTimeout);
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
				options.Cookie.IsEssential = true;
				string sessionCookieName = GxWebSession.GetSessionCookieName(VirtualPath);
				if (!string.IsNullOrEmpty(sessionCookieName))
				{
					options.Cookie.Name=sessionCookieName;
					GxWebSession.SessionCookieName = sessionCookieName;
				}
				string sameSite;
				SameSiteMode sameSiteMode = SameSiteMode.Unspecified;
				if (Config.GetValueOf("SAMESITE_COOKIE", out sameSite) && Enum.TryParse<SameSiteMode>(sameSite, out sameSiteMode))
				{
					options.Cookie.SameSite = sameSiteMode;
				}
			});

			if (RestAPIHelpers.ValidateCsrfToken())
			{
				services.AddAntiforgery(options =>
				{
					options.HeaderName = HttpHeader.X_CSRF_TOKEN_HEADER;
					options.SuppressXFrameOptionsHeader = true;
				});
			}
			if (Startup.IsMcp)
			{
				StartupMcp.AddService(services);
			}

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
			DefineCorsPolicy(services);
		}

		private void RegisterControllerAssemblies(IMvcBuilder mvcBuilder)
		{
			
			if (RestAPIHelpers.ServiceAsController())
			{
				mvcBuilder.AddMvcOptions(options => options.ModelBinderProviders.Insert(0, new QueryStringModelBinderProvider()));
				if (!string.IsNullOrEmpty(VirtualPath))
				{
					mvcBuilder.AddMvcOptions(options => options.Conventions.Add(new SetRoutePrefix(new RouteAttribute(VirtualPath))));
				}
				mvcBuilder.AddMvcOptions(options => options.AllowEmptyInputInBodyModelBinding = true);
			}

			if (RestAPIHelpers.JsonSerializerCaseSensitive())
			{
				mvcBuilder.AddJsonOptions(options => options.JsonSerializerOptions.PropertyNameCaseInsensitive = false);
			}

			mvcBuilder.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
				options.JsonSerializerOptions.Converters.Add(new StringConverter());
			});

			mvcBuilder.ConfigureApiBehaviorOptions(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					return new CustomBadRequestObjectResult(context);
				};
			});

			if (RestAPIHelpers.ServiceAsController())
			{
				RegisterRestServices(mvcBuilder);
				RegisterApiServices(mvcBuilder, gxRouting);
			}
			RegisterNativeServices(mvcBuilder);

		}

		private void RegisterNativeServices(IMvcBuilder mvcBuilder)
		{
			try
			{
				string controllers = Path.Combine(Startup.LocalPath, "bin", GX_CONTROLLERS);

				if (Directory.Exists(controllers))
				{
					foreach (string controller in Directory.GetFiles(controllers))
					{
						Console.WriteLine($"Loading controller {controller}");
						mvcBuilder.AddApplicationPart(Assembly.LoadFrom(controller)).AddControllersAsServices();
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Error loading gxcontrollers " + ex.Message);
			}

		}

		private void RegisterRestServices(IMvcBuilder mvcBuilder)
		{
			HashSet<string> serviceAssemblies = new HashSet<string>();
			foreach (string svcFile in gxRouting.svcFiles)
			{
				try
				{
					string[] controllerAssemblyQualifiedName = new string(File.ReadLines(svcFile).First().SkipWhile(c => c != '"')
															   .Skip(1)
															   .TakeWhile(c => c != '"')
															   .ToArray()).Trim().Split(',');
					string controllerAssemblyName = controllerAssemblyQualifiedName.Last();
					if (!serviceAssemblies.Contains(controllerAssemblyName))
					{
						serviceAssemblies.Add(controllerAssemblyName);
						string controllerAssemblyFile = Path.Combine(Startup.LocalPath, "bin", $"{controllerAssemblyName}.dll");

						if (File.Exists(controllerAssemblyFile))
						{
							GXLogging.Info(log, "Registering rest: " + controllerAssemblyName);
							mvcBuilder.AddApplicationPart(Assembly.LoadFrom(controllerAssemblyFile)).AddControllersAsServices();
						}
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error registering rest service", ex);
				}
			}
		}
		private void RegisterApiServices(IMvcBuilder mvcBuilder, GXRouting gxRouting)
		{
			HashSet<string> serviceAssemblies = new HashSet<string>();
			foreach (string grp in gxRouting.servicesPathUrl.Values)
			{
				try
				{
					string assemblyName = grp.Replace('\\', '.');
					if (!serviceAssemblies.Contains(assemblyName))
					{
						serviceAssemblies.Add(assemblyName);
						string controllerAssemblyFile = Path.Combine(Startup.LocalPath, "bin", $"{assemblyName}.dll");
						if (File.Exists(controllerAssemblyFile))
						{
							GXLogging.Info(log, "Registering api: " + grp);
							mvcBuilder.AddApplicationPart(Assembly.LoadFrom(controllerAssemblyFile)).AddControllersAsServices();
						}
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error registering api", ex);
				}
			}
		}

		private void DefineCorsPolicy(IServiceCollection services)
		{
			if (Preferences.CorsEnabled)
			{
				string corsAllowedOrigins = Preferences.CorsAllowedOrigins();
				if (!string.IsNullOrEmpty(corsAllowedOrigins))
				{
					string[] origins = corsAllowedOrigins.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
					foreach (string origin in origins)
					{
						GXLogging.Info(log, $"Adding origin to CORS policy:", origin);
					}
					services.AddCors(options =>
					{
						options.AddPolicy(name: CORS_POLICY_NAME,
										  policy =>
										  {
											  policy.WithOrigins(origins);
											  if (!corsAllowedOrigins.Contains(CORS_ANY_ORIGIN))
											  {
												  policy.AllowCredentials();
											  }
											  policy.AllowAnyHeader();
											  policy.AllowAnyMethod();
											  policy.SetPreflightMaxAge(TimeSpan.FromSeconds(CORS_MAX_AGE_SECONDS));
										  });
					});
				}
			}
		}

		private void ConfigureSessionService(IServiceCollection services, ISessionService sessionService)
		{
			
			if (sessionService is GxRedisSession)
			{
				GxRedisSession gxRedisSession = (GxRedisSession)sessionService;
				if (gxRedisSession.IsMultitenant)
				{
					GXLogging.Info(log, $"Using multi-tenant Redis for Distributed session, ConnectionString:{sessionService.ConnectionString}, InstanceName: {sessionService.InstanceName}");

					services.AddSingleton<IDistributedCache, TenantRedisCache>();
					services.AddDataProtection().PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(sessionService.ConnectionString), DATA_PROTECTION_KEYS).SetApplicationName("default");
				}
				else
				{
					GXLogging.Info(log, $"Using Redis for Distributed session, ConnectionString:{sessionService.ConnectionString}, InstanceName: {sessionService.InstanceName}");

					services.AddSingleton<IDistributedCache>(sp =>
						new CustomRedisSessionStore(sessionService.ConnectionString, TimeSpan.FromMinutes(Preferences.SessionTimeout), sessionService.InstanceName));

					services.AddDataProtection().PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(sessionService.ConnectionString), DATA_PROTECTION_KEYS).SetApplicationName(sessionService.InstanceName);
				}
			}
			else if (sessionService is GxDatabaseSession)
			{

				if (Preferences.IsBeforeConnectEventConfigured())
				{
					services.AddTransient<CacheResolver>(_ => connectionString =>
					{
						GXLogging.Info(log, $"Using SQLServer for request scoped Distributed session, ConnectionString:{sessionService.ConnectionString}, SchemaName: {sessionService.Schema}, TableName: {sessionService.TableName}");
						Action<SqlServerCacheOptions> cacheConfigOptions = options =>
						{
							options.ConnectionString = connectionString;
							options.SchemaName = sessionService.Schema;
							options.TableName = sessionService.TableName;
							options.DefaultSlidingExpiration = TimeSpan.FromMinutes(sessionService.SessionTimeout);
						};
						services.AddOptions();
						services.Configure(cacheConfigOptions);
						services.AddTransient<SqlServerCache>();
						return services.BuildServiceProvider().GetService<SqlServerCache>();
					});
					services.AddTransient<IDistributedCache, CustomCacheProvider>();
				}
				else
				{
					services.AddDistributedSqlServerCache(options =>
					{
						GXLogging.Info(log, $"Using SQLServer for Distributed session, ConnectionString:{sessionService.ConnectionString}, SchemaName: {sessionService.Schema}, TableName: {sessionService.TableName}");
						options.ConnectionString = sessionService.ConnectionString;
						options.SchemaName = sessionService.Schema;
						options.TableName = sessionService.TableName;
						options.DefaultSlidingExpiration = TimeSpan.FromMinutes(sessionService.SessionTimeout);
					});
				}
			}
		}
		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ILoggerFactory loggerFactory,
			IHttpContextAccessor contextAccessor,
			Microsoft.Extensions.Hosting.IHostApplicationLifetime applicationLifetime,
			IOptions<MimeMappingsOptions> mimeMappingsOptions)
		{
			// Registrar para el graceful shutdown
			applicationLifetime.ApplicationStopping.Register(OnShutdown);

			string baseVirtualPath = string.IsNullOrEmpty(VirtualPath) ? VirtualPath : $"/{VirtualPath}";
			LogConfiguration.SetupLog4Net();
			AppContext.Configure(contextAccessor);

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
			provider.Mappings[".ini"] = "text/plain";

			if (mimeMappingsOptions?.Value != null)
			{
				foreach (var mapping in mimeMappingsOptions.Value)
				{
					provider.Mappings[mapping.Key] = mapping.Value;
				}
			}
			if (GXUtil.CompressResponse())
			{
				app.UseResponseCompression();
			}
			app.UseRouting();
			app.UseCookiePolicy();
			if (Preferences.IsBeforeConnectEventConfigured())
			{
				app.UseMiddleware<EnableCustomSessionStoreMiddleware>();
			}
			app.UseAsyncSession();
			app.UseStaticFiles();
			ISessionService sessionService = GXSessionServiceFactory.GetProvider();
			app.UseMiddleware<TenantMiddleware>();

			ConfigureCors(app);
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
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				// Endpoints para health checks (Kubernetes probes)
				endpoints.MapHealthChecks($"{baseVirtualPath}/_gx/health/live", new HealthCheckOptions
				{
					Predicate = check => check.Tags.Contains("live")
				});

				endpoints.MapHealthChecks($"{baseVirtualPath}/_gx/health/ready", new HealthCheckOptions
				{
					Predicate = check => check.Tags.Contains("ready")
				});
				if (Startup.IsMcp)
				{
					StartupMcp.MapEndpoints(endpoints);
				}
			});

			if (log.IsCriticalEnabled && env.IsDevelopment())
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

			string tempMediaDir = string.Empty;
			if (Config.GetValueOf("TMPMEDIA_DIR", out string mediaPath) && !PathUtil.IsAbsoluteUrlOrAnyScheme(mediaPath))
			{
				tempMediaDir = mediaPath;
			}
			app.UseStaticFiles(new StaticFileOptions()
			{
				FileProvider = new PhysicalFileProvider(LocalPath),
				RequestPath = new PathString($"{baseVirtualPath}"),
				OnPrepareResponse = s =>
				{
					PathString path = s.Context.Request.Path;
					bool appSettingsPath = path.HasValue && path.Value.IndexOf($"/{APP_SETTINGS}", StringComparison.OrdinalIgnoreCase) >= 0;
					bool tempMediaPath = path.StartsWithSegments($"{baseVirtualPath}/{tempMediaDir}", StringComparison.OrdinalIgnoreCase);
					bool privatePath = path.StartsWithSegments($"{baseVirtualPath}/{GXRouting.PRIVATE_DIR}", StringComparison.OrdinalIgnoreCase);
					if (appSettingsPath || tempMediaPath || privatePath)
					{
						s.Context.Response.StatusCode = 401;
						s.Context.Response.Body = Stream.Null;
						s.Context.Response.ContentLength = 0;
					}
				},
				ContentTypeProvider = provider
			});

			app.UseExceptionHandler(new ExceptionHandlerOptions
			{
				ExceptionHandler = new CustomExceptionHandlerMiddleware().Invoke,
				AllowStatusCode404Response = true
			});

			string restBasePath = string.IsNullOrEmpty(VirtualPath) ? REST_BASE_URL : $"{VirtualPath}/{REST_BASE_URL}";
			string apiBasePath = string.IsNullOrEmpty(VirtualPath) ? string.Empty : $"{VirtualPath}/";
			IAntiforgery antiforgery = null;
			if (RestAPIHelpers.ValidateCsrfToken())
			{
				antiforgery = app.ApplicationServices.GetRequiredService<IAntiforgery>();
				app.UseAntiforgeryTokens(apiBasePath);
			}
			if (!RestAPIHelpers.ServiceAsController())
			{
				foreach (string p in gxRouting.servicesPathUrl.Keys)
				{
					servicesBase.Add(string.IsNullOrEmpty(VirtualPath) ? p : $"{VirtualPath}/{p}");
				}
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
				});
			}

			app.UseWebSockets();
			string basePath = string.IsNullOrEmpty(VirtualPath) ? string.Empty : $"/{VirtualPath}";
			Config.ScriptPath = string.IsNullOrEmpty(basePath) ? "/" : basePath;
			app.MapWebSocketManager(basePath);

			app.UseGXHandlerFactory(basePath);

			app.Run(async context => 
			{
				await Task.FromException(new PageNotFoundException(context.Request.Path.Value));
			});
			app.UseEnableRequestRewind();
		}

		private void ConfigureCors(IApplicationBuilder app)
		{
			if (Preferences.CorsEnabled)
			{
				app.UseCors(CORS_POLICY_NAME);
			}
		}

		private void ConfigureSwaggerUI(IApplicationBuilder app, string baseVirtualPath)
		{
			try
			{
				string baseVirtualPathWithSep = string.IsNullOrEmpty(baseVirtualPath) ? string.Empty : $"{baseVirtualPath.TrimStart('/')}/";
				foreach (string yaml in Directory.GetFiles(LocalPath, "*.yaml"))
				{
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

		private void OnShutdown()
		{
			GXLogging.Info(log, "Application gracefully shutting down... Waiting for in-process requests to complete.");
			ThreadUtil.WaitForEnd();
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
	}
	public class CustomExceptionHandlerMiddleware
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<CustomExceptionHandlerMiddleware>();
		public async Task Invoke(HttpContext httpContext)
		{
			string httpReasonPhrase=string.Empty;
			Exception ex = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
			HttpStatusCode httpStatusCode = (HttpStatusCode)httpContext.Response.StatusCode;
			if (ex!=null)
			{
				if (ex is PageNotFoundException)
				{
					httpStatusCode = HttpStatusCode.NotFound;
				}
				else if (ex is AntiforgeryValidationException)
				{
					httpStatusCode = HttpStatusCode.BadRequest;
					httpReasonPhrase = HttpHelper.InvalidCSRFToken;
					GXLogging.Error(log, $"Validation of antiforgery failed", ex);
				}
				else
				{
					httpStatusCode = HttpStatusCode.InternalServerError;
					GXLogging.Error(log, $"Internal error", ex);
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
				if (!string.IsNullOrEmpty(httpReasonPhrase))
				{
					IHttpResponseFeature responseReason = httpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>();
					if (responseReason!=null)
						responseReason.ReasonPhrase = httpReasonPhrase;
				}
			}
			await Task.CompletedTask;
		}
	}
	internal class EnableCustomSessionStoreMiddleware
	{
		private readonly RequestDelegate _next;

		public EnableCustomSessionStoreMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context, IDistributedCache distributedCache)
		{
			await _next(context);
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
			if (!string.IsNullOrEmpty(Startup.DefaultFileName))
			{
				return Redirect(Url.Content($"~/{Startup.DefaultFileName}"));
			}
			return NotFound();
		}
	}
	internal class SetRoutePrefix : IApplicationModelConvention
	{
		private readonly AttributeRouteModel _routePrefix ;
		public SetRoutePrefix(IRouteTemplateProvider route)
		{
			_routePrefix = new AttributeRouteModel(route);
		}
		public void Apply(ApplicationModel application)
		{
			foreach (var controller in application.Controllers)
			{
				foreach (var selector in controller.Selectors)
				{
					if (selector.AttributeRouteModel != null)
					{
						selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, selector.AttributeRouteModel);
					}
					else
					{
						selector.AttributeRouteModel = _routePrefix;
					}
				}
			}
		}
	}

	
	internal class HomeControllerConvention : IApplicationModelConvention
	{
		private static bool FindAndStoreDefaultFile()
		{
			string[] defaultFiles = { "default.htm", "default.html", "index.htm", "index.html" };
			foreach (string file in defaultFiles)
			{
				string filePath = Path.Combine(Startup.LocalPath, file);
				if (File.Exists(filePath))
				{
					Startup.DefaultFileName = file;
					return true;
				}
			}
			Startup.DefaultFileName = null;
			return false;
		}

		public void Apply(ApplicationModel application)
		{
			var homeController = application.Controllers.FirstOrDefault(c => c.ControllerType == typeof(HomeController));
			if (homeController != null)
			{
				if (!FindAndStoreDefaultFile())
				{
					application.Controllers.Remove(homeController);
				}
			}
		}
	}
	public static class SesssionAsyncExtensions
	{
		/// <summary>
		/// Ensures sessions load asynchronously by calling LoadAsync before accessing session data,
		/// forcing the session provider to avoid synchronous operations.
		/// </summary>
		/// <remarks>
		/// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-5.0
		/// The default session provider in ASP.NET Core will only load the session record from the underlying IDistributedCache store asynchronously if the
		/// ISession.LoadAsync method is explicitly called before calling the TryGetValue, Set or Remove methods. 
		/// Failure to call LoadAsync first will result in the underlying session record being loaded synchronously,
		/// which could potentially impact the ability of an application to scale.
		/// 
		/// See also:
		/// https://github.com/aspnet/Session/blob/master/src/Microsoft.AspNetCore.Session/DistributedSession.cs
		/// https://github.com/dotnet/AspNetCore.Docs/issues/1840#issuecomment-454182594
		/// </remarks>
		public static IApplicationBuilder UseAsyncSession(this IApplicationBuilder app)
		{
			app.UseSession();
			app.Use(async (context, next) =>
			{
				if (context.Session != null)
				{
					await context.Session.LoadAsync();
				}
				await next();
			});
			return app;
		}
	}
	public class MimeMappingsOptions : Dictionary<string, string> { }
}
