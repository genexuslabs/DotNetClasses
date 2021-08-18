

namespace GeneXusCryptography.Commons
{
    /// <summary>
    /// ISymmectricStreamCipherObject interface for EO
    /// </summary>
    public interface ISymmectricStreamCipherObject
    {
        /// <summary>
        /// Encrypts the given text with a stream encryption algorithm
        /// </summary>
        /// <param name="symmetricStreamAlgorithm">String SymmetrcStreamAlgorithm enum, algorithm name</param>
        /// <param name="key">String Hexa key for the algorithm excecution</param>
        /// <param name="IV">String Hexa IV (nonce) for those algorithms that uses, ignored if not</param>
        /// <param name="plainText">String UTF-8 plain text to encrypt</param>
        /// <returns>String Base64 encrypted text with the given algorithm and parameters</returns>
        string DoEncrypt(string symmetricStreamAlgorithm, string key, string IV, string plainText);

        /// <summary>
        /// Decrypts the given encrypted text with a stream encryption algorithm
        /// </summary>
        /// <param name="symmetricStreamAlgorithm">String SymmetrcStreamAlgorithm enum, algorithm name</param>
        /// <param name="key">String Hexa key for the algorithm excecution</param>
        /// <param name="IV">String Hexa IV (nonce) for those algorithms that uses, ignored if not</param>
        /// <param name="encryptedInput">String Base64 encrypted text with the given algorithm and parameters</param>
        /// <returns>plain text UTF-8</returns>
        string DoDecrypt(string symmetricStreamAlgorithm, string key, string IV, string encryptedInput);
    }
}
