


namespace GeneXusCryptography.Commons
{
    /// <summary>
    /// ISymmetricBlockCipherObject interface for EO
    /// </summary>
    public interface ISymmetricBlockCipherObject
    {
        /// <summary>
        /// Encrypts the given text with a block encryption algorithm
        /// </summary>
        /// <param name="symmetricBlockAlgorithm">string SymmetricBlockAlgorithm enum, symmetric block algorithm name</param>
        /// <param name="symmetricBlockMode">string SymmetricBlockModes enum, symmetric block mode name</param>
        /// <param name="symmetricBlockPadding">string SymmetricBlockPadding enum, symmetric block padding name</param>
        /// <param name="key">string Hexa key for the algorithm excecution</param>
        /// <param name="IV">string IV for the algorithm execution, must be the same length as the blockSize</param>
        /// <param name="plainText">string UTF-8 plain text to encrypt</param>
        /// <returns>string base64 encrypted text</returns>
        string DoEncrypt(string symmetricBlockAlgorithm, string symmetricBlockMode,
        string symmetricBlockPadding, string key, string IV, string plainText);

        /// <summary>
        /// Decrypts the given encrypted text with a block encryption algorithm
        /// </summary>
        /// <param name="symmetricBlockAlgorithm">string SymmetricBlockAlgorithm enum, symmetric block algorithm name</param>
        /// <param name="symmetricBlockMode">string SymmetricBlockModes enum, symmetric block mode name</param>
        /// <param name="symmetricBlockPadding">string SymmetricBlockPadding enum, symmetric block padding name</param>
        /// <param name="key">string Hexa key for the algorithm excecution</param>
        /// <param name="IV">string IV for the algorithm execution, must be the same length as the blockSize</param>
        /// <param name="encryptedInput">string Base64 text to decrypt</param>
        /// <returns>sting plaintext UTF-8</returns>
        string DoDecrypt(string symmetricBlockAlgorithm, string symmetricBlockMode,
                string symmetricBlockPadding, string key, string IV, string encryptedInput);
    }
}
