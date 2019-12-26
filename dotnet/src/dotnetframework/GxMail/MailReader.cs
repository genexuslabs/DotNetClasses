using System;
using System.Text;

namespace GeneXus.Mail.Internals.Pop3
{
	
	internal interface MailReader
	{
		int Read();
		string ReadLine();
		string GetSeparator();
		void SetSeparator(string separator);
		void SetEncoding(string charset);
		Encoding GetEncoding();
		void SetTextPlain(bool value);
		string ReadToEnd();
		void ResetState();
	}
}
