using System;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using ModelContextProtocol.AspNetCore;
using GeneXus.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace GeneXus.Application
{
	public class StartupMcp
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(StartupMcp).FullName);
		public static void AddService(IServiceCollection services)
		{
			Console.Out.WriteLine("Starting MCP Server...");
			var mcp = services.AddMcpServer(options =>
			{
				options.ServerInfo = new ModelContextProtocol.Protocol.Implementation
				{
					Name = "GxMcpServer",
					Version = Assembly.GetExecutingAssembly()
						.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0"
				};
			})
		.WithHttpTransport(transportOptions =>
		{
			// SSE endpoints (/sse, /message) require STATEFUL sessions to support server-to-client push
			transportOptions.Stateless = false;
			transportOptions.IdleTimeout = TimeSpan.FromMinutes(5);
			GXLogging.Debug(log, "MCP HTTP Transport configured: Stateless=false (SSE enabled), IdleTimeout=5min");
		});

			try
			{
				var mcpAssemblies = FileTools.MCPFileTools(Startup.LocalPath).ToList();
				foreach (var assembly in mcpAssemblies)
				{
					try
					{
						mcp.WithToolsFromAssembly(assembly);
						GXLogging.Debug(log, $"Successfully loaded MCP tools from assembly: {assembly.FullName}");
					}
					catch (Exception assemblyEx)
					{
						GXLogging.Error(log, $"Failed to load MCP tools from assembly: {assembly.FullName}", assemblyEx);
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error discovering MCP tool assemblies", ex);
			}
		}

		public static void MapEndpoints(IEndpointRouteBuilder endpoints)
		{
			// Register MCP endpoints at root, exposing /sse and /message		
			endpoints.MapMcp();
			GXLogging.Debug(log, "MCP Routing configured.");

		}
	}
}
