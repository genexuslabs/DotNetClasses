using System;
using System.Collections.Generic;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;
using log4net;
using OpenPop.Mime;
using System.Net.Mail;
using System.IO;
using GeneXus.Utils;
using System.Reflection;

namespace GeneXus.Mail
{
    internal class POP3SessionOpenPop : Pop3SessionBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(POP3SessionOpenPop));

        private Pop3Client client;

        public override int GetMessageCount()
        {
            return count;
        }

        public override void Login(GXPOP3Session sessionInfo)
        {
            GXLogging.Debug(log, "Using OpenPOP POP3 Implementation");
            _sessionInfo = sessionInfo;
            client = new Pop3Client();
            
            try
            {
                client.Connect(Host, Port, sessionInfo.Secure == 1);
                
                client.Authenticate(UserName, Password, OpenPop.Pop3.AuthenticationMethod.Auto);
                count = client.GetMessageCount();
                uIds = client.GetMessageUids();
                uIds.Insert(0, string.Empty);
            }
            catch (PopServerNotAvailableException e)
            {
                LogError("Login Error", "PopServer Not Available", MailConstants.MAIL_CantLogin, e, log);
            }
            catch (PopServerNotFoundException e)
            {
                LogError("Login Error", "Can't connect to host", MailConstants.MAIL_CantLogin, e, log);
			}
            catch (InvalidLoginException e)
            {
                LogError("Login Error", "Authentication error", MailConstants.MAIL_AuthenticationError, e, log);
			}
            catch (Exception e)
            {
                LogError("Login Error", e.Message, MailConstants.MAIL_CantLogin, e, log);
			}
        }

        public override void Logout(GXPOP3Session sessionInfo)
        {
            if (client != null)
            {
                client.Disconnect();
                client.Dispose();
                client = null;
            }
        }

        public override void Skip(GXPOP3Session sessionInfo)
        {
            if (lastReadMessage == count)
            {
                LogError("No messages to receive", "No messages to receive", MailConstants.MAIL_NoMessages, log);
				return;
            }
            ++lastReadMessage;            
        }

		internal static bool HasCROrLF(string data)
		{
			for (int index = 0; index < data.Length; ++index)
			{
				if (data[index] == '\r' || data[index] == '\n')
					return true;
			}
			return false;
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
			try {
				if (count > lastReadMessage)
				{
					Message m = null;
					try
					{
						m = client.GetMessage(++lastReadMessage);
					}
					catch (Exception e)
					{
						LogError("Receive message error", e.Message, MailConstants.MAIL_ServerRepliedErr, e, log);
					}

					if (m != null)
					{
						MailMessage msg;
						try
						{
							msg = m.ToMailMessage();
						}
						catch (ArgumentException ae)
						{
							GXLogging.Error(log, "Receive message error " + ae.Message + " subject:" + m.Headers.Subject, ae);
							PropertyInfo subjectProp = m.Headers.GetType().GetProperty("Subject");
							string subject = m.Headers.Subject;
							if (HasCROrLF(subject))
							{
								subjectProp.SetValue(m.Headers, subject.Replace('\r', ' ').Replace('\n', ' '));
								GXLogging.Warn(log, "Replaced CR and LF in subject " + m.Headers.Subject);
							}
							msg = m.ToMailMessage();
						}
						using (msg)
						{
							gxmessage.From = new GXMailRecipient(msg.From.DisplayName, msg.From.Address);
							SetRecipient(gxmessage.To, msg.To);
							SetRecipient(gxmessage.CC, msg.CC);
							gxmessage.Subject = msg.Subject;
							if (msg.IsBodyHtml)
							{
								gxmessage.HTMLText = msg.Body;
								MessagePart plainText = m.FindFirstPlainTextVersion();
								if (plainText != null)
								{
									gxmessage.Text += plainText.GetBodyAsText();
								}
							}
							else
							{
								gxmessage.Text = msg.Body;
							}
							if (msg.ReplyToList != null && msg.ReplyToList.Count > 0)
							{
								SetRecipient(gxmessage.ReplyTo, msg.ReplyToList);
							}

							gxmessage.DateSent = m.Headers.DateSent;
							if (gxmessage.DateSent.Kind == DateTimeKind.Utc && GeneXus.Application.GxContext.Current != null)
							{
								gxmessage.DateSent = DateTimeUtil.FromTimeZone(m.Headers.DateSent, "Etc/UTC", GeneXus.Application.GxContext.Current);
							}
							gxmessage.DateReceived = Internals.Pop3.MailMessage.GetMessageDate(m.Headers.Date);
							AddHeader(gxmessage, "DispositionNotificationTo", m.Headers.DispositionNotificationTo.ToString());
							ProcessMailAttachments(gxmessage, m.FindAllAttachments());
						}
					}
				}
            }catch(Exception e)
			{
				LogError("Receive message error", e.Message, MailConstants.MAIL_ServerRepliedErr, e, log);
			}
		}

        private void ProcessMailAttachments(GXMailMessage gxmessage, List<MessagePart> attachs)
        {
			if (attachs == null || attachs.Count == 0)
				return;

            if (DownloadAttachments)
            {
                if (!Directory.Exists(AttachDir))
                    Directory.CreateDirectory(AttachDir);
				
				foreach (var attach in attachs)
                {
                    string attachName = FixFileName(AttachDir, attach.FileName);
                    if (!string.IsNullOrEmpty(attach.ContentId) && attach.ContentDisposition != null && attach.ContentDisposition.Inline)
                    {
                        string cid = "cid:" + attach.ContentId;
                        attachName = String.Format("{0}_{1}", attach.ContentId, attachName);
                        gxmessage.HTMLText = gxmessage.HTMLText.Replace(cid, attachName);
                    }
					try {												
						attach.Save(new FileInfo(Path.Combine(AttachDir, attachName)));
						gxmessage.Attachments.Add(attachName);
					}
					catch (Exception e)
					{
						LogError("Could not add Attachment", "Failed to save attachment", MailConstants.MAIL_InvalidAttachment, e, log);
					}
                }
            }
        }

        private static void SetRecipient(GXMailRecipientCollection gxColl, MailAddressCollection coll)
        {
            foreach (var to in coll)
            {
                gxColl.Add(new GXMailRecipient(to.DisplayName, to.Address));
            }
        }

        public override void Delete(GXPOP3Session sessionInfo)
        {
            try
            {
                client.DeleteMessage(lastReadMessage);
            }
            catch (PopServerException e)
            {
                LogError("Delete message error", e.Message, MailConstants.MAIL_ServerRepliedErr, e, log);
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
                {
                    this.DownloadAttachments = true;
                }
            }
        }

    }
}
