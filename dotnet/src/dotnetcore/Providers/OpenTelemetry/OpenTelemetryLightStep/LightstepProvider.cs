
using System;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using log4net;
using GeneXus.Services;

namespace GeneXus.OpenTelemetry.Lightstep
{

	public class LightStepOpenTelemetry : IOpenTelemetryProvider
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LightStepOpenTelemetry));
		private const string LIGHTSTEP_INGREST_URL = "ingest.lightstep.com:443";
		private const string LIGHTSTEP_ACCESS_TOKEN = "LS_ACCESS_TOKEN";

		public LightStepOpenTelemetry(GXService s)
		{
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection services)
		{
			string lightstepToken = Environment.GetEnvironmentVariable(LIGHTSTEP_ACCESS_TOKEN);

			if (string.IsNullOrEmpty(lightstepToken))
			{
				log.Warn("OpenTelemetry Lightstep was not initialized due to missing 'LS_ACCESS_TOKEN' Environment Variable");
				return false;
			}

			services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
			{
				 tracerProviderBuilder
				 .AddOtlpExporter(opt =>
				 {
					 opt.Endpoint = new Uri(LIGHTSTEP_INGREST_URL);
					 opt.Headers = $"lightstep-access-token=${lightstepToken}";
				 })
				.AddGxAspNetInstrumentation();	
			 });

			return true;
		}
	}
}
