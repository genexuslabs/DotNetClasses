using log4net;
using System;

namespace GeneXus.Mail
{
	
	public abstract class GXMailSession
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXMailSession));

		protected short errorCode;
		protected string errorDescription;
		protected short errorDisplay;

		public GXMailSession()
		{
			errorCode = 0;
			errorDescription = "OK";
			errorDisplay = 0;
		}

		public short ErrCode
		{
			get
			{
				return errorCode;
			}
		}

		public string ErrDescription
		{
			get
			{
				return errorDescription;
			}
		}

        public short ErrDisplay
		{
			get
			{
				return errorDisplay;
			}
			set
			{
				errorDisplay = value;
			}
		}

		protected void ResetError()
		{
			errorCode = 0;
			errorDescription = "OK";
		}

		public void HandleMailException(MailException e)
		{
			errorCode = e.ErrorCode;
			errorDescription = e.Message;
			GXLogging.Error(log, e.Message, e);

			if (errorDisplay != 0)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
}
