using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Concurrent;
using System.Reflection;

namespace GeneXus.Encryption
{

	public class InvalidKeyException: Exception
	{
		public InvalidKeyException() : base("Invalid key")
		{
		}
		public InvalidKeyException(char c) : base("Invalid key " + c)
		{
		}
	}

	public class CryptoImpl
	{

		private static RandomNumberGenerator rng;

		public static string AJAX_ENCRYPTION_KEY = "GX_AJAX_KEY";
		public static string AJAX_ENCRYPTION_IV = "GX_AJAX_IV";
		public static string AJAX_SECURITY_TOKEN = "AJAX_SECURITY_TOKEN";
		public static string GX_AJAX_PRIVATE_KEY = "E7C360308E854317711A3D9983B98975";
		public static string GX_AJAX_PRIVATE_IV = "C01D04B1610243D2A2AF23E7952E8B18";
		const char NULL_CHARACTER = (char)0;

		private static int CHECKSUM_LENGTH = 6;
		private static string GX_ENCRYPT_KEYVALUE { get { throw new FileNotFoundException("Encryption keys file not found", "application.key or KeyResolver.dll"); } }

		private static ConcurrentDictionary<string, object> convertedKeys = new ConcurrentDictionary<string, object>();

		public static String Encrypt64(String value, String key)
		{
			if (string.IsNullOrEmpty(key) || key.Length != 32)
				throw new InvalidKeyException();

			try
			{
				if (string.IsNullOrEmpty(value))
					return string.Empty;

				byte[] str = encrypt(Encoding.UTF8.GetBytes(value), ConvertedKey(key));
				return Convert.ToBase64String(str, 0, str.Length);
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		public static string Encrypt(string value, string key, bool inverseKey)
		{
			if (inverseKey)
				key = key.Substring(16) + key.Substring(0, 16);

			return Encrypt(value, key);
		}

		public static string Encrypt(string value)
		{
			return Encrypt(value, false);
		}

		public static string Encrypt(string value, bool inverseKey)
		{
			string key = GetServerKey();
			if (inverseKey)
				key = key.Substring(16) + key.Substring(0, 16);

			return Encrypt(value, key);
		}

		public static string Encrypt(string value, string key)
		{
			string tmpBuf = addchecksum(value, getCheckSumLength());
			return Encrypt64(tmpBuf, key);
		}

		public static string Decrypt(string cfgBuf, string key, bool inverseKey)
		{
			string ret = "";
			Decrypt(ref ret, cfgBuf, inverseKey, key);
			return ret;
		}

		public static string Decrypt(string cfgBuf, string key)
		{
			return Decrypt(cfgBuf, key, false);
		}

		public static string Decrypt(string cfgBuf)
		{
			return Decrypt(cfgBuf, false);
		}

		public static string Decrypt(string cfgBuf, bool inverseKey)
		{
			string ret = "";
			Decrypt(ref ret, cfgBuf, inverseKey, null);
			return ret;
		}

		public static bool Decrypt(ref string ret, string cfgBuf)
		{
			return Decrypt(ref ret, cfgBuf, false, null);
		}

		static bool Decrypt(ref string ret, string cfgBuf, bool inverseKey, string key)
		{
			string chkSum, tmpBuf, decBuf;
			bool ok = false;
			
			if (string.IsNullOrEmpty(key))
				key = GetServerKey();
			if (inverseKey)
				key = key.Substring(16) + key.Substring(0, 16);

			tmpBuf = Decrypt64(cfgBuf, key);
			if (tmpBuf.Length < 6)
				return ok;
			chkSum = tmpBuf.Substring(tmpBuf.Length - 6, 6);
			decBuf = tmpBuf.Substring(0, tmpBuf.Length - 6);
			if (chkSum == CheckSum(decBuf, 6))
			{
				ret = decBuf;
				ok = true;
			}
		
			return ok;
		}

		private static object ConvertedKey(string key)
		{
			if (!convertedKeys.ContainsKey(key))
				convertedKeys.TryAdd(key, Twofish_Algorithm.makeKey(convertKey(key)));
			return convertedKeys[key];
		}


		private static byte[] convertKey(String a)
		{
			byte[] bout = new byte[a.Length / 2];

			int i = 0;
			int j = 0;
			for (; i < a.Length; i += 2, j++)
			{
				bout[j] = (byte)(toHexa(a[i]) * 16 + toHexa(a[i + 1]));
			}
			return bout;
		}

		private static byte toHexa(char c)
		{
			byte b;

			if ((c >= '0') && (c <= '9'))
				b = (byte)(c - '0');
			else if ((c >= 'a') && (c <= 'f'))
				b = (byte)(c - 'a' + 10);
			else if ((c >= 'A') && (c <= 'F'))
				b = (byte)(c - 'A' + 10);
			else
				throw new InvalidKeyException(c);

			return b;
		}

		public static String encrypt16(String value, String key)
		{
			return "";
		}

		public static String decrypt16(String value, String key)
		{
			return "";
		}

		public static String Decrypt64(String value, String key)
		{
			if (string.IsNullOrEmpty(value) || value.Trim().Length == 0)
				return "";

			if (string.IsNullOrEmpty(key) || key.Length != 32)
				throw new InvalidKeyException();

			value = value.TrimEnd(' ');

			try
			{

				byte[] str = decrypt(new Base64Decoder(value.ToCharArray()).GetDecoded(), ConvertedKey(key));
				return Encoding.UTF8.GetString(str, 0, str.Length).TrimEnd(' ');
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}


		public static int getCheckSumLength()
		{
			return CHECKSUM_LENGTH;
		}


		static string serverKey;
		public static string GetServerKey()
		{
			if (serverKey == null)
			{
				serverKey = GetFromKeyFile(0);  // First linea
				if (serverKey == null || serverKey.Length == 0)
				{
					serverKey = GetKeyFromAssembly(0);
					if (serverKey == null || serverKey.Length == 0)
						serverKey = GX_ENCRYPT_KEYVALUE;
				}
			}
			return serverKey;
		}

		static string siteKey;
		public static string GetSiteKey()
		{
			if (siteKey == null)
			{
				siteKey = GetFromKeyFile(1);    // Second line
				if (siteKey == null || siteKey.Length == 0)
				{
					siteKey = GetKeyFromAssembly(1);
					if (siteKey == null || siteKey.Length == 0)
						siteKey = GetServerKey();
				}
			}
			return siteKey;
		}

		static string GetFromKeyFile(int lineNo)
		{
			string configName = Path.Combine(CurrentDir, "application.key");
			if (!File.Exists(configName))
				return null;
			string s = null;
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			using (FileStream fs = new FileStream(configName, FileMode.Open, FileAccess.Read))
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					try
					{
						for (int i = 0; i < lineNo; i++)
							sr.ReadLine();
						s = sr.ReadLine();
					}
					catch
					{
						s = null;
					}
				}
			}
			return s;
		}
		static string GetKeyFromAssembly(int keyType)
		{
			string className = "KeyResolver";
			string method = "GetKey";
			string key = null;
			try
			{
				string assemblyPath = Path.Combine(CurrentDir, $"{className}.dll");

				if (File.Exists(assemblyPath))
				{

					Assembly assembly = Assembly.LoadFile(assemblyPath);
					Type typeInstance = assembly.GetType(className);

					if (typeInstance != null)
					{
						ConstructorInfo constructor = typeInstance.GetConstructor(Type.EmptyTypes);
						object instance = constructor.Invoke(null);

						MethodInfo methodInfo = typeInstance.GetMethod(method);
						object[] parameters = new object[] { keyType, null };
						methodInfo.Invoke(instance, parameters);
						key = (string)parameters[1];
					}
				}
			}
			catch (Exception)
			{
				return null;
			}

			return key;
		}

		public static String calcChecksum(String value, int start, int end, int length)
		{
			int ret = 0;

			for (int i = start; i < end; i++)
			{
				ret += value[i];
			}
			return inttohex(ret).ToUpper().PadLeft(length, '0');
		}

		static string inttohex(int intval)
		{
			string result = "";
			result = intval.ToString("X");
			return result;
		}

		public static String CheckSum(String value, int length)
		{
			return calcChecksum(value, 0, value.Length, length);
		}

		public static String addchecksum(String value, int length)
		{
			return value + calcChecksum(value, 0, value.Length, length);
		}

		private static char[] HEX_DIGITS =
		{
			'0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F'
		};

		public static byte[] encrypt(byte[] input, Object key)
		{
			int rest = 0;

			if (input.Length % 16 != 0)
				rest = 16 - (input.Length % 16);

			byte[] input_copy = new byte[input.Length + rest];
			byte[] output = new byte[input_copy.Length];

			Array.Copy(input, 0, input_copy, 0, input.Length);

			for (int i = 0; i < rest; i++)
			{
				input_copy[input.Length + i] = 32;
			}

			int count = input_copy.Length / 16;

			for (int idx = 0; idx < count; idx++)
			{
				Array.Copy(Twofish_Algorithm.blockEncrypt(input_copy, (uint)idx * 16, key), 0, output, idx * 16, 16);
			}

			return output;
		}
		private static String toString(byte[] ba, int offset, int length)
		{
			char[] buf = new char[length * 2];
			for (int i = offset, j = 0, k; i < offset + length;)
			{
				k = ba[i++];
				buf[j++] = HEX_DIGITS[(Twofish_Algorithm.ror((uint)k, 32, 4)/* >>> 4*/) & 0x0F];
				buf[j++] = HEX_DIGITS[k & 0x0F];
			}
			return new String(buf);
		}

		public static byte[] decrypt(byte[] input, Object key)
		{
			byte[] output = new byte[input.Length];

			int count = input.Length / 16;
			for (int idx = 0; idx < count; idx++)
			{
				Array.Copy(Twofish_Algorithm.blockDecrypt(input, (uint)idx * 16, key), 0, output, idx * 16, 16);
			}

			return output;
		}
		internal static RandomNumberGenerator RNG
		{
			get
			{
				return rng ?? (rng = RandomNumberGenerator.Create());
			}
		}

		public static string GetEncryptionKey()
		{
			int length = 16;
			byte[] ba = new byte[length];
#if !GXCOMMON
			RNG.GetBytes(ba);
#endif
			return toString(ba, 0, 16);
		}
		public static string GetRijndaelKey()
		{
			byte[] bytes = new byte[16];
			RNG.GetBytes(bytes);
			System.Text.StringBuilder buffer = new System.Text.StringBuilder(32);
			for (int i = 0; i < 16; i++)
			{
				buffer.Append(bytes[i].ToString("X").PadLeft(2, '0'));
			}
			return buffer.ToString();
		}
		public static string DecryptRijndael(string ivEncrypted, string key, out bool candecrypt)
		{
			AesCryptoServiceProvider aes = null;
			candecrypt = false;
			string encrypted = ivEncrypted.Length >= GX_AJAX_PRIVATE_IV.Length ? ivEncrypted.Substring(GX_AJAX_PRIVATE_IV.Length) : ivEncrypted;
			try
			{
				int discarded = 0;
				byte[] encryptedBytes = HexEncoding.GetBytes(encrypted, out discarded);
				if (encryptedBytes.Length > 0)
				{
					byte[] keyBytes = HexEncoding.GetBytes(key, out discarded);
					byte[] ivBytes = HexEncoding.GetBytes(GX_AJAX_PRIVATE_IV, out discarded);
					aes = new AesCryptoServiceProvider(); //CBC Mode
					aes.IV = ivBytes;
					aes.Key = keyBytes;
					aes.Padding = PaddingMode.Zeros;
					MemoryStream memoryStream;
					using (memoryStream = new MemoryStream(encryptedBytes))
					{
						CryptoStream cryptoStream;
						using (cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
						{
							cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
						}
					}
					string decrypted = Encoding.ASCII.GetString(memoryStream.ToArray());
					int zeroIdx = decrypted.IndexOf(NULL_CHARACTER);
					if (zeroIdx != -1)
					{
						decrypted = decrypted.Substring(0, zeroIdx);
					}
					candecrypt = true;
					return decrypted;
				}
			}
			catch (Exception) { }
			finally
			{
				if (aes != null) aes.Clear();
			}
			return encrypted;
		}
		public static string EncryptRijndael(string decrypted, string key)
		{
			AesCryptoServiceProvider aes = null;
			string encrypted = null;
			try
			{
				int discarded = 0;
				byte[] decryptedBytes = Encoding.ASCII.GetBytes(decrypted);
				byte[] keyBytes = HexEncoding.GetBytes(key, out discarded);
				byte[] ivBytes = HexEncoding.GetBytes(GX_AJAX_PRIVATE_IV, out discarded);
				aes = new AesCryptoServiceProvider(); //CBC Mode
				aes.IV = ivBytes;
				aes.Key = keyBytes;
				aes.Padding = PaddingMode.Zeros;
				MemoryStream memoryStream;
				using (memoryStream = new MemoryStream())
				{
					CryptoStream cryptoStream;
					using (cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cryptoStream.Write(decryptedBytes, 0, decryptedBytes.Length);
						cryptoStream.FlushFinalBlock();
						encrypted = HexEncoding.ToString(memoryStream.ToArray());
						int zeroIdx = encrypted.IndexOf(NULL_CHARACTER);
						if (zeroIdx != -1)
						{
							encrypted = encrypted.Substring(0, zeroIdx);
						}
					}
				}
				return encrypted;
			}
			catch (Exception) {
			}
			finally
			{
				if (aes != null) aes.Clear();
			}

			return decrypted;
		}

		static string s_currentDir;

		static string CurrentDir
		{
			get
			{
				if (string.IsNullOrEmpty(s_currentDir))
				{
					Assembly ass = Assembly.GetExecutingAssembly();
					FileInfo file = new FileInfo(new Uri(ass.CodeBase).LocalPath);

					s_currentDir = file.Directory.FullName;
				}

				return s_currentDir;
			}

		}

		
	}
}
