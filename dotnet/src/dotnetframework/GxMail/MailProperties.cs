using System;
using System.Text;
using System.Collections;

namespace GeneXus.Mail.Internals.Pop3
{
	
	internal class MailProperties : Hashtable
	{
		public MailProperties() : base()
		{
			PutKey("Content-Type: text/plain");
		}

		public void PutKey(string reply)
		{
			int pos = reply.IndexOf(":");
			if(pos != -1)
			{
				string key   = reply.Substring(0, pos);
				string val = reply.Substring(pos + 1).Replace("\t", " ").Replace(((char)13).ToString(), " ").Replace(((char)10).ToString(), " ").Trim();
				
				if(key.ToUpper().Equals("SUBJECT"))
				{
					val = JapaneseMimeDecoder.Decode(val);
				}
				
				if(this[key.ToUpper()] == null)
				{
					Add(key.ToUpper(), val);
				}
				else if(!key.ToUpper().Equals("RECEIVED")) //If it is the header Received, the first one is saved
				{
					this[key.ToUpper()] = val;
				}
			}
		}

		public string GetKeyPrincipal(string key)
		{
			string val = (string)this[key.ToUpper()];
		
			if(val != null)
			{
				int pos = val.IndexOf(";");
				if(pos >= 0)
				{
					return val.Substring(0, pos);
				}

				return val;
			}

			return "";
		}

		public string GetKeyProperty(string key, string property)
		{
			string val = (string)this[key.ToUpper()];
			property = property.ToUpper();

			if(val != null)
			{
				string[] valParts = val.Split(new char[] { ';' });
				for(int i=0; i<valParts.Length; i++)
				{
					string part = valParts[i].Trim();
					if(part.ToUpper().StartsWith(property + "="))
					{
						int eqIdx = part.IndexOf("=") + 1;
						return part.Substring(eqIdx, part.Length - eqIdx);
					}
				}
			}

			return "";
		}

		public string GetMimeMediaSubtype()
		{
			string mediaType = GetKeyPrincipal("Content-Type");
			int pos = mediaType.IndexOf("/");

			if(pos >= 0)
			{
				return mediaType.Substring(pos + 1, mediaType.Length - (pos + 1));
			}

			return mediaType.ToLower();
		}

		public string GetMimeMediaType()
		{
			string mediaType = GetKeyPrincipal("Content-Type");
			int pos = mediaType.IndexOf("/");

			if(pos >= 0)
			{
				return mediaType.Substring(0, pos);
			}

			return mediaType;
		}
	
		public string GetCharset()
		{
			string mediaType = (string)this["CONTENT-TYPE"];
			if (mediaType != null)
			{
				int pos = mediaType.IndexOf("charset=");
				if(pos >= 0)
				{
					pos = pos + 8;
					int pos2 = mediaType.IndexOf(";", pos);
					if (pos2 > 0)
						mediaType = mediaType.Substring(pos, pos2 - pos);
					else
						mediaType = mediaType.Substring(pos);
					if (mediaType.StartsWith("\""))
						mediaType = mediaType.Substring(1);
					if (mediaType.EndsWith("\""))
						mediaType = mediaType.Substring(0, mediaType.Length - 1);
					return mediaType;
				}
			}
			return "UTF-8";
		}
	
		public string GetField(string field)
		{
			string ret = (string)this[field.ToUpper()];
			return ret == null?"":ret;
		}
	}
}
