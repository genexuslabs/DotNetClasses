using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using GamSaml20;
using GamSaml20.Utils;
using NUnit.Framework;

namespace GamTest.Saml20
{
	[TestFixture]
	public class TestRedirectBinding
	{
		private static string resources;
		private static string BASE_PATH;

		private static string password;
		private static RedirectBinding redirectBindingLogoutResponse;

		[SetUp]
		public virtual void SetUp()
		{
			BASE_PATH = GetStartupDirectory();
			resources = Path.Combine(BASE_PATH, "Resources", "dummycerts");
			password = "dummy1";

			redirectBindingLogoutResponse = new RedirectBinding();
			string logoutResponse = "SAMLResponse=fVHLauQwEPwVo7tsSX7JwnYICVkCyR52ZnPYyyBLPYnBozZueZPPX8%2BEOQSWHJvqquqqbm8%2BTlPyFxYaMXRMpoIlEBz6Mbx27Pf%2BgWuWULTB2wkDdCwgu%2BlbsqdpNk%2F4imv8BTRjIEg2pUDmAnVsXYJBSyOZYE9AJjqzu31%2BMioVZl4wosOJJfdAcQw2XszfYpzJZNmEzk5vSDH7cft8OLMOdxgCuIjLT4gPmaUJU0vzB0se7zt2GKq8sMo7nov6yItKAdd60FwcRV0rXRWFhm01XC%2Fd40aq68ZrbS23FQheQJVz7SvNS%2B%2B9ODYbLO1GIlrhMZwbiB1TQpVclFw2e1kaKY3KU1XWf1jycm1wy8f69kJbPhv5vgtLBMs5P%2Buv%2BSlS%2Bj4Gj%2B%2BUBohZCa4ehmPDpa7VFs81fFBCcitBatk01ZCXWZt9el5%2Fs4s2rvR1ukMPyYudVvj%2BJrpsm93qHBCxrG%2Bzr6LZ%2F%2F7f%2FwM%3D&RelayState=http%3A%2F%2Frelaystate.com&SigAlg=http%3A%2F%2Fwww.w3.org%2F2001%2F04%2Fxmldsig-more%23rsa-sha256&Signature=ZhxqgSDAmtwxtUAXCafCNAXKLwL9iPgsqInuZfQ97dyPsGyszpgJftjgHBtoQpz159NjFpX0dGicVier2TQa82JBqgxUvdPT6mg%2FppdG7Z%2BnOXNttflqCd7mA3b%2FUOmWE4XgODz2mym%2BNPBmETAYmKofXo5ghpQc8IgGpI166%2F5VOwwhLcrg76HeYSxubxS4BoFUtLmpRnkaww9VQPZPIyh4kBmsCqe%2FV4QvM626ehdXDjPIciBgylt2ENMfQGZo83ubMB7KxgDNdErBgmTpILxftLn3ZH0FJAbM%2B3bzj6DFJ1yLuyUnUbdxOjoKaRskil853jKqmbvQtxRQ4QvZIg%3D%3D";
			redirectBindingLogoutResponse.Init(logoutResponse);
		}

		[Test]
		public void TestSignatureValidation_true()
		{
			SamlParms parms = new SamlParms();
			parms.TrustedCertPath = Path.Combine(resources, "saml20", "javacert.crt");
			Console.WriteLine(parms.TrustedCertPath);
			Assert.IsTrue(redirectBindingLogoutResponse.VerifySignatures(parms), "TestSignatureValidation_true Logout");

		}

		[Test]
		public void TestSignatureValidation_false()
		{
			SamlParms parms = new SamlParms();
			parms.TrustedCertPath = Path.Combine(resources, "saml20", "sha512_cert.crt");
			Assert.IsFalse(redirectBindingLogoutResponse.VerifySignatures(parms), "TestSignatureValidation_false Logout");
		}

		[Test]
		public void TestGetLogoutAssertions()
		{
			string expected = "{\"Destination\": \"https://localhost/GAM_SAML_ConnectorNetF/aslo.aspx\",\"InResponseTo\": \"_779d88aa-a6e0-4e63-8d68-5ddd0f97791a\",\"Value\": \"urn:oasis:names:tc:SAML:2.0:status:Success\",\"Issuer\": \"https://sts.windows.net/5ec7bbf9-1872-46c9-b201-a1e181996b35/\" }";
			Assert.AreEqual(expected, redirectBindingLogoutResponse.GetLogoutAssertions());
		}

		[Test]
		public void TestIsLogout()
		{
			string loginResponse = "SAMLResponse=5Vbfb%2BJGEH7uSf0frH03%2FoGNjRW40qSpkJJcFOipvZdoWQ%2FgO3vX3V0Hcn99Zw0mYHqE3lWqTn2yPDOe%2Beb7dmZ98XZd5NYTSJUJPiBexyUWcCbSjC8G5LfptR0TS2nKU5oLDgPCBXk7vFC0yP0yeQBVCq7AwiRcJRvrgFSSJ4KqTCWcFqASzZLJ6PYm8TtuUkqhBRM52X6zVgOy1LpMHGe1WnVW3Y6QC8d3Xc%2F5%2FfZmwpZQUGJdgdIZp7oGuQ3PBaP5Uijt%2FDq6fTQFHi8F58C0kHegrx1KmepQVa6JNb4akCy1w27kRj0v7rluHHleEEZe0I17YRB3%2BxjEm36mYkAeGcy7bBa5duy5gR0wd2bHaRrazAv6EaPhPEgR2VipCsbcUKQHxHf90HZD23enXj8JeokXdmK394FY7xuKkQQy%2FPHNDxsOk%2Fp7uU%2Fgaf6oUiAND8S6FrKg%2BnS4sWDj8zo0Aa4z%2FUyGe4SLT5p2mCgcWH%2Fyn0q6iP7sxn88FM%2FTMI0unH2QL6jLZKKprpSxtEyXIgXrPc0rOA1M1dHJpGIMlCJOndw5yr5ladR0%2FVVEbdWPfb%2FXDyK%2F6wZ9PAhhL%2FK9KHY9P%2Br77jco2ZLyv5ClgTCpZh9xAGpTY7vDWuOrV2F5Ha8Nq6BZPkpTaQQa0hw%2B4hqQovMZZjTPhfppYQIMygbQptRB9S0inMx5ZrIaDW9BL0V6miFWJDOgEuSG4lP5rqimXze8d0K%2F4%2B%2FkaK5BtiUPvY3kEUr%2BACwrMzDn4p8vH2fLh%2FPFBjYKOscSbnvG2DQzgcog%2FhlQHzg6oXtwz2zrQKdRlWKDDJBFLTO2g3UUMJzcY8sN2p31oMcvJWv8Lw3ttzmq9JKb0YcCqbbq1zPmcYLHE1ONeQrrc5VvtY6FEJKGtW73%2FOK5zHGjPMB8eHLpsISZODTf42MlZHqPFx7qCelUUo4nVOoXlv4me4vHFrIDx46qAxI1Mj6rNBx6j9yWmVUcQVySv5gxJrXhnN1FMYWxNmtihnGsNaW7MvVNsLvqs9fvejurFWeAPwgqS%2FRziSjXuPExIV%2Bct4YOq7cobXyv0nKdSaXN6%2FdCzWKdwtO%2Fy8GNWGT8%2F3k0jqwHA7XzNn8ZaNz9vjQ30fAv&RelayState=http%3A%2F%2Frelaystate.com&SigAlg=http%3A%2F%2Fwww.w3.org%2F2001%2F04%2Fxmldsig-more%23rsa-sha256&Signature=aeNkIQLkkrNIxVgd1slzZXJpkEzvU0LIwqMR9wWLRT%2FjMHo7ldaCeGlFk3H%2Bbr4l3qEttjsTBWgTGgPDgzax7DDCUSvJPdAh0YB14T9oZ243cxap2OOi483TkBPt%2BwM6Q4AaePWbH1NdUvFUmP9ovl4Ub3iC4O%2FmZFRR3l4TU4z5ZR5OO8%2FFm%2BppvYXf%2FJDbsTLkKgF72a1lD1YhNWdqYKx3%2BQ22x94osmXis3omG7cdNDlo8ULesWL2RVXzftjmHa9zqWidTrHjyA6fSouTV3pQHmzrI8t9g3tuk5jKzTbOPmF2KBhEPzvN26jH2Bdy5b4PCvkJ1L9VeJKlGwBejQ%3D%3D";
			RedirectBinding redirectBindingLoginResponse = new RedirectBinding();
			redirectBindingLoginResponse.Init(loginResponse);
			Assert.IsTrue(redirectBindingLogoutResponse.IsLogout(), "TestIsLogout Logout");
			Assert.IsFalse(redirectBindingLoginResponse.IsLogout(), "TestIsLogout Login");
		}

		[Test]
		public void TestGetRelayState()
		{
			string expected = "http://relaystate.com";
			Assert.AreEqual(expected, redirectBindingLogoutResponse.GetRelayState());
		}


		[Test]
		public void TestLoginRequest()
		{
			string function = "Login";
			RedirectBinding redirectBinding = new RedirectBinding();
			SamlParms parms = CreateParameters();
			string samlRequest = RedirectBinding.Login(parms, "http://relaystate.com");
			string queryString = GetQueryString(samlRequest);
			Dictionary<string, string> redirectMessage = ParseRedirect(queryString);

			//test login request parameters
			TestRequestParameters(redirectMessage, function);

			//test login signature
			TestRequestSignature(redirectMessage, function);

			//test login request xml parameters
			string xml = System.Text.Encoding.UTF8.GetString(GamSaml20.Utils.Encoding.DecodeAndInflateXmlParameter(redirectMessage["SAMLRequest"]));

			string expectedXml = $"<AuthnRequest ID=\"_idtralala\" Version=\"2.0\" IssueInstant=\"{GetIssuerInstant(xml, "AuthnRequest").Trim()}\" Destination=\"http://endpoint/saml\" AssertionConsumerServiceURL=\"http://myapp.com/acs\" ForceAuthn=\"false\" xmlns=\"urn:oasis:names:tc:SAML:2.0:protocol\">\r\n" +
			"  <Issuer xmlns=\"urn:oasis:names:tc:SAML:2.0:assertion\">EntityID</Issuer>\r\n" +
			"  <NameIDPolicy Format=\"urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress\" AllowCreate=\"true\" SPNameQualifier=\"SPEntityID\" />\r\n" +
			"  <RequestedAuthnContext Comparison=\"exact\">\r\n" +
			"    <AuthnContextClassRef xmlns=\"urn:oasis:names:tc:SAML:2.0:assertion\">urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</AuthnContextClassRef>\r\n" +
			"  </RequestedAuthnContext>\r\n" +
			"</AuthnRequest>";

			//"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><saml2p:AuthnRequest AssertionConsumerServiceURL=\"http://myapp.com/acs\" Destination=\"http://endpoint/saml\" ForceAuthn=\"false\" ID=\"_idtralala\" IssueInstant=\"" + GetIssuerInstant(xml, "saml2p:AuthnRequest") + "\" Version=\"2.0\" xmlns:saml2p=\"urn:oasis:names:tc:SAML:2.0:protocol\"><saml2:Issuer xmlns:saml2=\"urn:oasis:names:tc:SAML:2.0:assertion\"/><saml2p:NameIDPolicy AllowCreate=\"true\" Format=\"urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress\" SPNameQualifier=\"SPEntityID\"/><saml2p:RequestedAuthnContext Comparison=\"exact\"><saml2:AuthnContextClassRef xmlns:saml2=\"urn:oasis:names:tc:SAML:2.0:assertion\">urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport</saml2:AuthnContextClassRef></saml2p:RequestedAuthnContext></saml2p:AuthnRequest>";
			Assert.AreEqual(expectedXml, xml, "Test Login request xml parameters");

		}

		[Test]
		public void TestLogoutRequest()
		{
			string function = "Logout";
			RedirectBinding redirectBinding = new RedirectBinding();
			SamlParms parms = CreateParameters();
			string samlRequest = RedirectBinding.Logout(parms, "http://relaystate.com");
			string queryString = GetQueryString(samlRequest);
			Dictionary<string, string> redirectMessage = ParseRedirect(queryString);

			//test logout request parameters
			TestRequestParameters(redirectMessage, function);

			//test logout signature
			TestRequestSignature(redirectMessage, function);

			//test logout request xml parameters
			string xml = System.Text.Encoding.UTF8.GetString(GamSaml20.Utils.Encoding.DecodeAndInflateXmlParameter(redirectMessage["SAMLRequest"]));
			string expectedXml = $"<LogoutRequest ID=\"_idtralala\" Version=\"2.0\" IssueInstant=\"{GetIssuerInstant(xml, "LogoutRequest")}\" Destination=\"http://idp.com/slo\" Reason=\"urn:oasis:names:tc:SAML:2.0:logout:user\" xmlns=\"urn:oasis:names:tc:SAML:2.0:protocol\">\r\n" +
			"  <Issuer xmlns=\"urn:oasis:names:tc:SAML:2.0:assertion\">SPEntityID</Issuer>\r\n" +
			"  <NameID xmlns=\"urn:oasis:names:tc:SAML:2.0:assertion\">nameID</NameID>\r\n" +
			"  <SessionIndex>123456789</SessionIndex>\r\n" +
			"</LogoutRequest>";
			Assert.AreEqual(expectedXml, xml, "Test Logout request xml parameters");

		}

		private static string GetIssuerInstant(string xml, string name)
		{
			XmlDocument doc = SamlAssertionUtils.CanonicalizeXml(xml);
			XmlNodeList nodeList = doc.GetElementsByTagName(name);
			string issuer = nodeList[0].Attributes.GetNamedItem("IssueInstant").Value;
			return issuer;

		}

		private bool VerifySignature_internal(string certPath, Dictionary<string, string> redirectMessage)
		{

			byte[] signature = GamSaml20.Utils.Encoding.DecodeParameter(redirectMessage["Signature"]);
			string value;
			string signedMessage;
			if (redirectMessage.TryGetValue("RelayState", out value))
			{
				signedMessage = $"SAMLRequest={redirectMessage["SAMLRequest"]}";
				signedMessage += $"&RelayState={redirectMessage["RelayState"]}";
				signedMessage += $"&SigAlg={redirectMessage["SigAlg"]}";
			}
			else
			{
				signedMessage = $"SAMLRequest={redirectMessage["SAMLRequest"]}";
				signedMessage += $"&SigAlg={redirectMessage["SigAlg"]}";
			}

			byte[] query = System.Text.Encoding.UTF8.GetBytes(signedMessage);
			try
			{
				RSACryptoServiceProvider csp = Keys.GetPublicRSACryptoServiceProvider(certPath);

				if (csp == null)
				{
					Assert.Fail("VerifySignature_internal logout RSACryptoServiceProvider is null");
				}
				string sigalg = HttpUtility.UrlDecode(redirectMessage["SigAlg"]);
				GamSaml20.Utils.Hash hash = HashUtils.GetHashFromSigAlg(sigalg);

				return csp.VerifyData(query, CryptoConfig.MapNameToOID(HashUtils.ValueOf(hash)), signature);
			}
			catch (Exception e)
			{
				Assert.Fail("VerifySignature_internal", e);
				return false;
			}
		}

		private void TestRequestSignature(Dictionary<string, string> redirectMessage, string function)
		{
			bool verifies = VerifySignature_internal(Path.Combine(resources, "saml20", "javacert.crt"), redirectMessage);
			Assert.IsTrue(verifies, $"Test {function} request signature");
		}

		private void TestRequestParameters(Dictionary<string, string> redirectMessage, string function)
		{
			string relayState = DecodeParm(redirectMessage["RelayState"]);
			Assert.AreEqual("http://relaystate.com", relayState, $"Test {function} parameters RelayState");
			string sigAlg = DecodeParm(redirectMessage["SigAlg"]);
			Assert.AreEqual("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", sigAlg, $"Test {function} request parameters SigAlg");
		}

		private static string DecodeParm(string parm)
		{
			try
			{
				return HttpUtility.UrlDecode(parm);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				return "";
			}
		}

		private static string GetQueryString(string samlRequest)
		{
			try
			{
				string[] uri = samlRequest.Split('?');
				return uri[1];
			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				return "";
			}
		}

		private static SamlParms CreateParameters()
		{
			SamlParms parms = new SamlParms();
			parms.CertPath = Path.Combine(resources, "saml20", "mykeystore.pfx");
			parms.CertPass = password;
			parms.Acs = "http://myapp.com/acs";
			parms.ForceAuthn = false;
			parms.ServiceProviderEntityID = "SPEntityID";
			parms.IdentityProviderEntityID = "EntityID";
			parms.PolicyFormat = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
			parms.AuthContext = "urn:oasis:names:tc:SAML:2.0:ac:classes:PasswordProtectedTransport";
			parms.EndPointLocation = "http://endpoint/saml";
			parms.Id = "_idtralala";
			parms.SingleLogoutEndpoint = "http://idp.com/slo";
			parms.SessionIndex = "123456789";
			parms.NameID = "nameID";
			return parms;
		}

		private static Dictionary<string, string> ParseRedirect(string request)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			string[] redirect = request.Split('&');

			foreach (string s in redirect)
			{
				string[] res = s.Split('=');
				result[res[0]] = res[1];
			}
			return result;
		}

		private static string GetStartupDirectory()
		{
#pragma warning disable SYSLIB0044
			string dir = Assembly.GetCallingAssembly().GetName().CodeBase;
#pragma warning restore SYSLIB0044
			Uri uri = new Uri(dir);
			return Path.GetDirectoryName(uri.LocalPath);
		}
	}
}
