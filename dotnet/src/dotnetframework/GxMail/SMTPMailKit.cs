using System;
using System.IO;
using log4net;
using MimeKit;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net.Sockets;

namespace GeneXus.Mail
{

	internal class SMTPMailKit : ISMTPSession
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(SMTPMailKit));

		SmtpClient client;
		private string attachDir;

		public void Login(GXSMTPSession session)
		{
			GXLogging.Debug(log, "Connecting to host: " + session.Host + ", port: " + session.Port);
			attachDir = session.AttachDir;
			if (session.Port == 465 && session.Secure > 0)
			{
				GXLogging.Warn(log, "SMTP over SSL (Port 465) may not be supported. If this happen, please try using another Port (ex:587)");
			}

			client = new SmtpClient();

			if (session.Timeout > 0)
			{
				client.Timeout = session.Timeout * 1000;
			}

			try
			{
				client.Connect(session.Host.Trim(), session.Port);
			} catch (SocketException e) {
				ThrowLoginException(e);
			} catch (SslHandshakeException e) {
				ThrowLoginException(e);
			}
			catch (Exception e) {
				Exception inner = e.InnerException != null ? e.InnerException : e;
				GXLogging.Error(log, "SMTPConnection check failed", inner);
				throw new GXMailException(inner.Message);
			}

			try
			{
				if (session.Authentication > 0)
				{
					if (String.IsNullOrEmpty(session.AuthenticationMethod)) // Caso que se hace Auth Basic
					{
						if (String.IsNullOrEmpty(session.UserName) || String.IsNullOrEmpty(session.Password))
						{
							throw new BadCredentialsException();
						}
						else
						{
							client.Authenticate(session.UserName, session.Password);
						}

					}
					else // Caso de otros metodos de autenticacion
					{

						switch (session.AuthenticationMethod)
						{
							/*case GXSMTPSession.AuthMethod.XOAUTH2.ToString():*/
							case "XOAUTH2":
								var oauth2 = new SaslMechanismOAuth2(session.UserName, session.Password);
								client.Authenticate(oauth2);
								break;

							default:
								GXLogging.Error(log, "Authentication protocol is not supported");
								throw new Exception("Authentication protocol is not supported. Authentication protocol recieved: " + session.AuthenticationMethod);
						}
					}
				}
			}
			catch (MailKit.Security.AuthenticationException e)
			{
				Exception inner = e.InnerException != null ? e.InnerException : e;
				GXLogging.Error(log, "Authentication exception", inner);
				throw new GXMailException(inner.Message,GXInternetConstants.MAIL_AuthenticationError);
			}
			catch (Exception e) {
				Exception inner = e.InnerException != null ? e.InnerException : e;
				GXLogging.Error(log, "Authentication exception", inner);
				throw new GXMailException(inner.Message);
			}

		}

		private void ThrowLoginException(Exception e) {
			Exception inner = e.InnerException != null ? e.InnerException : e;
			GXLogging.Error(log, "SMTPConnection check failed", inner);
			throw new GXMailException(inner.Message, GXInternetConstants.MAIL_CantLogin);
		}

		public void Send(GXSMTPSession session, GXMailMessage msg)
		{
			if (client != null)
			{
				using (var mail = new MimeMessage())
				{
					string senderAddress = (!String.IsNullOrEmpty(msg.From.Address) ? msg.From.Address : session.Sender.Address);
					string senderName = (!String.IsNullOrEmpty(msg.From.Name) ? msg.From.Name : session.Sender.Name);
					if (String.IsNullOrEmpty(senderAddress))
					{
						session.HandleMailException(new GXMailException("SmtpSession Sender Address must be specified", GXInternetConstants.MAIL_InvalidSenderAddress));
						return;
					}
					GXLogging.Debug(log, "Sending Message");
					mail.From.Add (new MailboxAddress(senderName, senderAddress));
					/*mail.SubjectEncoding = GetEncoding();*/
					mail.Subject = msg.Subject;

					foreach (string key in msg.Headers.Keys)
					{
						mail.Headers.Add(key, (string)msg.Headers[key]);
					}

					try
					{
						SendAllRecipients(mail.To, msg.To);
						SendAllRecipients(mail.Cc, msg.CC);
						SendAllRecipients(mail.Bcc, msg.BCC);
						SendAllRecipients(mail.ReplyTo, msg.ReplyTo);
					}
					catch (Exception re)
					{
						session.HandleMailException(new GXMailException(re.Message, GXInternetConstants.MAIL_InvalidRecipient));
					}


					var builder = new BodyBuilder();

					if (!String.IsNullOrEmpty(msg.HTMLText))
					{
						builder.HtmlBody = msg.HTMLText;
					}
					else
					{
						builder.TextBody = msg.Text;
					}

					foreach (string item in msg.Attachments)
					{
						string fileName = item;
						try
						{
							fileName = Path.Combine(attachDir, item);
							builder.Attachments.Add(fileName);
						}
						catch (FileNotFoundException)
						{
							session.HandleMailException(new GXMailException("Can't find " + fileName, GXInternetConstants.MAIL_InvalidAttachment));
						}
						catch (Exception e)
						{
							session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_InvalidAttachment));
						}
					}

					mail.Body = builder.ToMessageBody();

					try
					{

						client.Send(mail);
						GXLogging.Debug(log, "Email successfully sent");
					}
					catch (ServiceNotAuthenticatedException e)
					{
						session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_AuthenticationError));
					}
					catch (Exception e)
					{
						session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_MessageNotSent));
					}

				}
			}
			else
			{
				session.HandleMailException(new GXMailException("Must login before sending message", GXInternetConstants.MAIL_CantLogin));
			}
		}

		private void SendAllRecipients(InternetAddressList coll, GXMailRecipientCollection gxcoll)
		{
			foreach (GXMailRecipient item in gxcoll)
			{
				if (!string.IsNullOrEmpty(item.Address))
				{
					coll.Add(new MailboxAddress(item.Name, item.Address));
				}
			}
		}

		public void Logout(GXSMTPSession session)
		{
			client = null;
			attachDir = string.Empty;
		}
	}
}
