using GeneXusCryptography.ChecksumUtils;
using GeneXusCryptography.Commons;
using GeneXusCryptography.Hash;
using GeneXusCryptography.HashUtils;
using Org.BouncyCastle.Utilities.Encoders;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;
using System;
using System.Security;

namespace GeneXusCryptography.Checksum
{
	[SecuritySafeCritical]
	public class ChecksumCreator : SecurityAPIObject, IChecksumObject
	{
		public ChecksumCreator() : base()
		{

		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/
		[SecuritySafeCritical]
		public string GenerateChecksum(string input, string inputType, string checksumAlgorithm)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("input", input, this.error);
			SecurityUtils.validateStringInput("inputType", inputType, this.error);
			SecurityUtils.validateStringInput("checksumAlgorithm", checksumAlgorithm, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/

			ChecksumInputType chksumInputType = ChecksumInputTypeUtils.getChecksumInputType(inputType, this.error);
			byte[] inputBytes = ChecksumInputTypeUtils.getBytes(chksumInputType, input, this.error);
			if (this.HasError())
			{
				return "";
			}
			ChecksumAlgorithm algorithm = ChecksumAlgorithmUtils.getChecksumAlgorithm(checksumAlgorithm, this.error);
			if (this.HasError())
			{
				return "";
			}
			return (ChecksumAlgorithmUtils.isHash(algorithm)) ? CalculateHash(inputBytes, algorithm)
					: CalculateCRC(inputBytes, algorithm);
		}

		[SecuritySafeCritical]
		public bool VerifyChecksum(string input, string inputType, string checksumAlgorithm, string digest)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateStringInput("input", input, this.error);
			SecurityUtils.validateStringInput("inputType", inputType, this.error);
			SecurityUtils.validateStringInput("checksumAlgorithm", checksumAlgorithm, this.error);
			SecurityUtils.validateStringInput("digest", digest, this.error);
			if (this.HasError()) { return false; };
			/*******INPUT VERIFICATION - END*******/

			if (digest == null) return false;
			string result = GenerateChecksum(input, inputType, checksumAlgorithm);
			if (SecurityUtils.compareStrings(result, "") || this.HasError())
			{
				return false;
			}
			string resCompare = digest.ToUpper(System.Globalization.CultureInfo.InvariantCulture).Contains("0X") ? "0X" + result : result;
			return SecurityUtils.compareStrings(resCompare, digest);
		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

		private string CalculateCRC(byte[] input, ChecksumAlgorithm checksumAlgorithm)
		{
			CRCParameters parms = ChecksumAlgorithmUtils.getParameters(checksumAlgorithm, this.error);
			if (this.HasError())
			{
				return "";
			}
			long aux = CalculateCRC(input, parms);
			if (aux == 0 || this.HasError())
			{
				return "";
			}
			switch (parms.Width)
			{
				case 8:
					return aux.ToString("X2", System.Globalization.CultureInfo.InvariantCulture);
				case 16:
					return aux.ToString("X4", System.Globalization.CultureInfo.InvariantCulture);
				case 32:
					return aux.ToString("X8", System.Globalization.CultureInfo.InvariantCulture);
				default:
					return aux.ToString("X", System.Globalization.CultureInfo.InvariantCulture);
			}
		}

		private string CalculateHash(byte[] input, ChecksumAlgorithm checksumAlgorithm)
		{
			HashAlgorithm alg = getHashAlgorithm(checksumAlgorithm);
			if (this.HasError())
			{
				return "";
			}
			Hashing hash = new Hashing();
			byte[] digest = null;
			try
			{
				digest = hash.CalculateHash(alg, input);
			}
			catch (Exception e)
			{
				error.setError("CH001", e.Message);
				return "";
			}
			if (hash.HasError())
			{
				this.error = hash.GetError();
				return "";
			}
			return Hex.ToHexString(digest);
		}

		private HashUtils.HashAlgorithm getHashAlgorithm(ChecksumAlgorithm checksumAlgorithm)
		{
			return HashAlgorithmUtils.getHashAlgorithm(ChecksumAlgorithmUtils.valueOf(checksumAlgorithm, this.error), this.error);
		}

		private static long CalculateCRC(byte[] input, CRCParameters parms)
		{
			long curValue = parms.Init;
			long topBit = 1L << (parms.Width - 1);
			long mask = (topBit << 1) - 1;

			for (int i = 0; i < input.Length; i++)
			{
				long curByte = ((long)(input[i])) & 0x00FFL;
				if (parms.ReflectIn)
				{
					curByte = Reflect(curByte, 8);
				}

				for (int j = 0x80; j != 0; j >>= 1)
				{
					long bit = curValue & topBit;
					curValue <<= 1;

					if ((curByte & j) != 0)
					{
						bit ^= topBit;
					}

					if (bit != 0)
					{
						curValue ^= parms.Polynomial;
					}
				}

			}

			if (parms.ReflectOut)
			{
				curValue = Reflect(curValue, parms.Width);
			}

			curValue = curValue ^ parms.FinalXor;

			return curValue & mask;
		}

		private static long Reflect(long input, int count)
		{
			long ret = input;
			for (int idx = 0; idx < count; idx++)
			{
				long srcbit = 1L << idx;
				long dstbit = 1L << (count - idx - 1);
				if ((input & srcbit) != 0)
				{
					ret |= dstbit;
				}
				else
				{
					ret = ret & (~dstbit);
				}
			}
			return ret;
		}
	}
}
