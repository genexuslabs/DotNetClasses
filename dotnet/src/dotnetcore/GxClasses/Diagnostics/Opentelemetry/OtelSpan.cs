using System;
using System.Diagnostics;
using OpenTelemetry.Trace;

namespace GxClasses.Diagnostics.Opentelemetry
{
	public class OtelSpan
	{
		private Activity _activity;
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

		public SpanKind Kind
		{ get => (SpanKind)_activity?.Kind; }

		public string ParentId
		{ get => _activity?.ParentId; }

		public string ParentSpanId
		{ get => _activity?.ParentSpanId.ToString(); }

		public string TraceId
		{ get => _activity?.TraceId.ToString(); }

		public SpanStatusCode Status
		{ get => (SpanStatusCode)_activity?.Status; }

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
