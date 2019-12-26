using System;
using System.Text;

namespace GeneXus.Mail.Internals.Smtp
{
	
	internal class MimeEncoder
	{
		public string EncodeString(string text, Encoding encoding)
		{
			StringBuilder sb = new StringBuilder();
			EncodeString(text, encoding, 68 - encoding.BodyName.Length, "=?" + encoding.BodyName + "?B?", false, sb);
			return sb.ToString();
		}

		private void EncodeString(string text, Encoding encoding, int len, string strEnd, bool blank, StringBuilder sb)
		{
			byte[] oldBytes = encoding.GetBytes(text);

			int j = ((oldBytes.Length + 2) / 3) * 4;
			int k;

			if(j > len && (k = text.Length) > 1)
			{
				EncodeString(text.Substring(0, k / 2), encoding, len, strEnd, blank, sb);
				EncodeString(text.Substring(k / 2, k-(k / 2)), encoding, len, strEnd, true, sb);
				return;
			}

			if(blank)
			{
				sb.Append(" ");
			}
			sb.Append(strEnd);

			sb.Append(Convert.ToBase64String(oldBytes));

			sb.Append("?=");
		}
	}
}
