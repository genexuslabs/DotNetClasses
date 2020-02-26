using System;

namespace GxClasses.Helpers
{
	internal class EnvVarReader
	{
		private const string ENVVAR_PREFIX = "GX_";
		public static bool GetEnvironmentValue(string name, out string value)
		{
			try
			{
				value = Environment.GetEnvironmentVariable($"{ENVVAR_PREFIX}{name.ToUpper()}");
				if (!string.IsNullOrEmpty(value))
					return true;

				return false;
			}
			catch
			{
				value = string.Empty;
				return false;
			}
		}

		public static bool GetEnvironmentValue(string serviceType, string serviceName, string propertyName, out string value)
		{
			try
			{
				string envVarName = $"{ENVVAR_PREFIX}{serviceType.ToUpper()}_{propertyName.ToUpper()}";
				value = Environment.GetEnvironmentVariable(envVarName);
				if (!string.IsNullOrEmpty(value))
					return true;

				envVarName = $"{ENVVAR_PREFIX}{serviceType.ToUpper()}__{serviceName.ToUpper()}_{propertyName.ToUpper()}";
				value = Environment.GetEnvironmentVariable(envVarName);

				if (!string.IsNullOrEmpty(value))
					return true;

				return false;
			}
			catch
			{
				value = string.Empty;
				return false;
			}
		}
	}
}
