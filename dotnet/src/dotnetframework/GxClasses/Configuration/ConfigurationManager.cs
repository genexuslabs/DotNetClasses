using System;
using GeneXus.Attributes;
using GeneXus.Services;

namespace GeneXus.Configuration
{
	[GXApi]
	public class ConfigurationManager
	{
		public static bool HasValue(string propName, string fileName = "")
		{
			if (string.IsNullOrEmpty(propName))
				return false;

			GXServices services = null;
			if (string.IsNullOrEmpty(fileName))
			{
				string propValue = "";
				if (Config.GetValueOf(propName, out propValue))
					return true;

				services = GXServices.Instance;
			}
			else
			{
				services = new GXServices();
				GXServices.LoadFromFile(fileName, ref services);
			}

			return !string.IsNullOrEmpty(GetValueFromGXServices(services, propName));
		}
		public static string GetValue(string propName, string fileName = "")
		{
			if (string.IsNullOrEmpty(propName))
				return null;

			GXServices services = null;
			if (string.IsNullOrEmpty(fileName))
			{
				string propValue = "";
				if (Config.GetValueOf(propName, out propValue))
					return propValue;

				services = GXServices.Instance;
			}
			else
			{
				services = new GXServices();
				GXServices.LoadFromFile(fileName, ref services);
			}

			return GetValueFromGXServices(services, propName);
		}

		private static string GetValueFromGXServices(GXServices services, string name)
		{
			string[] tokens = name.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
			if (tokens.Length < 2 || tokens.Length > 3)
				return null;

			string key = tokens[0];
			string property = tokens[1];
			if (tokens.Length == 3)
			{
				key = $"{tokens[0]}:{tokens[1]}";
				property = tokens[2];
			}

			GXService service = services.Get(key);
			if (service == null)
				return null;

			return service.Properties[property];
		}
	}
}
