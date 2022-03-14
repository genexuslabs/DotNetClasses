using System;
using System.Collections.Generic;
using System.Security.Authentication;
using log4net;
using MailKit;
using MailKit.Security;
using MailKit.Net.Pop3;
using MimeKit;
using GeneXus.Utils;
using System.IO;

namespace GeneXus.Mail
{
	internal class Pop3MailKit : Pop3SessionBase
	{

		private static readonly ILog log = LogManager.GetLogger(typeof(Pop3MailKit));

		private Pop3Client client;

		public override int GetMessageCount()
		{
			try
			{
				return client.Count;
			}
			catch (Exception e)
			{
				GXLogging.Error(log,"Could not get message count.", e);
				return count;
			}
		}

		public override void Login(GXPOP3Session sessionInfo)
		{
			GXLogging.Debug(log, "Using MailKit POP3 Implementation");
			_sessionInfo = sessionInfo;
			client = new Pop3Client();

			try
			{
				client.SslProtocols = sessionInfo.Secure == 1 ? SslProtocols.Tls12 : SslProtocols.None;
				client.Connect(Host, Port);
				if (String.IsNullOrEmpty(_sessionInfo.AuthenticationMethod))
				{
					if (String.IsNullOrEmpty(_sessionInfo.AuthenticationMethod)) // Caso que se hace Auth Basic					
						BasicAuth(_sessionInfo, client);
					else // Caso de otros metodos de autenticacion
					{
						switch (_sessionInfo.AuthenticationMethod)
						{
							case "XOAUTH2":
								var oauth2 = new SaslMechanismOAuth2(_sessionInfo.UserName, _sessionInfo.Password);
								client.Authenticate(oauth2);
								break;

							default:
								GXLogging.Error(log, "Authentication protocol is not supported");
								throw new Exception("Authentication protocol is not supported. Authentication protocol recieved: " + _sessionInfo.AuthenticationMethod);
						}
					}
				} else
					BasicAuth(_sessionInfo, client);
				count = client.Count;
				uIds = (List<string>)client.GetMessageUids();
				/*uIds.Insert(0, string.Empty);*/

			}
			catch (NotSupportedException e)
			{   // Caso en el que se intenta TLSv1.2 connection y no es exitoso el intento. No se intenta nuevamente con otro protocolo ya que .Net toma Tlsv1.1 e inferiores como deprecados
				LogError("Error logging in", "Could not establish TLS version as protocol.", GXInternetConstants.MAIL_CantLogin, e, log);
			}
			catch (AuthenticationException e)
			{
				LogError("Login Error", "Authentication error", MailConstants.MAIL_AuthenticationError, e, log);
			}
			catch (Exception e)
			{
				LogError("Login Error", e.Message, MailConstants.MAIL_CantLogin, e, log);
			}

		}

		private void BasicAuth(GXPOP3Session _sessionInfo, Pop3Client client)
		{
			if (String.IsNullOrEmpty(_sessionInfo.UserName) || String.IsNullOrEmpty(_sessionInfo.Password))
			{
				throw new BadCredentialsException();
			}
			else
			{
				client.Authenticate(_sessionInfo.UserName, _sessionInfo.Password);
			}
		}

		public override void Logout(GXPOP3Session sessionInfo)
		{
			if (client != null)
			{
				client.Disconnect(true);
				client.Dispose();
				client = null;
			}
		}

		public override void Delete(GXPOP3Session sessionInfo)
		{
			try
			{
				client.DeleteMessage(lastReadMessage);
			} catch (ServiceNotConnectedException e)
			{
				LogError("Service not connected", e.Message, MailConstants.MAIL_ServerRepliedErr, e, log);
			} catch (ServiceNotAuthenticatedException e)
			{
				LogError("Service not authenticated", e.Message, MailConstants.MAIL_AuthenticationError, e, log);
			}
			catch (Exception e)
			{
				LogError("Delete message error", e.Message, MailConstants.MAIL_ServerRepliedErr, e, log);
			}
		}

		public override void Receive(GXPOP3Session sessionInfo, GXMailMessage gxmessage)
		{
			if (client == null)
			{
				LogError("Login Error", "Must login", MailConstants.MAIL_CantLogin, log);
				return;
			}
			if (lastReadMessage == count)
			{
				LogDebug("No messages to receive", "No messages to receive", MailConstants.MAIL_NoMessages, log);
				return;
			}
			try
			{
				if (count > lastReadMessage)
				{
					MimeMessage msg = client.GetMessage(++lastReadMessage);
					if (msg != null)
					{
						gxmessage.From = new GXMailRecipient(msg.From[0].Name, msg.From[0].ToString());
						SetRecipient(gxmessage.To, msg.To);
						SetRecipient(gxmessage.CC, msg.Cc);
						gxmessage.Subject = msg.Subject;
						if (!String.IsNullOrEmpty(msg.HtmlBody))
						{
							gxmessage.HTMLText = msg.HtmlBody;
							string plainText = msg.GetTextBody(MimeKit.Text.TextFormat.Text);
							if (plainText != null)
							{
								gxmessage.Text += plainText;
							}
						}
						else
						{
							gxmessage.Text = msg.Body.ToString();
						}
						if (msg.ReplyTo != null && msg.ReplyTo.Count > 0)
						{
							SetRecipient(gxmessage.ReplyTo, msg.ReplyTo);
						}
						gxmessage.DateSent = Convert.ToDateTime(GetHeaderFromMimeMessage(msg, "Date"));
						if (gxmessage.DateSent.Kind == DateTimeKind.Utc && Application.GxContext.Current != null)
						{
							gxmessage.DateSent = DateTimeUtil.FromTimeZone(gxmessage.DateSent, "Etc/UTC", GeneXus.Application.GxContext.Current);
						}
						gxmessage.DateReceived = Internals.Pop3.MailMessage.GetMessageDate(GetHeaderFromMimeMessage(msg,"Delivery-Date"));
						AddHeader(gxmessage, "DispositionNotificationTo", GetHeaderFromMimeMessage(msg, "Disposition-Notification-To"));
						ProcessMailAttachments(gxmessage, msg.Attachments);
					}
				}
			}
			catch (ServiceNotConnectedException e)
			{
				LogError("Service not connected", e.Message, MailConstants.MAIL_ServerRepliedErr, e, log);
			}
			catch (ServiceNotAuthenticatedException e)
			{
				LogError("Service not authenticated", e.Message, MailConstants.MAIL_AuthenticationError, e, log);
			}
			catch (Exception e)
			{
				LogError("Receive message error", e.Message, MailConstants.MAIL_ServerRepliedErr, e, log);
			}
		}

		private static void SetRecipient(GXMailRecipientCollection gxColl, InternetAddressList coll)
		{
			foreach (var to in coll)
			{
				gxColl.Add(new GXMailRecipient(to.Name, to.ToString()));
			}
		}

		private string GetHeaderFromMimeMessage(MimeMessage msg, string headerValue)
		{
			return msg.Headers.Contains(headerValue) ? msg.Headers[msg.Headers.IndexOf(headerValue)].ToString() : null;
		}

		private void ProcessMailAttachments(GXMailMessage gxmessage, IEnumerable<MimeEntity> attachs)
		{
			if (attachs == null || attachs.GetEnumerator().MoveNext())	// Si MoveNext es false significa que el attach viene vacio
				return;

			if (DownloadAttachments)
			{
				if (!Directory.Exists(AttachDir))
					Directory.CreateDirectory(AttachDir);

				foreach (var attach in attachs)
				{
					string attachName = FixFileName(AttachDir, attach is MessagePart ? attach.ContentDisposition?.FileName : ((MimePart)attach).FileName);
					if (!string.IsNullOrEmpty(attach.ContentId) && attach.ContentDisposition != null && !attach.ContentDisposition.IsAttachment)
					{
						string cid = "cid:" + attach.ContentId;
						attachName = String.Format("{0}_{1}", attach.ContentId, attachName);
						gxmessage.HTMLText = gxmessage.HTMLText.Replace(cid, attachName);
					}
					try
					{
						SaveAttachedFile(attach, attachName);
						gxmessage.Attachments.Add(attachName);
					}
					catch (Exception e)
					{
						LogError("Could not add Attachment", "Failed to save attachment", MailConstants.MAIL_InvalidAttachment, e, log);
					}
				}
			}
		}

		private void SaveAttachedFile(MimeEntity attach, string attachName)
		{
			using (var stream = File.Create(AttachDir + attachName))
			{
				if (attach is MessagePart)
				{
					var part = (MessagePart) attach;
					part.Message.WriteTo(stream);
				}
				else
				{
					var part = (MimePart) attach;
					part.Content.DecodeTo(stream);
				}
			}
		}


		private String attachDir = string.Empty;

		public override string AttachDir
		{
			get
			{
				return attachDir;
			}
			set
			{
				attachDir = value;
				if (!string.IsNullOrEmpty(value))
					DownloadAttachments = true;
				else
					DownloadAttachments = false;
			}
		}

	}
}
