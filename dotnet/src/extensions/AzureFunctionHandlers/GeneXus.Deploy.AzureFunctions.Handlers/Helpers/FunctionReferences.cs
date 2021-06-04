using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using GeneXus.Utils;

namespace GeneXus.Deploy.AzureFunctions.Handlers.Helpers
{
	public static class FunctionReferences
	{
		public const string MappingsFile = "gxazmappings.json";
		public const string GeneXusServerlessAPIAssembly = "GeneXusServerlessAPI";
		public const string EventCustomPayloadItemFullClassName = "GeneXus.Programs.genexusserverlessapi.SdtEventCustomPayload_CustomPayloadItem";
		public const string EventMessageFullClassName = "GeneXus.Programs.genexusserverlessapi.SdtEventMessage";
		public const string EventMessagesFullClassName = "GeneXus.Programs.genexusserverlessapi.SdtEventMessages";
		public const string EventMessageResponseFullClassName = "GeneXus.Programs.genexusserverlessapi.SdtEventMessageResponse";
		public const string FunctionHandlerAssembly = "GeneXus.Deploy.AzureFunctions.Handlers.dll";
		public static string GetFunctionEntryPoint(FunctionContext context, ILogger log, string id)
		{
			//Get json file to know the GX procedure to call
			string rootPath = context.FunctionDefinition.PathToAssembly;
			rootPath = rootPath.Replace(FunctionReferences.FunctionHandlerAssembly, "");
			string mapFilePath = Path.Combine(rootPath, MappingsFile);
			string gxProcedure = null;

			if (File.Exists(mapFilePath))
			{
				using (StreamReader r = new StreamReader(mapFilePath))
				{
					string gxazMappings = r.ReadToEnd();

					//The file may have an entry for more than one function.

					List<GxAzMappings> gxMappingsList = JSONHelper.Deserialize<List<GxAzMappings>>(gxazMappings);

					GxAzMappings map = gxMappingsList.Find(x => x.FunctionName == context.FunctionDefinition.Name);
					gxProcedure = map.GXEntrypoint;
				}
			}
			else
			{
				string exMessage = string.Format("{0} GeneXus procedure could not be executed while processing Message Id {1}. Reason: configuration file not found.", FunctionExceptionType.SysRuntimeError, id);
				throw new Exception(exMessage);
			}
			return gxProcedure;
		}
	}

	
}
