using System.Net;
using System;
using System.Net.Http;
using System.Security;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using GeneXus.Application;
using GeneXus.Utils;

namespace GeneXus.Http
{
	internal class CSRFHelper
	{
		[SecuritySafeCritical]
		internal static bool HandleException(Exception e, HttpContext httpContext)
		{
			if (RestAPIHelpers.ValidateCsrfToken())
			{
				return HandleExceptionImp(e, httpContext);
			}
			return false;
		}
		[SecuritySafeCritical]
		private static bool HandleExceptionImp(Exception e, HttpContext httpContext)
		{
			if (e is HttpAntiForgeryException)
			{
				httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				httpContext.Response.StatusDescription = HttpHelper.InvalidCSRFToken;
				return true;
			}
			return false;
		}

		[SecuritySafeCritical]
		internal static void ValidateAntiforgery(HttpContext context)
		{
			if (RestAPIHelpers.ValidateCsrfToken())
			{
				ValidateAntiforgeryImp(context);
			}
		}
		[SecurityCritical]
		private static void ValidateAntiforgeryImp(HttpContext context)
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
}
