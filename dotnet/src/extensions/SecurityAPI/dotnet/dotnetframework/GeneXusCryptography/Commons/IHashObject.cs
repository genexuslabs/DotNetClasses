
using System.Security;

namespace GeneXusCryptography.Commons
{
    /// <summary>
    /// IHashObject interface for EO
    /// </summary>

    [SecuritySafeCritical]
    public interface IHashObject
    {
        /// <summary>
        /// Calculate the hash digest with the given algorithm
        /// </summary>
        /// <param name="hashAlgorithm">String HashAlgorithm enum, algorithm name</param>
        /// <param name="txtToHash">plain text to hcalculate hash</param>
        /// <returns>String Hexa representation of the txtToHash with the algorithm indicated</returns>
        string DoHash(string hashAlgorithm, string txtToHash);
    }
}
