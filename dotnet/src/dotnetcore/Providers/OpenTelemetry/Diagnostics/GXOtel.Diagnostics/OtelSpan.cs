using System;
using System.Diagnostics;
using GeneXus.Attributes;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace GeneXus.OpenTelemetry.Diagnostics
{
	[GXApi]
	public class OtelSpan
	{
		private Activity _activity;
		public enum SpanStatusCode
		{
			Unset,
			Ok,
			Error
		}

		internal OtelSpan(Activity activity)
		{
			_activity = activity;
		}

		public OtelSpan()
		{}

		public Activity Activity => _activity;

		#region EO Properties
		public GXSpanContext SpanContext => _activity == null ? null : new GXSpanContext(_activity.Context);

		public GXTraceContext GetContext => _activity == null ? null : new GXTraceContext(_activity.Context);
		public string SpanId => _activity?.Id;

		public string TraceId => _activity?.TraceId.ToHexString();

		#endregion

		#region Methods
		public void Stop()
		{
			_activity?.Stop();
		}
		public void RecordException(string message)
		{
			_activity?.RecordException(new Exception(message));
		}

		public void SetStringAttribute(string property, string value)
		{
			_activity?.SetTag(property, value);
			
		}
		public void SetLongAttribute(string property, long value)
		{
			_activity?.SetTag(property, value);

		}
		public void SetDoubleAttribute(string property, double value)
		{
			_activity?.SetTag(property, value);

		}
		public void SetBooleanAttribute(string property, bool value)
		{
			_activity?.SetTag(property, value);

		}
		public GXTraceContext AddBaggage(string property, string value)
		{  
			Baggage.SetBaggage(property, value);
			if (_activity != null)
			return new GXTraceContext(_activity.Context);
			else return null;
		}

		public string GetBaggageItem(string property,GXTraceContext gXTraceContext)
		{
			return Baggage.GetBaggage(property);
		}
		public void SetStatus(SpanStatusCode spanStatusCode, string message)
		{
			_activity?.SetStatus((ActivityStatusCode)spanStatusCode, message);
		}
		public void SetStatus(SpanStatusCode spanStatusCode)
		{
			_activity?.SetStatus((ActivityStatusCode)spanStatusCode);
		}

		#endregion

		#region Private Methods
		public bool IsRecording => IsSpanRecording();

		private bool IsSpanRecording()
		{
			if (_activity != null)
				return _activity.IsAllDataRequested;
			else
				return false;
		}
		#endregion

	}

	public class GXTraceContext : GXSpanContext
	{
		//Dummy class to be compatible with Java.
		//.NET does not requiere propagating the context explicitly in most of the cases.
		//https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Api/README.md#baggage-api

		public GXTraceContext(ActivityContext activityContext):base(activityContext) { }
		public GXTraceContext():base() { }
	}
	public class GXSpanContext 
	{
		private ActivityContext _activityContext;
		public GXSpanContext()
		{
			_activityContext = Activity.Current.Context;
		}

		public GXSpanContext(ActivityContext activityContext)
		{
			_activityContext = activityContext;
		}
		public ActivityContext ActivityContext { get { return _activityContext; } }

		public string TraceId => GetTraceId();

		public string SpanId => GetSpanId();

		private string GetTraceId()
		{
			if (_activityContext != null)
			{
				ActivityTraceId activityTraceId = _activityContext.TraceId;
				return activityTraceId.ToHexString();
			} else return null;
		}		
		private string GetSpanId()
		{
			if (_activityContext != null)
			{
				ActivitySpanId activitySpanId = _activityContext.SpanId;
				return activitySpanId.ToHexString();
			} else { return null; }
		}
		private GXActivityTraceFlags TraceFlags()
		{
			if (_activityContext != null) { 
				ActivityTraceFlags activityTraceFlags = _activityContext.TraceFlags;
			return (GXActivityTraceFlags)activityTraceFlags;
			}
			else { return GXActivityTraceFlags.None;}
		}

		private string TraceState()
		{
			if (_activityContext != null)
			return _activityContext.TraceState;
			else return null;
		}


		//
		// Summary:
		//     Specifies flags defined by the W3C standard that are associated with an activity.

		internal enum GXActivityTraceFlags
		{
			//
			// Summary:
			//     The activity has not been marked.
			None = 0,
			//
			// Summary:
			//     The activity (or more likely its parents) has been marked as useful to record.
			Recorded = 1
		}
	}
}
