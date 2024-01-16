using System;
using System.Net;

using Microsoft.Exchange.WebServices.Data;

namespace GeneXus.Mail.Exchange
{
	public static class Service
    {
 
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger(typeof(Service).FullName);
		static Service()
        {
            CertificateCallback.Initialize();
        }

        // The following is a basic redirection validation callback method. It 
        // inspects the redirection URL and only allows the Service object to 
        // follow the redirection link if the URL is using HTTPS. 
        //
        // This redirection URL validation callback provides sufficient security
        // for development and testing of your application. However, it may not
        // provide sufficient security for your deployed application. You should
        // always make sure that the URL validation callback method that you use
        // meets the security requirements of your organization.
        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;

            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }

            return result;
        }

        public static ExchangeService ConnectToService(IUserData userData)
        {
            return ConnectToService(userData, null);            
        }

        public static ExchangeService ConnectToService(IUserData userData, ITraceListener listener)
        {
            ExchangeService service = new ExchangeService(userData.Version);

            if (log.IsDebugEnabled)
            {
                service.TraceListener = new TraceListener();
                service.TraceFlags = TraceFlags.All;
                service.TraceEnabled = true;
                service.TraceEnablePrettyPrinting = true;
            }
            service.Credentials = new NetworkCredential(userData.EmailAddress, userData.Password.ToString());

            if (userData.AutodiscoverUrl == null)
            {
                service.AutodiscoverUrl(userData.EmailAddress, RedirectionUrlValidationCallback);
                userData.AutodiscoverUrl = service.Url;
            }
            else
            {
                service.Url = userData.AutodiscoverUrl;
            }

            return service;
        }

        public static ExchangeService ConnectToServiceWithImpersonation(
          IUserData userData,
          string impersonatedUserSMTPAddress)
        {
            return ConnectToServiceWithImpersonation(userData, impersonatedUserSMTPAddress, null);
        }

        public static ExchangeService ConnectToServiceWithImpersonation(
          IUserData userData,
          string impersonatedUserSMTPAddress,
          ITraceListener listener)
        {
            ExchangeService service = new ExchangeService(userData.Version);

            if (listener != null)
            {
                service.TraceListener = listener;
                service.TraceFlags = TraceFlags.All;
                service.TraceEnabled = true;
            }

            service.Credentials = new NetworkCredential(userData.EmailAddress, userData.Password);

            ImpersonatedUserId impersonatedUserId =
              new ImpersonatedUserId(ConnectingIdType.SmtpAddress, impersonatedUserSMTPAddress);

            service.ImpersonatedUserId = impersonatedUserId;

            if (userData.AutodiscoverUrl == null)
            {
                service.AutodiscoverUrl(userData.EmailAddress, RedirectionUrlValidationCallback);
                userData.AutodiscoverUrl = service.Url;
            }
            else
            {
                service.Url = userData.AutodiscoverUrl;
            }

            return service;
        }
    }
}
