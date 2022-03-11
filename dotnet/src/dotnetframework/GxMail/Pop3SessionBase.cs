using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace GeneXus.Mail
{
	internal abstract class Pop3SessionBase : IPOP3Session
	{

		protected GXPOP3Session _sessionInfo;

		public bool DownloadAttachments { get; set; }

		public virtual string Host { get; set; }

		public virtual int Port { get; set; }

		public virtual string UserName { get; set; }

		public virtual string Password { get; set; }

		public virtual int Timeout { get; set; }

		public short Authentication { get; set; }

		public string AuthenticationMethod { get; set; }

		public virtual string AttachDir { get; set; }

		public abstract int GetMessageCount();

		public abstract void Login(GXPOP3Session sessionInfo);

		protected void LogError(string title, string message, int code, Exception e, ILog log)
		{

#if DEBUG
			if (e != null && log.IsErrorEnabled) log.Error(message, e);
#endif
			if (_sessionInfo != null)
			{
				_sessionInfo.HandleMailException(new GXMailException(message, (short)code));
			}
		}

		protected void LogError(string title, string message, int code, ILog log)
		{
			LogError(title, message, code, null,log);
		}

		protected void LogDebug(string title, string message, int code, ILog log)
		{
			LogDebug(title, message, code, null, log);
		}

		protected void LogDebug(string title, string message, int code, Exception e, ILog log)
		{
#if DEBUG
			if (e != null && log.IsDebugEnabled) log.Debug(message, e);
#endif
			if (_sessionInfo != null)
			{
				_sessionInfo.HandleMailException(new GXMailException(message, (short)code));
			}
		}

		public abstract void Logout(GXPOP3Session sessionInfo);

		public abstract void Skip(GXPOP3Session sessionInfo);

		public abstract string GetNextUID(GXPOP3Session session);

		public abstract void Receive(GXPOP3Session sessionInfo, GXMailMessage gxmessage);

		public abstract void Delete(GXPOP3Session sessionInfo);
	}
}
