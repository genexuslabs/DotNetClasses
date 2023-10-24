using System;
using GeneXus.Configuration;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.Logging;

namespace GeneXus.Services.Log
{
	public static class GXLogService
	{
		private static string AZURE_APPLICATION_INSIGHTS_LOG = "AZUREAPPLICATIONINSIGHTS";
		private static string OPENTELEMETRY_SERVICE = "Observability";
		const string AZUREMONITOR_PROVIDER = "AZUREMONITOR";
		public static ILoggerFactory GetLogFactory()
		{
			if (GXServices.LoadedServices)
			{
				GXService providerService = GXServices.Instance?.Get(OPENTELEMETRY_SERVICE);
				if (providerService != null && providerService.Name.Equals(AZUREMONITOR_PROVIDER))
				{
					return AzureAppInsightsLogProvider.GetAzureMonitorLoggerFactory();
				}
			}
			if (Config.GetValueOf("LOG_OUTPUT", out string logProvider) && logProvider == AZURE_APPLICATION_INSIGHTS_LOG)
			{
				return AzureAppInsightsLogProvider.GetLoggerFactory();
			}
			return null;
		}
	}
}
