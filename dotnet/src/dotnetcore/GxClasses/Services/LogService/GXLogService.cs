using System;
using GeneXus.Configuration;
using Microsoft.Extensions.Logging;

namespace GeneXus.Services.Log
{
	public static class GXLogService
	{
		private static string AZURE_APPLICATION_INSIGHTS_LOG = "AZUREAPPLICATIONINSIGHTS";
		const string OTEL_AZUREMONITOR_EXPORTER = "OTEL_AZUREMONITOR_EXPORTER";
		const string OPENTELEMETRY = "OPENTELEMETRY";
		internal static string OTEL_LOGS_EXPORTER = "OTEL_LOGS_EXPORTER";
		const string LOG_OUTPUT = "LOG_OUTPUT";

		public static ILoggerFactory GetLogFactory()
		{
			string otelLogEnvVar = Environment.GetEnvironmentVariable(OTEL_LOGS_EXPORTER);
			if (string.IsNullOrEmpty(otelLogEnvVar) || !otelLogEnvVar.ToLower().Equals("none"))
			{ 
				if (Config.GetValueOf(LOG_OUTPUT, out string logProvider))
				{
					if (logProvider == OTEL_AZUREMONITOR_EXPORTER)
						return OpentelemetryLogProvider.GetAzureMonitorLoggerFactory();
					else if (logProvider == AZURE_APPLICATION_INSIGHTS_LOG)
						return OpentelemetryLogProvider.GetAzureAppInsightsLoggerFactory();
					else
						if (logProvider == OPENTELEMETRY)
						return OpentelemetryLogProvider.GetOpentelemetryLoggerFactory();
				}
			}
			return null;
		}
	}
}
