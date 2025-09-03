using System;
using System.Security;
using System.Xml;
using GamSaml20.Utils;
using GeneXus;
using log4net;
using Microsoft.IdentityModel.Tokens;

namespace GamSaml20
{
	[SecuritySafeCritical]
	public class PostBinding : IBinding
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(PostBinding));

		private XmlDocument xmlDoc;

		private XmlDocument verifiedDoc;

#if !NETCORE
		private XmlDocument nonCanonicalizedDoc;
#endif


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
#if !NETCORE
			XmlDocument nonDoc = new XmlDocument();
			nonDoc.XmlResolver = null; //disable parser's DTD reading - security meassure
			nonDoc.PreserveWhitespace = true;
			nonDoc.LoadXml(xml);
			this.nonCanonicalizedDoc = nonDoc;
#endif
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
			string verified = string.Empty;
#if NETCORE
			verified =  DSig.ValidateSignatures(this.xmlDoc, parms.TrustedCertPath);
#else
			verified =  DSig.ValidateSignatures(this.nonCanonicalizedDoc, parms.TrustedCertPath);
#endif
			if (verified.IsNullOrEmpty())
			{
				return false;
			}
			else
			{

#if NETCORE
				this.verifiedDoc = new XmlDocument();
				this.verifiedDoc.XmlResolver = null; //disable parser's DTD reading - security meassure
				this.verifiedDoc.PreserveWhitespace = true;
				this.verifiedDoc.LoadXml(verified);
#else
				this.verifiedDoc = GamSaml20.Utils.SamlAssertionUtils.CanonicalizeXml(verified);
#endif
				logger.Debug($"VerifySignatures - sanitized xmlDoc {this.verifiedDoc.OuterXml}");
				return true;
			}
		}

		[SecuritySafeCritical]
		public string GetLoginAssertions()
		{
			logger.Trace("GetLoginAssertions");
			return GamSaml20.Utils.SamlAssertionUtils.GetLoginInfo(this.verifiedDoc);
		}

		[SecuritySafeCritical]
		public string GetLogoutAssertions()
		{
			logger.Trace("GetLogoutAssertions");
			return GamSaml20.Utils.SamlAssertionUtils.GetLogoutInfo(this.verifiedDoc);
		}

		[SecuritySafeCritical]
		public string GetLoginAttribute(string name)
		{
			logger.Trace("GerLoginAttribute");
			return GamSaml20.Utils.SamlAssertionUtils.GetLoginAttribute(this.verifiedDoc, name);
		}

		[SecuritySafeCritical]
		public string GetRoles(string name)
		{
			logger.Trace("GetRoles");
			return GamSaml20.Utils.SamlAssertionUtils.GetRoles(this.verifiedDoc, name);
		}

		[SecuritySafeCritical]
		public bool IsLogout()
		{
			return GamSaml20.Utils.SamlAssertionUtils.IsLogout(this.xmlDoc);
		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/
	}
}
