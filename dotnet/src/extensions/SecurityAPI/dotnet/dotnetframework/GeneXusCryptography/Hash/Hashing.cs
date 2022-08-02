
using GeneXusCryptography.Commons;
using GeneXusCryptography.HashUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Security;
using SecurityAPICommons.Utils;
using Org.BouncyCastle.Utilities.Encoders;
using System.IO;

namespace GeneXusCryptography.Hash
{
	/// <summary>
	/// Implements hashing engines to calculate string digests
	/// </summary>
	[SecuritySafeCritical]
	public class Hashing : SecurityAPIObject, IHashObject
	{

		/// <summary>
		/// Hashing constructor
		/// </summary>
		public Hashing() : base()
		{

		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/



		/// <summary>
		/// Calculate the hash digest with the given algorithm
		/// </summary>
		/// <param name="hashAlgorithm">string HashAlgorithm enum, algorithm name</param>
		/// <param name="txtToHash">plain text to hcalculate hash</param>
		/// <returns>string Hexa representation of the txtToHash with the algorithm indicated</returns>
		[SecuritySafeCritical]
		public string DoHash(string hashAlgorithm, string txtToHash)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput("txtToHash", txtToHash, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			HashAlgorithm hashAlgorithmObj = HashAlgorithmUtils.getHashAlgorithm(hashAlgorithm, this.error);
			EncodingUtil eu = new EncodingUtil();
			byte[] input = eu.getBytes(txtToHash);
			if (eu.HasError())
			{
				this.error = eu.GetError();
			}
			if (this.HasError()) { return null; }

			byte[] resBytes = CalculateHash(hashAlgorithmObj, input);

			return this.HasError() ? "" : Hex.ToHexString(resBytes).ToUpper();
		}



		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

		[SecuritySafeCritical]
		public byte[] CalculateHash(HashAlgorithm hashAlgorithm, byte[] input)
		{
			IDigest alg = createHash(hashAlgorithm);
			byte[] buffer = new byte[8192];
			int n;
			byte[] retValue = new byte[alg.GetDigestSize()];
			try
			{
				using (Stream inputStream = new MemoryStream(input))
				{
					while ((n = inputStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						alg.BlockUpdate(buffer, 0, n);
					}
				}
				alg.DoFinal(retValue, 0);

			}
			catch (Exception e)
			{
				error.setError("HA001", e.Message);
				return null;
			}
			return retValue;
		}





		/// <summary>
		/// Build the hash engine
		/// </summary>
		/// <param name="hashAlgorithm">HashAlgorithm enum, algorithm name</param>
		/// <returns>IDigest algorithm instantiated class</returns>
		internal IDigest createHash(HashAlgorithm hashAlgorithm)
		{
			switch (hashAlgorithm)
			{
				case HashAlgorithm.MD5:
					return new MD5Digest();
				case HashAlgorithm.SHA1:
					return new Sha1Digest();
				case HashAlgorithm.SHA224:
					return new Sha224Digest();
				case HashAlgorithm.SHA256:
					return new Sha256Digest();
				case HashAlgorithm.SHA384:
					return new Sha384Digest();
				case HashAlgorithm.SHA512:
					return new Sha512Digest();
				case HashAlgorithm.BLAKE2B_224:
					return new Blake2bDigest(224);
				case HashAlgorithm.BLAKE2B_256:
					return new Blake2bDigest(256);
				case HashAlgorithm.BLAKE2B_384:
					return new Blake2bDigest(384);
				case HashAlgorithm.BLAKE2B_512:
					return new Blake2bDigest(512);
				case HashAlgorithm.BLAKE2S_128:
					return new Blake2sDigest(128);
				case HashAlgorithm.BLAKE2S_160:
					return new Blake2sDigest(160);
				case HashAlgorithm.BLAKE2S_224:
					return new Blake2sDigest(224);
				case HashAlgorithm.BLAKE2S_256:
					return new Blake2sDigest(256);
				case HashAlgorithm.GOST3411_2012_256:
					return new Gost3411_2012_256Digest();
				case HashAlgorithm.GOST3411_2012_512:
					return new Gost3411_2012_512Digest();
				case HashAlgorithm.GOST3411:
					return new Gost3411Digest();
				case HashAlgorithm.KECCAK_224:
					return new KeccakDigest(224);
				case HashAlgorithm.KECCAK_256:
					return new KeccakDigest(256);
				case HashAlgorithm.KECCAK_288:
					return new KeccakDigest(288);
				case HashAlgorithm.KECCAK_384:
					return new KeccakDigest(384);
				case HashAlgorithm.KECCAK_512:
					return new KeccakDigest(512);
				case HashAlgorithm.MD2:
					return new MD2Digest();
				case HashAlgorithm.MD4:
					return new MD4Digest();
				case HashAlgorithm.RIPEMD128:
					return new RipeMD128Digest();
				case HashAlgorithm.RIPEMD160:
					return new RipeMD160Digest();
				case HashAlgorithm.RIPEMD256:
					return new RipeMD256Digest();
				case HashAlgorithm.RIPEMD320:
					return new RipeMD320Digest();
				case HashAlgorithm.SHA3_224:
					return new Sha3Digest(224);
				case HashAlgorithm.SHA3_256:
					return new Sha3Digest(256);
				case HashAlgorithm.SHA3_384:
					return new Sha3Digest(384);
				case HashAlgorithm.SHA3_512:
					return new Sha3Digest(512);
				case HashAlgorithm.SM3:
					return new SM3Digest();
				case HashAlgorithm.TIGER:
					return new TigerDigest();
				case HashAlgorithm.WHIRLPOOL:
					return new WhirlpoolDigest();
				default:
					this.error.setError("HA002", "Unrecognized HashAlgorithm");
					return null;
			}
		}

	}
}
