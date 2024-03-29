

using System;
using System.Threading.Tasks;
using GeneXus.Configuration;
using GeneXus.Http;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
namespace GeneXus.Application
{
	public class ValidateAntiForgeryTokenMiddleware
	{
		static readonly IGXLogger log = GXLoggerFactory.GetLogger<ValidateAntiForgeryTokenMiddleware>();
		private readonly RequestDelegate _next;
		private readonly IAntiforgery _antiforgery;
		private string _restBasePath;
		private string _basePath;

		public ValidateAntiForgeryTokenMiddleware(RequestDelegate next, IAntiforgery antiforgery, string basePath)
		{
			_next = next;
			_antiforgery = antiforgery;
			_restBasePath = $"{basePath}{Startup.REST_BASE_URL}";
			_basePath = $"/{basePath}";
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
			if (!IsVerificationTokenServiceRequest(context)) 
				await _next(context);
		}
		private bool IsVerificationTokenServiceRequest(HttpContext context)
		{
			return context.Request.Path.Value.EndsWith(_restBasePath);
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
}
