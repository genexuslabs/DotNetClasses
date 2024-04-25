using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

namespace GeneXus.Services.Log
{
	public class OpentelemetryLogProvider : ILoggerFactory
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
					LogLevel loglevel = GetLogLevel();
					loggerFactory = LoggerFactory.Create(builder =>
					{
						builder.AddOpenTelemetry(options =>
						{
							options.AddAzureMonitorLogExporter(o => o.ConnectionString = appInsightsConnection);
							if (GenerateOtelLogsToConsole())
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

		public static ILoggerFactory GetAzureAppInsightsLoggerFactory()
		{
			string appInsightsConnection = Environment.GetEnvironmentVariable(APPLICATIONINSIGHTS_CONNECTION_STRING);
			try
			{

				if (appInsightsConnection != null)
				{
					LogLevel loglevel = GetLogLevel();
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

		public static ILoggerFactory GetOpentelemetryLoggerFactory()
		{
			LogLevel loglevel = GetLogLevel();
			loggerFactory = LoggerFactory.Create(builder => builder.AddOpenTelemetry(logging =>
			{
				var resourceBuilder = ResourceBuilder.CreateDefault()
				.AddTelemetrySdk();

				logging.SetResourceBuilder(resourceBuilder)
			   .AddOtlpExporter();

				if (GenerateOtelLogsToConsole())
					logging.AddConsoleExporter();
				
			})
			.SetMinimumLevel(loglevel));
			return loggerFactory;
		}
		private static LogLevel GetLogLevel()
		{
			string loglevelvalue = Environment.GetEnvironmentVariable(LOG_LEVEL_ENVVAR);
			LogLevel loglevel = LogLevel.Information;
			if (!string.IsNullOrEmpty(loglevelvalue))
			{
				if (!Enum.TryParse<LogLevel>(loglevelvalue, out loglevel))
				{
					CustomLogLevel customLogLevel = CustomLogLevel.info;
					if (Enum.TryParse<CustomLogLevel>(loglevelvalue, out customLogLevel))
					{
						loglevel = toLogLevel(customLogLevel);
					}
					else
						loglevel = LogLevel.Information;
				}
			}
			return loglevel;
		}

		private static bool GenerateOtelLogsToConsole()
		{
			string otelLogsEnvVar = Environment.GetEnvironmentVariable(GXLogService.OTEL_LOGS_EXPORTER);
			if (string.IsNullOrEmpty(otelLogsEnvVar)) { return false; }
			return otelLogsEnvVar.ToLower().Contains("console") || otelLogsEnvVar.ToLower().Contains("logging");
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
		private enum CustomLogLevel
		{
			none,
			all,
			debug,
			info,
			warn,
			error,
			fatal
		}
		private static LogLevel toLogLevel(CustomLogLevel customLogLevel)
		{
			switch (customLogLevel)
			{
				case CustomLogLevel.none: return LogLevel.None;
				case CustomLogLevel.all: return LogLevel.Trace;
				case CustomLogLevel.debug: return LogLevel.Debug;
				case CustomLogLevel.info: return LogLevel.Information;
				case CustomLogLevel.warn: return LogLevel.Warning;
				case CustomLogLevel.error: return LogLevel.Error;
				case CustomLogLevel.fatal: return LogLevel.Critical;
				default: return LogLevel.Information;
			}
		}
	}
}