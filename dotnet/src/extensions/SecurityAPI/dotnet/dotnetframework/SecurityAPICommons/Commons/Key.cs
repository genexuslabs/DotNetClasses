using System.Security;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;

namespace SecurityAPICommons.Commons
{
    [SecuritySafeCritical]
    public class Key : SecurityAPIObject
    {
		protected string algorithm;

		[SecuritySafeCritical]
		public string getAlgorithm()
		{
			return this.algorithm;
		}

		[SecuritySafeCritical]
        public virtual bool Load(string path) { return false; }

        [SecuritySafeCritical]
        public virtual bool LoadPKCS12(string path, string alias, string password) { return false; }

		[SecuritySafeCritical]
		public virtual bool FromBase64(string base64) { return false; }

		[SecuritySafeCritical]
		public virtual string ToBase64() { return ""; }

		[SecuritySafeCritical]
		public virtual AsymmetricKeyParameter getAsymmetricKeyParameter() { return null; }

		[SecuritySafeCritical]
		public virtual void setAlgorithm() { }

		[SecuritySafeCritical]
		public virtual AsymmetricAlgorithm getAsymmetricAlgorithm() { return null; }
	}
}
