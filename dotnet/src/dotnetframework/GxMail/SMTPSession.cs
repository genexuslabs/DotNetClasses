using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using log4net;
using GeneXus.Utils;
using GeneXus.Mail.Internals.Smtp;
using Org.Mentalis.Security.Ssl;
using Org.Mentalis.Security.Certificates;
using System.Collections.Generic;

namespace GeneXus.Mail.Internals
{
    
    internal class SMTPSession: ISMTPSession
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(SMTPSession));
        #region Mail Constants
        private const int CR = 13;
        private const int LF = 10;
		internal const int MAIL_CantLogin = 3;
		internal const int MAIL_MessageNotSent = 10;
		internal const int MAIL_NoRecipient = 13;
		internal const int MAIL_InvalidRecipient = 14;
		internal const int MAIL_InvalidAttachment = 15;
		internal const int MAIL_ConnectionLost = 19;
		internal const int MAIL_AuthenticationError = 24;
		internal const int MAIL_PasswordRefused = 25;
        #endregion

        #region Local Variables
        private static string localhostName;
        private const string CRLF = "\r\n";

        private bool authentication;
        private bool secureConnection;
        private string host;
        private string userName;
        private string password;
        private int port;
        private short timeout;
        private string lastResponse;
        private MimeEncoder mimeEncoder;

        private SecureSocket connection;
        private SecurityOptions options;
        #endregion

        #region Constructor
        static SMTPSession()
        {
            SetLocalHost();
        }

        public SMTPSession()
        {
            authentication = false;
            secureConnection = false;
            host = "";
            userName = "";
            password = "";
            port = 25;
            timeout = 30;
            lastResponse = "";
            mimeEncoder = new MimeEncoder();
        }
        #endregion

        #region Properties
        public bool Authentication
        {
            get
            {
                return authentication;
            }
            set
            {
                authentication = value;
            }
        }

        public string Host
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

        public string UserName
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

        public string Password
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

        public int Port
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

        public short Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }
        #endregion

        #region Public Methods
        public void Login(GXSMTPSession session)
        {
            SetSessionProperties(session);
            ConnectAndLogin();            
        }

        public void Send(GXSMTPSession session, GXMailMessage msg)
        {
            try
            {
                if ((msg.To.Count == 0) && (msg.CC.Count == 0) && (msg.BCC.Count == 0))
                {
                    throw new GXMailException("No main recipient specified", MAIL_NoRecipient);
                }

                try
                {
                    SendAndWaitResponse("MAIL FROM: <" + session.Sender.Address + ">", "2", MAIL_MessageNotSent);

                    SendRecipientList(msg.To);
                    SendRecipientList(msg.CC);
                    SendRecipientList(msg.BCC);

                    SendAndWaitResponse("DATA", "354", MAIL_MessageNotSent);

                    SendNL("FROM: \"" + ToEncodedString(session.Sender.Name) + "\" <" + session.Sender.Address + ">");                
                    SendAllRecipients(msg.To, GXInternetConstants.TO);
                    SendAllRecipients(msg.CC, GXInternetConstants.CC);
                    SendAllRecipients(msg.ReplyTo, GXInternetConstants.REPLY_TO);
                 
                    SendNL("MIME-Version: 1.0");
                    SendNL("SUBJECT: " + ToEncodedString(msg.Subject));
                    SendNL("DATE: " + GetNowAsString());

                    foreach (string key in msg.Headers.Keys)
                    {
                        SendNL(String.Format("{0}: {1}", key, msg.Headers[key]));
                    }
           
                    string sTime = DateTime.Now.Ticks.ToString();
                    bool isMultipart = ((msg.HTMLText.Length > 0) || (msg.Attachments.Count > 0));

                    if (msg.Attachments.Count > 0)
                    {
                        SendNL("Content-Type: multipart/mixed;boundary=\"" +
                            getStartMessageIdMixed(sTime) +
                            "\"\r\n\r\nThis message is in MIME format. Since your mail reader does not understand\r\nthis format, some or all of this message may not be legible.\r\n\r\n" +
                            getNextMessageIdMixed(sTime, false));

                    }

                    if ((msg.HTMLText.Length > 0) && (msg.Text.Length > 0))
                    {
                        SendNL("Content-Type: multipart/alternative;boundary=\"" +
                            getStartMessageIdAlternative(sTime) +
                            "\"\r\n\r\nThis message is in MIME format. Since your mail reader does not understand\r\nthis format, some or all of this message may not be legible.\r\n\r\n");
                    }

                    if (msg.Text.Length > 0)
                    {
                        SendNL("Content-Type:text/plain; charset=\"UTF-8\"\r\n");
                        SendTextUTF8(msg.Text);

                        if (msg.HTMLText.Length > 0)
                        {
                            SendNL("\r\n\r\n" + getNextMessageIdAlternative(sTime, false));
                        }
                    }

                    if (msg.HTMLText.Length > 0)
                    {
                        SendNL("Content-Type: text/html; charset=\"UTF-8\"\r\n");

                        SendTextUTF8(msg.HTMLText);
                        SendNL("");

                        if (msg.Text.Length > 0)
                        {
                            SendNL(getNextMessageIdAlternative(sTime, true));
                        }
                    }                    
                    SendAttachments(sTime, msg.Attachments, session.AttachDir);
                }
                catch (Exception exc)
                {
                    throw new GXMailException(exc.Message, MAIL_ConnectionLost);
                }

                SendAndWaitResponse(CRLF + ".", "2", MAIL_MessageNotSent);
            }
            catch (GXMailException exc)
            {
                session.HandleMailException(exc);
            }
        }

        private void SendAllRecipients(GXMailRecipientCollection coll, string cmd)
        {
            if (coll.Count > 0)
            {
                List<string> addresses = new List<string>();                
                foreach (GXMailRecipient recipient in coll)
                {
                    addresses.Add(GetRecipientAsString(recipient));
                }
                SendNL(cmd + ":" + String.Join(",", addresses.ToArray()));
            }
        }

        private string GetNowAsString()
        {
            string nowString = DateTime.Now.ToString("ddd, dd MMM yyyy HH:mm:ss", CultureInfo.CreateSpecificCulture("en-US"));
            int offset = DateTime.Now.Subtract(DateTime.Now.ToUniversalTime()).Hours;
            string offsetString = "";
            if (offset == 0)
            {
                offsetString = "-0000";
            }
            else
            {
                int absOffset = Math.Abs(offset);
                offsetString = ((absOffset == offset) ? "+" : "-");
                offsetString += (absOffset < 10) ? "0" : "";
                offsetString += absOffset.ToString() + "00";
            }
            nowString += " " + offsetString;
            return nowString;
        }

        public void Logout(GXSMTPSession session)
        {
            try
            {
                Logout();
            }
            catch (GXMailException exc)
            {
                session.HandleMailException(exc);
            }
        }
        #endregion

        #region Private Methods
        #region Connect, Login, Logout
        private void Connect(SecureProtocol protocol, bool directTLS)
        {
            try
            {
                GXLogging.Debug(log, "Connecting to host: " + host + ", port: " + port);
                options = new SecurityOptions(protocol);
                options.AllowedAlgorithms = SslAlgorithms.SECURE_CIPHERS;
                options.Entity = ConnectionEnd.Client;
                options.VerificationType = CredentialVerification.Manual;
                options.Verifier = new CertVerifyEventHandler(OnCertificateVerify);
                options.Flags = SecurityFlags.Default;
                options.CommonName = host;                
                if ((protocol == SecureProtocol.Tls1) && directTLS)
                {
                    options.Protocol = SecureProtocol.Tls1;
                }

                connection = new SecureSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, options);

                IPAddress[] serverIPs = Dns.GetHostAddresses(host.Trim());

                IAsyncResult result = connection.BeginConnect(new IPEndPoint(serverIPs[0], port), null, null);                
                if (result.AsyncWaitHandle.WaitOne(Timeout * 1000, true) && connection.Connected)
                {
                    string response = GetResponse();
                    GXLogging.Debug(log, "Server response: " + response);
                    if (!response.StartsWith("220"))
                    {
                        throw new IOException(response);
                    }
                }
                else
                {

                    GXLogging.Error(log, String.Format("Could not connect to host '{0}' with DNS resolved IP: {1} - Port {2}", host, serverIPs[0].ToString(), port.ToString()));                    
                    throw new GXMailException("Can't connect to host", MAIL_CantLogin);
                }
            }
            catch (IOException exc)
            {
                GXLogging.Error(log, "Bad server response", exc);
                throw new GXMailException(exc.Message, MAIL_CantLogin);
            }
            catch (Exception exc)
            {
                GXLogging.Error(log, "Can't connect to host", exc);
                throw new GXMailException("Can't connect to host", MAIL_CantLogin);
            }
        }

        private void ConnectAndLogin()
        {            
            if (secureConnection)
            {
                try
                {
                    ConnectSSL();                    
                }
                catch (GXMailException )
                {
                    LogoutSafe();
                    try
                    {
                        ConnectIndirectTLS();                        
                    }
                    catch (GXMailException )
                    {
                        LogoutSafe();
                        try
                        {
                            ConnectDirectTLS();                            
                        }
                        catch (GXMailException )
                        {
                            LogoutSafe();
                            ConnectNormal();
                        }
                    }
                }
            }
            else
            {
                ConnectNormal();
            }
        }

        private void ConnectIndirectTLS()
        {
            GXLogging.Debug(log, "Trying with Indirect TLS");
            Connect(SecureProtocol.Tls1, false);
            CheckLogin(true);
        }

        private void ConnectDirectTLS()
        {
            GXLogging.Debug(log, "Trying with Direct TLS");
            Connect(SecureProtocol.Tls1, true);
            CheckLogin(false);
        }

        private void ConnectSSL()
        {
            GXLogging.Debug(log, "Trying with SSL");
            Connect(SecureProtocol.Ssl3, false);
            CheckLogin(false);
        }

        private void ConnectNormal()
        {
            GXLogging.Debug(log, "Trying Normal Connection");
            Connect(SecureProtocol.None, false);
            CheckLogin(false);
        }

        private void OnCertificateVerify(SecureSocket socket, Certificate remote, CertificateChain chain, VerifyEventArgs e)
        {
            Certificate[] certs = chain.GetCertificates();
            chain.VerifyChain(socket.CommonName, AuthType.Server);
        }

        private void CheckLogin(bool startTLS)
        {
                try
                {
                    if (authentication)
                    {
                        SendAndWaitResponse("EHLO " + localhostName);
                        if (!startTLS)
                        {
                            startTLS = CheckStartTLS();
                        }
                        if (startTLS)
                        {
                            StartTLS();
                            SendAndWaitResponse("EHLO " + localhostName);
                        }                        
                        SendAndWaitResponse("AUTH LOGIN", "334", MAIL_AuthenticationError);
                        SendAndWaitResponse(ToBase64(userName), "334", MAIL_AuthenticationError);
                        SendAndWaitResponse(ToBase64(password), "235", MAIL_PasswordRefused);
                    }
                    else
                    {
                        SendAndWaitResponse("HELO " + localhostName);
                        if (startTLS)
                        {
                            StartTLS();
                            SendAndWaitResponse("HELO " + localhostName);
                        }
                    }
                }
                catch (GXMailException ex)
                {
                    switch (ex.ErrorCode)
                    {
                        case MAIL_AuthenticationError:
                            throw new AuthenticationException();
                        case MAIL_PasswordRefused:
                            throw new BadCredentialsException();
                        default:
                            throw ex;
                    }                    
                }
            
        }

        private bool CheckStartTLS()
        {
            return (this.lastResponse.IndexOf("STARTTLS") != -1);
        }

        private void StartTLS()
        {
            SendAndWaitResponse("STARTTLS");
            options.Protocol = SecureProtocol.Tls1;
            connection.ChangeSecurityProtocol(options);
        }

        private string ToBase64(string data)
        {
            ASCIIEncoding Encoder = new ASCIIEncoding();
            return Convert.ToBase64String(Encoder.GetBytes(data));
        }

        private void Logout()
        {
            SendNL("QUIT");

            try
            {
                connection.Close();//Don't use .Shutdown(SocketShutdown.Both); SocketController calls Shutdown(SocketShutdown.Both)on onReceive method.
            }
            catch (Exception exc)
            {
                GXLogging.Error(log, "Error logging out", exc);
                throw new GXMailException(exc.Message, MAIL_ConnectionLost);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        private void LogoutSafe()
        {
            try
            {
                Logout();
            }
            catch (GXMailException )
            {
            }
        }
        #endregion

        #region Send Methods
        private void SendAndWaitResponse(string cmdMsg)
        {
            SendAndWaitResponse(cmdMsg, "2", 0);
        }

        private void SendAndWaitResponse(string cmdMsg, string okResponse)
        {
            SendAndWaitResponse(cmdMsg, okResponse, 0);
        }

        private void SendAndWaitResponse(string cmdMsg, string okResponse, short errorCode)
        {
            SendTCP(cmdMsg + CRLF);

            string response = GetResponse();
            GXLogging.Debug(log,"Server response: " + response);
            if (!response.Equals(okResponse))
            {
                if (!response.StartsWith(okResponse))
                {
                    SendNL("RSET");
                    GXLogging.Error(log,"Command error, server response: " + response);
                    throw new GXMailException(response, errorCode);
                }
            }
        }

        private void SendText(string text)
        {
            
            StringBuilder strBuilder = new StringBuilder("");

            SendNL("");

            bool firstCharOfLine = true;
            int newLines = 0;
            int txtLen = text.Length;

            for (int i = 0; i < txtLen; i++)
            {
                char currChar = text[i];
                if ((currChar != CR) && (currChar != LF))
                {
                    if (newLines > 0)
                    {
                        for (int j = 0; j < newLines; j++)
                        {
                            strBuilder.Append(CRLF);
                        }
                        firstCharOfLine = true;
                        newLines = 0;
                    }
                    if (firstCharOfLine && (currChar == '.'))
                    {
                        strBuilder.Append('.');
                    }
                    strBuilder.Append(currChar);
                    firstCharOfLine = false;
                }
                else
                {
                    if (((i > 0) && ((text[i - 1] == CR) || (text[i - 1] == LF))) ||
                        ((i > 0) && (text[i - 1] != CR) && (text[i - 1] != LF) &&
                        (i < txtLen - 1) && (text[i + 1] != CR) && (text[i + 1] != LF)))
                    {
                        newLines++;
                    }
                }
            }

            SendNL(strBuilder.ToString());
        }

        private void SendTextUTF8(string text)
        {
            string[] textLines = text.Split(new char[] { '\n' });

            for (int i = 0; i < textLines.Length; i++)
            {
                string line = textLines[i];
                if (line.StartsWith("."))
                {
                    line = "." + line;
                }
                SendEncodedNL(line, Encoding.UTF8);
            }
        }

        private void SendNL(string str)
        {
            SendTCP(str + CRLF);
        }

        private void SendEncoded(string cmd, Encoding encoding)
        {
            GXLogging.Debug(log,"Encoded command: " + cmd + ", encoding: " + encoding.BodyName);
            try
            {
                byte[] toSend = encoding.GetBytes(cmd);
                int sent = connection.Send(toSend);
                while (sent != toSend.Length)
                {
                    sent += connection.Send(toSend, sent, toSend.Length - sent, SocketFlags.None);
                }
            }
            catch (Exception exc)
            {
                GXLogging.Error(log,"Error encoding message", exc);
                throw new GXMailException(exc.Message, MAIL_ConnectionLost);
            }
        }

        private void SendEncodedNL(string cmd, Encoding encoding)
        {
            SendEncoded(cmd + CRLF, encoding);
        }

        private void SendTCP(string cmd)
        {
            GXLogging.Debug(log,"Command: " + cmd);
            try
            {
                byte[] toSend = Encoding.Default.GetBytes(cmd);
                int sent = connection.Send(toSend);
                while (sent != toSend.Length)
                {
                    sent += connection.Send(toSend, sent, toSend.Length - sent, SocketFlags.None);
                }
            }
            catch (Exception exc)
            {
                GXLogging.Error(log,"Error sending command", exc);
                throw new GXMailException(exc.Message, MAIL_ConnectionLost);
            }
        }

        private void SendRecipientList(GXMailRecipientCollection recipients)
        {
            try
            {
                foreach (GXMailRecipient recipient in recipients)
                {
                    SendAndWaitResponse("RCPT TO: <" + recipient.Address + ">");
                }
            }
            catch (GXMailException exc)
            {
                throw new GXMailException(exc.Message, MAIL_InvalidRecipient);
            }
        }
        #endregion

        #region Attachments Management
        private void SendAttachments(string sTime, GxStringCollection attachments, string attachDir)
        {
            GXLogging.Debug(log,"Sending attachments, attachments path: " + attachDir);
            try
            {               
                int qty = attachments.Count;
                if (qty == 0)
                {
                    return;
                }
                SendNL("");
                for (int i = 1; i <= qty; i++)
                {
                    SendAttachment(sTime, attachments.Item(i), attachDir);
                }
                SendNL(getNextMessageIdMixed(sTime, true));
            }
            catch (Exception exc)
            {
                GXLogging.Error(log,"Error sending attachments", exc);
                throw new GXMailException(exc.Message, MAIL_ConnectionLost);
            }
        }

        private void SendAttachment(string sTime, string fileNamePath, string attachDir)
        {
            GXLogging.Debug(log,"Sending attachment: " + fileNamePath);
            string fileName = fileNamePath;
            string fullfileName = fileName;

            fileName = Path.GetFileName(fileNamePath);

            if (!Path.IsPathRooted(fullfileName))
                fullfileName = attachDir + fullfileName;

            SendNL(getNextMessageIdMixed(sTime, false));
            SendNL("Content-Type: " + "application/octet-stream");
            SendNL("Content-Transfer-Encoding: " + "base64");
            SendNL("Content-Disposition: " + "attachment; filename=\"" + ToEncodedString(fileName) + "\"");
            SendNL("");
            SendBase64Encoded(fullfileName);
            SendNL("");
        }

        private void SendBase64Encoded(string fileName)
        {
            byte[] binaryData;

            try
            {
                FileInfo f = new FileInfo(fileName);
                FileStream fs = f.OpenRead();

                binaryData = new Byte[fs.Length];
                long bytesRead = fs.Read(binaryData, 0, (int)fs.Length);
                fs.Close();
                string base64String = Convert.ToBase64String(binaryData, 0, binaryData.Length);

                SendTCP(base64String);
            }
            catch (FileNotFoundException exc)
            {
                GXLogging.Error(log,"Can't find " + fileName, exc);
                throw new GXMailException("Can't find " + fileName, MAIL_InvalidAttachment);
            }
            catch (Exception exc)
            {
                GXLogging.Error(log,"Invalid attach " + fileName, exc);
                throw new GXMailException("Invalid attach " + fileName, MAIL_InvalidAttachment);
            }
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
                while (ret > 0)
                {
                    response += Encoding.ASCII.GetString(buffer, 0, ret);
                    if (FinishedReading(response))
                    {
                        break;
                    }
                    ret = connection.Receive(buffer);
                }
            }
            catch (Exception exc)
            {
                GXLogging.Error(log,"Error reading server response", exc);
                throw new GXMailException(exc.Message, MAIL_ConnectionLost);
            }

            lastResponse = response.Trim();
            return lastResponse;
        }

        protected bool FinishedReading(string response)
        {
            string[] parts = response.Replace("\r\n", "\n").Split('\n');
            if (parts.Length > 1 && ((parts[parts.Length - 2].Length > 3 && parts[parts.Length - 2].Substring(3, 1).Equals(" ")) || (parts[parts.Length - 2].Length == 3)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Auxiliar Methods
        private void SetSessionProperties(GXSMTPSession session)
        {
            host = session.Host;
            port = session.Port;
            authentication = (session.Authentication == 1);
            secureConnection = (session.Secure == 1);
            userName = session.UserName;
            password = session.Password;
            timeout = session.Timeout;
        }

        private static void SetLocalHost()
        {
            try
            {
                localhostName = Dns.GetHostName();
            }
            catch (Exception )
            {
                localhostName = "unknown";
            }
        }

        private string ToEncodedString(string text)
        {
            Encoding encoding = GetEncoding();
            if (encoding != null)
            {
                return mimeEncoder.EncodeString(text, encoding);
            }
            else
            {
                if (IsAscii(text))
                    return text;
                else
                    return mimeEncoder.EncodeString(text, Encoding.UTF8);
            }
        }
        private bool IsAscii(string text)
        {
            char[] chars = text.ToCharArray();
            foreach (char c in chars)
            {
                if (c > 127)
                    return false;
            }
            return true;
        }
        private Encoding GetEncoding()
        {
            
            string cult;
            if (GeneXus.Configuration.Config.GetValueOf("Culture", out cult) && cult == "ja-JP")
            {
                return Encoding.GetEncoding("ISO-2022-JP");
            }
            return null;
        }

        private string getStartMessageIdMixed(string sTime)
        {
            return getStartMessage(sTime, "MIXED");
        }

        private string getNextMessageIdMixed(string sTime, bool end)
        {
            return getNextMessage(sTime, "MIXED", end);
        }

        private string getStartMessageIdAlternative(string sTime)
        {
            return getStartMessage(sTime, "ALTERNATIVE");
        }

        private string getNextMessageIdAlternative(string sTime, bool end)
        {
            return getNextMessage(sTime, "ALTERNATIVE", end);
        }

        private string getStartMessage(string sTime, string sPrefix)
        {
            return "----_=_NextPart_" + sPrefix + "_" + sTime;
        }

        private string getNextMessage(string sTime, string sPrefix, bool end)
        {
            return "--" + getStartMessage(sTime, sPrefix) + (end ? "--" : "");
        }
        private string GetRecipientAsString(GXMailRecipient recipient)
        {
            return "\"" + ToEncodedString(recipient.Name) + "\" <" + recipient.Address + ">";
        }
        #endregion
        #endregion
    }
}
