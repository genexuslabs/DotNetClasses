
using System;
using GeneXus.Services;
using GeneXus.Services.Common;
using GeneXus.Services.OpenTelemetry;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GeneXus.OpenTelemetry.Lightstep.AspNet
{
	public class LightStepOpenTelemetry : IOpenTelemetryProvider
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LightStepOpenTelemetry));
		private const string LIGHTSTEP_INGREST_URL = "ingest.lightstep.com:443";
		private const string LIGHTSTEP_ACCESS_TOKEN = "LS_ACCESS_TOKEN";
		private string accessToken;
		private string serviceName;
		private ServiceSettingsReader settingsReader;

		public LightStepOpenTelemetry(GXService s)
		{			
			settingsReader = new ServiceSettingsReader(String.Empty, s.Name, s);

			accessToken = settingsReader.GetPropertyValue("LIGHTSTEP_ACCESS_TOKEN", String.Empty);
			serviceName = settingsReader.GetPropertyValue("OPENTELEMETRY_SERVICE_NAME", String.Empty);

			string envVarValue = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
			if (!String.IsNullOrEmpty(envVarValue))
			{
				serviceName = envVarValue;
			}

			envVarValue = Environment.GetEnvironmentVariable(LIGHTSTEP_ACCESS_TOKEN);
			if (!String.IsNullOrEmpty(envVarValue))
			{
				accessToken = envVarValue;
			}

			if (string.IsNullOrEmpty(serviceName))
			{
				log.Warn("OpenTelemetry Lightstep was not initialized due to missing 'OTEL_SERVICE_NAME' Environment Variable");
			}

			if (string.IsNullOrEmpty(accessToken))
			{
				log.Warn("OpenTelemetry Lightstep was not initialized due to missing 'LS_ACCESS_TOKEN' Environment Variable");
			}
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection services)
		{
			if (String.IsNullOrEmpty(accessToken) || String.IsNullOrEmpty(serviceName))
			{
				log.Warn("OpenTelemetry Lightstep was not initialized due to missing configuration");
				return false;
			}
			string serviceVersion = "1.0.0";
			//TODO: Read dinamically.

			services.AddOpenTelemetryTracing(tracerProviderBuilder =>
			 {
				 tracerProviderBuilder
				 .AddOtlpExporter(opt =>
				 {
					 opt.Endpoint = new Uri(LIGHTSTEP_INGREST_URL);
					 opt.Headers = $"lightstep-access-token=${accessToken}";					 
				 })				 
				 .AddSource(serviceName)
				 .SetResourceBuilder(
					 ResourceBuilder.CreateDefault()
						 .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
				 .AddHttpClientInstrumentation()
				 .AddAspNetCoreInstrumentation()
				 .AddSqlClientInstrumentation();
			 });

			return true;
		}
	}
}
