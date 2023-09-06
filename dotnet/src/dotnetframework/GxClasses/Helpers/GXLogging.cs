using System;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Core;
#if NETCORE
using GeneXus.Services.Log;
using Microsoft.Extensions.Logging;
#endif

namespace GeneXus
{
	public class GXLoggerFactory
	{
#if NETCORE
		public static ILoggerFactory _instance = GXLogService.GetLogFactory();
#endif
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
		bool IsTraceEnabled();
		bool IsErrorEnabled();
		bool IsWarningEnabled();
		bool IsDebugEnabled();
		bool IsInfoEnabled();
		void LogTrace(string value);
		void LogError(string msg, Exception ex);

		void LogError(string msg);
		void LogWarning(Exception ex, string msg);
		void LogWarning(string msg);
		void LogDebug(string msg);
		void LogDebug(Exception ex, string msg);
		void LogInfo(string msg);
	}
#if NETCORE
	internal class GXLoggerMsExtensions : IGXLogger
	{

		internal GXLoggerMsExtensions(Microsoft.Extensions.Logging.ILogger logInstance)
		{
			log = logInstance;
		}
		internal Microsoft.Extensions.Logging.ILogger log { get; set; }
		public bool IsTraceEnabled()
		{
			return log.IsEnabled(LogLevel.Trace);
		}
		public bool IsErrorEnabled()
		{
			return log.IsEnabled(LogLevel.Error);
		}
		public bool IsWarningEnabled()
		{
			return log.IsEnabled(LogLevel.Warning);
		}
		public bool IsDebugEnabled()
		{
			return log.IsEnabled(LogLevel.Debug);
		}
		public bool IsInfoEnabled()
		{
			return log.IsEnabled(LogLevel.Information);
		}

		public void LogTrace(string value)
		{
			log.LogTrace(value);
		}
		public void LogError(string msg, Exception ex)
		{
			log.LogError(msg, ex);
		}
		public void LogError(string msg)
		{
			log.LogError(msg);
		}
		public void LogWarning(Exception ex, string msg)
		{
			log.LogWarning(msg, ex);
		}
		public void LogWarning(string msg)
		{
			log.LogWarning(msg);
		}
		public void LogDebug(string msg)
		{
			log.LogDebug(msg);
		}

		public void LogDebug(Exception ex, string msg)
		{
			log.LogDebug(msg, ex);
		}
		public void LogInfo(string msg)
		{
			log.LogInformation(msg);
		}
	}
#endif
	internal class GXLoggerLog4Net:IGXLogger
	{

		internal ILog log { get; set; }

		internal GXLoggerLog4Net(ILog logInstance)
		{
			log = logInstance;
		}
		public bool IsTraceEnabled()
		{
			return log.Logger.IsEnabledFor(Level.Trace);
		}
		public bool IsErrorEnabled()
		{
			return log.IsErrorEnabled;
		}
		public bool IsWarningEnabled()
		{
			return log.IsWarnEnabled;
		}
		public bool IsDebugEnabled()
		{
			return log.IsDebugEnabled;
		}
		public bool IsInfoEnabled()
		{
			return log.IsInfoEnabled;
		}

		public void LogTrace(string value)
		{
			log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, Level.Trace, value, null);
		}
		public void LogError(string msg, Exception ex)
		{
			log.Error(msg, ex);
		}
		public void LogError(string msg)
		{
			log.Error(msg);
		}
		public void LogWarning(Exception ex, string msg)
		{
			log.Warn(msg, ex);
		}
		public void LogWarning(string msg)
		{
			log.Warn(msg);
		}
		public void LogDebug(string msg)
		{
			log.Debug(msg);
		}

		public void LogDebug(Exception ex, string msg)
		{
			log.Debug(msg, ex);
		}
		public void LogInfo(string msg)
		{
			log.Info(msg);
		}

	}
	public static class GXLogging
	{
		public static void Trace(this ILog log, params string[] list)
		{
			if (log.Logger.IsEnabledFor(Level.Trace))
			{
				log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, Level.Trace, string.Join(" ", list), null);
			}
		}
		internal static void Trace(IGXLogger logger, params string[] list)
		{
			if (logger.IsTraceEnabled())
				logger.LogTrace(string.Join(" ", list));
		}
		public static void Error(ILog log, string msg, Exception ex)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(msg, ex);
			}
		}

		internal static void Error(IGXLogger logger, string msg, Exception ex)
		{
			if (logger.IsErrorEnabled())
			{
				logger.LogError(msg, ex);
			}
		}

		public static void ErrorSanitized(ILog log, string msg, Exception ex)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(Utils.StringUtil.Sanitize(msg, Utils.StringUtil.LogUserEntryWhiteList), ex);
			}
		}

		internal static void ErrorSanitized(IGXLogger logger, string msg, Exception ex)
		{
			if (logger.IsErrorEnabled())
			{
				logger.LogError(Utils.StringUtil.Sanitize(msg, Utils.StringUtil.LogUserEntryWhiteList), ex);
			}
		}

		public static void Error(ILog log, string msg1, string msg2, Exception ex)
		{
			Error(log, msg1 + msg2, ex);
		}

		internal static void Error(IGXLogger logger, string msg1, string msg2, Exception ex)
		{
			Error(logger, msg1 + msg2, ex);
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
		internal static void Error(IGXLogger logger, Exception ex, params string[] list)
		{
			if (logger.IsErrorEnabled())
			{
				foreach (string parm in list)
				{
					logger.LogError(parm);
				}
			}
		}
		public static void Error(ILog log, params string[] list)
		{
			Error(log, null, list);
		}

		internal static void Error(IGXLogger logger, params string[] list)
		{
			Error(logger, null, list);
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

		public static void Warn(IGXLogger logger, Exception ex, params string[] list)
		{
			if (logger.IsWarningEnabled())
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
		public static void Warn(ILog log, params string[] list)
		{
			Warn(log, null, list);
		}

		public static void Warn(IGXLogger logger, params string[] list)
		{
			Warn(logger, null, list);
		}

		public static void Warn(ILog log, string msg, Exception ex)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(msg, ex);
			}
		}
		internal static void Warn(IGXLogger logger, string msg, Exception ex)
		{
			if (logger.IsWarningEnabled())
			{
				logger.LogWarning(ex, msg);
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
		public static void Debug(IGXLogger logger, Exception ex, params string[] list)
		{
			if (logger.IsDebugEnabled())
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

		internal static void DebugSanitized(IGXLogger logger, Exception ex, params string[] list)
		{
			if (logger.IsDebugEnabled())
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

		public static void Debug(ILog log, params string[] list)
		{
			Debug(log, null, list);
		}

		public static void Debug(IGXLogger logger, params string[] list)
		{
			Debug(logger, null, list);
		}

		public static void DebugSanitized(ILog log, params string[] list)
		{
			DebugSanitized(log, null, list);
		}
		internal static void DebugSanitized(IGXLogger logger, params string[] list)
		{
			DebugSanitized(logger, null, list);
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
		internal static void Debug(IGXLogger logger, string msg, Exception ex)
		{
			if (logger.IsDebugEnabled())
			{
				logger.LogDebug(ex, msg);
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
		public static void Info(IGXLogger logger, params string[] list)
		{
			if (logger.IsInfoEnabled())
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
}
