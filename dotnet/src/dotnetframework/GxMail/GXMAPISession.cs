using System;
using GeneXus.Mail.Internals;

namespace GeneXus.Mail
{
	
	public class GXMAPISession : GXMailSession
	{
		private MAPISession session;

		public GXMAPISession()
		{
			session = new MAPISession();
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

		public short EditWindow
		{
			get
			{
				return session.EditWindow;
			}
			set
			{
				session.EditWindow = value;
			}
		}

		public short NewMessages
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

		public string Profile
		{
			get
			{
				return session.Profile;
			}
			set
			{
				session.Profile = value;
			}
		}

		public short ChangeFolder(string folder)
		{
			ResetError();
			session.ChangeFolder(this, folder);
			return errorCode;
		}

		public short Delete()
		{
			ResetError();
			session.Delete(this);
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

		public short MarkAsRead()
		{
			ResetError();
			session.MarkAsRead(this);
			return errorCode;
		}

		public short Receive(GXMailMessage msg)
		{
			ResetError();
			session.Receive(this, msg);
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
