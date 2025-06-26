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
		public enum LogLevel
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
				string loggerName;
				if (!string.IsNullOrEmpty(topic))
					loggerName = topic.StartsWith(LoggerPrefix) ? topic.Substring(1) : $"{DefaultUserLogNamespace}.{topic.Trim()}";
				else
					loggerName = DefaultUserLogNamespace;

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

		public static bool IsFatalEnabled()
		{
			return GetLogger(LoggerPrefix).IsCriticalEnabled;
		}
		public static bool IsErrorEnabled()
		{
			return GetLogger(LoggerPrefix).IsErrorEnabled;
		}

		public static bool IsWarnEnabled()
		{
			return GetLogger(LoggerPrefix).IsWarningEnabled;
		}

		public static bool IsInfoEnabled()
		{
			return GetLogger(LoggerPrefix).IsInfoEnabled;
		}

		public static bool IsDebugEnabled()
		{
			return GetLogger(LoggerPrefix).IsDebugEnabled;
		}

		public static bool IsTraceEnabled()
		{
			return GetLogger(LoggerPrefix).IsTraceEnabled;
		}

		public static bool IsEnabled(int logLevel)
		{
			return GetLogger(LoggerPrefix).LogLevelEnabled(logLevel);
		}

		public static bool IsEnabled(int logLevel, string topic = "")
		{
			return GetLogger(topic).LogLevelEnabled(logLevel);
		}

		public static void SetContext(string key, object value)
		{
			GetLogger(LoggerPrefix).SetContext(key, value);
		}

		public static void Write(string message, string topic, int logLevel, object data)
		{
			GetLogger(topic).Write(message, logLevel, data, false);
		}

		public static void Write(string message, string topic, int logLevel, object data, bool stackTrace)
		{
			GetLogger(topic).Write(message, logLevel, data, stackTrace);
		}
	}
}
