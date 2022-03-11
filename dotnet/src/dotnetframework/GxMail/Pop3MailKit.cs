using System;
using System.Collections.Generic;
using log4net;
using MailKit.Security;
using MailKit.Net.Pop3;

namespace GeneXus.Mail
{
	internal class Pop3MailKit : Pop3SessionBase
	{

		private static readonly ILog log = LogManager.GetLogger(typeof(Pop3MailKit));

		private Pop3Client client;
		private int count;
		private List<string> uIds;


		public override int GetMessageCount()
		{
			try
			{
				return client.Count;
			}
			catch (Exception e)
			{
				GXLogging.Error(log,"Could not get message count.", e);
				return count;
			}
		}

		public override void Login(GXPOP3Session sessionInfo)
		{
			GXLogging.Debug(log, "Using MailKit POP3 Implementation");
			_sessionInfo = sessionInfo;
			client = new Pop3Client();

			try
			{
				client.Connect(Host, Port, sessionInfo.Secure == 1 ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
				if (_sessionInfo.Authentication > 0)
				{
					if (String.IsNullOrEmpty(_sessionInfo.AuthenticationMethod)) // Caso que se hace Auth Basic
					{
						if (String.IsNullOrEmpty(_sessionInfo.UserName) || String.IsNullOrEmpty(_sessionInfo.Password))
						{
							throw new BadCredentialsException();
						}
						else
						{
							client.Authenticate(_sessionInfo.UserName, _sessionInfo.Password);
						}
					}
					else // Caso de otros metodos de autenticacion
					{
						switch (_sessionInfo.AuthenticationMethod)
						{
							case "XOAUTH2":
								var oauth2 = new SaslMechanismOAuth2(_sessionInfo.UserName, _sessionInfo.Password);
								client.Authenticate(oauth2);
								break;

							default:
								GXLogging.Error(log, "Authentication protocol is not supported");
								throw new Exception("Authentication protocol is not supported. Authentication protocol recieved: " + _sessionInfo.AuthenticationMethod);
						}
					}
				}
				count = client.Count;
				uIds = (List<string>)client.GetMessageUids();
				/*uIds.Insert(0, string.Empty);*/

			}
			catch (NotSupportedException e)
			{   // Caso en el que se intenta TLS connection y no es exitoso el intento. Se intenta nuevamente pero son SSL
				try
				{
					log.Warn("Could not establish TLS connection. Next try with SSL", e);
					client.Connect(Host, Port, SecureSocketOptions.SslOnConnect);
				}
				catch (NotSupportedException ex)
				{
					LogError("Error logging in", "Could neither establish TLS nor SSL connection.", GXInternetConstants.MAIL_CantLogin, ex, log);
				}
			}
			catch (AuthenticationException e)
			{
				LogError("Login Error", "Authentication error", MailConstants.MAIL_AuthenticationError, e, log);
			}
			catch (Exception e)
			{
				LogError("Login Error", e.Message, MailConstants.MAIL_CantLogin, e, log);
			}

		}

		public override void Logout(GXPOP3Session sessionInfo)
		{
			throw new NotImplementedException();
		}

		public override void Delete(GXPOP3Session sessionInfo)
		{
			throw new NotImplementedException();
		}

		public override string GetNextUID(GXPOP3Session session)
		{
			throw new NotImplementedException();
		}

		public override void Receive(GXPOP3Session sessionInfo, GXMailMessage gxmessage)
		{
			throw new NotImplementedException();
		}

		public override void Skip(GXPOP3Session sessionInfo)
		{
			throw new NotImplementedException();
		}

	}
}
