using System;

namespace GeneXus.Messaging.Core.Exceptions
{
	public class MessagingConsumeException: Exception
	{
		public int ErrCode { set; get; }		

		public MessagingConsumeException(string message) : base(message)
		{
		}

		public MessagingConsumeException(int code, string v) : base(v)
		{
			ErrCode = code;			
		}
	}
}
