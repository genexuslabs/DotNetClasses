using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using System.Threading;
using GeneXus.Utils;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GeneXus.Mail.Internals.Pop3
{
	
	internal class MailMessage
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(MailMessage));
		private const string CRLF = "\r\n";

		private static Hashtable monthList = new Hashtable();        
		private static ConcurrentDictionary<string, string> contentTypes = null;
		private static QuotedPrintableDecoder qpDecoder = new QuotedPrintableDecoder();

        private MailProperties keys = new MailProperties();
        
		private StringBuilder messageText = new StringBuilder();
		private StringBuilder messageHtml = new StringBuilder();

		private string attachmentsPath;
		private string attachments = "";

		private Exception timeoutException = null;
		private int timeout;
		private MailReader reader;
		private GeneXus.Mail.Util.AsyncRunner runner;
		private Thread mainThread = null;

		static MailMessage()
		{
			FillMonthList();
			FillContentTypes();
		}

		static void FillContentTypes()
		{
			contentTypes = new ConcurrentDictionary<string,string>();
			contentTypes["text/plain"]="txt";
			contentTypes["text/richtext"] = "rtx";
			contentTypes["text/html"] = "html";
			contentTypes["text/xml"] = "xml";
			contentTypes["message/rfc822"] = "eml";
			contentTypes["image/jpg"] = "jpg";
			contentTypes["image/jpeg"] = "jpg";
			contentTypes["image/gif"] = "gif";
			contentTypes["image/png"] = "png";
            
		}

		public MailMessage(MailReader reader, string attachmentsPath, int timeout)
		{
			this.reader = reader;
			this.attachments = "";
			this.attachmentsPath = attachmentsPath;
			this.timeout = timeout;

			this.mainThread = Thread.CurrentThread;
		}

		private void ResetTimer()
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

        public bool DownloadAttachments
        {
            get;
            set;
        }
#pragma warning disable CS0618 // Type or member is obsolete

		public void SetFinishReading(Exception exception)
		{
			this.timeoutException = exception;
			if (timeout > 0)
			{
				mainThread.Resume();
			}
		}

		public void ReadAllMessage()
		{
			this.timeoutException = null;
			if (timeout > 0)
			{
				runner = new GeneXus.Mail.Util.AsyncRunner(this, "ReadEntireMessage", Array.Empty<object>(), new GeneXus.Mail.Util.AsyncCallback(this, "SetFinishReading", false), timeout);
				runner.Run();
				mainThread.Suspend();
				if (this.timeoutException != null)
				{
					throw this.timeoutException;
				}
			}
			else
			{
				ReadEntireMessage();
				SetFinishReading(null);
			}
		}
#pragma warning restore CS0618 // Type or member is obsolete

		internal MailProperties Keys
        {
            get { return keys; }            
        }

		public void ReadEntireMessage()
		{
			try
			{
				keys = ReadHeader(reader);
				ReadMessage(reader, keys, null);
			}
			catch(Exception e)
			{
                GXLogging.Error(log,"ReadEntireMessage error", e);
				throw e;
			}
			finally
			{
				try
				{
					reader.ReadToEnd();
				}
				catch {}
				reader.ResetState();
			}
		}

		private string FixFileName(string AttachDir, string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = Path.GetRandomFileName();
			}
			if (Path.Combine(AttachDir, name).Length > 200)
			{
				name = Path.GetRandomFileName().Replace(".", "") + "." + Path.GetExtension(name);
			}
			Regex validChars = new Regex(@"[\\\/\*\?\|:<>]");
			return validChars.Replace(name, "_");
		}

		private static void FillMonthList()
		{
			monthList = new Hashtable();
			monthList.Add(1,"January");
			monthList.Add(2,"Feburary");
			monthList.Add(3,"March");
			monthList.Add(4,"April");
			monthList.Add(5,"May");
			monthList.Add(6,"June");
			monthList.Add(7,"July");
			monthList.Add(8,"August");
			monthList.Add(9,"September");
			monthList.Add(10,"October");
			monthList.Add(11,"November");
			monthList.Add(12,"December");
		}

		private void ReadMessage(MailReader reader, MailProperties partProps, string separator)
		{
			reader.SetEncoding(partProps.GetCharset());
			string mimeMediaType = partProps.GetMimeMediaType();
			if(string.Compare(mimeMediaType, "multipart", true) == 0) 
			{
				ReadMultipartBody(reader, partProps);
			}
			else
			{
				ReadBody(reader, partProps, separator);
			}
		}

		private MailProperties ReadHeader(MailReader reader)
		{
			string temp;

			string reply = reader.ReadLine();
			ResetTimer();

			if((reply == null) || reply.Equals("."))
			{
				return null;
			}
			MailProperties properties = null;

			while(!string.IsNullOrEmpty(reply))
			{
				if(properties == null)
				{
					properties = new MailProperties();
				}

				temp = reader.ReadLine();
				ResetTimer();
				if (temp == null)
				{
					return properties;
				}
				if(temp.Equals("."))
				{
					return properties;
				}

				bool returnProps = false;
				// MultiLine headers have a space\tab at begining of each extra line
				while((temp.StartsWith(" ")) || (temp.StartsWith("\t"))) 
				{
					reply = reply + CRLF + temp;
					temp  = reader.ReadLine();
					ResetTimer();

					if((temp == null) || temp.Equals("."))
					{
						returnProps = true;
						break;
					}
				} 

				properties.PutKey(reply);
                
				reply = temp;
				if(returnProps)
				{
					return properties;
				}
			}
			return properties;
		}

		private static string GetSeparator(MailProperties properties)
		{
			string separator = properties.GetKeyProperty("Content-Type", "boundary");

			if(separator.Length == 0)
			{
				return null;
			}

			if(separator[0] == '"')
			{
				separator = separator.Substring(1, separator.Length - 2);
			}

			return "--" + separator;
		}

		private void ReadMultipartBody(MailReader reader, MailProperties properties)
		{
			string separator = GetSeparator(properties);

			string subtype = properties.GetMimeMediaSubtype();

			reader.SetSeparator(null);

			string reply = reader.ReadLine();
			ResetTimer();

			if(reply != null)
			{
				while(reply != null && !reply.StartsWith(separator)) 
				{
					reply = reader.ReadLine();
					ResetTimer();
				}
			}

			reader.SetSeparator(separator);

			while (true)
			{
				MailProperties props = ReadHeader(reader);

				if(props == null)
				{
					break;
				}
				
				ReadMessage(reader, props, separator);
			}
		}

		private string GetFileName(string path, string name, string contentType)
		{
			bool tempFile = false;
			if(name.Trim().Length == 0)
			{
				name = "TempFile";
				tempFile = true;
			}
			name = new QuotedPrintableDecoder().DecodeHeader(name);
			name = RemoveQuotes(name);
		
			string extension = "";
			int lastDot = name.LastIndexOf(".");
			if(lastDot > 0 && !tempFile)
			{
				extension = name.Substring(lastDot, name.Length - lastDot);
				name = name.Substring(0, lastDot);
			}
			else
			{
				extension = "." + ExtensionFromContentType(contentType);
			}

			int idx = 1;
			string nameOri = "" + name;
			while(File.Exists(path + name + extension))
			{
				name = nameOri + " (" + idx + ")";
				idx = idx + 1;
			}

			return FixFileName(path, name + extension);
		}

		private static string ExtensionFromContentType(string contentType)
		{
			string extension = contentTypes[contentType];
			if (extension != null)
			{
				return extension.ToString();
			}
			return "";
		}

		private bool AttachmentContentDisposition(MailProperties partProps)
		{
			string contentDisposition = partProps.GetKeyPrincipal("Content-Disposition");
			if(string.Compare(contentDisposition, "attachment", true) == 0)
			{
				return true;
			}
			string transferEncoding = partProps.GetKeyPrincipal("Content-Transfer-Encoding");
			if((string.Compare(contentDisposition, "inline", true) == 0) && (string.Compare(transferEncoding, "base64", true) == 0))
			{
				return true;
			}
			return false;
		}

		private void ReadBody(MailReader reader, MailProperties partProps, String separator)
		{
			string contentType = partProps.GetKeyPrincipal("Content-Type");

			bool isTextPlain = (string.Compare(contentType, "text/plain", true) == 0);
			bool isTextHtml = (string.Compare(contentType, "text/html", true) == 0);
			bool isAttachment = (!isTextPlain && !isTextHtml) || AttachmentContentDisposition(partProps);
		
			string oldSeparator = reader.GetSeparator();
			reader.SetSeparator(separator);
            reader.SetTextPlain(isTextPlain);
            GXLogging.Debug(log,"isAttachment:" + isAttachment + " isTextHTML:" + isTextHtml + " istextplain:"  + isTextPlain);
			MimeDecoder decoder = GetDecoder(partProps.GetField("Content-Transfer-Encoding"));

			if(isAttachment)
			{
                string outname=string.Empty;
                try
                {                    
                    Stream output;
                    if (this.DownloadAttachments)
                    {        
                        string cid = GetAttachmentContentId(partProps);
                        
                        string fileName = qpDecoder.DecodeHeader(RemoveQuotes(partProps.GetKeyProperty("Content-Disposition", "filename")));
                        if (string.IsNullOrEmpty(fileName))
                        {
                            fileName = RemoveQuotes(partProps.GetKeyProperty("Content-Type", "name"));
                        }
						
                        bool inlineAttachment = partProps.GetKeyPrincipal("Content-Disposition").StartsWith("inline");
                        if (inlineAttachment && !string.IsNullOrEmpty(cid))
                        {
                            fileName = String.Format("{0}_{1}", cid, fileName);
                            outname = GetFileName(attachmentsPath, fileName, contentType);
                            this.messageHtml = this.messageHtml.Replace("cid:" + cid, outname);
                        }
                        else
                        {
                            outname = GetFileName(attachmentsPath, fileName, contentType);
                        }

                        attachments += outname + ";";

                        if (!Directory.Exists(attachmentsPath))
                            Directory.CreateDirectory(attachmentsPath);
                        output = new FileStream(attachmentsPath + outname, FileMode.Create);
                    }
                    else
                    {
                        output = new DummyStream();
                    }
                    try
                    {
                        decoder.DecodeFile(reader, output, runner);
                    }
                    catch (Exception e)
                    {
                        GXLogging.Error(log,"ReadBody error", e);
                        throw e;
                    }
                    finally
                    {
                        output.Close();
                    }
                }
                catch (DirectoryNotFoundException dex)
                {
                    GXLogging.Error(log,"Error with attachments, attachment path:" + attachmentsPath + outname, dex);
                    throw new CouldNotSaveAttachmentException(dex);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                        ex = ex.InnerException;
                    GXLogging.Error(log,"Error with attachments, attachment path:" + attachmentsPath + outname, ex);
                    throw new InvalidAttachmentException(ex);
                }
			}
			else
			{
				try
				{
					TextWriter output = new StringWriter();
					try
					{
						decoder.DecodeText(reader, output, runner);
						if(isTextPlain)
						{
							messageText.Append(((StringWriter)output).GetStringBuilder().ToString());
						}
						else if(isTextHtml)
						{
							messageHtml.Append(((StringWriter)output).GetStringBuilder().ToString());
						}
					}
					catch(Exception e)
					{
                        GXLogging.Error(log,"ReadBody error", e);
                        throw e;
					}
					finally
					{
						output.Close();
					}
				}
				catch(Exception ex1)
				{
					if (ex1.InnerException != null)
						ex1 = ex1.InnerException;
                    GXLogging.Error(log,"ReadBody error", ex1);
                    throw new InvalidMessageException(ex1);
				}
			}
			reader.SetSeparator(oldSeparator);
		}

        private static string GetAttachmentContentId(MailProperties partProps)
        {
            string cid = partProps.GetKeyPrincipal("Content-ID");
            if (!string.IsNullOrEmpty(cid) && cid.StartsWith("<") && cid.EndsWith(">"))
            {                
                cid = cid.Substring(1, cid.Length - 2);
            }
            return cid;
        }

		private MimeDecoder GetDecoder(string encoding)
		{
			if(string.Compare(encoding, "base64", true) == 0)
			{
				return new Base64Decoder();
			}
			else if(string.Compare(encoding, "quoted-printable", true) == 0)
			{
				return new QuotedPrintableDecoder();
			}

			return new DummyDecoder();
		}

		public string GetField(string field)
		{
			if (keys != null)
			{
				return keys.GetField(field);
			}
			return "";
		}

		public string GetMessageText()
		{
			return JapaneseMimeDecoder.Decode(messageText.ToString());
		}

		public string GetMessageHtmlText()
		{
			return messageHtml.ToString();
		}

		public void SetMessageAttachments(GeneXus.Utils.GxStringCollection attachs)
		{
			if(attachments.Length > 0)
			{
				string[] attachNames = attachments.Split(";".ToCharArray());
				foreach(string attach in attachNames)
				{
					if(attach.Length > 0)
					{
						attachs.Add(attach);
					}
				}
			}
		}

	

        private string[] GetMessageRecipient(string recipientString)
		{
			string[] recipParts = new string[2];
			int ltIdx = recipientString.IndexOf("<");
			if(ltIdx > 0)
			{
				recipParts[0] = RemoveQuotes(qpDecoder.DecodeHeader(recipientString.Substring(0, ltIdx).Trim()));
				recipParts[1] = RemoveQuotes(recipientString.Substring(ltIdx + 1, recipientString.Length - (ltIdx + 2)));
				return recipParts;
			}
			else if (ltIdx == 0)
			{
				recipientString = recipientString.Substring(1, recipientString.Length - 2);
			}
			
			recipParts[0] = recipientString;
			recipParts[1] = recipientString;
			return recipParts;
		}

		public void SetMessageRecipients(GXMailRecipientCollection recipients, string type)
		{
			string recsStr = GetField(type);
			if(!string.IsNullOrEmpty(recsStr))
			{
				bool inQuotes = false;
				List<String> recPartList = new List<string>();
				StringBuilder part = new StringBuilder();
				
				foreach (char c in recsStr)
				{
					if (c=='"')
						inQuotes = !inQuotes;
					if ((c==';' || c==',') && !inQuotes)
					{
						recPartList.Add(part.ToString());
						part = new StringBuilder();
					}
					else
					{
						part.Append(c);
					}
				}
				if (part.Length > 0)
					recPartList.Add(part.ToString());

				foreach (string recipient in recPartList)
				{
					if(!string.IsNullOrEmpty(recipient.Trim()))
					{
						string[] recipParts = GetMessageRecipient(recipient);
						recipients.Add(new GXMailRecipient(recipParts[0], recipParts[1]));
					}
				}
			}
		}

		public string GetMessageSubject()
		{
			return qpDecoder.DecodeHeader(GetField("Subject"));
		}

		public DateTime GetDateSent()
		{
			return GetMessageDate(GetField("Date"));
		}

		public DateTime GetDateReceived()
		{            
			string received = GetField("Received");
			int pos = received.LastIndexOf(";");
			if(pos != -1)
			{
				return GetMessageDate(received.Substring(pos+1).Trim());
			}
			return GetDateSent();
		}

        /*
         *  General sytax as defined in RFC822 for Date header is:
         *  [day ","] date time
         *  where
         *  day = Mon/Tue/Wed/Thu/Fri/Sat/Sun
         *  date = 1*2DIGITS month 2DIGITS
         *  month = Jan/Feb/Mar/Apr/May/Jun/Jul/Aug/Sep/Oct/Nov/Dec
         *  time = hour zone
         *  hour = 2DIGIT:2DIGIT[:2DIGIT] (hh:mm:ss) 00:00:00 - 23:59:59
         *  zone = (( "+" / "-" ) 4DIGIT) UT/GMT/EST/EDT/CST/CDT/MST/MDT/PST/PDT/1ALPHA
        */
        internal static DateTime GetMessageDate(string stringDate)
		{
            DateTime messageTime;
			try
			{
				bool oldDateSpec = (stringDate.IndexOf("/") != -1);
				bool hasStrDay = (stringDate.IndexOf(",") > 0); //StrDay is not empty
				string dateStr = RemoveQuotes(stringDate.Replace("/", " ").Replace(",", "").Trim());
				string[] dateParts = dateStr.Split(" ".ToCharArray());

				int i = hasStrDay?0:-1;
				if (!IsNumber(dateParts[i+1]))
					i = i + 1;

				if (oldDateSpec)
				{
					string[] tmpDateParts = new string[dateParts.Length];
					for(int j=0; j<dateParts.Length; j++)
					{
						if (j == (i+1))
							tmpDateParts[j] = dateParts[i+2];
						else if (j == (i+2))
							tmpDateParts[j] = dateParts[i+1];
						else
							tmpDateParts[j] = dateParts[j];
					}
					dateParts = tmpDateParts;
				}

				//parse date
				int day = int.Parse(dateParts[i+1]);
				int month = oldDateSpec?int.Parse(dateParts[i+2]):GetMonthIndex(dateParts[i+2]);
				int year = int.Parse(dateParts[i+3]);

				//parse time
				string[] tmpTime = dateParts[i+4].Split(":".ToCharArray());
				int hr = int.Parse(tmpTime[0]);
				int mm = 0, ss = 0;

				if (tmpTime.Length > 1) 
					mm = int.Parse(tmpTime[1]);

				if (tmpTime.Length > 2) 
					ss = int.Parse(tmpTime[2]);

                string timeZoneOffset = string.Empty;
                if (dateParts.Length > i + 5)
                {
                    timeZoneOffset = dateParts[i + 5];
                }

                messageTime = DateTimeUtil.DateTimeWithTimeZoneToUTC(year, month, day, hr, mm, ss, timeZoneOffset);              
                if (GeneXus.Application.GxContext.Current != null)
                {                    
                    messageTime = DateTimeUtil.FromTimeZone(messageTime, "Etc/UTC", GeneXus.Application.GxContext.Current);
                }
                else
                {
                    messageTime = messageTime.ToLocalTime();
                }
			}
			catch
			{
				messageTime = DateTime.Now;
			}
			return messageTime;
		}

		private static bool IsNumber(string strNum)
		{
			try
			{
				int.Parse(strNum);
				return true;
			}
			catch {}
			return false;
		}

		private static int GetMonthIndex(string month)
		{
			for(int i = 1; i <= monthList.Values.Count; i++)
			{
				string monthNamt = ((string)monthList[i]).Substring(0,3).ToLower(new System.Globalization.CultureInfo("en-US"));
				if(monthNamt == month.ToLower()) return i; 
			}
			return 1;
		}

		private static string RemoveQuotes(string fileName)
		{
			StringBuilder output = new StringBuilder();
			int len = fileName.Length;
			for(int i = 0; i < len; i++)
			{
				if(fileName[i] != '"')
				{
					output.Append(fileName[i]);
				}
			}

			return output.ToString();
		}
	}
}
