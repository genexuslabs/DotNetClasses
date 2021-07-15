using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Configuration;
using System.Reflection;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using GeneXus.Deploy.AzureFunctions.Handlers.Helpers;

namespace GeneXus.Deploy.AzureFunctions.Handlers
{
    public class Program
    {
		static async Task Main()
        {
			string roothPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
				.ConfigureServices(services =>
				{
					services.AddSingleton<ICallMappings, CallMappings>(x => new CallMappings(roothPath));
				})
				.Build();

            await host.RunAsync();
        }
    }
}