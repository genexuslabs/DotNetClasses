using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GeneXus.Application
{
	internal static class FileTools
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(FileTools).FullName);
		public static List<Assembly> MCPFileTools(string baseDirectory)
		{
			// List of Assemblies with MCP tools
			List<Assembly> mcpAssemblies = new();
			string currentBin = Path.Combine(baseDirectory, "bin");
			if (!Directory.Exists(currentBin))
			{
				currentBin = baseDirectory;
				if (!Directory.Exists(currentBin))
				{
					currentBin = "";
				}
			}
			if (!String.IsNullOrEmpty(currentBin))
			{
				GXLogging.Info(log, $"[{DateTime.Now:T}] Registering MCP tools.");
				List<string> L = Directory.GetFiles(currentBin, "*mcp_service.dll").ToList<string>();
				foreach (string mcpFile in L)
				{
					var assembly = Assembly.LoadFrom(mcpFile);
					foreach (var tool in assembly.GetTypes())
					{
						// Each MCP Assembly is added to the list to return for registration
						var attributes = tool.GetCustomAttributes().Where(att => att.ToString() == "ModelContextProtocol.Server.McpServerToolTypeAttribute");
						if (attributes != null && attributes.Any())
						{
							GXLogging.Info(log, $"[{DateTime.Now:T}] Loading tool {mcpFile}.");
							mcpAssemblies.Add(assembly);
						}
					}
				}
			}
			return mcpAssemblies;
		}
	}



}
