using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Grpc.Core;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GeneXus.Application
{
	public class StartupGrpc
	{
		const string ApplicationName = "GxGrpcStartup";
		static IGXLogger log = GXLoggerFactory.GetLogger(ApplicationName);

		public static void AddService(IServiceCollection services)
		{
			Console.Out.WriteLine("Starting gRPC Server...");
			services.AddGrpc(options =>
			{
				options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16 MB
				options.MaxSendMessageSize = 16 * 1024 * 1024;    // 16 MB
				options.EnableDetailedErrors = true;				
			});
			GXLogging.Debug(log, "gRPC Service configured.");
		}

		public static void MapEndpoints(IEndpointRouteBuilder endpoints)
		{
			Console.Out.WriteLine("Mapping gRPC entry points.");
			FileTools.GrpcTools(Startup.LocalPath, log).ToList().ForEach(
				   file => LoadGrpcServiceFromDll(Startup.LocalPath, endpoints, file.FullPath, file.ServiceName + "Service")
				);
			GXLogging.Debug(log, "gRPC endpoints configured.");
		}

		private static void LoadGrpcServiceFromDll(string baseDirectory, IEndpointRouteBuilder endpoints, string dllFileName, string className)
		{
			Console.Out.WriteLine("GRPC DLL :" + dllFileName);
			var assembly = Assembly.LoadFrom(dllFileName);
			var serviceType = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(className, StringComparison.CurrentCultureIgnoreCase));
			if (serviceType != null)
			{
				var mapGrpcServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions)
					.GetMethods()
					.FirstOrDefault(m => m.Name == "MapGrpcService" && m.IsGenericMethodDefinition);
				var genericMethod = mapGrpcServiceMethod?.MakeGenericMethod(serviceType);
				Console.Out.WriteLine("GRPC MAPPING :" + serviceType);
				genericMethod?.Invoke(null, new object[] { endpoints });
			}
			else
			{
				GXLogging.Debug(log, $"gRPC service '{className}' not found.");
			}
		}
	}
}