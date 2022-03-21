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
		public GXMailServiceSessionTest()
		{
			
		}

		
		private static GXMailServiceSession LoginOAuthApplication()
		{
			string appId, secret, tentantId;
			GXMailServiceSession session;
			InitializeSession(out appId, out secret, out tentantId, out session);

			Skip.If(String.IsNullOrEmpty(secret), "Skipped because Secret is empty");

			session.SetProperty(ExchangeSession.AuthenticationType, AuthenticationType.OAuthApplication.ToString());
			session.SetProperty(ExchangeSession.ClientSecretProperty, secret);
			
			session.Login();

			Assert.Equal(0, session.ErrCode);
			return session;
		}


		private static GXMailServiceSession LoginOAuthDelegate()
		{
			string appId, secret, tentantId;
			GXMailServiceSession session;
			InitializeSession(out appId, out secret, out tentantId, out session);

			

			session.SetProperty(ExchangeSession.AuthenticationType, AuthenticationType.OAuthDelegated.ToString());
			session.Password = Environment.GetEnvironmentVariable("EWS_PASSWORD");

			Skip.If(String.IsNullOrEmpty(session.Password), "Skipped because Password is empty");

			session.Login();

			Assert.Equal(0, session.ErrCode);
			return session;
		}

		private static GXMailServiceSession LoginOAuthDelegateInteractive()
		{
			string appId, secret, tentantId;
			GXMailServiceSession session;
			InitializeSession(out appId, out secret, out tentantId, out session);

			session.SetProperty(ExchangeSession.AuthenticationType, AuthenticationType.OAuthDelegatedInteractive.ToString());
			session.Password = Environment.GetEnvironmentVariable("EWS_PASSWORD");
			session.Login();

			Assert.Equal(0, session.ErrCode);
			return session;
		}

		private static GXMailServiceSession LoginOAuthUserNamePassword()
		{
			string appId, secret, tentantId;
			GXMailServiceSession session;
			InitializeSession(out appId, out secret, out tentantId, out session);

			session.SetProperty(ExchangeSession.AuthenticationType, AuthenticationType.OAuthUsernamePassword.ToString());
			session.Password = Environment.GetEnvironmentVariable("EWS_BASIC_PASSWORD");
			session.Login();

			Assert.Equal(0, session.ErrCode);
			return session;
		}

		private static GXMailServiceSession LoginBasic() //Default
		{			
			GXMailServiceSession session;
			string mailAddress = Environment.GetEnvironmentVariable("EWS_ADDRESS");

			Skip.If(String.IsNullOrEmpty(mailAddress), "Skipped because mailAddress is empty");			

			Assert.NotEmpty(mailAddress);

			session = new GXMailServiceSession();			
			session.SetProperty("ExchangeVersion", "Exchange2013_SP1");
			session.UserName = mailAddress;
			session.Password = Environment.GetEnvironmentVariable("EWS_BASIC_PASSWORD");
			session.Login();

			
			return session;
		}

		private static void InitializeSession(out string appId, out string secret, out string tentantId, out GXMailServiceSession session)
		{
			appId = Environment.GetEnvironmentVariable("EWS_APPID");
			secret = Environment.GetEnvironmentVariable("EWS_SECRET");
			tentantId = Environment.GetEnvironmentVariable("EWS_TENANTID");
			string mailAddress = Environment.GetEnvironmentVariable("EWS_ADDRESS");

			Skip.If(String.IsNullOrEmpty(appId), "Skipped because AppId is empty");
			Skip.If(String.IsNullOrEmpty(tentantId), "Skipped because TenantId is empty");

			Assert.NotEmpty(mailAddress);
			
			session = new GXMailServiceSession();
			session.SetProperty(ExchangeSession.AppIdProperty, appId);
			session.SetProperty(ExchangeSession.TenantIdProperty, tentantId);
			session.SetProperty("ExchangeVersion", "Exchange2013_SP1");
			session.UserName = mailAddress;
		}

		[SkippableFact]
		public void SendMailTestOAuthApp()
		{
			var session = LoginOAuthApplication();
			SendMail(session);
		}

		[SkippableFact]
		public void SendMailTestOAuthDelegate()
		{
			var session = LoginOAuthDelegate();
			SendMail(session);

		}

		[SkippableFact]
		public void SendMailTestOAuthDelegateInteractive()
		{
			var session = LoginOAuthDelegate();
			SendMail(session);

		}

		[SkippableFact]
		public void SendMailTestOAuthUserNamePassword()
		{
			var session = LoginOAuthUserNamePassword();
			SendMail(session);
		}

		[SkippableFact]
		public void SendMailTestBasic()
		{
			var session = LoginBasic();
			SendMail(session);
		}

		private static void SendMail(GXMailServiceSession session)
		{
			string mailSubject = $"TestMessage XUnit DotNetClasses ({Guid.NewGuid()})";
			string body = "Test Body";

			GXMailMessage m = new GXMailMessage();

			m.To.Add(new GXMailRecipient(session.UserName, session.UserName));
			m.To.Add(new GXMailRecipient(Environment.GetEnvironmentVariable("TEST_MAIL_NAME"), Environment.GetEnvironmentVariable("TEST_MAIL_ADDRESS")));
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
