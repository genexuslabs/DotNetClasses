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
		public static ILoggerFactory GetLogFactory()
		{		
			Config.GetValueOf("LOG_OUTPUT", out string logProvider);
			GXService providerService = GXServices.Instance?.Get(OPENTELEMETRY_SERVICE);
			if ((logProvider == AZURE_APPLICATION_INSIGHTS_LOG) || (providerService != null && providerService.Name.Equals("AZUREMONITOR")))
				return GeneXus.Services.Log.AzureAppInsightsLogProvider.GetLoggerFactory();
			else
				return null;
		}
	}
}
