using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.IO;
using log4net;
using GeneXus.Mail.Smtp;

namespace GeneXus.Mail
{
    internal class SMTPMailClient : ISMTPSession
    {
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(SMTPMailClient));

		SmtpClient client;
        private string attachDir;

		public void Login(GXSMTPSession session)
		{
			GXLogging.Debug(log, "Connecting to host: " + session.Host + ", port: " + session.Port);
			attachDir = session.AttachDir;
			client = new SmtpClient();
			client.Port = session.Port;
			client.EnableSsl = session.Secure > 0;
			client.Host = session.Host.Trim();
			client.UseDefaultCredentials = false;

			if (session.Port == 465 && session.Secure > 0)
			{
				//SMTP over SSL is not supported.
				session.HandleMailException(new GXMailException("SMTP over SSL (Port 465) not supported. Please use another Port (ex:587)", GXInternetConstants.MAIL_SMTPOverSSLNotSupported));
				return;
			}

			if (session.Timeout > 0)
			{
				client.Timeout = session.Timeout * 1000;
			}

			client.DeliveryMethod = SmtpDeliveryMethod.Network;

			if (session.Authentication > 0)
			{
				if (String.IsNullOrEmpty(session.UserName) || String.IsNullOrEmpty(session.Password))
				{
					throw new BadCredentialsException();
				}
				else
				{
					client.Credentials = new System.Net.NetworkCredential(session.UserName, session.Password);					
				}				
			}
			string validate = string.Empty;
			Configuration.Config.GetValueOf("SMTP_VALIDATION", out validate);
			if (string.IsNullOrEmpty(validate))
			{
				SmtpHelper.ValidateConnection(client, session.Sender.Address);
			}
		}

        public void Send(GXSMTPSession session, GXMailMessage msg)
        {
            if (client != null)
            {
				using (MailMessage mail = new MailMessage())
				{
					string senderAddress = (!String.IsNullOrEmpty(msg.From.Address) ? msg.From.Address : session.Sender.Address);
					string senderName = (!String.IsNullOrEmpty(msg.From.Name) ? msg.From.Name : session.Sender.Name);
					if (String.IsNullOrEmpty(senderAddress))
					{
						session.HandleMailException(new GXMailException("SmtpSession Sender Address must be specified", GXInternetConstants.MAIL_InvalidSenderAddress));
						return;
					}
					GXLogging.Debug(log, "Sending Message");
					mail.From = new MailAddress(senderAddress, senderName);
					mail.SubjectEncoding = GetEncoding();
					mail.Subject = msg.Subject;

					if (!String.IsNullOrEmpty(msg.HTMLText))
					{
						mail.Body = msg.HTMLText;
						mail.IsBodyHtml = true;
					}
					else
					{
						mail.Body = msg.Text;
					}
					
					foreach (string key in msg.Headers.Keys)
					{
						mail.Headers.Add(key, (string)msg.Headers[key]);
					}

					try
					{
						SendAllRecipients(mail.To, msg.To);
						SendAllRecipients(mail.CC, msg.CC);
						SendAllRecipients(mail.Bcc, msg.BCC);
						SendAllRecipients(mail.ReplyToList, msg.ReplyTo);
					}
					catch (Exception re)
					{
						session.HandleMailException(new GXMailException(re.Message, GXInternetConstants.MAIL_InvalidRecipient));
					}

					foreach (var item in msg.Attachments)
					{
						string fileName = item;
						try
						{
							fileName = System.IO.Path.Combine(attachDir, item);
							mail.Attachments.Add(new Attachment(fileName));
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

					try
					{

						client.Send(mail);
						GXLogging.Debug(log, "Email successfully sent");
					}
					catch (SmtpFailedRecipientsException e)
					{
						session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_InvalidRecipient));
					}
					catch (SmtpException e)
					{
						HandleError(session, e);
					}
				}
            }
            else
            {
                session.HandleMailException(new GXMailException("Must login before sending message", GXInternetConstants.MAIL_CantLogin));
            }
        }     

        private void SendAllRecipients(MailAddressCollection coll, GXMailRecipientCollection gxcoll)
        {
            foreach (GXMailRecipient item in gxcoll)
            {
                if (!String.IsNullOrEmpty(item.Address))
                {
                    coll.Add(new MailAddress(item.Address, item.Name));
                }
            }
        }

        public void Logout(GXSMTPSession session)
        {
            client = null;
            attachDir = string.Empty;
        }

        private Encoding GetEncoding()
        {
            
            string cult;
            if (GeneXus.Configuration.Config.GetValueOf("Culture", out cult) && cult == "ja-JP")
            {
                return Encoding.GetEncoding("ISO-2022-JP");
            }
            return Encoding.UTF8;
        }

        private static void HandleError(GXSMTPSession session, SmtpException e)
        {
            switch (e.StatusCode)
            {
                case SmtpStatusCode.MustIssueStartTlsFirst:
                    session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_AuthenticationError));
                    break;
                default:
                    session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_MessageNotSent));
                    break;
            }
        }
    }
}
