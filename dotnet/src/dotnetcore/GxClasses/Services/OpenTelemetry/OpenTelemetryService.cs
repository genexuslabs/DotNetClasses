using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GxClasses.Helpers;
using log4net;

namespace GeneXus.Services.OpenTelemetry
{
	internal class OpenTelemetryService
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(OpenTelemetryService));

		public static string OPENTELEMETRY_SERVICE = "OpenTelemetry";

		public interface IOpenTelemetryProvider
		{
			bool InstrumentApplication();
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

		internal static void Setup()
		{
			IOpenTelemetryProvider provider = OpenTelemetryService.GetOpenTelemetryProvider();
			if (provider == null)
			{
				provider.InstrumentApplication();
			}
		}
	}


}
