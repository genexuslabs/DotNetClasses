using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Utils;
using log4net;
#if NETCORE
using GxClasses.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using System.Net.Http;
#else
using System.ServiceModel.Web;
using System.ServiceModel;

#endif
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using GeneXus.Data;
using System.Runtime.Serialization;
using GeneXus.Mime;

namespace GeneXus.Http
{
	public enum GAMSecurityLevel
	{
		SecurityHigh = 2,
		SecurityLow = 1,
		SecurityNone = 0,
		SecurityObject = 3
	}

	public class HttpHeader
	{
		public static string AUTHENTICATE_HEADER = "WWW-Authenticate";
		public static string WARNING_HEADER = "Warning";
	}
	[DataContract()]
	public class HttpJsonError
	{
		[DataMember(Name = "code")]
		public string Code;
		[DataMember(Name = "message")]
		public string Message;
	}

	[DataContract()]
	public class WrappedJsonError
	{
		[DataMember(Name = "error")]
		public HttpJsonError Error;
	}

	public class HttpHelper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Http.HttpHelper));

		public static void SetResponseStatus(HttpContext httpContext, string statusCode, string statusDescription)
		{
#if !NETCORE
			var wcfcontext = WebOperationContext.Current;
			if (wcfcontext != null)
			{
				switch (statusCode)
				{
					case "400":
						wcfcontext.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
						break;
					case "404":
						wcfcontext.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
						break;
					case "409":
						wcfcontext.OutgoingResponse.StatusCode = HttpStatusCode.Conflict;
						break;
					case "103":
						wcfcontext.OutgoingResponse.StatusCode = HttpStatusCode.Forbidden;
						break;
					case "500":
						wcfcontext.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
						break;
					default:
						wcfcontext.OutgoingResponse.Headers.Add(HttpHeader.AUTHENTICATE_HEADER, OatuhUnauthorizedHeader(wcfcontext.IncomingRequest.Headers["Host"], statusCode, string.Empty));
						wcfcontext.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
						break;
				}
				wcfcontext.OutgoingResponse.StatusDescription = statusDescription.Replace(Environment.NewLine, string.Empty);
				GXLogging.Error(log, String.Format("ErrCode {0}, ErrDsc {1}", statusCode, statusDescription));
			}
			else
			{
#endif
				if (httpContext != null)
				{
					switch (statusCode)
					{
						case "400":
							httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
							break;
						case "404":
							httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
							break;
						case "409":
							httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
							break;
						case "103":
							httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
							break;
						case "500":
							httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
							break;
						default:
							httpContext.Response.Headers[HttpHeader.AUTHENTICATE_HEADER] = HttpHelper.OatuhUnauthorizedHeader(httpContext.Request.Headers["Host"], statusCode, string.Empty);
							httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
							break;
					}
#if !NETCORE
					httpContext.Response.StatusDescription =  statusDescription.Replace(Environment.NewLine, string.Empty);
					GXLogging.Error(log, String.Format("ErrCode {0}, ErrDsc {1}", statusCode, statusDescription));
				}
			}
#else
					httpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = statusDescription.Replace(Environment.NewLine, string.Empty);
					GXLogging.Error(log, String.Format("ErrCode {0}, ErrDsc {1}", statusCode, statusDescription));
				}

#endif
		}

		public static void SetResponseStatusAndJsonError(HttpContext httpContext, string statusCode, string statusDescription)
		{
			SetResponseStatus(httpContext, statusCode, statusDescription);
			if (httpContext != null)//<serviceHostingEnvironment aspNetCompatibilityEnabled="false" /> web.config
			{
				httpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
				var jsonError = new WrappedJsonError() { Error = new HttpJsonError() { Code = statusCode, Message = statusDescription } };
				httpContext.Response.Write(JSONHelper.Serialize(jsonError));
			}
#if !NETCORE
			else
			{
				var wcfcontext = WebOperationContext.Current;
				wcfcontext.OutgoingResponse.ContentType = MediaTypesNames.ApplicationJson;
				JsonFault jsonFault = new JsonFault(statusDescription, statusCode);
				throw new FaultException<JsonFault>(jsonFault, new FaultReason(statusDescription));
			}
#endif
		}
		public static Task SetResponseStatusAndJsonErrorAsync(HttpContext httpContext, string statusCode, string statusDescription)
		{
			SetResponseStatus(httpContext, statusCode, statusDescription);
			httpContext.Response.ContentType = MediaTypesNames.ApplicationJson;
			var jsonError = new WrappedJsonError() { Error = new HttpJsonError() { Code = statusCode, Message = statusDescription } };
			httpContext.Response.Write(JSONHelper.Serialize(jsonError));
			return Task.CompletedTask;
		}

		public static String OatuhUnauthorizedHeader(string realm, string errCode, string errDescription)
		{
			if (string.IsNullOrEmpty(errDescription))
				return String.Format("OAuth realm=\"{0}\"", realm);
			else
				return string.Format("OAuth realm=\"{0}\",error_code=\"{1}\",error_description=\"{2}\"", realm, errCode, errDescription);
		}
		public static string GetHttpRequestPostedFileType(HttpContext httpContext, string varName)
		{
			try
			{
				var pf = GetFormFile(httpContext, varName);
				if (pf != null)
					return FileUtil.GetFileType(pf.FileName);
			}
			catch { }
			return string.Empty;
		}
#if NETCORE
		public static IFormFile GetFormFile(HttpContext httpContext, String varName)
		{
			return httpContext.Request.Form.Files[varName];
		}
#else
		public static HttpPostedFile GetFormFile(HttpContext httpContext, String varName)
		{
			return httpContext.Request.Files[varName];
		}
#endif

		public static string GetHttpRequestPostedFileName(HttpContext httpContext, string varName)
		{
			try
			{
				var pf = GetFormFile(httpContext, varName);
				if (pf != null)
					return FileUtil.GetFileName(pf.FileName);
			}
			catch { }
			return string.Empty;
		}

		public static bool GetHttpRequestPostedFile(HttpContext httpContext, string varName, out string filePath)
		{
			filePath = null;
			if (httpContext != null)
			{
				var pf = GetFormFile(httpContext, varName);
				if (pf != null)
				{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					FileInfo fi = new FileInfo(pf.FileName);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					string tempDir = Preferences.getTMP_MEDIA_PATH();
					string ext = fi.Extension;
					if (ext != null)
						ext = ext.TrimStart('.');
					filePath = FileUtil.getTempFileName(tempDir, "BLOB", ext);
					GXLogging.Debug(log, "cgiGet(" + varName + "), fileName:" + filePath);
					GxFile file = new GxFile(tempDir, filePath);
#if NETCORE
					filePath = file.Create(pf.OpenReadStream());
#else
					filePath = file.Create(pf.InputStream);
#endif
					GXFileWatcher.Instance.AddTemporaryFile(file);
					return true;
				}
			}
			return false;
		}
		public static string RequestPhysicalApplicationPath(HttpContext context = null)
		{
#if NETCORE
			return Directory.GetParent(Directory.GetParent(FileUtil.GetStartupDirectory()).FullName).FullName;
#else
			if (context==null)
				return HttpContext.Current.Request.PhysicalApplicationPath; 
			else
				return context.Request.PhysicalApplicationPath; 
#endif
		}

#if NETCORE
		public static byte[] DownloadFile(string url, out HttpStatusCode statusCode)
		{
			var buffer = Array.Empty<byte>();
			using (var client = new HttpClient())
			{
				using (HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result)
				{
					if (response.IsSuccessStatusCode)
					{
						statusCode = HttpStatusCode.OK;
						using (Stream contentStream = response.Content.ReadAsStreamAsync().Result)
						{
							buffer = new byte[8192];
							var isMoreToRead = true;
							do
							{
								var read = contentStream.ReadAsync(buffer, 0, buffer.Length).Result;
								if (read == 0)
								{
									isMoreToRead = false;
								}
							}
							while (isMoreToRead);
						}
					}
					else
					{
						statusCode = response.StatusCode;
					}
				}
			}
			return buffer;
		}

#else
		internal static byte[] DownloadFile(string fileName, out HttpStatusCode statusCode)
		{
			byte[] binary = Array.Empty<byte>();
			try
			{
				WebClient Client = new WebClient();
				binary = Client.DownloadData(fileName);
				statusCode = HttpStatusCode.OK;
			}
			catch (WebException e) //An error occurred while downloading data.           
			{
				if (e.Response != null)
				{
					statusCode = ((HttpWebResponse)e.Response).StatusCode;
				}
				else
				{
					statusCode = HttpStatusCode.InternalServerError;
				}
				GXLogging.Error(log, "An error occurred while downloading data from url " + fileName + " " + e.Message, e);
			}
			return binary;
		}
#endif
		static bool NamedParametersQuery(string query)
		{
			return Preferences.UseNamedParameters && query.Contains("=");
		}
		public static string[] GetParameterValues(string query)
		{
			if (NamedParametersQuery(query))
			{
				NameValueCollection names = HttpUtility.ParseQueryString(query);
				string[] values = new string[names.Count];
				for (int i = 0; i < names.Count; i++)
					values[i] = names[i];

				return values;
			}
			else
			{
				return query.Split(',');
			}
		}

	}
#if NETCORE
	public class HttpCookieCollection : Dictionary<string, HttpCookie>
	{
		public new HttpCookie this[string key] {
			get {
				if (this.ContainsKey(key))
					return base[key];
				else
					return null;
			}
			set
			{
				base[key] = value;
			}
		}
	}
	public sealed class HttpCookie
	{
		//
		// Summary:
		//     Creates and names a new cookie.
		//
		// Parameters:
		//   name:
		//     The name of the new cookie.
		public HttpCookie(string name){
			Name = name;
		}

		//
		// Summary:
		//     Creates, names, and assigns a value to a new cookie.
		//
		// Parameters:
		//   name:
		//     The name of the new cookie.
		//
		//   value:
		//     The value of the new cookie.
		public HttpCookie(string name, string value)
		{
			Name = name;
			Value = value;
		}

		
		//
		// Summary:
		//     Gets or sets the domain to associate the cookie with.
		//
		// Returns:
		//     The name of the domain to associate the cookie with. The default value is the
		//     current domain.
		public string Domain { get; set; }
		//
		// Summary:
		//     Gets or sets the expiration date and time for the cookie.
		//
		// Returns:
		//     The time of day (on the client) at which the cookie expires.
		public DateTime Expires { get; set; }
		//
		// Summary:
		//     Gets a value indicating whether a cookie has subkeys.
		//
		// Returns:
		//     true if the cookie has subkeys, otherwise, false. The default value is false.
		public bool HasKeys { get; }
		//
		// Summary:
		//     Gets or sets a value that specifies whether a cookie is accessible by client-side
		//     script.
		//
		// Returns:
		//     true if the cookie has the HttpOnly attribute and cannot be accessed through
		//     a client-side script; otherwise, false. The default is false.
		public bool HttpOnly { get; set; }
		//
		// Summary:
		//     Gets or sets the name of a cookie.
		//
		// Returns:
		//     The default value is a null reference (Nothing in Visual Basic) unless the constructor
		//     specifies otherwise.
		public string Name { get; set; }
		//
		// Summary:
		//     Gets or sets the virtual path to transmit with the current cookie.
		//
		// Returns:
		//     The virtual path to transmit with the cookie. The default is /, which is the
		//     server root.
		public string Path { get; set; }
		//
		// Summary:
		//     Gets or sets a value indicating whether to transmit the cookie using Secure Sockets
		//     Layer (SSL)--that is, over HTTPS only.
		//
		// Returns:
		//     true to transmit the cookie over an SSL connection (HTTPS); otherwise, false.
		//     The default value is false.
		public bool Secure { get; set; }
		//
		// Summary:
		//     Gets or sets an individual cookie value.
		//
		// Returns:
		//     The value of the cookie. The default value is a null reference (Nothing in Visual
		//     Basic).
		public string Value { get; set; }
		
	}
	public static class HttpResponseExtensions
	{
		public static void AppendHeader(this HttpResponse response, string name, string value) {
			if (!response.HasStarted)
				response.Headers[name] = value;
		}
		public static void AddHeader(this HttpResponse response, string name, string value)
		{
			if (!response.HasStarted)
				response.Headers[name] = value;
		}

		public static void Write(this HttpResponse response, string value)
		{
			response.WriteAsync(value).Wait();
		}
		public static void WriteFile(this HttpResponse response, string fileName)
		{
			response.SendFileAsync(fileName).Wait();
		}
		
	}
#endif
	public static class HttpWebRequestExtensions
	{
		public static void SetReferer(this HttpWebRequest request, string referer)
		{
#if NETCORE
			request.Headers["Referer"] = referer;
#else
			request.Referer = referer;
#endif
		}
		public static void SetUserAgent(this HttpWebRequest request, string userAgent)
		{
#if NETCORE
			request.Headers["User-agent"] = userAgent;
#else
			request.UserAgent = userAgent;
#endif
		}
		public static void SetExpect(this HttpWebRequest request, string expect)
		{
#if NETCORE
			request.Headers["Expect"] = expect;
#else
			request.Expect = expect;
#endif
		}

		public static void SetKeepAlive(this HttpWebRequest request, bool keepAlive)
		{
#if NETCORE
			if (keepAlive)
				request.Headers["Connection"] = "Keep-Alive";
			else
				request.Headers["Connection"] = "Close";
#else
			request.KeepAlive = keepAlive;
#endif
		}

	}
	public static class HttpContextExtensions
	{
#if NETCORE
		static string NEWSESSION = "gxnewSession";
		public static void NewSessionCheck(this HttpContext context)
		{
			if (string.IsNullOrEmpty(context.Session.GetString(NEWSESSION)))
			{
				context.Session.SetString(NEWSESSION, true.ToString());
			}
			else
			{
				context.Session.SetString(NEWSESSION, false.ToString());
			}
		}
		public static bool IsNewSession(this HttpContext context)
		{
			return string.IsNullOrEmpty(context.Session.GetString(NEWSESSION)) || context.Session.GetString(NEWSESSION) == true.ToString();
		}
#else
		public static bool IsNewSession(this HttpContext context)
		{
			return context.Session.IsNewSession;
		}
#endif
	}

	public static class HttpRequestExtensions
	{
		public static HttpCookieCollection GetCookies(this HttpRequest request)
		{
#if NETCORE
			HttpCookieCollection cookieColl = new HttpCookieCollection();
			foreach (var v in request.Cookies)
				cookieColl[v.Key] = new HttpCookie(v.Key, v.Value);
			return cookieColl;
#else
			return request.Cookies;
#endif
		}
		public static HttpPostedFile GetFile(this HttpRequest request, string name)
		{
#if NETCORE
			if (MultipartRequestHelper.IsMultipartContentType(request.ContentType))
			{
				IFormFile file = request.Form.Files.GetFile(name);
				if (file != null)
					return new HttpPostedFile(file);
			}
			return null;
#else
			return request.Files[name];
#endif
		}
		public static HttpPostedFile GetFile(this HttpRequest request, int idx)
		{
#if NETCORE
			if (MultipartRequestHelper.IsMultipartContentType(request.ContentType))
			{
				if (idx < request.Form.Files.Count)
				{
					IFormFile file = request.Form.Files[idx];
					if (file != null)
						return new HttpPostedFile(file);
				}
			}
			return null;
#else
			return request.Files[idx];
#endif
		}
		public static int GetFileCount(this HttpRequest request)
		{
#if NETCORE
			if (MultipartRequestHelper.IsMultipartContentType(request.ContentType))
			{
				if (request.Form.Files != null)
					return request.Form.Files.Count();
			}
			return 0;
#else
			return request.Files.Count;
#endif
		}
		public static NameValueCollection GetQueryString(this HttpRequest request)
		{
#if NETCORE
			NameValueCollection paramValues = new NameValueCollection();

			foreach (var key in request.Query.Keys)
			{
				paramValues.Add(key, request.Query[key].ToString());
			}
			return paramValues;
#else
			return request.QueryString;
#endif
		}
		public static NameValueCollection GetParams(this HttpRequest request)
		{
#if NETCORE
			//Name - value pairs are added to the collection in the following order:
			//		Query - string parameters.
			//		Form fields.
			//		Cookies.
			//		Server variables.
			NameValueCollection paramValues = request.GetQueryString();
			try
			{
				foreach (var key in request.Form.Keys)
				{
					paramValues.Add(key, request.Form[key].ToString());
				}
			}
			catch (InvalidOperationException) {
				//The Form property is populated when the HTTP request Content-Type value is either "application/x-www-form-urlencoded" or "multipart/form-data".
			}
			foreach (var key in request.Cookies.Keys)
			{
				paramValues.Add(key, request.Cookies[key]);
			}
			return paramValues;
#else
			return request.Params;
#endif
		}


		public static string GetRawUrl(this HttpRequest request)
		{
#if NETCORE
			var httpContext = request.HttpContext;
			var requestFeature = httpContext.Features.Get<IHttpRequestFeature>();
			return requestFeature.RawTarget;
#else
			return request.RawUrl;
#endif
		}

		public static short GetIsSecureConnection(this HttpRequest request)
		{
#if NETCORE
			return (short)(request.IsHttps ? 1 : 0);
#else
			return (short)(request.IsSecureConnection ? 1:0);
#endif
		}
		public static string GetScheme(this HttpRequest request)
		{
#if NETCORE
			return request.Scheme;
#else
			return request.Url.Scheme;
#endif
		}
		public static int GetPort(this HttpRequest request)
		{
#if NETCORE
			if (request.Host.Port.HasValue)
				return request.Host.Port.Value;
			else
				return 0;
#else
			return request.Url.Port;

#endif
		}
		public static string GetHost(this HttpRequest request)
		{
#if NETCORE
			return request.Host.Host;
#else
			return request.Url.Host;

#endif
		}
		public static string GetUserAgent(this HttpRequest request)
		{
#if NETCORE
			return request.Headers["User-Agent"].ToString();
#else
			return request.UserAgent;
#endif
		}
		public static string GetAbsoluteUri(this HttpRequest request)
		{
#if NETCORE
			return request.GetEncodedUrl();
#else
			return request.Url.AbsoluteUri;
#endif
		}
		public static string GetMethod(this HttpRequest request)
		{
#if NETCORE
			return request.Method;
#else
			return request.HttpMethod;
#endif
		}
		public static string GetQuery(this HttpRequest request)
		{
#if NETCORE
			return request.QueryString.Value;
#else
			return request.Url.Query;
#endif
		}

		public static string GetAbsolutePath(this HttpRequest request)
		{
#if NETCORE
			if (request.PathBase!=null && request.PathBase.HasValue)
				return request.PathBase.Value + request.Path.Value;
			else
				return request.Path.Value;
#else
			return request.Url.AbsolutePath;
#endif
		}
		public static string GetFilePath(this HttpRequest request)
		{
#if NETCORE
			var basePath = string.Empty;
			if (request.PathBase.HasValue)
				basePath = request.PathBase.Value;
			if (request.Path.HasValue)
			{
				return basePath + request.Path.Value;
			}
			else
			{
				return string.Empty;
			}
#else
			return request.FilePath;
#endif
		}

		public static string GetApplicationPath(this HttpRequest request)
		{
#if NETCORE
			if (request.PathBase.HasValue)
				return request.PathBase.Value;
			else
				if (request.Path.HasValue && request.Path.Value.LastIndexOf('/') > 0)
				return request.Path.Value.Substring(0, request.Path.Value.LastIndexOf('/'));
			else
				return string.Empty;
#else
			return request.ApplicationPath;
#endif
		}
		public static Uri GetUrlReferrer(this HttpRequest request)
		{
#if NETCORE
			StringValues referer = request.Headers["Referer"];
			if (referer.Count > 0)
				return new UriBuilder(request.Headers["Referer"].ToString()).Uri;
			else
				return null;
#else
			return request.UrlReferrer;
#endif
		}

		public static Stream GetInputStream(this HttpRequest request)
		{
#if NETCORE
			return request.Body;
#else
			return request.InputStream;
#endif
		}


	}
}
