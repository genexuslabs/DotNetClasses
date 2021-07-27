using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Mail;
using GeneXus.Mail.Exchange;
using Xunit;

namespace DotNetUnitTest.GXMail
{
	public class GXMailServiceSessionTest
	{
		GXMailServiceSession session;
		public GXMailServiceSessionTest()
		{
			
		}

		private void LoginOAuth()
		{
			string appId = Environment.GetEnvironmentVariable("EWS_APPID");
			string secret = Environment.GetEnvironmentVariable("EWS_SECRET");
			string tentantId = Environment.GetEnvironmentVariable("EWS_TENANTID");

		
			Skip.If(String.IsNullOrEmpty(appId), "Skipped because AppId is empty");
			Skip.If(String.IsNullOrEmpty(secret), "Skipped because Secret is empty");
			Skip.If(String.IsNullOrEmpty(tentantId), "Skipped because TenantId is empty");

			session = new GXMailServiceSession();
			session.SetProperty(ExchangeSession.AppIdProperty, appId);
			session.SetProperty(ExchangeSession.ClientSecretProperty, secret);
			session.SetProperty(ExchangeSession.TenantIdProperty, tentantId);
			session.SetProperty("ExchangeVersion", "Exchange2013_SP1");

			session.UserName = "ggallotti@genexus.onmicrosoft.com";

			session.Login();

			Assert.Equal(0, session.ErrCode);
		}

		[SkippableFact]
		public void SendMailTest()
		{
			LoginOAuth();

			string mailSubject = $"TestMessage XUnit DotNetClasses ({Guid.NewGuid()})";
			string body = "Test Body";

			GXMailMessage m = new GXMailMessage();
			
			m.To.Add(new GXMailRecipient(session.UserName, session.UserName));
			m.To.Add(new GXMailRecipient("Gonzalo", "ggallotti@genexus.com"));
			m.From = new GXMailRecipient(session.UserName, session.UserName);
			m.Subject = mailSubject;
#pragma warning disable CA1303 // Do not pass literals as localized parameters
			m.Text = body;
#pragma warning restore CA1303 // Do not pass literals as localized parameters

			short result = session.Send(m);

			Assert.Equal(0, result);

		}
	}

}
