namespace GeneXus.Http.Client
{
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Security;
	using System.Security;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Services.Protocols;
	using GeneXus.Application;
	using GeneXus.Configuration;
	using GeneXus.Utils;

#if NETCORE
	using Microsoft.AspNetCore.WebUtilities;
#endif
	using Mime;


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
		public string FormdataSeparator;		
		public byte[] Boundarybytes;
		public byte[] EndBoundaryBytes;
		public string HeaderTemplate;
		public string ContentType;

		internal MultiPartTemplate()
		{
			Boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
			ContentType = $"multipart/form-data; boundary={Boundary}";
			FormdataSeparator = "\r\n";
			FormdataTemplate = "--" + Boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			Boundarybytes = Encoding.ASCII.GetBytes($"\r\n--{Boundary}\r\n");
			EndBoundaryBytes = Encoding.ASCII.GetBytes($"\r\n--{Boundary}--");
			HeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" + "Content-Type: {2}\r\n\r\n";
		}
	}
	public class GxHttpClient : IGxHttpClient, IDisposable
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GxHttpClient>();
		private const int DEFAULT_TIMEOUT = 30000;
		public const int _Basic = 0;
		public const int _Digest = 1;
		public const int _NTLM = 2;
		public const int _Kerberos = 3;
		const int StreamWriterDefaultBufferSize = 1024;
		Stream _sendStream;
		byte[] _receiveData;
		int _timeout = DEFAULT_TIMEOUT;
		short _statusCode = 0;
		string _proxyHost;
		int _proxyPort;
		short _errCode = 0;
		string _errDescription = string.Empty;
		NameValueCollection _headers;
		NameValueCollection _formVars;
		MultiPartTemplate _multipartTemplate;
		bool _isChunkedResponse;
		HttpResponseMessage _response;
		bool _eof;
		bool _encodingFound;
		string _charset;


		string _scheme = "http://";
		string _host;
		int _port;
		string _wsdlUrl;
		string _baseUrl;
		string _url;
		string _statusDescription = string.Empty;
		IGxContext _context;
#if NETCORE

		static IWebProxy _proxyObject;
		static object syncRootHttpInstance = new Object();
#else

		static WebProxy _proxyObject;
#endif
		ArrayList _authCollection;
		ArrayList _authProxyCollection;
		X509Certificate2Collection _certificateCollection;
		List<string> _fileCertificateCollection;
		Encoding _encoding;
		Encoding _contentEncoding;
		static object syncRoot = new Object();
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
				return null; 
			}
		}
		internal byte[] ReceiveData
		{
			get
			{
				ReadResponseData();
				return _receiveData;
			}
		}


#if NETCORE
		private async Task<byte[]> ReceiveDataAsync()
		{
			await ReadResponseDataAsync();
			return _receiveData;
		}

		private const int POOLED_CONNECTION_LIFETIME_MINUTES = 2;
		internal static ConcurrentDictionary<string, HttpClient> _httpClientInstances = new ConcurrentDictionary<string, HttpClient>();
		private static HttpClient GetHttpClientInstance(Uri URI, int timeout, ArrayList authCollection, ArrayList authProxyCollection, X509Certificate2Collection certificateCollection, List<string> fileCertificateCollection, string proxyHost, int proxyPort, out bool disposableInstance)
		{
			if (CacheableInstance(authCollection, authProxyCollection))
			{
				HttpClient value;
				disposableInstance = false;
				string key = HttpClientInstanceIdentifier(proxyHost, proxyPort, fileCertificateCollection, timeout);
				if (_httpClientInstances.TryGetValue(key, out value))
				{
					GXLogging.Debug(log, $"Getting httpClient cached instance");
					return value;
				}
				else
				{
					lock (syncRootHttpInstance)
					{
						if (_httpClientInstances.TryGetValue(key, out value))
						{
							GXLogging.Debug(log, $"Getting httpClient cached instance");
							return value;
						}
						value = new HttpClient(GetHandler(URI, authCollection, authProxyCollection, certificateCollection, proxyHost, proxyPort));
						value.Timeout = TimeSpan.FromMilliseconds(timeout);
						_httpClientInstances.TryAdd(key, value);
						return value;
					}
				}
			}
			else
			{
				disposableInstance = true;
				return new HttpClient(GetHandler(URI, authCollection, authProxyCollection, certificateCollection, proxyHost, proxyPort));
			}
		}

		private static string HttpClientInstanceIdentifier(string proxyHost, int proxyPort, List<string> fileCertificateCollection, int timeout)
		{
			bool defaultSslOptions = ServicePointManager.ServerCertificateValidationCallback == null;
			if (string.IsNullOrEmpty(proxyHost) && fileCertificateCollection.Count==0 && timeout== DEFAULT_TIMEOUT && defaultSslOptions)
			{
				return string.Empty;
			}
			else if (fileCertificateCollection.Count==0)
			{
				return $"{proxyHost}:{proxyPort}::{timeout}:{defaultSslOptions}";
			}
			else
			{
				return $"{proxyHost}:{proxyPort}:{string.Join(';', fileCertificateCollection)}:{timeout}:{defaultSslOptions}";
			}
		}

		private static bool CacheableInstance(ArrayList authCollection, ArrayList authProxyCollection)
		{
			return authCollection.Count == 0 && authProxyCollection.Count == 0 && Preferences.SingletonHttpClient();
		}
		private static SocketsHttpHandler GetHandler(Uri URI, ArrayList authCollection, ArrayList authProxyCollection, X509Certificate2Collection certificateCollection, string proxyHost, int proxyPort)
		{
			SocketsHttpHandler handler = new SocketsHttpHandler()
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(POOLED_CONNECTION_LIFETIME_MINUTES),
			};
			int maxConnections = Preferences.GetHttpClientMaxConnectionPerRoute();
			if (maxConnections != Preferences.DEFAULT_HTTPCLIENT_MAX_PER_ROUTE)
			{
				handler.MaxConnectionsPerServer = maxConnections;
			}
			GXLogging.Debug(log, $"Creating SocketsHttpHandler MaxConnectionsPerServer:{handler.MaxConnectionsPerServer}");
			ICredentials cc = getCredentialCache(URI, authCollection);
			if (cc != null)
			{
				handler.Credentials = getCredentialCache(URI, authCollection);
			}

			SetSslOptions(handler);

			if (GXUtil.CompressResponse())
			{
				handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			}
			if (certificateCollection.Count > 0)
			{
				if (handler.SslOptions.ClientCertificates == null)
				{
					handler.SslOptions.ClientCertificates = new X509CertificateCollection(certificateCollection);
				}
				else
				{
					foreach (X509Certificate2 cert in certificateCollection)
					{
						handler.SslOptions.ClientCertificates.Add(cert);
					}
				}
			}

			WebProxy proxy = getProxy(proxyHost, proxyPort, authProxyCollection);
			if (proxy != null)
			{
				handler.Proxy = proxy;
			}
			handler.UseCookies = false;
			return handler;

		}

		private static void SetSslOptions(SocketsHttpHandler handler)
		{
			if (ServicePointManager.ServerCertificateValidationCallback != null)
			{
				handler.SslOptions = new SslClientAuthenticationOptions
				{
					RemoteCertificateValidationCallback = ServicePointManager.ServerCertificateValidationCallback
				};
			}
		}
#else
		[SecuritySafeCritical]

		private static HttpClient GetHttpClientInstance(Uri URI, int timeout, ArrayList authCollection, ArrayList authProxyCollection, X509Certificate2Collection certificateCollection, string proxyHost, int proxyPort, CookieContainer cookies)
		{
			TimeSpan milliseconds = TimeSpan.FromMilliseconds(timeout);
			HttpClient value = new HttpClient(GetHandler(URI, milliseconds, authCollection, authProxyCollection, certificateCollection, proxyHost, proxyPort, cookies));
			value.Timeout = milliseconds;
			return value;
		}
		[SecuritySafeCritical]
		private static WinHttpHandler GetHandler(Uri URI, TimeSpan milliseconds, ArrayList authCollection, ArrayList authProxyCollection, X509Certificate2Collection certificateCollection, string proxyHost, int proxyPort, CookieContainer cookies)
		{
			WinHttpHandler handler = new WinHttpHandler();
			ICredentials cc = getCredentialCache(URI, authCollection);
			if (cc != null)
			{
				handler.ServerCredentials = cc;
			}
			if (ServicePointManager.ServerCertificateValidationCallback != null)
			{
				handler.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => ServicePointManager.ServerCertificateValidationCallback(sender, certificate, chain, sslPolicyErrors));
			}
			handler.CookieUsePolicy = CookieUsePolicy.UseSpecifiedCookieContainer;
			handler.CookieContainer = cookies;
			if (GXUtil.CompressResponse())
			{
				handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			}
			foreach (X509Certificate2 cert in certificateCollection)
				handler.ClientCertificates.Add(cert);
			WebProxy proxy = getProxy(proxyHost, proxyPort, authProxyCollection);
			if (proxy != null)
			{
				handler.Proxy = proxy;
				handler.WindowsProxyUsePolicy = WindowsProxyUsePolicy.UseCustomProxy;
			}
			
			handler.ReceiveDataTimeout = milliseconds;
			handler.ReceiveHeadersTimeout = milliseconds;

			return handler;
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
			_fileCertificateCollection = new List<string>();
			IncludeCookies = true;


			_proxyHost = string.Empty;
			try
			{
#if !NETCORE
				if (ProxyObject != null && ProxyObject.Address != null)
				{
					_proxyHost = ProxyObject.Address.Host;
					_proxyPort = ProxyObject.Address.Port;
				}
#endif
			}
			catch
			{
				_proxyObject = null;
			}

		}
#if NETCORE

		private IWebProxy ProxyObject
#else

		private static WebProxy ProxyObject
#endif
		{
			get {
				if (_proxyObject == null)
				{
					lock (syncRoot)
					{
						try
						{
#if NETCORE
							_proxyObject = WebRequest.GetSystemWebProxy();
#else
							_proxyObject = WebProxy.GetDefaultProxy();
#endif
						}
						catch (Exception e)
						{
							GXLogging.Warn(log, "Error getting ProxyObject", e);
						}
					}
				}
				return _proxyObject;
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
			if (IsAbsolute(sBaseUrl))
			{
				_url = sBaseUrl;
			}
			else
			{
				if (!string.IsNullOrEmpty(sBaseUrl) && sBaseUrl.StartsWith("/"))
					sBaseUrl = sBaseUrl.Substring(1, sBaseUrl.Length - 1);

				_url = _scheme + sHost + sPort + "/" + sBaseUrl;
			}
			if (_url.EndsWith("/"))
				_url = _url.Substring(0, _url.Length - 1);
		}
		bool IsAbsolute(string url)
		{
			return Uri.IsWellFormedUriString(url, UriKind.Absolute);
		}

		public void ClearHeaders()
		{
			_headers.Clear();
		}
		public void AddHeader(string name, string value)
		{
			GXLogging.Debug(log, "AddHeader ", name, "=", value);

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

		void SendVariables(Stream reqStream)
		{
			List<string> vars = new List<string>();
			for (int i = 0; i < _formVars.Count; i++)
			{
				if (_formVars.Keys[i] != null)
					vars.Add(buildVariableToSend(_formVars.Keys[i], _formVars[i], vars.Count));
			}
			if (vars.Count > 0)
			{
				string buffer = string.Join(variableSeparator(), vars.ToArray());
				WriteToStream(buffer, reqStream);
			}
		}
		void WriteToStream(string data, Stream stream, Encoding encoding=null)
		{
			Encoding streamEncoding = encoding != null ? encoding : new UTF8Encoding(false, true);
			using (StreamWriter sw = new StreamWriter(stream, streamEncoding, StreamWriterDefaultBufferSize, true))
			{
				sw.Write(data);
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
		string buildVariableToSend(string key, string value, int idx)
		{
			bool needsCLRF = idx > 0;
			if (IsMultipart)
			{
				if (needsCLRF)
					return MultiPart.FormdataSeparator + string.Format(MultiPart.FormdataTemplate, key, value);
				else
					return string.Format(MultiPart.FormdataTemplate, key, value);
			}
			else
			{
				return GXUtil.UrlEncode(key) + "=" + GXUtil.UrlEncode(value);
			}
		}
		public void AddString(string s)
		{
			WriteToStream(s, SendStream, _contentEncoding);
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
		void SetContentHeaders(HttpRequestMessage request, string contentType)
		{
			if (request.Content != null)
			{
				if (contentType != null)
				{
					HttpContentHeaders contentHeaders = request.Content.Headers;
					contentHeaders.ContentType = MediaTypeHeaderValue.Parse(contentType);
				}
#if NETCORE
				for (int i = 0; i < _headers.Count; i++)
				{
					string currHeader = _headers.Keys[i];
					string upperHeader = currHeader.ToUpper();

					if (upperHeader == "CONTENT-DISPOSITION")
					{
						GXLogging.Debug(log, "Adding Content-Disposition header: " + _headers[i]);
						request.Content.Headers.Add("Content-Disposition", _headers[i]);
					}
					else if (upperHeader == "CONTENT-RANGE")
					{
						GXLogging.Debug(log, "Adding Content-Range header: " + _headers[i]);
						request.Content.Headers.Add("Content-Range", _headers[i]);
					}
				}
#endif
			}
			InferContentType(contentType, request);
		}
		internal void SetHeaders(HttpRequestMessage request, CookieContainer cookies, out string contentType)
		{
			HttpRequestHeaders headers = request.Headers;
			contentType = null;
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
						break;
					case "ACCEPT":
						AddHeader(headers, "Accept", _headers[i]);
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
						AddHeader(headers, "User-Agent", _headers[i]);
						break;
					case "DATE":
						DateTime value;
						if (DateTime.TryParseExact(_headers[i], "ddd, dd MMM yyyy HH:mm:ss Z", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
						{
							headers.Date = value;
						}
						else
						{
							AddHeader(headers, currHeader, _headers[i]);
						}
						break;
					case "COOKIE":
						string allCookies = _headers[i];
						foreach (string cookie in allCookies.Split(';'))
						{
							if (cookie.Contains("="))
							{
								UriBuilder uriBuilder = new UriBuilder(request.RequestUri.Scheme, request.RequestUri.Host);
								Cookie pCookie = ParseCookie(cookie, request.RequestUri.Host);
								if (pCookie != null)
								{
									cookies.Add(uriBuilder.Uri, pCookie);
								}
							}
						}
						break;
					case "IF-MODIFIED-SINCE":
						DateTime dt;
						if (DateTime.TryParse(_headers[i], DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.AdjustToUniversal, out dt))
							request.Headers.IfModifiedSince = dt;
						break;
					default:
						AddHeader(headers, currHeader, _headers[i]);
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
		}
		Cookie ParseCookie(string cookie, string domain)
		{
			string[] values = cookie.TrimEnd(';').Split('=');
			if (values.Length >= 2) {
				string cookieName = values[0].Trim();
				string cookieValue = values[1];
				return new Cookie(cookieName, cookieValue) { Domain = domain };
			}
			return null;
		}
		void AddHeader(HttpRequestHeaders headers, string headerName, string headerValue)
		{
			headers.TryAddWithoutValidation(headerName, headerValue);
		}
		void InferContentType(string contentType, HttpRequestMessage req)
		{
			if (string.IsNullOrEmpty(contentType) && _formVars.Count > 0)
			{
				req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
			}
		}

		void SetHttpVersion(HttpRequestMessage req)
		{
			string httpVersion;
			if (Config.GetValueOf("HttpClientHttpVersion", out httpVersion))
			{
				if (httpVersion == "1.0")
					req.Version = HttpVersion.Version10;
#if NETCORE
				else if (httpVersion == "2.0")
					req.Version = HttpVersion.Version20;
				else if (httpVersion == "3.0")
					req.Version = HttpVersion.Version30;
#endif
				else
					req.Version = HttpVersion.Version11;

				GXLogging.Debug(log, "Setting HTTPClient request version to ", req.Version.ToString());
			}
			else
				req.Version = HttpVersion.Version11;
		}
		[SecuritySafeCritical]
		HttpResponseMessage ExecuteRequest(string method, string requestUrl, bool contextCookies)
		{
			CookieContainer cookies = contextCookies ? _context.GetCookieContainer(requestUrl, IncludeCookies) : new CookieContainer();

			GXLogging.Debug(log, string.Format("Start HTTPClient buildRequest: requestUrl:{0} method:{1}", requestUrl, method));
			HttpRequestMessage request;
			HttpClient client = null;
			int BytesRead;
			byte[] Buffer = new byte[1024];
			HttpResponseMessage response;

			request = new HttpRequestMessage()
			{
				RequestUri = new Uri(requestUrl),
				Method = new HttpMethod(method),
			};
			SetHeaders(request, cookies, out string contentType);
			SetHttpVersion(request);
			bool disposableInstance = true;
			try
			{
#if NETCORE
				request.PopulateCookies(cookies);
				client = GetHttpClientInstance(request.RequestUri, _timeout, _authCollection, _authProxyCollection, _certificateCollection, _fileCertificateCollection, _proxyHost, _proxyPort, out disposableInstance);
#else
				client = GetHttpClientInstance(request.RequestUri, _timeout, _authCollection, _authProxyCollection, _certificateCollection, _proxyHost, _proxyPort, cookies);
#endif
				using (MemoryStream reqStream = new MemoryStream())
				{
					SendVariables(reqStream);
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
					if (reqStream.Length > 0)
					{
						request.Content = new ByteArrayContent(reqStream.ToArray());
						SetContentHeaders(request, contentType);
					}
					else
						GXLogging.Debug(log, "No content to send, skipping request.Content assignment.");
#if NETCORE
					response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
					response.ExtractCookies(cookies);
#else
					response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
#endif
				}
			}
			finally
			{
				if (disposableInstance && client != null)
				{
					client.Dispose();
				}
			}
			return response;
		}
#if NETCORE
		async Task<HttpResponseMessage> ExecuteRequestAsync(string method, string requestUrl, bool contextCookies)
		{
			CookieContainer cookies = contextCookies ? _context.GetCookieContainer(requestUrl, IncludeCookies) : new CookieContainer();

			GXLogging.Debug(log, string.Format("Start HTTPClient buildRequest: requestUrl:{0} method:{1}", requestUrl, method));
			HttpRequestMessage request;
			HttpClient client = null;
			int BytesRead;
			byte[] Buffer = new byte[1024];
			HttpResponseMessage response;

			request = new HttpRequestMessage()
			{
				RequestUri = new Uri(requestUrl),
				Method = new HttpMethod(method),
			};
			SetHeaders(request, cookies, out string contentType);
			SetHttpVersion(request);
			bool disposableInstance = true;
			try
			{
				request.PopulateCookies(cookies);
				client = GetHttpClientInstance(request.RequestUri, _timeout, _authCollection, _authProxyCollection, _certificateCollection, _fileCertificateCollection, _proxyHost, _proxyPort, out disposableInstance);
				using (MemoryStream reqStream = new MemoryStream())
				{
					SendVariables(reqStream);
					SendStream.Seek(0, SeekOrigin.Begin);
					BytesRead = await SendStream.ReadAsync(Buffer, 0, 1024);
					GXLogging.Debug(log, "Start SendStream.Read: BytesRead " + BytesRead);
					while (BytesRead > 0)
					{
						GXLogging.Debug(log, "reqStream.Write: Buffer.length " + Buffer.Length + ",'" + Encoding.UTF8.GetString(Buffer, 0, Buffer.Length) + "'");
						await reqStream.WriteAsync(Buffer, 0, BytesRead);
						BytesRead = await SendStream.ReadAsync(Buffer, 0, 1024);
					}
					EndMultipartBoundary(reqStream);
					GXLogging.Debug(log, "End SendStream.Read: stream " + reqStream.ToString());
					reqStream.Seek(0, SeekOrigin.Begin);

					if (reqStream.Length > 0)
					{
						request.Content = new ByteArrayContent(reqStream.ToArray());
						SetContentHeaders(request, contentType);
					}
					else
						GXLogging.Debug(log, "No content to send, skipping request.Content assignment.");

					response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
					response.ExtractCookies(cookies);
				}
			}
			finally
			{
				if (disposableInstance && client != null)
				{
					client.Dispose();
				}
			}
			return response;
		}
#endif

		void ReadResponseData()
		{
			if (_receiveData == null && _response!=null)
			{
				_receiveData = Array.Empty<byte>();
				try
				{
#if NETCORE
					Stream stream = _response.Content.ReadAsStream();
#else
					Stream stream = _response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
#endif

					using (MemoryStream ms = new MemoryStream())
					{
						stream.CopyTo(ms);
						_receiveData = ms.ToArray();
					}
					_eof = true;
					int bytesRead = _receiveData.Length;
					GXLogging.Debug(log, "BytesRead " + _receiveData.Length);
					if (bytesRead > 0 && !_encodingFound)
					{
						_encoding = DetectEncoding(_charset, out _encodingFound, _receiveData, bytesRead);
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
		}
#if NETCORE
		async Task ReadResponseDataAsync()
		{
			if (_receiveData == null && _response != null)
			{
				_receiveData = Array.Empty<byte>();
				try
				{
					Stream stream = await _response.Content.ReadAsStreamAsync();

					using (MemoryStream ms = new MemoryStream())
					{
						await stream.CopyToAsync(ms);
						_receiveData = ms.ToArray();
					}
					_eof = true;
					int bytesRead = _receiveData.Length;
					GXLogging.Debug(log, "BytesRead " + _receiveData.Length);
					if (bytesRead > 0 && !_encodingFound)
					{
						_encoding = DetectEncoding(_charset, out _encodingFound, _receiveData, bytesRead);
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
		}
		#endif
		bool UseOldHttpClient(string name)
		{
			if (Config.GetValueOf("useoldhttpclient", out string useOld) && useOld.StartsWith("y", StringComparison.OrdinalIgnoreCase))
				return true;

#if !NETCORE
			try
			{
				string requestUrl = GetRequestURL(name);
				Uri uri = new Uri(requestUrl);
				if (!uri.IsDefaultPort)
					return true;
			}
			catch (Exception e)
			{
				GXLogging.Warn(log, "UseOldHttpClient", e);
			}
#endif
			return false;
		}

		public void Execute(string method, string name)
		{
			if (UseOldHttpClient(name))
			{
				GXLogging.Debug(log, "Using legacy GxHttpClient");
				WebExecute(method, name);
			}
			else
			{
				HttpClientExecute(method, name);
			}
		}
#if NETCORE
		public async Task ExecuteAsync(string method, string name)
		{
			if (UseOldHttpClient(name))
			{
				GXLogging.Debug(log, "Using legacy GxHttpClient");
				await WebExecuteAsync(method, name);
			}
			else
			{
				await HttpClientExecuteAsync(method, name);
			}
		}
#endif
		internal void ProcessResponse(HttpResponseMessage httpResponse)
		{
			_response = httpResponse;
			LoadResponseHeaders(_response);
			_statusCode = ((short)_response.StatusCode);
			_statusDescription = GetStatusCodeDescrption(_response);
			if ((_statusCode >= 400 && _statusCode < 600) && _errCode != 1)
			{
				_errCode = 1;
				_errDescription = "The remote server returned an error: (" + _statusCode + ") " + _statusDescription + ".";
			}
		}
		private void ProcessHttpClientException(Exception ex)
		{
			HttpRequestException httpex;
			TaskCanceledException tcex;
#if NETCORE
			AggregateException aex;
			if ((aex = ex as AggregateException) != null)
			{
				GXLogging.Warn(log, "Error Execute", aex);
				_errCode = 1;
				if (aex.InnerException != null)
					_errDescription = aex.InnerException.Message;
				else
					_errDescription = aex.Message;
				_response = new HttpResponseMessage();
				_response.Content = new StringContent(HttpHelper.StatusCodeToTitle(HttpStatusCode.InternalServerError));
				_response.StatusCode = HttpStatusCode.InternalServerError;
			}
			else
#endif
			if ((httpex = ex as HttpRequestException) != null)
			{
				GXLogging.Warn(log, "Error Execute", httpex);
				_errCode = 1;
				if (httpex.InnerException != null)
					_errDescription = httpex.Message + " " + httpex.InnerException.Message;
				else
					_errDescription = httpex.Message;
				_response = new HttpResponseMessage();
				_response.Content = new StringContent(HttpHelper.StatusCodeToTitle(HttpStatusCode.InternalServerError));
#if NETCORE
				_response.StatusCode = (HttpStatusCode)(httpex.StatusCode != null ? httpex.StatusCode : HttpStatusCode.InternalServerError);
#else
				_response.StatusCode = HttpStatusCode.InternalServerError;
#endif

			}
			else if ((tcex = ex as TaskCanceledException) != null)
			{
				GXLogging.Warn(log, "Error Execute", tcex);
				_errCode = 1;
				_errDescription = "The request has timed out. " + tcex.Message;
				_response = new HttpResponseMessage();
				_response.StatusCode = 0;
				_response.Content = new StringContent(String.Empty);
			}
			else
			{
				GXLogging.Warn(log, "Error Execute", ex);
				_errCode = 1;
				if (ex.InnerException != null)
					_errDescription = ex.Message + " " + ex.InnerException.Message;
				else
					_errDescription = ex.Message;
				_response = new HttpResponseMessage();
				_response.Content = new StringContent(HttpHelper.StatusCodeToTitle(HttpStatusCode.InternalServerError));
				_response.StatusCode = HttpStatusCode.InternalServerError;
			}
		}
#if NETCORE
		internal async Task HttpClientExecuteAsync(string method, string name)
		{
			_receiveData = null;
			_response = null;
			_errCode = 0;
			_errDescription = string.Empty;
			GXLogging.Debug(log, "Start Execute: method '" + method + "', name '" + name + "'");
			try
			{
				string requestUrl = GetRequestURL(name);
				bool contextCookies = _context != null && !String.IsNullOrEmpty(requestUrl);
				_response = await ExecuteRequestAsync(method, requestUrl, contextCookies);

				if (contextCookies)
					_context.UpdateSessionCookieContainer();

			}
			catch (Exception ex) {
				ProcessHttpClientException(ex);
			}

			GXLogging.Debug(log, "Reading response...");
			if (_response == null)
				return;
			ProcessResponse(_response);
			ClearSendStream();
		}
#endif
		public void HttpClientExecute(string method, string name)
		{
			_receiveData = null;
			_response = null;
			_errCode = 0;
			_errDescription = string.Empty;
			GXLogging.Debug(log, "Start Execute: method '" + method + "', name '" + name + "'");
			try
			{
				string requestUrl = GetRequestURL(name);
				bool contextCookies = _context != null && !String.IsNullOrEmpty(requestUrl);
				_response = ExecuteRequest(method, requestUrl, contextCookies);

				if (contextCookies)
					_context.UpdateSessionCookieContainer();

			}
			catch (Exception ex)
			{
				ProcessHttpClientException(ex);
			}
			GXLogging.Debug(log, "Reading response...");
			if (_response == null)
				return;
			ProcessResponse(_response);
			ClearSendStream();
		}
		NameValueCollection _respHeaders;
		private bool disposedValue;
		static bool IsChunkedOrEventStream(HttpResponseMessage response)
		{
			if (response == null)
				return false;
			bool isEventStream = response.Content?.Headers.ContentType?.MediaType == "text/event-stream";
			if (isEventStream)
				return true;

			bool isChunked = response.Headers.TransferEncodingChunked.HasValue && response.Headers.TransferEncodingChunked.Value;
			return isChunked;
		}
		internal void LoadResponseHeaders(HttpResponseMessage resp)
		{
			_respHeaders = new NameValueCollection();
			HttpResponseHeaders headers = resp.Headers;
			foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
			{
				_respHeaders.Add(header.Key, String.Join(",", header.Value));
			}
			HttpContentHeaders contentHeaders = resp.Content.Headers;
			foreach (KeyValuePair<string, IEnumerable<string>> header in contentHeaders)
			{
				_respHeaders.Add(header.Key, String.Join(",", header.Value));
			}
			_isChunkedResponse = IsChunkedOrEventStream(resp) ;

			if (_response.Content.Headers.ContentType == null)
				_charset = null;
			else
				_charset = _response.Content.Headers.ContentType.CharSet;
			_encodingFound = false;
			if (!string.IsNullOrEmpty(_charset))
			{
				int idx = _charset.IndexOf("charset=");
				if (idx > 0)
				{
					idx += 8;
					_charset = _charset.Substring(idx, _charset.Length - idx);
					_encoding = GetEncoding(_charset);
					if (_encoding != null)
						_encodingFound = true;
				}
				else
				{
					_charset = String.Empty;
				}
			}
		}

		private string GetStatusCodeDescrption(HttpResponseMessage message)
		{
#if NETCORE
			string statusCodeDescription = ReasonPhrases.GetReasonPhrase((int)message.StatusCode);
#else
			string statusCodeDescription = HttpWorkerRequest.GetStatusDescription((int)message.StatusCode);
#endif
			string reasonPhrase = message.ReasonPhrase;

			if (string.IsNullOrEmpty(reasonPhrase))
			{
				return statusCodeDescription;
			}
			else
			{
				return reasonPhrase;
			}
		}

		private void SetHeaders(HttpWebRequest req)
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

		static WebProxy getProxy(string proxyHost, int proxyPort, ArrayList authenticationCollection)
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

		private void SetHttpVersion(HttpWebRequest req)
		{

			string httpVersion;
			if (Config.GetValueOf("HttpClientHttpVersion", out httpVersion))
			{
				if (httpVersion == "1.0")
					req.ProtocolVersion = HttpVersion.Version10;
#if NETCORE
				else if (httpVersion == "2.0")
					req.ProtocolVersion = HttpVersion.Version20;
				else if (httpVersion == "3.0")
					req.ProtocolVersion = HttpVersion.Version30;
#endif
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
				if (IsAbsolute(name))
				{
					return name;
				}
				else
				{
					if (!string.IsNullOrEmpty(name) && name.IndexOf('/') == 0)
						return _url + name;
					else
						return _url + "/" + name;
				}
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
#if NETCORE
		async Task<HttpWebRequest> buildRequestAsync(string method, string requestUrl, CookieContainer cookies)
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
			SetHttpVersion(req);
			WebProxy proxy = getProxy(_proxyHost, _proxyPort, _authProxyCollection);
			if (proxy != null)
				req.Proxy = proxy;

			SetHeaders(req);

			if (!method.Equals(HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase) && !method.Equals(HttpMethod.Head.Method, StringComparison.OrdinalIgnoreCase))
			{
#if !NETCORE
                                using (Stream reqStream = req.GetRequestStream())
#else
				using (Stream reqStream = await req.GetRequestStreamAsync())
#endif
				{
					SendVariables(reqStream);
					SendStream.Seek(0, SeekOrigin.Begin);
					BytesRead = await SendStream.ReadAsync(Buffer, 0, 1024);
					GXLogging.Debug(log, "Start SendStream.Read: BytesRead " + BytesRead);
					while (BytesRead > 0)
					{
						GXLogging.Debug(log, "reqStream.Write: Buffer.length " + Buffer.Length + ",'" + Encoding.UTF8.GetString(Buffer, 0, Buffer.Length) + "'");
						await reqStream.WriteAsync(Buffer, 0, BytesRead);
						BytesRead = await SendStream.ReadAsync(Buffer, 0, 1024);
					}
					EndMultipartBoundary(reqStream);
				}
			}
			return req;
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
			SetHttpVersion(req);
			WebProxy proxy = getProxy(_proxyHost, _proxyPort, _authProxyCollection);
			if (proxy != null)
				req.Proxy = proxy;

			SetHeaders(req);

			if (!method.Equals(HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase) && !method.Equals(HttpMethod.Head.Method, StringComparison.OrdinalIgnoreCase))
			{
				using (Stream reqStream = req.GetRequestStream())
				{
					SendVariables(reqStream);
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
		static ICredentials getCredentialCache(Uri URI, ArrayList authenticationCollection)
		{
			string sScheme;
			GxAuthScheme auth;
			CredentialCache cc = null;

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
						if (cc == null)
						{
							cc = new CredentialCache();
						}
						cc.Add(URI, sScheme, new NetworkCredential(auth.User, auth.Password, auth.Realm));
					}
					else
					{
						if (cc == null)
						{
							cc = new CredentialCache();
						}
						cc.Add(URI, sScheme, new NetworkCredential(auth.User, auth.Password));
					}
				}
				catch (ArgumentException)
				{
				}
			}
			return cc;
		}
#if NETCORE
		private async Task WebExecuteAsync(string method, string name)
		{
			HttpWebRequest req;
			HttpWebResponse resp;

			_errCode = 0;
			_errDescription = string.Empty;

			GXLogging.Debug(log, "Start Execute: method '" + method + "', name '" + name + "'");
			try
			{
				string requestUrl = GetRequestURL(name);
				bool contextCookies = _context != null && !String.IsNullOrEmpty(requestUrl);
				CookieContainer cookies = contextCookies ? _context.GetCookieContainer(requestUrl, IncludeCookies) : new CookieContainer();
				req = await buildRequestAsync(method, requestUrl, cookies);

				resp = await req.GetResponseAsync() as HttpWebResponse;
				if (contextCookies)
					_context.UpdateSessionCookieContainer();
			}
			catch (Exception e)
			{
				resp = ProcessWebExecuteException(e);
			}


			_receiveData = Array.Empty<byte>();
			if (resp != null)
			{
				GXLogging.Debug(log, "Reading response...");
				loadResponseHeaders(resp);

				String charset = resp.ContentType;
				using (Stream rStream = resp.GetResponseStream())
				{
					try
					{
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
						using (MemoryStream ms = new MemoryStream())
						{
							await rStream.CopyToAsync(ms);
							_receiveData = ms.ToArray();
						}
						int bytesRead = _receiveData.Length;
						GXLogging.Debug(log, "BytesRead " + bytesRead);

						if (bytesRead > 0 && !encodingFound)
						{
							_encoding = DetectEncoding(charset, out encodingFound, _receiveData, bytesRead);
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
				_statusCode = (short)resp.StatusCode;
				_statusDescription = resp.StatusDescription;
				resp.Close();

				GXLogging.DebugSanitized(log, "_responseString " + ToString());
			}
			ClearSendStream();
		}
#endif
		private HttpWebResponse ProcessWebExecuteException(Exception ex)
		{
			WebException we;
			HttpWebResponse resp=null;
#if NETCORE
			AggregateException agge;
			if ((agge = ex as AggregateException) != null)
			{
				GXLogging.Warn(log, "Error Execute", agge);
				_errCode = 1;
				_errDescription = agge.Message;

				var baseEx = agge.GetBaseException() as WebException;
				if (baseEx != null)
				{
					resp = baseEx.Response as HttpWebResponse;
					_errDescription = baseEx.Message;
				}
			}
			else
#endif
			if ((we = ex as WebException)!=null)
			{
				GXLogging.Warn(log, "Error Execute", we);
				_errCode = 1;
				_errDescription = we.Message;
				resp = (HttpWebResponse)(we.Response);
			}
			else 
			{
				GXLogging.Warn(log, "Error Execute", ex);
				_errCode = 1;
				_errDescription = ex.Message;
			}
			return resp;

		}
		private void WebExecute(string method, string name)
		{
			HttpWebRequest req;
			HttpWebResponse resp = null;

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
			catch (Exception e)
			{
				resp = ProcessWebExecuteException(e);
			}


			_receiveData = Array.Empty<byte>();
			if (resp != null)
			{
				GXLogging.Debug(log, "Reading response...");
				loadResponseHeaders(resp);

				String charset = resp.ContentType;
				using (Stream rStream = resp.GetResponseStream())
				{
					try
					{
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
						using (MemoryStream ms = new MemoryStream())
						{
							rStream.CopyTo(ms);
							_receiveData = ms.ToArray();
						}
						int bytesRead = _receiveData.Length;
						GXLogging.Debug(log, "BytesRead " + bytesRead);

						if (bytesRead > 0 && !encodingFound)
						{
							_encoding = DetectEncoding(charset, out encodingFound, _receiveData, bytesRead);
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
				_statusCode = (short)resp.StatusCode;
				_statusDescription = resp.StatusDescription;
				resp.Close();

				GXLogging.DebugSanitized(log, "_responseString " + ToString());
			}
			ClearSendStream();
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
			if (m != null && m.Success)
			{
				string parsedEncoding = m.Value;
				parsedEncoding = parsedEncoding.Substring(startAt, parsedEncoding.Length - (startAt + 1));
				enc = GetEncoding(parsedEncoding);
			}
			return enc;
		}
		public bool Eof
		{
			get
			{
				return _eof;

			}
		}
		StreamReader _receivedChunkedStream;
		public string ReadChunk()
		{
			if (!_isChunkedResponse)
				return ToString();

			if (_response == null)
				return string.Empty;
			try
			{
				if (_receivedChunkedStream == null)
				{
#if NETCORE
					_receivedChunkedStream = new StreamReader(_response.Content.ReadAsStream());
#else
					_receivedChunkedStream = new StreamReader(_response.Content.ReadAsStreamAsync().GetAwaiter().GetResult());
#endif
				}
				_eof = _receivedChunkedStream.EndOfStream;
				if (!_eof)
				{
					string line = _receivedChunkedStream.ReadLine();
					if (line != null)
					{
						return line;
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, String.Format("Error reading chunk", ex));
			}
			return string.Empty;

		}
		public override string ToString()
		{
			if (ReceiveData == null)
				return string.Empty;
			if (_encoding == null)
				_encoding = Encoding.UTF8;
			string responseString = _encoding.GetString(ReceiveData);
			GXLogging.DebugSanitized(log, "_responseString " + responseString);
			return responseString;
		}
#if NETCORE
		public async Task<string> ToStringAsync()
		{
			byte[] bytes = await ReceiveDataAsync();
			if (bytes == null)
				return string.Empty;
			if (_encoding == null)
				_encoding = Encoding.UTF8;
			string responseString = _encoding.GetString(bytes);
			GXLogging.DebugSanitized(log, "_responseString " + responseString);
			return responseString;
		}
#endif

		public void ToFile(string fileName)
		{
			string pathName = fileName;

#if !NETCORE
			if (HttpContext.Current != null)
#endif
			if (fileName.IndexOfAny(new char[] { '\\', ':' }) == -1)
				pathName = Path.Combine(GxContext.StaticPhysicalPath(), fileName);

			if (ReceiveData != null)
			{
				File.WriteAllBytes(pathName, ReceiveData);
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
		public void GetHeader(string name, out Decimal value)
		{
			value = Convert.ToDecimal(GetHeader(name));
		}
		public void GetHeader(string name, out string value)
		{
			value = GetHeader(name);
		}
		public void GetHeader(string name, out DateTime value)
		{
			value = Convert.ToDateTime(GetHeader(name));
		}
		public void GetHeader(string name, out bool value)
		{
			value = Convert.ToBoolean(GetHeader(name));
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
			_fileCertificateCollection.Add(file);
			_certificateCollection.Add(c);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_receiveData = null;
					_sendStream?.Dispose();
					_receivedChunkedStream?.Dispose();
				}
				disposedValue = true;
			}
		}
		~GxHttpClient()
		{
		     Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
