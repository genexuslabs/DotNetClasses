using GeneXus.Attributes;
using GeneXus.Configuration;
using log4net;
using System.Collections.Concurrent;

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

		private const string LoggerPrefix = "$";
		private static readonly string DefaultUserLogNamespace = Config.GetValueOf("USER_LOG_NAMESPACE", LogConfiguration.USER_LOG_TOPIC);
		private static ConcurrentDictionary<string, IGXLogger> LoggerDictionary = new ConcurrentDictionary<string, IGXLogger>() {};
		internal static IGXLogger GetLogger(string topic)
		{
			if (LoggerDictionary.TryGetValue(topic, out IGXLogger logger))
			{
				return logger;
			}
			else
			{
				string loggerName = topic.StartsWith(LoggerPrefix) ? topic.Substring(1) : $"{DefaultUserLogNamespace}.{topic.Trim()}";
				logger = GXLoggerFactory.GetLogger(loggerName);
				LoggerDictionary.TryAdd(topic, logger);
				return logger;
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
