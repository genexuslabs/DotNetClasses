using System;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace GeneXus.Services.Log
{
	public class AzureAppInsightsLogProvider : ILoggerFactory
	{
		private static string APPLICATIONINSIGHTS_CONNECTION_STRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";
		private static string OPENTELEMETRY_SERVICE = "Observability";
		public static ILoggerFactory loggerFactory;
		
		public static ILoggerFactory GetLoggerFactory()
		{
			string appInsightsConnection = Environment.GetEnvironmentVariable(APPLICATIONINSIGHTS_CONNECTION_STRING);
			try
			{

				if (appInsightsConnection != null)
				{
					GXService providerService = GXServices.Instance?.Get(OPENTELEMETRY_SERVICE);
					if (providerService != null && providerService.Name.Equals("AZUREMONITOR"))
					{
						loggerFactory = LoggerFactory.Create(builder =>
						{
							builder.AddOpenTelemetry(options =>
							{
								options.AddAzureMonitorLogExporter(o => o.ConnectionString = appInsightsConnection);
								options.AddConsoleExporter();
							});
						});
					}
					else
					{
						loggerFactory = LoggerFactory.Create(builder => builder.AddApplicationInsights(

							configureTelemetryConfiguration: (config) =>
							config.ConnectionString = appInsightsConnection,
							configureApplicationInsightsLoggerOptions: (options) => { }
							)
						);
					}
				}
				else
				{
					throw new ArgumentNullException("APPLICATIONINSIGHTS_CONNECTION_STRING", "Application Insight Log could not be initialized due to missing APPLICATIONINSIGHTS_CONNECTION_STRING environment variable.");
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