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
					// OpenTelemetry.Instrumentation.SqlClient 1.15+ removed EnableConnectionLevelAttributes
					// and SetDbStatementForText. Connection-level attributes and db.statement text are
					// now emitted by default following the stable OpenTelemetry semantic conventions
					// (gated by OTEL_SEMCONV_STABILITY_OPT_IN at the consumer's runtime if needed).
					// For finer control re-enable via EnrichWithSqlCommand and manual Activity tagging.
				});
				string envvar = Environment.GetEnvironmentVariable("OTEL_TRACES_EXPORTER");
				if (envvar != null && envvar.Contains("console"))
					tracer.AddConsoleExporter();
			return tracer;
		}
	}
}
