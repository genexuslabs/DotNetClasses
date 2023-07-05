using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System.Linq;
using log4net.Layout;

namespace GeneXus.Configuration
{
	internal class LogConfiguration
	{
		private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(LogConfiguration));

		public  const string USER_LOG_TOPIC = "GeneXusUserLog";
		private const string LOG_LEVEL_ENVVAR = "GX_LOG_LEVEL";
		private const string LOG_LEVEL_USER_ENVVAR = "GX_LOG_LEVEL_USER";
		private const string LOG_OUTPUT_ENVVAR = "GX_LOG_OUTPUT";

		public static void SetupLog4Net()
		{
			SetupLog4NetFromEnvironmentVariables();
		}

		private static void SetupLog4NetFromEnvironmentVariables()
		{
			string logLevel = Environment.GetEnvironmentVariable(LOG_LEVEL_ENVVAR);

			if (!string.IsNullOrEmpty(logLevel))
			{
				Hierarchy h = (Hierarchy)LogManager.GetRepository();
				h.Root.Level = h.LevelMap[logLevel.ToUpper()];
			}

			string userLogLevel = Environment.GetEnvironmentVariable(LOG_LEVEL_USER_ENVVAR);

			if (!string.IsNullOrEmpty(userLogLevel))
			{
				ILoggerRepository[] repositories = LogManager.GetAllRepositories();
				foreach (ILoggerRepository repository in repositories)
				{
					Hierarchy hier = (Hierarchy)repository;
					ILogger logger = hier.GetLogger(USER_LOG_TOPIC);
					if (logger != null)
					{
						((Logger)logger).Level = hier.LevelMap[userLogLevel];
					}
				}
			}

			string appenderName = Environment.GetEnvironmentVariable(LOG_OUTPUT_ENVVAR);			
			if (!String.IsNullOrEmpty(appenderName))
			{
				Hierarchy h = (Hierarchy) LogManager.GetRepository();
				IAppender appenderToAdd = h.GetAppenders().FirstOrDefault(a => a.Name == appenderName);
				if (appenderToAdd == null)
				{
					LogConfiguration.logger.Warning($"Appender '{appenderName}' was not found on Log4Net Config file");
					return;
				}
				
				h.Root.AddAppender(appenderToAdd);
				ILogger logger = h.GetLogger(USER_LOG_TOPIC);
				if (logger != null)
				{
					((Logger)logger).AddAppender(appenderToAdd);
				}
				
			}
		}

	}
}
