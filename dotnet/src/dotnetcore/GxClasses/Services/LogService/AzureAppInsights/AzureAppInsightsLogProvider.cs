using System;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace GeneXus.Services.Log
{
	public class AzureAppInsightsLogProvider : ILoggerFactory
	{
		private static string APPLICATIONINSIGHTS_CONNECTION_STRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";
		private const string LOG_LEVEL_ENVVAR = "GX_LOG_LEVEL";
		public static ILoggerFactory loggerFactory;

		public static ILoggerFactory GetAzureMonitorLoggerFactory()
		{
			string appInsightsConnection = Environment.GetEnvironmentVariable(APPLICATIONINSIGHTS_CONNECTION_STRING);
			try
			{

				if (appInsightsConnection != null)
				{
					string loglevelvalue = Environment.GetEnvironmentVariable(LOG_LEVEL_ENVVAR);
					LogLevel loglevel = LogLevel.Information;
					if (!string.IsNullOrEmpty(loglevelvalue))
					{
						Enum.TryParse<LogLevel>(loglevelvalue, out loglevel);
					}
					loggerFactory = LoggerFactory.Create(builder =>
					{
						builder.AddOpenTelemetry(options =>
						{
							options.AddAzureMonitorLogExporter(o => o.ConnectionString = appInsightsConnection);
							options.AddConsoleExporter();
						}).SetMinimumLevel(loglevel);
					});
				}
				else
				{
					throw new ArgumentNullException(APPLICATIONINSIGHTS_CONNECTION_STRING, "Opentelemetry Provider is Azure Monitor. Application Insight Log could not be initialized due to missing APPLICATIONINSIGHTS_CONNECTION_STRING environment variable.");
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return loggerFactory;
		}

		public static ILoggerFactory GetLoggerFactory()
		{
			string appInsightsConnection = Environment.GetEnvironmentVariable(APPLICATIONINSIGHTS_CONNECTION_STRING);
			try
			{

				if (appInsightsConnection != null)
				{
					string loglevelvalue = Environment.GetEnvironmentVariable(LOG_LEVEL_ENVVAR);
					LogLevel loglevel = LogLevel.Information;
					if (!string.IsNullOrEmpty(loglevelvalue))
					{
						Enum.TryParse<LogLevel>(loglevelvalue, out loglevel);
					}
					loggerFactory = LoggerFactory.Create(builder => builder.AddApplicationInsights(

						configureTelemetryConfiguration: (config) =>
						config.ConnectionString = appInsightsConnection,
						configureApplicationInsightsLoggerOptions: (options) => { }
						).SetMinimumLevel(loglevel)
					);
				}
				else
				{
					throw new ArgumentNullException(APPLICATIONINSIGHTS_CONNECTION_STRING, "LogOutput is Application Insights. Application Insight Log could not be initialized due to missing APPLICATIONINSIGHTS_CONNECTION_STRING environment variable.");
				}
			}
			catch (Exception ex)
			{
				throw ex;
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