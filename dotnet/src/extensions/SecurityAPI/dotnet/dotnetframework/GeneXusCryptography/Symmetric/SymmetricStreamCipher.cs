
using GeneXusCryptography.Commons;
using GeneXusCryptography.SymmetricUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using System;

using System.Security;
using SecurityAPICommons.Utils;

namespace GeneXusCryptography.Symmetric
{
    /// <summary>
    /// Implements Symmetric Stream Cipher engines and the methods to encrypt and decrypt strings
    /// </summary>
    [SecuritySafeCritical]
    public class SymmetricStreamCipher : SecurityAPIObject, ISymmectricStreamCipherObject
    {


        public SymmetricStreamCipher() : base()
        {

        }


        /********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/



        /// <summary>
        /// Encrypts the given text with a stream encryption algorithm
        /// </summary>
        /// <param name="symmetricStreamAlgorithm">string SymmetrcStreamAlgorithm enum, algorithm name</param>
        /// <param name="key">string SymmetricBlockMode enum, mode name</param>
        ///  <param name="IV">String Hexa IV (nonce) for those algorithms that uses, ignored if not</param>
        /// <param name="plainText">string UTF-8 plain text to encrypt</param>
        /// <returns>string Base64 encrypted text with the given algorithm and parameters</returns>
        [SecuritySafeCritical]
        public string DoEncrypt(string symmetricStreamAlgorithm, string key, string IV,
        string plainText)
        {
            this.GetError().cleanError();
            SymmetricStreamAlgorithm algorithm = SymmetricStreamAlgorithmUtils.getSymmetricStreamAlgorithm(symmetricStreamAlgorithm, this.GetError());
            if (this.GetError().existsError())
            {
                return "";
            }

            IStreamCipher engine = getCipherEngine(algorithm);
            if (this.GetError().existsError())
            {
                return "";
            }
			/* KeyParameter keyParam = new KeyParameter(Hex.Decode(key));
             engine.Init(true, keyParam);*/
			byte[] keyBytes = SecurityUtils.GetHexa(key, "SS007", this.error);
			byte[] ivBytes = SecurityUtils.GetHexa(IV, "SS007", this.error);
			if (this.HasError())
			{
				return "";
			}
			KeyParameter keyParam = new KeyParameter(keyBytes);
            if (SymmetricStreamAlgorithmUtils.usesIV(algorithm, this.GetError()))
            {
                if (!this.GetError().existsError())
                {
                    ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, ivBytes);
					try
					{
						engine.Init(false, keyParamWithIV);
					}catch(Exception e)
					{
						this.error.setError("SS008", e.Message);
						return "";
					}
                }
            }
            else
            {
				try
				{
					engine.Init(false, keyParam);
				}catch(Exception e)
				{
					this.error.setError("SS009", e.Message);
					return "";
				}
            }
            EncodingUtil eu = new EncodingUtil();
            byte[] input = eu.getBytes(plainText);
            if (eu.GetError().existsError())
            {
                this.error = eu.GetError();
                return "";
            }
            byte[] output = new byte[input.Length];
            engine.ProcessBytes(input, 0, input.Length, output, 0);
            if (output == null || output.Length == 0)
            {
                this.GetError().setError("SS004", "Stream encryption exception");
                return "";
            }
            this.GetError().cleanError();
            return Base64.ToBase64String(output);

        }

        /// <summary>
        /// Decrypts the given encrypted text with a stream encryption algorithm
        /// </summary>
        /// <param name="symmetricStreamAlgorithm">string SymmetrcStreamAlgorithm enum, algorithm name</param>
        /// <param name="key">string SymmetricBlockMode enum, mode name</param>
        /// <param name="IV">String Hexa IV (nonce) for those algorithms that uses, ignored if not</param>
        /// <param name="encryptedInput">string Base64 encrypted text with the given algorithm and parameters</param>
        /// <returns>plain text UTF-8</returns>
        [SecuritySafeCritical]
        public string DoDecrypt(string symmetricStreamAlgorithm, string key, string IV,
            string encryptedInput)
        {
            this.GetError().cleanError();
            SymmetricStreamAlgorithm algorithm = SymmetricStreamAlgorithmUtils.getSymmetricStreamAlgorithm(symmetricStreamAlgorithm, this.GetError());
            if (this.GetError().existsError())
            {
                return "";
            }

            IStreamCipher engine = getCipherEngine(algorithm);
            if (this.GetError().existsError())
            {
                return "";
            }

			/* KeyParameter keyParam = new KeyParameter(Hex.Decode(key));
             engine.Init(false, keyParam);*/
			byte[] keyBytes = SecurityUtils.GetHexa(key, "SS010", this.error);
			byte[] ivBytes = SecurityUtils.GetHexa(IV, "SS010", this.error);
			if (this.HasError())
			{
				return "";
			}
			KeyParameter keyParam = new KeyParameter(keyBytes);
            if (SymmetricStreamAlgorithmUtils.usesIV(algorithm, this.GetError()))
            {
                if (!this.GetError().existsError())
                {
                    ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, ivBytes);
					try
					{
						engine.Init(false, keyParamWithIV);
					}catch(Exception e)
					{
						this.error.setError("SS011", e.Message);
						return "";
					}
                }
            }
            else
            {
				try
				{
					engine.Init(false, keyParam);
				}catch(Exception e)
				{
					this.error.setError("SS012", e.Message);
					return "";
				}
            }
            byte[] input = Base64.Decode(encryptedInput);
            byte[] output = new byte[input.Length];
            engine.ProcessBytes(input, 0, input.Length, output, 0);
            if (output == null || output.Length == 0)
            {
                this.GetError().setError("SS006", "Stream decryption exception");
                return "";
            }
            this.GetError().cleanError();
            EncodingUtil eu = new EncodingUtil();
            this.error = eu.GetError();
            return eu.getString(output);
        }


        /********EXTERNAL OBJECT PUBLIC METHODS  - END ********/



        /// <summary>
        /// Buils the StreamCipher
        /// </summary>
        /// <param name="algorithm">SymmetrcStreamAlgorithm enum, algorithm name</param>
        /// <returns>IStreamCipher with the algorithm Stream Engine</returns>
        private IStreamCipher getCipherEngine(SymmetricStreamAlgorithm algorithm)
        {

            IStreamCipher engine = null;

            switch (algorithm)
            {
                case SymmetricStreamAlgorithm.RC4:
                    engine = new RC4Engine();
                    break;
                case SymmetricStreamAlgorithm.HC128:
                    engine = new HC128Engine();
                    break;
                case SymmetricStreamAlgorithm.HC256:
                    engine = new HC256Engine();
                    break;
                case SymmetricStreamAlgorithm.SALSA20:
                    engine = new Salsa20Engine();
                    break;
                case SymmetricStreamAlgorithm.CHACHA20:
                    engine = new ChaChaEngine();
                    break;
                case SymmetricStreamAlgorithm.XSALSA20:
                    engine = new XSalsa20Engine();
                    break;
                case SymmetricStreamAlgorithm.ISAAC:
                    engine = new IsaacEngine();
                    break;
                case SymmetricStreamAlgorithm.VMPC:
                    engine = new VmpcEngine();
                    break;
                default:
                    this.GetError().setError("SS005", "Cipher " + algorithm + " not recognised.");
                    break;
            }
            return engine;

        }

    }
}
