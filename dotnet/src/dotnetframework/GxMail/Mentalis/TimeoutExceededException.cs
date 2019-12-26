using System;

namespace GeneXus.Mail
{
	/// <summary>
	/// Summary description for TimeoutExceededException.
	/// </summary>
	public class TimeoutExceededException : ApplicationException
	{
		public TimeoutExceededException(Exception innerException)
			: base("Timeout Exceeded", innerException)
		{
			
		}
	}
}
