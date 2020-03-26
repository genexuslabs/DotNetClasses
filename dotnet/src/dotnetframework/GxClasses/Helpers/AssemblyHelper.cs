using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GeneXus.Helpers
{
	public class AssemblyHelper
	{
		public static string GetUniqueId()
		{
			Assembly assembly = Assembly.GetCallingAssembly();
			DateTime datetime = GetBuildDate(assembly);
			if (datetime==null)
				datetime = new FileInfo(assembly.Location).LastWriteTimeUtc;

			return datetime.Ticks.ToString();
		}
		private static DateTime GetBuildDate(Assembly assembly)
		{
			const string BuildVersionMetadataSuffix = ".";

			var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			if (attribute?.InformationalVersion != null)
			{
				var value = attribute.InformationalVersion;
				var index = value.IndexOf(BuildVersionMetadataSuffix);
				if (index > 0)
				{
					value = value.Substring(0, index);
				}
				if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
				{
					return result;
				}
			}

			return default;
		}
	}
}
