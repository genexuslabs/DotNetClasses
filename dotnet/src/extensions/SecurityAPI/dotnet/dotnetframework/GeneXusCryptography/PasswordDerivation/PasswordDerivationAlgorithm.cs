
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace GeneXusCryptography.PasswordDerivation
{
    /// <summary>
    /// Implements PasswordDerivationAlgorithm enumerated
    /// </summary>
    [SecuritySafeCritical]
    public enum PasswordDerivationAlgorithm
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        NONE, SCrypt, Bcrypt, Argon2
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
    /// <summary>
    /// Implements PasswordDerivationAlgorithm associated functions
    /// </summary>
    [SecuritySafeCritical]
    public class PasswordDerivationAlgorithmUtils
    {
        /// <summary>
        /// Mapping between string name and PasswordDerivationAlgorithm enum representation
        /// </summary>
        /// <param name="passwordDerivationAlgorithm">string passwordDerivationAlgorithm</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>PasswordDerivationAlgorithm enum representation</returns>
        public static PasswordDerivationAlgorithm getPasswordDerivationAlgorithm(string passwordDerivationAlgorithm, Error error)
        {
            switch (passwordDerivationAlgorithm.Trim())
            {
                case "SCrypt":
                    return PasswordDerivationAlgorithm.SCrypt;
                case "Bcrypt":
                    return PasswordDerivationAlgorithm.Bcrypt;
                case "Argon2":
                    return PasswordDerivationAlgorithm.Argon2;
                default:
                    error.setError("PD001", "Unrecognized PasswordDerivationAlgorithm");
                    return PasswordDerivationAlgorithm.NONE;
            }
        }
        /// <summary>
        /// Mapping between and PasswordDerivationAlgorithm enum representation and string name
        /// </summary>
        /// <param name="passwordDerivationAlgorithm">PasswordDerivationAlgorithm enum, algorithm name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>PasswordDerivationAlgorithm value in string</returns>
        public static string valueOf(PasswordDerivationAlgorithm passwordDerivationAlgorithm, Error error)
        {
            switch (passwordDerivationAlgorithm)
            {
                case PasswordDerivationAlgorithm.SCrypt:
                    return "SCrypt";
                case PasswordDerivationAlgorithm.Bcrypt:
                    return "Bcrypt";
                case PasswordDerivationAlgorithm.Argon2:
                    return "Argon2";
                default:
                    error.setError("PD002", "Unrecognized PasswordDerivationAlgorithm");
                    return "Unrecognized algorithm";
            }
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="PasswordDerivationAlgorithm">PasswordDerivationAlgorithm enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<PasswordDerivationAlgorithm> GetValues<PasswordDerivationAlgorithm>()
        {
            return Enum.GetValues(typeof(PasswordDerivationAlgorithm)).Cast<PasswordDerivationAlgorithm>();
        }

    }
}
