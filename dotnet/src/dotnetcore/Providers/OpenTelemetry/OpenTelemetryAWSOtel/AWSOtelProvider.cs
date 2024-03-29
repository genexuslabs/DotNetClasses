using System;
using GeneXus.Services;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace GeneXus.OpenTelemetry.AWS
{
	public class AWSOtelProvider : IOpenTelemetryProvider
	{
		public AWSOtelProvider(GXService s)
		{
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection _)
		{
			string oltpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

			Sdk.CreateTracerProviderBuilder()
				.AddXRayTraceId()    // for generating AWS X-Ray compliant trace IDs
				.AddAWSInstrumentation()
				.AddOtlpExporter(options =>
				{
					if (!string.IsNullOrEmpty(oltpEndpoint))
					{
						options.Endpoint = new Uri(oltpEndpoint);
					}
				})
				.AddGxAspNetInstrumentation()
				.Build();

			Sdk.CreateMeterProviderBuilder()
				.AddGxMeterAspNetInstrumentation()
				.Build();

			Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());

			return true;
		}
	}
}