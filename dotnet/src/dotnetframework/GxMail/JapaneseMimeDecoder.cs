using System;
using System.Text;

namespace GeneXus.Mail
{
	
	public class JapaneseMimeDecoder
	{
		private static string[] encodingStarts = new string[] { "$B", "$@" };
		private static string[] encodingEnds = new string[] { "(B", "(J" };

		public static string Decode(string encoded)
		{
			int strLen = encoded.Length;
			int currentIndex = 0;

			StringBuilder buffer = new StringBuilder();

			while(currentIndex < strLen)
			{
				int encStart = GetEncodedLimit(encodingStarts, encoded, currentIndex);
				if(encStart != -1)
				{
					int encEnd = GetEncodedLimit(encodingEnds, encoded, encStart + 3);
					if(encEnd != -1)
					{
						if(currentIndex < encStart)
						{
							buffer.Append(encoded.Substring(currentIndex, encStart - currentIndex));
						}
						byte[] jpBytes = Encoding.GetEncoding("ISO-2022-JP").GetBytes(encoded.Substring(encStart, encEnd + 3 - encStart));
						buffer.Append(new String(Encoding.GetEncoding("ISO-2022-JP").GetChars(jpBytes)));
						currentIndex = encEnd + 3;
					}
					else
					{
						break;
					}
				}
				else
				{
					break;
				}
			}

			if(currentIndex < strLen)
			{
				buffer.Append(encoded.Substring(currentIndex));
			}

			return buffer.ToString();
		}

		private static int GetEncodedLimit(string[] limiters, string str, int fromIdx)
		{
			for(int i=0; i<limiters.Length; i++)
			{
				int idx = str.IndexOf(limiters[i], fromIdx);
				if(idx != -1)
				{
					return idx;
				}
			}
			return -1;
		}
	}
}
