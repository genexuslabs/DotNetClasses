using System;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Utilities.Encoders;

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	public class Random
	{
		internal static string Numeric(int length)
		{
			string s = "";
			byte[] buffer = new byte[sizeof(uint)];
			using (var rng = RandomNumberGenerator.Create())
			{


				for (int i = 0; i <= length / 10; i++)
				{
					rng.GetBytes(buffer);
					s += BitConverter.ToUInt32(buffer, 0).ToString();
				}
			}
			return s.Length >= length ? s.Substring(0, length) : Numeric(length);
		}

		[SecuritySafeCritical]
		internal static string Alphanumeric(int length)
		{


			char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
			byte[] data = new byte[length];
#if NETCORE
			var arraySpan = new Span<byte>(data);
			System.Security.Cryptography.RandomNumberGenerator.Fill(arraySpan);
#else
			RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
			crypto.GetBytes(data);
#endif
			StringBuilder result = new StringBuilder(length);
			foreach (byte b in data)
			{
				result.Append(chars[b % (chars.Length)]);
			}
			return result.ToString();
		}

		[SecuritySafeCritical]
		internal static string HexaBits(int length)
		{
			Byte[] result = new Byte[length / 8];

#if NETCORE
			var arraySpan = new Span<byte>(result);
			System.Security.Cryptography.RandomNumberGenerator.Fill(arraySpan);
#else

			using (System.Security.Cryptography.RNGCryptoServiceProvider rngCsp = new System.Security.Cryptography.RNGCryptoServiceProvider())
			{

				rngCsp.GetBytes(result);
			}
#endif
			return Hex.ToHexString(result);

		}

	}
}
