using System;
using GeneXus.Mail.Internals;

namespace GeneXus.Mail
{
	
	public class GXOutlookSession : GXMailSession
	{
		private OutlookSession session;

		public GXOutlookSession()
		{
			session = new OutlookSession();
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

		public short ChangeFolder(string fld)
		{
			ResetError();
			session.ChangeFolder(this, fld);
			return errorCode;
		}

		public short Delete()
		{
			ResetError();
			session.Delete(this);
			return errorCode;
		}

		public short MarkAsRead()
		{
			ResetError();
			session.MarkAsRead(this);
			return errorCode;
		}

		public short NewAppointment(string subject, string location, DateTime start, DateTime end, short allDay, int reminderMinutes)
		{
			ResetError();
			session.NewAppointment(this, subject, location, start, end, allDay, reminderMinutes);
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
