using System.Security;
using System.Xml;
using GamSaml20.Utils;
using GeneXus;
using log4net;

namespace GamSaml20
{
	[SecuritySafeCritical]
	public class PostBinding : IBinding
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PostBinding));

		private XmlDocument xmlDoc;


		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

		[SecuritySafeCritical]
		public PostBinding()
		{
			logger.Trace("PostBinding constructor");
			xmlDoc = null;
		}

		[SecuritySafeCritical]
		public void Init(string xml)
		{
			logger.Trace("init");
			this.xmlDoc = GamSaml20.Utils.SamlAssertionUtils.CanonicalizeXml(xml);
			logger.Debug($"Init - XML IdP response: {this.xmlDoc.OuterXml}");
		}

		[SecuritySafeCritical]
		public static string Login(SamlParms parms, string relayState)
		{
			//not implemented yet
			logger.Error("Login - NOT IMPLEMENTED");
			return string.Empty;
		}

		[SecuritySafeCritical]
		public static string Logout(SamlParms parms, string relayState)
		{
			//not implemented yet
			logger.Error("Logout - NOT IMPLEMENTED");
			return string.Empty;
		}


		[SecuritySafeCritical]
		public bool VerifySignatures(SamlParms parms)
		{
			return DSig.ValidateSignatures(this.xmlDoc, parms.TrustedCertPath);
		}

		[SecuritySafeCritical]
		public string GetLoginAssertions()
		{
			logger.Trace("GetLoginAssertions");
			return GamSaml20.Utils.SamlAssertionUtils.GetLoginInfo(this.xmlDoc);
		}

		[SecuritySafeCritical]
		public string GetLogoutAssertions()
		{
			logger.Trace("GetLogoutAssertions");
			return GamSaml20.Utils.SamlAssertionUtils.GetLogoutInfo(this.xmlDoc);
		}

		[SecuritySafeCritical]
		public string GetLoginAttribute(string name)
		{
			logger.Trace("GerLoginAttribute");
			return GamSaml20.Utils.SamlAssertionUtils.GetLoginAttribute(this.xmlDoc, name);
		}

		[SecuritySafeCritical]
		public string GetRoles(string name)
		{
			logger.Trace("GetRoles");
			return GamSaml20.Utils.SamlAssertionUtils.GetRoles(this.xmlDoc, name);
		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/
	}
}
