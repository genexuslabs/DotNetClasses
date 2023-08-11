using System;
using Azure.Monitor.OpenTelemetry.Exporter;
using GeneXus.Services;
using GeneXus.Services.OpenTelemetry;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
				var resourceBuilder = ResourceBuilder.CreateDefault()
					.AddTelemetrySdk();

				Sdk.CreateTracerProviderBuilder()
				.SetResourceBuilder(resourceBuilder)
				.AddAzureMonitorTraceExporter(o => o.ConnectionString = oltpEndpoint)
				.AddGxAspNetInstrumentation()
				.Build();

				Sdk.CreateMeterProviderBuilder()
				.SetResourceBuilder(resourceBuilder)
				.AddAzureMonitorMetricExporter(o => o.ConnectionString = oltpEndpoint)
				.Build();

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