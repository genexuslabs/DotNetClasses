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
			meter
				.AddAspNetCoreInstrumentation()
				.AddHttpClientInstrumentation()
				.AddRuntimeInstrumentation()
				.AddOtlpExporter();
			if (Environment.GetEnvironmentVariable("ENABLE_METRICS_CONSOLE_EXPORTER")?.ToLower() == "true")
				meter.AddConsoleExporter();
			return meter;
		}

	}
}
