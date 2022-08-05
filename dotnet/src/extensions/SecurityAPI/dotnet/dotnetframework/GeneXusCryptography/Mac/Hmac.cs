using System;
using System.Security;
using SecurityAPICommons.Commons;
using GeneXusCryptography.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Utilities.Encoders;
using GeneXusCryptography.Hash;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Macs;
using GeneXusCryptography.HashUtils;
using SecurityAPICommons.Utils;
using System.IO;

namespace GeneXusCryptography.Mac
{
	[SecuritySafeCritical]
	public class Hmac : SecurityAPIObject, IHmacObject
	{

		public Hmac() : base()
		{

		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/
		[SecuritySafeCritical]
		public string calculate(string plainText, string password, string algorithm)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			SecurityUtils.validateStringInput("password", password, this.error);
			SecurityUtils.validateStringInput("algorithm", algorithm, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			byte[] pass = SecurityUtils.HexaToByte(password, this.error);
			HashUtils.HashAlgorithm hashAlgorithm = HashAlgorithmUtils.getHashAlgorithm(algorithm, this.error);
			Stream input = SecurityUtils.StringToStream(plainText, this.error);
			if (this.HasError()) { return ""; }
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
				resBytes = calculate(inputStream, pass, hashAlgorithm);
			}

			return this.HasError() ? "" : Hex.ToHexString(resBytes);
		}

		[SecuritySafeCritical]
		public bool verify(string plainText, string password, string mac, string algorithm)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			SecurityUtils.validateStringInput("password", password, this.error);
			SecurityUtils.validateStringInput("algorithm", algorithm, this.error);
			SecurityUtils.validateStringInput("mac", mac, this.error);
			if (this.HasError()) { return false; };
			/*******INPUT VERIFICATION - END*******/

			string res = calculate(plainText, password, algorithm);
			return SecurityUtils.compareStrings(res, mac);
		}
		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

		private byte[] calculate(Stream input, byte[] password, HashUtils.HashAlgorithm algorithm)
		{

			IDigest digest = new Hashing().createHash(algorithm);
			if (this.HasError()) { return null; }

			HMac engine = new HMac(digest);
			try
			{
				engine.Init(new KeyParameter(password));
			}
			catch (Exception e)
			{
				this.error.setError("HM001", e.Message);
				return null;
			}

			byte[] buffer = new byte[8192];
			int n;
			byte[] retValue = new byte[engine.GetMacSize()];
			try
			{
				while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					engine.BlockUpdate(buffer, 0, n);
				}
				engine.DoFinal(retValue, 0);
			}
			catch (Exception e)
			{
				this.error.setError("HM002", e.Message);
				return null;
			}
			return retValue;
		}
	}
}
