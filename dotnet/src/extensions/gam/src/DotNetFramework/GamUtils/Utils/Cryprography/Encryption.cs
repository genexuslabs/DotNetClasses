using System;
using System.Security;
using System.Text;
using log4net;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

namespace GamUtils.Utils.Cryprography
{
	[SecuritySafeCritical]
	internal class Encryption
	{
	
		private static readonly ILog logger = LogManager.GetLogger(typeof(Encryption));

		[SecuritySafeCritical]
		public static string AesGcm(string input, string key, string nonce, int macSize, bool toEncrypt)
		{
			return toEncrypt ? Base64.ToBase64String(Internal_AesGcm(Encoding.UTF8.GetBytes(input), key, nonce, macSize, toEncrypt)) : Encoding.UTF8.GetString(Internal_AesGcm(Base64.Decode(input), key, nonce, macSize, toEncrypt));
		}

		[SecuritySafeCritical]
		private static byte[] Internal_AesGcm(byte[] inputBytes, string key, string nonce, int macSize, bool toEncrypt)
		{
			logger.Debug("Internal_AesGcm");

			IAeadBlockCipher cipher = new GcmBlockCipher(new AesEngine());
			AeadParameters AEADparams = new AeadParameters(new KeyParameter(Hex.Decode(key)), macSize, Hex.Decode(nonce));
			try
			{
				cipher.Init(toEncrypt, AEADparams);
				byte[] outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
				int length = cipher.ProcessBytes(inputBytes, 0, inputBytes.Length, outputBytes, 0);
				cipher.DoFinal(outputBytes, length);
				return outputBytes;
			}
			catch (Exception e)
			{
				logger.Error("Internal_AesGcm", e);
				return null;
			}
		}
	}
}
