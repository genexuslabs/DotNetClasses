using GeneXus.Services;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace GeneXus.OpenTelemetry.OpenTelemetry
{
	public class OpenTelemetryProvider : IOpenTelemetryProvider
	{
		public OpenTelemetryProvider(GXService s)
		{
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection services)
		{
			services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
			{
				tracerProviderBuilder
				.AddOtlpExporter()
				.AddGxAspNetInstrumentation();
			});
			services.AddOpenTelemetry().WithMetrics(metricsProviderBuilder =>
			{
				metricsProviderBuilder
				.AddOtlpExporter()
				.AddGxMeterAspNetInstrumentation();
			});
			return true;
		}
	}
}