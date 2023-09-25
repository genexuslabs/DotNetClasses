using System;
using Azure.Identity;
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
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<AzureAppInsights>();
		private const string APPLICATIONINSIGHTS_CONNECTION_STRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";

		public AzureAppInsights(GXService s)
		{
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection _)
		{
			string oltpEndpoint = Environment.GetEnvironmentVariable(APPLICATIONINSIGHTS_CONNECTION_STRING);
			try
			{
				var resourceBuilder = ResourceBuilder.CreateDefault()
					.AddTelemetrySdk();

				Sdk.CreateTracerProviderBuilder()
				.SetResourceBuilder(resourceBuilder)
				.AddAzureMonitorTraceExporter(o =>
				{
					if (!string.IsNullOrEmpty(oltpEndpoint))
						o.ConnectionString = oltpEndpoint;
					else
					{
						o.Credential = new DefaultAzureCredential();
						log.Debug("Connect to Azure monitor Opentelemetry Trace exporter using Default Azure credential");
					}
				})
				.AddGxAspNetInstrumentation()
				.Build();

				Sdk.CreateMeterProviderBuilder()
				.SetResourceBuilder(resourceBuilder)
				.AddAzureMonitorMetricExporter(o =>
				{
					if (!string.IsNullOrEmpty(oltpEndpoint))
						o.ConnectionString = oltpEndpoint;
					else
					{
						o.Credential = new DefaultAzureCredential();
						log.Debug("Connect to Azure monitor Opentelemetry Metrics exporter using Default Azure credential");
					}
				})
				.Build();
				return true;
			}

			else
			{
				GXLogging.Warn(log, "OpenTelemetry Azure Monitor was not initialized due to missing 'APPLICATIONINSIGHTS_CONNECTION_STRING' Environment Variable");
				return false;
			}
		}
	}
}