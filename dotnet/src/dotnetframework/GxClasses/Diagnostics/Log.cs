using GeneXus.Attributes;
using GeneXus.Configuration;
using log4net;
#if NETCORE
using GeneXus.Services.Log;
using Microsoft.Extensions.Logging;
#endif

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
		private static readonly string DefaultRepository = LogManager.GetRepository().Name;
		private static readonly string DefaultUserLogNamespace = Config.GetValueOf("USER_LOG_NAMESPACE", LogConfiguration.USER_LOG_TOPIC);
		private static readonly IGXLogger GlobalLog = new GXLoggerLog4Net(LogManager.GetLogger(DefaultRepository, DefaultUserLogNamespace));

		internal static IGXLogger GetLogger(string topic)
		{
			if (!string.IsNullOrEmpty(topic))
			{
				string loggerName = topic.StartsWith("$") ? topic.Substring(1) : string.Format("{0}.{1}", DefaultUserLogNamespace, topic.Trim());
				return GXLoggerFactory.GetLogger(loggerName);
			}
			else
			{
				return GlobalLog;
			}
		}

		public static void Write(int logLevel, string message, string topic)
		{
			Write(message, topic, logLevel);
		}
		public static void Write(string message, string topic, int logLevel)
		{
			LogLevel logLvl = (LogLevel)logLevel;
			WriteImp(message, topic, logLvl);
		}

		private static void WriteImp(string message, string topic, LogLevel logLvl)
		{
			if (logLvl != LogLevel.Off)
			{
				IGXLogger log = GetLogger(topic);
				switch (logLvl)
				{
					case LogLevel.Trace:
						GXLogging.Trace(log, message);
						break;
					case LogLevel.Debug:
						GXLogging.Debug(log, message);
						break;
					case LogLevel.Info:
						GXLogging.Info(log, message);
						break;
					case LogLevel.Warn:
						GXLogging.Warn(log, message);
						break;
					case LogLevel.Error:
						GXLogging.Error(log, message);
						break;
					case LogLevel.Fatal:
						GXLogging.Critical(log, message);
						break;
					default:
						GXLogging.Debug(log, message);
						break;
				}
			}
		}

		public static void Write(string message, string topic = "")
		{
			WriteImp(message, topic, LogLevel.Debug);
		}

		public static void Fatal(string message, string topic = "")
		{
			WriteImp(message, topic, LogLevel.Fatal);
		}

		public static void Error(string message, string topic = "")
		{
			WriteImp(message, topic, LogLevel.Error);
		}

		public static void Warning(string message, string topic = "")
		{
			WriteImp(message, topic, LogLevel.Warn);
		}

		public static void Info(string message, string topic = "")
		{
			WriteImp(message, topic, LogLevel.Info);
		}

		public static void Debug(string message, string topic = "")
		{
			WriteImp(message, topic, LogLevel.Debug);
		}
	}
}
