namespace GeneXus.Http.Server
{
	using System;
	using System.Web;
	using System.IO;
    using GeneXus.Application;
	using GeneXus.Configuration;
	using GeneXus.Utils;
#if NETCORE
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Http.Extensions;
	using System.Linq;
	using Microsoft.AspNetCore.Http.Features;
#endif

	public class GxHttpCookie
	{
		String _Name = "";
		String _Value = "";
		String _Path = "";
		DateTime _ExpirationDate = DateTimeUtil.NullDate();
		String _Domain = "";
		bool _Secure;
		bool _HttpOnly;
		internal static string GX_SESSION_ID = "GX_SESSION_ID";
		public GxHttpCookie()
		{
			_HttpOnly = HttpOnlyDefault();
		}

		public static Boolean HttpOnlyDefault()
		{
			string httpOnlyDefaultS = string.Empty;
			Boolean HttpOnly = true;
			if (Config.GetValueOf("cookie_httponly_default", out httpOnlyDefaultS))
			{
				if (String.Compare(httpOnlyDefaultS, "false", true) == 0)
					HttpOnly = false;
			}
			return HttpOnly;		
		}

		public String Name
		{
			set { _Name = value; }
			get { return _Name; }
		}

		public String CurrentValue
		{
			set { _Value = value; }
			get { return _Value; }
		}

		public String Path
		{
			set { _Path = value; }
			get { return _Path; }
		}

		public DateTime ExpirationDate
		{
			set { _ExpirationDate = value; }
			get { return _ExpirationDate; }
		}

		public String Domain
		{
			set { _Domain = value; }
			get { return _Domain; }
		}

		public bool Secure
		{
			set { _Secure = value; }
			get { return _Secure; }
		}

		public bool HttpOnly
		{
			set { _HttpOnly = value; }
			get { return _HttpOnly; }
		}
	}

	public class GxHttpResponse
	{
        HttpResponse _httpRes;
        IGxContext _context;

		public GxHttpResponse(IGxContext context)
		{
            _context = context;
		}
        [Obsolete("GxHttpResponse constructor with HttpResponse is deprecated", false)]
        public GxHttpResponse(HttpResponse httpR)
        {
            _httpRes = httpR;
        }

		public HttpResponse Response
		{
            get
            {
                if (_context!=null && _context.HttpContext != null)
                {
                    return _context.HttpContext.Response;
                }
                else 
                {
                    return _httpRes;
                }
            }
		}
		public short ErrCode
		{
			get {return 0;}
		}
		public string ErrDescription
		{
			get {return "";}
		}
		public short SetCookie(GxHttpCookie cookie)
		{
			return _context.SetCookie( cookie.Name, cookie.CurrentValue, cookie.Path, cookie.ExpirationDate, cookie.Domain, cookie.Secure ? 1:0, cookie.HttpOnly);
		}
		public void AddString( string s)
		{
			if (Response != null)
			{
				Response.Write(s);
			}			
		}

		public void AddFile( string fileName)
		{
			if (!string.IsNullOrEmpty(fileName))
			{
				Response.WriteFile(fileName.Trim());
			}
		}
		public void AppendHeader( string name, string value)
		{
			if(string.Compare(name, "Content-Disposition", true) == 0)
			{
				value = GetEncodedContentDisposition(value);
			}
            if (_context!=null) 
                _context.SetHeader(name, value);
		}
		private string GetEncodedContentDisposition(string value)
		{
			int filenameIdx = value.ToLower().IndexOf("filename");
			if(filenameIdx != -1)
			{
				int eqIdx = value.ToLower().IndexOf("=", filenameIdx);
				if (eqIdx != -1)
				{
					string filename = value.Substring(eqIdx + 1).Trim();
					try
					{
						value = value.Substring(0, eqIdx + 1) + Uri.EscapeDataString(filename);
					}
					catch(UriFormatException) //Contains High Surrogate Chars
					{
						value = value.Substring(0, eqIdx + 1) + GXUtil.UrlEncode(filename);
					}
				}
			}
			return value;
		}
	}

	public class GxSoapRequest : GxHttpRequest
	{
		IGxContext _context;
		public GxSoapRequest(IGxContext context) : base(context)
		{
			this._context = context;
		}
		public override string ToString()
		{
			if (this._context.SoapContext != null)
			{
				return this._context.SoapContext.ToString();
			}
			return base.ToString();
		}

		public GXSOAPContext SoapContext
		{
			get { return this._context.SoapContext; }
			set { this._context.SoapContext = value; }
		}
	}
	public class GxHttpRequest
	{
		HttpRequest _httpReq;
        IGxContext _context;
        string _referrer = "";

		public GxHttpRequest(IGxContext context)
        {
            _context = context;
			if (context.HttpContext != null)
			{
				_httpReq = context.HttpContext.Request;
			}
        }
        
        public GxHttpRequest(HttpRequest httpR)
		{
			_httpReq = httpR;
		}
		public HttpRequest Request
		{
			get	{ return _httpReq; }
		}
		public string Method
		{
			get
			{
				if (_httpReq == null)
					return "";
				return _httpReq.GetMethod();
			}
		}
		public short ErrCode
		{
			get {return 0;}
		}
		public string ErrDescription
		{
			get {return "";}
		}
		public string BaseURL
		{
			get 
			{
				if (_httpReq == null)
					return "";
#if NETCORE
				string baseURL = _httpReq.GetDisplayUrl();
#else
				Uri u = _httpReq.Url;
				string baseURL = u.GetLeftPart(UriPartial.Path);
#endif
				baseURL = baseURL.Substring(0, baseURL.LastIndexOf("/")) + "/";
				return baseURL; 
			}
		}
		public string QueryString
		{
			get	
			{
				if (_httpReq == null)
					return "";
				string urlQuery;
#if NETCORE
				urlQuery = _httpReq.QueryString.HasValue ? _httpReq.QueryString.Value : string.Empty;
#else
				urlQuery = _httpReq.Url.Query;
#endif
				if (urlQuery.Length == 0)
					return "";
				string url = (_context != null && _context.isAjaxRequest()) ? 
					((GxContext)_context).RemoveInternalParms(urlQuery) :
					urlQuery.Substring(1, urlQuery.Length - 1);
				return url.Replace("?","");
			}
		}
		public string ServerHost
		{
			get
			{
				if (_httpReq == null)
					return "";
				return _context.GetServerName();
			}
		}
		public short ServerPort
		{
			get
			{
				if (_httpReq == null)
					return 0;
				return (short) _context.GetServerPort();
			}
		}
		public short Secure
		{
			get
			{
				if (_httpReq == null)
					return 0;
				return _context.GetHttpSecure();
			}
		}
		public string ScriptPath
		{
			get
			{
				if (_httpReq == null)
					return "";
				return _httpReq.GetApplicationPath()+"/";
			}
		}
		public string ScriptName
		{
			get
			{
				if (_httpReq == null)
					return "";
				string virtualPath = _httpReq.GetFilePath();
				return virtualPath.Remove(0, virtualPath.LastIndexOf('/')+1) ;
			}
		}
		public string Referrer
		{
            set
            {
                _referrer = value;
            }
			get 
			{ 
				try
				{
                    string referer = _context.GetReferer();
                    return string.IsNullOrEmpty(referer) ? _referrer : referer;
				}
				catch 
				{ 
					return "";
				}
			}
		}
		public string RemoteAddress
		{
			get 
			{
				try
				{
#if !NETCORE
                    if (_httpReq != null)
						return _httpReq.UserHostAddress;
					else
						return String.Empty;
#else
					return _context.HttpContext.GetUserHostAddress();
#endif
				}
				catch 
				{
                    return String.Empty;
				}
			}
		}
		public string[] GetVariables()
		{
			if (_httpReq == null)
				return Array.Empty<string>();
#if NETCORE
			return _httpReq.Form.Keys.ToArray();
#else
			return _httpReq.Form.AllKeys;
#endif
		}
		public string GetHeader( string name)
		{
			if (_httpReq == null)
				return "";
			string hdr = _httpReq.Headers[name];
			if (hdr == null)
				return "";
			return hdr;
		}
		public string GetValue( string name)
		{
			if (_httpReq == null)
				return "";
			try
			{
				string s = _httpReq.Form[name];
				return string.IsNullOrEmpty(s) ? string.Empty : s;
			}
			catch (InvalidOperationException)
			{
				return string.Empty;
			}
		}
		public override string ToString()
		{
			if (_httpReq == null)
				return "";
#if NETCORE
			using StreamReader reader = new(_httpReq.Body);
			return reader.ReadToEnd();
#else
			using StreamReader reader = new(_httpReq.InputStream);
			return reader.ReadToEnd();
#endif
		}
		public void ToFile(string FileName)
		{
#if !NETCORE
			if (_httpReq != null)
			{
				_httpReq.SaveAs(FileName, false);
			}
#else
			using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write))
			{
				_httpReq.Body.CopyTo(fs);
			}
#endif
		}
	}
}
