using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Jayrock.Json;
using System.Runtime.Serialization;
#if NETCORE
using System.Linq;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
#endif
using log4net;

namespace GeneXus.Utils
{
#if NETCORE
	public class GxJsonConverter : JsonConverter<object>
	{
		public override bool CanConvert(Type typeToConvert)
		{
			return typeof(object) == typeToConvert;
		}
		public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.True:
					return false;
				case JsonTokenType.False:
					return false;
				case JsonTokenType.StartArray:
					return JsonSerializer.Deserialize<JArray>(ref reader, options);
				case JsonTokenType.StartObject:
					return JsonSerializer.Deserialize<JObject>(ref reader, options);
				default:
					using (JsonDocument document = JsonDocument.ParseValue(ref reader))
					{
						return document.RootElement.Clone().ToString();
					}
			}
		}

		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
	internal class TextJsonSerializer : GXJsonSerializer
	{
		internal override bool IsJsonNull(object jobject)
		{
			return jobject == null;
		}
		internal override T ReadJSON<T>(string json)
		{
			JsonSerializerOptions opts = new JsonSerializerOptions();
			opts.Converters.Add(new GxJsonConverter());
			return JsonSerializer.Deserialize<T>(json, opts);
		}
		internal override string WriteJSON<T>(T kbObject)
		{
			return JsonSerializer.Serialize<T>(kbObject);
		}
	}
#endif
	internal class JayRockJsonSerializer : GXJsonSerializer
	{
		internal override bool IsJsonNull(object jobject)
		{
			return jobject == Jayrock.Json.JNull.Value;
		}
		internal override T ReadJSON<T>(string json)
		{
			Jayrock.Json.JsonTextReader reader = new JsonTextReader(new StringReader(json));
			return (T)reader.DeserializeNext();

		}
		internal override string WriteJSON<T>(T kbObject)
		{
			if (kbObject != null)
			{
				return kbObject.ToString();
			}
			return null;
		}
	}
	internal enum GXJsonSerializerType
	{
		Utf8,
		Jayrock,
		TextJson
	}
	internal abstract class GXJsonSerializer
	{
		private static GXJsonSerializer s_instance = null;
		private static object syncRoot = new Object();
		static GXJsonSerializerType DefaultJSonSerializer= GXJsonSerializerType.Jayrock;
		internal static GXJsonSerializer Instance
		{
			get
			{
				if (s_instance == null)
				{
					lock (syncRoot)
					{
						if (s_instance == null)
						{
							switch (DefaultJSonSerializer)
							{
#if NETCORE
								case GXJsonSerializerType.TextJson:
									s_instance = new TextJsonSerializer();
									break;
#endif
								default:
									s_instance = new JayRockJsonSerializer();
									break;
							}
						}
					}
				}
				return s_instance;
			}
		}

		internal abstract bool IsJsonNull(object jobject);

		internal abstract T ReadJSON<T>(string json) where T : class;

		internal abstract string WriteJSON<T>(T kbObject) where T : class;
	}
	
	public class JSONHelper
	{
		
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.JSONHelper));
		public static bool IsJsonNull(object jobject)
		{
			return GXJsonSerializer.Instance.IsJsonNull(jobject);
		}
		public static T ReadJSON<T>(string json, GXBaseCollection<SdtMessages_Message> Messages = null) where T : class
		{
			try
			{
				if (!string.IsNullOrEmpty(json))
				{
					return GXJsonSerializer.Instance.ReadJSON<T>(json);
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
		public static T ReadJavascriptJSON<T>(string json, GXBaseCollection<SdtMessages_Message> Messages = null) where T : class
		{
			try
			{
				if (!string.IsNullOrEmpty(json))
				{

					return GXJsonSerializer.Instance.ReadJSON<T>(json);
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

		public static string WriteJSON<T>(T kbObject) where T:class
		{
			try
			{
				if (kbObject!=null)
				{
					return GXJsonSerializer.Instance.WriteJSON<T>(kbObject);
				}
				return null;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Serialize error ", ex);
			}
			return null;
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
				var settings = SerializationSettings(knownTypes);
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);
				
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
		internal static string WCFSerialize<T>(T kbObject, Encoding encoding, IEnumerable<Type> knownTypes, bool useSimpleDictionaryFormat) where T : class
		{
			try
			{
				var settings = WCFSerializationSettings(knownTypes, useSimpleDictionaryFormat);
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);
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
		internal static void WCFSerialize<T>(T kbObject, Encoding encoding, IEnumerable<Type> knownTypes, Stream stream) where T : class
		{
			try
			{
				var settings = WCFSerializationSettings(knownTypes);
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), settings);
				serializer.WriteObject(stream, kbObject);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Serialize error ", ex);
			}
		}
		static DataContractJsonSerializerSettings SerializationSettings(IEnumerable<Type> knownTypes)
		{
			return new DataContractJsonSerializerSettings() { DateTimeFormat = new DateTimeFormat(DateTimeUtil.JsonDateFormatMillis), KnownTypes=knownTypes };
		}
		static DataContractJsonSerializerSettings WCFSerializationSettings(IEnumerable<Type> knownTypes, bool useSimpleDictionaryFormat=false) {
			return new DataContractJsonSerializerSettings() { DateTimeFormat = new DateTimeFormat(DateTimeUtil.JsonDateFormatMillis), EmitTypeInformation = EmitTypeInformation.Never, UseSimpleDictionaryFormat= useSimpleDictionaryFormat, KnownTypes=knownTypes };
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
