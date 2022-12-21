using System;
using log4net;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace GeneXus.Configuration
{
	internal static class LogConfiguration
	{
		public const string USER_LOG_TOPIC = "GeneXusUserLog";
		private const string LOGLEVEL_ENVVAR = "GX_LOG_LEVEL";
		private const string USERLOGLEVEL_ENVVAR = "GX_LOG_LEVEL_USER";

		public static void SetupLog4Net()
		{
			SetupLog4NetFromEnvironmentVariables();
		}

		private static void SetupLog4NetFromEnvironmentVariables()
		{			
			string logLevel = Environment.GetEnvironmentVariable(LOGLEVEL_ENVVAR);

			if (!string.IsNullOrEmpty(logLevel))
			{
				Hierarchy h = (Hierarchy)LogManager.GetRepository();
				h.Root.Level = h.LevelMap[logLevel.ToUpper()];
			}

			string userLogLevel = Environment.GetEnvironmentVariable(USERLOGLEVEL_ENVVAR);

			if (!string.IsNullOrEmpty(userLogLevel))
			{
				ILoggerRepository[] repositories = LogManager.GetAllRepositories();
				foreach (ILoggerRepository repository in repositories)
				{
					Hierarchy hier = (Hierarchy)repository;
					ILogger logger = hier.GetLogger(USER_LOG_TOPIC);
					((Logger)logger).Level = hier.LevelMap[userLogLevel];
				}
			}
		}
	}
}
