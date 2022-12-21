
using System;
using GeneXus.Services.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using log4net;
using GeneXus.Services;
using GeneXus.Services.Common;
using System.Net.Http;

namespace GeneXus.OpenTelemetry.Lightstep.AspNet
{
	public class LightStepOpenTelemetry : IOpenTelemetryProvider
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LightStepOpenTelemetry));
		private const string LIGHTSTEP_INGREST_URL = "ingest.lightstep.com:443";
		private const string LIGHTSTEP_ACCESS_TOKEN = "LS_ACCESS_TOKEN";
		private string AccessToken;
		private string ServiceName;
		private ServiceSettingsReader settingsReader;

		public LightStepOpenTelemetry(GXService s)
		{			
			settingsReader = new ServiceSettingsReader(String.Empty, s.Name, s);

			AccessToken = settingsReader.GetPropertyValue("LIGHTSTEP_ACCESS_TOKEN", String.Empty);
			ServiceName = settingsReader.GetPropertyValue("OPENTELEMETRY_SERVICE_NAME", String.Empty);

			string envVarValue = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME");
			if (!String.IsNullOrEmpty(envVarValue))
			{
				ServiceName = envVarValue;
			}

			envVarValue = Environment.GetEnvironmentVariable(LIGHTSTEP_ACCESS_TOKEN);
			if (!String.IsNullOrEmpty(envVarValue))
			{
				AccessToken = envVarValue;
			}

			if (string.IsNullOrEmpty(ServiceName))
			{
				log.Warn("OpenTelemetry Lightstep was not initialized due to missing 'OTEL_SERVICE_NAME' Environment Variable");
			}

			if (string.IsNullOrEmpty(AccessToken))
			{
				log.Warn("OpenTelemetry Lightstep was not initialized due to missing 'LS_ACCESS_TOKEN' Environment Variable");
			}
		}

		public bool InstrumentAspNetCoreApplication(IServiceCollection services)
		{
			if (String.IsNullOrEmpty(AccessToken) || String.IsNullOrEmpty(ServiceName))
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
					 opt.Headers = $"lightstep-access-token=${AccessToken}";					 
				 })				 
				 .AddSource(ServiceName)
				 .SetResourceBuilder(
					 ResourceBuilder.CreateDefault()
						 .AddService(serviceName: ServiceName, serviceVersion: serviceVersion))
				 .AddHttpClientInstrumentation()
				 .AddAspNetCoreInstrumentation()
				 .AddSqlClientInstrumentation();
			 });

			return true;
		}
	}
}
