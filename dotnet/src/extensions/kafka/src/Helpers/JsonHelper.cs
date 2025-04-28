using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GeneXus.Messaging.Core
{
	public class JsonHelper
	{
		public static string Serialize<T>(T tobject) where T : new()
		{
			try
			{
				return Newtonsoft.Json.JsonConvert.SerializeObject(tobject);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return null;
		}
		public static Dictionary<string, string> Deserialize(string jsonValue) 
		{
			
			if (!string.IsNullOrEmpty(jsonValue))
			{
				try
				{
					var configDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonValue);

					var flattenedConfig = new Dictionary<string, string>();
					foreach (var kvp in configDictionary)
					{
						if (kvp.Value is Newtonsoft.Json.Linq.JObject nestedObject)
						{
							foreach (var nestedKvp in nestedObject.ToObject<Dictionary<string, string>>())
							{
								flattenedConfig[nestedKvp.Key] = nestedKvp.Value;
							}
						}
						else
						{
							flattenedConfig[kvp.Key] = kvp.Value.ToString();
						}
					}
					return flattenedConfig;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			return null;
		}

	}
}
