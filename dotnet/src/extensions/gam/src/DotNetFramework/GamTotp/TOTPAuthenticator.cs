using System;
using Google.Authenticator;
using System.Security.Cryptography;
using System.Security;
using log4net;

namespace GamTotp
{
	public class TOTPAuthenticator
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(TOTPAuthenticator));

		[SecuritySafeCritical]
		public static string GenerateKey(int length)
		{
			logger.Debug("GenerateKey");
			byte[] randomBytes = new byte[length];
			using (var rng = RandomNumberGenerator.Create()) { 
				rng.GetBytes(randomBytes);
				string str = Base32Encoding.ToString(randomBytes).TrimEnd('=');
				return str.Remove(length);
			}
		}

		[SecuritySafeCritical]
		public static string GenerateQRData(string accountName, string secretKey, string appName, string algorithm, int digits, int period)
		{
			logger.Debug("GenerateQRData");
			TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
			SetupCode setupInfo = tfa.GenerateSetupCode(appName, accountName, secretKey, true, 6);

			return setupInfo.QrCodeSetupImageUrl;
		}

		[SecuritySafeCritical]
		public static bool VerifyTOTPCode(string secretKey, string code, string algorithm, int digits, int period)
		{
			logger.Debug("VerifyTOTPCode");
			TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
			return tfa.ValidateTwoFactorPIN(secretKey, code, TimeSpan.FromSeconds(period), true);
		}
	}
}
