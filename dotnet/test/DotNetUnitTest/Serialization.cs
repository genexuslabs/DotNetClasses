using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using GeneXus.Application;
using GeneXus.Encryption;
using GeneXus.Http;
using Xunit;

namespace DotNetUnitTest
{
	public class SerializationTest
	{
		
		private void TestSetup()
		{
			var httpRequest = new HttpRequest("", "http://localhost/", "");
			var httpResponce = new HttpResponse(new StringWriter());
			var httpContext = new HttpContext(httpRequest, httpResponce);
			var sessionContainer =
				new HttpSessionStateContainer("id",
											   new SessionStateItemCollection(),
											   new HttpStaticObjectsCollection(),
											   10,
											   true,
											   HttpCookieMode.AutoDetect,
											   SessionStateMode.InProc,
											   false);
			httpContext.Items["AspSession"] =
				typeof(HttpSessionState)
				.GetConstructor(
									BindingFlags.NonPublic | BindingFlags.Instance,
									null,
									CallingConventions.Standard,
									new[] { typeof(HttpSessionStateContainer) },
									null)
				.Invoke(new object[] { sessionContainer });

			HttpContext.Current = httpContext;
		}

		[Fact]
		public void TestSessionRenew()
		{
			TestSetup();
			GxContext gxcontext = new GxContext();
			gxcontext.HttpContext = HttpContext.Current;
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
		}
	}
}
