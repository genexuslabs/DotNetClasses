using System.Net.Http;
using System.Security;
using System.Web;
using System.Web.Helpers;
using GeneXus.Application;
using GeneXus.Utils;
using log4net;

namespace GeneXus.Http
{
	internal class CSRFHelper
	{
		//AntiForgeryConfig.AdditionalDataProvider = new SessionIdAntiforgeryAdditionalDataProvider();
		
		[SecuritySafeCritical]
		internal static void ValidateAntiforgery(HttpContext context)
		{
			if (RestAPIHelpers.ValidateCsrfToken())
			{
				ValidateAntiforgeryImpl(context);
			}
		}
		[SecurityCritical]
		static void ValidateAntiforgeryImpl(HttpContext context)
		{
			string cookieToken, formToken;
			string httpMethod = context.Request.HttpMethod;
			string tokens = context.Request.Cookies[HttpHeader.X_CSRF_TOKEN_COOKIE]?.Value;
			string internalCookieToken = context.Request.Cookies[HttpHeader.X_CSRF_TOKEN_COOKIE]?.Value;
			if (httpMethod == HttpMethod.Get.Method && (string.IsNullOrEmpty(tokens) || string.IsNullOrEmpty(internalCookieToken)))
			{
				AntiForgery.GetTokens(null, out cookieToken, out formToken);
#pragma warning disable SCS0009 // The cookie is missing security flag HttpOnly
				HttpCookie cookie = new HttpCookie(HttpHeader.X_CSRF_TOKEN_COOKIE, formToken)
				{
					HttpOnly = false,
					Secure = GxContext.GetHttpSecure(context) == 1,
				};
#pragma warning restore SCS0009 // The cookie is missing security flag HttpOnly
				HttpCookie internalCookie = new HttpCookie(AntiForgeryConfig.CookieName, cookieToken)
				{
					HttpOnly = true,
					Secure = GxContext.GetHttpSecure(context) == 1,
				};
				context.Response.SetCookie(cookie);
				context.Response.SetCookie(internalCookie);
			}
			if (httpMethod == HttpMethod.Delete.Method || httpMethod == HttpMethod.Post.Method || httpMethod == HttpMethod.Put.Method)
			{
				cookieToken = context.Request.Cookies[AntiForgeryConfig.CookieName]?.Value;
				string headerToken = context.Request.Headers[HttpHeader.X_CSRF_TOKEN_HEADER];
				AntiForgery.Validate(cookieToken, headerToken);
			}
		}
	}

	
	public class SessionIdAntiforgeryAdditionalDataProvider : IAntiForgeryAdditionalDataProvider
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(SessionIdAntiforgeryAdditionalDataProvider));
		public string GetAdditionalData(HttpContextBase context)
		{
			GXLogging.Debug(log, $"Setting session id as additional CSRF token data:", context.Session.SessionID);
			return context.Session.SessionID.Trim();
		}
		[SecuritySafeCritical]
		public bool ValidateAdditionalData(HttpContextBase context, string additionalData)
		{
			bool validSession = context.Session.SessionID.Trim().CompareTo(additionalData.Trim()) == 0 ? true : false;
			GXLogging.Warn(log, $"Session id in CSRF token ({additionalData}) does not match the current session id ({context.Session.SessionID})");
			return validSession;
		}
	}
}