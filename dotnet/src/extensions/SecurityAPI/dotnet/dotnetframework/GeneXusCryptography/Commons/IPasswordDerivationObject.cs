

namespace GeneXusCryptography.Commons
{
    /// <summary>
    /// IPasswordDerivationObject interface for EO
    /// </summary>
    public interface IPasswordDerivationObject
    {


        /// <summary>
        /// Hashing and salting of a password with scrypt algorithm
        /// </summary>
        /// <param name="password">string UTF-8 to hash</param>
        /// <param name="salt"> string UTF-8 to use as salt</param>
        /// <param name="CPUCost">CPUCost must be larger than 1, a power of 2 and less than 2^(128 *blockSize / 8)</param>
        /// <param name="blockSize">The blockSize must be >= 1</param>
        /// <param name="parallelization">Parallelization must be a positive integer less than or equal to Integer.MAX_VALUE / (128 * blockSize* 8)</param>
        /// <param name="keyLenght"> fixed key length</param>
        /// <returns>Base64 hashed result</returns>
        string DoGenerateSCrypt(string password, string salt, int CPUCost, int blockSize,
        int parallelization, int keyLenght);

        /// <summary>
        /// Calculates SCrypt digest with arbitrary fixed parameters: CPUCost (N) = 16384, blockSize(r) = 8, parallelization(p) = 1, keyLenght = 256
        /// </summary>
        /// <param name="password">string UTF-8 to hash</param>
        /// <param name="salt"> string UTF-8 to use as salt</param>
        /// <returns>Base64 string generated result</returns>
        string DoGenerateDefaultSCrypt(string password, string salt);

        /// <summary>
        /// Hashing and salting of a password with bcrypt algorithm
        /// </summary>
        /// <param name="password">string UTF-8 to hash. the password bytes (up to 72 bytes) to use for this invocation.</param>
        /// <param name="salt">string UTF-8 to salt. The salt lenght must be 128 bits</param>
        /// <param name="cost">	The cost of the bcrypt function grows as 2^cost. Legal values are 4..31 inclusive.</param>
        /// <returns>string Base64 hashed password to store</returns>
        string DoGenerateBcrypt(string password, string salt, int cost);

        /// <summary>
        /// Calculates Bcrypt digest with arbitrary fixed cost parameter: cost = 6
        /// </summary>
        /// <param name="password">string UTF-8 to hash. the password bytes (up to 72 bytes) to use for this invocation.</param>
        /// <param name="salt">string UTF-8 to salt. The salt lenght must be 128 bits</param>
        /// <returns>string Base64 hashed password to store</returns>
        string DoGenerateDefaultBcrypt(string password, string salt);
    }
}
