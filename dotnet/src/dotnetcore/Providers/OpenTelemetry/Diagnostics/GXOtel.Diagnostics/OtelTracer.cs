using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneXus.Attributes;
using GeneXus.Services.OpenTelemetry;

namespace GeneXus.OpenTelemetry.Diagnostics
{
	[GXApi]
	public class OtelTracer
	{
		internal static ActivitySource activitySource;
		internal static ActivitySource ActivitySource
		{
			get
			{
				if (activitySource == null)
				{
					activitySource = string.IsNullOrEmpty(OpenTelemetryService.GX_ACTIVITY_SOURCE_VERSION) ? new(OpenTelemetryService.GX_ACTIVITY_SOURCE_NAME) : new(OpenTelemetryService.GX_ACTIVITY_SOURCE_NAME, OpenTelemetryService.GX_ACTIVITY_SOURCE_VERSION);
				}
				return activitySource;
			}
		}
		public enum SpanType
		{
			Internal,
			Server,
			Client,
			Producer,
			Consumer
		}

		public OtelSpan CreateSpan(string name)
		{
			//Returns null when the are no listeners.
			Activity activity = ActivitySource.StartActivity(name);
			if (activity != null) 
				return new OtelSpan(activity);
			return null;
		}

		public OtelSpan CreateSpan(string name, SpanType kind)
		{
			Activity activity = ActivitySource.StartActivity(name, (ActivityKind)kind);
				if (activity != null)
				return new OtelSpan(activity);
			else
				return null;
		}

		public OtelSpan CreateSpan(string name, GXTraceContext gxTraceContext, SpanType spanType)
		{
			Activity activity = ActivitySource.StartActivity(name,
														 kind: (ActivityKind)spanType,
														 parentContext: gxTraceContext.ActivityContext);
			return new OtelSpan(activity);

		}
	
		public OtelSpan CreateSpan(string name, GXTraceContext gxTraceContext, SpanType spanType, IList<GXSpanContext> gxSpanContexts)
		{
			//https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs#optional-links
			List<ActivityContext> contexts = new List<ActivityContext>();

			foreach (GXSpanContext gxSpanContext in gxSpanContexts)
			{
				ActivityContext context = gxSpanContext.ActivityContext;
				contexts.Add(context);
			}

			Activity activity = ActivitySource.StartActivity(name,
													 kind: (ActivityKind)spanType,
													 parentContext: gxTraceContext.ActivityContext,
													 links: contexts.Select(ctx => new ActivityLink(ctx)));
			return new OtelSpan(activity);
		}

		public OtelSpan CreateSpan(string name, GXTraceContext gxTraceContext, SpanType spanType, IList<GXSpanContext> gxSpanContexts, DateTime dateTime)
		{
			//https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs#optional-links
			List<ActivityContext> contexts = new List<ActivityContext>();

			foreach (GXSpanContext gxSpanContext in gxSpanContexts)
			{
				ActivityContext context = gxSpanContext.ActivityContext;
				contexts.Add(context);
			}

			Activity activity = ActivitySource.StartActivity(name,
													 kind: (ActivityKind)spanType,
													 parentContext: gxTraceContext.ActivityContext,
													 links: contexts.Select(ctx => new ActivityLink(ctx)),
													 startTime:dateTime);
			return new OtelSpan(activity);
		}

		public static OtelSpan GetCurrent()
		{
			return new OtelSpan(Activity.Current);
		}
	}
}
