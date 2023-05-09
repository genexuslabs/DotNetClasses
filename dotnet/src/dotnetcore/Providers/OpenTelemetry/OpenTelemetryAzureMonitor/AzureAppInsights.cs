using System;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using GeneXus.Services;
using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;
using log4net;

namespace GeneXus.OpenTelemetry.Azure
{
	public class AzureAppInsights : IOpenTelemetryProvider
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AzureAppInsights));
		private const string AZURE_OTEL_EXPORTER_CONNECTIONSTRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";
		private const string ACTIVITY_SOURCE = "OTel.AzureMonitor.GXApp";
		private const string ACTIVITY_NAME = "GeneXusApplicationActivity";

		public AzureAppInsights(GXService s)
		{
		}

		private static readonly ActivitySource GXActivitySource = new ActivitySource(ACTIVITY_SOURCE);

		public bool InstrumentAspNetCoreApplication(IServiceCollection _)
		{
			string oltpEndpoint = Environment.GetEnvironmentVariable(AZURE_OTEL_EXPORTER_CONNECTIONSTRING);
		
			if (!string.IsNullOrEmpty(oltpEndpoint))
			{ 
				using var tracerProvider = Sdk.CreateTracerProviderBuilder()
					.AddAzureMonitorTraceExporter(o =>
					{
						o.ConnectionString = oltpEndpoint;
					})
					.AddSource(ACTIVITY_SOURCE)
					.AddGxAspNetInstrumentation()
					.Build();

				using (var activity = GXActivitySource.StartActivity(ACTIVITY_NAME))
				{
				}
				return true;
			}
			else
			{ 
				log.Warn("OpenTelemetry Azure Monitor was not initialized due to missing 'AZURE_OTEL_EXPORTER_CONNECTIONSTRING' Environment Variable");
				return false;
			}
		}
	}
}