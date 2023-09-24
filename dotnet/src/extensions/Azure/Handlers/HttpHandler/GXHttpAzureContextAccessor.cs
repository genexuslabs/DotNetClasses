using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Cache;
using GeneXus.Utils;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Primitives;

namespace GeneXus.Deploy.AzureFunctions.HttpHandler
{
	public class GXHttpAzureContextAccessor : HttpContext
	{
		DefaultHttpContext defaultHttpContext = new DefaultHttpContext();
		public HttpResponse httpResponseData;
		private ICacheService2 _redis;
		private string sessionId;
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<GXHttpAzureContextAccessor>();
		internal const string AzureSessionId = "GX_AZURE_SESSIONID";
		public GXHttpAzureContextAccessor(HttpRequestData requestData, HttpResponseData responseData, ICacheService2 redis)
		{
			if (redis != null)
				_redis = redis;

			bool isSecure = false;
			foreach (var header in requestData.Headers)
			{
				string[] values = new Microsoft.Extensions.Primitives.StringValues(header.Value.Select(val => val).ToArray());
				defaultHttpContext.Request.Headers[header.Key] = new Microsoft.Extensions.Primitives.StringValues(values);

				if (header.Key == "Cookie")
				{
					sessionId = CookieValue(defaultHttpContext.Request.Headers[header.Key], AzureSessionId);
				}

				if (!isSecure)
					isSecure = GetSecureConnection(header.Key, defaultHttpContext.Request.Headers[header.Key]);

			}
			if (requestData.FunctionContext.BindingContext != null)
			{
				IReadOnlyDictionary<string, object> keyValuePairs = requestData.FunctionContext.BindingContext.BindingData;
				object queryparamsJson = requestData.FunctionContext.BindingContext.BindingData.GetValueOrDefault("Query");
				JsonNode queryparams = JsonNode.Parse((string)queryparamsJson);

				foreach (var keyValuePair in keyValuePairs)
				{
					if ((keyValuePair.Key != "Headers") && (keyValuePair.Key != "Query"))
					{
						JsonNode qKey = queryparams[keyValuePair.Key];
						if (qKey == null)
							defaultHttpContext.Request.RouteValues.Add(keyValuePair.Key.ToLower(), keyValuePair.Value);
					}
				}
			}

			defaultHttpContext.Request.Method = requestData.Method;
			defaultHttpContext.Request.Body = requestData.Body;
			defaultHttpContext.Request.Path = PathString.FromUriComponent(requestData.Url);
			defaultHttpContext.Request.QueryString = QueryString.FromUriComponent(requestData.Url);


			IHttpRequestFeature requestFeature = defaultHttpContext.Features.Get<IHttpRequestFeature>();
			requestFeature.RawTarget = defaultHttpContext.Request.Path.HasValue ? defaultHttpContext.Request.Path.Value : String.Empty;
			defaultHttpContext.Features.Set<IHttpRequestFeature>(requestFeature);

			if (string.IsNullOrEmpty(sessionId))
			{
				CreateSessionId(isSecure, responseData, requestData);
			}
			else //Refresh the session timestamp
			{
				if (Session is RedisHttpSession)
				{
					RedisHttpSession redisHttpSession = (RedisHttpSession)Session;
					//Check if session is in cache
					if (redisHttpSession.SessionKeyExists(sessionId))
					{
						bool success = redisHttpSession.RefreshSession(sessionId);
						if (!success)
							GXLogging.Debug(log, $"Azure Serverless: Session could not be refreshed :{sessionId}");
					}
				}
			}

			httpResponseData = new GxHttpAzureResponse(defaultHttpContext, responseData);
		}
		private bool GetSecureConnection(string headerKey, string headerValue)
		{
			if ((headerKey == "Front-End-Https") & (headerValue == "on"))
				return true;

			if ((headerKey == "X-Forwarded-Proto") & (headerValue == "https"))
				return true;

			return false;
		}
		private void CreateSessionId(bool isSecure, HttpResponseData responseData, HttpRequestData requestData)
		{
			sessionId = Guid.NewGuid().ToString();
			HttpCookie sessionCookie = new HttpCookie(AzureSessionId, sessionId);

			if (!isSecure)
				isSecure = requestData.Url.Scheme == "https";

			if (!DateTime.MinValue.Equals(DateTimeUtil.NullDate()))
				sessionCookie.Expires = DateTime.MinValue;
			sessionCookie.Path = "";
			sessionCookie.Domain = "";
			sessionCookie.HttpOnly = true;
			sessionCookie.Secure = isSecure;

			if (responseData.Cookies != null)
				responseData.Cookies.Append(sessionCookie);
			GXLogging.Debug(log, $"Create new Azure Session Id :{sessionId}");
		}
		private string CookieValue(string header, string name)
		{
			string[] words = header.Split(';');

			foreach (string word in words)
			{
				string[] parts = word.Split('=');
				if (parts[0].Trim() == name)
					return parts[1];
			}
			return string.Empty;
		}
		public override IFeatureCollection Features => defaultHttpContext.Features;

		public override HttpRequest Request => defaultHttpContext.Request;

		public override HttpResponse Response => httpResponseData;

		public override ConnectionInfo Connection => defaultHttpContext.Connection;

		public override WebSocketManager WebSockets => defaultHttpContext.WebSockets;

		public override ClaimsPrincipal User { get => defaultHttpContext.User; set => defaultHttpContext.User = value; }
		public override IDictionary<object, object> Items { get => defaultHttpContext.Items; set => defaultHttpContext.Items = value; }
		public override IServiceProvider RequestServices { get => defaultHttpContext.RequestServices; set => defaultHttpContext.RequestServices = value; }
		public override CancellationToken RequestAborted { get => defaultHttpContext.RequestAborted; set => defaultHttpContext.RequestAborted = value; }
		public override string TraceIdentifier { get => defaultHttpContext.TraceIdentifier; set => defaultHttpContext.TraceIdentifier = value; }
		public override ISession Session {

			get
			{
				if ((_redis != null) & (sessionId != null))
					return new RedisHttpSession(_redis, sessionId);
				else return new MockHttpSession();
			}

			set => defaultHttpContext.Session = value; }
		public override void Abort()
		{
			//throw new NotImplementedException();
		}
	}
	internal class GxAzureResponseHeaders : IHeaderDictionary
	{
		HeaderDictionary m_headers;
		HttpResponseData m_httpResponseData;
		internal GxAzureResponseHeaders(HttpResponseData httpResponseData)
		{
			m_headers = new HeaderDictionary();
			foreach (var header in httpResponseData.Headers)
			{
				string[] values = new Microsoft.Extensions.Primitives.StringValues(header.Value.Select(val => val).ToArray());
				m_headers.Add(header.Key, values);
			}
			m_httpResponseData = httpResponseData;
		}

		public StringValues this[string key]
		{
			get
			{
				return m_headers[key];
			}
			set
			{
				m_httpResponseData.Headers.Add(key, value.AsEnumerable());
				m_headers[key] = value;
			}
		}

		public long? ContentLength { get { return m_headers.ContentLength; } set {; } }

		public ICollection<string> Keys { get { return m_headers.Keys; } }
		public ICollection<StringValues> Values { get { return m_headers.Values; } }

		public int Count { get { return m_headers.Count; } }

		public bool IsReadOnly { get { return m_headers.IsReadOnly; } }

		public void Add(string key, StringValues value)
		{
			m_httpResponseData.Headers.Add(key, value.AsEnumerable());
			m_headers.Add(key, value);
		}

		public void Add(KeyValuePair<string, StringValues> item)
		{
			m_httpResponseData.Headers.Add(item.Key, item.Value.AsEnumerable());
			m_headers.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			m_httpResponseData.Headers.Clear();
			m_headers.Clear();
		}

		public bool Contains(KeyValuePair<string, StringValues> item)
		{
			return m_headers.Contains(item);
		}

		public bool ContainsKey(string key)
		{
			return m_headers.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex)
		{
			m_headers.CopyTo(array, arrayIndex);
		}

		public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
		{
			return m_headers.GetEnumerator();
		}

		public bool Remove(string key)
		{
			m_httpResponseData.Headers.Remove(key);
			return m_headers.Remove(key);
		}

		public bool Remove(KeyValuePair<string, StringValues> item)
		{
			m_httpResponseData.Headers.Remove(item.Key);
			return m_headers.Remove(item);
		}

		public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value)
		{
			return m_headers.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_headers.GetEnumerator();
		}
	}

	public class GxHttpAzureResponse : HttpResponse
	{
		HttpResponseData httpResponseData;
		HttpContext httpContext;

		private FeatureReferences<FeatureInterfaces> _features;

		private readonly static Func<IFeatureCollection, IHttpResponseFeature> _nullResponseFeature = f => null;
		private readonly static Func<IFeatureCollection, IHttpResponseBodyFeature> _nullResponseBodyFeature = f => null;
		private readonly static Func<IFeatureCollection, IResponseCookiesFeature> _newResponseCookiesFeature = f => new ResponseCookiesFeature(f);

		struct FeatureInterfaces
		{
			public IHttpResponseFeature Response;
			public IHttpResponseBodyFeature ResponseBody;
			public IResponseCookiesFeature Cookies;
		}
		public void Initialize()
		{
			_features.Initalize(httpContext.Features);
		}
		public void Initialize(int revision)
		{
			_features.Initalize(httpContext.Features, revision);
		}
		
		private IHttpResponseBodyFeature HttpResponseBodyFeature =>
		   _features.Fetch(ref _features.Cache.ResponseBody, _nullResponseBodyFeature);

		private IResponseCookiesFeature ResponseCookiesFeature =>
			_features.Fetch(ref _features.Cache.Cookies, _newResponseCookiesFeature);
		private IHttpResponseFeature HttpResponseFeature =>
		   _features.Fetch(ref _features.Cache.Response, _nullResponseFeature);

		public GxHttpAzureResponse(HttpContext context, HttpResponseData responseData)
		{
			httpResponseData = responseData;
			httpContext = context;
			_features.Initalize(context.Features);
		}
		public override HttpContext HttpContext => httpContext;

		public override int StatusCode { get => (int)httpResponseData.StatusCode; set => httpResponseData.StatusCode = (System.Net.HttpStatusCode)value; }

		public override IHeaderDictionary Headers
		{
			get 
			{
				return new GxAzureResponseHeaders(httpResponseData);
			}
		}
		public override Stream Body { get => httpResponseData.Body; set => httpResponseData.Body = value; }	
		public override long? ContentLength {get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public override string ContentType
		{
			get
			{
				var headers = from head in httpResponseData.Headers
							 where head.Key == "Content-Type"
							 select head;				
				foreach (var header in headers)
				{
					string[] values = new Microsoft.Extensions.Primitives.StringValues(header.Value.Select(val => val).ToArray());
					return (values.First());
				}
				return ("application/json");
			}

			set
			{
				if (!string.IsNullOrEmpty(ContentType))
					httpResponseData.Headers.Remove("Content-Type");
				httpResponseData.Headers.Add("Content-Type", value);
			}
		}
		public override IResponseCookies Cookies
		{
			get { return ResponseCookiesFeature.Cookies; }

		} 

		public override bool HasStarted
		{
			get { return HttpResponseFeature.HasStarted; }
		}

		public override void OnCompleted(Func<object, Task> callback, object state)
		{
			//throw new NotImplementedException();
		}
		public override void OnStarting(Func<object, Task> callback, object state)
		{
			//throw new NotImplementedException();
		}

		public override void Redirect(string location, bool permanent)
		{
			//throw new NotImplementedException();
		}
		public override PipeWriter BodyWriter
		{
			get
			{
				return (PipeWriter.Create(Body));		
			}
		}
	}
}
