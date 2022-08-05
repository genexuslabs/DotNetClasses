
using GeneXusCryptography.Commons;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Security;
using SecurityAPICommons.Utils;

namespace GeneXusCryptography.PasswordDerivation
{
	/// <summary>
	/// Implements password derivation functions
	/// </summary>
	[SecuritySafeCritical]
	public class PasswordDerivation : SecurityAPIObject, IPasswordDerivationObject
	{


		/// <summary>
		/// PasswordDerivation class constructor
		/// </summary>
		public PasswordDerivation() : base()
		{

		}


		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/



		/// <summary>
		/// Hashing and salting of a password with scrypt algorithm
		/// </summary>
		/// <param name="password">string to hash</param>
		/// <param name="salt"> string to use as salt</param>
		/// <param name="CPUCost">CPUCost must be larger than 1, a power of 2 and less than 2^(128 *blockSize / 8)</param>
		/// <param name="blockSize">The blockSize must be >= 1</param>
		/// <param name="parallelization">Parallelization must be a positive integer less than or equal to Integer.MAX_VALUE / (128 * blockSize* 8)</param>
		/// <param name="keyLenght"> fixed key length</param>
		/// <returns>Base64 hashed result</returns>
		[SecuritySafeCritical]
		public string DoGenerateSCrypt(string password, string salt, int CPUCost, int blockSize, int parallelization,
		int keyLenght)
		{
			this.error.cleanError();

			EncodingUtil eu = new EncodingUtil();
			byte[] bytePassword = eu.getBytes(password);
			if (eu.HasError()) { this.error = eu.GetError(); }

			byte[] byteSalt = SecurityUtils.HexaToByte(salt, this.error);
			if (this.HasError()) { return ""; }

			byte[] encryptedBytes = null;
			try
			{
				encryptedBytes = SCrypt.Generate(bytePassword, byteSalt, CPUCost, blockSize,
						parallelization, keyLenght);
			}
			catch (Exception e)
			{
				this.error.setError("PD001", e.Message);
				return "";
			}
			return Base64.ToBase64String(encryptedBytes);
		}

		/// <summary>
		/// Calculates SCrypt digest with arbitrary fixed parameters: CPUCost (N) = 16384, blockSize(r) = 8, parallelization(p) = 1, keyLenght = 256
		/// </summary>
		/// <param name="password">string to hash</param>
		/// <param name="salt"> string to use as salt</param>
		/// <returns>Base64 string generated result</returns>
		[SecuritySafeCritical]
		public string DoGenerateDefaultSCrypt(string password, string salt)
		{
			int N = 16384;
			int r = 8;
			int p = 1;
			int keyLenght = 256;
			return DoGenerateSCrypt(password, salt, N, r, p, keyLenght);
		}

		/// <summary>
		/// Hashing and salting of a password with bcrypt algorithm
		/// </summary>
		/// <param name="password">string to hash. the password bytes (up to 72 bytes) to use for this invocation.</param>
		/// <param name="salt">string hexadecimal to salt. The salt lenght must be 128 bits</param>
		/// <param name="cost">	The cost of the bcrypt function grows as 2^cost. Legal values are 4..31 inclusive.</param>
		/// <returns>string Base64 hashed password to store</returns>
		[SecuritySafeCritical]
		public string DoGenerateBcrypt(string password, string salt, int cost)
		{
			this.error.cleanError();

			EncodingUtil eu = new EncodingUtil();
			byte[] bytePassword = eu.getBytes(password);
			if (eu.HasError()) { this.error = eu.GetError(); }

			byte[] byteSalt = SecurityUtils.HexaToByte(salt, this.error);
			if (this.HasError()) { return ""; }

			byte[] encryptedBytes = null;

			try
			{
				encryptedBytes = BCrypt.Generate(bytePassword, byteSalt, cost);
			}
			catch (Exception e)
			{
				this.error.setError("PD002", e.Message);
				return "";
			}

			return Base64.ToBase64String(encryptedBytes);
		}

		/// <summary>
		/// Calculates Bcrypt digest with arbitrary fixed cost parameter: cost = 6
		/// </summary>
		/// <param name="password">string to hash. the password bytes (up to 72 bytes) to use for this invocation.</param>
		/// <param name="salt">string to salt. The salt lenght must be 128 bits</param>
		/// <returns>string Base64 hashed password to store</returns>
		[SecuritySafeCritical]
		public string DoGenerateDefaultBcrypt(string password, string salt)
		{
			int cost = 6;
			return DoGenerateBcrypt(password, salt, cost);
		}

		[SecuritySafeCritical]
		public string DoGenerateArgon2(string argon2Version10, string argon2HashType, int iterations, int memory,
		int parallelism, String password, string salt, int hashLength)
		{
			this.error.setError("PD004", "Not implemented function for Net");
			return "";

		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

	}
}
