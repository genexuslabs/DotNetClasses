using GeneXus.Configuration;
using Microsoft.Extensions.Logging;

namespace GeneXus.Services.Log
{
	public static class GXLogService
	{
		private static string AZURE_APPLICATION_INSIGHTS_LOG = "AZUREAPPLICATIONINSIGHTS";
		const string OTEL_AZUREMONITOR_EXPORTER = "OTEL_AZUREMONITOR_EXPORTER";
		const string LOG_OUTPUT = "LOG_OUTPUT";
		public static ILoggerFactory GetLogFactory()
		{
			if (Config.GetValueOf(LOG_OUTPUT, out string logProvider))
			{
				 if (logProvider == AZURE_APPLICATION_INSIGHTS_LOG)
					return AzureAppInsightsLogProvider.GetLoggerFactory();
				 else if (logProvider == OTEL_AZUREMONITOR_EXPORTER)
					AzureAppInsightsLogProvider.GetAzureMonitorLoggerFactory(); 
			}
			return null;
		}
	}
}
