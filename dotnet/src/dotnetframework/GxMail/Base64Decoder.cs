using System;
using System.IO;
using System.Text;

namespace GeneXus.Mail.Internals.Pop3
{
	
	internal class Base64Decoder : MimeDecoder
	{
		public override void DecodeFile(MailReader input, Stream output, GeneXus.Mail.Util.AsyncRunner runner)
		{
			byte[] converted = GetFromBase64String(input, runner);
			output.Write(converted, 0, converted.Length);
		}

		public override void DecodeText(MailReader input, TextWriter output, GeneXus.Mail.Util.AsyncRunner runner)
		{
			output.Write(input.GetEncoding().GetString(GetFromBase64String(input, runner)));
		}

		private byte[] GetFromBase64String(MailReader input, GeneXus.Mail.Util.AsyncRunner runner)
		{
			StringBuilder strBuilder = new StringBuilder();
			string str = null;
			while(true)
			{
				str = input.ReadLine();
				ResetTimer(runner);
				if(string.IsNullOrEmpty(str) || str.Equals("."))
					break;
				strBuilder.Append(str);
			}
			return Convert.FromBase64String(strBuilder.ToString());
		}
	}
}
