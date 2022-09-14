using GeneXus.Utils;
using Jayrock.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace GeneXus.SD.Store
{
	public class Util
	{
		public static DateTime FromUnixTime(long unixMillisecondsTime)
		{
			var dt = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
			return dt.AddMilliseconds(unixMillisecondsTime);
		}

		public static JObject FromJSonString(string s)
		{
			JObject _jsonArr = null;
			if (!string.IsNullOrEmpty(s))
			{
				_jsonArr = JSONHelper.ReadJSON<JObject>(s);
			}
			return _jsonArr;
		}

		private static byte[] GetHash(string inputString)
		{
			HashAlgorithm algorithm = SHA256.Create();  
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		public static string GetHashString(string inputString)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in GetHash(inputString))
				sb.Append(b.ToString("X2"));

			return sb.ToString();
		}

	}
}
