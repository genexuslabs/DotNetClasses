using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GeneXus.Mail.Internals.Pop3
{
    
    internal class QuotedPrintableDecoder : MimeDecoder
    {
        public override void DecodeFile(MailReader input, Stream output, GeneXus.Mail.Util.AsyncRunner runner)
        {
            Decode(input, output, input.GetEncoding(), false, runner);
        }

        public override void DecodeText(MailReader input, TextWriter output, GeneXus.Mail.Util.AsyncRunner runner)
        {
            Decode(input, output, input.GetEncoding(), false, runner);
        }

        public string DecodeHeader(string input)
        {
            string output = "";
            string line = input;
            int leftEqual = 0;
            int rightEqual = 0;

            int start = line.IndexOf("=?");
            if (start > 1)
            {
                output = input.Substring(0, start);
            }

            while (true)
            {
                leftEqual = line.IndexOf("=?", rightEqual);
                if (leftEqual == -1)
                {
                    break;
                }

				//Look for the following ?= Other than ?q?=
                //p.e. subject 'RE: Status bar spec/generation - info "incorrect" of pending issues')
                int nextRightEqual = line.IndexOf("?=", rightEqual);
                while (nextRightEqual >= 2 && line.Length > nextRightEqual + 1 && line.Substring(nextRightEqual - 2, 4).ToLower() == "?q?=")
                {
                    nextRightEqual = line.IndexOf("?=", nextRightEqual + 1);
                }
                rightEqual = nextRightEqual + 2;
                
                if (rightEqual == -1)
                {
                    break;
                }

                input = line.Substring(leftEqual, rightEqual - leftEqual);

                int QIdx = input.ToLower().IndexOf("?q?");
                int BIdx = input.ToLower().IndexOf("?b?");

                string left = input.Substring(0, ((QIdx == -1) ? BIdx : QIdx) + 3);

                if (QIdx != -1) 
                {
                    StringWriter sout = new StringWriter();                    
                    input = input.Substring(left.Length, input.Length - 2 - left.Length);
                    Decode(new RFC822Reader(new StringReader(input)), sout, GetEncoding(left), true, null);
                    output += sout.ToString();
                }
                else //BASE64
                {
                    input = input.Substring(left.Length, input.Length - 2 - left.Length);
                    byte[] converted = Convert.FromBase64String(input);
                    output += GetEncoding(left).GetString(converted);
                }
            }

            if ((rightEqual > 0) && (line.Length > (rightEqual + 1)))
            {
                output += line.Substring(rightEqual + 1);
            }
            else if (rightEqual == 0)
            {
                output = input;
            }

            return output;
        }

        public void Decode(MailReader input, Stream output, Encoding enc, bool header, GeneXus.Mail.Util.AsyncRunner runner)
        {
            string result = DecodeImp(input, enc, header, runner);
            if (result.StartsWith("\r\n"))
                result = result.Substring(2);
            byte[] buffer = enc.GetBytes(result);
            output.Write(enc.GetBytes(result), 0, buffer.Length);
        }

        public void Decode(MailReader input, TextWriter output, Encoding enc, bool header, GeneXus.Mail.Util.AsyncRunner runner)
        {
            string result = DecodeImp(input, enc, header, runner);
            output.Write(result);
        }

        private string DecodeImp(MailReader input, Encoding enc, bool header, GeneXus.Mail.Util.AsyncRunner runner)
        {
            int charRead = 0;
            StringBuilder sb = new StringBuilder();
            while ((charRead = input.Read()) != -1)
            {
                if (header)
                {
                    if (charRead == 13 || charRead == 10) // CR LF must be removed from header. 
                    {
                        continue;
                    }
                    else
                        if (charRead == 95) // Underscore in Header must be replaced by space.
                        {
                            charRead = ' ';
                        }
                }
                sb.Append((char)charRead);
                ResetTimer(runner);
            }
            return DecodeQuotedPrintable(sb.ToString(), enc);
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string DecodeQuotedPrintable(string input, Encoding enc)
        {
            input = input.Replace("=\r\n", string.Empty);
            var occurences = new Regex(@"(=[0-9A-Z][0-9A-Z])+", RegexOptions.Multiline);
            var matches = occurences.Matches(input);
            foreach (Match m in matches)
            {
                byte[] bytes = new byte[m.Value.Length / 3];
                for (int i = 0; i < bytes.Length; i++)
                {
                    string hex = m.Value.Substring(i * 3 + 1, 2);
                    int iHex = Convert.ToInt32(hex, 16);
                    bytes[i] = Convert.ToByte(iHex);
                }
                input = ReplaceFirst(input, m.Value, enc.GetString(bytes));
            }
            return input;
        }

        private Encoding GetEncoding(string text)
        {
            string encodingS = text.Substring(2, text.Length - 5);
            Encoding encoding;
            try
            {
                encoding = Encoding.GetEncoding(encodingS);
            }
            catch (Exception)
            {
                encoding = Encoding.Default;
            }
            return encoding;
        }

    }
}
