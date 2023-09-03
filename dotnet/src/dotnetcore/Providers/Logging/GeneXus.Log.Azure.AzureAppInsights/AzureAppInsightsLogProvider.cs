using System;
using GeneXus.Services;
using GeneXus.Services.Log;
using Microsoft.Extensions.Logging;

namespace GeneXus.Log.Azure
{
	public class AzureAppInsightsLogProvider : IGXLogProvider
	{
		private static string APPLICATIONINSIGHTS_CONNECTION_STRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";

		public static ILoggerFactory loggerFactory;

		public AzureAppInsightsLogProvider(GXService s) { }
		public ILoggerFactory GetLoggerFactory()
		{
			string appInsightsConnection = Environment.GetEnvironmentVariable(APPLICATIONINSIGHTS_CONNECTION_STRING);
			if (appInsightsConnection != null) { 
			loggerFactory = LoggerFactory.Create(builder => builder.AddApplicationInsights(

				configureTelemetryConfiguration: (config) =>
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