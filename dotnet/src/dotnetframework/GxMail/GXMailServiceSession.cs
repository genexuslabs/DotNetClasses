using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneXus.Mail;
using GeneXus.Mail.Exchange;

namespace GeneXus.Mail
{
    public class GXMailServiceSession : GXMailSession
    {
        private IMailService session;

        public GXMailServiceSession()
        {
            session = new ExchangeSession();
        }

        public string ServerUrl
        {
            get
            {
                return session.ServerUrl;
            }
            set
            {
                session.ServerUrl = value;
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

        public int Count
        {
            get
            {
                return session.Count;
            }
        }

        public bool NewMessages
        {
            get
            {
                return session.NewMessages;
            }
            set
            {
                session.NewMessages = value;
            }
        }

        public void SetProperty(String key, String value)
        {
            session.SetProperty(key, value);
        }

        public short ChangeFolder(string fld)
        {
            ResetError();
            session.ChangeFolder(this, fld);
            return errorCode;
        }

        public short Delete(GXMailMessage msg)
        {
            ResetError();
            session.Delete(this, msg);
            return errorCode;
        }

        public short MarkAs(GXMailMessage msg, bool isRead)
        {
            ResetError();
            session.MarkAs(this, msg, isRead);
            return errorCode;
        }

        public void Login()
        {
            ResetError();
            session.Login(this);
        }

        public void Logout()
        {
            session.Logout(this);
        }

        public short Receive(GXMailMessage msg)
        {
            ResetError();
            session.Receive(this, msg);
            return errorCode;
        }

        public short GetMailMessage(string msgId, bool fetchEntireMsg, GXMailMessage msg)
        {
            ResetError();
            session.GetMailMessage(this, msgId, fetchEntireMsg, msg);
            return errorCode;
        }

        public short Send(GXMailMessage msg)
        {
            ResetError();
            session.Send(this, msg);
            return errorCode;
        }
    }
}
