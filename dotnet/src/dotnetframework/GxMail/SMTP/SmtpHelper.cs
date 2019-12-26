using GeneXus.Metadata;
using log4net;
using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace GeneXus.Mail.Smtp
{
	public class SmtpHelper
	{

		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(SMTPMailClient));

		public static bool ValidateConnection(SmtpClient smtp, string senderAddress)
		{			
			string assFullName = typeof(SmtpClient).Assembly.FullName;
			Type smtpTransportType = GetType($"System.Net.Mail.SmtpTransport, {assFullName}");
			object SmtpTransport = null;
			try
			{
				SmtpTransport = GetFieldValue(smtp, "transport");
				if (SmtpTransport != null)
				{
					senderAddress = (string.IsNullOrEmpty(senderAddress)) ? "test@gmail.com" : senderAddress;
					Execute(smtpTransportType, SmtpTransport, "GetConnection", new object[1] { smtp.ServicePoint });
					var connection = GetFieldValue(SmtpTransport, "connection");
					Type mailCommandType = GetType($"System.Net.Mail.MailCommand, {assFullName}");
					Boolean isUnicodeSupported = (Boolean)Execute(typeof(SmtpClient), smtp, "IsUnicodeSupported", null);					
					Execute(mailCommandType, null, "Send", new object[] { connection, Encoding.ASCII.GetBytes("MAIL FROM:"), new MailAddress(senderAddress), isUnicodeSupported });					
				}
			}
			catch (TargetInvocationException e)
			{				
				Exception inner = e.InnerException;
				GXLogging.Error(log, "SMTPConnection check failed", inner);
				if (inner is WebException)
				{
					throw new GXMailException(inner.Message, GXInternetConstants.MAIL_CantLogin);
				}
				else
				{
					throw new GXMailException(inner.Message, GXInternetConstants.MAIL_AuthenticationError);
				}				
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "SMTPConnection check failed",  e);
				//Exception will be ignored to keep trying to send mail.
			}
			finally
			{
				if (smtp != null)
				{
					Execute(typeof(SmtpClient), smtp, "Abort", null);
				}
			}
			return true;
		}

		private static object GetFieldValue(object instance, string FieldName)
		{
			FieldInfo fInfo = instance.GetType().GetField(FieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			return fInfo.GetValue(instance);
		}

		private static Type GetType(string type)
		{
			return Type.GetType(type);
		}

		private static object Execute(Type type, object instance, string methodName, object[] parms)
		{
			MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			return methodInfo.Invoke(instance, parms);
		}

	}
}
