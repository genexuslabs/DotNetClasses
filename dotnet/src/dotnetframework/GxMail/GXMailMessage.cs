using System;
using GeneXus.Utils;
using System.Collections;

namespace GeneXus.Mail
{
	
	public class GXMailMessage
	{
        public string MessageId { get; set; }

		private GxStringCollection attachments;
		private GXMailRecipientCollection to;
		private GXMailRecipientCollection cc;
		private GXMailRecipientCollection bcc;
		private GXMailRecipientCollection replyto;
		private DateTime dateReceived;
		private DateTime dateSent;
		private GXMailRecipient from;
		private string htmlText;
		private string subject;
		private string text;
        private Hashtable headers;

		public GXMailMessage()
		{
			attachments = new GxStringCollection();
			to = new GXMailRecipientCollection();
			cc = new GXMailRecipientCollection();
			bcc = new GXMailRecipientCollection();
			replyto = new GXMailRecipientCollection();
			dateReceived = DateTime.MinValue;
			dateSent = DateTime.MinValue;
			from = new GXMailRecipient();
            headers = new Hashtable();
			htmlText = "";
			subject = "";
			text = "";            
		}

		public GxStringCollection Attachments
		{
			get
			{
				return attachments;
			}
		}

		public GXMailRecipientCollection To
		{
			get
			{
				return to;
			}
		}

		public GXMailRecipientCollection CC
		{
			get
			{
				return cc;
			}
		}

		public GXMailRecipientCollection BCC
		{
			get
			{
				return bcc;
			}
		}

        public GXMailRecipientCollection ReplyTo
        {
            get
            {
                return replyto;
            }
        }
        
        public DateTime DateReceived
		{
			get
			{
				return dateReceived;
			}
			set
			{
				dateReceived = value;
			}
		}

		public DateTime DateSent
		{
			get
			{
				return dateSent;
			}
			set
			{
				dateSent = value;
			}
		}

		public GXMailRecipient From
		{
			get
			{
				return from;
			}
			set
			{
				from = value;
			}
		}

		public string HTMLText
		{
			get
			{
				return htmlText;
			}
			set
			{
				htmlText = value;
			}
		}

		public string Subject
		{
			get
			{
				return subject;
			}
			set
			{
				subject = value;
			}
		}

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}

		public void Clear()
		{
			attachments.Clear();
			to.Clear();
			cc.Clear();
			bcc.Clear();
			replyto.Clear();
			dateReceived = DateTime.MinValue;
			dateSent = DateTime.MinValue;
			from = new GXMailRecipient();
			htmlText = string.Empty;
            subject = string.Empty;
            text = string.Empty;
            MessageId = string.Empty;
		}
		
		internal Hashtable Headers
        {
            set { headers = value; }
            get { return headers; }            
        }
		
        public void AddHeader(string key, string value)
        {
            if (!headers.ContainsKey(key))
                headers.Add(key, value);
            else            
                headers[key] = value;            
        }

        public string GetHeader(string key)
        {
            return (headers.ContainsKey(key.ToUpper()))? (string)headers[key] : string.Empty;
        }

		#region To Object
		
		#endregion

		#region From Object
		
		#endregion
	}
}
