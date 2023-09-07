

using System;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Http;
using log4net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
namespace GeneXus.Application
{
	public class ValidateAntiForgeryTokenMiddleware
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(ValidateAntiForgeryTokenMiddleware));

		private readonly RequestDelegate _next;
		private readonly IAntiforgery _antiforgery;
		private string _basePath;

		public ValidateAntiForgeryTokenMiddleware(RequestDelegate next, IAntiforgery antiforgery, String basePath)
		{
			_next = next;
			_antiforgery = antiforgery;
			_basePath = "/" + basePath;
		}

		public async Task Invoke(HttpContext context)
		{
			if (context.Request.Path.HasValue && context.Request.Path.Value.StartsWith(_basePath))
			{
				if (HttpMethods.IsPost(context.Request.Method) ||
				HttpMethods.IsDelete(context.Request.Method) ||
				HttpMethods.IsPut(context.Request.Method))
				{
					string cookieToken = context.Request.Cookies[HttpHeader.X_CSRF_TOKEN_COOKIE];
					string headerToken = context.Request.Headers[HttpHeader.X_CSRF_TOKEN_HEADER];
					GXLogging.Debug(log, $"Antiforgery validation, cookieToken:{cookieToken}, headerToken:{headerToken}");

					await _antiforgery.ValidateRequestAsync(context);
					GXLogging.Debug(log, $"Antiforgery validation OK");
				}
				else if (HttpMethods.IsGet(context.Request.Method))
				{
					SetAntiForgeryTokens(_antiforgery, context);
				}
			}
			if (!context.Request.Path.Value.EndsWith(_basePath)) //VerificationToken
				await _next(context);
		}
		internal static void SetAntiForgeryTokens(IAntiforgery _antiforgery, HttpContext context)
		{
			AntiforgeryTokenSet tokenSet = _antiforgery.GetAndStoreTokens(context);
			string sameSite;
			CookieOptions cookieOptions = new CookieOptions { HttpOnly = false, Secure = GxContext.GetHttpSecure(context) == 1 };
			SameSiteMode sameSiteMode = SameSiteMode.Unspecified;
			if (Config.GetValueOf("SAMESITE_COOKIE", out sameSite) && Enum.TryParse(sameSite, out sameSiteMode))
			{
				cookieOptions.SameSite = sameSiteMode;
			}
			context.Response.Cookies.Append(HttpHeader.X_CSRF_TOKEN_COOKIE, tokenSet.RequestToken, cookieOptions);
			GXLogging.Debug(log, $"Setting cookie ", HttpHeader.X_CSRF_TOKEN_COOKIE, "=", tokenSet.RequestToken, " samesite:" + sameSiteMode);
		}

	}
	public class SessionIdAntiforgeryAdditionalDataProvider : IAntiforgeryAdditionalDataProvider
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(SessionIdAntiforgeryAdditionalDataProvider));
		public string GetAdditionalData(HttpContext context)
		{
			context.NewSessionCheck();
			GXLogging.Debug(log, $"Setting session id as additional CSRF token data:", context.Session.Id);
			return context.Session.Id.Trim();
		}

		public bool ValidateAdditionalData(HttpContext context, string additionalData)
		{
			bool validSession = context.Session.Id.Trim().CompareTo(additionalData.Trim()) == 0 ? true : false;
			GXLogging.Warn(log, $"Session id in CSRF token ({additionalData}) does not match the current session id ({context.Session.Id})");
			return validSession;
		}
	}

}
