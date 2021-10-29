using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using GeneXus.Cache;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;
using GeneXus.Services;
using GxClasses.Web;
using GxClasses.Web.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace GeneXus.Deploy.AzureFunctions.Handlers
{
	public class Program
    {
		static async Task Main()
        {

			string roothPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string routePrefix = GetRoutePrefix();
			GXRouting.ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
				.ConfigureServices(services =>
				{
					services.AddSingleton<ICallMappings, CallMappings>(x => new CallMappings(roothPath));
				})
				.ConfigureServices(services =>
				{
					services.AddSingleton<IGXRouting, GXRouting>(x => new GXRouting(routePrefix));
				})
				.ConfigureServices(services =>
				{
					ISessionService sessionService = GXSessionServiceFactory.GetProvider();
					if (sessionService is GxRedisSession)
					{ 
						services.AddSingleton<ICacheService2>(x => new Redis(sessionService.ConnectionString, sessionService.SessionTimeout));
					}
					else
						services.AddSingleton<ICacheService2>(x => new InProcessCache());
				})
				.Build();

            await host.RunAsync();
        }
		private static string GetRoutePrefix()
		{
			//Read host.json file to get Route prefix
			string ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string hostFile = "host.json";
			string hostFilePath = Path.Combine(ContentRootPath, hostFile);

			//Default Azure value
			string routePrefix = "api";

			if (File.Exists(hostFilePath))
			{
				string hostFileContent = File.ReadAllText(hostFilePath);
				using (JsonDocument doc = JsonDocument.Parse(hostFileContent))
				{
					JsonElement root = doc.RootElement;
					if (root.TryGetProperty("extensions", out JsonElement extensionsElement))
					{
						if (extensionsElement.TryGetProperty("http", out JsonElement httpElement))
						{
							if (httpElement.TryGetProperty("routePrefix", out JsonElement routeElement))
								routePrefix = routeElement.GetString();
						}
					}
				}
			}
			return routePrefix;
		}
	}
}