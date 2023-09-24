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
#if NETCORE
		public static ILoggerFactory _instance = GXLogService.GetLogFactory();
#endif
	
		public static IGXLogger GetLogger(string topic)
		{
			string defaultUserLogNamespace = Configuration.Config.GetValueOf("USER_LOG_NAMESPACE", LogConfiguration.USER_LOG_TOPIC);
			string loggerName = defaultUserLogNamespace;
			if (!string.IsNullOrEmpty(topic))
			{
				loggerName = topic.StartsWith("$") ? topic.Substring(1) : string.Format("{0}.{1}", defaultUserLogNamespace, topic.Trim());
			}
#if NETCORE
			if (_instance != null)
			{
				return new GXLoggerMsExtensions(_instance.CreateLogger(loggerName));
			}
			string defaultRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly()).Name;
#else
			string defaultRepository = LogManager.GetRepository().Name;
#endif
			return new GXLoggerLog4Net(log4net.LogManager.GetLogger(defaultRepository, loggerName));
			
		}

		public static void Write(int logLevel, string message, string topic)
		{
			Write(message, topic, logLevel);
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
