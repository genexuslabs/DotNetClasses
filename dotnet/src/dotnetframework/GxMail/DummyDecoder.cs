using System;
using System.IO;
using System.Text;

namespace GeneXus.Mail.Internals.Pop3
{
	
	internal class DummyDecoder : MimeDecoder
	{
		private const string CRLF = "\r\n";

		public override void DecodeText(MailReader input, TextWriter output, GeneXus.Mail.Util.AsyncRunner runner)
		{
			string line;

			while(true)
			{
				line = input.ReadLine();
				ResetTimer(runner);
				if((line == null) || line.Equals("."))
				{
					break;
				}
				output.WriteLine(line);
			}
		}

		public override void DecodeFile(MailReader input, Stream output, GeneXus.Mail.Util.AsyncRunner runner)
		{
			string line;

			while(true)
			{
				line = input.ReadLine();
				ResetTimer(runner);
				if((line == null) || line.Equals("."))
				{
					break;
				}
				byte[] bytes = input.GetEncoding().GetBytes(line + CRLF);
				output.Write(bytes, 0, bytes.Length);
			}
		}
	}
}
