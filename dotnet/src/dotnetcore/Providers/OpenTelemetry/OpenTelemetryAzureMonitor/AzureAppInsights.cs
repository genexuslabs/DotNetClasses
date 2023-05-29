using System;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using GeneXus.Services;
using GeneXus.Services.OpenTelemetry;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GeneXus.OpenTelemetry.Azure
{
	public class AzureAppInsights : IOpenTelemetryProvider
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AzureAppInsights));
		private const string APPLICATIONINSIGHTS_CONNECTION_STRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";

		public AzureAppInsights(GXService s)
		{
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection services)
		{
			string oltpEndpoint = Environment.GetEnvironmentVariable(APPLICATIONINSIGHTS_CONNECTION_STRING);
		
			if (!string.IsNullOrEmpty(oltpEndpoint))
			{
				services.AddOpenTelemetry()
				.UseAzureMonitor( o =>
					{
						o.ConnectionString = oltpEndpoint;
					});

				return true;
			}
			else
			{ 
				log.Warn("OpenTelemetry Azure Monitor was not initialized due to missing 'APPLICATIONINSIGHTS_CONNECTION_STRING' Environment Variable");
				return false;
			}
		}
	}
}