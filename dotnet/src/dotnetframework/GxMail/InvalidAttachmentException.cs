using System;

namespace GeneXus.Mail
{
	
	public class InvalidAttachmentException : GXMailException
	{
		public InvalidAttachmentException(Exception innerException)
			: base("Invalid Attachment", innerException)
		{
		}
	}
}
