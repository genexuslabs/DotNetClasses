using System;
using System.Diagnostics;
using GeneXus.Application;
using OpenTelemetry.Trace;
using static GeneXus.OpenTelemetry.Diagnostics.OtelTracer;

namespace GeneXus.OpenTelemetry.Diagnostics
{
	public class OtelSpan
	{
		private Activity _activity;

		public enum SpanStatusCode
		{
			Unset,
			Ok,
			Error
		}
		public string Id
		{ get => _activity?.Id; }

		public bool IsAllDataRequested
		{
			get
			{
				if (_activity != null)
					return _activity.IsAllDataRequested;
				return false;
			}
		}

		public bool IsStopped
		{ get
			{
				if (_activity != null )
					return _activity.IsStopped;
				return false;
			}
		}

		public short Kind
		{ get => (short)_activity?.Kind; }

		public string ParentId
		{ get => _activity?.ParentId; }

		public string ParentSpanId
		{ get => _activity?.ParentSpanId.ToString(); }

		public string TraceId
		{ get => _activity?.TraceId.ToString(); }

		public short Status
		{ get => (short)_activity?.Status; }
		
		internal OtelSpan(Activity activity)
		{
			_activity = activity;
		}

		public OtelSpan()
		{
			_activity = Activity.Current;
		}
		public void Start()
		{
			_activity?.Start();
		}

		public void Stop()
		{
			_activity?.Stop();
		}
		public void RecordException(string message)
		{
			_activity.RecordException(new Exception(message));
		}

		public void SetTag(string property, string value)
		{
			_activity.SetTag(property, value);
		}
		public string GetTagItem(string property)
		{
			return _activity.GetTagItem(property).ToString();
		}
		public void AddBaggage(string property, string value)
		{
			_activity.AddBaggage(property, value);
		}
		public string GetBaggageItem(string property)
		{
			return _activity.GetBaggageItem(property).ToString();
		}
		public void SetStatus(SpanStatusCode spanStatusCode, string message)
		{
			_activity.SetStatus((ActivityStatusCode)spanStatusCode, message);
		}

		public SpanStatusCode GetStatus()
		{
			return (SpanStatusCode)_activity.GetStatus().StatusCode;
		}

		//ToDO
		//public void AddEvent()
		//{

		//}

	}
}
