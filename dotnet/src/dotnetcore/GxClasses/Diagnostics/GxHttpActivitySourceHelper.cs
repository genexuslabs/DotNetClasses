using System;
using System.Diagnostics;
using System.Reflection;


namespace GeneXus.Diagnostics
{
	public static class GxHttpActivitySourceHelper
	{
		public static string GX_HTTP_INSTRUMENTATION = "GeneXus.Instrumentation.Http";
		private static ActivitySource ActivitySource { get; } = CreateActivitySource();

		public const string ThreadIdTagName = "thread.id";
		public const string StatusCodeTagName = "otel.status_code";		

		private static ActivitySource CreateActivitySource()
		{
			Assembly assembly = typeof(GxHttpActivitySourceHelper).Assembly;
			string version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version;
			return new ActivitySource(GX_HTTP_INSTRUMENTATION, version);
		}

		public static void SetException(Activity activity, Exception exception)
		{
			string description = exception.Message;
			activity?.SetStatus(ActivityStatusCode.Error, description);
			activity?.SetTag(StatusCodeTagName, "ERROR");
			activity?.SetTag("otel.status_description", description);
			activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
		{
			{ "exception.type", exception.GetType().FullName },
			{ "exception.message", exception.Message },
			{ "exception.source", exception.Source },
			{ "exception.stacktrace", exception.ToString() },
		}));
		}	
	}
}
