using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GeneXus.Mail
{
	internal abstract class Pop3SessionBase : IPOP3Session
	{

		protected GXPOP3Session _sessionInfo;
		protected int lastReadMessage;
		protected int count;
		protected List<string> uIds;
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<Pop3SessionBase>();

		public bool DownloadAttachments { get; set; }

		public virtual string Host { get; set; }

		public virtual int Port { get; set; }

		public virtual string UserName { get; set; }

		public virtual string Password { get; set; }

		public virtual int Timeout { get; set; }		

		public string AuthenticationMethod { get; set; }

		public virtual string AttachDir { get; set; }

		public abstract int GetMessageCount();

		public abstract void Login(GXPOP3Session sessionInfo);

		public abstract void Logout(GXPOP3Session sessionInfo);

		public virtual void Skip(GXPOP3Session sessionInfo)
		{
			if (lastReadMessage == count)
			{
				LogError("No messages to receive", "No messages to receive", MailConstants.MAIL_NoMessages, log);
				return;
			}
			++lastReadMessage;
		}

		public abstract void Delete(GXPOP3Session sessionInfo);

		public virtual string GetNextUID(GXPOP3Session session)
		{
			if (lastReadMessage == count)
			{
				LogDebug("No messages to receive", "No messages to receive", MailConstants.MAIL_NoMessages, log);
				return string.Empty;
			}
			int nextMessage = lastReadMessage + 1;
			int zeroBasedIndexForNextMessage = nextMessage - 1;
			return uIds[zeroBasedIndexForNextMessage];
		}

		public abstract void Receive(GXPOP3Session sessionInfo, GXMailMessage gxmessage);

		protected void LogError(string title, string message, int code, Exception e, IGXLogger log)
		{

#if DEBUG
			if (e != null && log.IsErrorEnabled) GXLogging.Error(log, message, e);
#endif
			if (_sessionInfo != null)
			{
				_sessionInfo.HandleMailException(new GXMailException(message, (short)code));
			}
		}

		protected void LogError(string title, string message, int code, IGXLogger log)
		{
			LogError(title, message, code, null,log);
		}

		protected void LogDebug(string title, string message, int code, IGXLogger log)
		{
			LogDebug(title, message, code, null, log);
		}

		protected void LogDebug(string title, string message, int code, Exception e, IGXLogger log)
		{
#if DEBUG
			if (e != null && log.IsDebugEnabled) GXLogging.Debug(log, message, e);
#endif
			if (_sessionInfo != null)
			{
				_sessionInfo.HandleMailException(new GXMailException(message, (short)code));
			}
		}

		protected static void AddHeader(GXMailMessage msg, string key, string value)
		{
			if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
			{
				msg.Headers[key] = value;
			}
		}


	}
}
