using log4net;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace GeneXus.Mail.Exchange
{
	public class ExchangeSession : IMailService
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(ExchangeSession));
		private string _attachDir = string.Empty;
		private string _userName = string.Empty;
		private string _password = string.Empty;
		private int _timeout = 0;
		private bool _fetchOnlyNewMessages = true;
		private int _lastMailCount = 0;

		private ExchangeService _service = null;
		private FindItemsResults<Item> _allMail = null;
		private int lastReadMailidx;
		private GXMailServiceSession _session;

		private string _serverUrl = string.Empty;

		private Folder _currentFolder;
		private ExchangeVersion _version = ExchangeVersion.Exchange2007_SP1;

		// OAuth Support
		private string _appId;
		private string _clientSecret;
		private string _tenantId;
		private AuthenticationType _authenticationType = Exchange.AuthenticationType.Basic;

		public const string TenantIdProperty = "TenantId";
		public const string ClientSecretProperty = "ClientSecret";
		public const string AppIdProperty = "AppId";
		public const string AuthenticationType = "AuthenticationType";


		private static PropertySet MailProps = new PropertySet(BasePropertySet.IdOnly,
																				 EmailMessageSchema.InternetMessageId, // Message-ID
																				 EmailMessageSchema.From, // From
																				 EmailMessageSchema.ToRecipients, // To
																				 EmailMessageSchema.CcRecipients, // Cc                                                                                                                                                           
																				 EmailMessageSchema.Subject, // Subject
																				 EmailMessageSchema.DateTimeSent, // Date
																				 EmailMessageSchema.DateTimeReceived, // Date
																				 EmailMessageSchema.Sender,
																				 EmailMessageSchema.Body,
																				 //EmailMessageSchema.NormalizedBody,
																				 EmailMessageSchema.Attachments); // Sender

		private static PropertySet MailHeaders = new PropertySet(BasePropertySet.IdOnly, EmailMessageSchema.InternetMessageHeaders);

		public ExchangeSession()
		{

		}

		public void SetProperty(String key, String value)
		{
			switch (key)
			{
				case AuthenticationType:
					if (Enum.TryParse(value, out AuthenticationType authType))
					{
						_authenticationType = authType;
					}
					break;
				case ClientSecretProperty:
					_clientSecret = value;
					break;
				case AppIdProperty:
					_appId = value;
					break;
				case TenantIdProperty:
					_tenantId = value;
					break;
				case "ExchangeVersion":
					switch (value)
					{
						case "Exchange2007_SP1":
							_version = ExchangeVersion.Exchange2007_SP1;
							break;
						case "Exchange2010":
							_version = ExchangeVersion.Exchange2010;
							break;
						case "Exchange2010_SP1":
							_version = ExchangeVersion.Exchange2010_SP1;
							break;
						case "Exchange2010_SP2":
							_version = ExchangeVersion.Exchange2010_SP2;
							break;
						case "Exchange2013":
							_version = ExchangeVersion.Exchange2013;
							break;
						case "Exchange2013_SP1":
							_version = ExchangeVersion.Exchange2013_SP1;
							break;
						default:
							break;
					}
					break;
				default:
					break;
			}
		}

		public void Login(GXMailServiceSession sessionInfo)
		{
			if (_authenticationType == Exchange.AuthenticationType.Basic && (string.IsNullOrEmpty(_userName) || string.IsNullOrEmpty(_password)))
			{
				throw new BadCredentialsException();
			}

			_session = sessionInfo;

			UserData userConfig = new UserData()
			{
				EmailAddress = _userName,
				Password = _password,
				Version = _version
			};
			if (!string.IsNullOrEmpty(sessionInfo.ServerUrl))
			{
				userConfig.AutodiscoverUrl = new Uri(sessionInfo.ServerUrl);
			}

			try
			{
				System.Threading.Tasks.Task<AuthenticationResult> authTask = null;
				_service = new ExchangeService();
				_service.Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx");

				var pcaOptions = new PublicClientApplicationOptions
				{
					ClientId = _appId,
					TenantId = _tenantId
				};

				var pca = PublicClientApplicationBuilder
						.CreateWithApplicationOptions(pcaOptions).Build();

				string accessToken = null;

				switch (_authenticationType)
				{
					case Exchange.AuthenticationType.OAuthDelegated:
						_service.ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, userConfig.EmailAddress);
						_service.HttpHeaders.Add("X-AnchorMailbox", userConfig.EmailAddress);
						accessToken = _password; //Access Token is the Password										
						break;

					case Exchange.AuthenticationType.OAuthApplication:
						string[] ewsScopes = new string[] { "https://outlook.office365.com/.default" };
						_service.ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, userConfig.EmailAddress);
						_service.HttpHeaders.Add("X-AnchorMailbox", userConfig.EmailAddress);

						var cca = ConfidentialClientApplicationBuilder
						.Create(_appId)
						.WithClientSecret(_clientSecret)
						.WithTenantId(_tenantId)
						.Build();

						//Make the token request
						authTask = cca.AcquireTokenForClient(ewsScopes).ExecuteAsync();
						break;

					case Exchange.AuthenticationType.OAuthDelegatedInteractive:
						authTask = pca.AcquireTokenInteractive(new string[] { "https://outlook.office365.com/EWS.AccessAsUser.All" }).ExecuteAsync();
						break;
					case Exchange.AuthenticationType.Basic:
					default:
						var cred = new NetworkCredential(_userName, _password);
						authTask = pca.AcquireTokenByUsernamePassword(new string[] { "https://outlook.office365.com/EWS.AccessAsUser.All" }, cred.UserName, cred.SecurePassword).ExecuteAsync();
						break;
				}

				if (authTask != null)
				{
					authTask.Wait();					
					accessToken = authTask.Result.AccessToken;
				}

				_service.Credentials = new OAuthCredentials(accessToken);

				UpdateMailCount();
			}
			catch (Exception e)
			{
				log.Error("Exchange Login Error", e);
				HandleError(e, 3);				
			}
		}

		public void Logout(GXMailServiceSession sessionInfo)
		{
			_allMail = null;
			_service = null;
		}

		public void Skip(GXMailServiceSession sessionInfo)
		{
			if (_service == null)
			{
				HandleError(2);
				return;
			}
			lastReadMailidx++;
		}

		public string GetNextUID(GXMailServiceSession session)
		{
			if (_service == null)
			{
				HandleError(2);
				return string.Empty;
			}

			EmailMessage msg = GetEmailAtIndex(lastReadMailidx);
			if (msg != null)
			{
				return msg.InternetMessageId.ToString();
			}
			return string.Empty;
		}

		public void Send(GXMailServiceSession sessionInfo, GXMailMessage gxmessage)
		{
			if (_service == null)
			{
				HandleError(2);
				return;
			}
			bool anyError = false;

			string fromAddress = (!string.IsNullOrEmpty(gxmessage.From.Address)) ? gxmessage.From.Address : _userName;

			EmailMessage email = new EmailMessage(_service);

			email.From = new EmailAddress(gxmessage.From.Name, fromAddress);

			SetRecipient(email.ToRecipients, gxmessage.To);
			SetRecipient(email.CcRecipients, gxmessage.CC);
			SetRecipient(email.BccRecipients, gxmessage.BCC);
			SetRecipient(email.ReplyTo, gxmessage.ReplyTo);

			email.Subject = gxmessage.Subject;
			if (string.IsNullOrEmpty(gxmessage.HTMLText))
			{
				email.Body = new MessageBody(BodyType.Text, gxmessage.Text);
			}
			else
			{
				email.Body = new MessageBody(BodyType.HTML, gxmessage.HTMLText);
			}

			foreach (string attach in gxmessage.Attachments)
			{
				string attachFilePath = attach.Trim();
				if (Path.IsPathRooted(attachFilePath))
				{
					attachFilePath = Path.Combine(_attachDir, attach);
				}
				try
				{
					email.Attachments.AddFileAttachment(attachFilePath);
				}
				catch (FileNotFoundException)
				{
					anyError = true;
					sessionInfo.HandleMailException(new GXMailException("Can't find " + attachFilePath, GXInternetConstants.MAIL_InvalidAttachment));
				}
				catch (Exception e)
				{
					anyError = true;
					sessionInfo.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_InvalidAttachment));
				}
			}

			if (!anyError)
			{
				try
				{
					email.SendAndSaveCopy(WellKnownFolderName.SentItems);
				}
				catch (Exception e)
				{
					sessionInfo.HandleMailException(new GXMailException(e.Message, MailConstants.MAIL_MessageNotSent));
				}
			}
		}



		public void Receive(GXMailServiceSession sessionInfo, GXMailMessage gxmessage)
		{
			if (_service == null)
			{
				HandleError(2);
				return;
			}
			if (lastReadMailidx == _lastMailCount)
			{
				HandleError(5);
				return;
			}

			if (_allMail == null || _lastMailCount == 0) //First Time
			{
				UpdateMailCount();
				SearchFilter sf = null;
				if (_fetchOnlyNewMessages)
				{
					sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));
				}
				ItemView view = new ItemView(Math.Max(_lastMailCount, 1));

				_allMail = FindItemsCurrentFolder(sf, view);
			}

			EmailMessage msg;

			// Verify that the item is an email message.
			if (lastReadMailidx < _lastMailCount && _allMail.Items[lastReadMailidx] is EmailMessage)
			{
				msg = GetEmailAtIndex(lastReadMailidx);

				lastReadMailidx++;
				FetchEntireMessage(msg, gxmessage);
			}

		}

		private void FetchEntireMessage(EmailMessage msg, GXMailMessage gxmessage)
		{
			// Load the schematized Internet message headers into the corresponding EmailMessage properties.
			// This results in a GetItem operation call to EWS.
			if (FetchMessage(msg, MailProps))
			{
				gxmessage.MessageId = msg.InternetMessageId;
				gxmessage.From = new GXMailRecipient(msg.From.Name, msg.From.Address);
				SetRecipient(gxmessage.To, msg.ToRecipients);
				SetRecipient(gxmessage.CC, msg.CcRecipients);
				SetRecipient(gxmessage.ReplyTo, msg.ReplyTo);

				gxmessage.DateReceived = msg.DateTimeReceived;
				gxmessage.DateSent = msg.DateTimeSent;
				gxmessage.Subject = msg.Subject;

				if (msg.Body.BodyType == BodyType.HTML)
					gxmessage.HTMLText = msg.Body;
				else
					gxmessage.Text = msg.Body;

				if (!string.IsNullOrEmpty(_attachDir))
				{
					foreach (var attach in msg.Attachments)
					{
						if (attach is FileAttachment)
						{
							FileAttachment fileAttachment = attach as FileAttachment;
							string attachFileName = fileAttachment.Name;
							string filePath = System.IO.Path.Combine(_attachDir, attachFileName);
							fileAttachment.Load(filePath);
							gxmessage.Attachments.addNew(attachFileName);
						}
					}
				}
				if (FetchMessage(msg, MailHeaders) && msg.InternetMessageHeaders != null)
				{
					foreach (var msgHeader in msg.InternetMessageHeaders)
					{
						gxmessage.AddHeader(msgHeader.Name, msgHeader.Value);
					}
				}
			}
		}
		private Folder CurrentFolder
		{
			get
			{
				if (_currentFolder == null)
					return Folder.Bind(_service, WellKnownFolderName.Inbox);
				else
					return _currentFolder;
			}
		}

		private void UpdateMailCount()
		{
			if (_fetchOnlyNewMessages)
				_lastMailCount = CurrentFolder.UnreadCount;
			else
				_lastMailCount = CurrentFolder.TotalCount;
		}

		private EmailMessage GetEmailAtIndex(int mailIdx)
		{
			EmailMessage msg = null;
			if (mailIdx < _allMail.Items.Count)
			{
				// Cast the item to an email message.
				msg = _allMail.Items[mailIdx] as EmailMessage;
			}
			return msg;
		}

		private EmailMessage GetEmailWithId(string InternetMessageId)
		{
			if (!string.IsNullOrEmpty(InternetMessageId))
			{
				SearchFilter sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And, new SearchFilter.IsEqualTo(EmailMessageSchema.InternetMessageId, InternetMessageId));
				return (EmailMessage)FindItemsCurrentFolder(sf, new ItemView(1)).FirstOrDefault() ?? null;
			}
			return null;
		}

		private FindItemsResults<Item> FindItemsCurrentFolder(SearchFilter sf, ItemView itemView)
		{
			if (_currentFolder == null)
				return _service.FindItems(WellKnownFolderName.Inbox, sf, itemView);
			else
				return _service.FindItems(_currentFolder.Id, sf, itemView);
		}

		private bool FetchMessage(EmailMessage msg, PropertySet ptySet)
		{
			try
			{
				msg.Load(ptySet);
			}
			catch (Exception e)
			{
				HandleError(e, 1);
				return false;
			}
			return true;
		}

		private void HandleError(short errCode)
		{
			HandleError(null, errCode);
		}

		private void HandleError(Exception e, short errCode)
		{
			switch (errCode)
			{
				case 1:
					_session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_ErrorReceivingMessage));
					break;
				case 2:
					_session.HandleMailException(new GXMailException("Must login before sending message", GXInternetConstants.MAIL_CantLogin));
					break;
				case 3:
					_session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_AuthenticationError));
					break;
				case 5:
					_session.HandleMailException(new GXMailException("No new messages", GXInternetConstants.MAIL_NoMessages));
					break;
				case 6:
					_session.HandleMailException(new GXMailException("Folder not found", GXInternetConstants.MAIL_CantOpenFolder));
					break;
				case 7:
					_session.HandleMailException(new GXMailException("Mail Message with Id not found", GXInternetConstants.MAIL_EmailId_NotFound));
					break;

				default:
					_session.HandleMailException(new GXMailException(e.Message, GXInternetConstants.MAIL_MessageNotSent));
					break;
			}
		}

		private void SetRecipient(EmailAddressCollection emailAddressCollection, GXMailRecipientCollection gXMailRecipientCollection)
		{
			foreach (GXMailRecipient to in gXMailRecipientCollection)
			{
				emailAddressCollection.Add(new EmailAddress(to.Name, to.Address));
			}
		}

		private void SetRecipient(GXMailRecipientCollection gXMailRecipientCollection, EmailAddressCollection emailAddressCollection)
		{
			foreach (var to in emailAddressCollection)
			{
				gXMailRecipientCollection.Add(new GXMailRecipient(to.Name, to.Address));
			}
		}

		public void Delete(GXMailServiceSession sessionInfo, GXMailMessage gxmessage)
		{
			if (_service == null)
			{
				HandleError(2);
				return;
			}

			EmailMessage msg = GetEmailWithId(gxmessage.MessageId);
			if (msg != null)
			{
				msg.Delete(DeleteMode.MoveToDeletedItems);
			}
		}

		public short MarkAs(GXMailServiceSession sessionInfo, GXMailMessage gxmessage, bool isRead)
		{
			if (_service == null)
			{
				HandleError(2);
				return 0;
			}

			EmailMessage msg = GetEmailWithId(gxmessage.MessageId);
			if (msg != null)
			{
				msg.IsRead = isRead;
				msg.Update(ConflictResolutionMode.AutoResolve);
				return 0;
			}
			return 1;
		}

		public void ChangeFolder(GXMailServiceSession sessionInfo, string folder)
		{
			if (_service == null)
			{
				HandleError(2);
				return;
			}
			if (string.IsNullOrEmpty(folder))
			{
				_currentFolder = null;
				return;
			}

			try
			{
				_currentFolder = GetFolderByPath(folder);
			}
			catch (Exception)
			{
				HandleError(6);
			}
		}

		public Folder GetFolderByPath(string ewsFolderPath)
		{
			string[] folders = ewsFolderPath.Split('\\');

			Folder parentFolderId = null;
			Folder actualFolder = null;

			for (int i = 0; i < folders.Length; i++)
			{
				if (0 == i)
				{
					parentFolderId = GetTopLevelFolder(folders[i]);
					actualFolder = parentFolderId;
				}
				else
				{
					actualFolder = GetFolder(parentFolderId.Id, folders[i]);
					parentFolderId = actualFolder;
				}
			}
			return actualFolder;

		}

		private Folder GetTopLevelFolder(string folderName)
		{
			FindFoldersResults findFolderResults = _service.FindFolders(WellKnownFolderName.MsgFolderRoot, new FolderView(int.MaxValue));
			foreach (Folder folder in findFolderResults.Where(folder => folderName.Equals(folder.DisplayName, StringComparison.InvariantCultureIgnoreCase)))
				return folder;

			throw new Exception("Top Level Folder not found: " + folderName);
		}

		private Folder GetFolder(FolderId parentFolderId, string folderName)
		{
			FindFoldersResults findFolderResults = _service.FindFolders(parentFolderId, new FolderView(int.MaxValue));
			foreach (Folder folder in findFolderResults.Where(folder => folderName.Equals(folder.DisplayName, StringComparison.InvariantCultureIgnoreCase)))
				return folder;

			throw new Exception("Folder not found: " + folderName);

		}

		public string AttachDir
		{
			get
			{
				return _attachDir;
			}
			set
			{
				_attachDir = value;
			}
		}

		public int Count
		{
			get
			{
				return _lastMailCount;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string ServerUrl
		{
			get
			{
				return _serverUrl;
			}
			set
			{
				_serverUrl = value;
			}
		}

		public int Port
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string UserName
		{
			get
			{
				return _userName;
			}
			set
			{
				_userName = value;
			}
		}

		public string Password
		{
			get
			{
				return _password;
			}
			set
			{
				_password = value;
			}
		}

		public int Timeout
		{
			get
			{
				return _timeout;
			}
			set
			{
				_timeout = value;
			}
		}

		public bool NewMessages
		{
			get
			{
				return _fetchOnlyNewMessages;
			}
			set
			{
				_fetchOnlyNewMessages = value;

			}
		}

		public void GetMailMessage(GXMailServiceSession sessionInfo, string MsgId, bool dwnEntireMsg, GXMailMessage gxmessage)
		{
			if (_service == null)
			{
				HandleError(2);
				return;
			}

			EmailMessage msg = GetEmailWithId(MsgId);
			if (msg != null)
			{
				if (dwnEntireMsg)
				{
					FetchEntireMessage(msg, gxmessage);
				}
				else
				{
					gxmessage.MessageId = MsgId;
					gxmessage.Subject = msg.Subject;
				}
			}
			else
			{
				HandleError(7);
			}
		}
	}
	public enum AuthenticationType
	{
		Basic,
		OAuthDelegated,
		OAuthDelegatedInteractive,
		OAuthApplication
	}
}
