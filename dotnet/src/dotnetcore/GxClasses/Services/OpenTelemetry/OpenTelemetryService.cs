using System;
using GxClasses.Helpers;

namespace GeneXus.Services.OpenTelemetry
{
	public interface IOpenTelemetryProvider
	{
		bool InstrumentAspNetCoreApplication(Microsoft.Extensions.DependencyInjection.IServiceCollection services);
	}

	public static class OpenTelemetryService
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);

		private static string OPENTELEMETRY_SERVICE = "Observability";
		public static string GX_ACTIVITY_SOURCE_NAME = "GeneXus.Tracing";
		
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
					GXLogging.Info(log, "OpenTelemetry instrumentation started");
				}
			}
		}
	}


}
