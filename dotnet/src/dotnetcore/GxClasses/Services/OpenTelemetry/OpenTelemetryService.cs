using System;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Services.OpenTelemetry
{
	public interface IOpenTelemetryProvider
	{
		bool InstrumentAspNetCoreApplication(Microsoft.Extensions.DependencyInjection.IServiceCollection services);
	}

	internal static class OpenTelemetryService
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(OpenTelemetryService));
		private static string OPENTELEMETRY_SERVICE = "OpenTelemetry";
		
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
					otelProvider = (IOpenTelemetryProvider)Activator.CreateInstance(type);
				}
				catch (Exception e)
				{
					GXLogging.Error(log, "Couldn´t create OpenTelemetry provider.", e.Message, e);
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
					log.Debug("OpenTelemetry instrumentation started");
				}
			}
		}
	}


}
