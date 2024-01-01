using System;
using System.Collections.Generic;
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
				.AddConsoleExporter()
				.AddAspNetCoreInstrumentation()
				.AddHttpClientInstrumentation()
				.AddOtlpExporter();
			return meter;
		}

	}
}
