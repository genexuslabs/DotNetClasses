using System;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Core;
using System.Threading;
using log4net.Util;
using System.Globalization;
using System.Security;
#if NETCORE
using GeneXus.Services.Log;
using Microsoft.Extensions.Logging;
#endif

namespace GeneXus
{
	public class GXLoggerFactory
	{
#if NETCORE
		static ILoggerFactory _instance = GXLogService.GetLogFactory();
#endif
		public static IGXLogger GetLogger(string categoryName)
		{
#if NETCORE
			if (_instance != null)
			{
				return new GXLoggerMsExtensions(_instance.CreateLogger(categoryName));
			}
#endif
			return new GXLoggerLog4Net(log4net.LogManager.GetLogger(categoryName));
		}
		public static IGXLogger GetLogger<T>() where T : class
		{
#if NETCORE
			if (_instance != null)
			{
				return new GXLoggerMsExtensions(_instance.CreateLogger<T>());
			}
#endif
			return new GXLoggerLog4Net(log4net.LogManager.GetLogger(typeof(T)));
		}

	}
	public interface IGXLogger
	{
		bool IsTraceEnabled { get; }
		bool IsErrorEnabled { get; }
		bool IsWarningEnabled { get; }
		bool IsDebugEnabled { get; }
		bool IsInfoEnabled { get; }
		bool IsCriticalEnabled { get; }

		bool TraceEnabled();
		bool CriticalEnabled();
		bool ErrorEnabled();
		bool WarningEnabled();
		bool DebugEnabled();
		bool InfoEnabled();

	
		void LogTrace(string value);
		void LogTrace(string value, params string[] list);
		void LogError(string msg, Exception ex);

		void LogError(string msg);
		void LogError(string msg, params string[] list);
		void LogWarning(Exception ex, string msg);
		void LogWarning(string msg);
		void LogWarning(string msg, params string[] list);
		void LogDebug(string msg);
		void LogDebug(Exception ex, string msg);
		void LogDebug(string msg, params string[] list);
		void LogInfo(string msg);
		void LogInfo(string msg, params string[] list);
		void LogCritical(string msg);
		void LogCritical(Exception ex , string msg);
		void LogCritical(string msg, params string[] list);
	}
#if NETCORE
	internal class GXLoggerMsExtensions : IGXLogger
	{
		internal Microsoft.Extensions.Logging.ILogger log { get; set; }

		internal GXLoggerMsExtensions(Microsoft.Extensions.Logging.ILogger logInstance)
		{
			log = logInstance;
		}

		public bool IsTraceEnabled { get => TraceEnabled(); }
		public bool IsErrorEnabled { get => ErrorEnabled(); }
		public bool IsWarningEnabled { get => WarningEnabled(); }
		public bool IsDebugEnabled { get => DebugEnabled(); }
		public bool IsInfoEnabled { get => InfoEnabled(); }
		public bool IsCriticalEnabled { get => CriticalEnabled(); }
		public bool TraceEnabled()
		{
			return log.IsEnabled(LogLevel.Trace);
		}
		public bool ErrorEnabled()
		{
			return log.IsEnabled(LogLevel.Error);
		}
		public bool WarningEnabled()
		{
			return log.IsEnabled(LogLevel.Warning);
		}
		public bool DebugEnabled()
		{
			return log.IsEnabled(LogLevel.Debug);
		}
		public bool InfoEnabled()
		{
			return log.IsEnabled(LogLevel.Information);
		}
		public bool CriticalEnabled()
		{
			return log.IsEnabled(LogLevel.Critical);
		}
		public void LogTrace(string msg)
		{
			log.LogTrace(msg);
		}
		public void LogTrace(string msg, params string[] list)
		{
			log.LogTrace(msg, list);
		}
		public void LogError(string msg, Exception ex)
		{
			log.LogError(ex, msg);
		}
		public void LogError(string msg)
		{
			log.LogError(msg);
		}
		public void LogError(string msg, params string[] list)
		{
			log.LogError(msg, list);
		}
		public void LogWarning(Exception ex, string msg)
		{
			log.LogWarning(ex, msg);
		}
		public void LogWarning(string msg)
		{
			log.LogWarning(msg);
		}
		public void LogWarning(string msg, params string[] list)
		{
			log.LogWarning(msg, list);
		}
		public void LogDebug(string msg)
		{
			log.LogDebug(msg);
		}
		public void LogDebug(string msg, params string[] list)
		{
			log.LogDebug(msg, list);
		}
		public void LogDebug(Exception ex, string msg)
		{
			log.LogDebug(ex, msg);
		}
		public void LogInfo(string msg)
		{
			log.LogInformation(msg);
		}

		public void LogInfo(string msg, params string[] list)
		{
			log.LogInformation(msg, list);
		}
		public void LogCritical(string msg)
		{
			log.LogCritical(msg);
		}
		public void LogCritical(Exception ex, string msg)
		{
			log.LogCritical(ex, msg);
		}
		public void LogCritical(string msg, params string[] list)
		{
			log.LogCritical(msg, list);
		}
	}
#endif
	internal class GXLoggerLog4Net : IGXLogger
	{
#if NETCORE
		const string ThreadNameNet8 = ".NET TP Worker";
		const string ThreadNameNet6 = ".NET ThreadPool Worker";
		const string ThreadId = "threadid";
#endif
		private bool _traceEnabled = false;
		private bool _debugEnabled = false;
		internal ILog log { get; set; }

		internal GXLoggerLog4Net(ILog logInstance)
		{
			log = logInstance;
			_traceEnabled = log.Logger.IsEnabledFor(Level.Trace);
			_debugEnabled = log.IsDebugEnabled;
		}
		void SetThreadIdForLogging()
		{
#if NETCORE
			if (ThreadContext.Properties[ThreadId] == null)
			{
				string name = Thread.CurrentThread.Name;
				if (!string.IsNullOrEmpty(name) && name != ThreadNameNet6 && !name.StartsWith(ThreadNameNet8))
				{
					ThreadContext.Properties[ThreadId] = name;
				}
				else
				{
					try
					{
						ThreadContext.Properties[ThreadId] = SystemInfo.CurrentThreadId.ToString(NumberFormatInfo.InvariantInfo);
					}
					catch (SecurityException)
					{
						log.Debug("Security exception while trying to get current thread ID. Error Ignored. Empty thread name.");
						ThreadContext.Properties[ThreadId] = Thread.CurrentThread.GetHashCode().ToString(CultureInfo.InvariantCulture);
					}
				}
			}
#endif
		}
		public bool IsTraceEnabled { get => _traceEnabled; }
		public bool IsErrorEnabled { get => ErrorEnabled(); }
		public bool IsWarningEnabled { get => WarningEnabled(); }
		public bool IsDebugEnabled { get => _debugEnabled; }
		public bool IsInfoEnabled { get => InfoEnabled(); }
		public bool IsCriticalEnabled { get => CriticalEnabled(); }
		public bool TraceEnabled()
		{
			return _traceEnabled;
		}
		public bool ErrorEnabled()
		{
			return log.IsErrorEnabled;
		}
		public bool WarningEnabled()
		{
			return log.IsWarnEnabled;
		}
		public bool DebugEnabled()
		{
			return _debugEnabled;
		}
		public bool InfoEnabled()
		{
			return log.IsInfoEnabled;
		}
		public bool CriticalEnabled()
		{
			return log.IsFatalEnabled;
		}

		public void LogTrace(string value)
		{
			SetThreadIdForLogging();
			log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, Level.Trace, value, null);
		}

		public void LogTrace(string value, params string[] list)
		{
			SetThreadIdForLogging();
			StringBuilder message = new StringBuilder();
			message.Append(value);
			foreach (string parm in list)
			{
				message.Append(parm);
			}
			log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, Level.Trace, message.ToString(), null);
		}
		public void LogError(string msg, Exception ex)
		{
			SetThreadIdForLogging();
			log.Error(msg, ex);
		}
		public void LogError(string msg)
		{
			SetThreadIdForLogging();
			log.Error(msg);
		}
		public void LogError(string msg, params string[] list)
		{
			StringBuilder message = new StringBuilder();
			message.Append(msg);
			foreach (string parm in list)
			{
				message.Append(parm);
			}

			LogError(message.ToString());
		}
		public void LogWarning(Exception ex, string msg)
		{
			SetThreadIdForLogging();
			log.Warn(msg, ex);
		}
		public void LogWarning(string msg)
		{
			SetThreadIdForLogging();
			log.Warn(msg);
		}
		public void LogWarning(string msg, params string[] list)
		{
			StringBuilder message = new StringBuilder();
			message.Append(msg);
			foreach (string parm in list)
			{
				message.Append(parm);
			}

			LogWarning(message.ToString());
		}
		public void LogDebug(string msg)
		{
			SetThreadIdForLogging();
			log.Debug(msg);
		}

		public void LogDebug(Exception ex, string msg)
		{
			SetThreadIdForLogging();
			log.Debug(msg, ex);
		}
		public void LogDebug(string msg, params string[] list)
		{
			StringBuilder message = new StringBuilder();
			message.Append(msg);
			foreach (string parm in list)
			{
				message.Append(parm);
			}

			LogDebug(message.ToString());
		}
		public void LogInfo(string msg)
		{
			SetThreadIdForLogging();
			log.Info(msg);
		}
		public void LogInfo(string msg, params string[] list)
		{
			StringBuilder message = new StringBuilder();
			message.Append(msg);
			foreach (string parm in list)
			{
				message.Append(parm);
			}

			LogInfo(message.ToString());
		}

		public void LogCritical(string msg)
		{
			SetThreadIdForLogging();
			log.Fatal(msg);
		}
		public void LogCritical(Exception ex, string msg)
		{
			SetThreadIdForLogging();
			log.Fatal(msg, ex);
		}

		public void LogCritical(string msg, params string[] list)
		{
			StringBuilder message = new StringBuilder();
			message.Append(msg);
			foreach (string parm in list)
			{
				message.Append(parm);
			}

			LogCritical(message.ToString());
		}
	}
	public static class GXLogging
	{
		#region log4NET
		// Legacy //
		public static void Trace(this ILog log, params string[] list)
		{
			if (log.Logger.IsEnabledFor(Level.Trace))
			{
				log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, Level.Trace, string.Join(" ", list), null);
			}
		}
		public static void Error(ILog log, string msg, Exception ex)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(msg, ex);
			}
		}
		public static void ErrorSanitized(ILog log, string msg, Exception ex)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(Utils.StringUtil.Sanitize(msg, Utils.StringUtil.LogUserEntryWhiteList), ex);
			}
		}
		public static void Error(ILog log, string msg1, string msg2, Exception ex)
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
		public static void Error(ILog log, params string[] list)
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
		public static void Warn(ILog log, params string[] list)
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
		public static void Debug(ILog log, params string[] list)
		{
			Debug(log, null, list);
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
		public static void DebugSanitized(ILog log, params string[] list)
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
		public static void Debug(ILog log, string msg1, string msg2, Exception ex)
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
		#endregion

		#region Microsoft.Extensions
		internal static void Trace(IGXLogger logger, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsTraceEnabled)
					logger.LogTrace(string.Join(" ", list));
			}
		}
		internal static void Trace(IGXLogger logger, string msg, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsTraceEnabled)
					logger.LogTrace(msg, list);
			}
		}

		internal static void Trace(IGXLogger logger, Func<string> buildMsg)
		{
			if (logger != null)
			{
				if (logger.IsTraceEnabled)
				{
					string msg = buildMsg();	
					logger.LogTrace(msg);
				}
			}
		}
		internal static bool TraceEnabled(IGXLogger logger)
		{
			if (logger != null)
				return logger.IsTraceEnabled;
			else
				return false;
		}

		public static void Critical(IGXLogger logger, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsCriticalEnabled)
				{
					logger.LogCritical(string.Join(" ", list));
				}
			}
		}
		public static void Critical(IGXLogger logger, string msg, Exception ex)
		{
			if (logger != null)
			{
				if (logger.IsCriticalEnabled)
				{
					logger.LogCritical(ex, msg);
				}
			}
		}
		public static void Critical(IGXLogger logger, string msg, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsCriticalEnabled)
				{
					logger.LogCritical(msg, list);
				}
			}
		}

		public static void Error(IGXLogger logger, string msg, Exception ex)
		{
			if (logger != null)
			{
				if (logger.IsErrorEnabled)
				{
					logger.LogError(msg, ex);
				}
			}
		}
		public static void Error(IGXLogger logger, string msg, params string[] list )
		{
			if (logger != null)
			{
				if (logger.IsErrorEnabled)
				{
					logger.LogError(msg, list);
				}
			}
		}

		internal static void ErrorSanitized(IGXLogger logger, string msg, Exception ex)
		{
			if (logger != null)
			{
				if (logger.IsErrorEnabled)
				{
					logger.LogError(Utils.StringUtil.Sanitize(msg, Utils.StringUtil.LogUserEntryWhiteList), ex);
				}
			}
		}

		internal static void Error(IGXLogger logger, string msg1, string msg2, Exception ex)
		{
			Error(logger, msg1 + msg2, ex);
		}
		
		internal static void Error(IGXLogger logger, Exception ex, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsErrorEnabled)
				{
					logger.LogError(ex.Message);
					foreach (string parm in list)
					{
						logger.LogError(parm);
					}
				}
			}
		}
		internal static void Error(IGXLogger logger, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsErrorEnabled)
				{
					foreach (string parm in list)
					{
						logger.LogError(parm);
					}
				}
			}
		}
		public static void Warn(IGXLogger logger, Exception ex, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsWarningEnabled)
				{
					StringBuilder msg = new StringBuilder();
					foreach (string parm in list)
					{
						msg.Append(parm);
					}
					if (ex != null)
						logger.LogWarning(ex, msg.ToString());
					else
						logger.LogWarning(msg.ToString());
				}
			}
		}
		public static void Warn(IGXLogger logger, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsWarningEnabled)
				{
					StringBuilder msg = new StringBuilder();
					foreach (string parm in list)
					{
						msg.Append(parm);
					}

					logger.LogWarning(msg.ToString());
				}
			}
		}
		public static void Warn(IGXLogger logger, string msg, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsWarningEnabled)
				{
					logger.LogWarning(msg, list);
				}
			}
		}
		public static void Warn(IGXLogger logger, string msg, Exception ex)
		{
			if (logger != null)
			{
				if (logger.IsWarningEnabled)
				{
					logger.LogWarning(ex, msg);
				}
			}
		}
		internal static void DebugSanitized(IGXLogger logger, Exception ex, params string[] list)
		{
			if (logger != null)
			{ 
				if (logger.IsDebugEnabled)
				{
					StringBuilder msg = new StringBuilder();
					foreach (string parm in list)
					{
						msg.Append(Utils.StringUtil.Sanitize(parm, Utils.StringUtil.LogUserEntryWhiteList));
					}
					if (ex != null)
						logger.LogDebug(ex, msg.ToString());
					else
						logger.LogDebug(msg.ToString());
				}
			}
		}
		internal static void DebugSanitized(IGXLogger logger, params string[] list)
		{
			DebugSanitized(logger, null, list);
		}
		public static void Debug(IGXLogger logger, Exception ex, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsDebugEnabled)
				{
					StringBuilder msg = new StringBuilder();
					foreach (string parm in list)
					{
						msg.Append(parm);
					}
					if (ex != null)
						logger.LogDebug(ex, msg.ToString());
					else
						logger.LogDebug(msg.ToString());
				}
			}
		}
		public static void Debug(IGXLogger logger, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsDebugEnabled)
				{
					StringBuilder msg = new StringBuilder();
					foreach (string parm in list)
					{
						msg.Append(parm);
					}
				
					logger.LogDebug(msg.ToString());
				}
			}
		}
		
		public static void Debug(IGXLogger logger, string startMsg, Func<string> buildMsg)
		{
			if (logger != null)
			{
				if (logger.IsDebugEnabled)
				{
					string msg = buildMsg();
					logger.LogDebug(startMsg + msg);
				}
			}
		}
		public static void Debug(IGXLogger logger, string msg1, string msg2, Exception ex)
		{
			Debug(logger, msg1 + msg2, ex);
		}
		public static void Debug(IGXLogger logger, string msg, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsDebugEnabled)
				{
					logger.LogDebug(msg, list);
				}
			}
		}
		public static void Debug(IGXLogger logger, string msg, Exception ex)
		{
			if (logger != null)
			{
				if (logger.IsDebugEnabled)
				{
					logger.LogDebug(ex, msg);
				}
			}
		}
		public static void Info(IGXLogger logger, params string[] list)
		{
			if (logger != null)
			{
				if (logger.IsInfoEnabled)
				{
					StringBuilder msg = new StringBuilder();
					foreach (string parm in list)
					{
						msg.Append(parm);
					}
					logger.LogInfo(msg.ToString());
				}
			}
		}
		public static void Info(IGXLogger logger, string msg, params string[] list)
		{
			if (logger != null) { 
				if (logger.IsInfoEnabled)
				{
					logger.LogInfo(msg.ToString(), list);
				}
			}
		}
		#endregion
	}
}
