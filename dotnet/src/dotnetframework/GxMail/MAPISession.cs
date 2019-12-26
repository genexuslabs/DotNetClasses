using System;
using log4net;
using GeneXus.Utils;
using MAPI;

namespace GeneXus.Mail.Internals
{
	
	internal class MAPISession
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(MAPISession));

		private enum RecipientType
		{
			TO = 1,
			CC = 2,
			BCC = 3
		}

		private static Object optional = System.Reflection.Missing.Value;

		private short editWindow;
		private short newMessages;
		private string profile;
		private bool loggedIn;
		private bool inFolder;
		private string attachDir;

		private SessionClass session;
		private Messages readItems;
		private Message message;

		#region Constructor
		public MAPISession()
		{
			session = new SessionClass();
			editWindow = 0;
			newMessages = 1;
			profile = "";
			attachDir = "";
			loggedIn = false;
			inFolder = false;

			readItems = null;
			message = null;
		}
		#endregion

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
				if((readItems == null) || (!inFolder))
				{
					return 0;
				}
				else
				{
					return int.Parse(readItems.Count.ToString());
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

		public string Profile
		{
			get
			{
				return profile;
			}
			set
			{
				profile = value;
			}
		}
		#endregion

		#region Public Methods
		public void Delete(GXMAPISession session)
		{
			try
			{
				Delete();
			}
			catch(GXMailException exc)
			{
                GXLogging.Error(log, "Delete error", exc);
                session.HandleMailException(exc);
			}
		}

		public void Login(GXMAPISession session)
		{
			try
			{
				Login();
			}
			catch(GXMailException exc)
			{
                GXLogging.Error(log,"Login error", exc);
                session.HandleMailException(exc);
			}
		}

		public void Logout(GXMAPISession session)
		{
			try
			{
				Logout();
			}
			catch(GXMailException exc)
			{
                GXLogging.Error(log,"Logout error", exc);
                session.HandleMailException(exc);
			}
		}

		public void ChangeFolder(GXMAPISession session, string strFolder)
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

		public void MarkAsRead(GXMAPISession session)
		{
			try
			{
				MarkAsRead();
			}
			catch(GXMailException exc)
			{
                GXLogging.Error(log,"MarkAsRead error", exc);
                session.HandleMailException(exc);
			}
		}

		public void Receive(GXMAPISession session, GXMailMessage msg)
		{
			try
			{
				Receive(msg);
			}
			catch(GXMailException exc)
			{
                GXLogging.Error(log,"Receive error", exc);
                session.HandleMailException(exc);
			}
		}

		public void Send(GXMAPISession session, GXMailMessage msg)
		{
			try
			{
				Send(msg);
			}
			catch(GXMailException exc)
			{
                GXLogging.Error(log,"Send error", exc);
                session.HandleMailException(exc);
			}
		}
		#endregion

		#region Private Methods
		#region MarkAsRead, Delete
		private void MarkAsRead()
		{
			GXLogging.Debug(log,"Marking as read");
			if(message != null)
			{
				try
				{
					message.Unread = true;
					message.Update(optional, optional);
				}
				catch(Exception exc)
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

		private void Delete()
		{
			GXLogging.Debug(log,"Deleting message");
			if(message != null)
			{
				try
				{
					message.Delete(true);
					message = null;
				}
				catch(Exception exc)
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
		#endregion

		#region Change Folder
		private void ChangeFolder(string strFolder)
		{
			GXLogging.Debug(log,"Changing folder");
			if(newMessages < 0 || newMessages > 1)
			{
				GXLogging.Error(log,"Invalid NewMessages value");
				throw new GXMailException("Invalid NewMessages value", 27);
			}

			Folder defaultFolder = null;
			Folder parentFolder = null;
			Folder folder = null;
			bool foundFolder = false;

			inFolder = false;
			message = null;

			if(loggedIn)
			{
				try
				{
					defaultFolder = (Folder)session.GetDefaultFolder(CdoDefaultFolderTypes.CdoDefaultFolderInbox);
					
					if((defaultFolder != null) && !defaultFolder.Name.Equals(""))
					{
						parentFolder = GetParentFolder(defaultFolder, ref foundFolder);

						string[] foldersArr = strFolder.Split(new char[] { '\\' });
						for(int i=0; i<foldersArr.Length; i++)
						{
							if(string.IsNullOrEmpty(foldersArr[i]))
							{
								continue;
							}
							else if(foldersArr[i].Equals("."))
							{
								if(foundFolder)
								{
									folder = parentFolder;
								}
							}
							else if(foldersArr[i].Equals(".."))
							{
								if(!foundFolder)
								{
									throw new GXMailException("Could not open folder " + strFolder, 5);
								}
								else
								{
									foundFolder = false;
									folder = GetParentFolder(parentFolder, ref foundFolder);
								}
							}
							else if(foldersArr[i].Equals("Inbox"))
							{
								folder = (Folder)session.GetDefaultFolder(CdoDefaultFolderTypes.CdoDefaultFolderInbox);
							}
							else if(foldersArr[i].Equals("Outbox"))
							{
								folder = (Folder)session.GetDefaultFolder(CdoDefaultFolderTypes.CdoDefaultFolderOutbox);
							}
							else if(foldersArr[i].Equals("Sent Items"))
							{
								folder = (Folder)session.GetDefaultFolder(CdoDefaultFolderTypes.CdoDefaultFolderSentItems);
							}
							else if(foldersArr[i].Equals("Deleted Items"))
							{
								folder = (Folder)session.GetDefaultFolder(CdoDefaultFolderTypes.CdoDefaultFolderDeletedItems);
							}
							else if(folder != null)
							{
								foundFolder = false;
								folder = GetFolder((Folders)folder.Folders, foldersArr[i], ref foundFolder);
								if(!foundFolder)
								{
									break;
								}
							}
							else
							{
								foundFolder = false;
								folder = GetFolder((Folders)parentFolder.Folders, foldersArr[i], ref foundFolder);
								if(!foundFolder)
								{
									break;
								}
							}
						}
					}
				}
				catch(Exception exc)
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
					inFolder = true;
					readItems = (Messages)folder.Messages;
					if(newMessages == 1)
					{
						MessageFilter filter = (MessageFilter)readItems.Filter;
						filter.Unread = newMessages;
					}
					readItems.Sort(optional, optional);
				}
				else
				{
					GXLogging.Error(log,"Could not open folder");
					throw new GXMailException("Could not open folder", 5);
				}
			}
			else
			{
				GXLogging.Error(log,"Not logged in");
				throw new GXMailException("Not logged in", 2);
			}
		}

		private Folder GetParentFolder(Folder folder, ref bool parentFound)
		{
			Folder parentFolder = null;

			string storeID = folder.StoreID.ToString();
			object parent = folder.Parent;
			if(parent is SessionClass)
			{
				InfoStore iis = (InfoStore)session.GetInfoStore(storeID);
				parentFolder = (Folder)iis.RootFolder;
				parentFound = true;
			}
			else if(parent is Folders)
			{
				parentFolder = (Folder)((Folders)parent).Parent;
				parentFound = true;
			}
			else if(parent is InfoStore)
			{
				parentFolder = (Folder)((InfoStore)parent).RootFolder;
				parentFound = true;
			}
			else
			{
				parentFolder = folder;
				parentFound = true;
			}

			return parentFolder;
		}

		private Folder GetFolder(Folders folders, string strFolder, ref bool parentFound)
		{
			for(int i=1; i<=(int)folders.Count; i++)
			{
				Folder currFolder = (Folder)folders.get_Item(i);
				if(string.Compare(currFolder.Name.ToString(), strFolder, true) == 0)
				{
					parentFound = true;
					return currFolder;
				}
			}
			parentFound = false;
			return null;
		}
		#endregion

		#region Login, Logout
		private void Login()
		{
			GXLogging.Debug(log,"Logging in");
			if(editWindow < 0 || editWindow > 1)
			{
				GXLogging.Error(log,"Invalid EditWindow value");
				throw new GXMailException("Invalid EditWindow value", 28);
			}
			if(newMessages < 0 || newMessages > 1)
			{
				GXLogging.Error(log,"Invalid NewMessages value");
				throw new GXMailException("Invalid NewMessages value", 27);
			}

			if(!loggedIn)
			{
				try
				{
					session.Logon(profile, optional, optional, optional, optional, optional, optional);
					loggedIn = true;
				}
				catch(Exception exc)
				{
					GXLogging.Error(log,"Could not complete login", exc);
					throw new GXMailException("Could not complete login", 3);
				}

				ChangeFolder("");
			}
			else
			{
				GXLogging.Error(log,"Already logged in");
				throw new GXMailException("Already logged in", 1);
			}
		}

		private void Logout()
		{
			GXLogging.Debug(log,"Logging out");
			if(loggedIn)
			{
				try
				{
					readItems = null;
					session.Logoff();
					loggedIn = false;
				}
				catch(Exception exc)
				{
					GXLogging.Error(log,"Error Logging out", exc);
				}
			}
			else
			{
				GXLogging.Error(log,"Not logged in");
				throw new GXMailException("Not logged in", 2);
			}
		}
		#endregion

		#region Receive Message
		private void Receive(GXMailMessage msg)
		{
			GXLogging.Debug(log,"Receiving Message");
			msg.Clear();
			message = null;

			if(loggedIn)
			{
				if(inFolder)
				{
					string errorMessage = "General error";
					try
					{
						errorMessage = "Couldn't read item";
						if(newMessages == 1)
						{
							message = (Message)readItems.GetNext();
							while((message != null) && !((bool)message.Unread))
							{
								message = (Message)readItems.GetNext();
							}
						}
						else
						{
							message = (Message)readItems.GetNext();
						}
						errorMessage = "Internal error";
						if(message == null)
						{
							GXLogging.Debug(log,"No messages to receive");
                            throw new NoMessagesException();
						}
						
						CopyMessage(message, msg, ref errorMessage);
					}
					catch(Exception exc)
					{
						GXLogging.Error(log,errorMessage, exc);
						throw new GXMailException(errorMessage, 22);
					}
				}
				else
				{
                    GXLogging.Debug(log,"No messages to receive");
                    throw new NoMessagesException();
				}
			}
			else
			{
				GXLogging.Error(log,"Not logged in");
				throw new GXMailException("Not logged in", 2);
			}
		}

		private void CopyMessage(Message message, GXMailMessage msg, ref string errorMessage)
		{
			AddressEntry	sender;
			Recipients		recipients;
			Attachments		attachments;

			errorMessage = "Error reading subject";
			msg.Subject = message.Subject.ToString();
			errorMessage = "Error reading text";
			msg.Text = message.Text.ToString();
			errorMessage = "Error reading message dates";
			msg.DateReceived = (DateTime)message.TimeReceived;
			msg.DateSent = (DateTime)message.TimeSent;
			errorMessage = "Error reading sender";
			sender = (AddressEntry)message.Sender;
			errorMessage = "Error parsing sender";
			msg.From.Name = sender.Name.ToString();
			msg.From.Address = sender.Address.ToString();
			errorMessage = "Error reading recipients";
			recipients = (Recipients)message.Recipients;
			errorMessage = "Error parsing recipients";
			CopyRecipients(recipients, msg.To, RecipientType.TO);
			CopyRecipients(recipients, msg.CC, RecipientType.CC);
			CopyRecipients(recipients, msg.BCC, RecipientType.BCC);
			errorMessage = "Error reading attachments";
			attachments = (Attachments)message.Attachments;
			errorMessage = "Error parsing attachments";
			CopyAttachments(attachments, msg.Attachments);
		}

		private void CopyRecipients(Recipients fromList, GXMailRecipientCollection toList, RecipientType type)
		{
			GXLogging.Debug(log,"Copying Recipients: " + type);
			for(int i=1; i<=(int)fromList.Count; i++)
			{
				Recipient recipient = (Recipient)fromList.get_Item(i);
				if(((int)recipient.Type) == (int)type)
				{
					toList.Add(new GXMailRecipient(recipient.Name.ToString(), recipient.Address.ToString()));
				}
			}
		}

		private void CopyAttachments(Attachments fromList, GxStringCollection toList)
		{
			GXLogging.Debug(log,"Copying Attachments");
			int lastBar = 0;
			string fileName = "";
			Fields	fields;
			Field	field;

			for(int i=1; i<=(int)fromList.Count; i++)
			{
				Attachment attachment = (Attachment)fromList.get_Item(i);
				if(!string.IsNullOrEmpty(attachDir))
				{
					toList.Add(attachment.Name.ToString());
					fields = (Fields)attachment.Fields;

					// Long filename
					try
					{// 0x3707001E = PR_ATTACH_LONG_FILENAME
						field = (Field)fields.get_Item(0x3707001E, optional);
						fileName = field.Value.ToString();
					}
					catch(Exception ) {}

					if(string.IsNullOrEmpty(fileName))
					{// Long filename - UNICODE
						try
						{// 0x3707001E + 1 = PR_ATTACH_LONG_FILENAME
							field = (Field)fields.get_Item(0x3707001E + 1, optional);
							fileName = field.Value.ToString();
						}
						catch(Exception ) {}
					}

					if(string.IsNullOrEmpty(fileName))
					{// Short filename
						try
						{// 0x3704001E = PR_ATTACH_LONG_FILENAME
							field = (Field)fields.get_Item(0x3704001E + 1, optional);
							fileName = field.Value.ToString();
						}
						catch(Exception ) {}
					}

					if(string.IsNullOrEmpty(fileName))
					{// Short filename - UNICODE
						try
						{// 0x3704001E + 1 = PR_ATTACH_LONG_FILENAME
							field = (Field)fields.get_Item(0x3704001E + 1, optional);
							fileName = field.Value.ToString();
						}
						catch(Exception ) {}
					}

					if(string.IsNullOrEmpty(fileName))
					{
						fileName = attachment.Source.ToString();
					}

					if(string.IsNullOrEmpty(fileName))
					{
						fileName = attachment.Name.ToString();
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
						attachment.WriteToFile(attachDir + fileName);
					}
					catch(Exception exc)
					{
						GXLogging.Error(log,"Could not save attachment", exc);
						throw new GXMailException("Could not save attachment", 16);
					}
				}
				if(string.IsNullOrEmpty(fileName))
				{
					fileName = attachment.Name.ToString();
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
			if(loggedIn)
			{
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

				Folder outbox = null;

				try
				{
					outbox = (Folder)session.Outbox;
				}
				catch(Exception exc)
				{
					GXLogging.Error(log,"Not logged in", exc);
					throw new GXMailException("Not logged in", 2);
				}

				if(outbox != null)
				{
					try
					{
						Messages messages = (Messages)outbox.Messages;
						Message newMessage = (Message)messages.Add(msg.Subject, msg.Text, optional, optional);

						CopyRecipients(msg.To, (Recipients)newMessage.Recipients, RecipientType.TO);
						CopyRecipients(msg.CC, (Recipients)newMessage.Recipients, RecipientType.CC);
						CopyRecipients(msg.BCC, (Recipients)newMessage.Recipients, RecipientType.BCC);

						CopyAttachments(msg.Attachments, (Attachments)newMessage.Attachments);

						newMessage.Send(optional, (bool)(editWindow == 1), optional);
						session.DeliverNow();
					}
					catch(Exception exc)
					{
						GXLogging.Error(log,"Could not send message", exc);
						throw new GXMailException("Could not send message", 10);
					}
				}
			}
			else
			{
				GXLogging.Error(log,"Not logged in");
				throw new GXMailException("Not logged in", 2);
			}
		}

		private void CopyRecipients(GXMailRecipientCollection fromList, Recipients toList, RecipientType type)
		{
			GXLogging.Debug(log,"Copying Recipients: " + type);
			foreach(GXMailRecipient recipient in fromList)
			{
				try
				{
					Recipient newRecipient = (Recipient)toList.Add(recipient.Name, optional, (int)type, optional);
					newRecipient.Resolve(optional);
				}
				catch(Exception exc)
				{
					GXLogging.Error(log,"Invalid recipient " + recipient.Name, exc);
					throw new GXMailException("Invalid recipient " + recipient.Name, 14);
				}
			}
		}

		private void CopyAttachments(GxStringCollection fromList, Attachments toList)
		{
			GXLogging.Debug(log,"Copying Attachments");
			char pSep = System.IO.Path.DirectorySeparatorChar;
			if(!attachDir.EndsWith(pSep.ToString()))
			{
				attachDir += pSep.ToString();
			}

			foreach(string attach in fromList)
			{
				string fullFileName = attachDir;
				if(attach.StartsWith(pSep.ToString()))
				{
					fullFileName += attach.Substring(1);
				}
				else
				{
					fullFileName += attach;
				}

				try
				{
					Attachment newAttach = (Attachment)toList.Add(attach, 0, optional, fullFileName);
					newAttach.ReadFromFile(fullFileName);
				}
				catch(Exception exc)
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
