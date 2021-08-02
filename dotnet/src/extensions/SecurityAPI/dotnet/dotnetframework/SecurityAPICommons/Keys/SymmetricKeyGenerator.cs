
using SecurityAPICommons.Commons;
using System;
using System.Security;


namespace SecurityAPICommons.Keys
{
    /// <summary>
    /// Key generator
    /// </summary>
    [SecuritySafeCritical]
    public class SymmetricKeyGenerator : SecurityAPIObject
    {



        /// <summary>
        /// SymmetricKeyGenerator class constructor
        /// </summary>
        [SecuritySafeCritical]
        public SymmetricKeyGenerator() : base()
        {

        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/


        /// <summary>
        /// Generate a fixed lenght (bits) key
        /// </summary>
        /// <param name="symmetricKeyType">string symmetricKeyType</param>
        /// <param name="length">result key length</param>
        /// <returns>string Hexa fixed length secure random generated key</returns>
        [SecuritySafeCritical]
        public string doGenerateKey(string symmetricKeyType, int length)
        {
            SymmetricKeyType sKeyType = SymmetricKeyTypeUtils.getSymmetricKeyType(symmetricKeyType, this.error);
            if (sKeyType == SymmetricKeyType.GENERICRANDOM)
            {
                return genericKeyGenerator(length);
            }
            this.error.setError("SS003", "Unrecognized SymmetricKeyType");
            return "";
        }

        /// <summary>
        /// Generates a fixed length (bits) IV
        /// </summary>
        /// <param name="symmetricKeyType">string symmetricKeyType</param>
        /// <param name="length">result IV length</param>
        /// <returns>string Hexa fixed length secure random generated IV</returns>
        [SecuritySafeCritical]
        public string doGenerateIV(string symmetricKeyType, int length)
        {
            return doGenerateKey(symmetricKeyType, length);
        }

        /// <summary>
        /// Generates a fixed lenght (bits) nonce
        /// </summary>
        /// <param name="symmetricKeyType">string symmetricKeyType</param>
        /// <param name="length">result nonce length</param>
        /// <returns>string Hexa fixed length secure random generated nonce</returns>
        [SecuritySafeCritical]
        public string doGenerateNonce(string symmetricKeyType, int length)
        {
            return doGenerateKey(symmetricKeyType, length);
        }

        /******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

        /// <summary>
        /// Generate a fixed lenght (bits) key with secure random implementation
        /// </summary>
        /// <param name="length">int bits result key length on bits</param>
        /// <returns>string Hexa fixed length secure random generated key</returns>
        private string genericKeyGenerator(int length)
        {
            System.Security.Cryptography.RNGCryptoServiceProvider rngCsp = new System.Security.Cryptography.RNGCryptoServiceProvider();
            Byte[] result = new Byte[length / 8];
            rngCsp.GetBytes(result);
            string res = toHexaString(result);
            if (this.error.existsError())
            {
                return "";
            }
            return res;
        }



        /// <summary>
        /// Gets the string hexadecimal representation of a byte array
        /// </summary>
        /// <param name="digest">input byte</param>
        /// <returns>Strinx hexadecimal encoded representation</returns>
        public string toHexaString(byte[] digest)
        {

            string result = BitConverter.ToString(digest).Replace("-", string.Empty);
            if (result == null || result.Length == 0)
            {
                this.error.setError("SK005", "Error encoding hexa");
                return "";
            }
            this.error.cleanError();
            return result;
        }

    }
}
