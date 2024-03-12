using System;
using System.Text.RegularExpressions;
using GxClasses.Helpers;

namespace GeneXus.Services.OpenTelemetry
{
	public interface IOpenTelemetryProvider
	{
		bool InstrumentAspNetCoreApplication(Microsoft.Extensions.DependencyInjection.IServiceCollection services);
	}

	public static class OpenTelemetryService
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(OpenTelemetryService).FullName);

		const string OTEL_RESOURCE_ATTRIBUTES = "OTEL_RESOURCE_ATTRIBUTES";
		const string OTEL_SERVICE_NAME = "OTEL_SERVICE_NAME";
		const string OTEL_SERVICE_VERSION = "OTEL_SERVICE_VERSION";
		const string OPENTELEMETRY_SERVICE = "Observability";

		public static string GX_ACTIVITY_SOURCE_NAME= GetServiceNameValue(OTEL_RESOURCE_ATTRIBUTES);
		public static string GX_ACTIVITY_SOURCE_VERSION= GetServiceVersionValue(OTEL_RESOURCE_ATTRIBUTES);
	
		private static string GetServiceNameValue(string input)
		{
			string otelServiceNameEnvVar = Environment.GetEnvironmentVariable(OTEL_SERVICE_NAME);
			if (!string.IsNullOrEmpty(otelServiceNameEnvVar))
				return otelServiceNameEnvVar;

			string pattern = @"(?:\b\w+\b=\w+)(?:,(?:\b\w+\b=\w+))*";
			MatchCollection matches = Regex.Matches(input, pattern);

			foreach (Match match in matches)
			{
				string[] keyValue = match.Value.Split('=');

				if (keyValue[0] == "service.name")
				{
					return keyValue[1];
				}
			}
			return "GeneXus.Tracing";
		}
		private static string GetServiceVersionValue(string input)
		{
			string otelServiceNameEnvVar = Environment.GetEnvironmentVariable(OTEL_SERVICE_VERSION);
			if (!string.IsNullOrEmpty(otelServiceNameEnvVar))
				return otelServiceNameEnvVar;

			string pattern = @"(?:\b\w+\b=\w+)(?:,(?:\b\w+\b=\w+))*";
			MatchCollection matches = Regex.Matches(input, pattern);

			foreach (Match match in matches)
			{
				string[] keyValue = match.Value.Split('=');

				if (keyValue[0] == "service.version")
				{
					return keyValue[1];
				}
			}
			return string.Empty;
		}
		private static IOpenTelemetryProvider GetOpenTelemetryProvider()
		{
			IOpenTelemetryProvider otelProvider = null;
			GXService providerService = GXServices.Instance?.Get(OPENTELEMETRY_SERVICE);

			if (providerService != null)
			{				
				try
				{
					GXLogging.Debug(log, "Loading OpenTelemetry provider:", providerService.ClassName);
#if !NETCORE
					Type type = Type.GetType(providerService.ClassName, true, true);
#else
					Type type = AssemblyLoader.GetType(providerService.ClassName);
#endif
					otelProvider = (IOpenTelemetryProvider)Activator.CreateInstance(type, new object[] { providerService });
				}
				catch (Exception e)
				{
					GXLogging.Error(log, "Couldn't create OpenTelemetry provider.", e.Message, e);
					throw e;
				}
			}
			return otelProvider;
		}

		internal static void Setup(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
		{			
			IOpenTelemetryProvider provider = GetOpenTelemetryProvider();
			if (provider != null)
			{
				bool started = provider.InstrumentAspNetCoreApplication(services);
				if (started)
				{
					GXLogging.Info(log, "OpenTelemetry instrumentation started");
				}
			}
		}
	}


}
