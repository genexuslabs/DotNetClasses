using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;
using GeneXus.Services.Log;
using log4net;
using log4net.Config;
using log4net.Core;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GeneXus
{
	public static class GXLogging
	{

		private static Microsoft.Extensions.Logging.ILoggerFactory _instance = GXLogService.GetLogFactory();

		public static ILogger GetLogger<Type>() where Type : class
		{
			return _instance.CreateLogger<Type>();
		}

		public static void Trace(this ILog log, params string[] list)
		{
			if (log.Logger.IsEnabledFor(Level.Trace))
			{
				log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, Level.Trace, string.Join(" ", list), null);
			}
		}

		public static void Trace(ILogger log, params string[] list)
		{
			if (log.IsEnabled(LogLevel.Trace))
				log.LogTrace(string.Join(" ", list));
		}
		public static void Error(ILog log, string msg, Exception ex)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(msg, ex);
			}
		}

		public static void Error(ILogger log, string msg, Exception ex)
		{
			if (log.IsEnabled(LogLevel.Error))
			{
				log.LogError(msg, ex);
			}
		}

		public static void ErrorSanitized(ILog log, string msg, Exception ex)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(Utils.StringUtil.Sanitize(msg, Utils.StringUtil.LogUserEntryWhiteList), ex);
			}
		}

		public static void ErrorSanitized(ILogger log, string msg, Exception ex)
		{
			if (log.IsEnabled(LogLevel.Error))
			{
				log.LogError(Utils.StringUtil.Sanitize(msg, Utils.StringUtil.LogUserEntryWhiteList), ex);
			}
		}

		public static void Error(ILog log, string msg1, string msg2, Exception ex)
		{
			Error(log, msg1 + msg2, ex);
		}

		public static void Error(ILogger log, string msg1, string msg2, Exception ex)
		{
			Error(log, msg1 + msg2, ex);
		}
		public static void Error(ILog log, Exception ex, params string[] list)
		{
			if (log.IsErrorEnabled)
			{
				foreach (string parm in list)
				{
					log.Error(parm);
				}
			}
		}
		public static void Error(ILogger log, Exception ex, params string[] list)
		{
			if (log.IsEnabled(LogLevel.Error))
			{
				foreach (string parm in list)
				{
					log.LogError(parm);
				}
			}
		}
		public static void Error(ILog log, params string[] list)
		{
			Error(log, null, list);
		}

		public static void Error(ILogger log, params string[] list)
		{
			Error(log, null, list);
		}

		public static void Warn(ILog log, Exception ex, params string[] list)
		{
			if (log.IsWarnEnabled)
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(parm);
				}
				if (ex != null)
					log.Warn(msg, ex);
				else
					log.Warn(msg);
			}
		}

		public static void Warn(ILogger log, Exception ex, params string[] list)
		{
			if (log.IsEnabled(LogLevel.Warning))
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(parm);
				}
				if (ex != null)
					log.LogWarning(exception: ex, message: msg.ToString());
				else
					log.LogWarning(message: msg.ToString());
			}
		}
		public static void Warn(ILog log, params string[] list)
		{
			Warn(log, null, list);
		}

		public static void Warn(ILogger log, params string[] list)
		{
			Warn(log, null, list);
		}

		public static void Warn(ILog log, string msg, Exception ex)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(msg, ex);
			}
		}
		public static void Warn(ILogger log, string msg, Exception ex)
		{
			if (log.IsEnabled(LogLevel.Warning))
			{
				log.LogWarning
					(exception: ex, msg);
			}
		}
		public static void Debug(ILog log, Exception ex, params string[] list)
		{
			if (log.IsDebugEnabled)
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(parm);
				}
				if (ex != null)
					log.Debug(msg, ex);
				else
					log.Debug(msg);
			}
		}
		public static void Debug(ILogger log, Exception ex, params string[] list)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(parm);
				}
				if (ex != null)
					log.LogDebug(exception: ex, message: msg.ToString());
				else
					log.LogDebug(message: msg.ToString());
			}
		}

		public static void DebugSanitized(ILog log, Exception ex, params string[] list)
		{
			if (log.IsDebugEnabled)
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(Utils.StringUtil.Sanitize(parm, Utils.StringUtil.LogUserEntryWhiteList));
				}
				if (ex != null)
					log.Debug(msg, ex);
				else
					log.Debug(msg);
			}
		}

		public static void DebugSanitized(ILogger log, Exception ex, params string[] list)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(Utils.StringUtil.Sanitize(parm, Utils.StringUtil.LogUserEntryWhiteList));
				}
				if (ex != null)
					log.LogDebug(exception: ex, message: msg.ToString());
				else
					log.LogDebug(msg.ToString());
			}
		}

		public static void Debug(ILog log, params string[] list)
		{
			Debug(log, null, list);
		}

		public static void Debug(ILogger log, params string[] list)
		{
			Debug(log, null, list);
		}

		public static void DebugSanitized(ILog log, params string[] list)
		{
			DebugSanitized(log, null, list);
		}
		public static void DebugSanitized(ILogger log, params string[] list)
		{
			DebugSanitized(log, null, list);
		}

		public static void Debug(ILog log, string startMsg, Func<string> buildMsg)
		{
			if (log.IsDebugEnabled)
			{
				string msg = buildMsg();
				log.Debug(startMsg + msg);
			}
		}
		public static void Debug(ILogger log, string startMsg, Func<string> buildMsg)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				string msg = buildMsg();
				log.LogDebug(startMsg + msg);
			}
		}
		public static void Debug(ILog log, string msg1, string msg2, Exception ex)
		{
			Debug(log, msg1 + msg2, ex);
		}

		public static void Debug(ILogger log, string msg1, string msg2, Exception ex)
		{
			Debug(log, msg1 + msg2, ex);
		}
		public static void Debug(ILog log, string msg, Exception ex)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(msg, ex);
			}
		}
		public static void Debug(ILogger log, string msg, Exception ex)
		{
			if (log.IsEnabled(LogLevel.Debug))
			{
				log.LogDebug(exception: ex, msg);
			}
		}
		public static void Info(ILog log, params string[] list)
		{
			if (log.IsInfoEnabled)
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(parm);
				}
				log.Info(msg);
			}
		}
		public static void Info(ILogger log, params string[] list)
		{
			if (log.IsEnabled(LogLevel.Information))
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(parm);
				}
				log.LogInformation(msg.ToString());
			}
		}
		public static void Info(ILogger log, string msg, params string[] list)
		{
			if (log.IsEnabled(LogLevel.Information))
			{
				log.LogInformation(msg.ToString(), list);
			}
		}
	}
	internal class Log4NetProvider : ILoggerProvider
	{
		private readonly string _log4NetConfigFile;

		private readonly ConcurrentDictionary<string, ILogger> _loggers =
			new ConcurrentDictionary<string, ILogger>();

		internal Log4NetProvider(string log4NetConfigFile)
		{
			_log4NetConfigFile = log4NetConfigFile;
		}

		public ILogger CreateLogger(string categoryName)
		{
			return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
		}

		public void Dispose()
		{
			_loggers.Clear();
		}

		private ILogger CreateLoggerImplementation(string name)
		{
			return new Log4NetLogger(name, new FileInfo(_log4NetConfigFile));
		}
	}
	internal class Log4NetLogger : ILogger
	{
		private readonly string _name;

		private readonly ILog _log;

		private log4net.Repository.ILoggerRepository _loggerRepository;

		public Log4NetLogger(string name, FileInfo fileInfo)
		{
			_name = name;
			_loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

			_log = LogManager.GetLogger(_loggerRepository.Name, name);
			XmlConfigurator.ConfigureAndWatch(_loggerRepository, fileInfo);

			if (_log.IsDebugEnabled)
			{
				_log.Debug($"log4net configured with {fileInfo.FullName}");
			}

		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			switch (logLevel)
			{
				case LogLevel.Critical:
					return _log.IsFatalEnabled;
				case LogLevel.Debug:
				case LogLevel.Trace:
					return _log.IsDebugEnabled;
				case LogLevel.Error:
					return _log.IsErrorEnabled;
				case LogLevel.Information:
					return _log.IsInfoEnabled;
				case LogLevel.Warning:
					return _log.IsWarnEnabled;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel));
			}
		}

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception exception,
			Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			string message = $"{formatter(state, exception)} {exception}";

			if (!string.IsNullOrEmpty(message) || exception != null)
			{
				switch (logLevel)
				{
					case LogLevel.Critical:
						_log.Fatal(message);
						break;
					case LogLevel.Debug:
					case LogLevel.Trace:
						_log.Debug(message);
						break;
					case LogLevel.Error:
						_log.Error(message);
						break;
					case LogLevel.Information:
						_log.Info(message);
						break;
					case LogLevel.Warning:
						_log.Warn(message);
						break;
					default:
						_log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
						_log.Info(message, exception);
						break;
				}
			}
		}
	}

}
