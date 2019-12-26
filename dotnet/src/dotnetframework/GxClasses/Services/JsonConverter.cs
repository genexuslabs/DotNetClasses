using GeneXus.Utils;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace GeneXus.Application
{
	public class SDTConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(GxUserType).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new Exception("in the custom converter!");
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			GxUserType sdt = value as GxUserType;
			if (sdt != null)
			{
				writer.WriteRawValue(sdt.ToJSonString());
			}
			else
			{
				serializer.Serialize(writer, value);
			}
		}
	}

}
