using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Encryption;
using GeneXus.Http;
using GeneXus.Utils;
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
			var authMiddleware = new MyAuthMiddleware(next: (innerHttpContext) => Task.FromResult(0));

			await authMiddleware.Invoke(httpContext);
		}
	}
	public class MyAuthMiddleware
	{
		private readonly RequestDelegate _next;

		public MyAuthMiddleware(RequestDelegate next)
		{
			_next = next;
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
			await _next(context);
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