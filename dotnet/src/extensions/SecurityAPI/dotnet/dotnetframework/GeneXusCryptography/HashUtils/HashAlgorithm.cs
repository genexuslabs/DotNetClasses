
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;



namespace GeneXusCryptography.HashUtils
{
    /// <summary>
    /// Implements HashAlgorithm enumerated
    /// </summary>
    [SecuritySafeCritical]
    public enum HashAlgorithm
    {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1707 // Identifiers should not contain underscores
		NONE, MD5, SHA1, SHA224, SHA256, SHA384, SHA512, BLAKE2B_224, BLAKE2B_256, BLAKE2B_384, BLAKE2B_512, BLAKE2S_128, BLAKE2S_160, BLAKE2S_224, BLAKE2S_256, GOST3411_2012_256, GOST3411_2012_512, GOST3411, KECCAK_224, KECCAK_256, KECCAK_288, KECCAK_384, KECCAK_512, MD2, MD4, RIPEMD128, RIPEMD160, RIPEMD256, RIPEMD320, SHA3_224, SHA3_256, SHA3_384, SHA3_512, SHAKE_128, SHAKE_256, SM3, TIGER, WHIRLPOOL
#pragma warning restore CA1707 // Identifiers should not contain underscores
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}
    /// <summary>
    /// Implements HashAlgorithm associated functions
    /// </summary>
    [SecuritySafeCritical]
    public static class HashAlgorithmUtils
    {

        /// <summary>
        /// Mapping between string name and HashAlgorithm enum representation
        /// </summary>
        /// <param name="hashAlgorithm">string hashAlgorithm</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>HashAlgorithm enum representation</returns>
        public static HashAlgorithm getHashAlgorithm(string hashAlgorithm, Error error)
		{
			if(error == null) return HashAlgorithm.NONE; 
			if (hashAlgorithm == null)
			{
				error.setError("HA001", "Unrecognized HashAlgorihm");
				return HashAlgorithm.NONE;
			}
            switch (hashAlgorithm.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Trim())
            {
                case "MD5":
                    return HashAlgorithm.MD5;
                case "SHA1":
                    return HashAlgorithm.SHA1;
                case "SHA224":
                    return HashAlgorithm.SHA224;
                case "SHA256":
                    return HashAlgorithm.SHA256;
                case "SHA384":
                    return HashAlgorithm.SHA384;
                case "SHA512":
                    return HashAlgorithm.SHA512;
                case "BLAKE2B_224":
                    return HashAlgorithm.BLAKE2B_224;
                case "BLAKE2B_256":
                    return HashAlgorithm.BLAKE2B_256;
                case "BLAKE2B_384":
                    return HashAlgorithm.BLAKE2B_384;
                case "BLAKE2B_512":
                    return HashAlgorithm.BLAKE2B_512;
                case "BLAKE2S_128":
                    return HashAlgorithm.BLAKE2S_128;
                case "BLAKE2S_160":
                    return HashAlgorithm.BLAKE2S_160;
                case "BLAKE2S_224":
                    return HashAlgorithm.BLAKE2S_224;
                case "BLAKE2S_256":
                    return HashAlgorithm.BLAKE2S_256;
                case "GOST3411_2012_256":
                    return HashAlgorithm.GOST3411_2012_256;
                case "GOST3411_2012_512":
                    return HashAlgorithm.GOST3411_2012_512;
                case "GOST3411":
                    return HashAlgorithm.GOST3411;
                case "KECCAK_224":
                    return HashAlgorithm.KECCAK_224;
                case "KECCAK_256":
                    return HashAlgorithm.KECCAK_256;
                case "KECCAK_288":
                    return HashAlgorithm.KECCAK_288;
                case "KECCAK_384":
                    return HashAlgorithm.KECCAK_384;
                case "KECCAK_512":
                    return HashAlgorithm.KECCAK_512;
                case "MD2":
                    return HashAlgorithm.MD2;
                case "MD4":
                    return HashAlgorithm.MD4;
                case "RIPEMD128":
                    return HashAlgorithm.RIPEMD128;
                case "RIPEMD160":
                    return HashAlgorithm.RIPEMD160;
                case "RIPEMD256":
                    return HashAlgorithm.RIPEMD256;
                case "RIPEMD320":
                    return HashAlgorithm.RIPEMD320;
                case "SHA3-224":
                    return HashAlgorithm.SHA3_224;
                case "SHA3-256":
                    return HashAlgorithm.SHA3_256;
                case "SHA3-384":
                    return HashAlgorithm.SHA3_384;
                case "SHA3-512":
                    return HashAlgorithm.SHA3_512;
                case "SHAKE_128":
					error.setError("HA003", "Not implemented algorithm SHAKE_128");
					return HashAlgorithm.NONE;
				//return HashAlgorithm.SHAKE_128;
				case "SHAKE_256":
					error.setError("HA004", "Not implemented algorithm SHAKE_256");
					return HashAlgorithm.NONE;
				// return HashAlgorithm.SHAKE_256;
				case "SM3":
                    return HashAlgorithm.SM3;
                case "TIGER":
                    return HashAlgorithm.TIGER;
                case "WHIRLPOOL":
                    return HashAlgorithm.WHIRLPOOL;
                default:
                    error.setError("HA001", "Unrecognized HashAlgorihm");
                    return HashAlgorithm.NONE;
            }
        }

        /// <summary>
        /// Mapping between HashAlgorithm enum representation and string name
        /// </summary>
        /// <param name="hashAlgorithm">HashAlgorithm enum, algorithm name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>string name value of HashAlgorithm</returns>
        public static string valueOf(HashAlgorithm hashAlgorithm, Error error)
        {
			if(error == null) return "Unrecognized algorithm";
			switch (hashAlgorithm)
            {
                case HashAlgorithm.MD5:
                    return "MD5";
                case HashAlgorithm.SHA1:
                    return "SHA1";
                case HashAlgorithm.SHA224:
                    return "SHA224";
                case HashAlgorithm.SHA256:
                    return "SHA256";
                case HashAlgorithm.SHA384:
                    return "SHA384";
                case HashAlgorithm.SHA512:
                    return "SHA512";
                case HashAlgorithm.BLAKE2B_224:
                    return "BLAKE2B_224";
                case HashAlgorithm.BLAKE2B_256:
                    return "BLAKE2B_256";
                case HashAlgorithm.BLAKE2B_384:
                    return "BLAKE2B_384";
                case HashAlgorithm.BLAKE2B_512:
                    return "BLAKE2B_512";
                case HashAlgorithm.BLAKE2S_128:
                    return "BLAKE2S_128";
                case HashAlgorithm.BLAKE2S_160:
                    return "BLAKE2S_160";
                case HashAlgorithm.BLAKE2S_224:
                    return "BLAKE2S_224";
                case HashAlgorithm.BLAKE2S_256:
                    return "BLAKE2S_256";
                case HashAlgorithm.GOST3411_2012_256:
                    return "GOST3411_2012_256";
                case HashAlgorithm.GOST3411_2012_512:
                    return "GOST3411_2012_512";
                case HashAlgorithm.GOST3411:
                    return "GOST3411";
                case HashAlgorithm.KECCAK_224:
                    return "KECCAK_224";
                case HashAlgorithm.KECCAK_256:
                    return "KECCAK_256";
                case HashAlgorithm.KECCAK_288:
                    return "KECCAK_288";
                case HashAlgorithm.KECCAK_384:
                    return "KECCAK_384";
                case HashAlgorithm.KECCAK_512:
                    return "KECCAK_512";
                case HashAlgorithm.MD2:
                    return "MD2";
                case HashAlgorithm.MD4:
                    return "MD4";
                case HashAlgorithm.RIPEMD128:
                    return "RIPEMD128";
                case HashAlgorithm.RIPEMD160:
                    return "RIPEMD160";
                case HashAlgorithm.RIPEMD256:
                    return "RIPEMD256";
                case HashAlgorithm.RIPEMD320:
                    return "RIPEMD320";
                case HashAlgorithm.SHA3_224:
                    return "SHA3_224";
                case HashAlgorithm.SHA3_256:
                    return "SHA3_256";
                case HashAlgorithm.SHA3_384:
                    return "SHA3_384";
                case HashAlgorithm.SHA3_512:
                    return "SHA3_512";
               /* case HashAlgorithm.SHAKE_128:
                    return "SHAKE_128";
                case HashAlgorithm.SHAKE_256:
                    return "SHAKE_256";*/
                case HashAlgorithm.SM3:
                    return "SM3";
                case HashAlgorithm.TIGER:
                    return "TIGER";
                case HashAlgorithm.WHIRLPOOL:
                    return "WHIRLPOOL";
                default:
                    error.setError("HA002", "Unrecognized HashAlgorihm");
                    return "Unrecognized algorithm";
            }
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="HashAlgorithm">HashAlgorithm enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<HashAlgorithm> GetValues<HashAlgorithm>()
        {
            return Enum.GetValues(typeof(HashAlgorithm)).Cast<HashAlgorithm>();
        }
    }
}
