using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneXus.Utils;
using Jayrock.Json;
using System.Diagnostics;

namespace Artech.Genexus.SDAPI
{
	public class RemoteNotification
	{
		public short DeviceType { get; set; }
		public string DeviceToken { get; set; }

		public string Message { get; set; }
		public string Sound { get; set; }
		public string Badge { get; set; }
		public string Action { get; set; }        
		public short ExecutionTime { get; set; }
		public NotificationParameters Parameters { get; set; }

		public NotificationDelivery Delivery { get; set; }
		public string Title { get; set; }
		public string Icon { get; set; }

		public RemoteNotification()
		{
			Delivery = new NotificationDelivery();
		}

		internal static RemoteNotification FromGxUserType(GxUserType sdt)
		{
			RemoteNotification notification = new RemoteNotification();
			JObject jobj = sdt.GetJSONObject() as JObject;
			if (jobj != null)
			{
				object deviceTypeObj = jobj["DeviceType"];
				if (deviceTypeObj is Int16)
					notification.DeviceType = (short)deviceTypeObj;
				else
					notification.DeviceType = -1;

                notification.DeviceToken = TryGetObjectPropertyValue(jobj, "DeviceToken");
                notification.Message = TryGetObjectPropertyValue(jobj, "Message");
                notification.Title = TryGetObjectPropertyValue(jobj, "Title");
                notification.Icon = TryGetObjectPropertyValue(jobj, "Icon");
                notification.Sound = TryGetObjectPropertyValue(jobj, "Sound");
                notification.Badge = TryGetObjectPropertyValue(jobj, "Badge");
                notification.Delivery.Priority = TryGetObjectPropertyValue(jobj["Delivery"] as JObject, "Priority", "normal");				
				notification.ExecutionTime = 0;
				notification.Parameters = new NotificationParameters();

				JObject eventObj = jobj["Event"] as JObject;
				if (eventObj != null)
				{
					notification.Action = eventObj["Name"] as string;
					object executionTime = eventObj["Execution"];
					if (executionTime is Int16)
						notification.ExecutionTime = (short)executionTime;

					if (eventObj.Contains("Parameters"))
					{
						JArray arr = eventObj["Parameters"] as JArray;
						for (int i = 0; i < arr.Length; i++)
						{
							notification.Parameters.Add(arr.GetObject(i)["Name"] as string, arr.GetObject(i)["Value"] as string);
						}
					}
				}
			}
			return notification;
		}

        private static string TryGetObjectPropertyValue(JObject obj, string PtyName, string defaultValue)
        {
            if (obj != null && obj[PtyName] != null && !string.IsNullOrEmpty((string)obj[PtyName]))
            {
                return (string)obj[PtyName];
            }
            return defaultValue;
        }

        private static string TryGetObjectPropertyValue(JObject obj, string PtyName)
        {
            return TryGetObjectPropertyValue(obj, PtyName, String.Empty);
        }
	}


	public class NotificationDelivery
	{
		public string Priority { get; set; }

		public NotificationDelivery()
		{
			Priority = "normal";
		}
		//public string Expiration { get; set; }
	}
}
