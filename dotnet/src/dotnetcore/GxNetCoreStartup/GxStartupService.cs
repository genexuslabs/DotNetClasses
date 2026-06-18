using System;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Metadata;
using GeneXus.Utils;
using Microsoft.Extensions.Hosting;

namespace GeneXus.Application
{
	internal class GxStartupService : IHostedService
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxStartupService>();
		const string GX_STARTUP_PROCEDURE_KEY = "EVENT_BEFORE_STARTUP";

		private readonly IHostApplicationLifetime _lifetime;

		public GxStartupService(IHostApplicationLifetime lifetime)
		{
			_lifetime = lifetime;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			if (!Config.GetValueOf("AppMainNamespace", out string ns))
				return Task.CompletedTask;

			if (!Config.GetValueOf(GX_STARTUP_PROCEDURE_KEY, out string evtProcName) || string.IsNullOrWhiteSpace(evtProcName))
				return Task.CompletedTask;

			parseEventHandlingName(evtProcName, out string className, out string assemblyName);

			GXLogging.Info(log, $"Executing startup procedure: {ns}.{className} (assembly: {assemblyName})");
			try
			{
				GxContext context = GxContext.CreateDefaultInstance();
				object instance = ClassLoader.FindInstance(assemblyName, ns, className, new object[] { context }, null);
				if (instance == null)
				{
					GXLogging.Error(log, $"Startup procedure '{ns}.{className}' not found in assembly '{assemblyName}'.");
					_lifetime.StopApplication();
					return Task.CompletedTask;
				}
				ClassLoader.Execute(instance, "execute", new object[] { });
				GXLogging.Info(log, $"Startup procedure '{className}' executed successfully.");
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Startup procedure '{className}' failed. Application will stop.", ex);
				_lifetime.StopApplication();
			}
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		static void parseEventHandlingName(string input, out string className, out string assemblyName)
		{
			className = string.Empty;
			assemblyName = string.Empty;
			string[] inputSplitted = input.Split(',');
			if (inputSplitted.Length == 1)
			{
				className = inputSplitted[0].Trim();
				assemblyName = className;
			}
			if (inputSplitted.Length == 2)
			{
				className = inputSplitted[0].Trim();
				assemblyName = inputSplitted[1].Trim();
			}
		}
	}
}
