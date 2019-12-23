using System.IO;
using System.Reflection;

namespace GeneXus.Helpers
{
	public class AssemblyHelper
	{
		public static string GetUniqueId()
		{
			var datetime = new FileInfo(Assembly.GetCallingAssembly().Location).LastWriteTimeUtc;
			return datetime.Ticks.ToString();
		}
	}
}
