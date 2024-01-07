using System;
using GeneXus.Services.OpenTelemetry;
using OpenTelemetry.Trace;

namespace GeneXus.OpenTelemetry
{
	public static class GxTraceProviderBuilder
	{
		public static TracerProviderBuilder AddGxAspNetInstrumentation(this TracerProviderBuilder tracer)
		{
			tracer
				.AddAspNetCoreInstrumentation(opt =>
				{
					opt.RecordException = true;
				})
				.SetErrorStatusOnException(true)
				.AddSource("MySqlConnector")
				.AddSource(OpenTelemetryService.GX_ACTIVITY_SOURCE_NAME)
				.AddHttpClientInstrumentation(opt =>
				{
					opt.RecordException = true;
				})
				.AddSqlClientInstrumentation(opt =>
				{
					opt.RecordException = true;
					opt.EnableConnectionLevelAttributes = true;
					opt.SetDbStatementForText = true;
				});
				if (Environment.GetEnvironmentVariable("ENABLE_TRACE_CONSOLE_EXPORTER")?.ToLower() == "true")
					tracer.AddConsoleExporter();
			return tracer;
		}
	}
}
