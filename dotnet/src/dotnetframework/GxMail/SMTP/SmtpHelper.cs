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
#if NETCORE
		const string SMTP_TRANSPORT = "_transport";
		const string SMTP_CONNECTION = "_connection";
#else
		const string SMTP_TRANSPORT = "transport";
		const string SMTP_CONNECTION = "connection";
#endif
		const string SMTP_GET_CONNECTION = "GetConnection";
		const string SMTP_TRANSPORT_TYPE = "System.Net.Mail.SmtpTransport";
		const string SMTP_MAIL_COMMAND_TYPE = "System.Net.Mail.MailCommand";
		public static bool ValidateConnection(SmtpClient smtp, string senderAddress, bool checkUnicodeSupport=true)
		{
			string assFullName = typeof(SmtpClient).Assembly.FullName;
			Type smtpTransportType = GetType($"{SMTP_TRANSPORT_TYPE}, {assFullName}");
			object SmtpTransport = null;
			try
			{
				SmtpTransport = SmtpTransportField(smtp);
				if (SmtpTransport != null)
				{
					senderAddress = (string.IsNullOrEmpty(senderAddress)) ? "test@gmail.com" : senderAddress;
					SmtpGetConnection(smtpTransportType, SmtpTransport, smtp);
					var connection = SmtpConnectionField(SmtpTransport);
					if (checkUnicodeSupport)
					{
						Type mailCommandType = GetType($"{SMTP_MAIL_COMMAND_TYPE}, {assFullName}");
						Boolean isUnicodeSupported = (Boolean)Execute(typeof(SmtpClient), smtp, "IsUnicodeSupported", null);
						Execute(mailCommandType, null, "Send", new object[] { connection, Encoding.ASCII.GetBytes("MAIL FROM:"), new MailAddress(senderAddress), isUnicodeSupported });
					}
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
		internal static object SmtpTransportField(SmtpClient smtp)
		{
			return GetFieldValue(smtp, SMTP_TRANSPORT);
		}
		internal static object SmtpConnectionField(object smtpTransport)
		{
			return GetFieldValue(smtpTransport, SMTP_CONNECTION);
		}
		internal static void SmtpGetConnection(Type smtpTransportType, object SmtpTransport, SmtpClient smtp)
		{
#if NETCORE
			Execute(SmtpTransport.GetType(), SmtpTransport, SMTP_GET_CONNECTION, new object[] { smtp.Host, smtp.Port});
#else
			Execute(smtpTransportType, SmtpTransport, SMTP_GET_CONNECTION, new object[] { smtp.ServicePoint });
#endif
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
