using System;

namespace GeneXus.Messaging.Core.Exceptions
{
	public class MessageNotDeliveredException : Exception
	{
		public int ErrCode { set; get; }		

		public MessageNotDeliveredException(string message) : base(message)
		{
		}

		public MessageNotDeliveredException(int code, string v) : base(v)
		{
			ErrCode = code;			
		}
	}
}
