#region "Mail Management"

namespace GeneXus.Mail
{
using System;
using System.Collections;
using System.Web.Mail;
using System.Text;

	public class GxMailMessage : MailMessage
	{
		
		public GxMailMessage(): base()
		{
			_from = new GxRecip("","");
			_to = new GxLstRecipients();
			_bcc = new GxLstRecipients();
			_cc = new GxLstRecipients();
		}

		#region "Public Properties"

		public GxLstRecipients GxTo
		{
			get
			{
				return _to;
			}
		}
		public GxLstRecipients GxCc
		{
			get
			{
				return _cc;
			}
		}

		public GxLstRecipients GxBcc
		{
			get
			{
				return _bcc;
			}
		}

		public GxRecip GxFrom
		{
			get
			{
				return _from;
			}
			set
			{
				_from = value;
			}
		}
		#endregion

		static public string GetListAsString(GxLstRecipients lst)
		{
			StringBuilder bld = new StringBuilder();
			for (int i = 0; i < lst.Count ; i++)
			{
				bld.Append( ((GxRecip)lst[i]).FullName );
				if (lst.Count > 1 && i < lst.Count - 1)
					bld.Append(";");
			}
			return bld.ToString();
		}

		#region "Private Members"
		private GxRecip _from;
		private GxLstRecipients _to;
		private GxLstRecipients _cc;
		private GxLstRecipients _bcc;
		#endregion
			
	}
	
	public class GxSMTPSession
	{
		#region "Public Properties"
		public string Host
		{
			get
			{
				return SmtpMail.SmtpServer;
			}
			set
			{
				SmtpMail.SmtpServer = value;
			}
		}
		public string UserName
		{
			get { return "";}
			set {}
		}
		public string Password
		{
			get { return "";}
			set {}
		}
		#endregion
		#region "Public Methods"
		public void Login()
		{
		}
		public void Send(GxMailMessage msg)
		{
			try
			{
				msg.To = GxMailMessage.GetListAsString(msg.GxTo);
				msg.Bcc = GxMailMessage.GetListAsString(msg.GxBcc);
				msg.Cc = GxMailMessage.GetListAsString(msg.GxCc);
				msg.From = msg.GxFrom.FullName;
				SmtpMail.Send((MailMessage) msg);
			}
			catch (Exception e)
			{
				_errDescription = e.Message;
			}
		}
		public string ErrDescription
		{
			get
			{
				return _errDescription;
			}
		}
			
		public void Logout()
		{
		}
		#endregion
	
		private string _errDescription;
	}

	public class GxRecip
	{
		
		public GxRecip(string name,string address)
		{
			_name = name;
			_address = address;
		}

		public string FullName
		{
			get
			{
				StringBuilder bld = new StringBuilder();
				if (!String.IsNullOrEmpty(_name))
				{
					bld.Append("\"");  
					bld.Append(_name);
					bld.Append("\"<");
					bld.Append(_address);
					bld.Append(">");
				}
				else			
					bld.Append(_address);
				return bld.ToString();
			}
		}
		#region "Public Properties"
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}
		public string Address
		{
			get
			{
				return _address;
			}
			set
			{
				_address = value;
			}
		}
		#endregion

		#region "Private Members"
		private string _name;
		private string _address;
		#endregion
	}

	public class GxLstRecipients : ArrayList
	{
		
		public void New(string name,string address)
		{
			base.Add(new GxRecip(name,address));
		}
	}
}
#endregion	