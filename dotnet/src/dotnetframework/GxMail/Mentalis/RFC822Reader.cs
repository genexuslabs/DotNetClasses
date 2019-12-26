using System;
using System.IO;
using System.Text;
using System.Collections;

namespace GeneXus.Mail.Internals.Pop3
{
	/// <summary>
	/// Summary description for RFC822Reader.
	/// </summary>
	internal class RFC822Reader : MailReader
	{
		private TextReader reader;
		private string separator;
		private string terminator;
		private string lastLine;
		private string readLine = "";
		private int lastChar = 1;
		private bool returnLF = false;
		private ArrayList separators;
		private ArrayList terminators;

		public RFC822Reader(TextReader reader)
		{
			this.reader = reader;
			this.separators = new ArrayList();
			this.terminators = new ArrayList();
		}

		public string GetSeparator()
		{
			return separator;
		}

		public void SetSeparator(string separator)
		{
			if(separator == null)
			{
				this.separator = this.terminator = null;
			}		
			else
			{
				this.separator  = separator;
				this.terminator = separator + "--";
				if (!separators.Contains(this.separator))
					separators.Add(this.separator);
				if (!terminators.Contains(this.terminator))
					terminators.Add(this.terminator);
			}
		}

		public int Read()
		{
			if(returnLF)
			{
				returnLF = false;
				return 10;
			}

			if(readLine == null)
			{
				readLine = "";
				lastChar = 1;
				return -1;
			}

			if(lastChar >= readLine.Length)
			{
				bool returnCR = (readLine != null && lastChar >= readLine.Length);

				readLine  = ReadLine();
				lastChar  = 0;

				if(returnCR)
				{
					returnLF = true;
					return 13;
				}
			}

			if(readLine == null)
			{
				return -1;
			}

			if(readLine.Length == 0)
			{
				return 20;
			}
			else
			{
				return readLine[lastChar++];
			}
		}

        public void SetTextPlain(bool value)
        {
            if (this.reader is RFC822EndReader)
            {
                ((RFC822EndReader)this.reader).SetTextPlain(value);
            }
        }
		public void SetEncoding(string charset)
		{
			if (this.reader is RFC822EndReader)
			{
				lastLine = ((RFC822EndReader)this.reader).SetEncoding(charset, lastLine);
			}
		}

		public Encoding GetEncoding()
		{
			if (this.reader is RFC822EndReader)
			{
				return ((RFC822EndReader)this.reader).GetEncoding();
			}
			return Encoding.UTF8;
		}

		private String RealReadLine()
		{
			return reader.ReadLine();
		}

		public string ReadLine()
		{
			string outStr;

			if(lastLine == null)
			{
				outStr = RealReadLine();
			}
			else
			{
				outStr = lastLine;
				lastLine = null;
			}
		
			if(outStr != null)
			{
				if(separator != null && (string.IsNullOrEmpty(outStr) || /*outStr.StartsWith(separator)*/StartsWithSeparator(outStr)))
				{
					if(/*outStr.StartsWith(separator)*/StartsWithSeparator(outStr))
					{
						lastLine = null;
						return null;
					}

					lastLine = RealReadLine();
					if(lastLine != null)
					{
						if(lastLine.StartsWith(terminator) || terminators.Contains(lastLine))
						{
							SetSeparator(null);
							lastLine = null;
							return null;
						}
						else if(lastLine.StartsWith(separator) || separators.Contains(lastLine))
						{
							lastLine = null;
							return null;
						}
					}				
				}
			}
			return outStr;
		}	

		private bool StartsWithSeparator(string str)
		{
			for (int i=0; i<separators.Count; i++)
			{
				if (str.StartsWith((string)separators[i]))
					return true;
			}
			return false;
		}

		public string ReadToEnd()
		{
			return reader.ReadToEnd();
		}

		public void ResetState()
		{
			if (reader is RFC822EndReader)
			{
				((RFC822EndReader)reader).ResetState();
			}
		}
	}
}
