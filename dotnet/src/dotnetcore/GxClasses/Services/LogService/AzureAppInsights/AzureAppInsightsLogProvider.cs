using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace GeneXus.Services.Log
{
	public class AzureAppInsightsLogProvider : ILoggerFactory
	{
		private static string APPLICATIONINSIGHTS_CONNECTION_STRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";

		public static ILoggerFactory loggerFactory;
		
		public static ILoggerFactory GetLoggerFactory()
		{
			string appInsightsConnection = Environment.GetEnvironmentVariable(APPLICATIONINSIGHTS_CONNECTION_STRING);
			if (appInsightsConnection != null) { 
			loggerFactory = LoggerFactory.Create(builder => builder.AddApplicationInsights(

				configureTelemetryConfiguration: (config) =>
				//config.SetAzureTokenCredential
				config.ConnectionString = appInsightsConnection,
				configureApplicationInsightsLoggerOptions: (options) => { }
				)
			);
				
			}
			else
			{
				throw new ArgumentNullException("APPLICATIONINSIGHTS_CONNECTION_STRING","Application Insight Log could not be initialized due to missing APPLICATIONINSIGHTS_CONNECTION_STRING environment variable.");
			}
			return loggerFactory;
		}

		public void AddProvider(ILoggerProvider provider)
		{
			loggerFactory.AddProvider(provider);
		}

		public void Dispose()
		{
			loggerFactory.Dispose();
		}

		public ILogger CreateLogger(string name)
		{
			return loggerFactory.CreateLogger(name);
		}

	}
}