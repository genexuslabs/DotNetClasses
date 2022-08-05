
using GeneXusCryptography.Commons;
using GeneXusCryptography.SymmetricUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
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
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricStreamAlgorithm", symmetricStreamAlgorithm, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			EncodingUtil eu = new EncodingUtil();
			byte[] input = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}

			byte[] encryptedBytes = setUp(symmetricStreamAlgorithm, key, IV, input, true);
			if (this.HasError()) { return null; }


			return Base64.ToBase64String(encryptedBytes);

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
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("symmetricStreamAlgorithm", symmetricStreamAlgorithm, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("encryptedInput", encryptedInput, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			byte[] input = null;
			try
			{
				input = Base64.Decode(encryptedInput);
			}
			catch (Exception e)
			{
				this.error.setError("SS001", e.Message);
				return "";
			}

			byte[] decryptedBytes = setUp(symmetricStreamAlgorithm, key, IV, input, false);
			if (this.HasError()) { return null; }

			EncodingUtil eu = new EncodingUtil();
			String result = eu.getString(decryptedBytes);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}
			return result.Trim();
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

		private byte[] setUp(String symmetricStreamAlgorithm, string key, string IV, byte[] input, bool toEncrypt)
		{
			byte[] keyBytes = SecurityUtils.HexaToByte(key, this.error);
			byte[] ivBytes = SecurityUtils.HexaToByte(IV, this.error);
			SymmetricStreamAlgorithm algorithm = SymmetricStreamAlgorithmUtils
					.getSymmetricStreamAlgorithm(symmetricStreamAlgorithm, this.error);
			if (this.HasError()) { return null; }

			return encrypt(algorithm, keyBytes, ivBytes, input, toEncrypt);

		}

		private byte[] encrypt(SymmetricStreamAlgorithm algorithm, byte[] key, byte[] IV, byte[] input, bool toEncrypt)
		{
			IStreamCipher engine = getCipherEngine(algorithm);
			if (this.HasError()) { return null; }


			KeyParameter keyParam = new KeyParameter(key);

			try
			{
				if (SymmetricStreamAlgorithmUtils.usesIV(algorithm, this.error))
				{
					ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, IV);
					engine.Init(toEncrypt, keyParamWithIV);
				}
				else
				{
					engine.Init(toEncrypt, keyParam);
				}
			}
			catch (Exception e)
			{
				this.error.setError("SS003", e.Message);
				return null;
			}


			byte[] output = new byte[input.Length];
			try
			{
				engine.ProcessBytes(input, 0, input.Length, output, 0);
			}
			catch (Exception e)
			{
				this.error.setError("SS004", e.Message);
				return null;
			}
			return output;
		}

	}
}
