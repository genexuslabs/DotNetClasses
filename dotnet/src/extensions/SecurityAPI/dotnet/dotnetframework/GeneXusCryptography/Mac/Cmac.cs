using System;
using System.Security;
using SecurityAPICommons.Commons;
using GeneXusCryptography.Commons;
using GeneXusCryptography.SymmetricUtils;
using GeneXusCryptography.Symmetric;
using Org.BouncyCastle.Crypto;
using SecurityAPICommons.Utils;
using Org.BouncyCastle.Utilities.Encoders;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Macs;
using System.IO;

namespace GeneXusCryptography.Mac
{
	[SecuritySafeCritical]
	public class Cmac : SecurityAPIObject, ICmacObject
	{
		public Cmac() : base()
		{

		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/


		[SecuritySafeCritical]
		public string calculate(string plainText, string key, string algorithm, int macSize)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("algorithm", algorithm, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/


			SymmetricBlockAlgorithm symmetricBlockAlgorithm = SymmetricBlockAlgorithmUtils.getSymmetricBlockAlgorithm(algorithm,
		this.error);

			byte[] byteKey = SecurityUtils.HexaToByte(key, this.error);
			if (this.HasError()) { return ""; }

			SymmetricBlockCipher symCipher = new SymmetricBlockCipher();
			IBlockCipher blockCipher = symCipher.getCipherEngine(symmetricBlockAlgorithm);

			if (symCipher.HasError())
			{
				this.error = symCipher.GetError();
				return "";
			}


			int blockSize = blockCipher.GetBlockSize() * 8;

			if (macSize > blockSize)
			{
				this.error.setError("CM001", "The mac length must be less or equal than the algorithm block size.");
				return "";
			}

			if (blockSize != 64 && blockSize != 128)
			{
				this.error.setError("CM002", "The block size must be 64 or 128 bits for CMAC. Wrong symmetric algorithm");
				return "";
			}
			EncodingUtil eu = new EncodingUtil();
			byte[] inputText = eu.getBytes(plainText);
			if (eu.HasError())
			{
				error = eu.GetError();
				return null;
			}
			byte[] resBytes = null;
			using (Stream inputStream = new MemoryStream(inputText))
			{
				resBytes = calculate(inputStream, byteKey, macSize, blockCipher);
			}


			return this.HasError() ? "" : Hex.ToHexString(resBytes);

		}


		[SecuritySafeCritical]
		public bool verify(string plainText, string key, string mac, string algorithm, int macSize)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			SecurityUtils.validateStringInput("key", key, this.error);
			SecurityUtils.validateStringInput("mac", mac, this.error);
			SecurityUtils.validateStringInput("algorithm", algorithm, this.error);
			if (this.HasError()) { return false; };
			/*******INPUT VERIFICATION - END*******/

			string res = calculate(plainText, key, algorithm, macSize);
			return SecurityUtils.compareStrings(res, mac);
		}


		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/


		private byte[] calculate(Stream input, byte[] key, int macSize, IBlockCipher blockCipher)
		{
			ICipherParameters parms = new KeyParameter(key);

			CMac mac = macSize != 0 ? new CMac(blockCipher, macSize) : new CMac(blockCipher);

			try
			{
				mac.Init(parms);
			}
			catch (Exception e)
			{
				this.error.setError("CM003", e.Message);
				return null;
			}

			byte[] buffer = new byte[8192];
			int n;
			byte[] retValue = new byte[mac.GetMacSize()];
			try
			{
				while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					mac.BlockUpdate(buffer, 0, n);
				}
				mac.DoFinal(retValue, 0);
			}
			catch (Exception e)
			{

				this.error.setError("CM004", e.Message);
				return null;
			}

			return retValue;

		}
	}
}
