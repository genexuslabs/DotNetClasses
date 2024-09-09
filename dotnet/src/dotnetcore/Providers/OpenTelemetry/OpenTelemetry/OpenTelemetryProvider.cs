using GeneXus.Services;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System;
using Microsoft.IdentityModel.Tokens;

namespace GeneXus.OpenTelemetry.OpenTelemetry
{
	public class OpenTelemetryProvider : IOpenTelemetryProvider
	{
		const string OTEL_METRICS_EXPORTER = "OTEL_METRICS_EXPORTER";
		const string OTEL_TRACES_EXPORTER = "OTEL_TRACES_EXPORTER";
		public OpenTelemetryProvider(GXService s)
		{
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection services)
		{
			string envvar = Environment.GetEnvironmentVariable(OTEL_TRACES_EXPORTER);
			if (envvar.IsNullOrEmpty() || !envvar.ToLower().Equals("none")) {
				services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
				{
					tracerProviderBuilder
					.AddOtlpExporter()
					.AddGxAspNetInstrumentation();
				});
			}

			envvar = Environment.GetEnvironmentVariable(OTEL_METRICS_EXPORTER);
			if (envvar.IsNullOrEmpty() || !envvar.ToLower().Equals("none")) {
				services.AddOpenTelemetry().WithMetrics(metricsProviderBuilder =>
				{
					metricsProviderBuilder
					.AddGxMeterAspNetInstrumentation();
				});
			}
			return true;
		}
	}
}