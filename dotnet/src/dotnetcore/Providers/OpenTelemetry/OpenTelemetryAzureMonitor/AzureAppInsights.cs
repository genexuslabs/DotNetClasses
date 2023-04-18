using System;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using GeneXus.Services;
using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.Exporter;

namespace GeneXus.OpenTelemetry.Azure
{
	public class AzureAppInsights : IOpenTelemetryProvider
	{
		public AzureAppInsights(GXService s)
		{
		}

		private static readonly ActivitySource GXActivitySource = new ActivitySource(
		"OTel.AzureMonitor.GXApp");

		public bool InstrumentAspNetCoreApplication(IServiceCollection _)
		{
			string oltpEndpoint = Environment.GetEnvironmentVariable("AZURE_OTEL_EXPORTER_CONNECTIONSTRING");

			using var tracerProvider = Sdk.CreateTracerProviderBuilder()
				.AddAzureMonitorTraceExporter(o =>
				{
					o.ConnectionString = oltpEndpoint;
				})
				.AddGxAspNetInstrumentation()
				.Build();

			using (var activity = GXActivitySource.StartActivity("GXAppActivity", ActivityKind.Server))
			{
			}

			return true;
		}
	}
}