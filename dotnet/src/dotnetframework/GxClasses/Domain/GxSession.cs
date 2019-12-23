using System;
using System.Collections;
using System.Web;
#if !NETCORE
using System.Web.SessionState;
#else
using Microsoft.AspNetCore.Http;
#endif
using System.Threading;
using GeneXus.Utils;
using GeneXus.Encryption;
using GeneXus.Application;
using log4net;


namespace GeneXus.Http
{
    public interface IGxSession
    {
        void Set(string key, string val);
		void SetObject(string key, Object val);
		string Get(string key);
		Object GetObject(string key);
		void Remove(string key);		
		void Destroy();
        void Clear();
        string Id
        {
            get;
        }

    }

    public class GxWebSession : IGxSession
    {
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Http.GxWebSession));
        private HttpSessionState _httpSession;

        public GxWebSession()
        {
        }
        public GxWebSession(IGxContext context)
        {
            if (context.HttpContext != null)
            {
#if NETCORE
				_httpSession = new HttpSessionState(context.HttpContext.Session);
#else
				_httpSession = context.HttpContext.Session;
#endif
			}
		}

        public string Id
        {
            get
            {
                if (_httpSession != null)
				{
					GXLogging.Debug(log, "SessionId : " + _httpSession.SessionID);
					return _httpSession.SessionID;
				}
                return string.Empty;
            }
        }

        public void Set(string key, string val)
        {
            SetObject(key, val);
        }
        public void SetObject(string key, object val)
        {
            key = GXUtil.NormalizeKey(key);
			GXLogging.Debug(log, "Set Key" + key + "=" + val);
			if (_httpSession != null)
			{
				GXLogging.Debug(log, "SetObject SessionId : " + _httpSession.SessionID);
				_httpSession[key] = val;
			}
        }
        public string Get(string key)
        {
            object value = GetObject(key);
			if (value != null)
			{
				GXLogging.Debug(log, "Get key: " + key + "=" + value.ToString());
				return value.ToString();
			}
			else
			{
				GXLogging.Debug(log, "Get key: " + key + " is Empty");
				return string.Empty;
			}
        }
		public object GetObject(string key)
        {
            key = GXUtil.NormalizeKey(key);
            if (_httpSession != null)
            {
				GXLogging.Debug(log, "GetObject SessionId : " + _httpSession.SessionID);
				if (_httpSession[key] == null)
                    return null;
                return _httpSession[key];
            }
            return null;
        }
        public void Remove(string key)
        {
            key = GXUtil.NormalizeKey(key);
			GXLogging.Debug(log, "Remove key: " + key );
			if (_httpSession != null)
			{
				GXLogging.Debug(log, "Remove SessionId : " + _httpSession.SessionID);
				_httpSession.Remove(key);
			}
        }
        public void Destroy()
        {
            if (_httpSession != null)
            {
				GXLogging.Debug(log, "Destroy sessionId: " + _httpSession.SessionID);
				_httpSession.RemoveAll();
                _httpSession.Abandon();
#if !NETCORE
				SessionIDManager manager = new SessionIDManager();
                string newId = manager.CreateSessionID(HttpContext.Current);
                bool isRedirected = false;
                bool isAdded = false;
                manager.SaveSessionID(HttpContext.Current, newId, out isRedirected, out isAdded);
#endif
            }
        }
        public void Clear()
        {
            if (_httpSession != null)
            {
				GXLogging.Debug(log, "Clear sessionId: " + _httpSession.SessionID);
				object navHelper = _httpSession[GxContext.GX_NAV_HELPER];
                string ajaxEncriptionKey = _httpSession[CryptoImpl.AJAX_ENCRYPTION_KEY] as string;
                _httpSession.Clear();
                _httpSession[CryptoImpl.AJAX_ENCRYPTION_KEY] = ajaxEncriptionKey;
                _httpSession[GxContext.GX_NAV_HELPER] = navHelper;
            }
        }
        public static bool IsSessionExpired(HttpContext httpContext)
        {
            if (httpContext.Session != null)
            {
				if (httpContext.IsNewSession())
                {
                    string CookieHeaders = httpContext.Request.Headers["Cookie"];

                    if ((null != CookieHeaders) && (CookieHeaders.IndexOf("ASP.NET_SessionId") >= 0))
                    {
                        // IsNewSession is true, but session cookie exists,
                        // so, ASP.NET session is expired
                        return true;
                    }
                }
            }
            return false;
        }

    }

    public class GxSession : IGxSession
    {
        private Hashtable sessionValues;
        private string _id;

        public GxSession()
        {
            initialize();
        }

        private void initialize()
        {
            sessionValues = new Hashtable();
            _id = Guid.NewGuid().ToString();
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

		private void PutHashValue(string key, Object val)
		{
			sessionValues[key] = val;
		}
		private void PutHashValue(string key, string val)
        {
            sessionValues[key] = val;
        }
        private string GetHashValue(string key)
        {
            object val = sessionValues[key];
            if (val != null)
                return val.ToString();
            return string.Empty;
        }
		private Object GetHashValueObj(string key)
		{
			return sessionValues[key];
		}
		private void RemoveHashValue(string key)
        {
            sessionValues.Remove(key);
        }
        private void DestroyHash()
        {
            ClearHash();
            initialize();
        }
        private void ClearHash()
        {
            sessionValues.Clear();
        }
        public void Set(string key, string val)
        {
            key = GXUtil.NormalizeKey(key);
            PutHashValue(key, val);
        }
		public void SetObject(string key, Object val)
		{
			key = GXUtil.NormalizeKey(key);
			PutHashValue(key, val);
		}
		public string Get(string key)
        {
            key = GXUtil.NormalizeKey(key);
            return GetHashValue(key);
        }
		public Object GetObject(string key)
		{
			key = GXUtil.NormalizeKey(key);
			return GetHashValueObj(key);
		}

        public void Remove(string key)
        {
            key = GXUtil.NormalizeKey(key);
            RemoveHashValue(key);
        }
        public void Destroy()
        {
            DestroyHash();
        }
        public void Clear()
        {
            ClearHash();
        }        
    }
}
