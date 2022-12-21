using System;
using GeneXus.Services;
using OpenTelemetry;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;
using OpenTelemetry.Trace;

namespace GeneXus.OpenTelemetry.AWS
{
	public class AWSOtelAspNetProvider : IOpenTelemetryProvider
	{
		public bool InstrumentApplication()
		{
			TracerProvider tracerProvider = Sdk.CreateTracerProviderBuilder()
						.AddXRayTraceId()    // for generating AWS X-Ray compliant trace IDs
						.AddOtlpExporter(options =>
						{
							options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));
						})
						.AddAspNetCoreInstrumentation()
						.AddHttpClientInstrumentation()
						.AddAWSInstrumentation()
						.Build();

			Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());

			return true;
		}
	}
}