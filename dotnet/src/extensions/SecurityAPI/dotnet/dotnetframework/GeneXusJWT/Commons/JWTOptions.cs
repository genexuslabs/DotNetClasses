using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Keys;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Security;
using GeneXusJWT.JWTClaims;

namespace GeneXusJWT.GenexusComons
{
    [SecuritySafeCritical]
    public class JWTOptions : SecurityAPIObject
    {

        private PublicClaims publicClaims;
        private RegisteredClaims registeredClaims;
        private byte[] secret;
        private RevocationList revocationList;
        private CertificateX509 certificate;
		private PublicKey publicKey;
        private PrivateKeyManager privateKey;
        private HeaderParameters parameters;

        [SecuritySafeCritical]
        public JWTOptions() : base()
        {
            this.publicClaims = new PublicClaims();
            this.registeredClaims = new RegisteredClaims();
            this.revocationList = new RevocationList();
            this.parameters = new HeaderParameters();

        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/

        [SecuritySafeCritical]
        public void SetPrivateKey(PrivateKeyManager key)
        {
            this.privateKey = key;
        }

		[SecuritySafeCritical]
		public void SetPublicKey(PublicKey key)
		{
			this.publicKey = key;
		}

        [SecuritySafeCritical]
        public PrivateKeyManager GetPrivateKey()
        {
            return this.privateKey;
        }

        [SecuritySafeCritical]
        public void SetCertificate(CertificateX509 cert)
        {
            this.certificate = cert;
        }

        /*[SecuritySafeCritical]
        public CertificateX509 GetCertificate()
        {
            return this.certificate;
        }*/



        [SecuritySafeCritical]
        public void SetSecret(string value)
        {

            try
            {
                secret = Hex.Decode(value);
            }
            catch (Exception e)
            {
                this.error.setError("OP001", e.Message);
                secret = null;
            }

        }

        [SecuritySafeCritical]
        public bool AddCustomTimeValidationClaim(string key, string value, string customTime)
        {
            this.registeredClaims.setTimeValidatingClaim(key, value, customTime, this.error);
            if (this.HasError())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [SecuritySafeCritical]
        public bool AddRegisteredClaim(string registeredClaimKey, string registeredClaimValue)
        {
            return registeredClaims.setClaim(registeredClaimKey, registeredClaimValue, this.error);
        }

        [SecuritySafeCritical]
        public bool AddPublicClaim(string publicClaimKey, string publicClaimValue)
        {
            return publicClaims.setClaim(publicClaimKey, publicClaimValue, this.error);
        }

        [SecuritySafeCritical]
        public void AddRevocationList(RevocationList revocationList)
        {
            this.revocationList = revocationList;
        }

        [SecuritySafeCritical]
        public void DeteleRevocationList()
        {
            this.revocationList = new RevocationList();
        }

        [SecuritySafeCritical]
        public void AddHeaderParameter(string name, string value)
        {
            this.parameters.SetParameter(name, value);
        }



		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		public PublicKey GetPublicKey()
		{
			return (this.certificate == null) ? this.publicKey : this.certificate;
		}

		public bool hasPublicClaims()
        {
            return !publicClaims.isEmpty();
        }

        public bool hasRegisteredClaims()
        {
            return !registeredClaims.isEmpty();
        }

        public RegisteredClaims getAllRegisteredClaims()
        {
            return this.registeredClaims;
        }

        public PublicClaims getAllPublicClaims()
        {
            return this.publicClaims;
        }

        public long getcustomValidationClaimValue(string key)
        {
            return this.registeredClaims.getClaimCustomValidationTime(key);
        }

        public bool hasCustomTimeValidatingClaims()
        {
            return this.getAllRegisteredClaims().hasCustomValidationClaims();
        }


        public byte[] getSecret()
        {
            return this.secret;
        }

        public RevocationList getRevocationList()
        {
            return this.revocationList;
        }

        public HeaderParameters GetHeaderParameters()
        {
            return this.parameters;
        }



    }
}
