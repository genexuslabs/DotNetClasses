
using GeneXusCryptography.Commons;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Security;

namespace GeneXusCryptography.PasswordDerivation
{
    /// <summary>
    /// Implements password derivation functions
    /// </summary>
    [SecuritySafeCritical]
    public class PasswordDerivation : SecurityAPIObject, IPasswordDerivationObject
    {


        /// <summary>
        /// PasswordDerivation class constructor
        /// </summary>
        public PasswordDerivation() : base()
        {

        }


        /********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/



        /// <summary>
        /// Hashing and salting of a password with scrypt algorithm
        /// </summary>
        /// <param name="password">string to hash</param>
        /// <param name="salt"> string to use as salt</param>
        /// <param name="CPUCost">CPUCost must be larger than 1, a power of 2 and less than 2^(128 *blockSize / 8)</param>
        /// <param name="blockSize">The blockSize must be >= 1</param>
        /// <param name="parallelization">Parallelization must be a positive integer less than or equal to Integer.MAX_VALUE / (128 * blockSize* 8)</param>
        /// <param name="keyLenght"> fixed key length</param>
        /// <returns>Base64 hashed result</returns>
        [SecuritySafeCritical]
        public string DoGenerateSCrypt(string password, string salt, int CPUCost, int blockSize, int parallelization,
        int keyLenght)
        {
            this.error.cleanError();
            if (!areSCRyptValidParameters(CPUCost, blockSize, parallelization))
            {
                return "";
            }
            EncodingUtil eu = new EncodingUtil();

            byte[] encryptedBytes = SCrypt.Generate(eu.getBytes(password), Hex.Decode(salt), CPUCost, blockSize, parallelization, keyLenght);
            string result = Base64.ToBase64String(encryptedBytes);
            if (result == null || result.Length == 0)
            {
                this.error.setError("PD009", "SCrypt generation error");
                return "";
            }
            this.error.cleanError();
            return result;
        }

        /// <summary>
        /// Calculates SCrypt digest with arbitrary fixed parameters: CPUCost (N) = 16384, blockSize(r) = 8, parallelization(p) = 1, keyLenght = 256
        /// </summary>
        /// <param name="password">string to hash</param>
        /// <param name="salt"> string to use as salt</param>
        /// <returns>Base64 string generated result</returns>
        [SecuritySafeCritical]
        public string DoGenerateDefaultSCrypt(string password, string salt)
        {
            int N = 16384;
            int r = 8;
            int p = 1;
            int keyLenght = 256;
            return DoGenerateSCrypt(password, salt, N, r, p, keyLenght);
        }

        /// <summary>
        /// Hashing and salting of a password with bcrypt algorithm
        /// </summary>
        /// <param name="password">string to hash. the password bytes (up to 72 bytes) to use for this invocation.</param>
        /// <param name="salt">string hexadecimal to salt. The salt lenght must be 128 bits</param>
        /// <param name="cost">	The cost of the bcrypt function grows as 2^cost. Legal values are 4..31 inclusive.</param>
        /// <returns>string Base64 hashed password to store</returns>
        [SecuritySafeCritical]
        public string DoGenerateBcrypt(string password, string salt, int cost)
        {
            this.error.cleanError();
            if (!areBCryptValidParameters(password, salt, cost))
            {
                return "";
            }
            EncodingUtil eu = new EncodingUtil();
            byte[] encryptedBytes = BCrypt.Generate(eu.getBytes(password), Hex.Decode(salt), cost);
            string result = Base64.ToBase64String(encryptedBytes);
            if (result == null || result.Length == 0)
            {
                this.error.setError("PD010", "Brypt generation error");
                return "";
            }
            this.error.cleanError();
            return result;
        }

        /// <summary>
        /// Calculates Bcrypt digest with arbitrary fixed cost parameter: cost = 6
        /// </summary>
        /// <param name="password">string to hash. the password bytes (up to 72 bytes) to use for this invocation.</param>
        /// <param name="salt">string to salt. The salt lenght must be 128 bits</param>
        /// <returns>string Base64 hashed password to store</returns>
        [SecuritySafeCritical]
        public string DoGenerateDefaultBcrypt(string password, string salt)
        {
            int cost = 6;
            return DoGenerateBcrypt(password, salt, cost);
        }

        [SecuritySafeCritical]
        public string doGenerateArgon2(string argon2Version10, string argon2HashType, int iterations, int memory,
        int parallelism, string password, string salt, int hashLength)
        {
            this.error.setError("PD011", "Not implemented function for Net");
            return "";
        }

        [SecuritySafeCritical]
        public string DoGenerateArgon2(string argon2Version10, string argon2HashType, int iterations, int memory,
        int parallelism, String password, string salt, int hashLength)
        {
            this.error.setError("PD012", "Not implemented yet");
            return "";
        }

        /********EXTERNAL OBJECT PUBLIC METHODS  - END ********/



        /// <summary>
        /// Get BCrypt algorithm parameters revised
        /// </summary>
        /// <param name="pwd">password string to hash. the password bytes (up to 72 bytes) to use for this invocation.</param>
        /// <param name="salt">salt string to salt. The salt lenght must be 128 bits</param>
        /// <param name="cost">cost The cost of the bcrypt function grows as 2^cost. Legal values are 4..31 inclusive.</param>
        /// <returns>true if BCrypt parameters are correct</returns>
        private bool areBCryptValidParameters(string pwd, string salt, int cost)
        {
            EncodingUtil eu = new EncodingUtil();
            byte[] pwdBytes = eu.getBytes(pwd);
            byte[] saltBytes = Hex.Decode(salt);
            if (saltBytes.Length * 8 != 128)
            {
                this.error.setError("PD008", "The salt lenght must be 128 bits");
                return false;
            }
            if (cost < 4 || cost > 31)
            {
                this.error.setError("PD007", "The cost of the bcrypt function grows as 2^cost. Legal values are 4..31 inclusive.");
                return false;
            }
            if (pwdBytes.Length > 72)
            {
                this.error.setError("PD006", "The password bytes (up to 72 bytes) to use for this invocation.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get Scrypt algorithm parameters revised
        /// </summary>
        /// <param name="CPUCost">CPUCost must be larger than 1, a power of 2 and less than 2^(128 * blockSize / 8)</param>
        /// <param name="blockSize">The blockSize must be >= 1</param>
        /// <param name="parallelization">Parallelization must be a positive integer less than or equal to Integer.MAX_VALUE / (128 * blockSize* 8)</param>
        /// <returns> true if SCrypt parameters are correct</returns>
        private bool areSCRyptValidParameters(int CPUCost, int blockSize, int parallelization)
        {
            if (blockSize < 1)
            {
                this.error.setError("PD005", "The blockSize must be >= 1");
                return false;
            }
            if (CPUCost < 2 || CPUCost >= Math.Pow(2, 128 * blockSize / 8) || !isPowerOfTwo(CPUCost))
            {
                this.error.setError("PD004", "CPUCost must be larger than 1, a power of 2 and less than 2^(128 * blockSize / 8)");
                return false;
            }
            if (parallelization <= 0 || parallelization > int.MaxValue / (128 * blockSize * 8))
            {
                this.error.setError("PD003", "Parallelization must be a positive integer less than or equal to Integer.MAX_VALUE / (128 * blockSize * 8)");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number">int number to test</param>
        /// <returns>true if number is power of 2</returns>
        private static bool isPowerOfTwo(int number)
        {
            return number > 0 && ((number & (number - 1)) == 0);
        }

    }
}
