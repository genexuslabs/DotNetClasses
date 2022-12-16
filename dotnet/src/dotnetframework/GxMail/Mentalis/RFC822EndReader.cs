using System;
using System.IO;
using System.Text;
using log4net;

namespace GeneXus.Mail.Internals.Pop3
{
	/// <summary>
	/// Summary description for RFC822EndReader.
	/// </summary>
	internal class RFC822EndReader : StreamReader
	{
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(RFC822EndReader));

		private const int BUFFER_SIZE = 1024;
        private bool textPlain = false;
		private string lastLine;
		private bool isEndOfMessage = false;
		private byte[] byteBuffer;
		private char[] charBuffer;
		private int _maxCharsPerBuffer;
		private int charLen;
		private int byteLen;
		private int lastByteLen;
		private int charPos;
		private bool _isBlocked;
		private Encoding encoding;
		private Decoder decoder;

		public RFC822EndReader(Stream stream) : base(stream)
		{
			this.byteBuffer = new byte[BUFFER_SIZE];
			this.charLen = 0;
			this.byteLen = 0;
			this.lastByteLen = this.byteLen;
			this.charPos = 0;
			this._isBlocked = false;
			// Se inicializa con el encoding default de la maquina
			SetEncoding(Encoding.Default, string.Empty);
		}

		public string SetEncoding(string charset, string lastline)
		{
			try
			{
                return SetEncoding(Encoding.GetEncoding(charset), lastline);
            }
			catch(Exception ex) {
                GXLogging.Error(log,"RFC822EndReader set encoding error" + charset, ex);
                return lastline;
            
            }
		}

		public Encoding GetEncoding()
		{
			if (this.encoding != null)
				return this.encoding;
			return Encoding.UTF8;
		}

        public void SetTextPlain(bool value)
        {
            this.textPlain = value;
        }
		private string SetEncoding(Encoding newEncoding, string lastreadline)
		{
            string newLastreadline = lastreadline;
            if ((this.encoding == null) || (!this.encoding.Equals(newEncoding)))
			{

                Encoding lastLineEncoding = this.encoding;
				this.encoding = newEncoding;
				this.decoder = encoding.GetDecoder();
				this._maxCharsPerBuffer = encoding.GetMaxCharCount(BUFFER_SIZE);
				this.charBuffer = new char[this._maxCharsPerBuffer];
				if (this.lastByteLen != 0)
				{
                    //Una alternativa a los dos siguientes if es setear el encoding en el ReadHeader de MailMessage, le problema de cambiarlo
                    //en ese momento es que leer el resto del header que viene en ANSI podria traer problema.
                    
                    //Se convierte de encoding la lastline del buffer de RFC822Reader y se recalcula el charPos segun el nuevo array de chars
                    if (lastreadline != null && lastreadline.Length > 0 && lastLineEncoding != null)
                    {
                        newLastreadline = newEncoding.GetString(lastLineEncoding.GetBytes(lastreadline));
                        if (this.charPos >= lastreadline.Length && lastreadline.Length != newLastreadline.Length)
                            this.charPos = this.charPos - lastreadline.Length + newLastreadline.Length;
                    }
                    //Se convierte de encoding la lastline del buffer de RFC822EndReader y se recalcula el charPos segun el nuevo array de chars
                    if (lastLine != null && lastLine.Length > 0 && lastLineEncoding != null)
                    {
                        string newLastline = newEncoding.GetString(lastLineEncoding.GetBytes(lastLine));
                        if (this.charPos >= lastLine.Length && lastLine.Length != newLastline.Length)
                            this.charPos = this.charPos - lastLine.Length + newLastline.Length;
                        this.lastLine = newLastline;
                    }
                    this.charLen = 0;
					this.charLen = this.decoder.GetChars(this.byteBuffer, 0, this.lastByteLen, this.charBuffer, this.charLen);
                }
			}
            return newLastreadline;
		}

		public override int Read()
		{
			if(isEndOfMessage)
			{
				return -1;
			}

			return base.Read();
		}

		private bool DataAvailable()
		{
			if (this.BaseStream is Org.Mentalis.Security.Ssl.SecureNetworkStream)
			{
				try
				{
					if (!((Org.Mentalis.Security.Ssl.SecureNetworkStream)this.BaseStream).DataAvailable)
						return false;
				}
				catch(Exception ex) 
                {
                    GXLogging.Error(log,"DataAvailable error", ex);
                }
			}
			return true;
		}

		public override string ReadToEnd()
		{
			if (!DataAvailable())
			{
				return "";
			}

			int num;
			char[] buffer = new char[this.charBuffer.Length];
			StringBuilder builder = new StringBuilder(this.charBuffer.Length);
			while ((num = this.Read(buffer, 0, buffer.Length)) != 0)
			{
				builder.Append(buffer, 0, num);
				if (!DataAvailable())
				{
					break;
				}
			}
			return builder.ToString();
		}

		private string InternalReadLine()
		{
			if ((this.charPos == this.charLen) && (this.ReadBuffer() == 0))
			{
				return null;
			}
			StringBuilder builder = null;
			do
			{
				int charPos = this.charPos;
				do
				{
					char ch = this.charBuffer[charPos];
					switch (ch)
					{
						case '\r':
						case '\n':
							string str;
							if (builder != null)
							{
								builder.Append(this.charBuffer, this.charPos, charPos - this.charPos);
								str = builder.ToString();
							}
							else
							{
								str = new string(this.charBuffer, this.charPos, charPos - this.charPos);
							}
							this.charPos = charPos + 1;
							CheckLineBreak(ch, charPos);
							return str;
					}
					charPos++;
				}
				while (charPos < this.charLen);
				charPos = this.charLen - this.charPos;
				if (charPos < 0)
				{
					charPos = 0;
				}
				if (builder == null)
				{
					builder = new StringBuilder(charPos + 80);
				}
				builder.Append(this.charBuffer, this.charPos, charPos);
			}
			while (this.ReadBuffer() > 0);
			return builder.ToString();
		}

		private void CheckLineBreak(char ch, int charPos)
		{
			if (((ch == '\r') && ((this.charPos < this.charLen) || (this.ReadBuffer() > 0))) && (this.charBuffer[this.charPos] == '\n'))
			{
				this.charPos++;
			}
			if ((charPos + 1 < this.charLen) && (ch == '\r') && (this.charBuffer[charPos+1] == '\n'))
			{
				this.charPos = charPos + 2;
			}
			if ((charPos + 2 < this.charLen) && (ch == '\r') && (this.charBuffer[charPos+1] == '\r') && (this.charBuffer[charPos+2] == '\n'))
			{
				this.charPos = charPos + 3;
			}
		}

		private int ReadBuffer()
		{
			this.charLen = 0;
			this.byteLen = 0;
			this.charPos = 0;
			do
			{
				this.byteLen = this.BaseStream.Read(this.byteBuffer, 0, this.byteBuffer.Length);
				this.lastByteLen = this.byteLen;
				if (this.byteLen == 0)
				{
					return this.charLen;
				}
				this._isBlocked = this.byteLen < this.byteBuffer.Length;
				this.charLen += this.decoder.GetChars(this.byteBuffer, 0, this.byteLen, this.charBuffer, this.charLen);
			}
			while (this.charLen == 0);
			return this.charLen;
		}

		public void ResetState()
		{
			textPlain = false;
			this._isBlocked = false;
			isEndOfMessage = false;
			lastLine = null;
		}

		public override string ReadLine()
		{
			if(isEndOfMessage)
			{
				return null;
			}

			string outStr;

			if(lastLine == null)
			{
				outStr = InternalReadLine();
			}
			else
			{
				outStr = lastLine;
				lastLine = null;
			}
			
			if(outStr != null)
			{
				int dotIdx = outStr.IndexOf(".");
				if(string.IsNullOrEmpty(outStr))
				{
					lastLine = InternalReadLine();
					if(lastLine.Equals("."))
					{
						isEndOfMessage = true;
						outStr = null;
						lastLine = null;
					}
				}
				/*else if ((dotIdx == 0) && (outStr.Length == 1) && this.charLen>0 && this.charLen==this.charPos && this._isBlocked && this.textPlain)
				{
					GXLogging.Debug(log,"EndofMessage:" + outStr);
					isEndOfMessage = true;
					outStr = null;
					lastLine = null;
				} */
				else if((dotIdx == 0) && (outStr.Length == 1) && !DataAvailable())
				{
					isEndOfMessage = true;
					outStr = null;
					lastLine = null;
				}
				else if((dotIdx == 0) && outStr!=null && (outStr.Length > 1) && (outStr[1] != '.')) //Si no vienen dos '.' seguidos (uno escapeado)
				{
					// El exchange no manda un LF antes del ".", asi que puede estar
					// enseguida..
					if(outStr.Equals("."))
					{
						isEndOfMessage = true;
						outStr = null;
						lastLine = null;
					}
					else
					{
						outStr = outStr.Substring(1, outStr.Length-1);					
					}
				}
			}
			return outStr;
		}	

		private bool IsWin2003()
		{
			OperatingSystem os = Environment.OSVersion;
			Version version = os.Version;
			if (version.Major == 5 && version.Minor == 2)
			{
				return true;
			}
			return false;
		}
	}
}
