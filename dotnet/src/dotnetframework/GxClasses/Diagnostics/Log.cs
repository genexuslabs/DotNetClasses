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

		private static void LoggingFunc(string message, string topic, int logLevel, params string[] list)
		{
			IGXLogger log = GetLogger(topic);
			LogLevel logLvl = (LogLevel)logLevel;

			switch (logLvl)
			{
				case LogLevel.Off:
					break;
				case LogLevel.Trace:
					GXLogging.Trace(log, message, list);
					break;
				case LogLevel.Debug:
					GXLogging.Debug(log, message, list);
					break;
				case LogLevel.Info:
					GXLogging.Info(log, message, list);
					break;
				case LogLevel.Warn:
					GXLogging.Warn(log, message, list);
					break;
				case LogLevel.Error:
					GXLogging.Error(log, message, list);
					break;
				case LogLevel.Fatal:
					GXLogging.Critical(log, message, list);
					break;
				default:
					GXLogging.Debug(log, message, list);
					break;
			}
		}
		public static void Write(string message, string topic, int logLevel, string propertyvalue1)
		{
			LoggingFunc(message, topic, logLevel, propertyvalue1);
		}
		public static void Write(string message, string topic, int logLevel, string propertyvalue1, string propertyvalue2)
		{
			LoggingFunc(message, topic, logLevel, propertyvalue1, propertyvalue2);
		}
		public static void Write(string message, string topic, int logLevel, string propertyvalue1, string propertyvalue2, string propertyvalue3)
		{
			LoggingFunc(message, topic, logLevel, propertyvalue1, propertyvalue2, propertyvalue3);
		}
		public static void Write(string message, string topic, int logLevel, string propertyvalue1, string propertyvalue2, string propertyvalue3, string propertyvalue4)
		{
			LoggingFunc(message, topic, logLevel, propertyvalue1, propertyvalue2, propertyvalue3, propertyvalue4);
		}
		public static void Write(string message, string topic, int logLevel, string propertyvalue1, string propertyvalue2, string propertyvalue3, string propertyvalue4, string propertyvalue5)
		{
			LoggingFunc(message, topic, logLevel, propertyvalue1, propertyvalue2, propertyvalue3, propertyvalue4, propertyvalue5);
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
