using System;
using GeneXus.Configuration;
using Microsoft.Extensions.Logging;

namespace GeneXus.Services.Log
{
	public static class GXLogService
	{
		private static string AZURE_APPLICATION_INSIGHTS_LOG = "AZUREAPPLICATIONINSIGHTS";
		public static ILoggerFactory GetLogFactory()
		{		
			Config.GetValueOf("LOG_OUTPUT", out string logProvider);		
			if (logProvider == AZURE_APPLICATION_INSIGHTS_LOG)
				return GeneXus.Services.Log.AzureAppInsightsLogProvider.GetLoggerFactory();
			else
				return null;
		}
	}
}
