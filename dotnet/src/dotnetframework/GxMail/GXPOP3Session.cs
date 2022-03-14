using System;
using GeneXus.Mail.Internals;
using GeneXus.Configuration;
using log4net;

namespace GeneXus.Mail
{
	
	public class GXPOP3Session : GXMailSession
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(GXSMTPSession));
		private IPOP3Session session;
		private short secure;
		private short newMessages;

		public GXPOP3Session()
		{
			string implTakenLogMessage;
			Config.GetValueOf("OpenPOP",out string openPop);
			if (!Environment.Is64BitProcess && openPop == "legacy")
            {
                session = new POP3Session();
				implTakenLogMessage = "Using Pop3 Session legacy implementation";

			}
			/*else if (openPop == "SystemNetMail")	--> USAR ESTE NAMING PARA CUANDO SE QUITE POR DEFECTO LA IMPLEMENTACION MailClient*/
			else if (openPop == "MailKit")
			{
				session = new Pop3MailKit();
				implTakenLogMessage = "Using Pop3 Session MailKit library implementation";
			} else
			{
				session = new POP3SessionOpenPop();
				implTakenLogMessage = "Using Pop3 Session OpenPop.Net implementation";
				
			}
			GXLogging.Debug(log, implTakenLogMessage.Trim());
			secure = 0;
			newMessages = 1;
		}

		public string AttachDir
		{
			get
			{
				return session.AttachDir;
			}
			set
			{
				session.AttachDir = value;
			}
		}


		public short Secure
		{
			get
			{
				return secure;
			}
			set
			{
				secure = value;
			}
		}

		public int Count
		{
			get
			{
                return session.GetMessageCount();
			}
		}

		public string Host
		{
			get
			{
				return session.Host;
			}
			set
			{
				session.Host = value;
			}
		}

		public short NewMessages
		{
			get
			{
				return newMessages;
			}
			set
			{
				newMessages = value;
			}
		}

		public string UserName
		{
			get
			{
				return session.UserName;
			}
			set
			{
				session.UserName = value;
			}
		}

		public string Password
		{
            get
            {
                return session.Password;
            }
            set
			{
				session.Password = value;
			}
		}

		public int Port
		{
			get
			{
				return session.Port;
			}
			set
			{
				session.Port = value;
			}
		}

		public short Timeout
		{
			get
			{
				return (short)session.Timeout;
			}
			set
			{
				session.Timeout = value;
			}
		}

		public short Authentication
		{
			get
			{
				return (short)session.Authentication;
			}
			set
			{
				session.Authentication = value;
			}
		}

		public string AuthenticationMethod
		{
			get
			{
				return session.AuthenticationMethod;
			}
			set
			{
				session.AuthenticationMethod = value;
			}
		}

		public short Delete()
		{
			ResetError();
			session.Delete(this);
			return errorCode;
		}

		public short GetNextUID(ref string nextUID)
		{
			ResetError();
            nextUID = session.GetNextUID(this);
			return errorCode;
		}

		public short Login()
		{
			ResetError();
			session.Login(this);
			return errorCode;
		}

		public short Logout()
		{
			session.Logout(this);
			return errorCode;
		}

		public short Receive(GXMailMessage msg)
		{
			ResetError();
			session.Receive(this, msg);
			return errorCode;
		}

		public short Skip()
		{
			ResetError();
			session.Skip(this);
			return errorCode;
		}
	}
}
