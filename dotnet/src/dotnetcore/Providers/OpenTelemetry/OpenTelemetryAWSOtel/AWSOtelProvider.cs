using System;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Contrib.Extensions.AWSXRay.Trace;
using GeneXus.Services;
using GeneXus.Services.Common;

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
				.AddOtlpExporter(options =>
				{
					if (!string.IsNullOrEmpty(oltpEndpoint))
					{
						options.Endpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));
					}
				})
				.AddHttpClientInstrumentation()
				.AddAspNetCoreInstrumentation()
				.AddSqlClientInstrumentation()
				.AddAWSInstrumentation()
				.Build();

			Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());

			return true;
		}
	}
}