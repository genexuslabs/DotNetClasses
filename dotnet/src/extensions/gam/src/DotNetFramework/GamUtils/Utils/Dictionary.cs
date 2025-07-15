using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GeneXus.Utils;

namespace GamUtils.Utils
{
	public class Dictionary
	{
		private readonly Dictionary<string, object> userMap;
		private readonly JsonSerializerSettings jsonSettings;

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/
		public Dictionary()
		{
			this.userMap = new Dictionary<string, object>();
			this.jsonSettings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Include
			};
		}

		public object Get(string key)
		{
			object value;
			return userMap.TryGetValue(key, out value) ? value : null;
		}

		public void Set(string key, object value)
		{
			ObjectToMap(key, value);
		}

		public void Remove(string key)
		{
			userMap.Remove(key);
		}

		public void Clear()
		{
			userMap.Clear();
		}

		public string ToJsonString()
		{
			return JsonConvert.SerializeObject(userMap, Formatting.None, jsonSettings);
		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

		private void ObjectToMap(string key, object value)
		{
			if (value == null)
			{
				userMap[key] = null;
			}
			else if (value is IDictionary || value is IList || value is bool || value is int ||
					 value is long || value is float || value is double || value is decimal)
			{
				userMap[key] = value;
			}
			else if (value is GxUserType gxValue)
			{
				userMap[key] = JsonStringToObject(gxValue.ToJSonString());
			}
			else if (value is string strValue)
			{
				object parsed = TryParseJson(strValue);
				userMap[key] = parsed ?? strValue;
			}
			else
			{
				userMap[key] = value.ToString();
			}
		}

		private object JsonStringToObject(string jsonString)
		{
			try
			{
				var token = JToken.Parse(jsonString);
				if (token is JObject)
					return token.ToObject<Dictionary<string, object>>();
				if (token is JArray)
					return token.ToObject<List<object>>();
				return token;
			}
			catch
			{
				return null;
			}
		}

		private object TryParseJson(string json)
		{
			try
			{
				var token = JToken.Parse(json);
				if (token.Type == JTokenType.Object)
					return token.ToObject<Dictionary<string, object>>();
				if (token.Type == JTokenType.Array)
					return token.ToObject<List<object>>();
				if (token.Type == JTokenType.Boolean)
					return token.ToObject<bool>();
				if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
					return token.ToObject<double>();
				if (token.Type == JTokenType.String)
					return token.ToObject<string>();
				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}
