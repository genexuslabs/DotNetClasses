using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace GeneXus.OpenTelemetry
{
	public static class GXMeterProviderBuilder
	{
		public static MeterProviderBuilder AddGxMeterAspNetInstrumentation(this MeterProviderBuilder meter)
		{
			string envvar = Environment.GetEnvironmentVariable("OTEL_METRICS_EXPORTER");
			meter
				.AddAspNetCoreInstrumentation()
				.AddHttpClientInstrumentation()
				.AddRuntimeInstrumentation()
				.AddOtlpExporter();
			if (envvar != null && envvar.Contains("console"))
				meter.AddConsoleExporter();
			return meter;
		}

	}
}
