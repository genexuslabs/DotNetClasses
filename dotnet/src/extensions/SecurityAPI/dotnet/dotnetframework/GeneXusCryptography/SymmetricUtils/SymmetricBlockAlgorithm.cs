
using SecurityAPICommons.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;


namespace GeneXusCryptography.SymmetricUtils
{
    /// <summary>
    /// Implements SymmetricBlockAlgorithm enumerated
    /// </summary>
    [SecuritySafeCritical]
    public enum SymmetricBlockAlgorithm
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        NONE, AES, BLOWFISH, CAMELLIA, CAST5, CAST6, DES, TRIPLEDES, DSTU7624_128, DSTU7624_256, DSTU7624_512, GOST28147, NOEKEON, RC2, RC532, RC564, RC6, RIJNDAEL_128, RIJNDAEL_160, RIJNDAEL_192, RIJNDAEL_224, RIJNDAEL_256, SEED, SERPENT, SKIPJACK, SM4, TEA, THREEFISH_256, THREEFISH_512, THREEFISH_1024, TWOFISH, XTEA
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Implements SymmetricBlockAlgorithm associated functions
    /// </summary>
    [SecuritySafeCritical]
    public class SymmetricBlockAlgorithmUtils
    {
        /// <summary>
        /// Mapping between string name and SymmetricBlockAlgorithm enum representation
        /// </summary>
        /// <param name="symmetricBlockAlgorithm">string symmetricBlockAlgorithm</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>SymmetricBlockAlgorithm enum representaton</returns>
        public static SymmetricBlockAlgorithm getSymmetricBlockAlgorithm(string symmetricBlockAlgorithm, Error error)
        {
            switch (symmetricBlockAlgorithm.ToUpper().Trim())
            {
                case "AES":
                    return SymmetricBlockAlgorithm.AES;
                case "BLOWFISH":
                    return SymmetricBlockAlgorithm.BLOWFISH;
                case "CAMELLIA":
                    return SymmetricBlockAlgorithm.CAMELLIA;
                case "CAST5":
                    return SymmetricBlockAlgorithm.CAST5;
                case "CAST6":
                    return SymmetricBlockAlgorithm.CAST6;
                case "DES":
                    return SymmetricBlockAlgorithm.DES;
                case "TRIPLEDES":
                    return SymmetricBlockAlgorithm.TRIPLEDES;
                case "DSTU7624_128":
                    return SymmetricBlockAlgorithm.DSTU7624_128;
                case "DSTU7624_256":
                    return SymmetricBlockAlgorithm.DSTU7624_256;
                case "DSTU7624_512":
                    return SymmetricBlockAlgorithm.DSTU7624_512;
                case "GOST28147":
                    return SymmetricBlockAlgorithm.GOST28147;
                case "NOEKEON":
                    return SymmetricBlockAlgorithm.NOEKEON;
                case "RC2":
                    return SymmetricBlockAlgorithm.RC2;
                case "RC6":
                    return SymmetricBlockAlgorithm.RC6;
                case "RC532":
                    return SymmetricBlockAlgorithm.RC532;
                case "RC564":
                    return SymmetricBlockAlgorithm.RC564;
                case "RIJNDAEL_128":
                    return SymmetricBlockAlgorithm.RIJNDAEL_128;
                case "RIJNDAEL_160":
                    return SymmetricBlockAlgorithm.RIJNDAEL_160;
                case "RIJNDAEL_192":
                    return SymmetricBlockAlgorithm.RIJNDAEL_192;
                case "RIJNDAEL_224":
                    return SymmetricBlockAlgorithm.RIJNDAEL_224;
                case "RIJNDAEL_256":
                    return SymmetricBlockAlgorithm.RIJNDAEL_256;
                case "SEED":
                    return SymmetricBlockAlgorithm.SEED;
                case "SERPENT":
                    return SymmetricBlockAlgorithm.SERPENT;
                case "SKIPJACK":
                    return SymmetricBlockAlgorithm.SKIPJACK;
                case "SM4":
                    return SymmetricBlockAlgorithm.SM4;
                case "THREEFISH_256":
                    return SymmetricBlockAlgorithm.THREEFISH_256;
                case "THREEFISH_512":
                    return SymmetricBlockAlgorithm.THREEFISH_512;
                case "THREEFISH_1024":
                    return SymmetricBlockAlgorithm.THREEFISH_1024;
                case "TWOFISH":
                    return SymmetricBlockAlgorithm.TWOFISH;
                case "XTEA":
                    return SymmetricBlockAlgorithm.XTEA;
                case "TEA":
                    return SymmetricBlockAlgorithm.TEA;
                default:
                    error.setError("SB001", "Unrecognized SymmetricBlockAlgorithm");
                    return SymmetricBlockAlgorithm.NONE;
            }

        }


        /// <summary>
        /// Mapping between SymmetricBlockAlgorithm enum representation and string name
        /// </summary>
        /// <param name="symmetricBlockAlgorithm">SymmetricBlockAlgorithm enum, algorithm name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>value of SymmetricBlockAlgorithm in string</returns>
        public static string valueOf(SymmetricBlockAlgorithm symmetricBlockAlgorithm, Error error)
        {
            switch (symmetricBlockAlgorithm)
            {
                case SymmetricBlockAlgorithm.AES:
                    return "AES";
                case SymmetricBlockAlgorithm.BLOWFISH:
                    return "BLOWFISH";
                case SymmetricBlockAlgorithm.CAMELLIA:
                    return "CAMELLIA";
                case SymmetricBlockAlgorithm.CAST5:
                    return "CAST5";
                case SymmetricBlockAlgorithm.CAST6:
                    return "CAST6";
                case SymmetricBlockAlgorithm.DES:
                    return "DES";
                case SymmetricBlockAlgorithm.TRIPLEDES:
                    return "TRIPLEDES";
                case SymmetricBlockAlgorithm.DSTU7624_128:
                    return "DSTU7624_128";
                case SymmetricBlockAlgorithm.DSTU7624_256:
                    return "DSTU7624_256";
                case SymmetricBlockAlgorithm.DSTU7624_512:
                    return "DSTU7624_512";
                case SymmetricBlockAlgorithm.GOST28147:
                    return "GOST28147";
                case SymmetricBlockAlgorithm.NOEKEON:
                    return "NOEKEON";
                case SymmetricBlockAlgorithm.RC2:
                    return "RC2";
                case SymmetricBlockAlgorithm.RC6:
                    return "RC6";
                case SymmetricBlockAlgorithm.RC532:
                    return "RC532";
                case SymmetricBlockAlgorithm.RC564:
                    return "RC564";
                case SymmetricBlockAlgorithm.RIJNDAEL_128:
                    return "RIJNDAEL_128";
                case SymmetricBlockAlgorithm.RIJNDAEL_160:
                    return "RIJNDAEL_160";
                case SymmetricBlockAlgorithm.RIJNDAEL_192:
                    return "RIJNDAEL_192";
                case SymmetricBlockAlgorithm.RIJNDAEL_224:
                    return "RIJNDAEL_224";
                case SymmetricBlockAlgorithm.RIJNDAEL_256:
                    return "RIJNDAEL_256";
                case SymmetricBlockAlgorithm.SEED:
                    return "SEED";
                case SymmetricBlockAlgorithm.SERPENT:
                    return "SERPENT";
                case SymmetricBlockAlgorithm.SKIPJACK:
                    return "SKIPJACK";
                case SymmetricBlockAlgorithm.SM4:
                    return "SM4";
                case SymmetricBlockAlgorithm.THREEFISH_256:
                    return "THREEFISH_256";
                case SymmetricBlockAlgorithm.THREEFISH_512:
                    return "THREEFISH_512";
                case SymmetricBlockAlgorithm.THREEFISH_1024:
                    return "THREEFISH_1024";
                case SymmetricBlockAlgorithm.TWOFISH:
                    return "TWOFISH";
                case SymmetricBlockAlgorithm.XTEA:
                    return "XTEA";
                case SymmetricBlockAlgorithm.TEA:
                    return "TEA";
                default:
                    error.setError("SB002", "Unrecognized SymmetricBlockAlgorithm");
                    return "SymmetricBlockAlgorithm";
            }
        }

        /// <summary>
        /// Gets the block size lenght for the given algorithm
        /// </summary>
        /// <param name="algorithm">SymmetricBlockAlgorithm enum, algorithm name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>the specific block size for the algorithm, algorithm unknown if 0</returns>
        public static int getBlockSize(SymmetricBlockAlgorithm algorithm, Error error)
        {
            switch (algorithm)
            {

                case SymmetricBlockAlgorithm.BLOWFISH:
                case SymmetricBlockAlgorithm.CAST5:
                case SymmetricBlockAlgorithm.DES:
                case SymmetricBlockAlgorithm.GOST28147:
                case SymmetricBlockAlgorithm.RC2:
                case SymmetricBlockAlgorithm.RC532:
                case SymmetricBlockAlgorithm.SKIPJACK:
                case SymmetricBlockAlgorithm.XTEA:
                case SymmetricBlockAlgorithm.TRIPLEDES:
                case SymmetricBlockAlgorithm.TEA:
                    return 64;
                case SymmetricBlockAlgorithm.AES:
                case SymmetricBlockAlgorithm.CAMELLIA:
                case SymmetricBlockAlgorithm.CAST6:
                case SymmetricBlockAlgorithm.NOEKEON:
                case SymmetricBlockAlgorithm.RC564:
                case SymmetricBlockAlgorithm.RC6:
                case SymmetricBlockAlgorithm.SEED:
                case SymmetricBlockAlgorithm.SERPENT:
                case SymmetricBlockAlgorithm.SM4:
                case SymmetricBlockAlgorithm.TWOFISH:
                case SymmetricBlockAlgorithm.DSTU7624_128:
                case SymmetricBlockAlgorithm.RIJNDAEL_128:
                    return 128;
                case SymmetricBlockAlgorithm.RIJNDAEL_160:
                    return 160;
                case SymmetricBlockAlgorithm.RIJNDAEL_192:
                    return 192;
                case SymmetricBlockAlgorithm.RIJNDAEL_224:
                    return 224;
                case SymmetricBlockAlgorithm.DSTU7624_256:
                case SymmetricBlockAlgorithm.RIJNDAEL_256:
                case SymmetricBlockAlgorithm.THREEFISH_256:
                    return 256;
                case SymmetricBlockAlgorithm.DSTU7624_512:
                case SymmetricBlockAlgorithm.THREEFISH_512:
                    return 512;
                case SymmetricBlockAlgorithm.THREEFISH_1024:
                    return 1024;
                default:
                    error.setError("SB003", "Unrecognized SymmetricBlockAlgorithm");
                    return 0;
            }
        }

        /// <summary>
        /// Gets the key lenght for the given algorithm
        /// </summary>
        /// <param name="algorithm">SymmetricBlockAlgorithm enum, algorithm name</param>
        /// <param name="error">Error type for error management</param>
        /// <returns>array int with fixed length 3 with key, if array[0]=0 is range, else fixed values</returns>
        public static int[] getKeySize(SymmetricBlockAlgorithm algorithm, Error error)
        {
            int[] keySize = new int[3];

            switch (algorithm)
            {

                case SymmetricBlockAlgorithm.BLOWFISH:
                    keySize[0] = 0;
                    keySize[1] = 448;
                    break;
                case SymmetricBlockAlgorithm.CAMELLIA:
                case SymmetricBlockAlgorithm.SERPENT:
                case SymmetricBlockAlgorithm.TWOFISH:
                    keySize[0] = 128;
                    keySize[1] = 192;
                    keySize[2] = 256;
                    break;
                case SymmetricBlockAlgorithm.AES:
                case SymmetricBlockAlgorithm.CAST6:
                case SymmetricBlockAlgorithm.RC6:
                case SymmetricBlockAlgorithm.RIJNDAEL_128:
                case SymmetricBlockAlgorithm.RIJNDAEL_160:
                case SymmetricBlockAlgorithm.RIJNDAEL_192:
                case SymmetricBlockAlgorithm.RIJNDAEL_224:
                case SymmetricBlockAlgorithm.RIJNDAEL_256:
                    keySize[0] = 0;
                    keySize[1] = 256;
                    break;
                case SymmetricBlockAlgorithm.DES:
                    keySize[0] = 64;
                    break;
                case SymmetricBlockAlgorithm.TRIPLEDES:
                    keySize[0] = 128;
                    keySize[1] = 192;
                    break;
                case SymmetricBlockAlgorithm.DSTU7624_128:
                case SymmetricBlockAlgorithm.DSTU7624_256:
                case SymmetricBlockAlgorithm.DSTU7624_512:
                    keySize[0] = 128;
                    keySize[1] = 256;
                    keySize[2] = 512;
                    break;
                case SymmetricBlockAlgorithm.GOST28147:
                    keySize[0] = 256;
                    break;
                case SymmetricBlockAlgorithm.NOEKEON:
                case SymmetricBlockAlgorithm.SEED:
                case SymmetricBlockAlgorithm.SM4:
                case SymmetricBlockAlgorithm.XTEA:
                case SymmetricBlockAlgorithm.TEA:
                    keySize[0] = 128;
                    break;
                case SymmetricBlockAlgorithm.RC2:
                    keySize[0] = 0;
                    keySize[1] = 1024;
                    break;
                case SymmetricBlockAlgorithm.RC532:
                case SymmetricBlockAlgorithm.RC564:
                case SymmetricBlockAlgorithm.SKIPJACK:
                case SymmetricBlockAlgorithm.CAST5:
                    keySize[0] = 0;
                    keySize[1] = 128;
                    break;
                case SymmetricBlockAlgorithm.THREEFISH_256:
                case SymmetricBlockAlgorithm.THREEFISH_512:
                case SymmetricBlockAlgorithm.THREEFISH_1024:
                    keySize[0] = 128;
                    keySize[1] = 512;
                    keySize[2] = 1024;
                    break;
                    /*default:
                        error.setError("SB004", "Unrecognized SymmetricBlockAlgorithm");*/
            }
            return keySize;
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="SymmetricBlockAlgorithm">SymmetricBlockAlgorithm enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<SymmetricBlockAlgorithm> GetValues<SymmetricBlockAlgorithm>()
        {
            return Enum.GetValues(typeof(SymmetricBlockAlgorithm)).Cast<SymmetricBlockAlgorithm>();
        }
    }
}
