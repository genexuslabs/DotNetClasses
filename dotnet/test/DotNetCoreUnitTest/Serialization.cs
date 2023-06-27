using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Encryption;
using GeneXus.Http;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace xUnitTesting
{
	public class CoreSerializationTest
	{

		[Fact]
		public async Task TestSessionRenew()
		{
			var httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
			var authMiddleware = new MyAuthMiddleware();

			await authMiddleware.Invoke(httpContext);
		}
		[Fact]
		public async Task TestSessionCookieContainer()
		{
			DefaultHttpContext httpContext = new DefaultHttpContext() { Session = new MockHttpSession() };
			MyAuthMiddleware authMiddleware = new MyAuthMiddleware();

			await authMiddleware.SessionCookieContainerSerialization(httpContext);
		}

	}
	public class MyAuthMiddleware
	{
		public MyAuthMiddleware()
		{
		}
		public async Task SessionCookieContainerSerialization(HttpContext context)
		{
			GxContext gxcontext = new GxContext();
			gxcontext.HttpContext = context;
			string url = "https://test.net";
			CookieContainer container = gxcontext.GetCookieContainer(url, true);
			Cookie cookie = new Cookie("ROUTEID", ".http3");
			cookie.Path = "/";
			cookie.Expires = DateTime.MinValue;
			cookie.Domain = "test.net";
			container.Add(cookie);
			gxcontext.UpdateSessionCookieContainer();
			container = gxcontext.GetCookieContainer(url, true);
			Assert.Equal(1, container.Count);
			await Task.CompletedTask;
		}

		public async Task Invoke(HttpContext context)
		{
			GxContext gxcontext = new GxContext();
			gxcontext.HttpContext = context;

			GxWebSession websession = new GxWebSession(gxcontext);
			string InternalKeyAjaxEncryptionKey = "B0BFD5352E459FBE07B079F2FC2CE1D6";
			try
			{
				gxcontext.WriteSessionKey(CryptoImpl.AJAX_ENCRYPTION_KEY, InternalKeyAjaxEncryptionKey);
				websession.Renew();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
			Assert.Equal(InternalKeyAjaxEncryptionKey, gxcontext.ReadSessionKey<string>(CryptoImpl.AJAX_ENCRYPTION_KEY));
			await Task.CompletedTask;
		}
	}
	public class MockHttpSession : ISession
	{
		string _sessionId = Guid.NewGuid().ToString();
		readonly Dictionary<string, object> _sessionStorage = new Dictionary<string, object>();
		string ISession.Id => _sessionId;
		bool ISession.IsAvailable => throw new NotImplementedException();
		IEnumerable<string> ISession.Keys => _sessionStorage.Keys;
		void ISession.Clear()
		{
			_sessionStorage.Clear();
		}
		Task ISession.CommitAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
		Task ISession.LoadAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
		void ISession.Remove(string key)
		{
			_sessionStorage.Remove(key);
		}
		void ISession.Set(string key, byte[] value)
		{
			_sessionStorage[key] = Encoding.UTF8.GetString(value);
		}
		bool ISession.TryGetValue(string key, out byte[] value)
		{

			if (_sessionStorage.ContainsKey(key) && _sessionStorage[key] != null)
			{
				value = Encoding.ASCII.GetBytes(_sessionStorage[key].ToString());
				return true;
			}
			value = null;
			return false;
		}
	}
}