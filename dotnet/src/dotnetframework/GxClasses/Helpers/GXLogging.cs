using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace GeneXus
{

	public static class GXLogging
	{
		public static void Trace(this ILog log, params string[] list)
		{
			if (log.Logger.IsEnabledFor(Level.Trace))
			{
				log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, Level.Trace, String.Join(" ", list), null);
			}
		}
        public static void Error(ILog log, string msg, Exception ex)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(msg, ex);
			}
		}
        public static void Error(ILog log, string msg1, string msg2, Exception ex)
		{
			Error(log, msg1 + msg2, ex);
		}
        public static void Error(ILog log, Exception ex, params string[] list)
		{
			if (log.IsErrorEnabled){
				foreach (string parm in list){
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

		public static void DebugSanitized(ILog log, Exception ex, params string[] list)
		{
			if (log.IsDebugEnabled)
			{
				StringBuilder msg = new StringBuilder();
				foreach (string parm in list)
				{
					msg.Append(LogSanitization(parm));
				}
				if (ex != null)
					log.Debug(msg, ex);
				else
					log.Debug(msg);
			}
		}

		private static string LogSanitization(string input)
		{
			char[] charactersAllowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789+-_".ToCharArray();
			Dictionary<char, int> whiteList = new Dictionary<char, int>();
			int idx = 0;
			foreach (char c in charactersAllowed)
			{
				whiteList[c] = idx;
				idx++;
			}
			StringBuilder sanitizedInput = new StringBuilder();
			if (!string.IsNullOrEmpty(input))
			{
				foreach (char c in input)
				{
					if (whiteList.ContainsKey(c))
						sanitizedInput.Append(charactersAllowed[whiteList[c]]);
				}
				return sanitizedInput.ToString();
			}
			else
			{
				return String.Empty;
			}
		}

		public static void Debug(ILog log, params string[] list)
		{
			Debug(log, null, list);
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

	}
}
