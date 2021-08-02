﻿using GeneXusCryptography.ChecksumUtils;
using GeneXusCryptography.Commons;
using GeneXusCryptography.Hash;
using GeneXusCryptography.HashUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Utils;
using System;
using System.Security;

namespace GeneXusCryptography.Checksum
{
	[SecuritySafeCritical]
	public class ChecksumCreator : SecurityAPIObject, IChecksumObject
	{
		public ChecksumCreator(): base()
		{

		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/
		[SecuritySafeCritical]
		public string GenerateChecksum(string input, string inputType, string checksumAlgorithm)
		{
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
			string result = GenerateChecksum(input, inputType, checksumAlgorithm);
			if (SecurityUtils.compareStrings(result, "") || this.HasError())
			{
				return false;
			}
			string resCompare = digest.ToUpper().Contains("0X") ? "0X" + result : result;
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
					return aux.ToString("X2");
				case 16:
					return aux.ToString("X4");
				case 32:
					return aux.ToString("X8");
				default:
					return aux.ToString("X");
			}
		}

		private string CalculateHash(byte[] input, ChecksumAlgorithm checksumAlgorithm)
		{
			HashUtils.HashAlgorithm alg = getHashAlgorithm(checksumAlgorithm);
			if (this.HasError())
			{
				return "";
			}
			Hashing hash = new Hashing();
			byte[] digest = hash.calculateHash(alg, input);
			if (hash.HasError())
			{
				this.error = hash.GetError();
				return "";
			}
			return toHexaString(digest);
		}

		private HashUtils.HashAlgorithm getHashAlgorithm(ChecksumAlgorithm checksumAlgorithm)
		{
			return HashAlgorithmUtils.getHashAlgorithm(ChecksumAlgorithmUtils.valueOf(checksumAlgorithm, this.error), this.error);
		}

		private string toHexaString(byte[] digest)
		{
			string result = BitConverter.ToString(digest).Replace("-", string.Empty);
			if (result == null || result.Length == 0)
			{
				this.error.setError("HS001", "Error encoding hexa");
				return "";
			}
			return result.ToUpper().Trim();
		}

		private long CalculateCRC(byte[] input, CRCParameters parms)
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

		private long Reflect(long input, int count)
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
