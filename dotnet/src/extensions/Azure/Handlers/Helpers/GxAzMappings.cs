using System.Collections.Generic;
using System.IO;
using GeneXus.Utils;

namespace GeneXus.Deploy.AzureFunctions.Handlers.Helpers
{
	public class CallMappings : ICallMappings
	{
		public List<GxAzMappings> mappings;

		public CallMappings(string roothPath)
		{
			mappings = GetMappings(roothPath);
		}
		public List<GxAzMappings> GetMappings(string rootPath)
		{	
			string mappingsfile = Path.Combine(rootPath, FunctionReferences.MappingsFile);

			if (File.Exists(mappingsfile))
			{
				string mapping = File.ReadAllText(mappingsfile);
				List<GxAzMappings> gxMappingsList = JSONHelper.Deserialize<List<GxAzMappings>>(mapping);
				return gxMappingsList;
			}
			else return null;
		}
	}
	public interface ICallMappings
	{
		public List<GxAzMappings> GetMappings(string rootPath);
	}

	public class GxAzMappings
	{
		public string FunctionName
		{ get; set; }
		public string GXEntrypoint
		{ get; set; }
		
	}
}

