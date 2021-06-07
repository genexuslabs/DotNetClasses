namespace GeneXus.Application
{
	using System;
	using System.Collections;
	using System.IO;
	using System.Reflection;
	using GeneXus.Data;
	using GeneXus.XML;
	using GeneXus.Utils;
#if !NETCORE
	using System.Web.UI;
	using GeneXus.WebControls;
	using System.Messaging;
	using System.ServiceModel.Web;
	using GeneXus.UserControls;
#else
	using Microsoft.AspNetCore.Http;
	using GxClasses.Helpers;
	using Experimental.System.Messaging;
#endif
	using GeneXus.Configuration;
	using GeneXus.Metadata;
	using log4net;
	using Jayrock.Json;
	using GeneXus.Http;
	using System.Collections.Specialized;
	using System.Collections.Generic;
	using System.Text;
	using GeneXus.Data.NTier;
	using GeneXus.Resources;
	using System.Net;
	using TZ4Net;
	using System.Globalization;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using System.Web;
	using GeneXus.Http.Server;
	using GeneXus.Mime;
	using GeneXus.Printer;
	using System.Drawing;
	using System.Collections.Concurrent;
#if NETCORE
	using Microsoft.AspNetCore.Http.Features;
#endif
	using System.Threading;
	using System.Security.Claims;

	public interface IGxContext
	{
		void disableOutput();
		void enableOutput();
		bool isOutputEnabled();
		void setPortletMode();
		void setAjaxCallMode();
		void setAjaxEventMode();
		void setFullAjaxMode();
		void AddDeferredFrags();
		bool isPortletMode();
		bool isAjaxCallMode();
		bool isAjaxRequest();
		bool isSpaRequest();
		bool isSpaRequest(bool ignoreFlag);
		bool isAjaxEventMode();
		bool isPopUpObject();
		bool isFullAjaxMode();
		bool isMultipartRequest();
		void DisableSpaRequest();
		String AjaxCmpContent { get; set; }
		bool isCloseCommand { get; }
		OlsonTimeZone GetOlsonTimeZone();
		String GetTimeZone();
		Boolean SetTimeZone(String sTZ);
		HttpAjaxContext httpAjaxContext { get; }
		GxHttpContextVars httpContextVars { get; set; }
		T ReadSessionKey<T>(string key) where T : class;
		bool WriteSessionKey<T>(string key, T value) where T : class;

		void DoAfterInit();
		void PushCurrentUrl();
		bool isSmartDevice();

		bool IsMultipartRequest { get; }

		void PushAjaxCmpContent(String Content);
		int CmpDrawLvl { get; set; }
		bool isEnabled { get; set; }
		HttpContext HttpContext { get; set; }
		HtmlTextWriter OutputWriter { get; set; }
		ArrayList DataStores { get; }
		string Gx_ope { get; set; }
		int Gx_dbe { get; set; }
		string Gx_dbt { get; set; }
		string Gx_etb { get; set; }
		short Gx_eop { get; set; }
		short Gx_err { get; set; }
		string Gx_dbsqlstate { get; set; }

		string ClientID { get; set; }
		GxErrorHandlerInfo ErrorHandlerInfo { get; }
		string Gxuserid { get; set; }
		string Gxpasswrd { get; set; }
		string Gxdbname { get; set; }
		string Gxdbsrv { get; set; }
		string wjLoc { get; set; }
		int wjLocDisableFrm { get; set; }
		short wbHandled { get; set; }
		short wbGlbDoneStart { get; set; }
		msglist GX_msglist { get; set; }
		short nUserReturn { get; set; }
		string sCallerURL { get; set; }
		GXXMLWriter GX_xmlwrt { get; }
		FileIO FileIOInstance { get; }
		FtpService FtpInstance { get; }
		short nLocRead { get; set; }
		GxLocationCollection colLocations { get; set; }
		int nSOAPErr { get; set; }
		string sSOAPErrMsg { get; set; }
		string CleanAbsoluteUri { get; }
		string BaseUrl { get; set; }
		string ConfigSection { get; set; }
		IReportHandler reportHandler { get; set; }
		int handle { get; set; }
		bool isRedirected { get; }
		bool ResponseCommited { get; set; }
		bool DrawingGrid { get; set; }
		bool HtmlHeaderClosed { get; }
		bool DrawGridsAtServer { get; set; }

		void AddDataStore(IGxDataStore datastore);
		IGxDataStore GetDataStore(string id);
		void CloseConnections();
		void CommitDataStores();
		void Disconnect();
		void RollbackDataStores();
		void CommitDataStores(string callerName);
		void RollbackDataStores(string callerName);
		void CommitDataStores(string callerName, IDataStoreProvider dataStore);
		void RollbackDataStores(string callerName, IDataStoreProvider dataStore);
		string getCurrentLocation();
		string GetRemoteAddress();
		string GetServerName();
		int GetServerPort();
		string GetScriptPath();
		string GetPhysicalPath();
		string GetContextPath();
		string GetBuildNumber(int buildN);
		byte DeleteFile(string fileName);
		byte FileExists(string fileName);
		string GetDynUrl();

		int GetSoapErr();
		string GetSoapErrMsg();
		bool isRemoteGXDB();
		DateTime ServerNow(string dataSource);
		DateTime ServerNowMs(string dataSource);
		string ServerVersion(string dataSource);
		string DataBaseName(string dataSource);
		void SetProperty(string key, string value);
		string GetProperty(string key);
		void SetContextProperty(string key, object value);
		object GetContextProperty(string key);
		string PathToRelativeUrl(string name);
		string PathToRelativeUrl(string name, bool relativeToServer);
		string PathToUrl(string name);
		string GetContentType(string name);
		bool ExecuteBeforeConnect(IGxDataStore datastore);
		bool ExecuteAfterConnect(String datastoreName);
		int GetButtonType();
		string GetCssProperty(string propName, string propValue);
		string GetLanguage();
		string GetLanguageProperty(String propName);
		string FileToBase64(string filePath);
		string FileFromBase64(string b64);
		byte[] FileToByteArray(string filePath);
		string FileFromByteArray(byte[] bArray);
		IGxContext UtlClone();
		string GetRequestMethod();
		string GetRequestQueryString();
		void DeleteReferer(int popupLevel);
		void DeleteReferer();
		void PopReferer();
		string GetReferer();
		int GetBrowserType();
		bool IsLocalStorageSupported();
		bool ExposeMetadata();
		string GetBrowserVersion();
		short GetHttpSecure();
		string GetCookie(string name);
		short SetCookie(string name, string value, string path, DateTime expires, string domain, int secure, bool httponly);
		short SetCookie(string name, string value, string path, DateTime expires, string domain, int secure);
		byte ResponseContentType(string sContentType);
		byte RespondFile(string name);
		byte SetHeader(string name, string value);
		string GetHeader(string name);
		bool IsForward();
		void Redirect(String jumpUrl);
		void Redirect(String jumpUrl, bool bSkipPushUrl);
		void PopUp(String url);
		void PopUp(String url, Object[] returnParms);
		void WindowClosed();
		void NewWindow(GXWindow win);
		void DispatchAjaxCommands();
		void DoAjaxRefresh();
		void DoAjaxRefreshForm();
		void DoAjaxRefreshCmp(String sPrefix);
#if !NETCORE
        void DoAjaxLoad(int SId, GXWebRow row);
#endif
		void DoAjaxAddLines(int SId, int lines);
		void DoAjaxSetFocus(string ControlName);
		string BuildHTMLColor(int lColor);
		string BuildHTMLFont(String type, int size, String color);
		GxXmlContext Xml { get; }
		GxHttpResponse GX_webresponse { get; }

		HttpCookieCollection localCookies { get; set; }

		MessageQueueTransaction MQTransaction { get; set; }

		void AddJavascriptSource(string jsSrc, string urlBuildNumber);
		void AddJavascriptSource(string jsSrc, string urlBuildNumber, bool userDefined, bool inlined);
		void AddDeferredJavascriptSource(string jsSrc, string urlBuildNumber);
		void AddStyleSheetFile(string styleSheet);
		void AddWebAppManifest();
		bool JavascriptSourceAdded(string jsSrc);
		bool StyleSheetAdded(string styleSheet);
		void StatusMessage(string message);
		void AddComponentObject(string cmpCtx, string objName);
		void PrintReportAtClient(string reportFile, string printerRule);
		void SaveComponentMsgList(string cmpCtx);
		void SendComponentObjects();
		void SendServerCommands();
		void CloseHtmlHeader();
		void WriteHtmlText(string sText);
		void WriteHtmlTextNl(string sText);
		void skipLines(long lines);
		string convertURL(string file);
		string GetCompleteURL(string file);
		string GetImagePath(string id, string KBId, string theme);
		string GetImageSrcSet(string baseImage);
		string GetTheme();
		void SetDefaultTheme(string theme);
		int SetTheme(string theme);
		bool CheckContentType(string contentKey, string contentType, string fullPath);
		string ExtensionForContentType(string contentType);
		void SetSubmitInitialConfig(IGxContext context);
		string GetMessage(string id);
		string GetMessage(string id, object[] args);
		string GetMessage(string id, string language);
		int SetLanguage(string id);
		void SetWrapped(bool wrapped);
		bool GetWrapped();
		bool isCrawlerRequest();
		void SendWebValue(string sText);
		void SendWebAttribute(string sText);
		void SendWebValueSpace(string sText);
		void SendWebValueEnter(string sText);
		void SendWebValueComplete(string sText);
		void RenderUserControl(string controlType, string internalName, string htmlId, GxDictionary propbag);
		void ajax_rsp_command_close();
		void ajax_rsp_clear();
		void setWebReturnParms(Object[] retParms);
		void setWebReturnParmsMetadata(Object[] retParms);
		String getWebReturnParmsJS();
		String getWebReturnParmsMetadataJS();
		String getJSONResponse();
		LocalUtil localUtil { get; }
		IGxSession GetSession();
		CookieContainer GetCookieContainer(string url, bool includeCookies = true);
		bool WillRedirect();
		GXSOAPContext SoapContext { get; set; }
#if NETCORE
		void UpdateSessionCookieContainer();
#endif
		string GetCacheInvalidationToken();
		string GetURLBuildNumber(string resourcePath, string urlBuildNumber);
	}
#if NETCORE
	public class GxHttpContextAccesor : HttpContext
	{
		IHttpContextAccessor ctxAccessor;
		public GxHttpContextAccesor(IHttpContextAccessor ctxAccessor)
		{
			this.ctxAccessor = ctxAccessor;
		}
		public override ConnectionInfo Connection => ctxAccessor.HttpContext.Connection;

		public override IFeatureCollection Features => ctxAccessor.HttpContext.Features;

		public override IDictionary<object, object> Items { get => ctxAccessor.HttpContext.Items; set => ctxAccessor.HttpContext.Items=value; }

		public override HttpRequest Request => ctxAccessor.HttpContext.Request;

		public override CancellationToken RequestAborted { get => ctxAccessor.HttpContext.RequestAborted; set => ctxAccessor.HttpContext.RequestAborted=value; }
		public override IServiceProvider RequestServices { get => ctxAccessor.HttpContext.RequestServices; set => ctxAccessor.HttpContext.RequestServices=value; }

		public override HttpResponse Response => ctxAccessor.HttpContext.Response;

		public override ISession Session { get => ctxAccessor.HttpContext.Session; set => ctxAccessor.HttpContext.Session=value; }
		public override string TraceIdentifier { get => ctxAccessor.HttpContext.TraceIdentifier; set => ctxAccessor.HttpContext.TraceIdentifier=value; }
		public override ClaimsPrincipal User { get => ctxAccessor.HttpContext.User; set => ctxAccessor.HttpContext.User=value; }

		public override WebSocketManager WebSockets => ctxAccessor.HttpContext.WebSockets;

		public override void Abort()
		{
			ctxAccessor.HttpContext.Abort();
		}
	}
#endif
	[Serializable]
	public class GxContext : IGxContext
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Application.GxContext));
		internal static string GX_SPA_REQUEST_HEADER = "X-SPA-REQUEST";
		internal static string GX_SPA_REDIRECT_URL = "X-SPA-REDIRECT-URL";
		internal const string GXLanguage = "GXLanguage";
		internal const string GXTheme = "GXTheme";
		[NonSerialized]
		HttpContext _HttpContext;
		[NonSerialized]
		HtmlTextWriter _outputWriter;
		[NonSerialized]
		NameValueCollection _httpHeaders;
		ArrayList _DataStores;
		[NonSerialized]
		GxXmlContext _XMLContext;
		[NonSerialized]
		GxErrorHandlerInfo _errorHandlerInfo;
		string _gxUserId;
		string _clientId = string.Empty;
		string _gxPasswrd;
		string _gxDbName;
		string _gxDbSrv;
		short _wbHandled;
		short _wbGlbDoneStart;
		[NonSerialized]
		msglist _gxMsgList;
		short _nUserReturn;
		string _sCallerURL;
		[NonSerialized]
		FileIO _fileIoInstance;
		[NonSerialized]
		FtpService _ftpInstance;
		short _nLocRead;
		[NonSerialized]
		GxLocationCollection _colLocations;
		int _nSOAPErr;
		string _sSOAPErrMsg;
		string _currentLocation = "";
		string _configSection = "";
		int _handle = -1;
		object beforeCommitObj, afterCommitObj, beforeRollbackObj, afterRollbackObj, beforeConnectObj, afterConnectObj;
		bool inBeforeCommit;
		bool inAfterCommit;
		bool inBeforeRollback;
		bool inAfterRollback;
		bool SkipPushUrl;
		[NonSerialized]
		Hashtable _properties;
		[NonSerialized]
		IReportHandler _reportHandler;
		string _theme = "";
		[NonSerialized]
		ArrayList _reportHandlerToClose;
		private bool configuredEventHandling;
		private static string _physicalPath;
		[NonSerialized]
		private LocalUtil _localUtil;
		private bool _responseCommited;
		private bool _refreshAsGET;
		private bool wrapped;
		public bool IsCrawlerRequest { get; set; }
		private int drawGridsAtServer = -1;
		public bool HtmlHeaderClosed { private set; get; }
		public bool DrawingGrid { set; get; }
		public GxHttpContextVars httpContextVars { set; get; }
		[NonSerialized]
		private IGxSession _session;
		private bool _isSumbited;
		[NonSerialized]
		private OlsonTimeZone _currentTimeZone;

		[NonSerialized]
		MessageQueueTransaction _mqTransaction;
		bool _mqTransactionNull = true;

		[NonSerialized]
		HttpCookieCollection _localCookies;
		string _HttpRequestMethod;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		[NonSerialized]
		Dictionary<string, CookieContainer> cookieContainers;
		static string COOKIE_CONTAINER = "GX_COOKIECONTAINER";
		private bool _ignoreSpa;

		private const string _serviceWorkerFileName = "service-worker.js";
		private bool? _isServiceWorkerDefined = null;

		private const string _webAppManifestFileName = "manifest.json";
		private bool? _isWebAppManifestDefined = null;

		private static string CACHE_INVALIDATION_TOKEN;

		private bool IsServiceWorkerDefined
		{
			get
			{
				if (_isServiceWorkerDefined == null)
				{
					_isServiceWorkerDefined = CheckFileExists(_serviceWorkerFileName);
				}
				return _isServiceWorkerDefined == true;
			}
		}

		private bool IsWebAppManifestDefined
		{
			get
			{
				if (_isWebAppManifestDefined == null)
				{
					_isWebAppManifestDefined = CheckFileExists(_webAppManifestFileName);
				}
				return _isWebAppManifestDefined == true;
			}
		}
		public static GxContext CreateDefaultInstance()
		{
			GxContext context = new GxContext();
			DataStoreUtil.LoadDataStores(context);
			string theme = Preferences.GetDefaultTheme();
			if (!string.IsNullOrEmpty(theme))
				context.SetDefaultTheme(theme);
			return context;
		}
		public GxContext()
		{
			_DataStores = new ArrayList(2);
			LocalInitialize();
			_errorHandlerInfo = new GxErrorHandlerInfo();
			setContext(this);
			httpContextVars = new GxHttpContextVars();
			GXLogging.Debug(log, "GxContext.Ctr Default handle:", () => _handle.ToString());
		}
		public GxContext(int handle, string location)
		{
			_DataStores = new ArrayList(2);
			_currentLocation = location;
			_handle = handle;
			_errorHandlerInfo = new GxErrorHandlerInfo();
			setContext(this);
			httpContextVars = new GxHttpContextVars();
		}
		public GxContext(String location)
		{
			GXLogging.Debug(log, "GxContext.Ctr, parameters location=", location);
			_DataStores = new ArrayList(2);
			_errorHandlerInfo = new GxErrorHandlerInfo();
			setContext(this);
			httpContextVars = new GxHttpContextVars();
			GXLogging.Debug(log, "Return GxContext.Ctr");
		}

		public GxContext(int handle, ArrayList dataStores, HttpContext httpContext)
		{
			_DataStores = dataStores;
			_HttpContext = httpContext;
			_HttpRequestMethod = "";
			_handle = handle;
			_errorHandlerInfo = new GxErrorHandlerInfo();
			setContext(this);
			httpContextVars = new GxHttpContextVars();
		}
#if NETCORE
		private Dictionary<string, IEnumerable<Cookie>> ToSerializableCookieContainer(Dictionary<string, CookieContainer> cookies)
		{
			if (cookies == null)
				return null;
			Dictionary<string, IEnumerable<Cookie>> serializableCookies = new Dictionary<string, IEnumerable<Cookie>>();
			foreach(string key in cookies.Keys)
			{
				serializableCookies[key] = cookies[key].GetCookies();
			}
			return serializableCookies;
		}
		private Dictionary<string, CookieContainer> FromSerializableCookieContainer(Dictionary<string, IEnumerable<Cookie>> cookies)
		{
			if (cookies == null)
				return null;
			Dictionary<string, CookieContainer> serializableCookies = new Dictionary<string, CookieContainer>();
			
			foreach (string key in cookies.Keys)
			{
				CookieCollection cookieco = new CookieCollection();
				IEnumerable<Cookie> cookiesEnum = cookies[key];
				foreach(Cookie c in cookiesEnum)
				{
					cookieco.Add(c);
				}
				CookieContainer cc = new CookieContainer();
				cc.Add(cookieco);
				serializableCookies[key] = cc;
			}
			return serializableCookies;
		}
#else
		private Dictionary<string, CookieContainer> ToSerializableCookieContainer(Dictionary<string, CookieContainer> cookies){
			return cookies;
		}
		private Dictionary<string, CookieContainer> FromSerializableCookieContainer(Dictionary<string, CookieContainer> cookies)
		{
			return cookies;
		}
#endif
		public void UpdateSessionCookieContainer()
		{
			IGxSession tempStorage = GetSession();
			tempStorage.Set(COOKIE_CONTAINER, ToSerializableCookieContainer(cookieContainers));
		}
		public CookieContainer GetCookieContainer(string url, bool includeCookies = true)
		{
			try
			{
				CookieContainer container = null;
				IGxSession tempStorage = GetSession();
#if NETCORE
				cookieContainers = FromSerializableCookieContainer(tempStorage.Get<Dictionary<string, IEnumerable<Cookie>>>(COOKIE_CONTAINER));
#else
				cookieContainers =tempStorage.Get<Dictionary<string, CookieContainer>>(COOKIE_CONTAINER);
#endif
				if (cookieContainers == null)
				{
					cookieContainers = new Dictionary<string, CookieContainer>();
					tempStorage.Set(COOKIE_CONTAINER, ToSerializableCookieContainer(cookieContainers));
				}
				string domain = (new Uri(url)).GetLeftPart(UriPartial.Authority);
				if (cookieContainers.TryGetValue(domain, out container) && includeCookies)
				{
					return container;
				}
				else
				{
					container = new CookieContainer();
					cookieContainers[domain] = container;
				}
				return container;
			}
			catch (Exception ex)
			{
				GXLogging.Debug(log, ex, "GetCookieContainer error url:", url);

			}
			return new CookieContainer();
		}

		[NonSerialized]
		static GxContext _currentGxContext;
		static public GxContext Current
		{
			get
			{
#if !NETCORE
                if (HttpContext.Current != null)
                {

                    GxContext currCtx = (GxContext)HttpContext.Current.Items["CURRENT_GX_CONTEXT"];
                    if (currCtx != null)
                        return currCtx;
                }
                else
                {

                    return _currentGxContext;
                }
                return null;
#else
				return _currentGxContext;
#endif
			}
		}
		static void setContext(GxContext ctx)
		{
#if !NETCORE
            if (HttpContext.Current != null)
                HttpContext.Current.Items["CURRENT_GX_CONTEXT"] = ctx;
            else
#endif
			_currentGxContext = ctx;
		}

		public LocalUtil localUtil
		{
			get
			{
				if (_localUtil == null) _localUtil = GXResourceManager.GetLocalUtil(GetLanguage());
				return _localUtil;
			}
		}

		public bool ResponseCommited
		{
			get
			{
				return _responseCommited;
			}
			set
			{
				_responseCommited = value;
			}
		}

		private bool bCloseCommand;

		public void ajax_rsp_command_close()
		{
			bCloseCommand = true;
			try
			{
				JObject closeParms = new JObject();
				closeParms.Put("values", HttpAjaxContext.GetParmsJArray(this.returnParms));
				closeParms.Put("metadata", HttpAjaxContext.GetParmsJArray(this.returnParmsMetadata));
				httpAjaxContext.appendAjaxCommand("close", closeParms);
			}
			catch (Exception)
			{
			}
		}

		public void ajax_rsp_clear()
		{
			httpAjaxContext.ajax_rsp_clear();
		}

		public bool isCloseCommand { get { return bCloseCommand; } }

		[NonSerialized]
		private Object[] returnParms = Array.Empty<Object>();
		private Object[] returnParmsMetadata = Array.Empty<Object>();

		public void setWebReturnParms(Object[] retParms)
		{
			this.returnParms = retParms;
		}

		public void setWebReturnParmsMetadata(Object[] retParmsMetadata)
		{
			this.returnParmsMetadata = retParmsMetadata;
		}
		public static string GetHttpRequestPostedFileType(HttpContext httpContext, string varName)
		{
			try
			{
				HttpPostedFile pf = httpContext.Request.GetFile(varName);
				if (pf != null)
					return FileUtil.GetFileType(pf.FileName);
			}
			catch { }
			return string.Empty;
		}

		public static string GetHttpRequestPostedFileName(HttpContext httpContext, string varName)
		{
			try
			{
				HttpPostedFile pf = httpContext.Request.GetFile(varName);
				if (pf != null)
					return FileUtil.GetFileName(pf.FileName);
			}
			catch { }
			return string.Empty;
		}

		public static bool GetHttpRequestPostedFile(IGxContext gxContext, string varName, out string fileToken)
		{
			var httpContext = gxContext.HttpContext;
			fileToken = null;
			if (httpContext != null)
			{
				HttpPostedFile pf = httpContext.Request.GetFile(varName);
				if (pf != null)
				{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					FileInfo fi = new FileInfo(pf.FileName);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					string tempDir = Preferences.getTMP_MEDIA_PATH();
					string ext = fi.Extension;
					if (ext != null)
						ext = ext.TrimStart('.');
					string filePath = FileUtil.getTempFileName(tempDir);
					GXLogging.Debug(log, "cgiGet(" + varName + "), fileName:" + filePath);
					GxFile file = new GxFile(tempDir, filePath, GxFileType.PrivateAttribute);
					filePath = file.Create(pf.InputStream);
					string fileGuid = GxUploadHelper.GetUploadFileGuid();
					fileToken = GxUploadHelper.GetUploadFileId(fileGuid);

					GxUploadHelper.CacheUploadFile(fileGuid, filePath, pf.FileName, ext, file, gxContext);

				return true;
				}
			}
			return false;
		}

		public String getWebReturnParmsJS()
		{
			return HttpAjaxContext.GetParmsJArray(this.returnParms).ToString();
		}

		public String getWebReturnParmsMetadataJS()
		{
			return HttpAjaxContext.GetParmsJArray(this.returnParmsMetadata).ToString();
		}

		[NonSerialized]
		HttpAjaxContext _httpAjaxContext;
		public HttpAjaxContext httpAjaxContext
		{
			get
			{
				if (_httpAjaxContext == null)
					_httpAjaxContext = new HttpAjaxContext();
				return _httpAjaxContext;
			}
		}

		private String sAjaxCmpContent = String.Empty;

		public String AjaxCmpContent
		{
			get { return sAjaxCmpContent; }
			set { sAjaxCmpContent = value; }
		}

		public void PushAjaxCmpContent(String Content)
		{
			sAjaxCmpContent += Content;
		}


		private bool bIsEnabled = true;

		public bool isEnabled
		{
			get { return bIsEnabled; }
			set { bIsEnabled = value; }
		}

		public bool isOutputEnabled()
		{
			return this.isEnabled;
		}

		public void disableOutput()
		{
			this.isEnabled = false;
		}

		public void enableOutput()
		{
			this.isEnabled = true;
		}

		private int nCmpDrawLvl;

		public int CmpDrawLvl
		{
			get { return nCmpDrawLvl; }
			set { nCmpDrawLvl = value; }
		}

		protected bool PortletMode;
		protected bool AjaxCallMode;
		protected bool AjaxEventMode;
		protected bool FullAjaxMode;

		public void setPortletMode()
		{ PortletMode = true; }

		public void setAjaxCallMode()
		{ AjaxCallMode = true; }

		public void setAjaxEventMode()
		{ AjaxEventMode = true; }

		public void setFullAjaxMode()
		{ FullAjaxMode = true; }

		public bool isPortletMode()
		{ return PortletMode; }

		public bool isAjaxCallMode()
		{ return AjaxCallMode; }

		public bool isAjaxEventMode()
		{ return AjaxEventMode; }

		public bool isAjaxRequest()
		{ return isAjaxCallMode() || isAjaxEventMode() || isPortletMode(); }

		public bool isFullAjaxMode()
		{ return FullAjaxMode; }


		public bool isSpaRequest(bool ignoreFlag)
		{

			if (!ignoreFlag && _ignoreSpa)
			{
				return false;
			}
			return GetRequestMethod() == "GET" && !isAjaxRequest() && HttpContext.Request.Headers[GX_SPA_REQUEST_HEADER] == "1";
		}

		public bool isSpaRequest()
		{
			return isSpaRequest(false);
		}

		public bool isMultipartRequest()
		{ return IsMultipartRequest; }


		public void DisableSpaRequest()
		{
			_ignoreSpa = true;
		}

		private StringCollection deferredFragments = new StringCollection();
		private StringCollection javascriptSources = new StringCollection();
		private StringCollection styleSheets = new StringCollection();

		private HashSet<string> deferredJavascriptSources = new HashSet<string>();


		public void AddJavascriptSource(string jsSrc, string urlBuildNumber)
		{
			AddJavascriptSource(jsSrc, urlBuildNumber, false, true);
		}

		public void ClearJavascriptSources()
		{
			javascriptSources.Clear();
		}

		public void AddDeferredFrags()
		{
			foreach (string each_fragment in deferredFragments)
			{
				WriteHtmlText(each_fragment);
			}
		}

		public void AddJavascriptSource(string jsSrc, string urlBuildNumber, bool userDefined, bool isInlined)
		{
			if (!string.IsNullOrWhiteSpace(jsSrc) && !JavascriptSourceAdded(jsSrc))
			{
				javascriptSources.Add(jsSrc);
				string queryString = GetURLBuildNumber(jsSrc, urlBuildNumber);
				string attributes = "";
				if (userDefined)
				{
					queryString = "";
					attributes = "data-gx-external-script";
				}
				string fragment = "<script type=\"text/javascript\" src=\"" + GetCompleteURL(jsSrc) + queryString + "\" " + attributes + "></script>";
				if (isAjaxRequest() || isInlined || jsSrc == "jquery.js" || jsSrc == "gxcore.js")
				{
					WriteHtmlText(fragment);
				}
				else
				{
					deferredFragments.Add(fragment);
				}

				// After including jQuery, include all the deferred Javascript files
				if (jsSrc == "jquery.js")
				{
					foreach (string deferredJsSrc in deferredJavascriptSources)
					{
						AddJavascriptSource(deferredJsSrc, "", false, true);
					}
				}
				// After including gxgral, set the Service Worker Url if one is defined
				if (jsSrc == "gxgral.js" && IsServiceWorkerDefined)
				{
					WriteHtmlText($"<script>gx.serviceWorkerUrl = \"{GetCompleteURL(_serviceWorkerFileName)}\";</script>");
				}
			}
		}

		[Obsolete("AddJavascriptSource with 1 argument is deprecated", false)]
		public void AddJavascriptSource(string jsSrc)
		{
			AddJavascriptSource(jsSrc, string.Empty);
		}

		public void AddDeferredJavascriptSource(string jsSrc, string urlBuildNumber)
		{
			deferredJavascriptSources.Add(GetCompleteURL(jsSrc) + urlBuildNumber);
		}

		public bool JavascriptSourceAdded(string jsSrc)
		{
			return javascriptSources.Contains(jsSrc);
		}

		public void AddStyleSheetFile(string styleSheet)
		{
			styleSheets.Add(styleSheet);
		}

		public string GetURLBuildNumber(string resourcePath, string urlBuildNumber)
		{
			if (string.IsNullOrEmpty(urlBuildNumber) && !PathUtil.IsAbsoluteUrl(resourcePath) && !PathUtil.HasUrlQueryString(resourcePath))
			{
				return "?" + GetCacheInvalidationToken();
			}
			else
			{
				return urlBuildNumber;
			}
		}

		public string GetCacheInvalidationToken()
		{
			if (String.IsNullOrEmpty(CACHE_INVALIDATION_TOKEN))
			{
				string token;
				if (Config.GetValueOf("CACHE_INVALIDATION_TOKEN", out token))
				{
					CACHE_INVALIDATION_TOKEN = token;
				}
				else
				{
					CACHE_INVALIDATION_TOKEN = Math.Truncate(NumberUtil.Random() * 1000000).ToString();
				}
			}
			return CACHE_INVALIDATION_TOKEN;
		}

		public bool StyleSheetAdded(string styleSheet)
		{
			return styleSheets.Contains(styleSheet);
		}

		public void AddWebAppManifest()
		{
			if (IsWebAppManifestDefined)
			{
				WriteHtmlText($"<link rel=\"manifest\" href=\"{GetCompleteURL(_webAppManifestFileName)}\">");
			}
		}

		private bool CheckFileExists(string fileName)
		{
			bool fileExists = false;
			try
			{
				string path = Path.Combine(this.GetPhysicalPath(), fileName);
				fileExists = File.Exists(path);
				GXLogging.Info(log, $"Searching if file exists ({fileName}). Found: {fileExists}");
			}
			catch (Exception e)
			{
				fileExists = false;
				GXLogging.Error(log, e, $"Failed searching for a file ({fileName})");
			}
			return fileExists;
		}

		public void StatusMessage(string message)
		{
			StackFrame frame = new StackFrame(1);
#if NETCORE
			ILog statusLog = log4net.LogManager.GetLogger(frame.GetMethod().DeclaringType);
#else
            ILog statusLog = log4net.LogManager.GetLogger(frame.GetMethod().DeclaringType.FullName);
#endif
			GXLogging.Info(statusLog, message);
			Console.WriteLine(message);
		}

		public void AddComponentObject(string cmpCtx, string objName)
		{
			httpAjaxContext.ComponentObjects.Put(cmpCtx, objName);
		}

		public void SaveComponentMsgList(string cmpCtx)
		{
			httpAjaxContext.Messages.Put(cmpCtx, this.GX_msglist.GetJSONObject());
		}

		public void PrintReportAtClient(string reportFile)
		{
			PrintReportAtClient(reportFile, "");
		}

		public void PrintReportAtClient(string reportFile, string printerRule)
		{
			httpAjaxContext.PrintReportAtClient(PathToUrl(reportFile), printerRule);
		}

		public void SendComponentObjects()
		{
			httpAjaxContext.HiddenValues.Put("GX_CMP_OBJS", httpAjaxContext.ComponentObjects);
		}

		public void SendServerCommands()
		{
			if (!isAjaxRequest() && httpAjaxContext.Commands.Count > 0)
			{
				httpAjaxContext.HiddenValues.Put("GX_SRV_COMMANDS", httpAjaxContext.Commands.JSONArray);
			}
		}

		public int GetButtonType()
		{
			if (DrawGridsAtServer)
			{
				return TYPE_SUBMIT;
			}
			return TYPE_BUTTON;
		}

		public string GetCssProperty(string propName, string propValue)
		{
			int browserType = GetBrowserType();

			#region Align
			if (string.Compare(propName, "align", true) == 0)
			{
				if (browserType == BROWSER_FIREFOX)
				{
					return "-moz-" + propValue;
				}
				else if (browserType == BROWSER_CHROME || browserType == BROWSER_SAFARI)
				{
					return "-khtml-" + propValue;
				}
			}
			#endregion

			return propValue;
		}

		public bool DrawGridsAtServer
		{
			get
			{
				if (drawGridsAtServer == -1)
				{
					drawGridsAtServer = 0;
					string data;
					if (Config.GetValueOf("DrawGridsAtServer", out data))
					{
						if (string.Compare(data, "always", true) == 0)
						{
							drawGridsAtServer = 1;
						}
						else if (string.Compare(data, "ie6", true) == 0)
						{
							if (GetBrowserType() == BROWSER_IE)
							{
								if (GetBrowserVersion().StartsWith("6"))
								{
									drawGridsAtServer = 1;
								}
							}
						}
					}
				}
				return (drawGridsAtServer == 1);
			}
			set
			{
				drawGridsAtServer = value ? 1 : 0;
			}
		}

		private void LocalInitialize()
		{
			if (_handle == -1)
			{
				_handle = GxUserInfo.NewHandle();
			}
#if !NETCORE
            if (Preferences.Instrumented)
            {
                GxUserInfo.setProperty(_handle, GxDefaultProps.USER_NAME, Environment.UserName);
                GxUserInfo.setProperty(_handle, GxDefaultProps.PGM_NAME, AppDomain.CurrentDomain.FriendlyName);
                GxUserInfo.setProperty(_handle, GxDefaultProps.START_TIME, DateTime.Now.ToString());
            }
#endif
			GX_msglist = new msglist();
			Config.LoadConfiguration();
		}


		public static bool isReorganization { get; set; }


		public bool IsMultipartRequest
		{
			get
			{
				if (this.HttpContext != null)
#if NETCORE
					return MultipartRequestHelper.IsMultipartContentType(this.HttpContext.Request.ContentType);
#else
                    return this.HttpContext.Request.Files.Count > 0;
#endif
				else

					return false;
			}
		}
		public IGxContext UtlClone()
		{
			//Context for new LUW
			GxContext ctx = new GxContext();
			DataStoreUtil.LoadDataStores(ctx);
			ctx.SetPhysicalPath(this.GetPhysicalPath());
			ctx.SetSession(this.GetSession());
			if (this.HttpContext != null)
			{
				ctx.HttpContext = this.HttpContext;
				ctx.localCookies = this.localCookies;
			}
			ctx.httpContextVars = this.httpContextVars;
			return ctx;
		}
		public HttpContext HttpContext
		{
			get
			{
#if !NETCORE
                if (_HttpContext == null && HttpContext.Current != null)
                {
                    HttpContext = HttpContext.Current;
                }
#endif
				return _HttpContext;
			}

			set
			{
				_HttpContext = value;
				IsCrawlerRequest = isCrawlerRequest_impl();

				if (IsForward())
				{
					_HttpRequestMethod = "GET";
				}
				else
				{
					_HttpRequestMethod = _HttpContext.Request.GetMethod();
				}
			}
		}
		public bool IsForward()
		{
			if (_HttpContext != null)
			{
				string callMethod = (string)_HttpContext.Items["gx_webcall_method"];
				if ((callMethod != null) && (string.Compare(callMethod, "forward", true) == 0))
				{
					return true;
				}
			}
			return false;
		}

		public bool isCrawlerRequest_impl()
		{
			if (_HttpContext != null && _HttpContext.Request.QueryString.ToString().Contains("_escaped_fragment_"))
			{
				return true;
			}
			return false;
		}

		public HtmlTextWriter OutputWriter
		{
			get { return _outputWriter; }
			set { _outputWriter = value; }
		}
		public ArrayList DataStores
		{
			get { return _DataStores; }
		}
		public void AddDataStore(IGxDataStore datastore)
		{
			datastore.Handle = this.handle;
			_DataStores.Add(datastore);
		}
		public GxXmlContext Xml
		{
			get
			{
				if (_XMLContext == null)
					_XMLContext = new GxXmlContext(GetPhysicalPath());
				return _XMLContext;
			}
		}
		public string Gx_ope
		{
			get { return _errorHandlerInfo.Gx_ope; }
			set { _errorHandlerInfo.Gx_ope = value; }
		}
		public int Gx_dbe
		{
			get { return _errorHandlerInfo.Gx_dbe; }
			set { _errorHandlerInfo.Gx_dbe = value; }
		}
		public string Gx_dbt
		{
			get { return _errorHandlerInfo.Gx_dbt; }
			set { _errorHandlerInfo.Gx_dbt = value; }
		}
		public string Gx_etb
		{
			get { return _errorHandlerInfo.Gx_etb; }
			set { _errorHandlerInfo.Gx_etb = value; }
		}
		public short Gx_eop
		{
			get { return _errorHandlerInfo.Gx_eop; }
			set { _errorHandlerInfo.Gx_eop = value; }
		}
		public short Gx_err
		{
			get { return _errorHandlerInfo.Gx_err; }
			set { _errorHandlerInfo.Gx_err = value; }
		}

		public string Gx_dbsqlstate
		{
			get { return _errorHandlerInfo.Gx_dbsqlstate; }
			set { _errorHandlerInfo.Gx_dbsqlstate = value; }
		}
		public GxErrorHandlerInfo ErrorHandlerInfo
		{
			get { return _errorHandlerInfo; }
		}
		public string Gxuserid
		{
			get { return _gxUserId; }
			set { _gxUserId = value; }
		}
		public string Gxpasswrd
		{
			get { return _gxPasswrd; }
			set { _gxPasswrd = value; }
		}
		public string Gxdbname
		{
			get { return _gxDbName; }
			set { _gxDbName = value; }
		}
		public string Gxdbsrv
		{
			get { return _gxDbSrv; }
			set { _gxDbSrv = value; }
		}
		public string wjLoc
		{
			get { return httpContextVars.wjLoc; }
			set { httpContextVars.wjLoc = value; }
		}
		public int wjLocDisableFrm
		{
			get { return httpContextVars.wjLocDisableFrm; }
			set { httpContextVars.wjLocDisableFrm = value; }
		}
		public short wbHandled
		{
			get { return _wbHandled; }
			set { _wbHandled = value; }
		}
		public short wbGlbDoneStart
		{
			get { return _wbGlbDoneStart; }
			set { _wbGlbDoneStart = value; }
		}
		public msglist GX_msglist
		{
			get { return _gxMsgList; }
			set { _gxMsgList = value; }
		}
		public short nUserReturn
		{
			get { return _nUserReturn; }
			set { _nUserReturn = value; }
		}
		public string sCallerURL
		{
			get { return _sCallerURL; }
			set { _sCallerURL = value; }
		}
		public FileIO FileIOInstance
		{
			get
			{
				if (_fileIoInstance == null)
					_fileIoInstance = new FileIO();
				return _fileIoInstance;
			}
		}
		public FtpService FtpInstance
		{
			get
			{
				if (_ftpInstance == null)
					_ftpInstance = new FtpService();
				return _ftpInstance;
			}
		}
		public short nLocRead
		{
			get { return _nLocRead; }
			set { _nLocRead = value; }
		}
		public GxLocationCollection colLocations
		{
			get { return _colLocations; }
			set { _colLocations = value; }
		}
		public int nSOAPErr
		{
			get { return _nSOAPErr; }
			set { _nSOAPErr = value; }
		}
		public string sSOAPErrMsg
		{
			get { return _sSOAPErrMsg; }
			set { _sSOAPErrMsg = value; }
		}
		public IReportHandler reportHandler
		{
			get { return _reportHandler; }
			set
			{
				if (_reportHandlerToClose == null)
				{
					_reportHandlerToClose = new ArrayList();
				}
				if (value != null)
				{
					_reportHandlerToClose.Add(value);
				}
				_reportHandler = value;
			}
		}
		public int handle
		{
			get { return _handle; }
			set { _handle = value; }
		}
		public GXXMLWriter GX_xmlwrt
		{
			get { return Xml.Writer; }
		}
		public IGxDataStore GetDataStore(string id)
		{
			foreach (IGxDataStore ds in _DataStores)
				if (ds.Id.ToUpper() == id.ToUpper())
					return ds;
			return null;
		}
		public void CloseConnections()
		{
			GxUserInfo.RemoveHandle(this.handle);
			foreach (IGxDataStore ds in _DataStores)
				ds.CloseConnections();

			if (_reportHandlerToClose != null)
			{
				for (int i = 0; i < _reportHandlerToClose.Count; i++)
				{
					try
					{
						((IReportHandler)_reportHandlerToClose[i]).GxShutdown();
					}
					catch (Exception ex)
					{
						GXLogging.Error(log, "Error closing report", ex);
					}
				}
				_reportHandlerToClose.Clear();
			}
			if (localCookies != null)
				this.localCookies.Clear();
			if (javascriptSources != null)
				this.javascriptSources.Clear();
			if (deferredJavascriptSources != null)
				this.deferredJavascriptSources.Clear();
			if (this._colLocations != null)
				this._colLocations.Clear();
			if (this.DataStores != null)
				this._DataStores.Clear();
			if (this._gxMsgList != null)
				this._gxMsgList.Clear();
		}
		public void CommitDataStores()
		{
			CommitDataStores("");
		}
		public void CommitDataStores(string callerName, IDataStoreProvider ds)
		{
			ExecuteBeforeCommit(callerName);
			if (ds != null && (ds is DataStoreProvider dataStoreProvider))
			{
				dataStoreProvider.commitDataStores(callerName);
			}
			else
			{
				foreach (IGxDataStore d in _DataStores)
					d.Commit();
			}
			ExecuteAfterCommit(callerName);

			if (!_mqTransactionNull)
			{
				CommitMQ();
			}

		}
		public void CommitDataStores(string callerName)
		{
			CommitDataStores(callerName, null);
		}

		private void CommitMQ()
		{
			if (MQTransaction != null)
				if (MQTransaction.Status == MessageQueueTransactionStatus.Pending)
					MQTransaction.Commit();
		}
		private void AbortMQ()
		{
			if (MQTransaction != null)
				if (MQTransaction.Status == MessageQueueTransactionStatus.Pending)
					MQTransaction.Abort();
		}

		public void Disconnect()
		{
			foreach (IGxDataStore ds in _DataStores)
				ds.Disconnect();
			GXLogging.Debug(log, "Local Disconnect");
		}

		public void RollbackDataStores()
		{
			RollbackDataStores("");
		}
		public void RollbackDataStores(string callerName)
		{
			RollbackDataStores(callerName, null);
		}
		public void RollbackDataStores(string callerName, IDataStoreProvider ds)
		{
			ExecuteBeforeRollback(callerName);

			if (ds != null && (ds is DataStoreProvider dataStoreProvider))
			{
				dataStoreProvider.rollbackDataStores(callerName);
			}
			else
			{
				foreach (IGxDataStore d in _DataStores)
					d.Rollback();
			}

			ExecuteAfterRollback(callerName);
			if (!_mqTransactionNull)
			{
				AbortMQ();
			}
		}
		public DateTime ServerNow(string dataSource)
		{
			IGxDataStore dstore = GetDataStore(dataSource);
			return (dstore.DateTime);
		}
		public DateTime ServerNowMs(string dataSource)
		{
			IGxDataStore dstore = GetDataStore(dataSource);
			return (dstore.DateTimeMs);
		}
		public string ServerVersion(string dataSource)
		{
			IGxDataStore dstore = GetDataStore(dataSource);
			return (dstore.Version);
		}
		public string DataBaseName(string dataSource)
		{
			IGxDataStore dstore = GetDataStore(dataSource);
			return dstore.Connection.DatabaseName;
		}
		public bool isRemoteGXDB()
		{
			return Preferences.Remote ? !Preferences.RemoteLocation.Equals(_currentLocation) : false;
		}

		public string getCurrentLocation()
		{
			return _currentLocation;
		}
		public virtual string CleanAbsoluteUri
		{
			get
			{
				string QueryString = RemoveInternalParms(_HttpContext.Request.GetQuery());
				return _HttpContext.Request.GetAbsolutePath() + ((QueryString.Length > 0 && !QueryString.Contains("?")) ? "?" : "") + QueryString;
			}

		}
		public virtual string BaseUrl
		{
			get
			{
				try
				{
					//Don´t use Uri.ToString() it returns unescaped canonical representation
					return AbsoluteUri;
				}
				catch
				{
					return "";
				}
			}
			set { }
		}
		public string ConfigSection
		{
			get { return _configSection; }
			set { _configSection = value; }
		}
		static public string StdClassesVersion()
		{
			object[] customAtts = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
			if (customAtts != null && customAtts.Length > 0)
			{
				AssemblyFileVersionAttribute verAtt = customAtts[0] as AssemblyFileVersionAttribute;
				if (verAtt != null)
					return verAtt.Version;
			}
			return Assembly.GetAssembly(typeof(GxContext)).GetName().Version.ToString();
		}

		public MessageQueueTransaction MQTransaction
		{
			get { return _mqTransaction; }
			set
			{
				_mqTransaction = value;
				_mqTransactionNull = false;
			}
		}

		public HttpCookieCollection localCookies
		{
			get { return _localCookies; }
			set { _localCookies = value; }
		}

		public GxHttpResponse GX_webresponse
		{
			get
			{
				return new GxHttpResponse(this);
			}
		}

		public const int TYPE_RESET = 0;
		public const int TYPE_SUBMIT = 1;
		public const int TYPE_BUTTON = 2;

		public const int BROWSER_OTHER = 0;
		public const int BROWSER_IE = 1;
		public const int BROWSER_NETSCAPE = 2;
		public const int BROWSER_OPERA = 3;
		public const int BROWSER_UP = 4;
		public const int BROWSER_POCKET_IE = 5;
		public const int BROWSER_FIREFOX = 6;
		public const int BROWSER_CHROME = 7;
		public const int BROWSER_SAFARI = 8;
		public const int BROWSER_EDGE = 9;
		public const int BROWSER_OPERA_MINI = 10;
		public const int BROWSER_INDEXBOT = 20;
		public string GetRequestMethod()
		{
			try
			{
				if (string.IsNullOrEmpty(_HttpRequestMethod))
				{
					if (_HttpContext != null && _HttpContext.Request != null)
						_HttpRequestMethod = _HttpContext.Request.GetMethod();
					else
						_HttpRequestMethod = string.Empty;
				}
				return _HttpRequestMethod;
			}
			catch
			{
				return "";
			}
		}
		public string RemoveInternalParms(string query)
		{
			query = RemoveEventPrefix(query);
			query = RemoveInternalSuffixes(query);
			return query;
		}
		internal static string RemoveInternalSuffixes(string query)
		{
			int idx = query.IndexOf(GXNavigationHelper.POPUP_LEVEL);
			if (idx == 1)
				return "";
			if (idx > 1)
				query = query.Substring(0, idx - 1);
			idx = query.IndexOf("gx-no-cache=");
			if (idx >= 0)
			{
				idx = (idx > 0) ? idx - 1 : idx;
				query = query.Substring(0, idx);
			}
			return query;
		}
		private string RemoveEventPrefix(string query)
		{
			if (IsGxAjaxRequest() || isAjaxEventMode())
			{
				int comIdx = query.IndexOf(",");
				if (comIdx == -1)
					comIdx = query.IndexOf("&");
				if (comIdx != -1)
					query = query.Substring(comIdx + 1);
			}
			return query;
		}
		public string GetRequestQueryString()
		{
			try
			{
				string requestQuery = _HttpContext.Request.GetQuery();
				if (requestQuery.Length == 0)
					return "";
				string query = requestQuery.Substring(1, requestQuery.Length - 1);
				return RemoveInternalParms(query);
			}
			catch
			{
				return "";
			}
		}

		public const string GX_NAV_HELPER = "GX_NAV_HELPER";

		public GXNavigationHelper GetNavigationHelper()
		{
			GXNavigationHelper helper = ReadSessionKey<GXNavigationHelper>(GX_NAV_HELPER);
			if (helper == null)
			{
				helper = new GXNavigationHelper();
				if (WriteSessionKey(GX_NAV_HELPER, helper))
					return helper;
				else
					return null;
			}
			else
			{
				return helper;
			}
		}
		public T ReadSessionKey<T>(string key) where T : class
		{
			try
			{
				if (httpAjaxContext != null && httpAjaxContext.SessionType == SessionType.NO_SESSION)
					return default(T);
				else if (_HttpContext != null && _HttpContext.Session != null)
				{
#if !NETCORE
                    return (T)_HttpContext.Session[key];
#else
					string value = _HttpContext.Session.GetString(key);
					if (value != null)
					{
						return JSONHelper.DeserializeNullDefaultValue<T>(value);
					}
#endif
				}
				return default(T);
			}
			catch (InvalidOperationException) //Session has not been configured for this application or request. IE 11
			{
				return default(T);
			}
		}
		public bool WriteSessionKey<T>(string key, T value) where T : class
		{
			try
			{
				if (HttpContext != null && _HttpContext.Session != null && httpAjaxContext != null && httpAjaxContext.SessionType != SessionType.NO_SESSION)
				{

#if !NETCORE
                    HttpContext.Session[key] = value;
#else
					if (!_HttpContext.Response.HasStarted)
					{
						HttpContext.Session.SetString(key, (value != null ? JSONHelper.Serialize(value) : string.Empty));
					}
#endif
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (InvalidOperationException) //Session has not been configured for this application or request. IE 11
			{
				return false;
			}
		}

		internal string GetRequestNavUrl()
		{
			string sUrl = string.Empty;
			if (isAjaxRequest() && _HttpContext != null)
			{
				Uri u = _HttpContext.Request.GetUrlReferrer();
				sUrl = (u != null) ? GetPublicUrl(u.AbsoluteUri) : string.Empty;
				int qMarkPos;
				if ((qMarkPos = sUrl.IndexOf('?')) != -1)
				{
					sUrl = sUrl.Substring(0, qMarkPos) + u.Query;
					if (!string.IsNullOrEmpty(u.Fragment))
					{
						sUrl += u.Fragment;
					}
				}
			}
			else
			{
				sUrl = AbsoluteUri;
			}
			return sUrl;
		}

		internal string AbsoluteUri
		{
			get
			{
				if (DynamicPortRequest(_HttpContext.Request))
				{
					string absoluteUri = string.Empty;
					string host = _HttpContext.Request.Headers["X-Forwarded-Host"];
					if (!string.IsNullOrEmpty(host))
					{
						string Resource = _HttpContext.Request.GetRawUrl().Replace('\\', '/');
						absoluteUri = String.Format("{0}://{1}{2}", GetServerSchema(), host, Resource);
					}
					else
					{
						absoluteUri = String.Format("{0}://{1}{2}", GetServerSchema(), _HttpContext.Request.Headers["Host"], _HttpContext.Request.GetRawUrl());
					}
					GXLogging.Debug(log, "AbsoluteUri dynamicport:", absoluteUri);
					return absoluteUri;
				}
				else
				{
					return GetPublicUrl(_HttpContext.Request.GetAbsoluteUri());
				}
			}
		}

		public string GetPublicUrl(string absoluteUrl)
		{
			UriBuilder uriB = new UriBuilder(absoluteUrl) { Scheme = GetServerSchema() };
			return uriB.Uri.AbsoluteUri.Replace(":80/", "/");
		}

		public void DeleteReferer(int popupLevel)
		{
			GXNavigationHelper navHelper = GetNavigationHelper();
			if (navHelper != null)
				navHelper.DeleteStack(popupLevel);
		}

		public void DeleteReferer()
		{
			GXNavigationHelper navHelper = this.GetNavigationHelper();
			if (navHelper != null)
			{
				string sUrl = this.GetRequestNavUrl().Trim();
				int popupLevel = navHelper.GetUrlPopupLevel(sUrl);
				this.DeleteReferer(popupLevel);
			}
		}

		public Boolean isPopUpObject()
		{
			GXNavigationHelper navHelper = GetNavigationHelper();
			if (navHelper != null)
				return navHelper.GetUrlPopupLevel(GetRequestNavUrl()) != -1;
			else
				return false;
		}

		public void WindowClosed()
		{
			int popupLevel = GetNavigationHelper().GetUrlPopupLevel(GetRequestNavUrl());
			if (popupLevel == -1)
				PopReferer();
			else
				DeleteReferer(popupLevel);
		}

		public void PopReferer()
		{
			GXNavigationHelper navHelper = GetNavigationHelper();
			if (navHelper != null)
				navHelper.PopUrl(GetRequestNavUrl());
		}

		public string GetReferer()
		{
			try
			{
				if (!IsLocalStorageSupported())
				{
					GXNavigationHelper navHelper = GetNavigationHelper();
					if (navHelper != null)
						return navHelper.GetRefererUrl(GetRequestNavUrl());
					else
						return "";
				}
				else
				{
					string temp = (this.httpAjaxContext != null && this.httpAjaxContext.FormVars != null) ? this.httpAjaxContext.FormVars["sCallerURL"] : "";
					string referer = (!String.IsNullOrEmpty(temp)) ? temp : String.Empty;
					if (string.IsNullOrEmpty(referer))
					{
						GXNavigationHelper nav = this.GetNavigationHelper();
						string selfUrl = GetRequestNavUrl().Trim();
						if (nav.Count() > 0)
						{
							referer = nav.PeekUrl(selfUrl);
						}
						if (string.IsNullOrEmpty(referer) && this.HttpContext != null && this.HttpContext.Request != null && this.HttpContext.Request.GetUrlReferrer() != null)
						{
							temp = this.HttpContext.Request.GetUrlReferrer().AbsoluteUri;
							referer = (!selfUrl.Equals(temp)) ? temp : referer;
						}
					}
					return referer;
				}
			}
			catch
			{
				return string.Empty;
			}
		}

		public void PushCurrentUrl()
		{
			if (GetRequestMethod().Equals("GET") && !isAjaxRequest())
			{
				string sUrl = GetRequestNavUrl().Trim();
				GXNavigationHelper navHelper = GetNavigationHelper();
				if (navHelper != null)
				{
					string topURL = navHelper.PeekUrl(sUrl);
					if (topURL != sUrl && !String.IsNullOrEmpty(sUrl))
					{
						navHelper.PushUrl(sUrl, false);
					}
				}
			}
		}
		public void DoAfterInit()
		{
		}

		public bool isSmartDevice()
		{
			String userAgent = null;
			try
			{
				if (_HttpContext != null)
					userAgent = _HttpContext.Request.GetUserAgent();
			}
			catch { }
			if (userAgent != null)
			{
				if ((userAgent.IndexOf("Windows CE")) != -1)
					return true;
				else if ((userAgent.IndexOf("iPhone")) != -1)
					return true;
				else if ((userAgent.IndexOf("Android")) != -1)
					return true;
				else if ((userAgent.IndexOf("BlackBerry")) != -1)
					return true;
				else if ((userAgent.IndexOf("Opera Mini")) != -1)
					return true;
			}
			return false;
		}
		public bool ExposeMetadata()
		{
			return Preferences.ExposeMetadata;
		}
		public bool IsLocalStorageSupported()
		{
			bool supported = String.IsNullOrEmpty(this.GetCookie("GXLocalStorageSupport"));
			if (supported)
			{
				switch (GetBrowserType())
				{
					case GxContext.BROWSER_OPERA_MINI:
						supported = false;
						break;
					case GxContext.BROWSER_IE:
						float ver;
						float.TryParse(GetBrowserVersion(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out ver);
						bool docTypeDefined = Preferences.DocType != HTMLDocType.NONE && Preferences.DocType != HTMLDocType.UNDEFINED;
						supported = ver >= 9 && docTypeDefined;
						break;
				}
			}
			return supported;
		}

		public int GetBrowserType()
		{
			String userAgent;
			try
			{
				if (_HttpContext != null)
					userAgent = _HttpContext.Request.GetUserAgent();
				else
					return 0;
			}
			catch
			{
				return 0;
			}
			if (userAgent != null)
			{
				if (userAgent.ToUpper().IndexOf("CHROME") != -1)
				{
					return BROWSER_CHROME;
				}

				if (userAgent.ToUpper().IndexOf("FIREFOX") != -1)
				{
					return BROWSER_FIREFOX;
				}
				else if ((userAgent.IndexOf("Edge")) != -1)
				{
					return BROWSER_EDGE;
				}
				if ((userAgent.IndexOf("MSIE")) != -1)
				{
					if ((userAgent.IndexOf("Windows CE")) != -1)
						return BROWSER_POCKET_IE;
					else
						return BROWSER_IE;
				}
				else if (userAgent.IndexOf("Trident") != -1)
				{
					return BROWSER_IE;
				}
				else if (userAgent.ToUpper().IndexOf("OPERA MINI") != -1)
				{
					return BROWSER_OPERA_MINI;
				}
				else if (userAgent.ToUpper().IndexOf("OPERA") != -1)
				{
					return BROWSER_OPERA;
				}
				else if (userAgent.ToUpper().IndexOf("UP.Browser") != -1)
				{
					return BROWSER_UP;
				}

				else if (userAgent.ToUpper().IndexOf("SAFARI") != -1)
				{
					return BROWSER_SAFARI;
				}
				else if (userAgent.ToUpper().IndexOf("MOZILLA/") != -1)
				{
					return BROWSER_NETSCAPE;
				}
				else if (userAgent.ToUpper().IndexOf("MOZILLA/") != -1)
				{
					return BROWSER_NETSCAPE;
				}
				else if (Regex.IsMatch(userAgent, "Googlebot|AhrefsBot|bingbot|MJ12bot", RegexOptions.IgnoreCase))
				{
					return BROWSER_INDEXBOT;
				}
			}
			return BROWSER_OTHER;
		}
		public string GetBrowserVersion()
		{
			try
			{
				String userAgent = _HttpContext.Request.GetUserAgent();
				int type = GetBrowserType();
				int i, i2;

				if (type == BROWSER_EDGE)
				{
					MatchCollection matches = Regex.Matches(userAgent, " Edge\\/([0-9]+)\\.");
					if (matches.Count > 0)
						return matches[0].Groups[1].Value;
				}
				if (type == BROWSER_POCKET_IE || type == BROWSER_IE)
				{
					i = userAgent.IndexOf("MSIE");
					if (i >= 0)
					{
						i2 = userAgent.IndexOf(";", i);
						if (i2 != -1)
						{
							string version = userAgent.Substring(i + 4, i2 - i - 4).Trim();
							return (version.StartsWith("7") && userAgent.ToLower().IndexOf("trident") >= 0) ? "8" : version;
						}
					}
					else
					{
						i = userAgent.IndexOf("rv:");
						i2 = userAgent.IndexOf(".", i);
						return userAgent.Substring(i + 3, i2 - i - 3).Trim();
					}
				}
				else if (type == BROWSER_OPERA)
				{
					//Mozilla/4.0 (Windows NT 4.0;US) Opera 3.60  [en]
					i = userAgent.IndexOf("Opera") + 6;
					i2 = userAgent.IndexOf(" ", i);
					if (i2 != -1)
						return userAgent.Substring(i, i2 - i).Trim();

				}
				else if (type == BROWSER_FIREFOX)
				{

					i = userAgent.IndexOf("Firefox/") + 8;
					i2 = userAgent.IndexOf(" ", i);
					if (i2 != -1)
						return userAgent.Substring(i, i2 - i).Trim();
					else
						return userAgent.Substring(i).Trim();
				}
				else if (type == BROWSER_CHROME)
				{
					i = userAgent.IndexOf("Chrome/") + 7;
					i2 = userAgent.IndexOf(" ", i);
					if (i2 != -1)
						return userAgent.Substring(i, i2 - i).Trim();

				}
				else if (type == BROWSER_SAFARI)
				{
					i = userAgent.IndexOf("Version/") + 8;
					i2 = userAgent.IndexOf(" ", i);
					if (i2 != -1)
						return userAgent.Substring(i, i2 - i).Trim();

				}
				else if (type == BROWSER_NETSCAPE)
				{

					i = userAgent.IndexOf("Mozilla/") + 8;
					i2 = userAgent.IndexOf(" ", i);
					if (i2 != -1)
						return userAgent.Substring(i, i2 - i).Trim();

				}
				else if (type == BROWSER_UP)
				{

					i = userAgent.IndexOf("UP.Browser/") + 8;
					i2 = userAgent.IndexOf("-", i);
					if (i2 != -1)
						return userAgent.Substring(i, i2 - i).Trim();
				}
				return "";
			}
			catch
			{
				return "";
			}
		}
		public virtual short GetHttpSecure()
		{
			try
			{
				if (_HttpContext.Request.GetIsSecureFrontEnd())
				{
					GXLogging.Debug(log, "Front-End-Https header activated");
					return 1;
				}
				else
					return _HttpContext.Request.GetIsSecureConnection();
			}
			catch
			{
				return 0;
			}
		}

		public void WriteHtmlText(string Content)
		{
			if (!ResponseCommited)
			{
				if (!isEnabled)
				{
					if (httpAjaxContext.isAjaxContent() || isSpaRequest())
						httpAjaxContext.writeAjaxContent(Content);

				}
				else
				{
					OutputWriter.Write(Content);
				}
			}

		}
		public void RenderUserControl(string controlType, string internalName, string htmlId, GxDictionary propbag)
		{
			string ucServerContent = String.Empty;

			if (GeneXus.UserControls.UserControlFactory.Instance != null)
			{
				propbag.Set("ContainerName", htmlId);
				ucServerContent = GeneXus.UserControls.UserControlFactory.Instance.RenderControl(controlType, internalName, propbag);
			}

			WriteHtmlText($"<div class=\"gx_usercontrol\" id=\"{htmlId}\">{ucServerContent}</div>");
		}

		public virtual void skipLines(long nToSkip)
		{
			for (int i = 0; i < nToSkip; i++)
				WriteHtmlText("\n");
		}

		public void WriteHtmlTextNl(string Content)
		{
			WriteHtmlText(Content);
			skipLines(1);
		}

		public void SendWebAttribute(string sText)
		{
			WriteHtmlText(GXUtil.AttributeEncode(sText));
		}

		public void SendWebValue(string sText)
		{
			WriteHtmlText(GXUtil.ValueEncode(sText));
		}
		public void SendWebValueSpace(string sText)
		{
			WriteHtmlText(GXUtil.ValueEncode(sText, true, false));
		}
		public void SendWebValueEnter(string sText)
		{
			WriteHtmlText(GXUtil.ValueEncode(sText, false, true));
		}
		public void SendWebValueComplete(string sText)
		{
			WriteHtmlText(GXUtil.ValueEncode(sText, true, true));
		}
		public string convertURL(string file)
		{
			string url = string.Empty;
			if (file.IndexOf(".") != -1)
				url = GetCompleteURL(file);
			else
				url = GetCompleteURL(GetImagePath(file, "", GetTheme()));

			if (this.GetWrapped() && !this.DrawGridsAtServer && !PathUtil.IsAbsoluteUrl(url) && !this.IsCrawlerRequest)
			{ //Is called from WebWrapper.GetResponse. 
				int ix = url.LastIndexOf("/");
				if (ix > 0)
					url = url.Substring(ix + 1, url.Length - ix - 1);
			}
			return url;
		}
		public string GetCompleteURL(string file)
		{
			String fout = file.Trim();

			if (PathUtil.IsAbsoluteUrl(file) || file.StartsWith("//") || (file.Length > 2 && file[1] == ':'))
			{
				return fout;
			}

			if (file.StartsWith("/"))
			{
				if (file.StartsWith(GetScriptPath()))
					return fout;
				return GetContextPath() + fout;
			}

			if (!String.IsNullOrEmpty(StaticContentBase) || Preferences.RewriteEnabled)
			{
				if (PathUtil.IsAbsoluteUrl(StaticContentBase))
					return StaticContentBase + fout;
				else
					return GetScriptPath() + StaticContentBase + fout;
			}
			else
				return file;

		}
		string staticContentBase;
		public string StaticContentBase
		{
			get
			{
				if (staticContentBase == null)
				{
					string dir = "";
					if (Config.GetValueOf("STATIC_CONTENT", out dir))
					{
						if (!(dir.EndsWith("/") || dir.EndsWith("\\")) && !String.IsNullOrEmpty(dir))
							staticContentBase = dir + "/";
						else
							staticContentBase = dir;
					}
					else
					{
						staticContentBase = "";
					}
				}
				return staticContentBase;
			}

			set { staticContentBase = value; }

		}

		public string getJSONResponse(string cmpContext = "")
		{
			if (isRedirected || isCloseCommand)
				return string.Empty;
			return httpAjaxContext.getJSONResponse(cmpContext);
		}

		public String getJSONResponse()
		{
			if (isRedirected || isCloseCommand)
				return string.Empty;
			return httpAjaxContext.getJSONResponse();
		}
		public string GetCookie(string name)
		{
			string cookieVal = string.Empty;
			HttpCookie cookie = TryGetCookie(localCookies, name);
			if (cookie == null && _HttpContext != null)
			{
				cookie = TryGetCookie(_HttpContext.Request.GetCookies(), name);
			}
			if (cookie != null && cookie.Value != null)
			{
#if NETCORE
				if (name == GxHttpCookie.GX_SESSION_ID && cookie.Value.Contains('+')) //Cookie compatibility with java, cookie value is already decoded
					cookieVal = cookie.Value;
				else
					cookieVal = HttpUtility.UrlDecode(cookie.Value);
#else
                cookieVal = HttpUtility.UrlDecode(cookie.Value);
#endif
			}
			return cookieVal;
		}

		private HttpCookie TryGetCookie(HttpCookieCollection cookieColl, string name)
		{
			if (cookieColl != null)
			{
				return cookieColl[name];
			}
			return null;
		}
		public short SetCookie(string name, string cookieValue, string path, DateTime expires, string domain, int secure)
		{

			return SetCookie(name, cookieValue, path, expires, domain, secure, GeneXus.Http.Server.GxHttpCookie.HttpOnlyDefault());
		}
		public short SetCookie(string name, string cookieValue, string path, DateTime expires, string domain, int secure, bool httponly)
		{
			if (_HttpContext == null || localCookies == null)
				return 0;
			HttpCookie cookie = new HttpCookie(name, GXUtil.UrlEncode(cookieValue));
			cookie.Path = path.TrimEnd();
			//HttpCookie.Path default is /, which is the server root. 
			//In Genexus: If path isn’t specified, the cookie is valid for the web panels that are in the same directory as the one it is stored in, or in subordinated directories

			if (!expires.Equals(DateTimeUtil.NullDate()))
				cookie.Expires = expires;
			cookie.Domain = String.IsNullOrEmpty(domain) ? cookie.Domain : domain;
			cookie.Secure = (secure == 1) ? true : false;
			cookie.HttpOnly = httponly;
#if NETCORE

			if (localCookies.ContainsKey(name))
			{
				_HttpContext.Response.Cookies.Delete(name);
			}
			CookieOptions cookieOptions = new CookieOptions()
			{
				Domain = cookie.Domain,
				HttpOnly = cookie.HttpOnly,
				Path = cookie.Path,
				Secure = cookie.Secure
			};
			string sameSite;
			SameSiteMode sameSiteMode = SameSiteMode.Unspecified;
			if (Config.GetValueOf("SAMESITE_COOKIE", out sameSite) && Enum.TryParse<SameSiteMode>(sameSite, out sameSiteMode))
			{
				cookieOptions.SameSite = sameSiteMode;
			}
			if (!expires.Equals(DateTimeUtil.NullDate()))
				cookieOptions.Expires = DateTime.SpecifyKind(cookie.Expires, DateTimeKind.Utc);

			_HttpContext.Response.Cookies.Append(name, cookie.Value, cookieOptions);
			localCookies[name] = cookie;
#else
            if (_HttpContext.Response.Cookies.Get(name) != null)
            {

                try
                {
                    _HttpContext.Response.Cookies.Set(cookie);
                }
                catch (HttpException) { }
                localCookies.Set(cookie);
            }
            else
            {

                try
                {
                    _HttpContext.Response.Cookies.Add(cookie);
                }
                catch (HttpException) { }
                localCookies.Add(cookie);
            }
#endif
			return 0;
		}
		public byte ResponseContentType(String sContentType)
		{
			if (_HttpContext == null || this.GetWrapped())
				return 0;
			if (String.IsNullOrEmpty(sContentType))
				return 1;
#if NETCORE
			if (!_HttpContext.Response.HasStarted)
#endif
				_HttpContext.Response.ContentType = sContentType;
			return 0;
		}
		public byte RespondFile(string name)
		{
			if (_HttpContext == null || this.GetWrapped())
				return 0;
			FileInfo fi = new FileInfo(name);
			ResponseContentType(contentTypeForExtension(fi.Extension));
			_HttpContext.Response.WriteFile(name);
			return 0;
		}
		public byte SetHeader(string name, string value)
		{
			if (_HttpContext == null || this.GetWrapped())
				return 0;
			if (_httpHeaders == null)
			{
				_httpHeaders = new NameValueCollection();
			}
			_httpHeaders[name] = value;
			SetCustomHttpHeader(name, value);
			return 0;
		}

		private void SetCustomHttpHeader(string name, string value)
		{
			_HttpContext.Response.AppendHeader(name, value);

#if !NETCORE
            switch (name.ToUpper())
            {
                case "CACHE-CONTROL":
                    var Cache = _HttpContext.Response.Cache;
                    string[] values = value.Split(',');
                    foreach (var v in values)
                    {
                        switch (v.Trim().ToUpper())
                        {
                            case "PUBLIC":
                                Cache.SetCacheability(HttpCacheability.Public);
                                break;
                            case "PRIVATE":
                                Cache.SetCacheability(HttpCacheability.Private);
                                break;
                            case "NO-CACHE":
                                Cache.SetCacheability(HttpCacheability.NoCache);
                                break;
                            case "NO-STORE":
                                Cache.AppendCacheExtension("no-store, must-revalidate");
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }
#else
			switch (name.ToUpper())
			{
				case "CACHE-CONTROL":
					switch (value.ToUpper())
					{
						case "PUBLIC":
							_HttpContext.Response.AddHeader("Cache-Control", "public");
							break;
						case "PRIVATE":
							_HttpContext.Response.AddHeader("Cache-Control", "private");
							break;
						default:
							GXLogging.Warn(log, String.Format("Could not set Cache Control Http Header Value '{0}' to HttpResponse", value));
							break;
					}
					break;
			}
#endif
		}

		public string GetHeader(string name)
		{
			if (_httpHeaders != null)
			{
				return _httpHeaders[name];
			}
			else
			{
				return null;
			}
		}
		private bool ForwardAsWebCallMethod()
		{
			string callMethod = "";
			Config.GetValueOf("WEB_CALL_METHOD", out callMethod);
			if (string.Compare(callMethod, "forward", true) == 0)
			{
				return true;
			}
			return false;
		}
		private void DoRedirect(string jumpUrl)
		{
			pushUrlSessionStorage();
			_HttpContext.Items["gx_webcall_method"] = "redirect";
			SetHeader("Location", jumpUrl);
			_HttpContext.Response.StatusCode = 301;
		}
		private void DoForward(string jumpUrl)
		{
#if !NETCORE
            _HttpContext.Items["gx_webcall_method"] = "forward";
            _HttpContext.RewritePath(jumpUrl);
            IHttpHandler handler = new GeneXus.HttpHandlerFactory.HandlerFactory().GetHandler(_HttpContext, "GET", jumpUrl, jumpUrl);
            handler.ProcessRequest(_HttpContext);
#endif
		}
		protected void httpRedirect(String jumpUrl)
		{
			Redirected = true;
			if (ResponseCommited)
				return;
			if (_HttpContext == null)
				return;
			if (ForwardAsWebCallMethod())
			{
				try
				{
					DoForward(jumpUrl);
				}
				catch (Exception)
				{

					DoRedirect(jumpUrl);
				}
			}
			else
			{
				DoRedirect(jumpUrl);
			}
			ResponseCommited = true;
		}

		private bool Redirected;

		public bool isRedirected
		{
			get { return Redirected; }

		}
		internal static string GX_AJAX_REQUEST_HEADER = "GxAjaxRequest";

		private bool IsGxAjaxRequest()
		{
			if (!string.IsNullOrEmpty(HttpContext.Request.Headers[GX_AJAX_REQUEST_HEADER]))
			{
				return true;
			}
			return false;
		}
		private string UrlPrefix(string url)
		{
			if (!url.Contains("?"))
				return "?";
			else if (url.Contains("="))
				return "&";
			else
				return ",";

		}
		private void Redirect_impl(String url, GXWindow win)
		{
			if (!IsGxAjaxRequest() && !isAjaxRequest() && win == null)
			{
				GXNavigationHelper navHelper = GetNavigationHelper();
				int popupLvl = navHelper.GetUrlPopupLevel(GetRequestNavUrl());
				string popLvlParm = "";
				if (popupLvl != -1)
				{
					popLvlParm = UrlPrefix(url);
					popLvlParm += GXUtil.UrlEncode("gxPopupLevel=" + popupLvl + ";");
				}

				if (isSpaRequest(true))
				{
					pushUrlSessionStorage();
					HttpContext.Response.Headers[GX_SPA_REDIRECT_URL] = url + popLvlParm;
				}
				else
				{
					httpRedirect(url + popLvlParm);
				}
			}
			else
			{
				try
				{
					if (win != null)
					{
						httpAjaxContext.appendAjaxCommand("popup", win.GetJSONObject());
					}
					else
					{
						JObject cmdParms = new JObject();
						cmdParms.Put("url", url);
						if (this.wjLocDisableFrm > 0)
						{
							cmdParms.Put("forceDisableFrm", this.wjLocDisableFrm); //Disable Web Form ONLY when redirecting to WebPage.
						}
						Redirected = true;
						httpAjaxContext.appendAjaxCommand("redirect", cmdParms);
					}
				}
				catch (Exception)
				{
					httpRedirect(url);
				}
			}
		}

		private void pushUrlSessionStorage()
		{
			if (this.IsLocalStorageSupported() && !SkipPushUrl)
			{
				PushCurrentUrl();
			}
			SkipPushUrl = false;
		}
		public void Redirect(String url)
		{
			Redirect(url, false);
		}

		public void Redirect(String url, bool bSkipPushUrl)
		{
			SkipPushUrl = bSkipPushUrl;
			if (!Redirected)
			{
				Redirect_impl(url, null);
			}
		}

		public void PopUp(String url)
		{
			PopUp(url, Array.Empty<Object>());
		}

		public void PopUp(String url, Object[] returnParms)
		{

			GXWindow win = new GXWindow();
			win.Url = url;
			win.SetReturnParms(returnParms);
			NewWindow(win);

		}

		public void NewWindow(GXWindow win)
		{
			Redirect_impl(win.Url, win);
		}

		private void DoAjaxRefresh(string command)
		{
			string refreshMethod = "POST";
			if (_refreshAsGET)
			{
				refreshMethod = "GET";
			}
			httpAjaxContext.appendAjaxCommand(command, refreshMethod);
		}

		public void DoAjaxRefresh()
		{
			DoAjaxRefresh("refresh");
		}

		public void DoAjaxRefreshForm()
		{
			DoAjaxRefresh("refresh_form");
		}

		public void DoAjaxRefreshCmp(String sPrefix)
		{
			httpAjaxContext.appendAjaxCommand("cmp_refresh", sPrefix);
		}
#if !NETCORE
        public void DoAjaxLoad(int SId, GXWebRow row)
        {
            JObject JSONRow = new JObject();
            JSONRow.Put("grid", SId);
            JSONRow.Put("props", row.parentGrid.GetJSONObject());
            JSONRow.Put("values", row.parentGrid.GetValues());
            httpAjaxContext.appendLoadData(SId, JSONRow);
        }
#endif
		public void DoAjaxAddLines(int SId, int lines)
		{
			JObject JSONData = new JObject();
			JSONData.Put("grid", SId);
			JSONData.Put("count", lines);
			httpAjaxContext.appendAjaxCommand("addlines", JSONData);
		}
		public void DoAjaxSetFocus(string ControlName)
		{
			httpAjaxContext.appendAjaxCommand("set_focus", ControlName);
		}

		public void DispatchAjaxCommands()
		{
			if (!ResponseCommited)
			{
				if (!this.IsMultipartRequest)
				{
					_HttpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
				}
				SendFinalJSONResponse(httpAjaxContext.getJSONResponse());
				ResponseCommited = true;
			}
		}

		public void CloseHtmlHeader()
		{
			this.WriteHtmlTextNl("</head>");
			this.HtmlHeaderClosed = true;
		}

		public void SendFinalJSONResponse(string json)
		{
			bool isMultipartResponse = !ResponseCommited && this.IsMultipartRequest;
			if (isMultipartResponse)
			{
				_HttpContext.Response.Write("<html><head></head><body><input type='hidden' data-response-content-type='application/json' value='");
			}
			_HttpContext.Response.Write(json);
			if (isMultipartResponse)
				_HttpContext.Response.Write("'/></body></html>");
		}

		public string BuildHTMLColor(int lColor)
		{
			return ColorTranslator.ToHtml(Color.FromArgb(lColor));

		}
		float getHTMLVersion()
		{
#if !NETCORE
            if (_HttpContext.Request.Browser.MajorVersion >= 4 &&
                (_HttpContext.Request.Browser.Type.ToUpper().IndexOf("IE") > -1 ||
                _HttpContext.Request.Browser.Type.ToUpper().IndexOf("NS") > -1))
                return 4.0f;
#endif
			return 1.0f;
		}
		public string BuildHTMLFont(String type, int size, String color)
		{
			StringBuilder buffer = new StringBuilder();
			if (!String.IsNullOrEmpty(color))
			{
				if (getHTMLVersion() < 4.0)
					buffer.Append("<FONT FACE=\"").Append(type).Append("\" SIZE=").Append(size.ToString()).Append(" COLOR=").Append(color).Append(">");
				else
					buffer.Append("<SPAN STYLE=\"font:").Append(size.ToString()).Append("pt '").Append(type).Append("';color:").Append(color).Append("\">");
			}
			return buffer.ToString();
		}

		public string GetRemoteAddress()
		{
			try
			{
#if NETCORE
				return _HttpContext.Request.Host.Host;
#else
                return _HttpContext.Request.UserHostAddress;
#endif
			}
			catch
			{
				return "";
			}
		}

		public virtual string GetServerName()
		{

			try
			{
				string serverName;
				if (Config.GetValueOf("SERVER_NAME", out serverName))
					return serverName;
#if !NETCORE
                serverName = _HttpContext.Request.ServerVariables["http_host"];
#endif
				if (String.IsNullOrEmpty(serverName))
				{
					serverName = _HttpContext.Request.GetHost();
				}
				int pos = serverName.IndexOf(':');
				if (pos > 0)
					return serverName.Substring(0, pos);
				return serverName;
			}
			catch

			{
				return "";
			}
		}
		private static bool DynamicPortRequest(HttpRequest request)
		{
			return request.Headers != null && (!string.IsNullOrEmpty(request.Headers["Host"]) || !string.IsNullOrEmpty(request.Headers["X-Forwarded-Host"]));
		}
		internal static int GetServerPort(HttpRequest request, bool isSecure)
		{
			try
			{
				if (request != null)
				{
					string host = request.Headers["Host"];
					if (!string.IsNullOrEmpty(host))
					{
						int idx = host.IndexOf(':');
						if (idx >= 0)
							return int.Parse(host.Substring(idx + 1));
					}
					return request.GetPort(isSecure);
				}
				else return 0;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "GetServerPort error", ex);
				return 0;
			}

		}
		public virtual int GetServerPort()
		{
			return GetServerPort(_HttpContext != null ? _HttpContext.Request : null, GetHttpSecure() == 1);
		}
		private bool RequestDefaultPort
		{
			get
			{
                try
                {
                    return _HttpContext.Request.IsDefaultPort();
                }
                catch
				{
					return false;
				}
			}
		}
		public virtual string GetServerSchema()
		{
			try
			{
				if (GetHttpSecure() == 1)
				{
					return GXUri.UriSchemeHttps;
				}
				return _HttpContext.Request.GetScheme();
			}
			catch
			{
				return GXUri.UriSchemeHttp;
			}
		}
		private bool FrontEndHttps()
		{
			if (CheckHeaderValue("Front-End-Https", "on") || CheckHeaderValue("X-Forwarded-Proto", "https"))
			{
				GXLogging.Debug(log, "Front-End-Https header activated");
				return true;
			}
			else
			{
				return false;
			}
		}
		private bool CheckHeaderValue(String headerName, String headerValue)
		{
			string httpsHeader = _HttpContext.Request.Headers[headerName];
			if (!string.IsNullOrEmpty(httpsHeader) && httpsHeader.Equals(headerValue, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return false;
		}
		public virtual string GetScriptPath()
		{
			try
			{
#if NETCORE
				var request = _HttpContext.Request;
				if (request.GetRawUrl().EndsWith(HttpHelper.GXOBJECT, StringComparison.OrdinalIgnoreCase))
				{
					if (request.PathBase != null && request.PathBase.HasValue)
						return request.PathBase.Value + "/";
					else
						return Config.ScriptPath + "/";
				}
#endif
				string appPath = _HttpContext.Request.GetApplicationPath();
				if (appPath.EndsWith("/"))
					return _HttpContext.Request.GetApplicationPath();
				else
					return _HttpContext.Request.GetApplicationPath() + "/";
			}
			catch
			{
				return string.Empty;
			}
		}

		public void SetPhysicalPath(string path)
		{
			_physicalPath = path;
		}

		public string GetPhysicalPath()
		{
			if (string.IsNullOrEmpty(_physicalPath))
			{
				try
				{
					if (HttpContext != null)
					{
						string phPath = HttpHelper.RequestPhysicalApplicationPath(_HttpContext);
						if (phPath.EndsWith("\\") || phPath.EndsWith("/"))
							_physicalPath = phPath;
						else
							_physicalPath = phPath + "/";
					}
					else
					{
						_physicalPath = "";
					}
				}
				catch (Exception ex)
				{
					GXLogging.Debug(log, "GetPhysicalPath error", ex);
					_physicalPath = "";
				}
			}
			return _physicalPath;
		}
		public static bool IsRestService
		{
			get
			{
#if !NETCORE
                return WebOperationContext.Current != null;
#else
				return false;
#endif
			}
		}
#if NETCORE
		static bool _isHttpContext;
#endif
		public static bool IsHttpContext
		{
			get
			{
#if !NETCORE
                return HttpContext.Current != null;
#else
				return _isHttpContext;
#endif
			}
#if NETCORE
			set
			{
				_isHttpContext = value;
			}
#endif
		}

		public static string StaticPhysicalPath()
		{
			try
			{
				if (IsHttpContext)
				{
					string phPath = HttpHelper.RequestPhysicalApplicationPath();
					if (phPath.EndsWith("\\") || phPath.EndsWith("/"))
						return phPath;
					else
						return phPath + "/";
				}
				else if (IsRestService)
				{
					return Directory.GetParent(FileUtil.GetStartupDirectory()).FullName + Path.DirectorySeparatorChar;
				}
				else if (!string.IsNullOrEmpty(_physicalPath))
				{
					return _physicalPath;
				}

				else
				{
					return Directory.GetCurrentDirectory();
				}

			}
			catch
			{
				return "";
			}

		}
		public byte DeleteFile(string fileName)
		{
			return FileUtil.DeleteFile(PathUtil.CompletePath(fileName, GetPhysicalPath()));
		}
		public byte FileExists(string fileName)
		{
			return FileUtil.FileExists(PathUtil.CompletePath(fileName, GetPhysicalPath()));
		}
		public string GetDynUrl()
		{
			return "";
		}
		public bool CheckContentType(string contentKey, string contentType, string fullPath)
		{
			if (String.IsNullOrEmpty(contentType.Trim()))
			{
				int lastDot = fullPath.LastIndexOf(".");
				if (lastDot != -1)
				{
					string ext = fullPath.Substring(lastDot + 1);
					contentType = contentTypeForExtension(ext);
				}
			}
			return contentType.ToLower().StartsWith(contentKey.ToLower() + "/");
		}
		string contentTypeForExtension(string fExt)
		{
			fExt = fExt.Trim();
			foreach (string[] ctExt in contentTypes)
				if (String.Compare(ctExt[0], fExt, StringComparison.OrdinalIgnoreCase) == 0)
					return ctExt[1];
			return String.Empty;
		}
		public string ExtensionForContentType(string contentType)
		{
			if (!string.IsNullOrEmpty(contentType))
			{
				contentType = contentType.ToLower();
				foreach (string[] ct in contentTypes)
					if (ct[1] == contentType)
						return ct[0];
			}
			return String.Empty;
		}
		static String[][] contentTypes = new string[][] {
															 new string[] {"txt"	, "text/plain"},
															 new string[] {"rtx"	, "text/richtext"},
															 new string[] {"htm"	, MediaTypesNames.TextHtml},
															 new string[] {"html"	, MediaTypesNames.TextHtml},
															 new string[] {"xml"	, "text/xml"},
															 new string[] {"rtf"	, "text/rtf"},
															 new string[] {"a3gpp"  , "audio/3gpp"},
															 new string[] {"aif"    , "audio/x-aiff"},
															 new string[] {"au"		, "audio/basic"},
															 new string[] {"m4a"    , "audio/mp4"},
															 new string[] {"mp3"    , "audio/mpeg"},
															 new string[] {"wav"    , "audio/wav"},
															 new string[] {"wav"    , "audio/x-wav"},
															 new string[] {"caf"    , "audio/x-caf"},
															 new string[] {"ram"    , "audio/x-pn-realaudio"},
															 new string[] {"bmp"    , "image/bmp"},
															 new string[] {"gif"    , "image/gif"},
															 new string[] {"jpg"    , "image/jpeg"},
															 new string[] {"jpeg"	, "image/jpeg"},
															 new string[] {"jpe"    , "image/jpeg"},
															 new string[] {"jpg"    , "application/jpg"},
															 new string[] {"jpeg"   , "application/jpeg"},
															 new string[] {"jfif"	, "image/pjpeg"},
															 new string[] {"tif"    , "image/tiff"},
															 new string[] {"tiff"	, "image/tiff"},
															 new string[] {"png"    , "image/png"},
															 new string[] {"png"    , "image/x-png"},
															 new string[] {"mpg"    , "video/mpeg"},
															 new string[] {"mpeg"	, "video/mpeg"},
															 new string[] {"mov"    , "video/quicktime"},
															 new string[] {"qt"		, "video/quicktime"},
															 new string[] {"avi"    , "video/x-msvideo"},
															 new string[] {"mp4"    , "video/mp4"},
															 new string[] {"divx"   , "video/x-divx"},
															 new string[] {"3gp"    , "video/3gpp"},
															 new string[] {"3g2"    , "video/3gpp2"},
															 new string[] {"exe"    , "application/octet-stream"},
															 new string[] {"dll"    , "application/x-msdownload"},
															 new string[] {"ps"		, "application/postscript"},
															 new string[] {"pdf"    , "application/pdf"},
															 new string[] {"tgz"    , "application/x-compressed"},
															 new string[] {"zip"    , "application/zip"},
															 new string[] {"zip"    , "application/x-zip-compressed"},
															 new string[] {"tar"    , "application/x-tar"},
															 new string[] {"rar"    , "application/x-rar-compressed"},
															 new string[] {"gz"		, "application/x-gzip"}
														 };

		public int GetSoapErr()
		{
			return _nSOAPErr;
		}
		public string GetSoapErrMsg()
		{
			return _sSOAPErrMsg;
		}
		void configEventHandling()
		{
			string evtProcName, ns, className, assemblyName;
			if (Config.GetValueOf("AppMainNamespace", out ns))
			{
				if (Config.GetValueOf("EVENT_BEFORE_COMMIT", out evtProcName))
				{
					parseEventHandlingName(evtProcName, out className, out assemblyName);
					beforeCommitObj = ClassLoader.FindInstance(assemblyName, ns, className, new Object[] { this }, null);
				}
				if (Config.GetValueOf("EVENT_AFTER_COMMIT", out evtProcName))
				{
					parseEventHandlingName(evtProcName, out className, out assemblyName);
					afterCommitObj = ClassLoader.FindInstance(assemblyName, ns, className, new Object[] { this }, null);
				}
				if (Config.GetValueOf("EVENT_BEFORE_ROLLBACK", out evtProcName))
				{
					parseEventHandlingName(evtProcName, out className, out assemblyName);
					beforeRollbackObj = ClassLoader.FindInstance(assemblyName, ns, className, new Object[] { this }, null);
				}
				if (Config.GetValueOf("EVENT_AFTER_ROLLBACK", out evtProcName))
				{
					parseEventHandlingName(evtProcName, out className, out assemblyName);
					afterRollbackObj = ClassLoader.FindInstance(assemblyName, ns, className, new Object[] { this }, null);
				}
				if (Config.GetValueOf("EVENT_BEFORE_CONNECT", out evtProcName))
				{
					parseEventHandlingName(evtProcName, out className, out assemblyName);
					beforeConnectObj = ClassLoader.FindInstance(assemblyName, ns, className, new Object[] { this }, null);
				}
				if (Config.GetValueOf("EVENT_AFTER_CONNECT", out evtProcName))
				{
					parseEventHandlingName(evtProcName, out className, out assemblyName);
					afterConnectObj = ClassLoader.FindInstance(assemblyName, ns, className, new Object[] { this }, null);
				}
			}
			configuredEventHandling = true;
		}
		static void parseEventHandlingName(string input, out string className, out string assemblyName)
		{
			className = "";
			assemblyName = "";
			string[] inputSplitted = input.Split(',');
			if (inputSplitted.Length == 1)
			{
				className = inputSplitted[0].Trim();
				assemblyName = className;
			}
			if (inputSplitted.Length == 2)
			{
				className = inputSplitted[0].Trim();
				assemblyName = inputSplitted[1].Trim();
			}
			return;
		}
		public void ExecuteBeforeCommit(string callerName)
		{
			if (!configuredEventHandling) configEventHandling();
			if (!inBeforeCommit)
			{
				inBeforeCommit = true;
				if (beforeCommitObj != null)
					ClassLoader.ExecuteVoidRef(beforeCommitObj, "execute", new Object[] { callerName });
				inBeforeCommit = false;
			}
		}
		public void ExecuteAfterCommit(string callerName)
		{
			if (!configuredEventHandling) configEventHandling();
			if (!inAfterCommit)
			{
				inAfterCommit = true;
				if (afterCommitObj != null)
					ClassLoader.ExecuteVoidRef(afterCommitObj, "execute", new Object[] { callerName });
				inAfterCommit = false;
			}
		}
		public void ExecuteBeforeRollback(string callerName)
		{
			if (!configuredEventHandling) configEventHandling();
			if (!inBeforeRollback)
			{
				inBeforeRollback = true;
				if (beforeRollbackObj != null)
					ClassLoader.ExecuteVoidRef(beforeRollbackObj, "execute", new Object[] { callerName });
				inBeforeRollback = false;
			}
		}
		public void ExecuteAfterRollback(string callerName)
		{
			if (!configuredEventHandling) configEventHandling();
			if (!inAfterRollback)
			{
				inAfterRollback = true;
				if (afterRollbackObj != null)
					ClassLoader.ExecuteVoidRef(afterRollbackObj, "execute", new Object[] { callerName });
				inAfterRollback = false;
			}
		}
		public bool ExecuteBeforeConnect(IGxDataStore datastore)
		{
			if (!configuredEventHandling) configEventHandling();
			if (beforeConnectObj != null)
			{
				GXLogging.Debug(log, "ExecuteBeforeConnect");
				ClassLoader.ExecuteVoidRef(beforeConnectObj, "execute", new Object[] { datastore });
				return true;
			}
			else
			{
				return false;
			}
		}
		public bool ExecuteAfterConnect(String datastoreName)
		{
			if (!configuredEventHandling) configEventHandling();
			if (afterConnectObj != null)
			{
				GXLogging.Debug(log, "ExecuteAfterConnect");
				ClassLoader.ExecuteVoidRef(afterConnectObj, "execute", new Object[] { datastoreName });
				return true;
			}
			else
			{
				return false;
			}
		}
		~GxContext()
		{
			GxUserInfo.RemoveHandle(_handle);
			GXFileWatcher.Instance.DeleteTemporaryFiles(_handle);
		}
		public void SetProperty(string key, string value)
		{
			if (HttpContext != null && HttpContext.Session != null)
			{
				GXLogging.Debug(log, "HttpContext.Session.setProperty(", key, ")=", value);
				WriteSessionKey(key, value);
			}
			else
			{
				if (_properties == null)
				{
					_properties = new Hashtable();
				}
				GXLogging.Debug(log, "GxContext.Properties.getProperty(", key, ")=", value);
				_properties[key] = value;
			}
		}
		public string GetProperty(string key)
		{
			string property = null;
			if (HttpContext != null && HttpContext.Session != null)
			{
				property = ReadSessionKey<string>(key);
			}
			else
			{
				if (_properties != null && _properties.Contains(key))
				{
					property = (string)_properties[key];
				}
			}
			return property;
		}
		public void SetContextProperty(string key, object value)
		{
			if (_properties == null)
			{
				_properties = new Hashtable();
			}
			_properties[key] = value;
		}
		public object GetContextProperty(string key)
		{
			object property = null;
			if (_properties != null && _properties.Contains(key))
			{
				property = _properties[key];
			}
			return property;
		}

		public string PathToUrl(string path)
		{
			GXLogging.Debug(log, "PathToUrl:", () => GetContextPath() + " relativePath:" + PathToRelativeUrl(path));
#pragma warning disable SYSLIB0013 // EscapeUriString
			return Uri.EscapeUriString(GetContextPath()) + PathToRelativeUrl(path, false);
#pragma warning disable SYSLIB0013 // EscapeUriString
		}

		public string PathToRelativeUrl(string path)
		{
			return PathToRelativeUrl(path, true);
		}

		public string PathToRelativeUrl(string path, bool relativeToServer)
		{
			Uri uri;
			string scriptPath = GetScriptPath();
			if (!Uri.TryCreate(path, UriKind.Absolute, out uri) || uri.Scheme != GXUri.UriSchemeFile)
			{
				//Relative URL => make sure it honour relativeToServer
				if (!relativeToServer && path.StartsWith(scriptPath))
					path = path.Remove(0, scriptPath.Length);
				return path;
			}
			string Resource = path;
			string fileName = Path.GetFileNameWithoutExtension(Resource);
			string basePath = GetPhysicalPath();
			if (string.IsNullOrEmpty(path.Trim()))
				return string.Empty;
			Resource = Resource.Replace('\\', '/');
			string basePath1 = basePath.ToLower().Replace('\\', '/');
			if (Resource.ToLower().StartsWith(basePath1) && Resource.Length >= basePath.Length)
				Resource = Resource.Substring(basePath.Length);
			else
			{
				Resource = Resource.Substring(Resource.LastIndexOf("/") + 1);
			}

#pragma warning disable SYSLIB0013 // EscapeUriString
			Resource = StringUtil.ReplaceLast(Resource, fileName, Uri.EscapeUriString(fileName));
#pragma warning disable SYSLIB0013 // EscapeUriString
			if (relativeToServer)
				return scriptPath + Resource;
			return Resource;
		}

		public string GetContextPath()
		{
			string serverName = GetServerName();
			int serverPort = GetServerPort();
			if (serverName.IndexOf(':') >= 0 || RequestDefaultPort || serverPort == 0)
			{
				return String.Format("{0}://{1}{2}", GetServerSchema(), serverName, GetScriptPath());
			}
			else
			{
				return String.Format("{0}://{1}:{2}{3}", GetServerSchema(), serverName, serverPort, GetScriptPath());
			}
		}

		public string GetBuildNumber(int buildN)
		{
			int buildNumber = buildN;
			string aux = String.Empty;
			if (Config.GetValueOf("GX_BUILD_NUMBER", out aux))
			{
				Int32.TryParse(aux, out buildNumber);
				buildNumber = Math.Max(buildN, buildNumber);
			}
			return buildNumber.ToString();

		}
		private static bool IsKnownContentType(string type)
		{
			if (!string.IsNullOrEmpty(type))
			{
				for (int i = 0; i < contentTypes.Length; i++)
				{
					if (contentTypes[i].Length >= 2)
					{
						if (string.Compare(type.Trim(), contentTypes[i][1], true) == 0)
							return true;
					}
				}
			}
			return false;
		}

		public string GetContentType(string type)
		{
			type = type.Trim();
			if (IsKnownContentType(type))
			{
				return type;
			}
			string emptyContentType = MediaTypesNames.TextHtml;
			try
			{
				string ltype = type.ToLower();
				string contentType = contentTypeForExtension(ltype);
				if (!String.IsNullOrEmpty(contentType))
					return contentType;

				string ext = Path.GetExtension(ltype);
				if (ext != null)
					ext = ext.TrimStart('.');
				contentType = contentTypeForExtension(ext);

				if (String.IsNullOrEmpty(contentType))
					contentType = emptyContentType;

				return contentType;
			}
			catch
			{
				return emptyContentType;
			}
		}

		public string GetMessage(string id, string language)
		{
			if (string.IsNullOrEmpty(language))
				return GXResourceManager.GetMessage(GetLanguage(), id);
			else
				return GXResourceManager.GetMessage(language, id);
		}
		public string GetMessage(string id, object[] args)
		{
			return GXResourceManager.GetMessage(GetLanguage(), id, args);
		}
		public string GetMessage(string id)
		{
			return GXResourceManager.GetMessage(GetLanguage(), id);
		}
		public void SetWrapped(bool wrapped)
		{
			this.wrapped = wrapped;
		}
		public bool GetWrapped()
		{
			return this.wrapped || this.IsCrawlerRequest;
		}

		public IGxSession GetSession()
		{
			if (this._session == null)
			{
				if (IsStandalone)
					this._session = new GxSession();
				else
					this._session = new GxWebSession(this);
			}
			return this._session;
		}

		internal bool IsStandalone => this._session is GxSession || this._isSumbited || this.HttpContext == null;

		internal void SetSession(IGxSession value)
		{
			if (value != null)
				this._session = value;
		}

		public int SetLanguage(string id)
		{
			if (Config.GetLanguageProperty(id, "code") != null)
			{
				SetProperty(GXLanguage, id);
				_localUtil = GXResourceManager.GetLocalUtil(id);
				_refreshAsGET = true;
				return 0;
			}
			else
			{
				return 1;
			}
		}
		private int SetLanguageWithoutSession(string id)
		{
			if (Config.GetLanguageProperty(id, "code") != null)
			{
				SetContextProperty(GXLanguage, id);
				_localUtil = GXResourceManager.GetLocalUtil(id);
				_refreshAsGET = true;
				return 0;
			}
			else
			{
				return 1;
			}
		}
		public string GetLanguage()
		{
			string prop = GetProperty(GXLanguage);
			if (!String.IsNullOrEmpty(prop))
				return prop;
			else if (Config.GetValueOf("LANG_NAME", out prop))
			{
				if (HttpContext != null && HttpContext.Session != null)
				{
					WriteSessionKey(GXLanguage, prop);
				}
				return prop;
			}
			else
				return "English";
		}
		public string GetLanguageProperty(String propName)
		{

			return Config.GetLanguageProperty(GetLanguage(), propName);
		}

		internal static string GX_REQUEST_TIMEZONE = "GxTZOffset";

		public OlsonTimeZone ClientTimeZone
		{
			get
			{
				if (_currentTimeZone != null)
					return _currentTimeZone;
				string sTZ = _HttpContext == null ? "" : (string)_HttpContext.Request.Headers[GX_REQUEST_TIMEZONE];
				GXLogging.Debug(log, "ClientTimeZone GX_REQUEST_TIMEZONE header:", sTZ);
				if (String.IsNullOrEmpty(sTZ))
				{
					sTZ = (string)GetCookie(GX_REQUEST_TIMEZONE);
					GXLogging.Debug(log, "ClientTimeZone GX_REQUEST_TIMEZONE cookie:", sTZ);
				}
				try
				{
					_currentTimeZone = String.IsNullOrEmpty(sTZ) ? TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id) : _currentTimeZone = TimeZoneUtil.GetInstanceFromOlsonName(sTZ);
				}
				catch (Exception e1)
				{
					GXLogging.Warn(log, "ClientTimeZone _currentTimeZone error", e1);
					try
					{
						_currentTimeZone = TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id);
					}
					catch (Exception e2)
					{
						GXLogging.Warn(log, "ClientTimeZone GetInstanceFromWin32Id error", e2);
						Preferences.StorageTimeZonePty storagePty = Preferences.getStorageTimezonePty();
						if (storagePty == Preferences.StorageTimeZonePty.Undefined)
							_currentTimeZone = null;
						else
							throw e2;
					}
				}
				return _currentTimeZone;
			}
		}

		public OlsonTimeZone GetOlsonTimeZone()
		{
			return TimeZoneUtil.GetInstanceFromOlsonName(GetTimeZone());
		}

		public String GetTimeZone()
		{
			string sTZ = GetProperty("GXTimezone");
			if (!String.IsNullOrEmpty(sTZ))
			{
				SetTimeZone(sTZ);
			}
			if (_currentTimeZone == null)
				_currentTimeZone = ClientTimeZone;
			return _currentTimeZone == null ? TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id).Name : _currentTimeZone.Name;
		}

		public Boolean SetTimeZone(String sTZ)
		{
			sTZ = StringUtil.RTrim(sTZ);
			Boolean ret = false;
			try
			{
				_currentTimeZone = TimeZoneUtil.GetInstanceFromOlsonName(sTZ);
				ret = true;
			}
			catch (Exception)
			{
				try
				{
					_currentTimeZone = TimeZoneUtil.GetInstanceFromWin32Id(sTZ);
					ret = true;
				}
				catch (Exception)
				{
					_currentTimeZone = TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id);
				}
			}
			SetProperty("GXTimezone", _currentTimeZone.Name);
			return ret;
		}
		private static ConcurrentDictionary<string, HashSet<string>> m_imagesDensity = new ConcurrentDictionary<string, HashSet<string>>();

		static Hashtable m_images = new Hashtable();

		const string IMAGES_TXT = "Images.txt";
		Hashtable Images
		{
			get
			{
				lock (m_images)
				{
					if (m_images.Count == 0)
					{

						string dir = GetPhysicalPath();
						string[] imageFiles = null;
						string filename = IMAGES_TXT;
						string imgDir = "";
						if (String.IsNullOrEmpty(dir) && _HttpContext == null)
						{

							int srchIx = 0;
							string[] paths = new string[] { ".\\", "..\\" };
							bool found = false;
							while (!found && srchIx < paths.Length)
							{
								dir = paths[srchIx++];
								imageFiles = Directory.GetFiles(dir, "*.txt");
								for (int i = 0; i < imageFiles.Length; i++)
									if (imageFiles[i].EndsWith(IMAGES_TXT, StringComparison.OrdinalIgnoreCase))
									{
										found = true;
										break;
									}
							}
							imgDir = dir;
						}
						else
							imageFiles = Directory.GetFiles(dir, "*.txt");

						string KBPrefix = "";
						for (int i = 0; i < imageFiles.Length; i++)
						{
							if (imageFiles[i].EndsWith(IMAGES_TXT, StringComparison.OrdinalIgnoreCase))
							{
								FileInfo f = new FileInfo(imageFiles[i]);
								filename = f.Name;
								KBPrefix = filename.Remove(filename.IndexOf(IMAGES_TXT, StringComparison.OrdinalIgnoreCase));
								Hashtable idNameMapping = new Hashtable();
								try
								{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
									using (FileStream fs = new FileStream(PathUtil.CompletePath(filename, dir), FileMode.Open, FileAccess.Read))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
									{
										using (StreamReader sr = new StreamReader(fs))
										{
											string line = sr.ReadLine();
											while (line != "[IdName]" && line != null)
												line = sr.ReadLine(); ;
											line = sr.ReadLine();
											while (line != "[Location]" && line != null)
											{
												string[] ls = line.Split('=');
												idNameMapping.Add(ls[0], ls[1]);
												line = sr.ReadLine();
											}
											while (line != "[Location]" && line != null)
												line = sr.ReadLine(); ;
											line = sr.ReadLine();
											while (line != null)
											{
												string[] parts = line.Split('=', ',');
												string imagePath = parts[4];
												string intExt = parts[3];
												if (intExt[0] != 'E' && intExt[0] != 'e' && !Path.IsPathRooted(imagePath) && !imagePath.ToLower().StartsWith("http:"))
													imagePath = imgDir + KBPrefix + "Resources/" + imagePath;
												string parts12 = '_' + parts[1] + '_' + parts[2];

												m_images[KBPrefix + parts[0] + parts12] = imagePath;
												string name;
												if ((name = (string)(idNameMapping[parts[0]])) != null)
													m_images[KBPrefix + name + parts12] = imagePath;

												if (parts.Length > 5 && !string.IsNullOrEmpty(parts[5]))
												{
													foreach (string density in parts[5].Split('|'))
													{
														if (!m_imagesDensity.ContainsKey(imagePath))
															m_imagesDensity[imagePath] = new HashSet<string>();
														m_imagesDensity[imagePath].Add(density.Substring(1, density.Length - 1));
													}
												}
												line = sr.ReadLine();
											}
										}
									}
								}
								catch (FileNotFoundException)
								{
									GXLogging.Debug(log, $"{filename} file not found");
								}
							}
						}

					}
				}
				return m_images;
			}
		}

		public string GetImagePath(string id, string KBId, string theme)
		{
			string lang = GetLanguage();
			string ret = (string)(Images[KBId + id + "_" + lang + "_" + theme]);
			if (ret != null)
				return ret;
			else
			{
				GXLogging.Debug(log, "Image not found at Images.txt. Image id:", () => KBId + id + " language:" + lang + " theme:" + theme);
				return id;
			}
		}
		public string GetImageSrcSet(string baseImage)
		{
			if (!String.IsNullOrEmpty(baseImage))
			{
				string key = baseImage;
				if (!String.IsNullOrEmpty(StaticContentBase) && baseImage.Contains(StaticContentBase))
				{
					key = baseImage.Substring(baseImage.LastIndexOf(StaticContentBase) + StaticContentBase.Length);
				}

				HashSet<string> densities;
				if (m_imagesDensity.TryGetValue(key, out densities))
				{
					string basePath = baseImage.Substring(0, baseImage.LastIndexOf(Path.GetFileName(baseImage)));
					List<string> srcSetList = new List<string>();
					foreach (string density in densities)
					{
						srcSetList.Add(string.Format("{0}{1}-{4}{2} {3}", basePath, Path.GetFileNameWithoutExtension(baseImage), Path.GetExtension(baseImage), density, density.Replace('.', '-')));
					}
					return string.Join(",", srcSetList.ToArray());
				}
			}
			return "";
		}

		public string GetTheme()
		{
			Hashtable cThemeMap = ReadSessionKey<Hashtable>(GXTheme);
			if (cThemeMap != null && cThemeMap.Contains(_theme))
				return (string)cThemeMap[_theme];
			else
				return _theme;
		}
		public int SetTheme(string t)
		{
			if (string.IsNullOrEmpty(t))
				return 0;
			else
			{
				Hashtable cThemeMap = ReadSessionKey<Hashtable>(GXTheme);
				if (cThemeMap == null)
					cThemeMap = new Hashtable();
				if (!cThemeMap.Contains(_theme))
					cThemeMap.Add(_theme, t);
				else
					cThemeMap[_theme] = t;
				return WriteSessionKey(GXTheme, cThemeMap) ? 1 : 0;
			}
		}
		public void SetDefaultTheme(string t)
		{
			_theme = t;
		}

		public string FileToBase64(string filePath)
		{
			if (!string.IsNullOrEmpty(filePath))
			{
				GxFile auxFile = new GxFile(GetPhysicalPath(), filePath, GxFileType.Private);
				return auxFile.ToBase64();
			}
			else
			{
				return string.Empty;
			}
		}
		public string FileFromBase64(string b64)
		{
			string filePath = FileUtil.getTempFileName(Preferences.getTMP_MEDIA_PATH());
			GxFile auxFile = new GxFile(GetPhysicalPath(), filePath, GxFileType.Private);
			auxFile.FromBase64(b64);
			GXFileWatcher.Instance.AddTemporaryFile(new GxFile("", new GxFileInfo(filePath, "")), this);
			return filePath;
		}

		public byte[] FileToByteArray(string filePath)
		{
			if (String.IsNullOrEmpty(filePath))
				return Array.Empty<byte>();
			GxFile auxFile = new GxFile(GetPhysicalPath(), filePath, GxFileType.Private);
			return auxFile.ToByteArray();
		}
		public string FileFromByteArray(byte[] bArray)
		{
			string filePath = FileUtil.getTempFileName(Preferences.getTMP_MEDIA_PATH());
			GxFile auxFile = new GxFile(GetPhysicalPath(), filePath, GxFileType.Private);
			auxFile.FromByteArray(bArray);
			GXFileWatcher.Instance.AddTemporaryFile(new GxFile("", new GxFileInfo(filePath, "")), this);
			return filePath;
		}

		public bool isCrawlerRequest()
		{
			return this.IsCrawlerRequest;
		}

		static DateTime startupDate = DateTime.MinValue;
		static public DateTime StartupDate
		{
			get
			{
				if (startupDate == DateTime.MinValue)
				{
					DateTime dt = DateTime.Now.ToUniversalTime();
					startupDate = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
				}
				return startupDate;
			}
		}

		public void SetSubmitInitialConfig(IGxContext context)
		{
			GXLogging.Debug(log, "SetSubmitInitialConfig:", () => _handle.ToString() + " clientid:" + context.ClientID);
			this._isSumbited = true;
			this.SetDefaultTheme(context.GetTheme());
			this.SetPhysicalPath(context.GetPhysicalPath());
			this.SetLanguageWithoutSession(context.GetLanguage());
			this.ClientID = context.ClientID;
			InitializeSubmitSession(context, this);
		}

		private static string[] copyKeys = { "GAMConCli", "GAMSession", "GAMError", "GAMErrorURL", "GAMRemote" };
		private static void InitializeSubmitSession(IGxContext oldContext, IGxContext newContext)
		{
			IGxSession parentSession = oldContext.GetSession();
			IGxSession newSession = newContext.GetSession();
			if (parentSession != null && newSession != null)
			{
				foreach (var item in copyKeys)
				{
					newSession.Set(item, parentSession.Get(item));
				}
			}
		}

		public bool WillRedirect()
		{
			return !String.IsNullOrEmpty(StringUtil.RTrim(wjLoc));
		}
#region IGxContext Members

		private const string CLIENT_ID_HEADER = "GX_CLIENT_ID";
		public string ClientID
		{
			get
			{
				if (string.IsNullOrEmpty(_clientId))
				{
					_clientId = this.GetCookie(CLIENT_ID_HEADER);
					if (string.IsNullOrEmpty(_clientId))
					{
						_clientId = Guid.NewGuid().ToString();
						this.SetCookie(CLIENT_ID_HEADER, _clientId, string.Empty, DateTime.MaxValue, string.Empty, GetHttpSecure());
					}
				}
				return _clientId;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					_clientId = value;
				}
			}
		}

		public GXSOAPContext SoapContext { get; set; }

#endregion
	}
	public class GxXmlContext
	{
		GXXMLReader _XMLR;
		GXXMLWriter _XMLW;
		string _basePath;

		public GxXmlContext(string basePath)
		{
			_basePath = basePath;
		}
		public GXXMLReader Reader
		{
			get
			{
				if (_XMLR == null)
					_XMLR = new GXXMLReader(_basePath);
				return _XMLR;
			}
		}
		public GXXMLWriter Writer
		{
			get
			{
				if (_XMLW == null)
					_XMLW = new GXXMLWriter(_basePath);
				return _XMLW;
			}
		}
	}
	public class GXSOAPContext
	{
		StringBuilder xml = new StringBuilder();
		StringWriter sWriter = null;
		GXXMLWriter xmlWriter = null;

		public void AppendXml(string xml)
		{
			if (sWriter == null)
				startMessage();
			xmlWriter.WriteRawText(xml);
		}
		public GXXMLWriter XmlWriter
		{
			get
			{
				if (sWriter == null)
					startMessage();
				return xmlWriter;
			}
		}
		void startMessage()
		{
			sWriter = new StringWriter();
			xmlWriter = new GXXMLWriter();
			xmlWriter.Open(sWriter);
		}
		public void EndMessage()
		{
			if (xmlWriter != null && sWriter != null)
			{
				// End message
				xmlWriter.Close();
				string sRet = sWriter.ToString();
				sWriter.Close();

				// Output
				if (sRet != null)
					xml.Append(sRet);

				// Reset
				xmlWriter = null;
				sWriter = null;
			}
		}
		public override string ToString()
		{
			return xml.ToString();
		}
	}
	public class GxNullContext : GxContext
	{
		Uri _baseUrl;
		public GxNullContext(ArrayList dataStores) : base(-1, dataStores, null)
		{
		}
		public override string BaseUrl
		{
			get { return _baseUrl.ToString(); }
			set { _baseUrl = new Uri(value); }
		}
		public override short GetHttpSecure()
		{
			return 0;
		}
		public override string GetServerName()
		{
			try
			{
				return _baseUrl.Host;
			}
			catch
			{
				return "";
			}
		}
		public override int GetServerPort()
		{
			try
			{
				return _baseUrl.Port;
			}
			catch
			{
				return 0;
			}

		}
		public override string GetScriptPath()
		{
			try
			{
				return _baseUrl.AbsolutePath + "/";
			}
			catch
			{
				return "";
			}
		}
	}

}
