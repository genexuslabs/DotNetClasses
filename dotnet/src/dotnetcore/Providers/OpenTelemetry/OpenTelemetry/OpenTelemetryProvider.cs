using GeneXus.Services;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace GeneXus.OpenTelemetry.OpenTelemetry
{
	public class OpenTelemetryProvider : IOpenTelemetryProvider
	{
		public OpenTelemetryProvider(GXService s)
		{
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection services)
		{
			services.AddOpenTelemetryTracing(tracerProviderBuilder =>
			{
				tracerProviderBuilder
				.AddOtlpExporter()
			   .SetErrorStatusOnException(true)
			   .AddHttpClientInstrumentation()
			   .AddAspNetCoreInstrumentation(opt =>
			   {
				   opt.RecordException = true;
			   })
			   .AddSqlClientInstrumentation()
			   .Build();
			});

			return true;
		}
	}
}