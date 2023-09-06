using System;
using GeneXus.Application;
using GxClasses.Helpers;
using Microsoft.Extensions.Logging;

namespace GeneXus.Services.Log
{
	public interface IGXLogProvider : ILoggerFactory
	{
		ILoggerFactory GetLoggerFactory();
	}

	public static class GXLogService
	{
		private static string LOG_SERVICE = "Log";
	
		public static ILoggerFactory GetLogFactory()
		{
			IGXLogProvider gxLogProvider = null;
			GXService providerService = GXServices.Instance?.Get(LOG_SERVICE);

			if (providerService != null)
			{
				try
				{
#if !NETCORE
					Type type = Type.GetType(providerService.ClassName, true, true);
#else
					Type type = AssemblyLoader.GetType(providerService.ClassName);
#endif
					gxLogProvider = (IGXLogProvider)Activator.CreateInstance(type, new object[] { providerService });
					return gxLogProvider.GetLoggerFactory();
				}
				catch (Exception e)
				{
					throw e;
				}
			}
			else
			{
				return null;
			}
			
		}
	}
}
