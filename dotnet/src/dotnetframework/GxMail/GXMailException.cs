using System;

namespace GeneXus.Mail
{
    
    public abstract class MailException : Exception
    {
        private short errorCode;

		public MailException(string message, Exception e) : base(message, e) { }
		public MailException(string message) : base(message) { }
        public MailException(string message, short errorCode)
            : this(message)
        {
            this.errorCode = errorCode;
        }

        public short ErrorCode
        {
            get
            {
                return errorCode;
            }
        }
    }

    public class GXMailException : MailException
    {
        public GXMailException(string message) : base(message) { }
		public GXMailException(string message, Exception e) : base(message, e) { }
		public GXMailException(string message, short errorCode)
            : base(message, errorCode)
        {
        }
    }

    public class AuthenticationException : MailException
    {
        public AuthenticationException() : base("Authentication error", MailConstants.MAIL_AuthenticationError) { }
    }
    public class BadCredentialsException : MailException
    {
        public BadCredentialsException() : base("User or password refused", MailConstants.MAIL_PasswordRefused) { }
    }

    public class NoMessagesException : GXMailException
    {
        public NoMessagesException() : base("No messages to receive", MailConstants.MAIL_NoMessages) { }
    }
}
