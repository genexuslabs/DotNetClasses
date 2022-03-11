using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using GeneXus.Mail.Internals.Pop3;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Certificates;
using System.Linq;

namespace GeneXus.Mail.Internals
{
	
    internal class POP3Session : Pop3SessionBase
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(POP3Session));
		
		private const string CRLF = "\r\n";

		private string userName;
		private string password;
		private string attachDir;
		private string host;
		private int timeout;
		private int port;
		private int count;
		private bool newMessages;
		private bool secureConnection;

		private int lastReadMessage;
		private int readerTimeout;

		private SecureSocket connection;
		private MailReader mailReader;

		public POP3Session()
		{
			userName = string.Empty;
            password = string.Empty;
            attachDir = string.Empty;
            host = string.Empty;
			port = 110;
			timeout = 3000;
			newMessages = true;
			secureConnection = false;
			count = 0;
			readerTimeout = -1;			
		}

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

        public override int GetMessageCount()
		{
			return count;
		}

		public override string Host
		{
			get
			{
				return host;
			}
			set
			{
				host = value;
			}
		}

		public override string UserName
		{
			get
			{
				return userName;
			}
			set
			{
				userName = value;
			}
		}

		public override string Password
		{
            get
            {
                return password;
            }
            set
			{
				password = value;
			}
		}

		public override int Port
		{
			get
			{
				return port;
			}
			set
			{
				port = value;
			}
		}

		public override int Timeout
		{
			get
			{
				return timeout;
			}
			set
			{
				timeout = value * 1000;
				readerTimeout = timeout;
			}
		}

		#region Public Methods
		public override void Delete(GXPOP3Session session)
		{
			try
			{
				Delete();	
			}
			catch(GXMailException exc)
			{
#if DEBUG
                GXLogging.Error(log,"Delete error", exc);
#endif
                session.HandleMailException(exc);
			}
		}

		public override string GetNextUID(GXPOP3Session session)
		{
            string nextUID = string.Empty;
			try
			{
				GetNextUID(ref nextUID);	
			}
            catch (NoMessagesException exc)
            {
#if DEBUG
                GXLogging.Debug(log,"Receive error", exc);
#endif
                session.HandleMailException(exc);
            }
			catch(GXMailException exc)
			{
#if DEBUG
                GXLogging.Error(log,"GetNextUID error", exc);
#endif				
                session.HandleMailException(exc);
			}
            return nextUID;
		}

		public override void Login(GXPOP3Session session)
		{
			secureConnection = (session.Secure == 1);
			newMessages = (session.NewMessages == 1);

			try
			{
				ConnectAndLogin();	
			}
			catch(GXMailException exc)
			{
#if DEBUG
                GXLogging.Error(log,"Login error", exc);
#endif
                session.HandleMailException(exc);
			}
		}

		public override void Logout(GXPOP3Session session)
		{
			try
			{
				Logout();	
			}
			catch(GXMailException exc)
			{
#if DEBUG
                GXLogging.Error(log,"Logout error", exc);
#endif
                session.HandleMailException(exc);
			}
		}

		public override void Receive(GXPOP3Session session, GXMailMessage msg)
		{
            try
            {
                Receive(msg);
            }
            catch (NoMessagesException exc)
            {
#if DEBUG
                GXLogging.Debug(log,"Receive error", exc);
#endif
                session.HandleMailException(exc);
            }
            catch (GXMailException exc)
            {
#if DEBUG
                GXLogging.Error(log,"Receive error", exc);
#endif
                session.HandleMailException(exc);
            }
		}

		public override void Skip(GXPOP3Session session)
		{
			try
			{
				Skip();	
			}
            catch (NoMessagesException exc)
            {
#if DEBUG
                GXLogging.Debug(log,"Receive error", exc);
#endif
                session.HandleMailException(exc);
            } 
			catch(GXMailException exc)
			{
#if DEBUG
                GXLogging.Error(log,"Skip error", exc);
#endif
                session.HandleMailException(exc);
			}
		}
		#endregion

		#region Private Methods
		private void ConnectAndLogin()
		{
			if(secureConnection)
			{
				try
				{
					ConnectSSL();
				}
				catch(GXMailException )
				{
					ConnectTLS();
				}
			}
			else
			{
				ConnectNormal();
			}
		}

		private void ConnectTLS()
		{
			GXLogging.Debug(log,"Trying with TLS");
			Connect(SecureProtocol.Tls1);
			CheckLogin();
		}

		private void ConnectSSL()
		{
			GXLogging.Debug(log,"Trying with SSL");
			Connect(SecureProtocol.Ssl3);
			CheckLogin();
		}

		private void ConnectNormal()
		{
			GXLogging.Debug(log,"Trying Normal Connection");
			Connect(SecureProtocol.None);
			CheckLogin();
		}

		private void Connect(SecureProtocol protocol)
		{
			try
			{
				GXLogging.Debug(log,"Connecting to host: " + host + ", port: " + port);
				SecurityOptions options = new SecurityOptions(protocol);
                options.AllowedAlgorithms = SslAlgorithms.ALL;

				options.Entity = ConnectionEnd.Client;
				options.VerificationType = CredentialVerification.Manual;
				options.Verifier = new CertVerifyEventHandler(OnCertificateVerify);
				options.Flags = SecurityFlags.Default;
				options.CommonName = host;

				connection = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);

				IPHostEntry serverIPs = Dns.GetHostEntry(host.Trim());
				IPAddress hostAddress = serverIPs.AddressList.First(a => (a.AddressFamily == connection.AddressFamily)); ;
				connection.Connect(new IPEndPoint(hostAddress, port));

				string response = GetResponse();
				GXLogging.Debug(log,"Connect Server response: " + response);
				if(!response.StartsWith("+OK"))
				{
					throw new IOException(response);
				}
			}
			catch(IOException exc)
			{
				GXLogging.Error(log,"Bad server response", exc);
				throw new GXMailException(exc.Message, MailConstants.MAIL_CantLogin);
			}
			catch(Exception exc)
			{
				GXLogging.Error(log,"Can't connect to host", exc);
                throw new GXMailException("Can't connect to host", MailConstants.MAIL_CantLogin);
			}
		}

		private void OnCertificateVerify(SecureSocket socket, Certificate remote, CertificateChain chain, VerifyEventArgs e) 
		{
			Certificate[] certs = chain.GetCertificates();
			chain.VerifyChain(socket.CommonName, AuthType.Server);
		}

		private void Delete()
		{
            SendAndWaitResponse("DELE " + lastReadMessage, MailConstants.MAIL_ServerReplyInvalid);
		}

		private void GetNextUID(ref string nextUID)
		{
			if(lastReadMessage == count)
			{
                throw new NoMessagesException();
			}

			SendNL("UIDL " + (lastReadMessage + 1));

			string response = GetResponse();

			string valueToReturn = "";
			if (GeneXus.Configuration.Config.GetValueOf("POP3MesageUID", out valueToReturn) && valueToReturn == "SessionNumber")
				nextUID = getMessageNbr(response);
			else
				nextUID = getMessageUid(response);
		}

		private string getMessageNbr(string response)
		{
			string uid = "";
			int firstBlank = response.IndexOf(" ");
			if (firstBlank == -1)
			{
				return "";
			}
			int scndBlank = response.IndexOf(" ", firstBlank + 1);
			try
			{
				if (scndBlank > 0)
				{
					uid = response.Substring(firstBlank, scndBlank - firstBlank).Trim();
				}
				else
				{
					uid = response.Substring(firstBlank).Trim();
				}
			}
			catch (Exception ) { }
			return uid;
		}

		private string getMessageUid(string response)
		{
			string[] parsedResponse = response.Split(' ');
			if (parsedResponse.Length > 2)
				return parsedResponse[2];
			else if (parsedResponse.Length > 1)
				return parsedResponse[1];
			return "";
		}
		private void CheckLogin()
		{
            SendAndWaitResponse("USER " + userName, MailConstants.MAIL_ServerReplyInvalid);
			SendAndWaitResponse("PASS " + password, MailConstants.MAIL_ServerReplyInvalid);

			count = GetIntValue("STAT");
			
			try
			{                
				lastReadMessage = newMessages?GetIntValue("LAST"):0;
			}
			catch(GXMailException )
			{
                throw new GXMailException("POP3 server does not support NewMessages = 1", MailConstants.MAIL_LastNotSupported);
			}
            GXLogging.Debug(log,"lastReadMessage " + lastReadMessage);

		}

		private void Logout()
		{
            SendAndWaitResponse("QUIT", MailConstants.MAIL_ServerReplyInvalid);
			count = 0;
			try
			{
                connection.Close();//Don't use .Shutdown(SocketShutdown.Both); SocketController calls Shutdown(SocketShutdown.Both)on onReceive method.
			}
			catch(Exception exc)
			{
				GXLogging.Error(log,"Error logging out", exc);
                throw new GXMailException(exc.Message, MailConstants.MAIL_ConnectionLost);
			}
			finally
			{
				if(connection != null)
				{
					connection.Close();
				}
			}
		}

		private void Receive(GXMailMessage msg)
		{
			if(lastReadMessage == count)
			{
                throw new NoMessagesException();
			}
			
			msg.Clear();

			char pSep = System.IO.Path.DirectorySeparatorChar;
			if(!attachDir.EndsWith(pSep.ToString()))
			{
				attachDir += pSep.ToString();
			}

			SendNL("RETR " + (++lastReadMessage));
            GXLogging.Debug(log,"Receive " + lastReadMessage);

			MailMessage message = null;
			try
			{
                mailReader = new RFC822Reader(new RFC822EndReader(new SecureNetworkStream(connection)));
				message = new MailMessage(mailReader, attachDir, readerTimeout);
                message.DownloadAttachments = this.DownloadAttachments;
				message.ReadAllMessage();
			}
			catch(InvalidMessageException ime)
			{
#if DEBUG
                GXLogging.Error(log,"Receive error", ime);
#endif
                throw new GXMailException(ime.Message, GXInternetConstants.MAIL_InvalidValue);
			}
            catch (CouldNotSaveAttachmentException dae)
			{
#if DEBUG
                GXLogging.Error(log,"Receive error", dae);
#endif
                throw new GXMailException(dae.Message, GXInternetConstants.MAIL_CantSaveAttachment);
			}
            catch(InvalidAttachmentException iae)
			{
#if DEBUG
                GXLogging.Error(log,"Receive error", iae);
#endif
                throw new GXMailException(iae.Message, GXInternetConstants.MAIL_InvalidAttachment);
			}
			catch(TimeoutExceededException toe)
			{
#if DEBUG
                GXLogging.Error(log,"Receive error", toe);
#endif
                throw new GXMailException(toe.Message, GXInternetConstants.MAIL_TimeoutExceeded);
			}
			catch(Exception exc)
			{
#if DEBUG
                GXLogging.Error(log,"Receive error", exc);
#endif
                throw new GXMailException(exc.Message, MailConstants.MAIL_ConnectionLost);
			}
			try
			{
				if (message != null)
				{
                    GXMailRecipientCollection msgFrom = new GXMailRecipientCollection();
                    message.SetMessageRecipients(msgFrom, GXInternetConstants.FROM);
                    if (msgFrom.Count > 0)
                    {
                        msg.From = msgFrom.Item(1);
                    }
                    message.SetMessageRecipients(msg.ReplyTo, GXInternetConstants.REPLY_TO);
					message.SetMessageRecipients(msg.To, GXInternetConstants.TO);
					message.SetMessageRecipients(msg.CC, GXInternetConstants.CC);
                    msg.Headers = message.Keys;
					msg.DateSent = message.GetDateSent();
					msg.DateReceived = message.GetDateReceived();

					msg.Subject = message.GetMessageSubject();

					msg.Text = message.GetMessageText();
					msg.HTMLText = message.GetMessageHtmlText();
				
					message.SetMessageAttachments(msg.Attachments);
				}
			}
			catch(Exception exc)
			{
                GXLogging.Error(log,"Error Receiving", exc);
				throw new GXMailException(exc.Message, GXInternetConstants.MAIL_InvalidValue);
			}
		}

		private void Skip()
		{
			if(lastReadMessage == count)
			{
                throw new NoMessagesException();
			}
			
			++lastReadMessage;
		}
		#endregion

		#region Send Methods
		private void SendAndWaitResponse(string cmdMsg, short errorCode)
		{
			SendTCP(cmdMsg + CRLF);

			string response = GetResponse();
			GXLogging.Debug(log,"SendAndWait Server response: " + response);
			string serverResponse = response;

			int firstBlank = serverResponse.IndexOf(" ");
			if(firstBlank != -1)
			{
				serverResponse = serverResponse.Substring(firstBlank).Trim();
			}
			if(response.StartsWith("-ERR")) 
			{
				GXLogging.Error(log,"Command error, server response: " + serverResponse);
                throw new GXMailException("Server replied with an error: " + serverResponse, MailConstants.MAIL_ServerRepliedErr);
			}
			if(!response.StartsWith("+OK"))
			{
				GXLogging.Error(log,"Command error, server response: " + serverResponse);
				throw new GXMailException(response, errorCode);
			}
		}

		private void SendTCP(string cmd)
		{
			GXLogging.Debug(log,"Command: " + cmd);
			try
			{
				byte[] toSend = Encoding.ASCII.GetBytes(cmd);
				int sent = connection.Send(toSend);
				while(sent != toSend.Length) 
				{
					sent += connection.Send(toSend, sent, toSend.Length - sent, SocketFlags.None);
				}
			}
			catch(Exception exc)
			{
				GXLogging.Error(log,"Error sending command", exc);
                throw new GXMailException(exc.Message, MailConstants.MAIL_ConnectionLost);
			}
		}

		private void SendNL(string str)
		{
			SendTCP(str + CRLF);
		}
		#endregion

		#region Server Response
		private string GetResponse()
		{
			int ret = 0;
			string response = "";
			byte[] buffer = new byte[1024];
			try
			{                                
				ret = connection.Receive(buffer);
                
				while(ret > 0) 
				{
					response += Encoding.UTF8.GetString(buffer, 0, ret);
					if(FinishedReading(response))
					{
						break;
					}
					ret = connection.Receive(buffer);
				}
			}
			catch(Exception exc)
			{
				GXLogging.Error(log,"Error reading server response", exc);
                throw new GXMailException(exc.Message, MailConstants.MAIL_ConnectionLost);
			}
				
			return response.Trim();
		}

		protected bool FinishedReading(string response) 
		{
			string[] parts = response.Replace("\r\n", "\n").Split('\n');
			if((parts.Length > 1 && string.IsNullOrEmpty(parts[parts.Length - 1])) || (parts.Length > 1 && ((parts[parts.Length - 2].Length > 3 && parts[parts.Length - 2].Substring(3, 1).Equals(" ")) || (parts[parts.Length - 2].Length == 3))))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private int GetIntValue(string cmdStr)
		{
			SendNL(cmdStr);

			string response = GetResponse();
			GXLogging.Debug(log,"GetIntValue Server response: " + response);
			int firstBlank = response.IndexOf(" ");
			if(firstBlank == -1)
			{
				return 0;
			}
			int scndBlank = response.IndexOf(" ", firstBlank + 1);
			int retVal = 0;
			try
			{
				if(scndBlank > 0)
				{
					retVal = int.Parse(response.Substring(firstBlank, scndBlank - firstBlank).Trim());
				}
				else
				{
					retVal = int.Parse(response.Substring(firstBlank).Trim());
				}
			}
			catch(Exception ) {}

			return retVal;
		}
		#endregion

	}
}
