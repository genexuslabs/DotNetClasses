using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security;
using System.Linq;
using GeneXus.Encryption;
using GeneXus.Configuration;

namespace GeneXus.Utils
{
	[SecuritySafeCritical]
	public class FtpService
	{

		private Socket _DataSocket;
		private Socket _ControlSocket;

		private Uri _RequestUri;
		private Uri _ProxyUri;

		private IWebProxy	_Proxy;
		private ServicePoint _ServicePoint;

		private bool _bPassiveMode;

		private StringBuilder _sbControlSocketLog;
		private int _status;
		private string _statusDescription;
		private string _defaultUser;
		private string _defaultPassword;

		public FtpService()
		{
			_sbControlSocketLog = new StringBuilder();

		}

		public bool Passive 
		{
			set { _bPassiveMode = value; }
			get { return _bPassiveMode; }
		}

		public IWebProxy Proxy 
		{
			get 
			{
				return _Proxy;
			}
			set 
			{
				_Proxy = value;
			}
		}
		string DefaultUser
		{
			get
			{
				if (string.IsNullOrEmpty(_defaultUser))
				{
					string cfgBuf;
					if (Config.GetValueOf("FTP_DEFAULT_USER", out cfgBuf))
					{
						CryptoImpl.Decrypt(ref _defaultUser, cfgBuf);
					}
				}
				return _defaultUser;
			}
		}
		string DefaultPassword
		{
			get
			{
				if (string.IsNullOrEmpty(_defaultPassword))
				{
					string cfgBuf;
					if (Config.GetValueOf("FTP_DEFAULT_PASSWORD", out cfgBuf))
					{
						CryptoImpl.Decrypt(ref _defaultPassword, cfgBuf);
					}
				}
				return _defaultPassword;
			}
		}

		public void Connect(string sUrl, string user, string pass)
		{
			ResetError();
			Disconnect();
			if ( ! sUrl.StartsWith( "ftp:"))
			{
				if (! sUrl.StartsWith( "//"))
					sUrl = "//" + sUrl;
				sUrl = "ftp:" + sUrl;
			}
			Uri Url= new Uri( sUrl);
			if(Url.Scheme != "ftp")  
				throw new NotSupportedException("This protocol is not supported");
			
			_RequestUri = Url;
			_bPassiveMode = false;

			if (user.Trim().Length == 0)
				user = DefaultUser;
			if (pass.Trim().Length == 0)
				pass = DefaultPassword;

			if(_Proxy != null) 
			{
				
				_ProxyUri = GetProxyUri();

				if(_ProxyUri != null) 
				{

					if(_Proxy.Credentials != null) 
					{
						NetworkCredential cred = _Proxy.Credentials.GetCredential(_ProxyUri,null);

						user = cred.UserName;
						pass = cred.Password;

						if (String.IsNullOrEmpty(user))
							user = DefaultUser;

						if (String.IsNullOrEmpty(pass))
							pass = DefaultPassword;
					}
					else
						
					user = user + "@" + _RequestUri.Host.ToString();
				}
#pragma warning disable SYSLIB0014 // ServicePointManager 			
				_ServicePoint = ServicePointManager.FindServicePoint( _RequestUri, _Proxy);
			} 
			else 
			{
				
				_ServicePoint = ServicePointManager.FindServicePoint( _RequestUri,null);
#pragma warning disable SYSLIB0014 // ServicePointManager 
			}

			if (!DoLogin(user,pass)) 
				ProcessApplicationError("Login Failed.");

		}

		FtpStream FileToFtpStream(string source)
		{
			try
			{
				FtpStream writeStream;
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				using (FileStream readStream = new FileStream(source, FileMode.Open, FileAccess.Read))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				{

					using (writeStream = new FtpStream(new MemoryStream(), false, true, false))
					{

						int Length = 256;
						Byte[] buffer = new Byte[Length];
						int bytesread = readStream.Read(buffer, 0, Length);

						while (bytesread > 0)
						{
							writeStream.Write(buffer, 0, bytesread);
							bytesread = readStream.Read(buffer, 0, Length);
						}
					}
				}
				return writeStream;
			}
			catch (Exception e)
			{
				ProcessApplicationError("", e);
				return null;
			}

		}

		void FtpStreamToFile( string target, Stream readStream)
		{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			using (FileStream writeStream = new FileStream(target, FileMode.OpenOrCreate, FileAccess.Write))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			{
				using (readStream)
				{
					readStream.Position = 0;

					int Length = 256;
					Byte[] buffer = new Byte[Length];
					int bytesread = readStream.Read(buffer, 0, Length);

					while (bytesread > 0)
					{
						writeStream.Write(buffer, 0, bytesread);
						bytesread = readStream.Read(buffer, 0, Length);
					}
				}
			}
		}

		public void Put(string source, string target, string transferMode)
		{
			string targetPath, sourcePath, targetName, sourceName;
			ResetError();

			pathAndName( "", source, out sourcePath, out sourceName);
			pathAndName( sourceName, target, out targetPath, out targetName);

			FtpStream writeStream = FileToFtpStream( sourcePath+"\\"+sourceName);

			if (_status != 0)
				return;

			if(_bPassiveMode)
			{
				OpenPassiveDataConnection();								
			}
			else
				OpenDataConnection();									

			string sztype = "I";
			if(transferMode == "A") 
			{
				sztype = "A";
			}

			SendCommand("TYPE",sztype);

			ResponseDescription resp_desc = ReceiveCommandResponse();

			if(!resp_desc.PositiveCompletion) 
			{
				ProcessApplicationError("Data negotiation failed:\n" + _sbControlSocketLog.ToString());
				CloseDataConnection();
				return;
			}

			setFtpDir( targetPath);

			SendCommand("STOR", targetName);
			resp_desc = ReceiveCommandResponse();

			if(resp_desc.PositivePreliminary) 
			{
				if(writeStream != null)
				{
					Socket DataConnection;
					if( _bPassiveMode)
						DataConnection = _DataSocket;							
					else
						DataConnection = _DataSocket.Accept();								
					if(DataConnection == null)
					{
						ProcessProtocolViolationError("Accept failed ");						
					}
				
					SendData(writeStream, DataConnection);							
					DataConnection.Close();										
					ResponseDescription resp = ReceiveCommandResponse();
					if(! resp.PositiveCompletion) 
						_status = resp.Status;
					else
						_status = 0;
					_statusDescription = resp.StatusDescription;
				}
				else
				{  
					ProcessApplicationError("Data to be uploaded not specified");
					CloseDataConnection();
					return;
				}					
			}
			else
			{
				
				ProcessApplicationError(ComposeExceptionMessage(resp_desc, _sbControlSocketLog.ToString()));
				CloseDataConnection();
				return;
			}
			CloseDataConnection();
			
			return;
		}

		public void Get(string source, string target, string transferMode)
		{

			string targetPath, sourcePath, targetName, sourceName;
			ResetError();
			if(_bPassiveMode)
			{
				OpenPassiveDataConnection();								
			}
			else
				OpenDataConnection();									

			if (_status != 0)
				return;

			string sztype = "I";
			if(transferMode == "A") 
			{
				sztype = "A";
			}

			SendCommand("TYPE",sztype);

			ResponseDescription resp_desc = ReceiveCommandResponse();

			if(!resp_desc.PositiveCompletion) 
			{
				ProcessApplicationError("Data negotiation failed:\n" + _sbControlSocketLog.ToString());
				CloseDataConnection();
				return;
			}
			pathAndName( "", source, out sourcePath, out sourceName);
			setFtpDir( sourcePath);

			SendCommand("RETR", sourceName);
			resp_desc = ReceiveCommandResponse();

			if(resp_desc.PositivePreliminary) 
			{
				Socket DataConnection;
				if( _bPassiveMode)
					DataConnection = _DataSocket;							
				else
					DataConnection = _DataSocket.Accept();											
				if(DataConnection == null)
				{
					ProcessProtocolViolationError("DataConnection failed ");
				}					
				Stream datastream = ReceiveData(DataConnection);								
				DataConnection.Close();										

				ResponseDescription resp = ReceiveCommandResponse();
				if(! resp.PositiveCompletion) 
					_status = resp.Status;
				else
				{
					_status = 0;
				}
				_statusDescription = resp.StatusDescription;
				pathAndName( sourceName, target, out targetPath, out targetName);
				this.FtpStreamToFile( targetPath+"\\"+targetName, datastream);
			} 
			else 
			{
				ProcessApplicationError(ComposeExceptionMessage(resp_desc, _sbControlSocketLog.ToString()));
				CloseDataConnection();
				return;
			}
			
			CloseDataConnection();
		}

		public void Delete(string target)
		{
			string targetPath, targetName;
			ResetError();

			if (_ControlSocket == null)
			{
				ProcessApplicationError("Connection not open.");
				return;
			}

			pathAndName( "", target, out targetPath, out targetName);

			if (_status != 0)
				return;

			setFtpDir( targetPath);
			ResponseDescription resp;

			SendCommand("DELE", targetName);
			resp = ReceiveCommandResponse();

			if(! resp.PositiveCompletion) 
				_status = resp.Status;
			else
				_status = 0;
			_statusDescription = resp.StatusDescription;
			return;
		}

		public void GetStatusText( out string status)
		{
			status = _statusDescription;
		}

		public void GetControlSocketLog( out string status)
		{
			status = this._sbControlSocketLog.ToString();
		}

		public void GetErrorCode( out int status)
		{
			status = _status;
		}

		public void GetErrorCode( out short status)
		{
			status = Convert.ToInt16(_status);
		}

		public void Disconnect()
		{
			ResetError();
			if (_ControlSocket != null)
				CloseControlConnection();
		}

		private void pathAndName( string defaultName, string pathName, out string path, out string name)
		{
			int pos;
			pathName = pathName.Replace( '\\', '/');
			pos = pathName.LastIndexOf('/');
			if ( pos == -1)
			{
				path = "";
				if (pathName.Length == 0)
					name = defaultName;
				else
					name = pathName;
				return;
			}
			if (pathName.EndsWith("/"))
			{
				path = pathName;
				name = defaultName;
			}
			else
			{
				path = pathName.Substring(0,pos);
				name = pathName.Substring(pos+1);
			}
		}
		private void setFtpDir(string path)
		{
			path = path.Replace( '\\', '/');
			string[] dirs = path.Split( '/');
			SendCommand("CWD" , "/");
			ResponseDescription resp_desc = ReceiveCommandResponse();
			if(!resp_desc.PositiveCompletion) 
			{
				ProcessApplicationError("Error setting directory: root");
				CloseDataConnection();
				return;
			}
			foreach(string dir in dirs)
				if (dir.Trim().Length != 0)
				{
					SendCommand("CWD" , dir);
					resp_desc = ReceiveCommandResponse();
					if(!resp_desc.PositiveCompletion) 
					{
						ProcessApplicationError("Error setting directory:" + dir);
						CloseDataConnection();
						return;
					}
				}

		}

		private bool DoLogin(String UserID, String Password)
		{
			ResponseDescription resp;
		
			OpenControlConnection(_ServicePoint.Address);

			if (_status!=0)
				return false;
			SendCommand("USER" , UserID);

			resp = ReceiveCommandResponse();
		
			if(resp.Status == 331) 
			{
				SendCommand("PASS", Password);
			} 
			else
				return false;
		
			resp = ReceiveCommandResponse();
		
			if(resp.Status == 230)
				return true;
			return false;	
		}

		private void OpenDataConnection()
		{
			if (_ControlSocket == null)
			{
				ProcessApplicationError("Connection not open.");
				return;
			}

			if( _DataSocket != null) 
			{ 
				ProcessApplicationError("Data socket is already open.");
				return;
			}
			_DataSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

			IPHostEntry localHostEntry = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress hostAddress = localHostEntry.AddressList.First(a => (a.AddressFamily == _DataSocket.AddressFamily)); ;
			IPEndPoint epListener = new IPEndPoint(hostAddress, 0);
			_DataSocket.Bind(epListener);

			_DataSocket.Listen(5); 

			IPEndPoint localEP = (IPEndPoint) _DataSocket.LocalEndPoint;
			String szLocal = FormatAddress(localEP.Address.GetAddressBytes(), localEP.Port);

			SendCommand("PORT",szLocal);                
      
			ResponseDescription resp_desc = ReceiveCommandResponse();
			
			if( !resp_desc.PositiveCompletion )
			{
				
				ProcessApplicationError("Couldnt open data connection\n" + ComposeExceptionMessage(resp_desc, _sbControlSocketLog.ToString()));
				return;
			}
		}

		private void OpenPassiveDataConnection()
		{
			if (_ControlSocket == null)
			{
				ProcessApplicationError("Connection not open.");
				return;
			}
			if( _DataSocket != null) 
			{ 
				ProcessProtocolViolationError("");
				return;
			}
			int Port;
		
			SendCommand("PASV","");                

			ResponseDescription resp_desc = ReceiveCommandResponse();
		
			if( !resp_desc.PositiveCompletion )
			{
				
				ProcessApplicationError("Couldnt open passive data connection\n" + ComposeExceptionMessage(resp_desc, _sbControlSocketLog.ToString()));
				return;
			}
		
			Port = getPort(resp_desc.StatusDescription);
		
			_DataSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );       		
			if( _DataSocket == null )
			{
				ProcessProtocolViolationError("Error in creating Data Socket");	
			} 					
			IPHostEntry serverHostEntry = Dns.GetHostEntry(_RequestUri.Host);
			IPAddress hostAddress = serverHostEntry.AddressList.First(a => (a.AddressFamily == _DataSocket.AddressFamily)); ; ;
			IPEndPoint serverEndPoint = new IPEndPoint(hostAddress, Port);

			try
			{
				if (GXUtil.IsWindowsPlatform)
				{
					_DataSocket.Connect(serverEndPoint);
				}
				else
				{
					ProcessProtocolViolationError("OpenPassiveDataConnection not supported in this platform");
				}
			} 
			catch
			{
				_DataSocket.Close();
				_DataSocket = null;				
				ProcessProtocolViolationError("Passive connection failure");   
			}
		
			return;
		}

		private Uri GetProxyUri() 
		{
			
			Uri u = null;
			if(_Proxy != null && !_Proxy.IsBypassed(_RequestUri)) 
			{
				u = _Proxy.GetProxy(_RequestUri);
			}
			return u;
		}

		private void OpenControlConnection(Uri uriToConnect)
		{

			String Host = uriToConnect.Host;
			int Port = uriToConnect.Port;

			if (_ControlSocket != null) 
			{
				ProcessProtocolViolationError("Control connection already in use");
			}

			_ControlSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			EndPoint clientIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
			EndPoint clientEndPoint = clientIPEndPoint;

			try
			{
				_ControlSocket.Bind(clientEndPoint);
			}
			catch (Exception e)
			{
				_ControlSocket.Close();
				_ControlSocket = null;
				ProcessApplicationError(" Error in opening control connection", e);
				return;
			}

			clientEndPoint = _ControlSocket.LocalEndPoint;
			try
			{
				IPHostEntry serverHostEntry = Dns.GetHostEntry(Host);
				IPAddress ipAddresses = serverHostEntry.AddressList.First(a => (a.AddressFamily == _ControlSocket.AddressFamily));
				IPEndPoint serverEndPoint = new IPEndPoint(ipAddresses, Port);

				try
				{
					if (GXUtil.IsWindowsPlatform)
					{
						_ControlSocket.Connect(serverEndPoint);
					}
					else
					{
						ProcessProtocolViolationError("OpenControlConnection not supported in this platform");
					}
				}
				catch (Exception e)
				{  
					_ControlSocket.Close();
					_ControlSocket = null;

					String eee = "Winsock error: "
						+ Convert.ToString(System.Runtime.InteropServices.Marshal.GetLastWin32Error());

					ProcessApplicationError(eee, e);
					return;
				}

				MemoryStream responseStream = new MemoryStream();
				while (true)
				{
					int BufferSize = 256;
					Byte[] recvbuffer = new Byte[BufferSize + 1];
					int bytesread = _ControlSocket.Receive(recvbuffer, BufferSize, 0);
					responseStream.Write(recvbuffer, 0, bytesread);
					if (IsCompleteResponse(responseStream))
						break;
				}
				return;
			}
			catch (Exception e)
			{
				_ControlSocket.Close();
				_ControlSocket = null;
				ProcessApplicationError("", e);
				return;
			}
		}

		void CloseDataConnection()
		{
			if(_DataSocket != null)
			{

				_DataSocket.Close();
				_DataSocket = null;		
			}
		}

		private void CloseControlConnection()
		{

			_ControlSocket.Close();
			_ControlSocket = null;
		}	

		private Stream ReceiveData(Socket Accept)
		{
			if( _DataSocket == null ) 
			{
				throw new ArgumentNullException();
			}
        
			MemoryStream responseStream = new MemoryStream();
			int BufferSize = 256;
			Byte [] recvbuffer = new Byte[BufferSize + 1];        
			while(true) 
			{
				int bytesread = 0;
				recvbuffer[bytesread] = (Byte)'\0';            			          
				bytesread = Accept.Receive(recvbuffer, BufferSize, 0 );			
				if( bytesread <= 0 ) 
					break;  			
				responseStream.Write(recvbuffer,0,bytesread);														
			} 				
			
			return responseStream;
		}

		public int SendCommand(String RequestedMethod)
		{
			SendCommand(RequestedMethod, "");
			return 0;
		}
    	private void SendCommand (String RequestedMethod, String Parametertopass)
		{
			String Command = RequestedMethod;
	    
			if(!String.IsNullOrEmpty(Parametertopass))
				Command = Command +	" " + Parametertopass ;

			Command = Command +  "\r\n";

			_sbControlSocketLog.Append(Command);
		
			Byte[] sendbuffer = Encoding.ASCII.GetBytes(Command.ToCharArray());		
			if( _ControlSocket == null ) 
			{
				ProcessProtocolViolationError("");            
			}
			int cbReturn = _ControlSocket.Send( sendbuffer,Command.Length, 0);

			if(cbReturn < 0) 
			{
				ProcessApplicationError("Error writing to control socket");
				return;
			}
		
			return;
		}
		
		private ResponseDescription ReceiveCommandResponse()
		{ 
			ResponseDescription resp_desc = new ResponseDescription();
		
			int StatusCode = 0;
			String StatusDescription = null;
		
			bool bCompleteResponse=false;     
			if( _ControlSocket == null ) 
			{   	
				ProcessApplicationError("Control Connection not opened");
				return null;
			}
        
			MemoryStream responseStream = new MemoryStream();
		
			while(true) 
			{			
				int BufferSize = 256;
				Byte[] recvbuffer = new Byte[BufferSize + 1];
				int bytesread = 0;
				recvbuffer[0] = (Byte)'\0';				
				bytesread = _ControlSocket.Receive(recvbuffer, BufferSize, 0 );									

				if( bytesread <= 0 ) 
					break;  
				
				responseStream.Write(recvbuffer,0,bytesread);

				String szResponse = Encoding.ASCII.GetString(recvbuffer,0,bytesread);
				_sbControlSocketLog.Append(szResponse);
			
				bCompleteResponse = IsCompleteResponse(responseStream);				
				if(bCompleteResponse)
				{				
					break;			
				}
			}

			if(bCompleteResponse)
			{
				
				try
				{
					responseStream.Position=0;
					Byte [] bStatusCode = new Byte[3];
					responseStream.Read (bStatusCode, 0, 3) ;
					String statuscodestr=Encoding.ASCII.GetString(bStatusCode,0,3);
					StatusCode = Convert.ToInt16( statuscodestr);
				}
				catch
				{
					StatusCode =  -1;
				}
				
				int responsesize = (int)responseStream.Length;
				responseStream.Position = 0;
				Byte [] bStatusDescription = new Byte[responsesize];
				responseStream.Read (bStatusDescription, 0, responsesize) ;			
			
				StatusDescription = Encoding.ASCII.GetString(bStatusDescription,4,responsesize-4).Trim();			
			}
			else
			{
				
				ProcessProtocolViolationError("");            
			}

			resp_desc.Status = StatusCode;
			resp_desc.StatusDescription = StatusDescription;
		
			return resp_desc;
		}
	
		private bool IsCompleteResponse(Stream responseStream)
		{		
			bool bIsComplete = false;		
			int responselength = (int) responseStream.Length;
			responseStream.Position = 0;		
			if(responselength >= 5) 
			{
				int StatusCode=-1;									
				Byte[] ByteArray = new Byte[responselength];			
				String statuscodestr;							
				responseStream.Read(ByteArray,0,responselength);		
				statuscodestr=Encoding.ASCII.GetString(ByteArray,0,responselength);			
				if (responselength==5 && ByteArray[responselength-1] == '\n')	
				{
					
					bIsComplete = true;
				}			
				else if ((ByteArray [responselength-1] == '\n') && (ByteArray [responselength-2] == '\r'))	
				{							
					bIsComplete = true;
				}			
				if (responselength==5 && ByteArray[responselength-1] == '\n')	
				{
					
					bIsComplete = true;
				}			
				if(bIsComplete) 
				{
					try
					{
						StatusCode = Convert.ToInt16( statuscodestr.Substring(0,3));
					}
					catch
					{
						StatusCode =  -1;					
					}		
					if (statuscodestr[3] == '-') 
					{
						// multiline response verify whether response is complete, reponse must be ended with CRLF					
						//find out the beginning of last line					
						int lastlinestart =0;
						for(lastlinestart=responselength-2;lastlinestart >0;lastlinestart--)
						{						
							if ( ByteArray [lastlinestart] == '\n' && ByteArray [lastlinestart-1] == '\r')
								break;						
						}					
						if(lastlinestart ==0) 
						{
							bIsComplete = false; // Multilines not recieved						
						}
						else if(statuscodestr[lastlinestart+4] != ' ') //still not completed
						{
							bIsComplete = false;
						}
						else
						{
							int endcode = -1;						
							try
							{
								endcode = Convert.ToInt16( statuscodestr.Substring(lastlinestart+1,3));
							}
							catch
							{
								endcode = -1;
							}		
							if (endcode != StatusCode)
								bIsComplete = false; // error invalid response					
						}
						
					}
					else if (statuscodestr[3] != ' ')
					{
						StatusCode = -1;														
					}
				}
			}
			else
			{			
				bIsComplete = false;						
			}
			return bIsComplete;                
		}

		private int SendData(Stream requestStream, Socket Accept)
		{
			if( Accept == null ) 
			{
				throw new ArgumentNullException(nameof(Accept));			
			}
            FtpStream ftpStream = requestStream as FtpStream;
			ftpStream.InternalPosition=0;

            int Length = (int)ftpStream.InternalLength;

			Byte [] sendbuffer = new Byte[Length];

            ftpStream.InternalRead(sendbuffer, 0, Length);		
			int cbReturn = Accept.Send( sendbuffer, Length, 0);

            ftpStream.InternalClose();
		
			return cbReturn;
		}
		private String FormatAddress(byte[] Address, int Port )
		{
			StringBuilder sb = new StringBuilder(32);

			sb.Append(string.Join(",", Address));
			sb.Append(',');
			sb.Append( Port / 256 );
			sb.Append(',');
			sb.Append(Port % 256 );
			return sb.ToString();
		}
		
		private int getPort(String str)
		{
			int Port=0;
			int pos1 = str.IndexOf("(");
			int pos2 = str.IndexOf(",");
			for(int i =0; i<3;i++) 
			{			
				pos1 = pos2+1;
				pos2 = str.IndexOf(",",pos1);
			}		
			pos1 = pos2+1;
			pos2 = str.IndexOf(",",pos1);		
			String PortSubstr1=str.Substring(pos1,pos2-pos1);
		
			pos1 = pos2+1;
			pos2 = str.IndexOf(")",pos1);
			String PortSubstr2=str.Substring(pos1,pos2-pos1);
		
			Port = Convert.ToInt32(PortSubstr1) * 256 ;
			Port = Port + Convert.ToInt32(PortSubstr2);									
			return Port;
		}
		internal void ResetError()
		{
			this._status = 0;
			this._statusDescription = "";
		}
		internal void ProcessApplicationError( string s)
		{
			this._status = 1;
			this._statusDescription = s;
			
		}
		internal void ProcessApplicationError( string s, Exception e)
		{
			this._status = 1;
			this._statusDescription = s + e.Message;
			
		}
		internal void ProcessProtocolViolationError( string s)
		{
			this._status = 1;
			this._statusDescription = new ProtocolViolationException(s).ToString();
			
		}
		internal string ComposeExceptionMessage(ResponseDescription resp_desc, string log) 
		{

			StringBuilder sb = new StringBuilder();

			sb.Append("FTP Protocol Error.....\n");
			sb.Append("Status: " + resp_desc.Status + "\n");
			sb.Append("Description: " + resp_desc.StatusDescription + "\n");
			sb.Append("\n");

			sb.Append("--------------------------------\n");
			sb.Append(log);
			sb.Append("\n");

			return sb.ToString();
		}
	
	}
	public class ResponseDescription 
	{
		private int _dwStatus;
		private string _szStatusDescription;

		public int Status 
		{
			get { return _dwStatus; }
			set { _dwStatus = value; }
		}
		public string StatusDescription 
		{
			get { return _szStatusDescription; }
			set { _szStatusDescription = value; }
		}
		public bool PositivePreliminary 
		{
			get { return ( _dwStatus / 100 == 1); }
		}
		public bool PositiveCompletion 
		{
			get { return ( _dwStatus / 100 == 2); }
		}
		public bool PositiveIntermediate 
		{
			get { return ( _dwStatus / 100 == 3); }
		}
		public bool TransientNegativeCompletion 
		{
			get { return ( _dwStatus / 100 == 4); }
		}
		public bool PermanentNegativeCompletion 
		{
			get { return ( _dwStatus / 100 == 5); }
		}
	}

	internal class FtpStream: Stream 
	{
		private Stream _Stream;
		private bool _fCanRead;
		private bool _fCanWrite;
		private bool _fCanSeek;

		private bool _fClosedByUser;

		internal FtpStream(Stream stream, bool canread, bool canwrite, bool canseek) 
		{
			_Stream = stream;
			_fCanRead = canread;
			_fCanWrite = canwrite;
			_fCanSeek = canseek;
		}
		public override bool CanRead 
		{
			get { return _fCanRead; }
		}
		public override bool CanWrite 
		{
			get { return _fCanWrite; }
		}
		public override bool CanSeek 
		{
			get { return _fCanSeek; }
		}
		public override long Length 
		{
			get 
			{
				throw new NotSupportedException("This stream cannot be seeked");
			}
		}
		public override long Position 
		{
			get 
			{
				throw new NotSupportedException("This stream cannot be seeked");
			}
			set 
			{
				throw new NotSupportedException("This stream cannot be seeked");
			}
		}
		public override long Seek(long offset, SeekOrigin origin) 
		{
			throw new NotSupportedException("This stream cannot be seeked");
		}
		public override void Flush() 
		{
		}
		public override void
			SetLength(long value) 
		{
			throw new NotSupportedException("This stream cannot be seeked");
		}
		public override void Close() 
		{
			_fClosedByUser = true;
		}
		public override void Write(Byte [] buffer, int offset, int length) 
		{
			if(_fClosedByUser)
				throw new IOException("Cannot operate on a closed stream");
			
			InternalWrite(buffer,offset,length);
		}

		internal void InternalWrite(Byte [] buffer, int offset, int length) 
		{
			_Stream.Write(buffer,offset,length);
		}

		public override int Read(Byte [] buffer, int offset, int length) 
		{
			if(_fClosedByUser)
				throw new IOException("Cannot operate on a closed stream");
			
			return InternalRead(buffer,offset,length);
		}
		internal int InternalRead(Byte [] buffer, int offset, int length) 
		{
			return _Stream.Read(buffer,offset,length);
		}
		internal long InternalPosition 
		{
			set 
			{
				_Stream.Position = value;
			}
		}
		internal long InternalLength 
		{
			get 
			{
				return _Stream.Length;
			}
		}
		internal void InternalClose()
		{
			_Stream.Close();
		}
	}
} 
