using System;
using GeneXus.Mail.Internals;
using GeneXus.Utils;
using GeneXus.Configuration;
using log4net;

namespace GeneXus.Mail
{
    
    public class GXSMTPSession : GXMailSession
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXSMTPSession));

        private ISMTPSession session;
        private string attachDir;
        private short authentication;
        private short secure;

        private string host;
        private string userName;
        private string password;
        private int port;
        private short timeout;
        private GXMailRecipient sender;

        public GXSMTPSession()
        {
            initialize();
        }

        public GXSMTPSession(string attachDirectory)
        {
            initialize();
            attachDir = attachDirectory;
        }

        private void initialize()
        {
            string smtpclient = string.Empty;
            if (Config.GetValueOf("SMTPSession", out smtpclient) && smtpclient == "legacy")
            {
                session = new SMTPSession();
                GXLogging.Debug(log,"Using SMTP Session legacy implementation");
            }
            else
            {
                session = new SMTPMailClient();
            }
            authentication = 0;
            secure = 0;
            host = string.Empty;
            userName = string.Empty;
            password = string.Empty;
            attachDir = string.Empty;
            port = 25;
            timeout = 30;
            sender = new GXMailRecipient();
        }

        public string AttachDir
        {
            get
            {
                return attachDir;
            }
            set
            {
                attachDir = value;
                if (!String.IsNullOrEmpty(attachDir.Trim()))
                {
                    char pSep = System.IO.Path.DirectorySeparatorChar;
                    if (!attachDir.EndsWith(pSep.ToString()))
                    {
                        attachDir += pSep.ToString();
                    }
                }

            }
        }

        public short Authentication
        {
            get
            {
                return authentication;
            }
            set
            {
                authentication = value;
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

        public string Host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
            }
        }

        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        public short Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        public GXMailRecipient Sender
        {
            get
            {
                return sender;
            }
            set
            {
                sender = value;
            }
        }

        public short Login()
        {
            ResetError();
			try
			{
				session.Login(this);
			}
			catch (MailException exc)
			{
				HandleMailException(exc);
			}			
            return errorCode;
        }

        public short Send(GXMailMessage msg)
        {
            ResetError();
            session.Send(this, msg);
            return errorCode;
        }

        public short Logout()
        {
            session.Logout(this);
            return errorCode;
        }
    }
}
