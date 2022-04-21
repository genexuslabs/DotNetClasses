namespace GeneXus.Http.Client
{
	using System;
	using System.Text;
	using System.Collections.Specialized;
	using System.IO;
	using log4net;
	using GeneXus.Application;
	using System.Net;
	using GeneXus.Configuration;
	using GeneXus.Utils;
	using System.Text.RegularExpressions;
	using Mime;
	using System.Collections;
	using System.Security.Cryptography.X509Certificates;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Threading.Tasks;
	using System.Web.Services.Protocols;
	using System.Web;
	using System.Net.Http.Headers;
	using System.Net.Http;
	using System.Security;


	public interface IGxHttpClient
	{
		Stream SendStream
		{
			get;
		}
		Stream ReceiveStream
		{
			get;
		}
	}
	public class MultiPartTemplate
	{
		public string Boundary;
		public string FormdataTemplate;
		public byte[] Boundarybytes;
		public byte[] EndBoundaryBytes;
		public string HeaderTemplate;
		public string ContentType;

		internal MultiPartTemplate()
		{
			Boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
			ContentType = $"multipart/form-data; boundary={Boundary}";
			FormdataTemplate = "\r\n--" + Boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
			Boundarybytes = Encoding.ASCII.GetBytes($"\r\n--{Boundary}\r\n");
			EndBoundaryBytes = Encoding.ASCII.GetBytes($"\r\n--{Boundary}--");
			HeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" + "Content-Type: {2}\r\n\r\n";
		}
	}
	public class GxHttpClient : IGxHttpClient
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Http.Client.GxHttpClient));
		public const int _Basic = 0;
		public const int _Digest = 1;
		public const int _NTLM = 2;
		public const int _Kerberos = 3;
		Stream _sendStream;
		Stream _receiveStream;
		int _timeout = 30000;
		short _statusCode = 0;
		string _proxyHost;
		int _proxyPort;
		short _errCode = 0;
		string _errDescription = string.Empty;
		NameValueCollection _headers;
		NameValueCollection _formVars;
		MultiPartTemplate _multipartTemplate;
		HttpClientHandler handlerInstance;


		string _scheme = "http://";
		string _host;
		int _port;
		string _wsdlUrl;
		string _baseUrl;
		string _url;
		string _statusDescription = string.Empty;
		IGxContext _context;
#if NETCORE
		IWebProxy _proxyObject;
#else
		WebProxy _proxyObject;
#endif
		ArrayList _authCollection;
		ArrayList _authProxyCollection;
		X509Certificate2Collection _certificateCollection;
		Encoding _encoding;
		Encoding _contentEncoding;


		public MultiPartTemplate MultiPart
		{
			get
			{
				if (_multipartTemplate == null)
					_multipartTemplate = new MultiPartTemplate();
				return _multipartTemplate;
			}
		}
		public Stream SendStream
		{
			get
			{
				if (_sendStream == null)
					_sendStream = new MemoryStream();
				return _sendStream;
			}
			set
			{
				_sendStream = value;
			}
		}
		public Stream ReceiveStream
		{
			get
			{
				if (_receiveStream == null)
					_receiveStream = new MemoryStream();

				return _receiveStream;
			}
		}

#if NETCORE
		[SecurityCritical]
		private HttpClientHandler GetHandler()
		{
			lock (syncRoot)
			{
				if (handlerInstance == null)
					handlerInstance = new HttpClientHandler();
			}
			return handlerInstance;
		}
#else
		[SecurityCritical]
		private WinHttpHandler GetHandler()
		{
			lock (syncRoot)
			{
				if (handlerInstance == null)
					handlerInstance = new WinHttpHandler();
			}
			return handlerInstance;
		}
#endif
		public GxHttpClient(IGxContext context) : this()
		{
			_context = context;
		}
		public GxHttpClient() : base()
		{
			_headers = new NameValueCollection();
			_formVars = new NameValueCollection();
			_host = string.Empty;
			_wsdlUrl = string.Empty;
			_baseUrl = string.Empty;
			_url = string.Empty;
			_authCollection = new ArrayList();
			_authProxyCollection = new ArrayList();
			_certificateCollection = new X509Certificate2Collection();
			IncludeCookies = true;


			_proxyHost = string.Empty;
			try
			{
#if NETCORE
				_proxyObject = WebRequest.GetSystemWebProxy();

#else
				_proxyObject = WebProxy.GetDefaultProxy();
				if (_proxyObject != null && _proxyObject.Address != null)
				{
					_proxyHost = _proxyObject.Address.Host;
					_proxyPort = _proxyObject.Address.Port;
				}
#endif
			}
			catch
			{
				_proxyObject = null;
			}

		}

		public short Digest
		{
			get { return _Digest; }
		}
		public short getDigest()
		{
			return _Digest;
		}
		public short Basic
		{
			get { return _Basic; }
		}
		public short getBasic()
		{
			return _Basic;
		}

		public short NTLM
		{
			get { return _NTLM; }
		}
		public short getNTLM()
		{
			return _NTLM;
		}
		public short Kerberos
		{
			get { return _Kerberos; }
		}
		public short ErrCode
		{
			get { return _errCode; }
		}

		public string ErrDescription
		{
			get { return _errDescription; }
		}
		public string Host
		{
			get { return _host; }
			set
			{
				_host = value == null ? value : value.Trim();
				buildUrl();
			}
		}
		public string WSDLURL
		{
			get { return _wsdlUrl; }
			set
			{
				_wsdlUrl = value == null ? value : value.Trim();
			}
		}
		public string BaseURL
		{
			get { return _baseUrl; }
			set
			{
				_baseUrl = value == null ? value : value.Trim();
				buildUrl();
			}
		}
		public int Port
		{
			get { return _port; }
			set
			{
				_port = value;
				buildUrl();
			}
		}
		public int ProxyServerPort
		{
			get { return _proxyPort; }
			set { _proxyPort = value; }
		}
		public void set_ProxyPort(int port)
		{
			_proxyPort = port;
		}
		public int get_ProxyPort()
		{
			return _proxyPort;
		}

		public string ProxyServerHost
		{
			get { return _proxyHost; }
			set { _proxyHost = value; }
		}
		public bool IncludeCookies { get; set; }
		public void set_ProxyHost(string host)
		{
			_proxyHost = host;
		}
		public string get_ProxyHost()
		{
			return _proxyHost;
		}
		public void AddAuthentication(int scheme, string realm, string user, string password)
		{
			if (scheme >= _Basic && scheme <= _Kerberos)
				_authCollection.Add(new GxAuthScheme(scheme, realm, user, password));
		}

		public void AddProxyAuthentication(int scheme, string realm, string user, string password)
		{
			if (scheme >= _Basic && scheme <= _Kerberos)
				_authProxyCollection.Add(new GxAuthScheme(scheme, realm, user, password));
		}

		public int Secure
		{
			get { return _scheme == "https://" ? 1 : 0; }
			set
			{
				if (value == 1)
					_scheme = "https://";
				else
					_scheme = "http://";
				buildUrl();
			}
		}
		public string Url
		{
			get { return _url; }
			set { _url = value; }
		}
		public int Timeout
		{
			get { return _timeout / 1000; }
			set
			{
				if (value == 0)
					_timeout = 3600000;
				else
					_timeout = value * 1000;
			}

		}
		public short StatusCode
		{
			get { return _statusCode; }
		}
		public string ReasonLine
		{
			get { return _statusDescription; }
		}
		void buildUrl()
		{
			string sPort, sHost, sBaseUrl;
			if (_port == 0)
				sPort = (Secure == 1) ? ":443" : string.Empty;
			else
				sPort = ":" + _port.ToString();
			sHost = _host;
			if (!string.IsNullOrEmpty(sHost))
			{
				if (sHost.StartsWith("//"))
					sHost = sHost.Substring(2, sHost.Length - 2);
				if (sHost.EndsWith("/"))
					sHost = sHost.Substring(0, sHost.Length - 1);
			}
			sBaseUrl = _baseUrl;
			if (!string.IsNullOrEmpty(sBaseUrl) && sBaseUrl.StartsWith("/"))
				sBaseUrl = sBaseUrl.Substring(1, sBaseUrl.Length - 1);
			_url = _scheme + sHost + sPort + "/" + sBaseUrl;
			if (_url.EndsWith("/"))
				_url = _url.Substring(0, _url.Length - 1);
		}
		public void ClearHeaders()
		{
			_headers.Clear();
		}
		public void AddHeader(string name, string value)
		{
			if (name.Equals("content-type", StringComparison.OrdinalIgnoreCase))
			{
				if (value.StartsWith(MediaTypesNames.MultipartFormData, StringComparison.OrdinalIgnoreCase) &&
					value.IndexOf("boundary=") == -1)
				{
					IsMultipart = true;
					value = MultiPart.ContentType;
				}

				try
				{
					int index = value.LastIndexOf("charset", StringComparison.OrdinalIgnoreCase);
					if (index > 0)
					{
						int equalsIndex = value.IndexOf('=', index) + 1;
						if (equalsIndex >= 0)
						{
							String charset = value.Substring(equalsIndex).Trim();
							int lastIndex = charset.IndexOf(' ');
							if (lastIndex != -1)
							{
								charset = charset.Substring(0, lastIndex);
							}
							charset = charset.Replace('\"', ' ').Replace('\'', ' ').Trim();
							_contentEncoding = GetEncoding(charset);
						}
					}
				}
				catch (Exception ex)
				{
					GXLogging.Error(log, String.Format("Error parsing charset ", value, ex));
				}
			}
			_headers.Set(name, value);
		}
		public void ClearVariables()
		{
			_formVars.Clear();
		}
		public void AddVariable(string name, string value)
		{
			_formVars.Add(name, value);
		}

		void sendVariables(Stream reqStream)
		{
			List<string> vars = new List<string>();
			for (int i = 0; i < _formVars.Count; i++)
			{
				if (_formVars.Keys[i] != null)
					vars.Add(buildVariableToSend(_formVars.Keys[i], _formVars[i]));
			}
			if (vars.Count > 0)
			{
				string buffer = string.Join(variableSeparator(), vars.ToArray());
				StreamWriter sw = new StreamWriter(reqStream);
				sw.Write(buffer);
				sw.Flush();
			}
		}
		bool IsMultipart { get; set; }

		string variableSeparator()
		{
			if (IsMultipart)
				return string.Empty;
			else
				return "&";
		}
		string buildVariableToSend(string key, string value)
		{
			if (IsMultipart)
			{
				return string.Format(MultiPart.FormdataTemplate, key, value);
			}
			else
			{
				return GXUtil.UrlEncode(key) + "=" + GXUtil.UrlEncode(value);
			}
		}
		public void AddString(string s)
		{
			StreamWriter sw;
			if (_contentEncoding != null)
				sw = new StreamWriter(SendStream, _contentEncoding);
			else
				sw = new StreamWriter(SendStream);
			sw.Write(s);
			sw.Flush();
		}
		public void ClearFiles()
		{
			ClearSendStream();
		}
		public void ClearStrings()
		{
			ClearSendStream();
		}
		private void ClearSendStream()
		{
			SendStream = null;
		}
		public void AddFile(string s, string name)
		{
			StartMultipartFile(name, s);
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			using (FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			{
				byte[] buffer = new Byte[1024];
				int bytesRead = fs.Read(buffer, 0, 1024);
				while (bytesRead > 0)
				{
					SendStream.Write(buffer, 0, bytesRead);
					bytesRead = fs.Read(buffer, 0, 1024);
				}
			}
		}

		private void StartMultipartFile(string name, string s)
		{
			if (IsMultipart && File.Exists(s))
			{
				s = Path.GetFileName(s);
				if (string.IsNullOrEmpty(name))
				{
					name = Path.GetFileNameWithoutExtension(s);
				}
				SendStream.Write(MultiPart.Boundarybytes, 0, MultiPart.Boundarybytes.Length);
				string header = string.Format(MultiPart.HeaderTemplate, name, s, MimeMapping.GetMimeMapping(s));
				byte[] headerbytes = Encoding.UTF8.GetBytes(header);
				SendStream.Write(headerbytes, 0, headerbytes.Length);
			}
		}
		private void EndMultipartBoundary(Stream reqStream)
		{
			if (IsMultipart)
				reqStream.Write(MultiPart.EndBoundaryBytes, 0, MultiPart.EndBoundaryBytes.Length);
		}

		void setHeaders(HttpRequestMessage request, CookieContainer cookies)
		{
			HttpContentHeaders contentHeaders = request.Content.Headers;
			HttpRequestHeaders headers = request.Headers;
			string contentType = null;
			for (int i = 0; i < _headers.Count; i++)
			{
				string currHeader = _headers.Keys[i];
				string upperHeader = currHeader.ToUpper();
				switch (upperHeader)
				{
					case "CONNECTION":
						if (_headers[i].ToUpper() == "CLOSE")
							headers.ConnectionClose = true;
						break;
					case "CONTENT-TYPE":
						contentType = _headers[i].ToString();
						contentHeaders.ContentType = MediaTypeHeaderValue.Parse(_headers[i].ToString());
						break;
					case "ACCEPT":
						headers.Add("Accept", _headers[i]);
						break;
					case "EXPECT":
						if (string.IsNullOrEmpty(_headers[i]))
							headers.ExpectContinue = false;
						else
							headers.ExpectContinue = true;
						break;
					case "REFERER":
						headers.Referrer = new Uri(_headers[i]);
						break;
					case "USER-AGENT":
						headers.Add("User-Agent", _headers[i]);
						break;
					case "DATE":
						DateTime value;
						if (DateTime.TryParseExact(_headers[i], "ddd, dd MMM yyyy HH:mm:ss Z", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
						{
							headers.Date = value;
						}
						else
						{
							headers.Add(currHeader, _headers[i]);
						}
						break;
					case "COOKIE":
						string allCookies = _headers[i];
						foreach (string cookie in allCookies.Split(';'))
						{
							if (cookie.Contains("="))
							{
								cookies.Add(new Uri(request.RequestUri.Host), new Cookie(cookie.Split('=')[0], cookie.Split('=')[1]) { Domain = request.RequestUri.Host });
							}
						}
						break;
					case "IF-MODIFIED-SINCE":
						DateTime dt;
						if (DateTime.TryParse(_headers[i], DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, out dt))
							request.Headers.IfModifiedSince = dt;
						break;
					default:
						headers.Add(currHeader, _headers[i]);
						break;
				}
			}
			string httpConnection;
			if (Config.GetValueOf("HttpClientConnection", out httpConnection))
			{
				if (httpConnection == "Close")
					headers.ConnectionClose = true;
				else
					headers.ConnectionClose = false;
			}
			InferContentType(contentType, request);
		}
		void InferContentType(string contentType, HttpRequestMessage req)
		{
			if (string.IsNullOrEmpty(contentType) && _formVars.Count > 0)
			{
				req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
			}
		}

		void setHttpVersion(HttpRequestMessage req)
		{
			string httpVersion;
			if (Config.GetValueOf("HttpClientHttpVersion", out httpVersion))
			{
				if (httpVersion == "1.0")
					req.Version = HttpVersion.Version10;
				else
					req.Version = HttpVersion.Version11;
			}
			else
				req.Version = HttpVersion.Version11;
		}
		[SecuritySafeCritical]
		HttpResponseMessage ExecuteRequest(string method, string requestUrl, CookieContainer cookies)
		{
			GXLogging.Debug(log, String.Format("Start NetCore HTTPClient buildRequest: requestUrl:{0} method:{1}", requestUrl, method));
			HttpRequestMessage request;
			HttpClient client;
			int BytesRead;
			Byte[] Buffer = new Byte[1024];

			request = new HttpRequestMessage();
			request.RequestUri = new Uri(requestUrl);
#if NETCORE
			HttpClientHandler handler = GetHandler();
			handler.Credentials = getCredentialCache(request.RequestUri, _authCollection);
#else
			WinHttpHandler handler = GetHandler();
			handler.ServerCredentials = getCredentialCache(request.RequestUri, _authCollection);
#endif

			if (GXUtil.CompressResponse())
			{
				handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			}
			handler.CookieContainer = cookies;

			foreach (X509Certificate2 cert in _certificateCollection)
				handler.ClientCertificates.Add(cert);

			request.Method = new HttpMethod(method);
			setHttpVersion(request);
			WebProxy proxy = getProxy(_proxyHost, _proxyPort, _authProxyCollection);
			if (proxy != null)
				handler.Proxy = proxy;
			HttpResponseMessage response;
			using (client = new HttpClient(handler))
			{
				client.Timeout = TimeSpan.FromMilliseconds(_timeout);
				client.BaseAddress = request.RequestUri;

				using (MemoryStream reqStream = new MemoryStream())
				{
					sendVariables(reqStream);
					SendStream.Seek(0, SeekOrigin.Begin);
					BytesRead = SendStream.Read(Buffer, 0, 1024);
					GXLogging.Debug(log, "Start SendStream.Read: BytesRead " + BytesRead);
					while (BytesRead > 0)
					{
						GXLogging.Debug(log, "reqStream.Write: Buffer.length " + Buffer.Length + ",'" + Encoding.UTF8.GetString(Buffer, 0, Buffer.Length) + "'");
						reqStream.Write(Buffer, 0, BytesRead);
						BytesRead = SendStream.Read(Buffer, 0, 1024);
					}
					EndMultipartBoundary(reqStream);
					GXLogging.Debug(log, "End SendStream.Read: stream " + reqStream.ToString());
					reqStream.Seek(0, SeekOrigin.Begin);
					request.Content = new ByteArrayContent(reqStream.ToArray());
					setHeaders(request, handler.CookieContainer);
					response = client.SendAsync(request).GetAwaiter().GetResult();
				}
			}
			return response;
		}
		void ReadReponseContent(HttpResponseMessage response)
		{
			_receiveStream = new MemoryStream();
			int BytesRead;
			Byte[] Buffer = new Byte[1024];
			try
			{
				Stream stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
				string charset;
				if (response.Content.Headers.ContentType == null)
					charset = null;
				else
					charset = response.Content.Headers.ContentType.CharSet;
				Buffer = new Byte[1024];
				BytesRead = stream.Read(Buffer, 0, 1024);
				GXLogging.Debug(log, "BytesRead " + BytesRead);
				bool encodingFound = false;
				if (!string.IsNullOrEmpty(charset))
				{
					int idx = charset.IndexOf("charset=");
					if (idx > 0)
					{
						idx += 8;
						charset = charset.Substring(idx, charset.Length - idx);
						_encoding = GetEncoding(charset);
						if (_encoding != null)
							encodingFound = true;
					}
					else
					{
						charset = String.Empty;
					}
				}
				while (BytesRead > 0)
				{
					if (!encodingFound)
					{
						_encoding = DetectEncoding(charset, out encodingFound, Buffer, BytesRead);
					}
					_receiveStream.Write(Buffer, 0, BytesRead);
					BytesRead = stream.Read(Buffer, 0, 1024);
					GXLogging.Debug(log, "BytesRead " + BytesRead);
				}
			}
			catch (IOException ioEx)
			{
				if (_errCode == 1)
					GXLogging.Warn(log, "Could not read response", ioEx);
				else
					throw ioEx;
			}
			_receiveStream.Seek(0, SeekOrigin.Begin);
		}
		public void Execute(string method, string name)
		{
			if (Config.GetValueOf("useoldhttpclient", out string useOld) && useOld.StartsWith("y", StringComparison.OrdinalIgnoreCase))
			{
				WebExecute(method, name);
			}
			else
			{
				HttpClientExecute(method, name);
			}
		}

		public void HttpClientExecute(string method, string name)
		{
			HttpResponseMessage response = null;
			Byte[] Buffer = new Byte[1024];
			_errCode = 0;
			_errDescription = string.Empty;
			GXLogging.Debug(log, "Start Execute: method '" + method + "', name '" + name + "'");
			try
			{
				string requestUrl = GetRequestURL(name);
				bool contextCookies = _context != null && !String.IsNullOrEmpty(requestUrl);
				CookieContainer cookies = contextCookies ? _context.GetCookieContainer(requestUrl, IncludeCookies) : new CookieContainer();
				response = ExecuteRequest(method, requestUrl, cookies);
#if NETCORE
				if (contextCookies)
					_context.UpdateSessionCookieContainer();
#endif
			}
#if NETCORE
			catch (AggregateException aex)
			{
				GXLogging.Warn(log, "Error Execute", aex);
				_errCode = 1;
				if (aex.InnerException != null)
					_errDescription = aex.InnerException.Message;
				else
					_errDescription = aex.Message;
				response = new HttpResponseMessage();
				response.Content = new StringContent(_errDescription);
				response.StatusCode = HttpStatusCode.InternalServerError;
			}
#endif
			catch (HttpRequestException e)
			{
				GXLogging.Warn(log, "Error Execute", e);
				_errCode = 1;
				if (e.InnerException != null)
					_errDescription = e.Message + " " + e.InnerException.Message;
				else
					_errDescription = e.Message;
				response = new HttpResponseMessage();
				response.Content = new StringContent(_errDescription);
#if NETCORE
				response.StatusCode = (HttpStatusCode)(e.StatusCode != null ? e.StatusCode : HttpStatusCode.InternalServerError);
#else
				response.StatusCode = HttpStatusCode.InternalServerError;
#endif
			}
			catch (TaskCanceledException e)
			{
				GXLogging.Warn(log, "Error Execute", e);
				_errCode = 1;
				_errDescription = "The request has timed out. " + e.Message;
				response = new HttpResponseMessage();
				response.StatusCode = 0;
				response.Content = new StringContent("");
			}
			catch (Exception e)
			{
				GXLogging.Warn(log, "Error Execute", e);
				_errCode = 1;
				if (e.InnerException != null)
					_errDescription = e.Message + " " + e.InnerException.Message;
				else
					_errDescription = e.Message;
				response = new HttpResponseMessage();
				response.Content = new StringContent(_errDescription);
				response.StatusCode = HttpStatusCode.InternalServerError;
			}
			GXLogging.Debug(log, "Reading response...");
			if (response == null)
				return;
			LoadResponseHeaders(response);
			ReadReponseContent(response);
			_statusCode = ((short)response.StatusCode);
			_statusDescription = response.ReasonPhrase;
			if ((_statusCode >= 400 && _statusCode < 600) && _errCode != 1)
			{
				_errCode = 1;
				_errDescription = "The remote server returned an error: (" + _statusCode + ") " + response.ReasonPhrase + ".";
			}
			ClearSendStream();
			GXLogging.Debug(log, "_responseString " + ToString());
		}
		NameValueCollection _respHeaders;
		void LoadResponseHeaders(HttpResponseMessage resp)
		{
			_respHeaders = new NameValueCollection();
			foreach (KeyValuePair<string, IEnumerable<string>> header in resp.Headers)
			{
				_respHeaders.Add(header.Key, String.Join(",", header.Value));
			}
			foreach (KeyValuePair<string, IEnumerable<string>> header in resp.Content.Headers)
			{
				_respHeaders.Add(header.Key, String.Join(",", header.Value));
			}
		}
		private void setHeaders(HttpWebRequest req)
		{
			string contentType = null;
			for (int i = 0; i < _headers.Count; i++)
			{
				string currHeader = _headers.Keys[i];
				string upperHeader = currHeader.ToUpper();
				switch (upperHeader)
				{
					case "CONNECTION":
						if (_headers[i].ToUpper() == "CLOSE")
							req.SetKeepAlive(false);
						break;
					case "CONTENT-TYPE":
						contentType = _headers[i];
						req.ContentType = _headers[i];
						break;
					case "ACCEPT":
						req.Accept = _headers[i];
						break;
					case "EXPECT":
						if (string.IsNullOrEmpty(_headers[i]))
							req.ServicePoint.Expect100Continue = false;
						else
							req.SetExpect(_headers[i]);
						break;
					case "REFERER":
						req.SetReferer(_headers[i]);
						break;
					case "USER-AGENT":
						req.SetUserAgent(_headers[i]);
						break;
					case "DATE":
						DateTime value;
						if (DateTime.TryParseExact(_headers[i], "ddd, dd MMM yyyy HH:mm:ss Z", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
						{
							req.Date = value;
						}
						else
						{
#if NETCORE
							req.Headers[currHeader] = _headers[i];
#else
							req.Headers.Add(currHeader, _headers[i]);
#endif
						}
						break;
					case "COOKIE":
						string allCookies = _headers[i];
						foreach (string cookie in allCookies.Split(';'))
						{
							if (cookie.Contains("="))
							{
#if NETCORE
								req.CookieContainer.Add(new Uri(req.RequestUri.Host), new Cookie(cookie.Split('=')[0], cookie.Split('=')[1]) { Domain = req.RequestUri.Host });
#else
								req.CookieContainer.Add(new Cookie(cookie.Split('=')[0], cookie.Split('=')[1]) { Domain = req.RequestUri.Host });
#endif
							}
						}
						break;
					case "IF-MODIFIED-SINCE":
#if !NETCORE
						DateTime dt;
						if (DateTime.TryParse(_headers[i], DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, out dt))
							req.IfModifiedSince = dt;
						break;
#endif
					default:
#if NETCORE
						req.Headers[currHeader] = _headers[i];
#else
						req.Headers.Add(currHeader, _headers[i]);
#endif
						break;
				}
			}
			string httpConnection;
			if (Config.GetValueOf("HttpClientConnection", out httpConnection))
			{
				if (httpConnection == "Close")
					req.SetKeepAlive(false);
				else
					req.SetKeepAlive(true);
			}
			InferContentType(contentType, req);
		}
		private void InferContentType(string contentType, HttpWebRequest req)
		{

			if (string.IsNullOrEmpty(contentType) && _formVars.Count > 0)
			{
				req.ContentType = "application/x-www-form-urlencoded";
			}
		}

		WebProxy getProxy(string proxyHost, int proxyPort, ArrayList authenticationCollection)
		{
			if (proxyHost.Length > 0)
			{
				WebProxy newProxy = new WebProxy(proxyHost, proxyPort);
#if !NETCORE
				if (_proxyObject != null)
				{
					newProxy.BypassProxyOnLocal = _proxyObject.BypassProxyOnLocal;
					newProxy.BypassList = _proxyObject.BypassList;
				}
#endif
				newProxy.Credentials = getCredentialCache(newProxy.Address, authenticationCollection);
				return newProxy;
			}
			return null;
		}

		private void setHttpVersion(HttpWebRequest req)
		{

			string httpVersion;
			if (Config.GetValueOf("HttpClientHttpVersion", out httpVersion))
			{
				if (httpVersion == "1.0")
					req.ProtocolVersion = HttpVersion.Version10;
				else
					req.ProtocolVersion = HttpVersion.Version11;
			}
			else
				req.ProtocolVersion = HttpVersion.Version11;
		}

		public string GetRequestURL(string name)
		{
			if (String.IsNullOrEmpty(_url))
				return name;
			else
			{
				if (!string.IsNullOrEmpty(name) && name.IndexOf('/') == 0)
					return _url + name;
				else
					return _url + "/" + name;
			}
		}
#if !NETCORE
		public void ConfigureHttpClientProtocol(string name, SoapHttpClientProtocol httpC)
		{
			string url = GetRequestURL(name);
			httpC.Url = url;
			httpC.Credentials = getCredentialCache(new Uri(url), _authCollection);
			WebProxy proxy = getProxy(_proxyHost, _proxyPort, _authProxyCollection);
			if (proxy != null)
				httpC.Proxy = proxy;
			foreach (X509Certificate2 cert in _certificateCollection)
				httpC.ClientCertificates.Add(cert);
			httpC.Timeout = _timeout;
		}
#endif
		HttpWebRequest buildRequest(string method, string requestUrl, CookieContainer cookies)
		{
			GXLogging.Debug(log, String.Format("Start HTTPClient buildRequest: requestUrl:{0} method:{1}", requestUrl, method));
			int BytesRead;
			Byte[] Buffer = new Byte[1024];
#pragma warning disable SYSLIB0014 // WebRequest
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(requestUrl);
#pragma warning disable SYSLIB0014 // WebRequest

			if (GXUtil.CompressResponse())
			{
				req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			}

			req.Credentials = getCredentialCache(req.RequestUri, _authCollection);
			req.CookieContainer = cookies;
			foreach (X509Certificate2 cert in _certificateCollection)
				req.ClientCertificates.Add(cert);
			req.Method = method.Trim();
			req.Timeout = _timeout;
			setHttpVersion(req);
			WebProxy proxy = getProxy(_proxyHost, _proxyPort, _authProxyCollection);
			if (proxy != null)
				req.Proxy = proxy;

			setHeaders(req);

			if (!method.Equals(HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase) && !method.Equals(HttpMethod.Head.Method, StringComparison.OrdinalIgnoreCase))
			{
#if !NETCORE
				using (Stream reqStream = req.GetRequestStream())
#else
				using (Stream reqStream = req.GetRequestStreamAsync().GetAwaiter().GetResult())
#endif
				{
					sendVariables(reqStream);
					SendStream.Seek(0, SeekOrigin.Begin);
					BytesRead = SendStream.Read(Buffer, 0, 1024);
					GXLogging.Debug(log, "Start SendStream.Read: BytesRead " + BytesRead);
					while (BytesRead > 0)
					{
						GXLogging.Debug(log, "reqStream.Write: Buffer.length " + Buffer.Length + ",'" + Encoding.UTF8.GetString(Buffer, 0, Buffer.Length) + "'");
						reqStream.Write(Buffer, 0, BytesRead);
						BytesRead = SendStream.Read(Buffer, 0, 1024);
					}
					EndMultipartBoundary(reqStream);
				}
			}
			return req;
		}

		public ICredentials GetCredentials(string url)
		{
			return getCredentialCache(new Uri(url), _authCollection);
		}
		ICredentials getCredentialCache(Uri URI, ArrayList authenticationCollection)
		{
			string sScheme;
			GxAuthScheme auth;
			CredentialCache cc = new CredentialCache();

			for (int i = 0; i < authenticationCollection.Count; i++)
			{
				auth = (GxAuthScheme)authenticationCollection[i];
				switch (auth.Scheme)
				{
					case _Basic:
						sScheme = "Basic";
						break;
					case _Digest:
						sScheme = "Digest";
						break;
					case _NTLM:
						sScheme = "NTLM";
						break;
					case _Kerberos:
						sScheme = "Negotiate";
						break;
					default:
						continue;
				}
				try
				{
					if ((sScheme == "NTLM" || sScheme == "Negotiate") && auth.User.Trim().Length == 0 && auth.Password.Trim().Length == 0)
					{
						return System.Net.CredentialCache.DefaultCredentials;
					}
					else if (sScheme != "Basic")
					{
						cc.Add(URI, sScheme, new NetworkCredential(auth.User, auth.Password, auth.Realm));
					}
					else
					{
						cc.Add(URI, sScheme, new NetworkCredential(auth.User, auth.Password));
					}
				}
				catch (ArgumentException)
				{
				}
			}
			return cc;
		}

		private void WebExecute(string method, string name)
		{
			HttpWebRequest req;
			HttpWebResponse resp = null;
			int BytesRead;
			Byte[] Buffer = new Byte[1024];

			_errCode = 0;
			_errDescription = string.Empty;

			GXLogging.Debug(log, "Start Execute: method '" + method + "', name '" + name + "'");
			try
			{
				string requestUrl = GetRequestURL(name);
				bool contextCookies = _context != null && !String.IsNullOrEmpty(requestUrl);
				CookieContainer cookies = contextCookies ? _context.GetCookieContainer(requestUrl, IncludeCookies) : new CookieContainer();
				req = buildRequest(method, requestUrl, cookies);

#if NETCORE
				resp = req.GetResponse() as HttpWebResponse;
				if (contextCookies)
					_context.UpdateSessionCookieContainer();
#else
				resp = (HttpWebResponse)req.GetResponse();
#endif
			}
			catch (WebException e)
			{
				GXLogging.Warn(log, "Error Execute", e);
				_errCode = 1;
				_errDescription = e.Message;
				resp = (HttpWebResponse)(e.Response);
				if (resp == null)
					return;
			}
#if NETCORE
			catch (AggregateException aex)
			{
				GXLogging.Warn(log, "Error Execute", aex);
				_errCode = 1;
				_errDescription = aex.Message;

				var baseEx = aex.GetBaseException() as WebException;
				if (baseEx != null)
				{
					resp = baseEx.Response as HttpWebResponse;
					_errDescription = baseEx.Message;
				}
				if (resp == null)
					return;
			}
#endif

			GXLogging.Debug(log, "Reading response...");
			loadResponseHeaders(resp);
			_receiveStream = new MemoryStream();
			using (Stream rStream = resp.GetResponseStream())
			{
				try
				{
					Buffer = new Byte[1024];
					BytesRead = rStream.Read(Buffer, 0, 1024);
					GXLogging.Debug(log, "BytesRead " + BytesRead);
					bool encodingFound = false;
					String charset = resp.ContentType;
					if (!string.IsNullOrEmpty(charset))
					{
						int idx = charset.IndexOf("charset=");
						if (idx > 0)
						{
							idx += 8;
							charset = charset.Substring(idx, charset.Length - idx);
							_encoding = GetEncoding(charset);
							if (_encoding != null)
								encodingFound = true;
						}
						else
						{
							charset = String.Empty;
						}
					}
					while (BytesRead > 0)
					{
						if (!encodingFound)
						{
							_encoding = DetectEncoding(charset, out encodingFound, Buffer, BytesRead);
						}
						_receiveStream.Write(Buffer, 0, BytesRead);
						BytesRead = rStream.Read(Buffer, 0, 1024);
						GXLogging.Debug(log, "BytesRead " + BytesRead);
					}
				}
				catch (IOException ioEx)
				{
					if (_errCode == 1)
						GXLogging.Warn(log, "Could not read response", ioEx);
					else
						throw ioEx;
				}
			}
			_receiveStream.Seek(0, SeekOrigin.Begin);
			_statusCode = (short)resp.StatusCode;
			_statusDescription = resp.StatusDescription;
			resp.Close();
			ClearSendStream();
			GXLogging.Debug(log, "_responseString " + ToString());

		}
		private Encoding GetEncoding(string charset)
		{
			Encoding enc = null;
			try
			{
				enc = Encoding.GetEncoding(charset);
				switch (enc.CodePage)
				{
					case 65001:
						enc = new UTF8Encoding(false);
						break;
					case 1200:
						enc = new UnicodeEncoding(false, false);
						break;
					case 1201:
						enc = new UnicodeEncoding(true, false);
						break;
					case 12000:
						enc = new UTF32Encoding(false, false);
						break;
					case 12001:
						enc = new UTF32Encoding(true, false);
						break;
					default:

						break;
				}
			}
			catch (ArgumentException)
			{
			}
			return enc;
		}

		private Encoding DetectEncoding(string charset, out bool encodingFound, byte[] Buffer, int BytesRead)
		{
			Encoding enc = null;
			if (!string.IsNullOrEmpty(charset))
			{
				enc = GetEncoding(charset);
			}

			if (enc == null)
			{
				string responseText = Encoding.ASCII.GetString(Buffer, 0, BytesRead);
				if (responseText != null && !String.IsNullOrEmpty(responseText.Trim()))
				{
					enc = ExtractEncodingFromCharset(responseText, "encoding=[\'\"][^\'+ +\"]*[\'\"]", 10);
					if (enc == null)
					{
						enc = ExtractEncodingFromCharset(responseText, "charset=[\'\"][^\'+ +\"]*[\'\"]", 9);
					}
				}
			}
			if (enc != null)
			{
				encodingFound = true;
			}
			else
			{
				encodingFound = false;
			}
			return enc;
		}

		private Encoding ExtractEncodingFromCharset(string responseText, string regExpP, int startAt)
		{
			Encoding enc = null;

			Match m = Regex.Match(responseText, regExpP);
			string parsedEncoding = string.Empty;
			if (m != null && m.Success)
			{
				parsedEncoding = m.Value;
				parsedEncoding = parsedEncoding.Substring(startAt, parsedEncoding.Length - (startAt + 1));
				enc = GetEncoding(parsedEncoding);
			}
			return enc;
		}

		public override string ToString()
		{
			if (_encoding == null)
				_encoding = Encoding.UTF8;
			MemoryStream ms = (MemoryStream)ReceiveStream;
			ms.Seek(0, SeekOrigin.Begin);
			Byte[] Buffer = ms.ToArray();
			return _encoding.GetString(Buffer, 0, Buffer.Length);
		}
		public void ToFile(string fileName)
		{
			FileStream fs;
			string pathName = fileName;

#if !NETCORE
			if (HttpContext.Current != null)
#endif
			if (fileName.IndexOfAny(new char[] { '\\', ':' }) == -1)
				pathName = Path.Combine(GxContext.StaticPhysicalPath(), fileName);
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			using (fs = new FileStream(pathName, FileMode.Create, FileAccess.Write))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			{
				ReceiveStream.Seek(0, SeekOrigin.Begin);
				Byte[] Buffer = new Byte[1024];
				int BytesRead = ReceiveStream.Read(Buffer, 0, 1024);
				while (BytesRead > 0)
				{
					fs.Write(Buffer, 0, BytesRead);
					BytesRead = ReceiveStream.Read(Buffer, 0, 1024);
				}
				ReceiveStream.Seek(0, SeekOrigin.Begin);
			}
		}

		void loadResponseHeaders(WebResponse resp)
		{
#if NETCORE
			_respHeaders = new NameValueCollection();
			foreach (string key in resp.Headers.AllKeys)
			{
				_respHeaders.Add(key, resp.Headers[key]);
			}
#else
			_respHeaders = new NameValueCollection(resp.Headers);
#endif
		}

		public string GetHeader(string name)
		{
			if (_respHeaders != null && _respHeaders.Get(name) != null)
				return _respHeaders.Get(name);
			else
				return string.Empty;
		}
		public void GetHeader(string name, out short value)
		{
			value = Convert.ToInt16(GetHeader(name));
		}
		public void GetHeader(string name, out int value)
		{
			value = Convert.ToInt32(GetHeader(name));
		}
		public void GetHeader(string name, out long value)
		{
			value = Convert.ToInt64(GetHeader(name));
		}
		public void GetHeader(string name, out double value)
		{
			value = Convert.ToDouble(GetHeader(name));
		}
		public void GetHeader(string name, out string value)
		{
			value = GetHeader(name);
		}
		public void GetHeader(string name, out DateTime value)
		{
			value = Convert.ToDateTime(GetHeader(name));
		}
		public void AddCertificate(string cert)
		{
			Regex r = new Regex(@"(\s*\((?'fName'\S+)\s*\,\s*(?'pass'\S+)\s*\)|(?'fName'\S+))");
			foreach (Match m in r.Matches(cert))
				AddCertificate(m.Groups["fName"].Value, m.Groups["pass"].Value);
		}
		public void AddCertificate(string file, string pass)
		{
			X509Certificate2 c;
			if (pass == null || pass.Trim().Length == 0)
			{
				c = new X509Certificate2(file);
			}
			else
			{
				c = new X509Certificate2(file, pass);
			}
			_certificateCollection.Add(c);
		}
	}

	class GxAuthScheme
	{
		private int _scheme;
		private string _realm;
		private string _user;
		private string _password;

		public GxAuthScheme(int scheme, string realm, string user, string password)
		{
			_scheme = scheme;
			_realm = realm;
			_user = user;
			_password = password;
		}

		public int Scheme
		{
			get { return _scheme; }
		}
		public string Realm
		{
			get { return _realm; }
		}
		public string User
		{
			get { return _user; }
		}
		public string Password
		{
			get { return _password; }
		}

	}

}
