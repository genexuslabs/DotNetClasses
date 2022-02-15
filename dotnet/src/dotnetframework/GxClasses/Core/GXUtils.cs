#define WIN32

namespace GeneXus.Utils
{
	using System;
	using GeneXus.Application;
	using TZ4Net;
	using Cache;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using log4net;
	using System.Text.RegularExpressions;
	using System.Data.SqlTypes;
	using GeneXus.Services;

	public class GxMail
	{
		public static int Send(string to, string cc, string bcc, string subject, string body, string sattch, string fromString)
		{
			
			return 1;
		}
		public static int LogOff()
		{
			return 1;
		}
		public static int LoginPOP3(string to, string cc, string bcc, int i, int w, string from)
		{
			return 1;
		}
		public static int LoginSMTP(string to, string cc, string bcc)
		{
			return 1;
		}
		public static int Login(string userName)
		{
			return 1;
		}
		public static int GXMSnd(string a, string b, string c, string d)
		{
			return 1;
		}
		public static int GXMSndb(string a, string b, string c, string d, int i)
		{
			return 1;
		}
		public static void Receive()
		{
		}
		public static void MError()
		{
		}
		public static short Logout()
		{
			return 1;
		}
		public static void AttachDir()
		{
		}
		public static void DisplayMessages()
		{
		}
		public static void AddressFormat()
		{
		}
	}
	
	public class BooleanUtil
	{
		public static bool Val(string valString)
		{
			if (string.IsNullOrEmpty(valString))
				return false;
			else
			{
				valString = valString.Trim();
				return (valString.Equals("true", StringComparison.OrdinalIgnoreCase) || valString.Equals("1"));
			}
		}
	}
	
	public class TimeZoneUtil
	{
		public static OlsonTimeZone GetInstanceFromWin32Id(string sTZ)
		{
			lock (OlsonTimeZone.SyncRoot)
			{
				try
				{
					return OlsonTimeZone.GetInstanceFromWin32Id(sTZ);
				}
				catch (ArgumentException)
				{
					return OlsonTimeZone.CurrentTimeZone;
				}
			}
		}

		public static OlsonTimeZone GetInstanceFromOlsonName(string sTZ)
		{
			lock (OlsonTimeZone.SyncRoot)
			{
				return OlsonTimeZone.GetInstanceFromOlsonName(sTZ);
			}
		}
	}
	
	public class LVCharUtil
	{

		public static int Lines(string s, decimal len)
		{
			try
			{
				return Convert.ToInt32(Math.Ceiling(s.Length / (double)len));
			}
			catch
			{   
				return -1;
			}
		}
		public static string GetLine(string s, int line, short len)
		{
			int start = ((line - 1) * len);
			if (start > s.Length || start < 0)
				return "";
			if (start + len > s.Length)
				return s.Substring(start, s.Length - start);
			else
				return s.Substring(start, len);
		}
		public static int LinesWrap(string s, decimal len1)
		{
			LineType lineType, lastLineType = LineType.Full; ;
			int len = Convert.ToInt32(len1);
			int i = 0;
			int end = -1;
			int start = 0;
			s = NormalizeNewLine(s);
			while (true)
			{
				if (lastLineType == LineType.NewLineEnded)
					
					start = end + 1;
									
				else if (lastLineType == LineType.SpaceEnded)
					
					start = end + 1 + 1;
				else
					start = end + 1;
				if (start >= s.Length)
					return i;
				end = getEnd(start, len, s);
				lineType = getEndType(s, end);
				if (lineType == LineType.Full)
				{
					while (end > start)
					{
						if (end == s.Length - 1)    
							break;
						if (s[end] == ' ')              
							break;
						if (s[end] == '\n')             
							if (end > 0)
								if (s[end - 1] == '\r')
									break;
						end--;
					}
					if (end == start)
						end = getEnd(start, len, s);
				}
				lastLineType = lineType;
				i++;
			}
		}
		public static string GetLineWrap(string s, int line, short len)
		{
			LineType lineType, lastLineType = LineType.Full;
			int i = 0;
			int end = -1;
			int start = 0;
			s = NormalizeNewLine(s);
			while (i < line)
			{
				if (lastLineType == LineType.NewLineEnded)
					
					start = end + 1;
									
				else if (lastLineType == LineType.SpaceEnded)
					
					start = end + 1 + 1;
				else
					start = end + 1;
				if (start >= s.Length)
					return "";
				end = getEnd(start, len, s);
				lineType = getEndType(s, end);
				if (lineType == LineType.Full)
				{
					while (end > start)
					{
						if (end == s.Length - 1)    
							break;
						if (s[end] == ' ')              
							break;
						if (s[end] == '\n')             
							if (end > 0)
								if (s[end - 1] == '\r')
									break;
						end--;
					}
					if (end == start)
						end = getEnd(start, len, s);
				}
				lastLineType = lineType;
				i++;
			}
			return s.Substring(start, end - start + 1).Replace(StringUtil.NewLine(), "");
		}
		static int getEnd(int start, int len, string s)
		{
			int end = Math.Min(start + len - 1, s.Length - 1);
			
			int newLinePos;
			if (s.Length - 1 >= end + 2)
				newLinePos = s.Substring(start, end + 2 - start + 1).IndexOf(StringUtil.NewLine());
			else if (s.Length - 1 >= end + 1)
				newLinePos = s.Substring(start, end + 1 - start + 1).IndexOf(StringUtil.NewLine());
			else
				newLinePos = s.Substring(start, end - start + 1).IndexOf(StringUtil.NewLine());
			if (newLinePos != -1)
				return start + newLinePos + StringUtil.NewLine().Length - 1;

			return end;
		}
		static LineType getEndType(string s, int end)
		{
			if (s.Length - 1 >= end + 1)
				if (s[end + 1] == ' ')
					return LineType.SpaceEnded;
			if (s.Length - 1 >= end + 2)
				if (s[end + 1] == '\r' && s[end + 2] == '\n')
					return LineType.NewLineEnded;
			if (s.Length - 1 == end)
				return LineType.EOS;

			return LineType.Full;
		}
		enum LineType
		{
			SpaceEnded,
			NewLineEnded,
			EOS,
			Full
		}
		static string NormalizeNewLine(string value)
		{
			char NewLineChar = '\n';
			if (!string.IsNullOrEmpty(value) && value.IndexOf(NewLineChar) >= 0)
				return Regex.Replace(value, "(?<!\r)\n", StringUtil.NewLine());
			else
				return value;
		}
	}

	public class FileIO
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.FileIO));
		const short GX_ASCDEL_BADFMTSTR = -10;
		const short GX_ASCDEL_WRITEERROR = -9;
		const short GX_ASCDEL_INVALIDDATE = -7;
		const short GX_ASCDEL_OVERFLOW = -6;
		const short GX_ASCDEL_INVALIDFORMAT = -5;
		const short GX_ASCDEL_ENDOFDATA = -4;
		const short GX_ASCDEL_OPENERROR = -2;
		const short GX_ASCDEL_INVALIDSEQUENCE = -1;
		const short GX_ASCDEL_SUCCESS = 0;
		FileStream _fsr;
		FileStream _fsw;
		StreamReader _sr;
		StreamWriter _sw;
		string _fldDelimiterR;
		string _fldDelimiterW;
		string _strDelimiterR;
		string _strDelimiterW;
		string _currentLineR;
		string _currentLineW;
		int _lastPos;
		string _encodingR;
		string _encodingW;
		
		enum FileIOStatus
		{
			Closed,
			Open,
			DataReady
		}
		FileIOStatus _readStatus;
		FileIOStatus _writeStatus;

		public short dfropen(string fileName, int recSize, string fldDelimiter, string strDelimiter)
		{
			return dfropen(fileName, recSize, fldDelimiter, strDelimiter, "");
		}
		public short dfropen(string fileName, int recSize, string fldDelimiter, string strDelimiter, string encoding)
		{
			if (_readStatus != FileIOStatus.Closed)
			{
				GXLogging.Error(log, "Error ADF0005: open function in use");
				return GX_ASCDEL_INVALIDSEQUENCE;
			}
			fileName = Path.Combine(GxContext.StaticPhysicalPath(), fileName);
			try
			{
				_fsr = new FileStream(Path.GetFullPath(fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024);
			}
			catch (FileNotFoundException fe)
			{
				GXLogging.Error(log, "Error ADF0001", fe);
				return GX_ASCDEL_OPENERROR;
			}
			catch (DirectoryNotFoundException de)
			{
				GXLogging.Error(log, "Error ADF0001", de);
				return GX_ASCDEL_OPENERROR;
			}
			_fldDelimiterR = CleanDelimiter(fldDelimiter);
			_strDelimiterR = strDelimiter;
			_encodingR = encoding;
			_sr = new StreamReader(_fsr, GXUtil.GxIanaToNetEncoding(encoding, false));
			_lastPos = 0;
			_readStatus = FileIOStatus.Open;
			return GX_ASCDEL_SUCCESS;
		}
		string CleanDelimiter(string s)
		{
			string value = s.Replace(@"\t", "\t");
			
			if (value.Trim().Length == 0)
				return value;
			return value.Trim();
		}
		public short dfrnext()
		{
			_readStatus = FileIOStatus.Open;
			if (_readStatus == FileIOStatus.Closed)
				return GX_ASCDEL_INVALIDSEQUENCE;
			try
			{
				_currentLineR = _sr.ReadLine();
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error ADF0002", e);
			}
			if (_currentLineR == null)
				return GX_ASCDEL_ENDOFDATA;
			_lastPos = 0;
			_readStatus = FileIOStatus.DataReady;
			return GX_ASCDEL_SUCCESS;
		}
		public short dfrgnum(out short num)
		{
			short err;
			num = (short)dfrgnum1(out err);
			return err;
		}
		public short dfrgnum(out int num)
		{
			short err;
			num = (int)dfrgnum1(out err);
			return err;
		}
		public short dfrgnum(out long num)
		{
			short err;
			num = (long)dfrgnum1(out err);
			return err;
		}
		public short dfrgnum(out double num)
		{
			short err;
			num = (double)dfrgnum1(out err);
			return err;
		}
		public short dfrgnum(out decimal num)
		{
			short err;
			num = (decimal)dfrgnum1(out err);
			return err;
		}
		public double dfrgnum1(out short err)
		{
			if (_readStatus != FileIOStatus.DataReady)
			{
				err = GX_ASCDEL_INVALIDSEQUENCE;
				GXLogging.Error(log, "Error ADF0004 o ADF0006");
				return GX_ASCDEL_SUCCESS;
			}
			string fld;
			err = getNextFld(-1, out fld);
			if (err == GX_ASCDEL_SUCCESS)
			{
				string outStr = StringUtil.ExtractNumber(fld, CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
				try
				{
					err = GX_ASCDEL_SUCCESS;
					return Convert.ToDouble(outStr, CultureInfo.InvariantCulture.NumberFormat);
				}
				catch (Exception e)
				{
					err = GX_ASCDEL_INVALIDFORMAT;
					GXLogging.Error(log, "Error ADF0008", e);
					return GX_ASCDEL_SUCCESS;
				}
			}
			else
			{
				return err;
			}
		}
		public short dfrgtxt(out string s, int length)
		{
			s = null;
			if (_readStatus != FileIOStatus.DataReady)
			{
				GXLogging.Error(log, "Error ADF0004 o ADF0006");
				return GX_ASCDEL_INVALIDSEQUENCE;
			}
			string fld;
			short err = getNextFld(length, out fld);
			fld = stripDelimiters(fld);
			s=fld;
			if (err == GX_ASCDEL_SUCCESS)
			{
				if (length != 0)
				{
					try
					{
						err = substringByte(_encodingR, fld, length, out s);
					}
					catch
					{
						return GX_ASCDEL_OVERFLOW;
					}
				}
			}
			return err;
		}
		public short dfrgdate(out DateTime d, string fmt, string sep)
		{
			short retval;
			d = DateTimeUtil.NullDate();
			int year = 0; int month = 0; int day = 0;
			if (_readStatus != FileIOStatus.DataReady)
			{
				GXLogging.Error(log, "Error ADF0004 o ADF0006");
				return GX_ASCDEL_INVALIDSEQUENCE;
			}

			string fld;
			retval =  getNextFld(-1, out fld);

			if (retval == GX_ASCDEL_SUCCESS)
			{
				try
				{
					string[] values = fld.Split(sep[0]);

					for (int i = 0; i < 3; i++)
					{
						int value = Convert.ToInt32(values[i]);
						switch (fmt[i])
						{
							case 'y':
								year = value;
								break;
							case 'm':
								month = value;
								break;
							case 'd':
								day = value;
								break;
							default:
								return GX_ASCDEL_BADFMTSTR;
						}
					}

					if (month == 0 && day == 0 && year == 0)
					{
						d = DateTimeUtil.NullDate();
					}
					else if (month < 1 || month > 12 || day < 1 || day > 31)
					{
						retval = GX_ASCDEL_INVALIDDATE;
						GXLogging.Error(log, "Error ADF0010");
					}
					else
					{
						d = new DateTime(year, month, day);
					}
				}
				catch
				{
					retval = GX_ASCDEL_INVALIDFORMAT;
				}
			}
			return retval;
		}
		short getNextFld(int length, out string field)
		{
			short retval = GX_ASCDEL_SUCCESS;
			int dlPos;
			if (_encodingR.Trim().Length > 0)
			{
				return getNextFldByte(length, out field);
			}

			if (length == -1 && _fldDelimiterR.Length == 0)
			{
				field = string.Empty;
				return retval;
			}

			if (_lastPos >= _currentLineR.Length)
			{
				field = string.Empty;
				return retval;
			}
			
			int oPos = _lastPos;
			
			if (_fldDelimiterR.Length == 0) 
			{
				if (length > 0)
				{
					if (_currentLineR.Length > _lastPos + length)
					{
						dlPos = _lastPos + length;
						retval = GX_ASCDEL_OVERFLOW;
					}
					else
					{
						dlPos = _currentLineR.Length;
					}
				}
				else
				{ 
					dlPos = _currentLineR.Length;
				}
				_lastPos = dlPos;
			}
			else
			{
				if (_strDelimiterR.Length > 0 && _currentLineR.IndexOf(_strDelimiterR, _lastPos) == _lastPos) 
				{
					int strEndDelimiterPos = _currentLineR.IndexOf(_strDelimiterR, _lastPos + _strDelimiterR.Length);
					dlPos = _currentLineR.IndexOf(_fldDelimiterR, strEndDelimiterPos + _strDelimiterR.Length);
				}
				else
				{
					dlPos = _currentLineR.IndexOf(_fldDelimiterR, _lastPos);
				}
				if (dlPos == -1)    
					dlPos = _currentLineR.Length;
				_lastPos = dlPos + _fldDelimiterR.Length;       
			}
			if (dlPos < oPos)
			{
				field = string.Empty;
			}
			else
			{
				field = _currentLineR.Substring(oPos, dlPos - oPos);
			}
			return retval;
		}
		short getNextFldByte(int length, out string field)
		{
			short retval = GX_ASCDEL_SUCCESS;
			int dlPos;
			Encoding enc = GXUtil.GxIanaToNetEncoding(_encodingR, false);
			byte[] currentLineBytes = enc.GetBytes(_currentLineR);
			byte[] fldDelimiterBytes = enc.GetBytes(_fldDelimiterR);
			byte[] strDelimiterBytes = enc.GetBytes(_strDelimiterR);

			if (length == -1 && _fldDelimiterR.Length == 0)
			{
				field = string.Empty;
				return retval;
			}

			if (_lastPos >= currentLineBytes.Length)
			{
				field = string.Empty;
				return retval;
			}
			
			int oPos = _lastPos;
			
			if (_fldDelimiterR.Length == 0) 
			{
				if (length > 0)
				{
					if (currentLineBytes.Length > _lastPos + length)
					{
						dlPos = _lastPos + length;
						retval = GX_ASCDEL_OVERFLOW;
					}
					else
						dlPos = currentLineBytes.Length;
				}
				else
				{ 
					dlPos = _currentLineR.Length;
				}
				_lastPos = dlPos;       
			}
			else
			{
				if (_strDelimiterR.Length > 0 && IndexOfArray(currentLineBytes, strDelimiterBytes, _lastPos) == _lastPos) 
				{
					int strEndDelimiterPos = IndexOfArray(currentLineBytes, strDelimiterBytes, _lastPos + strDelimiterBytes.Length);
					dlPos = IndexOfArray(currentLineBytes, fldDelimiterBytes, strEndDelimiterPos + strDelimiterBytes.Length);
				}
				else
				{
					dlPos = IndexOfArray(currentLineBytes, fldDelimiterBytes, _lastPos);
				}

				if (dlPos == -1)    
					dlPos = currentLineBytes.Length;
				_lastPos = dlPos + fldDelimiterBytes.Length;

				if (length > 0)
				{
					if ((dlPos - oPos) > length)
						retval = GX_ASCDEL_OVERFLOW;
				}
			}

			if (dlPos < oPos)
			{
				field = string.Empty;
			}
			else
			{
				field = enc.GetString(currentLineBytes, oPos, dlPos - oPos);
			}
			return retval;
		}

		private int IndexOfArray(byte[] arr1, byte[] arr2, int pos)
		{
			if (arr2.Length == 0 || arr1.Length == 0)
				return GX_ASCDEL_INVALIDSEQUENCE;
			if (arr2.Length > arr1.Length)
				return GX_ASCDEL_INVALIDSEQUENCE;
			bool equal = false;

			for (int i = pos; i < arr1.Length; i++)
			{
				equal = false;
				if (arr1[i] == arr2[0] && (arr2.Length - 1) <= (arr1.Length - (i + 1)))
				{
					equal = true;
					int i1 = 0;
					for (int j = i1; j < arr2.Length; j++)
					{
						equal = equal && (arr1[i + i1] == arr2[j]);
						i1++;
					}
					if (equal) return i;
				}
			}
			return GX_ASCDEL_INVALIDSEQUENCE;

		}

		string stripDelimiters(string s)
		{
			
			if (_strDelimiterR.Length == 0) 
				return s;
			if (s.Length == 0)
				return s;
			if (s.StartsWith(_strDelimiterR))
				s = s.Remove(0, 1);
			if (s.Substring(s.Length - 1) == _strDelimiterR)
				s = s.Remove(s.Length - 1, 1);
			return s;
		}
		public short dfrclose()
		{
			if (_readStatus == FileIOStatus.Closed)
				return GX_ASCDEL_INVALIDSEQUENCE;
			try
			{
				_sr.Close();
				_fsr.Close();
				_readStatus = FileIOStatus.Closed;
			}
			catch { }
			return GX_ASCDEL_SUCCESS;
		}
		public short dfwopen(string fileName, string fldDelimiter, string strDelimiter)
		{
			return dfwopen(fileName, fldDelimiter, strDelimiter, "");
		}
		public short dfwopen(string fileName, string fldDelimiter, string strDelimiter, string encoding)
		{
			return dfwopen(fileName, fldDelimiter, strDelimiter, 0, encoding);
		}
		public short dfwopen(string fileName, string fldDelimiter, string strDelimiter, int append, string encoding)
		{
			if (_writeStatus != FileIOStatus.Closed)
			{
				GXLogging.Error(log, "Error ADF0005: open function in use");
				return GX_ASCDEL_INVALIDSEQUENCE;
			}
			try
			{
				FileMode mode = FileMode.Create;
				if (append == 1)
				{
					mode = FileMode.Append;
				}
				fileName = Path.Combine(GxContext.StaticPhysicalPath(), fileName);
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				_fsw = new FileStream(fileName, mode, FileAccess.Write, FileShare.ReadWrite, 1024);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			}
			catch (DirectoryNotFoundException e)
			{
				GXLogging.Error(log, "Error ADF0001", e);
				return GX_ASCDEL_OPENERROR;
			}
			_fldDelimiterW = CleanDelimiter(fldDelimiter);
			_strDelimiterW = strDelimiter;
			_encodingW = encoding;
			_sw = new StreamWriter(_fsw, GXUtil.GxIanaToNetEncoding(encoding, false));
			_currentLineW = null;
			_writeStatus = FileIOStatus.Open;
			return GX_ASCDEL_SUCCESS;
		}
		public void setNewLineBehavior(string newline)
		{
			if (_sw != null)
			{
				_sw.NewLine = newline;
			}
		}
		public short dfwnext()
		{
			if (_writeStatus != FileIOStatus.DataReady)
				return GX_ASCDEL_INVALIDSEQUENCE;
			try
			{
				if (_currentLineW != null)
				{
					_sw.WriteLine(_currentLineW);
					_currentLineW = null;
				}
				_writeStatus = FileIOStatus.Open;
				return GX_ASCDEL_SUCCESS;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Error ADF0003", ex);
				return GX_ASCDEL_WRITEERROR;
			}
		}
		public short dfwpnum(decimal num, int dec)
		{
			if (_writeStatus == FileIOStatus.Closed)
			{
				GXLogging.Error(log, "Error ADF0004");
				return GX_ASCDEL_INVALIDSEQUENCE;
			}
			appendFld(StringUtil.Str(num, 18, dec).TrimStart(null));
			return GX_ASCDEL_SUCCESS;
		}
		public short dfwptxt(string s, int len)
		{
			if (_writeStatus == FileIOStatus.Closed)
			{
				GXLogging.Error(log, "Error ADF0004");
				return GX_ASCDEL_INVALIDSEQUENCE;
			}
			string substr;
			short retval = substringByte(_encodingW, s, len, out substr);
			if (retval == GX_ASCDEL_SUCCESS)
			{
				appendFld(_strDelimiterW + substr + _strDelimiterW);
			}
			return retval;
		}
		short substringByte(string sEnc, string s, int len, out string substring)
		{
			short retval = GX_ASCDEL_SUCCESS;
			if (sEnc.Trim().Length == 0)
			{
				if (len == 0)
				{
					substring = s;
				}
				else if (s.Length <= len)
				{
					substring = s;
				}
				else
				{
					retval = GX_ASCDEL_OVERFLOW;
					substring = s.Substring(0, len);
				}
			}
			else
			{
				Encoding enc = GXUtil.GxIanaToNetEncoding(sEnc, false);
				byte[] b1 = enc.GetBytes(s);
				if (len == 0)
				{
					substring = s;
				}
				else if (b1.Length <= len)
				{
					substring = s;
				}
				else
				{
					retval = GX_ASCDEL_OVERFLOW;
					substring = enc.GetString(b1, 0, len);
				}
			}
			return retval;
		}

		public short dfwpdate(DateTime dt, string fmt, string sep)
		{
			if (_writeStatus == FileIOStatus.Closed)
			{
				GXLogging.Error(log, "Error ADF0004");
				return GX_ASCDEL_INVALIDSEQUENCE;
			}
			try
			{
				appendFld(new DateTimeUtil(CultureInfo.InvariantCulture, AMPMFmt.T12).DToC(dt, DateTimeUtil.PictureFormatFromString(fmt), sep));
			}
			catch (FormatException ex)
			{
				GXLogging.Error(log, "Error ADF0012", ex);
				return GX_ASCDEL_BADFMTSTR;
			}
			return GX_ASCDEL_SUCCESS;
		}
		int appendFld(string s)
		{
			if (_currentLineW == null)
				_currentLineW = s;
			else
				_currentLineW = _currentLineW + _fldDelimiterW + s;
			_writeStatus = FileIOStatus.DataReady;
			return 0;
		}
		public short dfwclose()
		{
			if (_writeStatus == FileIOStatus.Closed)
				return GX_ASCDEL_INVALIDSEQUENCE;
			try
			{
				_sw.Close();
				_fsw.Close();
				_writeStatus = FileIOStatus.Closed;
			}
			catch { }
			return GX_ASCDEL_SUCCESS;
		}

	}
	public class GxRegex
	{
		static int lastErrCode;
		static string lastErrDescription;
		static void resetError()
		{
			lastErrCode = 0;
			lastErrDescription = "";
		}
		static void setError(int errCode, string errDescription)
		{
			lastErrCode = errCode;
			lastErrDescription = errDescription;
		}
		static string normalizeText(string txt)
		{
			
			return txt.Replace(StringUtil.NewLine(), "\n");
		}
		static public bool IsMatch(string txt, string rex)
		{
			resetError();
			try
			{
				Regex r = new Regex(rex, RegexOptions.Multiline);
				return r.Match(normalizeText(txt)).Success;
			}
			catch (Exception e)
			{
				setError(1, e.Message);
			}
			return false;
		}
		static public string Replace(string txt, string rex, string repl)
		{
			resetError();
			try
			{
				Regex r = new Regex(rex, RegexOptions.Multiline);
				return r.Replace(normalizeText(txt), repl);
			}
			catch (Exception e)
			{
				setError(1, e.Message);
			}
			return "";
		}
		static public GxSimpleCollection<string> Split(string txt, string rex)
		{
			resetError();
			try
			{
				Regex r = new Regex(rex, RegexOptions.ExplicitCapture | RegexOptions.Multiline);
				GxSimpleCollection<string> c = new GxSimpleCollection<string>();
				c.AddRange(r.Split(normalizeText(txt)));
				return c;
			}
			catch (Exception e)
			{
				setError(1, e.Message);
			}
			return new GxSimpleCollection<string>();
		}
		static public GxUnknownObjectCollection Matches(string txt, string rex)
		{
			resetError();
			try
			{
				Regex r = new Regex(rex, RegexOptions.Multiline);
				MatchCollection mc = r.Matches(normalizeText(txt));
				GxUnknownObjectCollection c = new GxUnknownObjectCollection();
				foreach (Match m in mc)
					c.Add(new GxRegexMatch(m));
				return c;
			}
			catch (Exception e)
			{
				setError(1, e.Message);
			}
			return new GxUnknownObjectCollection();
		}
		public static int GetLastErrCode()
		{
			return lastErrCode;
		}
		public static string GetLastErrDescription()
		{
			return lastErrDescription;
		}
	}

	public class GxRegexMatch
	{
		string value;
		GxStringCollection groups;

		public GxRegexMatch()
		{
			value = "";
			groups = new GxStringCollection();
		}
		public GxRegexMatch(Match m)
		{
			groups = new GxStringCollection();
			if (m.Success)
			{
				value = m.Groups[0].Value;
				for (int i = 1; i < m.Groups.Count; i++)
					groups.Add(m.Groups[i].Value);
			}
		}
		public GxStringCollection Groups
		{
			get { return groups; }
		}
		public string Value
		{
			get { return value; }
		}
	}

	public enum HTMLElement
	{
		IMG,
		SPAN,
		INPUT,
		META,
		LINK,
		OPTION
	}
	public enum HTMLDocType
	{
		HTML4,
		HTML4S,
		XHTML1,
		HTML5,
		NONE,
		UNDEFINED
	}

	public static class GuidUtil
	{
		public static int Compare(Guid guidA, Guid guidB, int mode)
		{
			if (mode == 1) 
			{
				SqlGuid sqlGuidA = guidA;
				SqlGuid sqlGuidB = guidB;
				return sqlGuidA.CompareTo(sqlGuidB);
			}

			return String.Compare(guidA.ToString(), guidB.ToString());
		}
	}

	public class CacheAPI
	{
        private const string DEFAULT_CACHEID = "DefaultCache";
        public static CacheAPI Database { get { return new CacheAPI(CacheFactory.CACHE_DB); } }
        public static CacheAPI FilesCache { get { return new CacheAPI(CacheFactory.CACHE_FILES); } }
        public static CacheAPI SmartDevices { get { return new CacheAPI(CacheFactory.CACHE_SD); } }

		private static ICacheService cache = CacheFactory.Instance;
		private string cacheId;

		public CacheAPI()
        {
            cacheId = DEFAULT_CACHEID;
        }

		public CacheAPI(string name)
		{
			cacheId = name;
		}

		public static CacheAPI GetCache(string name)
		{
			return new CacheAPI(name);
		}

		public static void ClearAllCaches()
		{
			cache.ClearAllCaches();
		}

		public void Set(string key, string value, int durationMinutes)
		{
			cache.Set<string>(cacheId, key, value, durationMinutes);
		}

		public string Get(string key)
		{
			string value = "";
			cache.Get<string>(cacheId, key, out value);
			return value;
		}

		public Boolean Contains(string key)
		{
			string value = "";
			return cache.Get<string>(cacheId, key, out value);
		}

		public void Remove(string key)
		{
			if (cacheId == CacheFactory.CACHE_SD && !string.IsNullOrEmpty(key))
				key.ToLower();
			cache.Clear(cacheId, key);
		}

		public void Clear()
		{
			cache.ClearCache(cacheId);
		}
	}
	public class GxStorageProvider
	{
		protected ExternalProvider provider;
		public GxStorageProvider()
		{
			provider = ServiceFactory.GetExternalProvider();
		}
		public GxStorageProvider(GxStorageProvider other)
		{
			provider = other.provider;
		}
		void ValidProvider()
		{
			if (provider == null)
				throw new Exception("External provider not found");
		}
		public bool Upload(string filefullpath, string storageobjectfullname, GxFile uploadedFile, GXBaseCollection<SdtMessages_Message> messages)
		{
			try
			{				
				ValidProvider();
				GxFileType acl = GxFileType.PublicRead;
				if (String.IsNullOrEmpty(storageobjectfullname))
				{
					storageobjectfullname = Path.GetFileName(filefullpath);
				}				
				string url = provider.Upload(filefullpath, storageobjectfullname, acl);
				uploadedFile.FileInfo = new GxExternalFileInfo(storageobjectfullname, url, provider, acl);
				return true;
			}
			catch (Exception ex)
			{
                StorageMessages(ex, messages);
                return false;
			}
		}
		public bool UploadPrivate(string filefullpath, string storageobjectfullname, GxFile uploadedFile,GXBaseCollection<SdtMessages_Message> messages)
		{
			try
			{
				ValidProvider();
				if (String.IsNullOrEmpty(storageobjectfullname))
				{
					storageobjectfullname=Path.GetFileName(filefullpath);
				}
				GxFileType acl = GxFileType.Private;
				string url = provider.Upload(filefullpath, storageobjectfullname, acl);
				uploadedFile.FileInfo = new GxExternalFileInfo(storageobjectfullname, url, provider, acl);
				return true;
			}
			catch (Exception ex)
			{
                StorageMessages(ex, messages);
                return false;
			}
		}
        public bool Download(string storageobjectfullname, GxFile localFile, GXBaseCollection<SdtMessages_Message> messages)
        {
            try
            {
                ValidProvider();
                string destFileName;

                if (Path.IsPathRooted(localFile.GetAbsoluteName()))
                    destFileName = localFile.GetAbsoluteName();
                else
                    destFileName = Path.Combine(GxContext.StaticPhysicalPath(), localFile.Source);
                provider.Download(storageobjectfullname, destFileName, GxFileType.PublicRead);
                return true;
            }
			catch (Exception ex)
			{
                StorageMessages(ex, messages);
                return false;
			}
		}

		public bool DownloadPrivate(string storageobjectfullname, GxFile localFile, GXBaseCollection<SdtMessages_Message> messages)
		{
			try
			{				
				ValidProvider();
				string destFileName;

				if (Path.IsPathRooted(localFile.GetAbsoluteName()))
					destFileName = localFile.GetAbsoluteName();
				else
					destFileName = Path.Combine(GxContext.StaticPhysicalPath(), localFile.Source);
				provider.Download(storageobjectfullname, destFileName, GxFileType.Private);
				return true;
			}
			catch (Exception ex)
			{
				StorageMessages(ex, messages);
				return false;
			}
		}


		public bool Get(string storageobjectfullname, GxFile externalFile, GXBaseCollection<SdtMessages_Message> messages)
		{
			try
			{
				ValidProvider();
				GxFileType acl = GxFileType.PublicRead;
				string url = provider.Get(storageobjectfullname, acl, 0);
				if (String.IsNullOrEmpty(url))
				{
					GXUtil.ErrorToMessages("Get Error", "File doesn't exists", messages);
					return false;
				}
				else
				{
					externalFile.FileInfo = new GxExternalFileInfo(storageobjectfullname, url, provider, acl);
					return true;
				}
			}
			catch (Exception ex)
			{
                StorageMessages(ex, messages);
                return false;
			}

		}
		
		public bool GetPrivate(string storageobjectfullname, GxFile externalFile, int expirationMinutes, GXBaseCollection<SdtMessages_Message> messages)
		{
            try
            {
				ValidProvider();
				GxFileType acl = GxFileType.Private;
				string url = provider.Get(storageobjectfullname, acl, expirationMinutes);
				if (String.IsNullOrEmpty(url))
				{
					GXUtil.ErrorToMessages("Get Error", "File doesn't exists", messages);
					return false;
				}
				else
				{
					externalFile.FileInfo = new GxExternalFileInfo(storageobjectfullname, url, provider, acl);
					return true;
				}
			}
			catch (Exception ex)
			{
                StorageMessages(ex, messages);
                return false;
			}

		}

		public bool GetDirectory(string directoryFullName, GxDirectory externalDirectory, GXBaseCollection<SdtMessages_Message> messages)
		{
			try
			{
				ValidProvider();
				string path = provider.GetDirectory(directoryFullName);
				if (String.IsNullOrEmpty(path))
				{
					GXUtil.ErrorToMessages("Get Error", "Directory doesn't exists", messages);
					return false;
				}
				else
				{
					externalDirectory.DirectoryInfo = new GxExternalDirectoryInfo(directoryFullName, path, provider);
					return true;
				}		
			}
			catch (Exception ex)
			{
                StorageMessages(ex, messages);
				return false;
			}

		}

        protected void StorageMessages(Exception ex, GXBaseCollection<SdtMessages_Message> messages)
        {
            if (messages != null && ex != null)
            {
                SdtMessages_Message msg = new SdtMessages_Message();
                if (provider!=null && provider.GetMessageFromException(ex, msg))
                {
                    msg.gxTpr_Type = 1;
                    StringBuilder str = new StringBuilder();
                    str.Append(ex.Message);
                    while (ex.InnerException != null)
                    {
                        str.Append(ex.InnerException.Message);
                        ex = ex.InnerException;
                    }
                    msg.gxTpr_Description = str.ToString();
                    messages.Add(msg);
                }
                else {
                    GXUtil.ErrorToMessages("Storage Error", ex, messages);
                }
            }
        }
    }


}



