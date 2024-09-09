using System;
using System.Security;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Keys;

namespace GeneXusCryptography.AsymmetricUtils
{
	[SecuritySafeCritical]
	public class SignatureStandardOptions : SecurityAPIObject
	{
		private CertificateX509 certificate;
		private PrivateKeyManager privateKey;

		private SignatureStandard signatureStandard;

		private bool encapsulated;

		[SecuritySafeCritical]
		public SignatureStandardOptions() : base()
		{
			this.signatureStandard = SignatureStandard.CMS;
			this.encapsulated = false;
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/
		public void SetPrivateKey(PrivateKeyManager key)
		{
			this.privateKey = key;
		}

		public void SetCertificate(CertificateX509 cert)
		{
			this.certificate = cert;
		}

		public bool SetSignatureStandard(String standard)
		{
			this.signatureStandard = SignatureStandardUtils.getSignatureStandard(standard, this.error);
			return this.HasError() ? false : true;
		}

		public void SetEncapsulated(bool value) { this.encapsulated = value; }

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		public PrivateKeyManager GetPrivateKey()
		{
			return this.privateKey;
		}

		public CertificateX509 GetCertificate() { return this.certificate; }

		public SignatureStandard GetSignatureStandard() { return this.signatureStandard; }

		public bool GetEncapsulated() { return this.encapsulated; }
	}
}
