using System;
using GeneXus.Attributes;
using GeneXus.Configuration;
using log4net;

namespace GeneXus.Diagnostics
{
	[GXApi]
	public class Log
	{
		private enum LogLevel
		{
			Off = 0,
			Trace = 1,
			Debug = 5,
			Info = 10,
			Warn = 15,
			Error = 20,
			Fatal = 30
		}
#if NETCORE
		static readonly string defaultRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly()).Name;
#else
		static readonly string defaultRepository = LogManager.GetRepository().Name;
#endif
		public static string defaultUserLogNamespace = Configuration.Config.GetValueOf("USER_LOG_NAMESPACE", LogConfiguration.USER_LOG_TOPIC);

		static readonly ILog globalLog = LogManager.GetLogger(defaultRepository, defaultUserLogNamespace);

		private static ILog GetLogger(string topic)
		{
			if (!String.IsNullOrEmpty(topic))
			{
				string loggerName = topic.StartsWith("$") ? topic.Substring(1) : string.Format("{0}.{1}", defaultUserLogNamespace, topic.Trim());
				return LogManager.GetLogger(defaultRepository, loggerName);
			}
			return globalLog;
		}

		public static void Write(int logLevel, string message, string topic)
		{
			Write(message, topic, logLevel);
		}

		public static void Write(string message, string topic, int logLevel)
		{
			ILog log = GetLogger(topic);
			LogLevel logLvl = (LogLevel)logLevel;

			switch (logLvl)
			{
				case LogLevel.Off: 
					break;
				case LogLevel.Trace:
					log.Debug(message);
					break;
				case LogLevel.Debug:
					log.Debug(message);
					break;
				case LogLevel.Info:
					log.Info(message);
					break;
				case LogLevel.Warn:
					log.Warn(message);
					break;
				case LogLevel.Error:
					log.Error(message);
					break;
				case LogLevel.Fatal:
					log.Fatal(message);
					break;
				default:
					log.Debug(message);
					break;
			}			
		}
		
		public static void Write(string message, string topic = "")
		{
			GetLogger(topic).Debug(message);
		}
		
		public static void Fatal(string message, string topic = "")
		{
			GetLogger(topic).Fatal(message);
		}

		public static void Error(string message, string topic = "")
		{
			GetLogger(topic).Error(message);
		}

		public static void Warning(string message, string topic = "")
		{
			GetLogger(topic).Warn(message);
		}

		public static void Info(string message, string topic = "")
		{
			GetLogger(topic).Info(message);
		}

		public static void Debug(string message, string topic = "")
		{
			GetLogger(topic).Debug(message);
		}
	}
}
