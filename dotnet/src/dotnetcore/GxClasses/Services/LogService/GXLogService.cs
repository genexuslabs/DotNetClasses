using System;
using GeneXus.Configuration;
using Microsoft.Extensions.Logging;

namespace GeneXus.Services.Log
{
	public static class GXLogService
	{
		private static string AZURE_APPLICATION_INSIGHTS_LOG = "AZUREAPPLICATIONINSIGHTS";
		private const string LOG_OUTPUT_ENVVAR = "GX_LOG_OUTPUT";
		public static ILoggerFactory GetLogFactory()
		{
			string logoutput = Environment.GetEnvironmentVariable(LOG_OUTPUT_ENVVAR);
			if (logoutput == null) { 

				Config.GetValueOf("LOG_OUTPUT", out string logProvider);
				logoutput = logProvider;
			}
			if (logoutput == AZURE_APPLICATION_INSIGHTS_LOG)
				return GeneXus.Services.Log.AzureAppInsightsLogProvider.GetLoggerFactory();
			else
				return null;
		}
	}
}
