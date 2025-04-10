using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using GamSaml20.Utils;
using GeneXus;
using log4net;

namespace GamSaml20
{
	[SecuritySafeCritical]
	public class RedirectBinding : IBinding
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(RedirectBinding));


		private XmlDocument xmlDoc;
		private Dictionary<string, string> redirectMessage;

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

		[SecuritySafeCritical]
		public RedirectBinding()
		{
			logger.Debug("RedirectBinding constructor");
		}


		[SecuritySafeCritical]
		public void Init(string queryString)
		{
			logger.Trace("Init");
			logger.Debug($"Init - queryString : {queryString}");
			this.redirectMessage = ParseRedirect(queryString);
			string xml = System.Text.Encoding.UTF8.GetString(GamSaml20.Utils.Encoding.DecodeAndInflateXmlParameter(this.redirectMessage["SAMLResponse"]));
			this.xmlDoc = GamSaml20.Utils.SamlAssertionUtils.CanonicalizeXml(xml);
			logger.Debug($"Init - XML IdP response: {this.xmlDoc.OuterXml}");
		}

		[SecuritySafeCritical]
		public static string Login(SamlParms parms, string relayState)
		{
			logger.Trace("Login_RedirectBinding");
			XElement request = GamSaml20.Utils.SamlAssertionUtils.CreateLoginRequest(parms.Id, parms.EndPointLocation, parms.Acs, parms.IdentityProviderEntityID, parms.PolicyFormat, parms.AuthContext, parms.ServiceProviderEntityID, parms.ForceAuthn);
			return GenerateQuery(request, parms.EndPointLocation, parms.CertPath, parms.CertPass, relayState);
		}

		[SecuritySafeCritical]
		public static string Logout(SamlParms parms, string relayState)
		{
			logger.Trace("Logout_RedirectBinding");
			XElement request = GamSaml20.Utils.SamlAssertionUtils.CreateLogoutRequest(parms.Id, parms.ServiceProviderEntityID, parms.NameID, parms.SessionIndex, parms.SingleLogoutEndpoint);
			return GenerateQuery(request, parms.SingleLogoutEndpoint, parms.CertPath, parms.CertPass, relayState);
		}

		[SecuritySafeCritical]
		public bool VerifySignatures(SamlParms parms)
		{
			logger.Trace("VerifySignature");

			try
			{
				return VerifySignature_internal(parms.TrustedCertPath);
			}
			catch (Exception e)
			{
				logger.Error("VerifySignature", e);
				return false;
			}
		}

		[SecuritySafeCritical]
		public string GetLogoutAssertions()
		{
			logger.Trace("GetLogoutAssertions");
			return SamlAssertionUtils.GetLogoutInfo(this.xmlDoc);
		}

		[SecuritySafeCritical]
		public string GetRelayState()
		{
			logger.Trace("GetRelayState");
			string value;
			string relayState = this.redirectMessage.TryGetValue("RelayState", out value) ? value : String.Empty;
			return HttpUtility.UrlDecode(relayState);

		}

		[SecuritySafeCritical]
		public string GetLoginAssertions()
		{
			//Getting user's data by URL parms (GET) is deemed insecure so we are not implementing this method for redirect binding
			logger.Error("GetLoginAssertions - NOT IMPLEMENTED insecure SAML implementation");
			return string.Empty;
		}

		[SecuritySafeCritical]
		public string GetRoles(string name)
		{
			//Getting user's data by URL parms (GET) is deemed insecure so we are not implementing this method for redirect binding
			logger.Error("GetLoginRoles - NOT IMPLEMENTED insecure SAML implementation");
			return string.Empty;
		}

		[SecuritySafeCritical]
		public string GetLoginAttribute(string name)
		{
			//Getting user's data by URL parms (GET) is deemed insecure so we are not implementing this method for redirect binding
			logger.Error("GetLoginAttribute - NOT IMPLEMENTED insecure SAML implementation");
			return string.Empty;
		}

		[SecuritySafeCritical]
		public bool IsLogout()
		{
			return GamSaml20.Utils.SamlAssertionUtils.IsLogout(this.xmlDoc);
		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

		private bool VerifySignature_internal(string certPath)
		{
			logger.Trace("VerifySignature_internal");

			byte[] signature = GamSaml20.Utils.Encoding.DecodeParameter(this.redirectMessage["Signature"]);
			string value;
			string signedMessage;
			if (this.redirectMessage.TryGetValue("RelayState", out value))
			{
				signedMessage = $"SAMLResponse={this.redirectMessage["SAMLResponse"]}";
				signedMessage += $"&RelayState={this.redirectMessage["RelayState"]}";
				signedMessage += $"&SigAlg={this.redirectMessage["SigAlg"]}";
			}
			else
			{
				signedMessage = $"SAMLResponse={this.redirectMessage["SAMLResponse"]}";
				signedMessage += $"&SigAlg={this.redirectMessage["SigAlg"]}";
			}

			byte[] query = System.Text.Encoding.UTF8.GetBytes(signedMessage);
			try
			{
				RSACryptoServiceProvider csp = Keys.GetPublicRSACryptoServiceProvider(certPath);

				if (csp == null)
				{
					logger.Error("VerifySignature_internal logout RSACryptoServiceProvider is null");
				}
				string sigalg = HttpUtility.UrlDecode(this.redirectMessage["SigAlg"]);
				Hash hash = HashUtils.GetHashFromSigAlg(sigalg);

				return csp.VerifyData(query, CryptoConfig.MapNameToOID(HashUtils.ValueOf(hash)), signature);
			}
			catch (Exception e)
			{
				logger.Error("VerifySignature_internal", e);
				return false;
			}
		}

		private static Dictionary<string, string> ParseRedirect(string request)
		{
			logger.Trace("ParseRedirect");
			Dictionary<string, string> result = new Dictionary<string, string>();
			string[] redirect = request.Split('&');

			foreach (string s in redirect)
			{
				string[] res = s.Split('=');
				result[res[0]] = res[1];
			}
			return result;
		}

		private static string GenerateQuery(XElement request, string destination, string certPath, string certPass, string relayState)
		{
			logger.Trace("GenerateQuery");
			try
			{
				string samlRequestParameter = GamSaml20.Utils.Encoding.DelfateAndEncodeXmlParameter(request.ToString());
				string relayStateParameter = HttpUtility.UrlEncode(relayState);
				Hash hash = Keys.IsBase64(certPath) ? HashUtils.GetHash(certPass.ToUpper().Trim()) : HashUtils.GetHash(Keys.GetHash(certPath, certPass));

				string sigAlgParameter = HttpUtility.UrlEncode(HashUtils.GetSigAlg(hash));

				string query = $"SAMLRequest={samlRequestParameter}&RelayState={relayStateParameter}&SigAlg={sigAlgParameter}";

				string signatureParameter = HttpUtility.UrlEncode(SignRequest(query, certPath, certPass, hash));


				query += $"&Signature={signatureParameter}";
				logger.Debug($"GenerateQuery - query: {query}");
				return $"{destination}?{query}";
			}
			catch (Exception e)
			{
				logger.Error("GenerateQuery", e);
				return String.Empty;
			}
		}

		private static string SignRequest(string query, string path, string password, Hash hash)
		{
			logger.Trace("SignRequest");

			RSACryptoServiceProvider csp = Keys.GetPrivateRSACryptoServiceProvider(path, password);
			try
			{
				byte[] encrypted = csp.SignData(System.Text.Encoding.UTF8.GetBytes(query), CryptoConfig.MapNameToOID(HashUtils.ValueOf(hash)));
				return Convert.ToBase64String(encrypted);
			}
			catch (Exception e)
			{
				logger.Error("SignRequest", e);
				return String.Empty;
			}
		}


	}
}
