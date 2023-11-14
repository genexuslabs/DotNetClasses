using System.Diagnostics;
using GeneXus.Application;
using GeneXus.Attributes;

namespace GeneXus.OpenTelemetry.Diagnostics
{
	[GXApi]
	public class OtelTracer
	{
		public enum SpanType
		{
			Client,
			Consumer,
			Internal,
			Producer,
			Server
		}

		public OtelSpan CreateSpan(string name, SpanType kind)
		{
			Activity activity = GXBaseObject.ActivitySource.StartActivity(name, (ActivityKind)kind);
			return new OtelSpan(activity);
		}

		public OtelSpan GetCurrent()
		{
			return new OtelSpan(Activity.Current);
		}

		public bool HasListeners()
		{
			return GXBaseObject.ActivitySource.HasListeners();
		}
	}
}
