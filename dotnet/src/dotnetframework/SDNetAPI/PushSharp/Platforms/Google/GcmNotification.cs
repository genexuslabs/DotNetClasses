using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using PushSharp.Common;
using Jayrock.Json;
using System.IO;
using Artech.Genexus.SDAPI;

namespace PushSharp.Android
{
	public class GcmNotification : PushSharp.Common.Notification
	{
		public static GcmNotification ForSingleResult(GcmMessageTransportResponse response, int resultIndex)
		{
			var result = new GcmNotification();
			result.Platform = PlatformType.AndroidGcm;
			result.Tag = response.Message.Tag;
			result.RegistrationIds.Add(response.Message.RegistrationIds[resultIndex]);
			result.CollapseKey = response.Message.CollapseKey;
			result.JsonData = response.Message.JsonData;
			result.DelayWhileIdle = response.Message.DelayWhileIdle;
			return result;
		}

		public static GcmNotification ForSingleRegistrationId(GcmNotification msg, string registrationId)
		{
			var result = new GcmNotification();
			result.Platform = PlatformType.AndroidGcm;
			result.Tag = msg.Tag;
			result.RegistrationIds.Add(registrationId);
			result.CollapseKey = msg.CollapseKey;
			result.JsonData = msg.JsonData;
			result.DelayWhileIdle = msg.DelayWhileIdle;
			return result;
		}

		public GcmNotification()
		{
			this.Platform = PlatformType.AndroidGcm;

			this.RegistrationIds = new List<string>();
			this.CollapseKey = string.Empty;
			this.JsonData = string.Empty;
			this.DelayWhileIdle = null;
		}

		/// <summary>
		/// Registration ID of the Device
		/// </summary>
		public List<string> RegistrationIds
		{
			get;
			set;
		}

		/// <summary>
		/// Only the latest message with the same collapse key will be delivered
		/// </summary>
		public string CollapseKey
		{
			get;
			set;
		}

		/// <summary>
		/// JSON Payload to be sent in the message
		/// </summary>
		public string JsonData
		{
			get;
			set;
		}

		/// <summary>
		/// If true, C2DM will only be delivered once the device's screen is on
		/// </summary>
		public bool? DelayWhileIdle
		{
			get;
			set;
		}

		/// <summary>
		/// Time in seconds that a message should be kept on the server if the device is offline.  Default is 4 weeks.
		/// </summary>
		public int? TimeToLive
		{
			get;
			set;
		}

		internal string GetJson()
		{
			var json = new JObject();
			
			if (!string.IsNullOrEmpty(this.CollapseKey))
				json.Put("collapse_key", this.CollapseKey);
			
			if (this.TimeToLive.HasValue)
				json.Put("time_to_live", this.TimeToLive.Value);

			json.Put("registration_ids", new JArray(this.RegistrationIds.ToArray()));
				
			if (this.DelayWhileIdle.HasValue)
				json.Put("delay_while_idle", this.DelayWhileIdle.Value);

			if (!string.IsNullOrEmpty(this.JsonData))
			{
                var jsonData = Utils.FromJSonString(this.JsonData);

				if (jsonData != null)
					json.Put("data", jsonData );
			}

			return json.ToString();
		}

		public override string ToString()
		{
			return GetJson();
		}
	}
}
