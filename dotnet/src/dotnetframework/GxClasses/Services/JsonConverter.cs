using GeneXus.Utils;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using Jayrock.Json;

namespace GeneXus.Application
{
	public class SDTConverter : JsonConverter<object>
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(GxUserType).IsAssignableFrom(objectType);
		}
		public override object Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
		{
			throw new Exception("in the custom converter!");
		}
		public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			GxUserType sdt = value as GxUserType;
			if (sdt != null)
			{

				JsonSerializer.Serialize<JObject>((JObject)sdt.GetJSONObject());
			}
			else
			{
				JsonSerializer.Serialize(writer, value, options);
			}
		}
	}

}
