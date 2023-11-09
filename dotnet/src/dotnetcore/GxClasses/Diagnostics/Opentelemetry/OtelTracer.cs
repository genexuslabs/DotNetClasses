using System.Diagnostics;
using GeneXus.Application;
using GeneXus.Attributes;

namespace GxClasses.Diagnostics.Opentelemetry
{
	[GXApi]
	public class OtelTracer
	{
		public enum SpanKind
		{
			Client,
			Consumer,
			Internal,
			Producer,
			Server
		}

		public static OtelSpan CreateSpan(string name, SpanKind kind)
		{
			Activity activity = GXBaseObject.ActivitySource.StartActivity(name, (ActivityKind)kind);
			return new OtelSpan(activity);
		}

		public static OtelSpan GetCurrent()
		{
			return new OtelSpan(Activity.Current);
		}

		public static bool HasListeners()
		{
			return GXBaseObject.ActivitySource.HasListeners();
		}
	}
}
