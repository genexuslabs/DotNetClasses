using System;
using System.Collections.Generic;
using System.Text;
using OpenPop.Pop3;
using OpenPop.Pop3.Exceptions;
using log4net;
using OpenPop.Mime;
using System.Net.Mail;
using System.IO;
using GeneXus.Utils;
using System.Text.RegularExpressions;
using System.Reflection;

namespace GeneXus.Mail
{
    internal class POP3SessionOpenPop : IPOP3Session
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(POP3SessionOpenPop));

        private Pop3Client client;
        private GXPOP3Session _sessionInfo;
        private int lastReadMessage;
        private int count;
        private List<string> uIds;

        public int GetMessageCount()
        {
            return count;
        }

        public void Login(GXPOP3Session sessionInfo)
        {
            GXLogging.Debug(log, "Using OpenPOP POP3 Implementation");
            _sessionInfo = sessionInfo;
            client = new Pop3Client();
            
            try
            {
                client.Connect(Host, Port, sessionInfo.Secure == 1);
                
                client.Authenticate(UserName, Password, AuthenticationMethod.Auto);
                count = client.GetMessageCount();
                uIds = client.GetMessageUids();
                uIds.Insert(0, string.Empty);
            }
            catch (PopServerNotAvailableException e)
            {
                LogError("Login Error", "PopServer Not Available", MailConstants.MAIL_CantLogin, e);
            }
            catch (PopServerNotFoundException e)
            {
                LogError("Login Error", "Can't connect to host", MailConstants.MAIL_CantLogin, e);
            }
            catch (InvalidLoginException e)
            {
                LogError("Login Error", "Authentication error", MailConstants.MAIL_AuthenticationError, e);
            }
            catch (Exception e)
            {
                LogError("Login Error", e.Message, MailConstants.MAIL_CantLogin, e);
            }
        }

        public void Logout(GXPOP3Session sessionInfo)
        {
            if (client != null)
            {
                client.Disconnect();
                client.Dispose();
                client = null;
            }
        }

        public void Skip(GXPOP3Session sessionInfo)
        {
            if (lastReadMessage == count)
            {
                LogError("No messages to receive", "No messages to receive", MailConstants.MAIL_NoMessages);
                return;
            }
            ++lastReadMessage;            
        }

        public string GetNextUID(GXPOP3Session session)
        {
            if (lastReadMessage == count)
            {
                LogDebug("No messages to receive", "No messages to receive", MailConstants.MAIL_NoMessages);
                return "";
            }
            return uIds[lastReadMessage + 1];
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
		public void Receive(GXPOP3Session sessionInfo, GXMailMessage gxmessage)
        {
            if (client == null)
            {
                LogError("Login Error", "Must login", MailConstants.MAIL_CantLogin);
                return;
            }

            if (lastReadMessage == count)
            {
                LogDebug("No messages to receive", "No messages to receive", MailConstants.MAIL_NoMessages);
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
						LogError("Receive message error", e.Message, MailConstants.MAIL_ServerRepliedErr, e);
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
#if DEBUG
							if (log.IsErrorEnabled) log.Error("Receive message error " + ae.Message + " subject:" + m.Headers.Subject, ae);
#endif
							PropertyInfo subjectProp = m.Headers.GetType().GetProperty("Subject");
							string subject = m.Headers.Subject;
							if (HasCROrLF(subject))
							{
								subjectProp.SetValue(m.Headers, subject.Replace('\r', ' ').Replace('\n', ' '));
#if DEBUG
								if (log.IsWarnEnabled) log.Warn("Replaced CR and LF in subject " + m.Headers.Subject);
#endif
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
							gxmessage.DateReceived = GeneXus.Mail.Internals.Pop3.MailMessage.GetMessageDate(m.Headers.Date);
							AddHeader(gxmessage, "DispositionNotificationTo", m.Headers.DispositionNotificationTo.ToString());
							ProcessMailAttachments(gxmessage, m.FindAllAttachments());
						}
					}
				}
            }catch(Exception e)
			{
				LogError("Receive message error", e.Message, MailConstants.MAIL_ServerRepliedErr, e);
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
						LogError("Could not add Attachment", "Failed to save attachment", MailConstants.MAIL_InvalidAttachment, e);
					}
                }
            }
        }
       
        private static void AddHeader(GXMailMessage msg, string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                msg.Headers[key] = value;
            }
        }

        private string FixFileName(string attachDir, string name)
        {
			if (string.IsNullOrEmpty(name))
			{
				name = Path.GetRandomFileName();
			}
			if (Path.Combine(AttachDir, name).Length > 200)
			{
				name = Path.GetRandomFileName().Replace(".", "") + "." + Path.GetExtension(name);
			}
			Regex validChars = new Regex(@"[\\\/\*\?\|:<>]");
            return validChars.Replace(name, "_");
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        private static void SetRecipient(GXMailRecipientCollection gxColl, MailAddressCollection coll)
        {
            foreach (var to in coll)
            {
                gxColl.Add(new GXMailRecipient(to.DisplayName, to.Address));
            }
        }

        public void Delete(GXPOP3Session sessionInfo)
        {
            try
            {
                client.DeleteMessage(lastReadMessage);
            }
            catch (PopServerException e)
            {
                LogError("Delete message error", e.Message, MailConstants.MAIL_ServerRepliedErr, e);
            }
        }

        private void LogError(string title, string message, int code)
        {
            LogError(title, message, code, null);
        }

        private void LogError(string title, string message, int code, Exception e)
        {

#if DEBUG
            if (e != null && log.IsErrorEnabled) log.Error(message, e);
#endif
            if (_sessionInfo != null)
            {
                _sessionInfo.HandleMailException(new GXMailException(message, (short)code));
            }
        }

        private void LogDebug(string title, string message, int code)
        {
            LogDebug(title, message, code, null);
        }

        private void LogDebug(string title, string message, int code, Exception e)
        {
#if DEBUG
            if (e != null && log.IsDebugEnabled) log.Debug(message, e);
#endif
            if (_sessionInfo != null)
            {
                _sessionInfo.HandleMailException(new GXMailException(message, (short)code));
            }
        }

        private String attachDir = string.Empty;

        public string AttachDir
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

        public bool DownloadAttachments { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Timeout { get; set; }

    }
}
