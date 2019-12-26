using System;
using System.IO;

namespace GeneXus.Mail.Internals.Pop3
{
	
	internal abstract class MimeDecoder
	{
		public abstract void DecodeText(MailReader input, TextWriter output, GeneXus.Mail.Util.AsyncRunner runner);
		public abstract void DecodeFile(MailReader input, Stream output, GeneXus.Mail.Util.AsyncRunner runner);

		protected void ResetTimer(GeneXus.Mail.Util.AsyncRunner runner)
		{
			try
			{
				if (runner != null)
				{
					runner.ResetTimer();
				}
			}
			catch {}
		}
	}
}
