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
			IGXLogger log = GetLogger(topic);
			LogLevel logLvl = (LogLevel)logLevel;

			switch (logLvl)
			{
				case LogLevel.Off: 
					break;
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
		
		public static void Write(string message, string topic = "")
		{
			IGXLogger log = GetLogger(topic);
			GXLogging.Debug(log, message);
		}
		
		public static void Fatal(string message, string topic = "")
		{
			IGXLogger log = GetLogger(topic);
			GXLogging.Critical(log, message);
		}

		public static void Error(string message, string topic = "")
		{
			IGXLogger log = GetLogger(topic);
			GXLogging.Error(log, message);
		}

		public static void Warning(string message, string topic = "")
		{
			IGXLogger log = GetLogger(topic);
			GXLogging.Warn(log, message);
		}

		public static void Info(string message, string topic = "")
		{
			IGXLogger log = GetLogger(topic);
			GXLogging.Info(log, message);
		}

		public static void Debug(string message, string topic = "")
		{
			IGXLogger log = GetLogger(topic);
			GXLogging.Debug(log, message);
		}
	}
}
