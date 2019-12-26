using System;

namespace GeneXus.Mail
{
	
	public class InvalidMessageException : ApplicationException
	{
		public InvalidMessageException(Exception innerException)
			: base("Invalid Message", innerException)
		{
		}
	}
}
