using System.Xml;
using GamSaml20Net.Utils;
using GeneXus;
using log4net;

namespace GamSaml20Net
{
    public class PostBinding: IBinding
    {
		private static readonly ILog logger = LogManager.GetLogger(typeof(PostBinding));

		private XmlDocument xmlDoc;


		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

		public PostBinding()
		{
			logger.Trace("PostBinding constructor");
			xmlDoc = null;
		}

		public void Init(string xml)
		{
			logger.Trace("init");
			this.xmlDoc = GamSaml20Net.Utils.SamlAssertionUtils.CanonicalizeXml(xml);
			logger.Debug($"Init - XML IdP response: {this.xmlDoc.OuterXml}");
		}

		public static string Login(SamlParms parms, string relayState)
		{
			//not implemented yet
			logger.Error("Login - NOT IMPLEMENTED");
			return string.Empty;
		}

		public static string Logout(SamlParms parms, string relayState)
		{
			//not implemented yet
			logger.Error("Logout - NOT IMPLEMENTED");
			return string.Empty;
		}


		public bool VerifySignatures(SamlParms parms)
		{
			return DSig.ValidateSignatures(this.xmlDoc, parms.TrustedCertPath);
		}

		public string GetLoginAssertions()
		{
			logger.Trace("GetLoginAssertions");
			return GamSaml20Net.Utils.SamlAssertionUtils.GetLoginInfo(this.xmlDoc);
		}

		public string GetLogoutAssertions()
		{
			logger.Trace("GetLogoutAssertions");
			return GamSaml20Net.Utils.SamlAssertionUtils.GetLogoutInfo(this.xmlDoc);
		}

		public string GetLoginAttribute(string name)
		{
			logger.Trace("GerLoginAttribute");
			return GamSaml20Net.Utils.SamlAssertionUtils.GetLoginAttribute(this.xmlDoc, name);
		}

		public string GetRoles(string name)
		{
			logger.Trace("GetRoles");
			return GamSaml20Net.Utils.SamlAssertionUtils.GetRoles(this.xmlDoc, name);
		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/
	}
}
