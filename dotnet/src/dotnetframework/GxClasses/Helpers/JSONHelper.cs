using GeneXus;
using GeneXus.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GeneXus.Utils
{
	public class JSONHelper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.JSONHelper));

		public static T ReadJSON<T>(string json, GXBaseCollection<SdtMessages_Message> Messages = null) where T : class
		{
			try
			{
				if (!string.IsNullOrEmpty(json))
				{
					Jayrock.Json.JsonTextReader reader = new Jayrock.Json.JsonTextReader(new StringReader(json));
					return (T)reader.DeserializeNext();
				}
				else
				{
					GXUtil.ErrorToMessages("FromJson Error", "Empty json", Messages);
					return default(T);
				}
			}
			catch (Exception ex)
			{
				GXUtil.ErrorToMessages("FromJson Error", ex, Messages);
				GXLogging.Error(log, "FromJsonError ", ex);
				return default(T);
			}
		}

		public static string Serialize<T>(T kbObject) where T : class
		{
			return Serialize<T>(kbObject, Encoding.UTF8);
		}

		public static string Serialize<T>(T kbObject, Encoding encoding) where T : class
		{
			return Serialize<T>(kbObject, encoding, null);
		}

		public static string Serialize<T>(T kbObject, Encoding encoding, IEnumerable<Type> knownTypes) where T : class
		{
			try
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
				using (MemoryStream stream = new MemoryStream())
				{
					serializer.WriteObject(stream, kbObject);
					return encoding.GetString(stream.ToArray());
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Serialize error ", ex);
			}
			return null;
		}
		public static T Deserialize<T>(string kbObject, Encoding encoding, IEnumerable<Type> knownTypes) where T : class, new()
		{
			return Deserialize<T>(kbObject, encoding, knownTypes, new T());
		}
		public static T Deserialize<T>(string kbObject, Encoding encoding, IEnumerable<Type> knownTypes, T defaultValue) where T : class
		{
			if (!string.IsNullOrEmpty(kbObject))
			{
				try
				{
					using (MemoryStream stream = new MemoryStream(encoding.GetBytes(kbObject)))
					{
						DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
#pragma warning disable SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
						return (T)serializer.ReadObject(stream);
#pragma warning restore SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Deserialize error ", ex);
				}
			}
			return defaultValue;
		}
		public static T Deserialize<T>(string kbObject, Encoding encoding) where T : class, new()
		{
			return Deserialize<T>(kbObject, Encoding.Unicode, null, new T());
		}

		public static T Deserialize<T>(string kbObject) where T : class, new()
		{
			return Deserialize<T>(kbObject, Encoding.Unicode);
		}

		public static T DeserializeNullDefaultValue<T>(string kbObject) where T : class
		{
			return Deserialize<T>(kbObject, Encoding.Unicode, null, null);
		}

	}

}
