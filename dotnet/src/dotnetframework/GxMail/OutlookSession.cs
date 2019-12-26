using System;
using System.IO;
using System.Collections;
using log4net;
using Outlook;
using GeneXus.Utils;

namespace GeneXus.Mail.Internals
{
	
	internal class OutlookSession
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(OutlookSession));

		private enum RecipientType
		{
			TO = 1,
			CC = 2,
			BCC = 3,
			REPLYTO = 4
		}

		private static Object optional = System.Reflection.Missing.Value;

		private short editWindow;
		private short newMessages;
		private string attachDir;
		private bool firstRead;

		private ApplicationClass session;
		private Items readItems;
		private MailItem message;

		public OutlookSession()
		{
			session = new ApplicationClass();
			editWindow = 0;
			newMessages = 1;
			attachDir = "";
			firstRead = true;

			readItems = null;
			message = null;
		}

		#region Properties
		public string AttachDir
		{
			get
			{
				return attachDir;
			}
			set
			{
				attachDir = value;
			}
		}

		public int Count
		{
			get
			{
				if(readItems == null)
				{
					return 0;
				}
				else
				{
					return readItems.Count;
				}
			}
		}

		public short EditWindow
		{
			get
			{
				return editWindow;
			}
			set
			{
				editWindow = value;
			}
		}

		public short NewMessages
		{
			get
			{
				return newMessages;
			}
			set
			{
				newMessages = value;
			}
		}
		#endregion

		#region Public Methods
		public void ChangeFolder(GXOutlookSession session, string strFolder)
		{
			try
			{
				ChangeFolder(strFolder);
			}
			catch(GXMailException exc)
			{
				session.HandleMailException(exc);
			}
		}

		public void Delete(GXOutlookSession session)
		{
			try
			{
				Delete();
			}
			catch(GXMailException exc)
			{
				session.HandleMailException(exc);
			}
		}

		public void MarkAsRead(GXOutlookSession session)
		{
			try
			{
				MarkAsRead();
			}
			catch(GXMailException exc)
			{
				session.HandleMailException(exc);
			}
		}

		public void NewAppointment(GXOutlookSession session, string subject, string location, DateTime start, DateTime end, short allDay, int reminderMinutes)
		{
			try
			{
				NewAppointment(subject, location, start, end, allDay, reminderMinutes);
			}
			catch(GXMailException exc)
			{
				session.HandleMailException(exc);
			}
		}

		public void Receive(GXOutlookSession session, GXMailMessage msg)
		{
			try
			{
				Receive(msg);
			}
			catch(GXMailException exc)
			{
				session.HandleMailException(exc);
			}
		}

		public void Send(GXOutlookSession session, GXMailMessage msg)
		{
			try
			{
				Send(msg);
			}
			catch(GXMailException exc)
			{
				session.HandleMailException(exc);
			}
		}
		#endregion

		#region Private Methods
		#region Change Folder
		private void ChangeFolder(string strFolder)
		{
			GXLogging.Debug(log,"Changing folder");
			if(session == null)
			{
				GXLogging.Error(log,"Could not start Outlook");
				throw new GXMailException("Could not start Outlook", 4);
			}

			if(newMessages < 0 || newMessages > 1)
			{
				GXLogging.Error(log,"Invalid NewMessages value");
				throw new GXMailException("Invalid NewMessages value", 27);
			}

			_NameSpace ns;
			MAPIFolder defaultFolder = null;
			object parent = null;
			MAPIFolder folder = null;

			message = null;

			try
			{
				ns = session.GetNamespace("MAPI");
				defaultFolder = ns.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
				
				if((defaultFolder != null) && !string.IsNullOrEmpty(defaultFolder.Name))
				{
					parent = defaultFolder.Parent;

					string[] foldersArr = strFolder.Split(new char[] { '\\' });
					for(int i=0; i<foldersArr.Length; i++)
					{
						if(string.IsNullOrEmpty(foldersArr[i]))
						{
							continue;
						}
						else if(foldersArr[i].Equals("."))
						{
							folder = (MAPIFolder)parent;
						}
						else if(foldersArr[i].Equals(".."))
						{
							folder = (MAPIFolder)((MAPIFolder)parent).Parent;
						}
						else if(foldersArr[i].Equals("Inbox"))
						{
							folder = ns.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
						}
						else if(foldersArr[i].Equals("Outbox"))
						{
							folder = ns.GetDefaultFolder(OlDefaultFolders.olFolderOutbox);
						}
						else if(foldersArr[i].Equals("Sent Items"))
						{
							folder = ns.GetDefaultFolder(OlDefaultFolders.olFolderSentMail);
						}
						else if(foldersArr[i].Equals("Deleted Items"))
						{
							folder = ns.GetDefaultFolder(OlDefaultFolders.olFolderDeletedItems);
						}
						else if(folder != null)
						{
							folder = GetFolder(((MAPIFolder)parent).Folders, foldersArr[i]);
							if(folder == null)
							{
								break;
							}
						}
						else
						{
							folder = GetFolder((Folders)defaultFolder.Folders, foldersArr[i]);
							if(folder == null)
							{
								break;
							}
						}
					}
				}
			}
			catch(System.Exception exc)
			{
				GXLogging.Error(log,"Could not open folder", exc);
				throw new GXMailException("Could not open folder", 5);
			}
			if(folder == null)
			{
				folder = defaultFolder;
			}
			if(folder != null)
			{
				readItems = folder.Items;
				readItems.Sort("[ReceivedTime]", false);
			}
			else
			{
				GXLogging.Error(log,"Could not open folder");
				throw new GXMailException("Could not open folder", 5);
			}
		}

		private MAPIFolder GetFolder(Folders folders, string strFolder)
		{
			for(int i=1; i<=(int)folders.Count; i++)
			{
				MAPIFolder currFolder = folders[i];
				if(string.Compare(currFolder.Name.ToString(), strFolder, true) == 0)
				{
					return currFolder;
				}
			}
			return null;
		}
		#endregion

		#region MarkAsRead, Delete, NewAppointment
		private void Delete()
		{
			GXLogging.Debug(log,"Deleting message");
			if(session == null)
			{
				GXLogging.Error(log,"Could not start Outlook");
				throw new GXMailException("Could not start Outlook", 4);
			}

			if(message != null)
			{
				try
				{
					message.Delete();
					message = null;
				}
				catch(System.Exception exc)
				{
					GXLogging.Error(log,"Error deleting message", exc);
				}
			}
			else
			{
				GXLogging.Error(log,"No current message");
				throw new GXMailException("No current message", 26);
			}
		}

		private void MarkAsRead()
		{
			GXLogging.Debug(log,"Marking as read");
			if(session == null)
			{
				GXLogging.Error(log,"Could not start Outlook");
				throw new GXMailException("Could not start Outlook", 4);
			}

			if(message != null)
			{
				try
				{
					message.UnRead = false;
				}
				catch(System.Exception exc)
				{
					GXLogging.Error(log,"Error marking as read", exc);
				}
			}
			else
			{
				GXLogging.Error(log,"No current message");
				throw new GXMailException("No current message", 26);
			}
		}

		private void NewAppointment(string subject, string location, DateTime start, DateTime end, short allDay, int reminderMinutes)
		{
			GXLogging.Debug(log,"Adding New Appointment");
			if(session == null)
			{
				GXLogging.Error(log,"Could not start Outlook");
				throw new GXMailException("Could not start Outlook", 4);
			}
			
			_AppointmentItem appoint;

			try
			{
				appoint = (_AppointmentItem)session.CreateItem(OlItemType.olAppointmentItem);
				appoint.Subject = subject;
				appoint.Location = location;
				appoint.Start = start;
				appoint.End = end;
				appoint.AllDayEvent = (allDay == 1);
				appoint.ReminderSet = (reminderMinutes > 0);
				if(appoint.ReminderSet)
				{
					appoint.ReminderMinutesBeforeStart = reminderMinutes;
				}
				if(editWindow != 1)
				{
					appoint.Save();
				}
				else
				{
					appoint.Display(false);
				}
			}
			catch(System.Exception exc)
			{
				GXLogging.Error(log,"Internal error", exc);
				throw new GXMailException("Internal error", 22);
			}
		}
		#endregion

		#region Receive Message
		private void Receive(GXMailMessage msg)
		{
			GXLogging.Debug(log,"Receiving Message");
			if(session == null)
			{
				GXLogging.Error(log,"Could not start Outlook");
				throw new GXMailException("Could not start Outlook", 4);
			}

			msg.Clear();
			message = null;

			try
			{
				if (readItems == null)
				{
					ChangeFolder("inbox");
				}

				if(newMessages == 1)
				{
					if(firstRead)
					{
						message = (MailItem)readItems.Find("[Unread] = True");
						firstRead = false;
					}
					else
					{
						message = (MailItem)readItems.GetNext();
					}
				}
				else
				{
					if(firstRead)
					{
						message = (MailItem)readItems.GetFirst();
						firstRead = false;
					}
					else
					{
						message = (MailItem)readItems.GetNext();
					}
				}
			}
			catch(System.Exception exc)
			{
				GXLogging.Error(log,"Could not receive message", exc);
				throw new GXMailException("Could not receive message", 22);
			}

			if(message != null)
			{
				CopyMessage(message, msg);
			}
			else
			{
                GXLogging.Debug(log,"No messages to receive");
                throw new NoMessagesException();
			}
		}

		private void CopyMessage(MailItem message, GXMailMessage msg)
		{
			string errorMessage = "Error reading subject";
			try
			{
				msg.Subject = message.Subject;
				errorMessage = "Error reading text";
				msg.Text = message.Body;
				msg.HTMLText = message.HTMLBody;
				errorMessage = "Error reading message dates";
				msg.DateReceived = message.ReceivedTime;
				msg.DateSent = message.SentOn;
				errorMessage = "Error reading sender";
				msg.From.Name = message.SenderName;
				msg.From.Address = GetSenderEmailAddress(message);
				errorMessage = "Error reading recipients";
				Recipients recipients = message.Recipients;
				errorMessage = "Error parsing recipients";
				CopyRecipients(recipients, msg.To, RecipientType.TO);
				CopyRecipients(recipients, msg.CC, RecipientType.CC);
				CopyRecipients(recipients, msg.BCC, RecipientType.BCC);
                CopyRecipients(message.ReplyRecipients, msg.ReplyTo, RecipientType.REPLYTO);
				errorMessage = "Error reading attachments";
				Attachments attachments = message.Attachments;
				errorMessage = "Error parsing attachments";
				CopyAttachments(attachments, msg.Attachments);
			}
			catch(System.Exception exc)
			{
				GXLogging.Error(log,errorMessage, exc);
				throw new GXMailException(errorMessage, 22);
			}
		}

		private string GetSenderEmailAddress(MailItem message)
		{
			try
			{
			    //Gets the SMTP address of the sender for Exchange. In that case message.SenderEmailAddress returns the main LDAP
				if(message.SenderEmailType.Equals("EX"))
				{
					MAPI.SessionClass mapiSess = new MAPI.SessionClass();
					mapiSess.Logon(optional, optional, false, false, optional, optional, optional);

					MAPI.Message mapiMsg = (MAPI.Message)mapiSess.GetMessage(message.EntryID, ((MAPIFolder)message.Parent).StoreID);
					MAPI.AddressEntry mapiSender = (MAPI.AddressEntry)mapiMsg.Sender;
					MAPI.Fields mapiFields = (MAPI.Fields)mapiSender.Fields;
					for(int i=1; i<=(int)mapiFields.Count; i++)
					{
						MAPI.Field mapiField = (MAPI.Field)mapiFields.get_Item(i, "");
						int id = 0;
						try
						{
							id = int.Parse(mapiField.ID.ToString());
						}
						catch(FormatException) {}
						if(id == 972947486)
						{
							return mapiField.Value.ToString();
						}
					}
				}
			}
			catch(System.Exception exc)
			{
				GXLogging.Error(log,"Error reading sender EmailAddress", exc);
			}

			return message.SenderEmailAddress;
		}

		private void CopyRecipients(Recipients fromList, GXMailRecipientCollection toList, RecipientType type)
		{
			GXLogging.Debug(log,"Copying Recipients: " + type);
			for(int i=1; i<=fromList.Count; i++)
			{
				Recipient recipient = fromList[i];
				if(recipient.Type == (int)type)
				{
					toList.Add(new GXMailRecipient(recipient.Name, recipient.Address));
				}
			}
		}

		private void CopyAttachments(Attachments fromList, GxStringCollection toList)
		{
			GXLogging.Debug(log,"Copying Attachments");
			int lastBar = 0;
			string fileName = "";
			if(string.IsNullOrEmpty(attachDir))
				attachDir = Environment.CurrentDirectory;

			for(int i=1; i<=fromList.Count; i++)
			{
				Attachment attachment = fromList[i];
				if(!string.IsNullOrEmpty(attachDir))
				{
					try
					{
						fileName = attachment.FileName;
					}
					catch(System.Exception exc)
					{
						GXLogging.Error(log,"Error reading attachment FileName", exc);
						continue;
					}

					lastBar = fileName.LastIndexOf("\\");
					if(lastBar != -1)
					{
						fileName = fileName.Substring(lastBar + 1);
					}

					char pSep = System.IO.Path.DirectorySeparatorChar;
					if(!attachDir.EndsWith(pSep.ToString()))
					{
						attachDir += pSep.ToString();
					}

					try
					{
						attachment.SaveAsFile(attachDir + fileName);
					}
					catch(System.Exception exc)
					{
						GXLogging.Error(log,"Could not save attachment", exc);
						throw new GXMailException("Could not save attachment", 16);
					}
				}
				lastBar = fileName.LastIndexOf("\\");
				if(lastBar != -1)
				{
					toList.Add(fileName.Substring(lastBar + 1));
				}
				else
				{
					toList.Add(fileName);
				}
			}
		}
		#endregion

		#region Send Message
		private void Send(GXMailMessage msg)
		{
			GXLogging.Debug(log,"Sending Message");
			if(session == null)
			{
				GXLogging.Error(log,"Could not start Outlook");
				throw new GXMailException("Could not start Outlook", 4);
			}

			if(editWindow < 0 || editWindow > 1)
			{
				GXLogging.Error(log,"Invalid EditWindow value");
				throw new GXMailException("Invalid EditWindow value", 28);
			}
			if(msg.To.Count == 0)
			{
				GXLogging.Error(log,"No main recipient specified");
				throw new GXMailException("No main recipient specified", 13);
			}

			try
			{
				MailItem newMessage = (MailItem)session.CreateItem(OlItemType.olMailItem);
				newMessage.Subject = msg.Subject;
				if(msg.Text.Length > 0)
				{
					newMessage.Body = msg.Text;
				}
				if(msg.HTMLText.Length > 0)
				{
					newMessage.HTMLBody = msg.HTMLText;
				}

				CopyRecipients(msg.To, newMessage.Recipients, RecipientType.TO);
				CopyRecipients(msg.CC, newMessage.Recipients, RecipientType.CC);
				CopyRecipients(msg.BCC, newMessage.Recipients, RecipientType.BCC);
				
				CopyAttachments(msg.Attachments, newMessage.Attachments);

				if((editWindow == 1) || (!newMessage.Recipients.ResolveAll()))
				{
					newMessage.Display(false);
				}
				else
				{
					newMessage.Send();
				}
			}
			catch(System.Exception exc)
			{
				GXLogging.Error(log,"Could not send message", exc);
				throw new GXMailException("Could not send message ("+exc.Message+")", 10);
			}
		}

		private void CopyRecipients(GXMailRecipientCollection fromList, Recipients toList, RecipientType type)
		{
			GXLogging.Debug(log,"Copying Recipients: " + type);
			foreach(GXMailRecipient recipient in fromList)
			{
				Recipient newRecipient = null;
				try
				{
					newRecipient = toList.Add(recipient.Name);
					newRecipient.Type = (int)type;
				}
				catch(System.Exception exc)
				{
					GXLogging.Error(log,"Invalid recipient " + recipient.Name, exc);
					throw new GXMailException("Invalid recipient " + recipient.Name, 14);
				}
				if(newRecipient == null)
				{
					GXLogging.Error(log,"Invalid recipient " + recipient.Name);
					throw new GXMailException("Invalid recipient " + recipient.Name, 14);
				}
			}
		}

		private void CopyAttachments(GxStringCollection fromList, Attachments toList)
		{
			GXLogging.Debug(log,"Copying Attachments");
			if(string.IsNullOrEmpty(attachDir))
				attachDir = Environment.CurrentDirectory;

			char pSep = System.IO.Path.DirectorySeparatorChar;
			if(!attachDir.EndsWith(pSep.ToString()))
			{
				attachDir += pSep.ToString();
			}

			foreach(string attach in fromList)
			{
				string fullFileName = attachDir;
				if (!Path.IsPathRooted(attach))
				{
					if(attach.StartsWith(pSep.ToString()))
					{
						fullFileName += attach.Substring(1);
					}
					else
					{
						fullFileName += attach;
					}
				}
				else
				{
					fullFileName = attach;
				}

				try
				{
					Attachment newAttach = toList.Add(fullFileName, optional, optional, attach);
				}
				catch(System.Exception exc)
				{
					GXLogging.Error(log,"Invalid attachment " + fullFileName, exc);
					throw new GXMailException("Invalid attachment " + fullFileName, 15);
				}
			}
		}
		#endregion
		#endregion
	}
}
