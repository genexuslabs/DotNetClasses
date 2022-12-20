using System;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;

namespace GeneXus.Otel.AWS
{
	public class AWSOtelAspNetProvider : IOpenTelemetryProvider
	{	
		public bool InstrumentAspNetCoreApplication(IServiceCollection _)
		{
			Sdk.CreateTracerProviderBuilder()
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