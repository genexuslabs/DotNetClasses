using GeneXus.Application;
using GeneXus.Configuration;
using log4net;
using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Linq;
#if NETCORE
using Microsoft.AspNetCore.Http;
using TZ4Net;
using GxClasses.Helpers;
using System.Net;
using NUglify;
using NUglify.Html;
#endif
using GeneXus.Web.Security;

using System.Web;
using GeneXus.Data;
using GeneXus.Encryption;

using System.DirectoryServices;
#if !NETCORE
using TZ4Net;
#endif
using GeneXus.Cryptography;
using GeneXus.Data.NTier;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using GeneXus.Storage;
using GeneXus.Services;

namespace GeneXus.Utils
{
	public class GxDefaultProps
	{
		public static string USER_NAME = "GX_USERID";
		public static string PGM_NAME = "PgmName";
		public static string START_TIME = "StartTime";
		public static string USER_ID = "UserId";
		public static string WORKSTATION = "GX_WRKST";

	}

	public class ThreadSafeRandom
	{
		private static RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
		public static double NextDouble()
		{
			lock (random)
			{
				var bytes = new Byte[8];
				random.GetBytes(bytes);

				var ul = BitConverter.ToUInt64(bytes, 0) / (1 << 11);
				double d = ul / (double)(1UL << 53);
				return d;
			}
		}
		public static void NextBytes(byte[] result)
		{
			lock (random)
			{
				random.GetBytes(result);
			}
		}
	}

	public class NumberUtil
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.NumberUtil));

		private NumberFormatInfo numberFormat;
		public NumberUtil(NumberFormatInfo numFmt)
		{
			numberFormat = numFmt;
		}
		public static int Int(double value)
		{
			return Convert.ToInt32(Math.Floor(value));
		}
		public static int Int(decimal value)
		{
			return Int((double)value);
		}
		public static int Int(int value)
		{
			return Int((double)value);
		}
		public static long Int(long value)
		{
			return Convert.ToInt64(Math.Floor(Convert.ToDouble(value)));
		}
		public static double Round(double value, int digits)
		{
			if (digits >= 0 && digits <= 15)//Math.Round Rounding digits must be between 0 and 15, inclusive.
				return Math.Round(value, digits, MidpointRounding.AwayFromZero);
			else
			{
				int sign = Math.Sign(value);
				double scale = DecPow(digits);
				double round = Math.Floor(Math.Abs(value) * scale + 0.5);
				return (sign * round / scale);
			}
		}
		public static decimal Round(decimal value, int digits)
		{
			if (digits >= 0 && digits <= 15)//Math.Round Rounding digits must be between 0 and 15, inclusive.
				return Math.Round(value, digits, MidpointRounding.AwayFromZero);
			else
			{
				int sign = Math.Sign(value);
				double scale = DecPow(digits);
				return (decimal)(sign * Math.Floor((Math.Abs(value) * (decimal)scale) + 0.5M) / (decimal)scale);
			}
		}
		public static double RoundToEven(double num, int dec)
		{
			double pot = DecPow(dec);
			int sign = 1;
			if (num < 0)
				sign = -1;
			num = Math.Round(num * pot * sign);
			return (num / pot) * sign;
		}
		public static decimal RoundToEven(decimal num, int dec)
		{
			decimal pot = (decimal)DecPow(dec);
			int sign = 1;
			if (num < 0)
				sign = -1;
			num = Math.Round(num * pot * sign);
			return (num / pot) * sign;
		}

		public static double DecPow(int digits)
		{
			double pow = 0;
			switch (digits)
			{
				case 1: pow = 10; break;
				case 2: pow = 100; break;
				case 3: pow = 1000; break;
				case 4: pow = 10000; break;
				case 5: pow = 100000; break;
				case 6: pow = 1000000; break;
				case 7: pow = 10000000; break;
				case 8: pow = 100000000; break;
				case 9: pow = 1000000000; break;
				case 10: pow = 10000000000; break;
				case 11: pow = 100000000000; break;
				case 12: pow = 1000000000000; break;
				case 13: pow = 10000000000000; break;
				case 14: pow = 100000000000000; break;
				case 15: pow = 1000000000000000; break;
				case 16: pow = 10000000000000000; break;
				default: pow = Math.Pow(10, digits); break;
			}
			return pow;
		}

		public static double Trunc(double num, int dec)
		{
			int sign = 1;
			if (num < 0)
				sign = -1;
			double pow = DecPow(dec);
			return (Math.Floor(num * pow * sign) / pow) * sign;
		}
		public static decimal Trunc(decimal num, int dec)
		{
			int sign = 1;
			if (num < 0)
				sign = -1;
			decimal pow = (decimal)DecPow(dec);
			num = (decimal)Math.Floor(num * pow * sign);
			return (num / pow) * sign;

		}

		static String StrUnexponentString(String num)
		{
			try
			{
				int epos = num.IndexOf('E');
				if (epos != -1 && (epos + 1) < num.Length)
				{
					String exponent;
					int scaleAdj = 0;
					if (num[epos + 1] == '+')
					{
						if (epos + 2 < num.Length)
						{

							exponent = num.Substring(epos + 2);
						}
						else
						{
							return num;
						}
					}
					else
					{
						exponent = num.Substring(epos + 1);
					}
					if (Int32.TryParse(exponent, out scaleAdj))
					{

						num = num.Substring(0, epos);

						int point = num.IndexOf('.');
						int scale = num.Length - (point == -1 ? num.Length : point + 1) - scaleAdj;

						StringBuilder val = new StringBuilder(point == -1 ? num : num.Substring(0, point) + num.Substring(point + 1));

						val.Append('0', -scale);
						return val.ToString();
					}
					else
						return num;
				}
				else
				{
					return num;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Warn(log, ex, "StrUnexponentString error");
				return num;
			}
		}

		public static string ParseNumber(string valString, string dSep)
		{
			// Parsing stop when finding invalid character.
			// Result has point as decimal separator
			string text = valString.Trim();
			StringBuilder outStr = new StringBuilder();

			bool point = false;
			bool first = true;

			int len = text.Length;

			for (int i = 0; i < len; i++)
			{
				char c = text[i];

				if (c >= '0' && c <= '9')
				{
					outStr.Append(c);
				}
				else if (dSep.Length > 0 && c == dSep[0] && !point)
				{
					outStr.Append('.');
					point = true;
				}
				else if (c == '-' && first)
				{
					outStr.Append('-');
					first = false;
				}
				else
				{
					break;
				}
			}
			return outStr.ToString();
		}
		public static decimal ValParse(string valString, string dSep)
		{
			if (valString == null || valString.Trim().Length == 0)
				return 0;
			string outStr;

			outStr = ParseNumber(StrUnexponentString(valString), dSep);
			try
			{
				decimal result = Convert.ToDecimal(outStr.ToString(), CultureInfo.InvariantCulture.NumberFormat);
				return result;
			}
			catch
			{
				return 0;
			}
		}
		public static decimal ValExtract(string valString, string dSep)
		{
			if (valString == null || valString.Trim().Length == 0)
				return 0;
			string outStr;
			// ExtractNumber ignores invalid characters.La ExtractNumber parsea el valor e ignora los caracteres invalidos.
			outStr = StringUtil.ExtractNumber(valString, dSep);
			try
			{
				decimal result = Convert.ToDecimal(outStr.ToString(), CultureInfo.InvariantCulture.NumberFormat);
				return result;
			}
			catch
			{
				return 0;
			}
		}
		[Obsolete("Val(string valString, bool invariant) is deprecated", false)]
		public static decimal Val(string valString, bool invariant)
		{
			return ValParse(valString, ".");
		}
		public static decimal Val(string valString)
		{
			// valString considers '.' as decimal separator.
			return ValParse(valString, ".");
		}
		public static decimal Val(string valString, string decSep)
		{
			return ValParse(valString, decSep);
		}
		public decimal CToND(string val)
		{
			// CTON considers configuration decimal separtor 
			return ValExtract(val, numberFormat.NumberDecimalSeparator);
		}
		public decimal CToN(String val, string decSep, string thousandsSep)
		{
			if (thousandsSep.Length > 0)
				val = val.Replace(thousandsSep, "");
			if (decSep.Length > 0 && decSep != numberFormat.NumberDecimalSeparator)
				val = val.Replace(decSep, numberFormat.NumberDecimalSeparator);
			return ValExtract(val, numberFormat.NumberDecimalSeparator);
		}
		public static double Random()
		{
			return ThreadSafeRandom.NextDouble();
		}
		internal static string RandomGuid()
		{
			return Guid.NewGuid().ToString();
		}

		public static double RSeed(double i)
		{
			return i;
		}
		public static decimal RSeed(decimal i)
		{
			return i;
		}
		public static double Rand()
		{
			return Math.Abs(Random());
		}
		public static int Aleat()
		{
			return (int)(Random());
		}

	}
	public abstract class GXPicture
	{
		protected string nullMask;
		protected string mask;
		public GXPicture(String picture, int len)
		{
			if (picture.StartsWith("@") && picture.IndexOf('!') > 0)
			{
				setMask(string.Concat(Enumerable.Repeat("!", len)));
			}
			else
			{
				setMask(picture);
			}
		}

		protected void setMask(String mask)
		{
			this.mask = mask;
			this.nullMask = getNullMask(mask);
		}

		public static String getNullMask(String picture)
		{
			return picture.Replace('9', ' ').Replace('X', ' ').Replace('!', ' ').Replace('Z', ' ').Replace('A', ' ').Replace('M', ' ');
		}
		public static bool isSeparator(char c)
		{
			return (c != '9' && c != 'X' && c != '!' && c != 'Z' && c != 'A' && c != 'M');
		}
		public char getCaret(char cIn, int pos, StringBuilder b)
		{
			switch (mask[pos])
			{
				case '9':
					if (!char.IsDigit(cIn))
						return ' ';
					else
						return cIn;

				case 'X':
					return cIn;
				case 'A':
				case 'M':
				case '!':
					return char.ToUpper(cIn);
				case 'Z':
					if (char.IsDigit(cIn) && cIn != '0')
					{
						return cIn;
					}
					else
					{
						if (cIn == '0')
						{
							bool first = true;
							for (int i = 0; i < pos; i++)
							{
								if (b[i] != ' ')
								{
									first = false;
									break;
								}
							}

							if (first)
							{
								return ' ';
							}
							else
							{
								return '0';
							}
						}
					}
					break;
			}

			return ' ';
		}
	}
	public class GXStringPicture : GXPicture
	{
		public GXStringPicture(string mask, int length) : base(mask, length)
		{
		}
		public string FormatValid(string oldText)
		{
			if (string.IsNullOrEmpty(oldText))
				return nullMask;
			else
			{
				if (StringUtil.RTrim(mask).Length == 0)
					return StringUtil.RTrim(oldText);

				StringBuilder ret = new StringBuilder(nullMask);
				int len = nullMask.Length;
				int maskIndex = 0;
				int j = 0;
				char c;
				while (maskIndex < len && j < oldText.Length)
				{
					c = oldText[j];

					if (isSeparator(mask[maskIndex]))
					{
						if (c == mask[maskIndex])
							j++;
						maskIndex++;
					}
					else
					{
						ret[maskIndex] = getCaret(c, maskIndex, ret);
						maskIndex++;
						j++;
					}
				}

				return (ret.ToString());
			}
		}
	}


	public class StringUtil
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.StringUtil));

		private NumberFormatInfo numFmtInfo;
		private const int STR_LEN_DEFAULT = 10;
		private const int STR_DEC_DEFAULT = 0;
		const char ASTER = '%';
		const char QMARK = '_';
		static char[] numbersAndSep = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '-' };
		static char[] numbers = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		public StringUtil(NumberFormatInfo numFmt)
		{
			numFmtInfo = numFmt;
		}

		private static int Compare(double d, decimal m)
		{
			const double decimalMin = (double)decimal.MinValue;
			const double decimalMax = (double)decimal.MaxValue;
			if (d < decimalMin) return -1;
			if (d > decimalMax) return 1;
			return ((decimal)d).CompareTo(m);
		}

		public static string NToC(decimal val, int digits, int decimals, string decSep, string thousandsSep)
		{
			if (decimals == 0 && val == 0)
			{
				return "0".PadLeft(digits);
			}
			else
			{
				if (val != 0)
				{
					//Avoid Round function (poor performance).
					double d1 = NumberUtil.DecPow(digits - decimals - (decimals > 0 ? 1 : 0)) - ((val < 0) ? 1 : 0);
					double d2 = NumberUtil.DecPow(decimals + 1);
					//Return *** when number exceeds the maximum length (digits)
					d1 = d1 - 5 / d2;

					if (Compare(d1, Math.Abs(val)) < 0)//If the value exceeds one that rounded does not fit in the picture
					{
						return StringUtil.Replicate('*', digits);
					}
				}
				string picture = GetPictureFromLD(digits, decimals, thousandsSep.Length);
				string res = string.Format(CultureInfo.InvariantCulture.NumberFormat, picture, val);
				if ((decimals == 0 && thousandsSep.Length == 0))
					return res;
				else
					return ReplaceSeparators(res, decSep, thousandsSep);
			}
		}

		public static string ReplaceLast(string Source, string Find, string Replace)
		{
			int Place = Source.LastIndexOf(Find);
			string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
			return result;
		}

		private static string ReplaceSeparators(string number, string decSep, string thousandsSep)
		{
			StringBuilder sb = new StringBuilder();
			StringBuilder sbStart = new StringBuilder();
			foreach (char c in number)
			{
				if (c == '.')
					sb.Append(decSep);
				else if (c == ',')
					sb.Append(thousandsSep);
				else
					sb.Append(c);
			}
			return sbStart.Append(sb).ToString();
		}
		private void AssignNumberFormat(int decimals)
		{
			try
			{
				numFmtInfo.NumberDecimalDigits = decimals;
			}
			catch (Exception e)
			{
				GXLogging.Error(log, e, "Couldn't AssignNumberFormat");
			}

		}
		public static string BoolToStr(bool value)
		{
			return (value ? "true" : "false");
		}
		public static string BoolToStr(short value)
		{
			return (value == 1 ? "true" : "false");
		}
		public static bool StrToBool(string value)
		{
			return (string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(value, "1", StringComparison.OrdinalIgnoreCase) == 0);
		}
		public static Guid StrToGuid(string sGuid)
		{
			if (string.IsNullOrEmpty(sGuid) || sGuid.Trim().Length == 0)
				return new Guid();
			try
			{
				return new Guid(sGuid);
			}
			catch (FormatException ex)
			{
				GXLogging.Warn(log, ex, "StrToGuid error sGuid:", sGuid);
				return new Guid();
			}
		}
		public static string Str(decimal numToFormat, int lenString)
		{
			return Str(numToFormat, lenString, STR_DEC_DEFAULT);
		}
		public static string Str(decimal numToFormat)
		{
			return Str(numToFormat, STR_LEN_DEFAULT, STR_DEC_DEFAULT);
		}
		public static string Str(decimal val, int digits, int decimals)
		{
			return NToC(val, digits, decimals, ".", "");
		}

		public static string LTrimStr(decimal numToFormat, int lenString)
		{
			return LTrim(Str(numToFormat, lenString));
		}
		public static string LTrimStr(decimal numToFormat)
		{
			return LTrim(Str(numToFormat));
		}
		public static string LTrimStr(decimal val, int digits, int decimals)
		{
			return LTrim(Str(val, digits, decimals));
		}
		private enum FORMAT_SECTION
		{
			POSITIVE_VALUES = 1,
			NEGATIVE_VALUES = -1,
			ZEROS = 0
		}
		private static string GxPictureToNetPicture(string gxpicture, bool separatorsAsLiterals, FORMAT_SECTION section)
		{
			if (string.IsNullOrEmpty(gxpicture))
				return string.Empty;

			bool inDecimals = false;
			StringBuilder strPicture = new StringBuilder("{0,");
			strPicture.Append(gxpicture.Length);
			strPicture.Append(':');
			bool blankwhenzero = true;
			bool explicitSign = (gxpicture[0] == '+');
			bool withoutMinusSign = (gxpicture[0] == '(' && gxpicture[gxpicture.Length - 1] == ')') || gxpicture.EndsWith("DB") || explicitSign;

			if (section == FORMAT_SECTION.NEGATIVE_VALUES && withoutMinusSign)
			//If it has a sign, then use the first section (which by default assigns only a negative sign).
			{
				strPicture.Append(';');//Section Separator.
			}
			if (section == FORMAT_SECTION.ZEROS)
			{
				strPicture.Append(';');//Section Separator.
				strPicture.Append(';');//Section Separator.
			}
			for (int i = 0; i < gxpicture.Length; i++)
			{
				if (gxpicture[i] == 'Z')
				{
					if (inDecimals)
					{
						//The Z on the right of the decimal point are taken as 9

						//The "##" format string causes the value to be rounded to the nearest digit preceding the decimal, 
						//where rounding away from zero is always used. For example, formatting 34.5 with "##" would result in the value 35.
						strPicture.Append('0');
					}
					else
						strPicture.Append('#');
				}
				else if (gxpicture[i] == '9')
				{
					strPicture.Append('0');
					blankwhenzero = false;
				}
				else if (gxpicture[i] == '.')
				{
					inDecimals = true;
					if (i > 0 && strPicture[strPicture.Length - 1] == '#') strPicture[strPicture.Length - 1] = '0';
					if (separatorsAsLiterals)
						strPicture.Append("\".\"");
					else
						strPicture.Append(gxpicture[i]);
				}
				else if (gxpicture[i] == ',')
				{
					if (separatorsAsLiterals)
						strPicture.Append("\",\"");
					else
						strPicture.Append(gxpicture[i]);
				}
				else
				{
					//0,#,.,,,%,E0, E+0, E-0,e0,e+0,e-0, are characters with special meaning in Custom Numeric Format Strings of .net
					//That's why others are delimited by comma
					char delimiter = '"';
					if (gxpicture[i] == '\"')
						delimiter = '\'';
					strPicture.Append(delimiter);

					switch (gxpicture[i])
					{
						case '(':
						case ')':
							//Pictures (99.9) => 12.5    -12.5
							if (section != FORMAT_SECTION.NEGATIVE_VALUES && withoutMinusSign && (i == 0 || i == gxpicture.Length - 1))
							{
								strPicture.Append(' ');
							}
							else
							{
								strPicture.Append(gxpicture[i]);
							};
							break;
						case '+':
							//Pictures +99.9 =>  +12.5       -12.5
							if (explicitSign && i == 0 && section == FORMAT_SECTION.ZEROS)
							{
								strPicture.Append(' ');
							}
							else if (explicitSign && i == 0 && section == FORMAT_SECTION.NEGATIVE_VALUES)
							{
								strPicture.Append('-');
							}
							else
							{
								strPicture.Append('+');
							}
							break;
						case 'D':
							//Picture 99.9DB   => 12.5 CR      12.5 DB 
							if (i + 1 < gxpicture.Length && gxpicture[i + 1] == 'B' && (section != FORMAT_SECTION.NEGATIVE_VALUES))
							{
								if (section == FORMAT_SECTION.POSITIVE_VALUES)
									strPicture.Append('C');
								else
									strPicture.Append(' ');
							}
							else
							{
								strPicture.Append('D');
							}
							break;
						case 'B':
							if (i - 1 >= 0 && gxpicture[i - 1] == 'D' && (section != FORMAT_SECTION.NEGATIVE_VALUES))
							{
								if (section == FORMAT_SECTION.POSITIVE_VALUES)
									strPicture.Append('R');
								else
									strPicture.Append(' ');
							}
							else
							{
								strPicture.Append('B');
							}
							break;
						default:
							strPicture.Append(gxpicture[i]);
							break;

					}
					strPicture.Append(delimiter);
				}
			}
			if (blankwhenzero && section == FORMAT_SECTION.ZEROS)//Z,ZZZ,ZZZ.ZZ format 0.00 to "". sac.20145
			{
				return Replicate(' ', gxpicture.Length);
			}
			else
			{
				return strPicture.Append('}').ToString();
			}
		}
		static bool useLiteralSeparators(string gxpicture)
		{

			// If it has non-numerical characters, then the separators are used as literals
			// to honor the positioning based on the digits and ignoring
			// special characters like - or /. Ex: pic = "999,999-99" if the separators
			// are not literals it is "12,345,6-78", if they are literal it is 123,456-78
			int cpos = gxpicture.IndexOfAny(new char[] { '-', '/' });
			if (cpos == -1)
				return false;
			else
			{
				if (gxpicture[cpos] == '-' && (cpos == 0 || cpos == gxpicture.Length - 1))  // minus sign
					return false;
				else
					return true;
			}
		}

		public static bool CachePictures = true;
		private static ConcurrentDictionary<string, string> m_Pictures = new ConcurrentDictionary<string, string>();
		public static string GetPictureFromLD(int digits, int decimals, int thousandSepLen)
		{
			string picture;
			string key = $"{digits}-{decimals}-{thousandSepLen}";
			if (CachePictures)
			{
				if (m_Pictures.TryGetValue(key, out picture))
					return picture;
			}
			//digits = number of digits to the left of the comma + 1 (decimal point if there are decimals) + number of decimals
			StringBuilder str = new StringBuilder("{0,");
			int decimalPartLen = decimals > 0 ? decimals + 1 : 0;
			int wholePartLen = digits - decimalPartLen;

			// The length of the string will be the maximum between digits and the length of the picture.
			// In the same way, the length of a textblock containing a decimal variable in genexus is calculated.
			int cantSeparators = (wholePartLen / 3 - (wholePartLen % 3 == 0 ? 1 : 0));
			int maxLenFormattedNumber = cantSeparators * thousandSepLen + digits;
			str.Append(maxLenFormattedNumber); //Padleft with white if the string length is less than 'maxLenFormattedNumber'.
			str.Append(':');
			for (int i = wholePartLen; i >= 1; i--)
			{
				if (i == 1)
				{
					str.Append('0');
				}
				else
				{
					if (thousandSepLen != 0 && i != wholePartLen && i % 3 == 0)
					{
						str.Append(",#");
					}
					else
					{
						str.Append('#');
					}
				}
			}
			for (int i = 0; i < decimalPartLen; i++)
			{
				if (i == 0) str.Append('.');
				else str.Append('0');
			}
			str.Append('}');
			picture = str.ToString();
			m_Pictures[key] = picture;
			return picture;
		}
		public static string Concat(string init, string last)
		{
			char[] trimChars = { ' ' };
			StringBuilder fmtString = new StringBuilder(init.TrimEnd(trimChars));
			fmtString.Append(last);
			return fmtString.ToString();
		}
		public static string Concat(string init, string last, string separator)
		{
			char[] trimChars = { ' ' };
			StringBuilder fmtString = new StringBuilder(init.TrimEnd(trimChars));
			fmtString.Append(separator);
			fmtString.Append(last);
			return fmtString.ToString();
		}
		public static string SubStr(string strIn, int first, int len)
		{
			first--;
			try
			{
				if (len < 0)
				{
					return strIn.Substring(first);
				}
				if (strIn.Length <= first + len)
				{
					len = strIn.Length - first;
				}
				return strIn.Substring(first, len);
			}
			catch
			{
				return "";
			}
		}
		public static string Substring(string strIn, int first, int len)
		{
			return SubStr(strIn, first, len);
		}
		public static string SubstringByte(string strIn, int lenInBytes, string encoding)
		{
			string result = string.IsNullOrEmpty(strIn) ? string.Empty : strIn;
			try
			{
				if (ByteCount(result, encoding) < lenInBytes)
				{
					return result;
				}
				else if (result.Length > lenInBytes)
				{
					result = result.Substring(0, lenInBytes);
				}

				while (ByteCount(result, encoding) > lenInBytes && result.Length > 0)
					result = result.Substring(0, result.Length - 1);
				return result;
			}
			catch
			{
				return result;
			}
		}
		public static int StrCmp(string left, string right)
		{
			//string compare CULTURE INDEPENDENT
			return String.CompareOrdinal(RTrim(left), RTrim(right));
		}

		public static bool StrCmp2(string left, string right)
		{
			return String.Compare(left, right) == 0;
		}

		public static string PadR(string text, int size, string fill)
		{
			return Left(RTrim(text) + Replicate(fill, size), size);
		}
		public static string PadL(string text, int size, char fillChar)
		{
			string trimText = text.Trim();
			int len = trimText.Length;

			if (size > len)
			{
				int cantRep = ((size - len) / 1) + 1;
				string head = Left(Replicate(fillChar, cantRep), size - len);
				return (Left(head + trimText, size));
			}
			return (Left(trimText, size));

		}
		public static string PadL(string text, int size, string fill)
		{
			string trimText = text.Trim();
			int len = trimText.Length;

			if (size > len)
			{
				int cantRep = ((size - len) / fill.Length) + 1;
				string head = Left(Replicate(fill, cantRep), size - len);
				return (Left(head + trimText, size));
			}
			return (Left(trimText, size));
		}

		public static string Replicate(char paddingChar, int size)
		{
			string ret = "";
			if (size <= 0)
				return ret;
			else
				return ret.PadRight(size, paddingChar);
		}
		public static string Replicate(string str, int size)
		{
			if (size <= 0)
				return "";
			StringBuilder ret = new StringBuilder();

			for (int i = 0; i < size; i++)
			{
				ret.Append(str);
			}

			return ret.ToString();
		}

		static public int Len(string s)
		{
			if (s == null)
				return 0;
			return RTrim(s).Length;
		}
		static public string Trim(string s)
		{
			if (!string.IsNullOrEmpty(s))
				return s.Trim(' ');
			else
				return s;
		}
		static public string RTrim(string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				int len = s.Length;
				if (len > 0 && s[len - 1] == ' ')
					return s.TrimEnd(' ');
				else
					return s;
			}
			else
				return string.Empty;
		}
		static public string LTrim(string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				int len = s.Length;
				if (len > 0 && s[0] == ' ')
					return s.TrimStart(' ');
				else
					return s;
			}
			else
				return s;
		}

		static public string Upper(string s)
		{
			if (!string.IsNullOrEmpty(s))
				return s.ToUpper();
			else
				return s;
		}
		static public string Lower(string s)
		{
			if (!string.IsNullOrEmpty(s))
				return s.ToLower();
			else
				return s;
		}
		static public string NewLine()
		{
			return "\r\n";
		}
		/*
         FormatExpression is a string expression (character or varchar) having zero or more parameter markers 
         (up to 9, from 1 to 9). For example: Format( "%1's age is %2 years old.", "John", "13"). In the example, 
         parameter markers are %1 and %2. They state where, in the resulting string, must be embeded the values 
         of "John" and "13". The result must be "John's age is 13 years old". 
         If a '%' sign must be included in FormatExpression? it must be preceded by the '\' (backslash) sign. For example: 
         "This is not a parameter marker: \%1". 
        */
		static public string Format(string value, string v1, string v2, string v3, string v4, string v5, string v6, string v7, string v8, string v9)
		{
			string indexs = "0123456789";
			string[] vs = { v1, v2, v3, v4, v5, v6, v7, v8, v9 };
			StringBuilder stringBuilder = new StringBuilder();
			bool skip = false;
			if (!String.IsNullOrEmpty(value))
			{
				for (int i = 0; i < value.Length; i++)
				{

					if (value[i] == '\\')
					{
						if ((i + 1 < value.Length) && value[i + 1] != '%')
							stringBuilder.Append(value[i]);

						skip = true;
					}
					else if (value[i] == '%' && (i + 1 < value.Length) && indexs.IndexOf(value[i + 1]) != -1 && !skip)
					{
						stringBuilder.Append(vs[Convert.ToInt32(value[i + 1].ToString()) - 1]);
						i++;
						skip = false;
					}
					else
					{
						stringBuilder.Append(value[i]);
						skip = false;
					}
				}
				String res = stringBuilder.ToString();
				if (res == value)
				{
					//Parameters format {0}, {1}, ...
					res = String.Format(value, new object[] { v1, v2, v3, v4, v5, v6, v7, v8, v9 });
				}
				return res;
			}
			else
			{
				return value;
			}
		}
		public string Format(string value, string picture)
		{
			if (value == null)
				return "";

			if (picture.StartsWith("@") && picture.IndexOf('!') > 0)
				picture = string.Concat(Enumerable.Repeat("!", value.Length));

			GXStringPicture gx = new GXStringPicture(picture, picture.Length);

			return gx.FormatValid(value);
		}
		public string Format(int value, string Format)
		{
			return FormatNumber(value.ToString(numFmtInfo), Format);
		}
		public string Format(long value, string Format)
		{
			return FormatNumber(value.ToString(numFmtInfo), Format);
		}
		public string Format(float value, string Format)
		{
			int decimals = Format.Length - Format.Trim().IndexOf('.') - 1;
			int oldDecimals = numFmtInfo.NumberDecimalDigits;
			AssignNumberFormat(Math.Max(decimals, numFmtInfo.NumberDecimalDigits));
			string s = value.ToString("F", numFmtInfo);
			AssignNumberFormat(oldDecimals);
			return FormatNumber(s, Format);
		}
		public string Format(double value, string Format)
		{
			int decimals = Format.Length - Format.Trim().IndexOf('.') - 1;
			int oldDecimals = numFmtInfo.NumberDecimalDigits;
			AssignNumberFormat(Math.Max(decimals, numFmtInfo.NumberDecimalDigits));
			string s = value.ToString("F", numFmtInfo);
			AssignNumberFormat(oldDecimals);
			return FormatNumber(s, Format);
		}
		public string Format(decimal value, string gxpicture)
		{
			string formatt = gxpicture.Trim();
			// The number of digits is the maximum between the length of the picture and the number of digits. The number of digits is unknown here so 
			// the length of the picture is taken into account.
			// If the picture does not have group separators they are not added. 
			bool thousandSep = formatt.IndexOf(',') >= 0;

			FORMAT_SECTION section = FORMAT_SECTION.POSITIVE_VALUES;
			if (value < 0)
			{
				section = FORMAT_SECTION.NEGATIVE_VALUES;
			}
			else if (value == 0)
			{
				section = FORMAT_SECTION.ZEROS;
			}
			bool separatorsAsLiterals = useLiteralSeparators(gxpicture);

			string picture = GxPictureToNetPicture(gxpicture, separatorsAsLiterals, section);
			//It must consider format because it can have other types of characters that are not Z or 9 or. neither ,.
			string res;
			if (!string.IsNullOrEmpty(picture))
			{
				res = string.Format(CultureInfo.InvariantCulture.NumberFormat, picture, value);
			}
			else
			{
				res = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
			}
			if (separatorsAsLiterals)
			{
				return res;
			}
			else if (!thousandSep)
			{
				return ReplaceSeparators(res, numFmtInfo.NumberDecimalSeparator, string.Empty);
			}
			else
			{
				return ReplaceSeparators(res, numFmtInfo.NumberDecimalSeparator, numFmtInfo.NumberGroupSeparator);
			}

		}
		string FormatNumber(string s, string p)
		{
			return FormatNumber(s, p, numFmtInfo.NumberDecimalSeparator, numFmtInfo.NumberGroupSeparator);
		}
		string FormatNumber(string s, string p, string decSep, string thousandsSep)
		{
			int sStart, pStart, pDec, i, j, k;
			bool leftZ = false;
			bool rightZ = false;

			char groupSeparator;
			if (thousandsSep.Length > 0)
				groupSeparator = thousandsSep[0];
			else
				groupSeparator = '\0';
			s = s.Trim();
			s = ExtractNumber(s, numFmtInfo.NumberDecimalSeparator);
			p = p.Trim();
			if ((sStart = s.IndexOf('.')) == -1)        // There are no decimals in the number
				sStart = s.Length;
			pDec = 0;
			if ((pStart = p.IndexOf('.')) == -1)        // There are no decimals in the picture
				pStart = p.Length;
			else
				pDec = p.Length - pStart;               // decimal count (including point)
			StringBuilder result = new StringBuilder(new string(' ', Math.Max(p.Length, s.Length)));
			// Process the left of the decimal point
			j = sStart - 1;
			k = pStart - 1;
			for (i = k; i >= 0; i--)
			{
				switch (p[i])
				{
					case '9':
						if (j < 0)
							result[k--] = '0';
						else if (s[j] == ' ')
							result[k--] = '0';
						else
							result[k--] = s[j];
						j--;
						break;
					case 'Z':
						if (j < 0)
							result[k--] = ' ';
						else if (leftZ || leftZero(s, j))
						{
							result[k--] = ' ';
							leftZ = true;
						}
						else
							result[k--] = s[j];
						j--;
						break;
					case ',':
						if (j < 0)
						{
							if (i > 0)
								if (p[i - 1] == '9')
									if (groupSeparator != '\0')
										result[k--] = groupSeparator;
						}
						else if ((j == 0 && s[j] != '-') || j > 0)
							if (groupSeparator != '\0')
								result[k--] = groupSeparator;
						break;
					default:
						if (!leftZ)
							result[k--] = p[i];
						break;
				}
			}
			//Process the rigth of the decimal point
			if (pDec > 0)
			{
				char decimalSeparator;
				if (decSep.Length > 0)
					decimalSeparator = decSep[0];
				else
					decimalSeparator = '\0';
				j = sStart;
				for (i = pStart; i < p.Length; i++)
				{
					switch (p[i])
					{
						case '9':
							if (j < s.Length)
								result[i] = s[j];
							else
								result[i] = '0';
							j++;
							break;
						case 'Z':
							if (rightZ || rightZero(s, j))
							{
								result[i] = ' ';
								rightZ = true;
							}
							else if (j < s.Length)
								result[i] = s[j];
							j++;
							break;
						case '.':
							if (decimalSeparator != '\0')
								result[i] = decimalSeparator;
							j++;
							break;
						default:
							result[i] = p[i];
							break;
					}
				}
			}
			string sResult = result.ToString().Trim();

			bool endWithSep = false;
			if (sResult.Length >= 1)//Optimization: do Endwith only when necessary
				endWithSep = decSep.Length == 1 ? sResult[sResult.Length - 1] == decSep[0] : sResult.EndsWith(decSep);
			if (endWithSep)
				sResult = sResult.Substring(0, sResult.Length - 1);
			// Delete decimal point at the end of the string
			return sResult;
		}
		static bool leftZero(string s, int len)
		{
			if (s.IndexOfAny(numbersAndSep, 0, len + 1) == -1)
				return true;
			return false;
		}
		static bool rightZero(string s, int pos)
		{
			if (s.Length > pos && s.IndexOfAny(numbers, pos, s.Length - pos) == -1)
				return true;
			return false;
		}

		static public string ExtractNumber(string n, string dSep)
		{
			// Parse a string containing a numbers. Ignore invalid characters
			// The result is normalized so that the decimal point is always '.'

			string gSep = ",";
			if (dSep == ",") gSep = ".";

			//If the number has more than one '.' then it is not a decimal separator (it is part of the picture, eg C.I.) so they are removed.
			if (n.IndexOf('.') != n.LastIndexOf('.'))
				n = n.Replace(".", string.Empty);

			//Retorn an invariant number: with '.' as decimal separator and no group separator
			bool replaceGSep = gSep.Length > 0;
			bool replaceDSep = dSep.Length > 0 && dSep[0] != '.';
			if (n != null && n.Length > 0 && (replaceDSep || replaceGSep))
			{
				StringBuilder res = new StringBuilder();
				bool first = true;
				foreach (char c in n)
				{
					if (c == dSep[0])
					{
						if (replaceDSep)
							res.Append('.');
						else
							res.Append(c);
						first = false;
					}
					else if (char.IsDigit(c))
					{
						res.Append(c);
						first = false;
					}
					else if ((c == '-') && first)
					{
						res.Append(c);
						first = false;
					}
					else if (c == gSep[0] && !replaceGSep)
					{
						res.Append(c);
						first = false;
					}
				}
				return res.ToString();
			}
			return n;
		}
		static public string FormatLong(long value)
		{
			return value.ToString();
		}
		static public string FormatBool(bool value)
		{
			return (value ? "1" : "0");
		}
		static public bool ToBoolean(string value)
		{
			return (value != null && value.Length > 0 && value[0] == '1');
		}
		public static string Space(int spaces)
		{
			return new string(' ', spaces);
		}
		public static string Right(string text, int size)
		{
			int length = text.Length;
			int leftMargin = length - size;

			return text.Substring(leftMargin < 0 ? 0 : leftMargin, leftMargin < 0 ? length : size);
		}

		public static string Left(string text, int size)
		{
			int length = text.Length;
			if (length == size)
			{
				return text;
			}
			else
			{
				int size1 = size < 0 ? 0 : size;
				return text.Substring(0, size < length ? size1 : length);
			}
		}

		static public bool Like(string str, string ptrn)
		{
			return Like(str, ptrn, ' ');
		}
		static public bool Like(string str, string ptrn, char escape)
		{
			bool found;

			int wildLen,
				asterPos,
				wildPtr,
				backplaces,
				srchPtr,
				srchLen,
				scapeCount;

			char wildChr,
				srchChr;

			found = true;
			srchLen = str.Length - 1;
			wildLen = ptrn.Length - 1;
			asterPos = -1;
			wildPtr = 0;
			srchPtr = 0;
			scapeCount = 0;

			wildChr = ' ';
			srchChr = ' ';

			bool useEscape = escape != ' ';
			bool isEscape = false;
			bool applyEscape = false;

			while (wildPtr <= wildLen)
			{
				wildChr = ptrn[wildPtr];
				if (useEscape && !isEscape)
					isEscape = wildChr == escape;
				else
					isEscape = false;

				if (!isEscape)
				{
					if (srchPtr <= srchLen)
						srchChr = str[srchPtr - scapeCount];
					else
						srchChr = ' ';
				}

				if (isEscape)
				{
					applyEscape = true;
					wildPtr++;
					srchPtr++;
					scapeCount++;
				}
				else if (srchChr == wildChr || ((!applyEscape && wildChr == QMARK) && srchPtr <= srchLen))
				{
					found = true;
					srchPtr++;
					if (wildPtr != wildLen || srchPtr > srchLen)
						wildPtr++;
				}
				else if (!applyEscape && wildChr == ASTER)
				{
					found = true;
					asterPos = wildPtr;
					wildPtr++;
				}
				else
				{
					found = false;
					if (asterPos == -1 || srchPtr > srchLen)
						break;
					else if (asterPos == wildPtr - 1)
						srchPtr++;
					else
					{
						backplaces = wildPtr - (asterPos + 1);
						wildPtr = asterPos + 1;
						srchPtr = srchPtr - backplaces + 1;
					}
				}
				if (!isEscape)
				{
					applyEscape = false;
				}
			}
			return (found && (srchPtr > srchLen || (!applyEscape && wildChr == ASTER)));
		}

		public static string Chr(int asciiValue)
		{
			char retVal = (char)asciiValue;
			return retVal.ToString();
		}

		public static int Asc(string value)
		{
			if (value.Length == 0)
				return 32;
			return Convert.ToInt32(value[0]);
		}
		public static bool StartsWith(string s1, string s2)
		{
			if (s1 == null || s2 == null)
				return false;
			else
				return s1.StartsWith(s2, StringComparison.Ordinal);
		}
		public static bool EndsWith(string s1, string s2)
		{
			if (s1 == null || s2 == null)
				return false;
			else
				return s1.EndsWith(s2, StringComparison.Ordinal);
		}
		public static bool Contains(string s1, string s2)
		{
			if (s1 == null || s2 == null)
				return false;
			else
				return s1.Contains(s2);
		}
		public static string CharAt(string s2, int idx)
		{
			if (string.IsNullOrEmpty(s2) || s2.Length < idx || idx <= 0)
				return string.Empty;
			else
				return s2[idx - 1].ToString();
		}
		public static int StringSearch(string s1, string s2, int start)
		{
			if (s1 == null)
				s1 = string.Empty;
			if (start > s1.Length)
				return 0;
			if (start <= 0)
				start = 1;
			if (s2 == null)
				s2 = string.Empty;
			return s1.IndexOf(s2, start - 1) + 1;
		}
		public static int StringSearchRev(string s1, string s2, int start)
		{
			if (start == 0)
				return 0;
			if (s1 == null)
				s1 = string.Empty;
			if (start < 0 || start > s1.Length)
				start = s1.Length;
			if (s2 == null)
				s2 = string.Empty;
			return s1.LastIndexOf(s2, start - 1) + 1;
		}

		public static string StringReplace(string s, string substring, string replacement)
		{
			if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(substring))
				return s;
			else
				return s.Replace(substring, replacement);
		}
		public static int ByteCount(string s, string encoding)
		{
			Encoding enc;
			if (encoding.Trim().Length == 0)
#if NETCORE
                enc = Encoding.UTF8;
#else
				enc = Encoding.Default;
#endif
			else
				enc = GXUtil.GxIanaToNetEncoding(encoding, false);
			byte[] bts = enc.GetBytes(s);
			return bts.Length;
		}

		public static string JSONEncode(string s)
		{
			if (string.IsNullOrEmpty(s))
				return string.Empty;

			int length = s.Length;
			StringBuilder sb = new StringBuilder(length + 2);

			for (int index = 0; index < length; index++)
			{
				char ch = s[index];

				if ((ch == '\\') || (ch == '"') || (ch == '>'))
				{
					sb.Append('\\');
					sb.Append(ch);
				}
				else if (ch == '\b')
					sb.Append("\\b");
				else if (ch == '\t')
					sb.Append("\\t");
				else if (ch == '\n')
					sb.Append("\\n");
				else if (ch == '\f')
					sb.Append("\\f");
				else if (ch == '\r')
					sb.Append("\\r");
				else
				{
					if (ch < ' ')
					{
						sb.Append("\\u");
						sb.Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
					}
					else
					{
						sb.Append(ch);
					}
				}
			}

			return sb.ToString();
		}

		public static string EncodeString(string s)
		{
			UnicodeEncoding enc = new UnicodeEncoding();
			byte[] b = enc.GetBytes(GXUtil.ValueEncodeFull(s));
			return Convert.ToBase64String(b);
		}
		public static string DecodeString(string s)
		{
			UnicodeEncoding enc = new UnicodeEncoding();
			byte[] b = Convert.FromBase64String(s);
#if !NETCORE
			return GXUtil.ValueDecodeFull(enc.GetString(b));
#else
            return enc.GetString(b, 0, b.Length);
#endif
		}
		public static string ToBase64(string data)
		{
			try
			{
				byte[] encData_byte = new byte[data.Length];
				encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
				string encodedData = Convert.ToBase64String(encData_byte);
				return encodedData;
			}
			catch (Exception e)
			{
				throw new Exception("Error in ToBase64" + e.Message);
			}
		}
		public static string FromBase64(string data)
		{
			try
			{
				System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
				System.Text.Decoder utf8Decode = encoder.GetDecoder();

				byte[] todecode_byte = Convert.FromBase64String(data);
				int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
				char[] decoded_char = new char[charCount];
				utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
				string result = new String(decoded_char);
				return result;
			}
			catch (Exception e)
			{
				throw new Exception("Error in FromBase64" + e.Message);
			}
		}
		public static bool NotNumeric(string num)
		{
			num = num.Trim();
			foreach (char c in num)
				if (!Char.IsNumber(c))
					return true;
			return false;
		}

	}
	class CalendarUtilities
	{
		private Calendar newCal;
		private bool isGregorian;

		public static void ChangeCalendar(CultureInfo ci, Calendar cal)
		{
			CalendarUtilities util = new CalendarUtilities(cal);

			// Is the new calendar supported?
			if (Array.Exists(ci.OptionalCalendars, util.CalendarExists))
				ci.DateTimeFormat.Calendar = cal;
		}
		public static bool IsGregorian(Calendar cal)
		{
			return cal != null && cal.GetType().Name.Contains("Gregorian"); ;
		}
		private CalendarUtilities(Calendar cal)
		{
			newCal = cal;
			isGregorian = IsGregorian(cal);
		}

		private bool CalendarExists(Calendar cal)
		{
			if (cal.ToString() == newCal.ToString())
			{
				if (isGregorian)
				{
					if (((GregorianCalendar)cal).CalendarType ==
					   ((GregorianCalendar)newCal).CalendarType)
						return true;
				}
				else
				{
					return true;
				}
			}
			return false;
		}
	}
	public class LocalUtil
	{

		DateTimeUtil dtu;
		StringUtil stu;
		NumberUtil nu;
		string defaultDatePicture;
		CultureInfo cultureInfo;

		public DateTimeUtil DTUtil { get { return dtu; } }

		public LocalUtil(String language)
		{
			cultureInfo = Config.GetCultureForLang(language);

			string defaultTimePicture, defaultYearLimit, defaultAmPm, decimalPoint, groupSep;
			AMPMFmt timeAmPmFormat = AMPMFmt.T12;
			if (Config.GetValueOf("YearLimit", out defaultYearLimit))
				cultureInfo.DateTimeFormat.Calendar.TwoDigitYearMax = Convert.ToInt32(defaultYearLimit) + 100 - 1;  //Receive lower limit
			else
				cultureInfo.DateTimeFormat.Calendar.TwoDigitYearMax = 2039;    // Default del yearlimit

			cultureInfo.DateTimeFormat.AMDesignator = "AM";     // Force AM and PM to format 
			cultureInfo.DateTimeFormat.PMDesignator = "PM";     // datetimes

			cultureInfo.DateTimeFormat.TimeSeparator = ":";     // force time separator to all languages
			cultureInfo.DateTimeFormat.DateSeparator = "/";     // force date separator to all languages

			if (Config.GetValueOf("DatePattern", out defaultDatePicture))
			{
				DateTimeUtil.useConfigForDates = true;
				cultureInfo.DateTimeFormat.ShortDatePattern = defaultDatePicture;
			}
			else
			{
				defaultDatePicture = Config.GetLanguageProperty(language, "date_fmt");
				if (defaultDatePicture == null)
					Config.GetValueOf("DateFormat", out defaultDatePicture);
				if (!String.IsNullOrEmpty(defaultDatePicture))
					cultureInfo.DateTimeFormat.ShortDatePattern = DateTimeUtil.DateFormatFromPicture(DateTimeUtil.PictureFormatFromString(defaultDatePicture))[0];
			}
			if (Config.GetValueOf("TimePattern", out defaultTimePicture))
			{
				DateTimeUtil.useConfigForTimes = true;
				cultureInfo.DateTimeFormat.ShortTimePattern = defaultTimePicture;
			}
			else
			{
				defaultAmPm = Config.GetLanguageProperty(language, "time_fmt");
				if (defaultAmPm == null)
					Config.GetValueOf("TimeAmPmFormat", out defaultAmPm);
				if (!String.IsNullOrEmpty(defaultAmPm))
					timeAmPmFormat = (AMPMFmt)(defaultAmPm == "24" ? AMPMFmt.T24 : AMPMFmt.T12);
			}

			decimalPoint = Config.GetLanguageProperty(language, "decimal_point");
			if (decimalPoint == null)
				Config.GetValueOf("DECIMAL_POINT", out decimalPoint);
			if (decimalPoint == ",")
				groupSep = ".";
			else
				groupSep = ",";

			cultureInfo.NumberFormat.NumberDecimalSeparator = decimalPoint;
			cultureInfo.NumberFormat.NumberGroupSeparator = groupSep;

			dtu = new DateTimeUtil(cultureInfo, timeAmPmFormat);
			stu = new StringUtil(cultureInfo.NumberFormat);
			nu = new NumberUtil(cultureInfo.NumberFormat);
		}
		public CultureInfo CultureInfo
		{
			get { return cultureInfo; }
		}

		public decimal CToND(string val)
		{
			return nu.CToND(val);
		}
		public decimal CToN(String val, string decSep, string thousandsSep)
		{
			return nu.CToN(val, decSep, thousandsSep);
		}
		public string Format(string value, string Format)
		{
			return stu.Format(value, Format);
		}

		public string Format(int value, string Format)
		{
			return stu.Format(value, Format);
		}
		public string Format(long value, string Format)
		{
			return stu.Format(value, Format);
		}
		public string Format(float value, string Format)
		{
			return stu.Format(value, Format);
		}
		public string Format(double value, string Format)
		{
			return stu.Format(value, Format);
		}
		public string Format(decimal value, string gxpicture)
		{
			return stu.Format(value, gxpicture);
		}
		public string Format(DateTime dt, string format)
		{

			int dateLength = 0;
			int timeLength = 0;
			bool computeTime;
			if (format.IndexOf("/") == -1)
				computeTime = true;
			else
				computeTime = false;
			ExtractPicturePattern(format, ref dateLength, ref timeLength, ref computeTime);
			return TToC(dt, dateLength, timeLength, -1, -1);
		}

		public static void ExtractPicturePattern(string format, ref int dateLength, ref int timeLength, ref bool computeTime)
		{
			for (int i = 0; i < format.Length; i++)
			{
				switch (format[i])
				{
					case '9':
						if (!computeTime)
							dateLength++;
						else
							timeLength++;
						break;
					case ':':
					case '.':
						if (computeTime)
							timeLength++;
						break;
					case '/':
						if (!computeTime)
							dateLength++;
						break;
					case ' ':
						computeTime = true;
						break;
				}
			}
		}
		public int GetYear(int y)
		{
			return dtu.GetYear(y);
		}
		public DateTime YMDToD(int year, int month, int day)
		{
			return dtu.YMDToD(year, month, day);
		}
		public DateTime YMDHMSToT(int year, int month, int day, int hour, int min, int sec)
		{
			return dtu.YMDHMSToT(year, month, day, hour, min, sec);
		}
		public DateTime YMDHMSMToT(int year, int month, int day, int hour, int min, int sec, int mil)
		{
			return dtu.YMDHMSMToT(year, month, day, hour, min, sec, mil);
		}
		public string DToC(DateTime dt, int picFmt)
		{
			return dtu.DToC(dt, picFmt);
		}
		public string DToC(DateTime dt, int picFmt, string separator)
		{
			return dtu.DToC(dt, picFmt, separator);
		}
		public string TToC(DateTime dt, int lenDate, int lenTime, int ampmFmt, int dateFmt)
		{
			return dtu.TToC(dt, lenDate, lenTime, ampmFmt, dateFmt);
		}

		public string TToC(DateTime dt, int lenDate, int lenTime, int ampmFmt, int dateFmt,
			String dSep, String tSep, String dtSep)
		{
			return dtu.TToC(dt, lenDate, lenTime, ampmFmt, dateFmt,
			dSep, tSep, dtSep);
		}
		public string Time()
		{
			return dtu.Time();
		}
		public DateTime CToT(string strDate, int picFmt, int ampmFmt)
		{
			return dtu.CToT(strDate, picFmt, ampmFmt);
		}
		public DateTime CToT(string strDate)
		{
			return CToT(strDate, DateTimeUtil.PictureFormatFromString(defaultDatePicture));
		}
		public DateTime CToT(string strDate, int picFmt)
		{
			return dtu.CToT(strDate, picFmt);
		}
		public DateTime CToD(string strDate, int picFmt)
		{
			return dtu.CToD(strDate, picFmt);
		}
		public DateTime CToD(string strDate)
		{
			return dtu.CToD(strDate);
		}
		public int VCDate(string date, int picFmt)
		{
			return dtu.VCDate(date, picFmt);
		}
		public int VCDateTime(string date, int picFmt, int ampmFmt)
		{
			return dtu.VCDateTime(date, picFmt, ampmFmt);
		}
		public DateTime ParseDateParm(string valueString)
		{
			return dtu.ParseDateParm(valueString);
		}
		public DateTime ParseDTimeParm(string valueString)
		{
			return dtu.ParseDTimeParm(valueString);
		}
		public DateTime ParseDateOrDTimeParm(string valueString)
		{
			return dtu.ParseDateOrDTimeParm(valueString);
		}
	}
	public enum AMPMFmt
	{
		T24,
		T12
	}
	static class StringExtension
	{
		public static string ToCamelCase(this string str)
		{
			if (!string.IsNullOrEmpty(str) && str.Length > 1 && !Char.IsUpper(str[0]))
			{
				return Char.ToUpperInvariant(str[0]) + str.Substring(1);
			}
			return str;
		}
	}
	public class DateTimeUtil
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.DateTimeUtil));
		private CultureInfo cultureInfo;
		public static bool useConfigForDates;
		public static bool useConfigForTimes;
		AMPMFmt timeAmPmFormat = AMPMFmt.T12;
		const long timeConversionFactor = 10000000L;
		readonly static DateTime datetTime1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		internal static DateTime nullDate = ResetTime(DateTime.MinValue);

		enum DatePictureFmt
		{
			ANSI,
			M2D2Y2,
			D2M2Y2,
			M2D2Y4,
			D2M2Y4,
			Y2M2D2,
			Y4M2D2
		}
		enum DateFmt
		{
			ANSI,
			YMD,
			MDY,
			DMY
		}

		public DateTimeUtil(CultureInfo culture, AMPMFmt amPmFormat)
		{
			cultureInfo = culture;
			timeAmPmFormat = amPmFormat;
		}

		static string DateFormatFromPicture0(int picFmt, string separator)
		{
			string dtFmt;
			dtFmt = DateFormatFromPicture(picFmt)[0];
			if (!useConfigForDates)
			{
				if (separator.Length == 1 && separator[0] == '-')
				{
					dtFmt = dtFmt.Replace('/', '-');
				}
				else if (separator.Length == 0)
				{
					char[] dtFmtArr = dtFmt.ToCharArray();
					StringBuilder dtFmtNew = new StringBuilder();
					foreach (char c in dtFmtArr)
					{
						if (c != '/')
							dtFmtNew.Append(c);
					}
					dtFmt = dtFmtNew.ToString();
				}
			}
			return dtFmt;
		}
		public static string[] DateFormatFromPicture(int picFmt)
		{
			string[] dtFmt = new string[5];
			// Returns all possible pictures for a DateTime given a format   
			dtFmt[4] = "";  // One supported possibility is that it has no date
			switch ((DatePictureFmt)picFmt)
			{
				case DatePictureFmt.ANSI:
					dtFmt[0] = "yyyy/MM/dd";
					dtFmt[1] = "yy/MM/dd";
					dtFmt[2] = "yy/M/d";
					dtFmt[3] = "yyyy/M/d";
					break;
				case DatePictureFmt.M2D2Y2:
					dtFmt[0] = "MM/dd/yy";
					dtFmt[1] = "MM/dd/yyyy";
					dtFmt[2] = "M/d/yy";
					dtFmt[3] = "M/d/yyyy";
					break;
				case DatePictureFmt.D2M2Y2:
					dtFmt[0] = "dd/MM/yy";
					dtFmt[1] = "dd/MM/yyyy";
					dtFmt[2] = "d/M/yy";
					dtFmt[3] = "d/M/yyyy";
					break;
				case DatePictureFmt.M2D2Y4:
					dtFmt[0] = "MM/dd/yyyy";
					dtFmt[1] = "MM/dd/yy";
					dtFmt[2] = "M/d/yy";
					dtFmt[3] = "M/d/yyyy";
					break;
				case DatePictureFmt.D2M2Y4:
					dtFmt[0] = "dd/MM/yyyy";
					dtFmt[1] = "dd/MM/yy";
					dtFmt[2] = "d/M/yy";
					dtFmt[3] = "d/M/yyyy";
					break;
				case DatePictureFmt.Y2M2D2:
					dtFmt[0] = "yy/MM/dd";
					dtFmt[1] = "yyyy/MM/dd";
					dtFmt[2] = "yy/M/d";
					dtFmt[3] = "yyyy/M/d";
					break;
				case DatePictureFmt.Y4M2D2:
					dtFmt[0] = "yyyy/MM/dd";
					dtFmt[1] = "yy/MM/dd";
					dtFmt[2] = "yy/M/d";
					dtFmt[3] = "yyyy/M/d";
					break;
				default:
					dtFmt = new string[3];
					dtFmt[0] = CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern;
					dtFmt[1] = CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern;
					dtFmt[2] = "";
					break;
			}
			return dtFmt;
		}
		public string DateFormatFromSize(int dateSize, int dateFmt, string separator)
		{
			string dtFmt;
			if (useConfigForDates)
			{
				return cultureInfo.DateTimeFormat.ShortDatePattern;
			}
			else
			{
				if (dateSize == 0)
					return "";

				switch ((DateFmt)dateFmt)
				{
					case DateFmt.ANSI:
						dtFmt = "yyyy/MM/dd";
						break;
					case DateFmt.YMD:
						dtFmt = "yy/MM/dd";
						break;
					case DateFmt.MDY:
						dtFmt = "MM/dd/yy";
						break;
					case DateFmt.DMY:
						dtFmt = "dd/MM/yy";
						break;
					default:
						dtFmt = cultureInfo.DateTimeFormat.ShortDatePattern;
						break;
				}
				if (dtFmt.IndexOf("yyyy") != -1)
				{
					if (dateSize == 8)
						dtFmt = dtFmt.Replace("yyyy", "yy");
				}
				else
				{
					if (dateSize == 10)
						dtFmt = dtFmt.Replace("yy", "yyyy");
				}
				if (separator.Length == 1 && separator[0] == '-')
				{
					dtFmt = dtFmt.Replace('/', '-');
				}
				else if (separator.Length == 0)
				{
					char[] dtFmtArr = dtFmt.ToCharArray();
					StringBuilder dtFmtNew = new StringBuilder();
					foreach (char c in dtFmtArr)
					{
						if (c != '/')
							dtFmtNew.Append(c);
					}
					dtFmt = dtFmtNew.ToString();
				}
			}
			return dtFmt;
		}
		public static int MapTimeFormat(String dateFmt)
		{
			if (dateFmt == "12") return 1;
			else return 0;
		}
		public static int MapDateTimeFormat(String dateFmt)
		{   //Same mapping used in default_fn_parm_val( $ttoc($, 5, [t(X,3)])\prolog.
			if (dateFmt.Equals("ANSI"))
				return (int)DateFmt.ANSI;
			else if (dateFmt.Equals("MDY") || dateFmt.Equals("MDY4"))
				return (int)DateFmt.MDY;
			else if (dateFmt.Equals("DMY") || dateFmt.Equals("DMY4"))
				return (int)DateFmt.DMY;
			else if (dateFmt.Equals("YMD") || dateFmt.Equals("Y4MD"))
				return (int)DateFmt.YMD;
			else return (int)DateFmt.MDY;
		}
		public static int MapDateFormat(String dateFmt)
		{   //Same mapping used in map_date_format_l/prolog.
			int dtFmt;
			switch (dateFmt.ToUpper())
			{
				case "ANSI":
					dtFmt = (int)DatePictureFmt.ANSI;
					break;
				case "MDY":
					dtFmt = (int)DatePictureFmt.M2D2Y2;
					break;
				case "DMY":
					dtFmt = (int)DatePictureFmt.D2M2Y2;
					break;
				case "MDY4":
					dtFmt = (int)DatePictureFmt.M2D2Y4;
					break;
				case "DMY4":
					dtFmt = (int)DatePictureFmt.D2M2Y4;
					break;
				case "YMD":
					dtFmt = (int)DatePictureFmt.Y2M2D2;
					break;
				case "Y4MD":
					dtFmt = (int)DatePictureFmt.Y4M2D2;
					break;
				default:
					dtFmt = (int)DatePictureFmt.D2M2Y4;
					break;
			}
			return dtFmt;
		}
		public static int PictureFormatFromString(string picFmt)
		{
			int dtFmt;
			switch (picFmt.ToUpper())
			{
				case "ANSI":
					dtFmt = (int)DatePictureFmt.ANSI;
					break;
				case "YMD":
					dtFmt = (int)DatePictureFmt.Y4M2D2;
					break;
				case "MDY":
					dtFmt = (int)DatePictureFmt.M2D2Y4;
					break;
				case "DMY":
					dtFmt = (int)DatePictureFmt.D2M2Y4;
					break;
				default:
					dtFmt = (int)DatePictureFmt.D2M2Y4;
					break;
			}
			return dtFmt;
		}
		public string TimeFormatFromSize(int timeSize, int ampmFmt, string separator)
		{
			return TimeFormatFromSize(timeSize, ampmFmt, separator, true, false);
		}
		string TimeFormatFromSize(int timeSize, int ampmFmt, string separator, bool ampmseparator, bool allowsOneDigitTime)
		{
			string tFormat;
			if (useConfigForTimes)
			{
				return cultureInfo.DateTimeFormat.ShortTimePattern;
			}
			string hh = allowsOneDigitTime ? "h" : "hh";
			string mm = allowsOneDigitTime ? "m" : "mm";
			string ss = allowsOneDigitTime ? "s" : "ss";
			string ms = "fff";

			if (timeSize == 0)
				tFormat = "";
			else if (timeSize == 2)
				tFormat = AMPMFormatString(hh, ampmFmt, ampmseparator);
			else if (timeSize == 5)
				tFormat = AMPMFormatString(hh + separator + mm, ampmFmt, ampmseparator);
			else if (timeSize == 12)
				tFormat = AMPMFormatString(hh + separator + mm + separator + ss + "." + ms, ampmFmt, ampmseparator);
			else
				tFormat = AMPMFormatString(hh + separator + mm + separator + ss, ampmFmt, ampmseparator);
			return tFormat;
		}
		string AMPMFormatString(string tmFmt, int format, bool ampmseparator)
		{
			if (format < 0)
				format = (int)timeAmPmFormat;
			switch ((AMPMFmt)format)
			{
				case AMPMFmt.T24:
					return tmFmt.Replace('h', 'H');
				default:
					return tmFmt.Replace('H', 'h') + (ampmseparator ? " tt" : "tt");
			}
		}
		static string FormatEmptyDate(string pic)
		{
			StringBuilder emptyDT = new StringBuilder(pic);
			emptyDT.Replace('d', ' ');
			emptyDT.Replace('M', ' ');
			emptyDT.Replace('y', ' ');

			if (Preferences.BlankEmptyDates)
			{
				emptyDT.Replace('/', ' ');
				emptyDT.Replace(':', ' ');
				emptyDT.Replace('m', ' ');
				emptyDT.Replace('s', ' ');
				emptyDT.Replace('f', ' ');
				if (emptyDT.ToString().IndexOf('t') == -1)  // 24 hours
				{
					emptyDT.Replace('h', ' ');
					emptyDT.Replace('H', ' ');
				}
				else                                // 12 hours
				{
					emptyDT.Replace("hh", "  ");
					emptyDT.Replace("HH", "  ");
					emptyDT.Replace("tt", "  ");
					emptyDT.Replace('t', ' ');
				}
			}
			else
			{
				emptyDT.Replace('m', '0');
				emptyDT.Replace('s', '0');
				emptyDT.Replace('f', '0');
				if (emptyDT.ToString().IndexOf('t') == -1)  // 24 hours
				{
					emptyDT.Replace('h', '0');
					emptyDT.Replace('H', '0');
				}
				else                                // 12 hours
				{
					emptyDT.Replace("hh", "12");
					emptyDT.Replace("HH", "12");
					emptyDT.Replace("tt", "AM");
					emptyDT.Replace('t', 'a');
				}
			}
			return emptyDT.ToString();
		}
		string TimeFormatFromSDT(String S, String TSep, bool allowsOneDigitTime)
		{
			int pos1, pos2, pos3, tSize, ampmFmt;
			tSize = 0;

			pos1 = S.IndexOf(TSep);
			pos2 = S.LastIndexOf(TSep);
			pos3 = S.LastIndexOf(".");
			if (pos1 == -1 && pos2 == -1)   // It has neither minutes nor seconds
			{
				if (S.IndexOf('/') != -1)   // It has date
				{
					if (S.Trim().IndexOf(' ') != -1)    // Blank: it has time at least
						tSize = 2;
				}
				else                        // Does not have date
				{
					String S1 = S.Trim('P', 'p', 'm', 'M', 'a', 'A', ' ');
					if (S1.Length > 0 && S1.Length <= 2)
						tSize = 2;
					else
						tSize = 0;
				}
			}
			if (pos1 == pos2 && pos1 != -1)     // Has minutes
				tSize = 5;
			if (pos1 != pos2)       // Has seconds
				tSize = 8;
			if (pos1 != pos2 && pos3 > pos2) //milliseconds after seconds
				tSize = 12;
			if (S.IndexOf('A') != -1 || S.IndexOf('P') != -1 || S.IndexOf('a') != -1 || S.IndexOf('p') != -1)
				ampmFmt = 1;
			else
				ampmFmt = 0;
			return TimeFormatFromSize(tSize, ampmFmt, ":", false, allowsOneDigitTime);
		}
		static public int Day(DateTime d)
		{
			if (d == nullDate)
				return 0;
			return d.Day;
		}
		static public int Month(DateTime d)
		{
			if (d == nullDate)
				return 0;
			return d.Month;
		}
		static public int Year(DateTime d)
		{
			if (d == nullDate)
				return 0;
			return d.Year;
		}
		static public int Hour(DateTime d)
		{
			return d.Hour;
		}
		static public int Minute(DateTime d)
		{
			return d.Minute;
		}
		static public int Second(DateTime d)
		{
			return d.Second;
		}
		static public int MilliSecond(DateTime d)
		{
			return d.Millisecond;
		}
		static public short CurrentOffset(IGxContext context)
		{
			if (context == null)
				context = GxContext.Current;
			TimeSpan ts = CurrentOffset(context.GetOlsonTimeZone());
			return (short)(ts.Hours * 60 + ts.Minutes);
		}
		static private TimeSpan CurrentOffset(OlsonTimeZone clientTimeZone)
		{
			DateTime currentDate = DateTime.Now;
			try
			{
				return (clientTimeZone == null ? CurrentTimeZoneGetUtcOffset(currentDate) : clientTimeZone.GetUtcOffset(currentDate));
			}
			catch (ArgumentOutOfRangeException)
			{
				//Avoid InSpringForwardGap/InFallBackRange condition
				return (clientTimeZone == null ? CurrentTimeZoneGetUtcOffset(currentDate) : clientTimeZone.GetUtcOffset(currentDate.AddHours(-1)));
			}
		}
		static TimeSpan CurrentTimeZoneGetUtcOffset(DateTime dt)
		{
#if NETCORE
            return TimeZoneInfo.Local.GetUtcOffset(dt);
#else
			return TimeZone.CurrentTimeZone.GetUtcOffset(dt);
#endif
		}
		static DateTime CurrentTimeZoneToLocalTime(DateTime dt)
		{
#if NETCORE
            return TimeZoneInfo.ConvertTime(dt, TimeZoneInfo.Local);
#else
			return TimeZone.CurrentTimeZone.ToLocalTime(dt);
#endif
		}
		static DateTime CurrentTimeZoneToUniversalTime(DateTime dt)
		{
#if NETCORE
            return TimeZoneInfo.ConvertTimeToUtc(dt, TimeZoneInfo.Local);
#else
			return TimeZone.CurrentTimeZone.ToUniversalTime(dt);
#endif
		}

		static public DateTime Today(IGxContext context)
		{
			if (Preferences.useTimezoneFix())
				return ResetMillisecondsTicks(ResetTime(ConvertDateTime(DateTime.Now, TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id), context.GetOlsonTimeZone())));
			return ResetMillisecondsTicks(DateTime.Today);
		}
		static public DateTime Now(IGxContext context)
		{
			return ResetMillisecondsTicks(NowTicks(context));
		}

		static public DateTime NowTicks(IGxContext context)
		{
			if (Preferences.useTimezoneFix())
				return ConvertDateTime(DateTime.Now, TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id), context.GetOlsonTimeZone());
			return DateTime.Now;
		}

		static public DateTime NowMS(IGxContext context)
		{
			return ResetMicroseconds(NowTicks(context));
		}

		public int GetYear(int y)
		{
			return cultureInfo.DateTimeFormat.Calendar.ToFourDigitYear(y);
		}
		public DateTime YMDToD(int year, int month, int day)
		{
			int year1 = GetYear(year);
			if (1 <= year1 && year1 <= 9999 && 1 <= month && 1 <= day)
			//Basic checkups are done before to avoid many calls to the constructor
			{
				try
				{
					return new DateTime(year1, month, day, cultureInfo.Calendar);
				}
				catch
				{
					return nullDate;
				}
			}
			return nullDate;
		}
		public DateTime YMDHMSToT(int year, int month, int day, int hour, int min, int sec)
		{
			return YMDHMSMToT(year, month, day, hour, min, sec, 0, true);
		}
		public DateTime YMDHMSToT(int year, int month, int day, int hour, int min, int sec, bool centuryConversion)
		{
			return YMDHMSMToT(year, month, day, hour, min, sec, 0, centuryConversion);
		}

		public DateTime YMDHMSMToT(int year, int month, int day, int hour, int min, int sec, int mil)
		{
			return YMDHMSMToT(year, month, day, hour, min, sec, mil, true);
		}

		public DateTime YMDHMSMToT(int year, int month, int day, int hour, int min, int sec, int mil, bool centuryConversion)
		{
			try
			{
				int year1 = year;
				if (centuryConversion)
					year1 = GetYear(year);
				return new DateTime(year1, month, day, hour, min, sec, mil, cultureInfo.DateTimeFormat.Calendar);
			}
			catch
			{
				return nullDate;
			}
		}

		public static DateTime DateTimeWithTimeZoneToUTC(int year, int month, int day, int hr, int mm, int ss, string timeZoneOffset)
		{
			DateTime dt;
			if (timeZoneOffset.Length == 5)
			{
				dt = new DateTime(year, month, day, hr, mm, ss, DateTimeKind.Utc);
				int sign = (timeZoneOffset[0] == '-') ? 1 : -1;
				int offsetH = sign * Int32.Parse(timeZoneOffset.Substring(1, 2));
				int offsetM = sign * Int32.Parse(timeZoneOffset.Substring(3, 2));
				dt = dt.Add(new TimeSpan(offsetH, offsetM, 0));
			}
			else
			{
				dt = new DateTime(year, month, day, hr, mm, ss, DateTimeKind.Local);
			}
			return dt.ToUniversalTime();
		}
#if !NETCORE
		[Obsolete("DToC with 2 arguments is deprecated", false)]
#endif
		public string DToC(DateTime dt, int picFmt)
		{
			if (dt == nullDate)
				return FormatEmptyDate(DateFormatFromPicture(picFmt)[0]);
			else
				return dt.ToString(DateFormatFromPicture(picFmt)[0], cultureInfo);
		}
		private static bool isNullJsonDate(string value)
		{
			return (string.IsNullOrEmpty(value) || value.Trim('0', '-', 'T', ':', ' ').Length == 0);
		}
		//Datetime to character using invariant culture
		public static string DToC2(DateTime dt)
		{
			if (dt == nullDate)
				return ("0000-00-00");
			else
				return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		}
		public static DateTime CToD2(string value)
		{
			if (isNullJsonDate(value))
				return nullDate;
			else
			{
				if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ret))
				{
					return ret;
				}
			}
			return nullDate;
		}
		public static DateTime CToT2(string value)
		{
			if (isNullJsonDate(value))
				return nullDate;
			else
			{
				DateTime ret = nullDate;
				if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out ret))
				{
					if (value.StartsWith(GxDateString.NullValue))
					{
						value = value.Substring(GxDateString.NullValue.Length);
						DateTime.TryParse(GxDateString.GregorianDate + value, CultureInfo.InvariantCulture, DateTimeStyles.None, out ret);

					}
				}
				if (Preferences.useTimezoneFix())
					ret = fromUniversalTime(ret);
				return ret;
			}
		}
		public static string TToC2(DateTime dt)
		{
			return TToC2(dt, true);
		}
		public static string TToC2(DateTime dt, bool toUTC)
		{
			return TToCRest(dt, "0000-00-00T00:00:00", JsonDateFormat, toUTC);
		}

		public static string TToC3(DateTime dt)
		{
			return TToC3(dt, true);
		}
		internal const string JsonDateFormatMillis = "yyyy-MM-ddTHH:mm:ss.fff";
		internal const string JsonDateFormat = "yyyy-MM-ddTHH:mm:ss";
		
		public static string TToC3(DateTime dt, bool toUTC)
		{
			return TToCRest(dt, "0000-00-00T00:00:00.000", JsonDateFormatMillis, toUTC);
		}

		static string TToCRest(DateTime dt, String nullString, String formatStr, bool toUTC=true)
		{
			if (dt == nullDate)
				return FormatEmptyDate(nullString);
			else
			{
				DateTime ret = Preferences.useTimezoneFix() ? (toUTC ? toUniversalTime(dt) : dt) : dt;
				return ret.ToString(formatStr, CultureInfo.InvariantCulture);
			}
		}

		public string DToC(DateTime dt, int picFmt, string separator)
		{
			string picture = DateFormatFromPicture0(picFmt, separator);
			if (dt == nullDate)
				return FormatEmptyDate(picture);
			else
				return dt.ToString(picture, cultureInfo);
		}
		public string TToC(DateTime dt, int lenDate, int lenTime, int ampmFmt, int dateFmt)
		{
			String separator = (lenTime > 0 && lenDate > 0) ? " " : string.Empty;
			string timeFmt = separator + TimeFormatFromSize(lenTime, ampmFmt, ":");
			if (dt == nullDate)
				return FormatEmptyDate(DateFormatFromSize(lenDate, dateFmt, "/") + timeFmt);
			else
			{
				return dt.ToString(DateFormatFromSize(lenDate, dateFmt, "/") + timeFmt, cultureInfo).TrimStart().TrimEnd();
			}
		}
		public string TToC(DateTime dt, int lenDate, int lenTime, int ampmFmt, int dateFmt,
			String dSep, String tSep, String dtSep)
		{
			string date = DateFormatFromSize(lenDate, dateFmt, dSep);
			string time = TimeFormatFromSize(lenTime, ampmFmt, tSep);
			string sep = (lenDate > 0 && lenTime > 0 ? dtSep : "");

			if (dt == nullDate)
				return FormatEmptyDate(date + sep + time);
			else
			{
				return dt.ToString(date + sep + time, cultureInfo).TrimStart().TrimEnd();
			}
		}

		static public byte Dow(DateTime dt)
		{
			return (byte)(dt.DayOfWeek + 1);
		}
		static public string CDow(DateTime dt, string lang)
		{
			return dt.ToString("dddd", Config.GetCultureForLang(lang)).ToCamelCase();
		}
		static public string CMonth(DateTime dt, string lang)
		{
			return dt.ToString("MMMM", Config.GetCultureForLang(lang)).ToCamelCase();
		}
		public string Time()
		{
			return DateTime.Now.ToString(TimeFormatFromSize(8, -1, ":"), cultureInfo);
		}

		static public DateTime AddMth(DateTime dt, int cantMonths)
		{
			if (dt == nullDate && cantMonths < 0)
				return nullDate;
			return dt.AddMonths(cantMonths);
		}
		static public DateTime AddYr(DateTime dt, int cantYears)
		{
			if (dt == nullDate && cantYears < 0)
				return nullDate;
			return dt.AddYears(cantYears);
        }
        static public DateTime DateEndOfMonth(DateTime dt)
        {
            int lastDay = DateTime.DaysInMonth(dt.Year, dt.Month);
			return new DateTime(dt.Year, dt.Month, lastDay, 0, 0, 0);
		}
		static public DateTime EndOfMonth(DateTime dt)
		{
			int lastDay = DateTime.DaysInMonth(dt.Year, dt.Month);
			return new DateTime(dt.Year, dt.Month, lastDay, dt.Hour, dt.Minute, dt.Second);
		}
		static public int Age(DateTime dt)
		{
			return Age(dt, DateTime.Now);
		}
		static public int Age(DateTime dtSust, DateTime dtMinu)
		{
			int yearspan;
			yearspan = (dtMinu.Year - dtSust.Year);
			if ((dtMinu.Month < dtSust.Month) ||
				((dtMinu.Month == dtSust.Month) && (dtMinu.Day < dtSust.Day)))
			{
				//  subtract one year
				yearspan--;
			}
			else
				if ((dtMinu.Month == dtSust.Month) && (dtMinu.Day == dtSust.Day))
			{
				if ((dtMinu.Hour < dtSust.Hour) ||
					((dtMinu.Hour == dtSust.Hour) && (dtMinu.Minute < dtSust.Minute)) ||
					((dtMinu.Hour == dtSust.Hour) && (dtMinu.Minute == dtSust.Minute) && (dtMinu.Second < dtSust.Second)))
				{
					//  subtract one year
					yearspan--;
				}
			}
			return yearspan;
		}
		static public double TDiffMs(DateTime dtMinu, DateTime dtSust)
		{
			return Convert.ToDouble(dtMinu.Subtract(dtSust).TotalMilliseconds / 1000.0);
		}
		static public long TDiff(DateTime dtMinu, DateTime dtSust)
		{
			return Convert.ToInt64(dtMinu.Subtract(dtSust).TotalSeconds);
		}
        static public int DDiff(DateTime dtMinu, DateTime dtSust)
        {
			return Convert.ToInt32((dtMinu - dtSust).TotalDays);
		}
		static public DateTime TAdd(DateTime dt, int seconds)
		{
			if (dt == nullDate && seconds < 0)
				return nullDate;
			return dt.AddSeconds(seconds);
		}
		static public DateTime TAddMs(DateTime dt, double seconds)
		{
			if (dt == nullDate && seconds < 0)
				return nullDate;
			if (seconds % 1 == 0)
				return dt.AddSeconds((int)seconds);
			else
				return dt.AddMilliseconds(seconds * 1000);
		}
		static public DateTime DAdd(DateTime dt, int days)
		{
			if (dt == nullDate && days < 0)
				return nullDate;
			return dt.AddDays(days);
		}
        public DateTime CToT(string strDate, int picFmt, int ampmFmt)
        {
            if (isNullDateTime(strDate, picFmt, ampmFmt))
				return nullDate;
			else
				return ParseExactDateTime(strDate, picFmt);
		}
		DateTime ParseExactDateTime(string strDate, int picFmt)
		{
			string[] fmtVec = DateFormatFromPicture(picFmt);
			string[] fmts = new string[fmtVec.Length];
			int i = 0;
			string timePicture = TimeFormatFromSDT(strDate, ":", false);

			foreach (string s in fmtVec)
				fmts[i++] = String.IsNullOrEmpty(s) ? timePicture : (s + " " + timePicture).Trim();
			DateTime dtValue = nullDate;
			bool ok = DateTime.TryParseExact(strDate.Trim(), fmts, cultureInfo.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out dtValue);
			if (!ok)
			{
				timePicture = TimeFormatFromSDT(strDate, ":", true);
				i = 0;
				foreach (string s in fmtVec)
					fmts[i++] = String.IsNullOrEmpty(s) ? timePicture : (s + " " + timePicture).Trim();

				DateTime.TryParseExact(strDate.Trim(), fmts, cultureInfo.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out dtValue);
			}
			return dtValue;
		}
		public DateTime CToT(string strDate, int picFmt)
		{
			return CToT(strDate, picFmt, 0);
		}
		public DateTime CToD(string strDate, int picFmt)
		{
			return ParseExactDate(strDate, picFmt);
		}
		public DateTime CToD(string strDate)
		{
			string defaultDatePicture = "";
			if (Config.GetValueOf("DateFormat", out defaultDatePicture))
			{
				if (strDate.IndexOf(":") != -1)
					return ParseExactDateTime(strDate, PictureFormatFromString(defaultDatePicture));
				else
					return ParseExactDate(strDate, PictureFormatFromString(defaultDatePicture));
			}
			return nullDate;
		}
		string[] modifyFormatStrings(string[] fmtVec)
		{
			if (cultureInfo.DateTimeFormat.DateSeparator != "/")
			{
				int sLen = fmtVec.Length;
				string[] s1 = new string[sLen * 2];
				fmtVec.CopyTo(s1, 0);
				int i = sLen;
				foreach (string s in fmtVec)
					s1[i++] = s.Replace("/", cultureInfo.DateTimeFormat.DateSeparator);
				return s1;
			}
			else
				return fmtVec;
		}
		DateTime ParseExactDate(string strDate, int picFmt)
		{
			string[] fmtVec = DateFormatFromPicture(picFmt);
			string[] fmts = modifyFormatStrings(fmtVec);
			string oldSeparator = cultureInfo.DateTimeFormat.DateSeparator;

			try
			{
				DateTime dValue;
				if (oldSeparator != "/") cultureInfo.DateTimeFormat.DateSeparator = "/";
				if (DateTime.TryParseExact(strDate.Trim(), fmts, cultureInfo.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault, out dValue))
					return dValue;
				else
					return nullDate;
			}
			catch
			{
				return nullDate;
			}
			finally
			{
				if (oldSeparator != "/") cultureInfo.DateTimeFormat.DateSeparator = oldSeparator;
			}
		}
		static public DateTime ServerNowMs(IGxContext context, IDataStoreProvider dataStore)
		{
			if (dataStore == null)
				return ServerNowMs(context, new DataStoreHelperBase().getDataStoreName());
			if (Preferences.useTimezoneFix())
				return ResetMicroseconds(ConvertDateTime(dataStore.serverNowMs(), TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id), context.GetOlsonTimeZone()));
			return ResetMicroseconds(dataStore.serverNowMs());
		}
		static public DateTime ServerNow(IGxContext context, IDataStoreProvider dataStore)
		{
			if (dataStore == null)
				return ServerNow(context, new DataStoreHelperBase().getDataStoreName());
			if (Preferences.useTimezoneFix())
				return ResetMillisecondsTicks(ConvertDateTime(dataStore.serverNow(), TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id), context.GetOlsonTimeZone()));
			return ResetMillisecondsTicks(dataStore.serverNow());
		}
#if !NETCORE
		[Obsolete("ServerNow with string dataSource is deprecated, use ServerNow(IGxContext context, Data.NTier.IDataStoreProvider dataStore) instead", false)]
#endif
		static public DateTime ServerNowMs(IGxContext context, string dataSource)
		{
			if (Preferences.useTimezoneFix())
				return ResetMicroseconds( ConvertDateTime(context.ServerNowMs(dataSource), TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id), context.GetOlsonTimeZone()));
			return ResetMicroseconds(context.ServerNowMs(dataSource));
		}
		static public DateTime ServerNow(IGxContext context, string dataSource)
		{
			if (Preferences.useTimezoneFix())
				return ResetMillisecondsTicks(ConvertDateTime(context.ServerNow(dataSource), TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id), context.GetOlsonTimeZone()));
			return ResetMillisecondsTicks(context.ServerNow(dataSource));
		}
		public static string ServerTime(IGxContext context, IDataStoreProvider dataStore)
		{
			if (dataStore == null)
				return ServerTime(context, new DataStoreHelperBase().getDataStoreName());
			return context.localUtil.TToC(dataStore.serverNow(), 0, 8, -1, -1);
		}

		public static DateTime ServerDate(IGxContext context, IDataStoreProvider dataStore)
		{
			if (dataStore == null)
				return ServerDate(context, new DataStoreHelperBase().getDataStoreName());
			return ResetTime(dataStore.serverNow());
		}
#if !NETCORE
		[Obsolete("ServerTime with string dataSource is deprecated, use ServerTime(IGxContext context, Data.NTier.IDataStoreProvider dataStore) instead", false)]
#endif
		public static string ServerTime(IGxContext context, string dataSource)
		{
			return context.localUtil.TToC(context.ServerNow(dataSource), 0, 8, -1, -1);
		}
		//       [Obsolete("ServerDate with string dataSource is deprecated, use ServerDate(IGxContext context, Data.NTier.IDataStoreProvider dataStore) instead", false)]
		public static DateTime ServerDate(IGxContext context, string dataSource)
		{
			return ResetTime(context.ServerNow(dataSource));
		}
		public static DateTime ResetTime(DateTime dt)
		{
			//DateTime dtRet = new DateTime(dt.Year,dt.Month,dt.Day,0,0,0,0);
			//return dtRet; 
			//This is more efficient than creating a new datetime..
			dt = dt.AddMilliseconds(dt.Millisecond * -1);
			dt = dt.AddSeconds(dt.Second * -1);
			dt = dt.AddMinutes(dt.Minute * -1);
			dt = dt.AddHours(dt.Hour * -1);
			return dt;
		}
		public static DateTime ResetDate(DateTime dt)
		{
			//DateTime dtRet = new DateTime(1,1,1,dt.Hour,dt.Minute,dt.Second);
			//return dtRet;
			//This is more efficient than creating a new datetime.
			dt = dt.AddDays((dt.Day * -1) + 1);
			dt = dt.AddMonths((dt.Month * -1) + 1);
			dt = dt.AddYears((dt.Year * -1) + 1);
			return dt;
		}
		public static DateTime ResetMilliseconds(DateTime dt)
		{
			//DateTime dtRet = new DateTime(dt.Year,dt.Month,dt.Day,dt.Hour,dt.Minute,dt.Second,0);
			//reutrn dtRet;
			//This is more efficient than creating a new datetime.
			dt = dt.AddMilliseconds(dt.Millisecond * -1);
			return dt;
		}
		public static DateTime ResetMillisecondsTicks(DateTime dt)
		{
			//Reset milliseconds and millisecond fractions
			DateTime dtRet = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
			return dtRet;
		}

		public static DateTime ResetMicroseconds(DateTime dt)
		{
			DateTime dtRet = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
			return dtRet;
		}

		public static DateTime NullDate()
		{
			return nullDate;
		}
		private static bool isDateTimeValue(string value)
		{

			return (value.IndexOf(':') > 0 ||
				value.ToUpper().EndsWith("M") ||
				value.Length == 2);
		}
		/**
        * Validate a date in a String. It supports dates in the format specified in the 'date format'
        * preference as well as dates in the same format but without separators. It uses the value
        * of the preference 'First Year of 20th Century' to convert dates with 2-digits years.
        */
		public int VCDate(string date, int picFmt)
		{
			int ampmFmt;
			if (isDateTimeValue(date))
			{
				if (date.IndexOf('A') != -1 || date.IndexOf('P') != -1)
					ampmFmt = 1;
				else
					ampmFmt = 0;
				return VCDateTime(date, picFmt, ampmFmt);
			}
			if (isNullDate(date))
				return 1;
			return (CToD(date, picFmt) == nullDate) ? 0 : 1;
		}
		public int VCDateTime(string date, int picFmt, int ampmFmt)
		{
			if (isNullDateTime(date, picFmt, ampmFmt))
				return 1;
			return (CToT(date, picFmt, ampmFmt) == nullDate) ? 0 : 1;
		}
		private static bool isNullDate(string date)
		{
			StringBuilder dt = new StringBuilder(date);
			dt.Replace('/', ' ');
			dt.Replace('-', ' ');
			dt.Replace(':', ' ');
			if (dt.ToString().Trim().Length > 0)
				return false;
			return true;
		}
		private static bool isNullDate(DateTime date)
		{
			return (date.Year == 1 && date.Month == 1 && date.Day == 1);
		}
		private static bool isNullDateTime(string date, int format, int ampmFmt)
		{
			// It always comes with date / time
			StringBuilder dt = new StringBuilder(date);
			dt.Replace('/', ' ');
			dt.Replace('-', ' ');
			if (dt.ToString().Trim().Length > 0)
			{
				return isNullTimeValue(dt.ToString(), ampmFmt);
			}
			return true;
		}
		private static bool isNullTimeValue(string date, int ampmFmt)
		{
			StringBuilder dt = new StringBuilder(date);
			if (ampmFmt == 1)   // 12 hours format
			{
				dt.Replace("12:00:00.000", "");
				dt.Replace("12:00:00", "");
				dt.Replace("12:00", "");
				dt.Replace("12 ", "");
				dt.Replace("AM", "");
			}
			dt.Replace("00:00:00.000", "");
			dt.Replace("00:00:00", "");
			dt.Replace("00:00", "");
			dt.Replace("00", "");

			if (dt.ToString().Trim().Length > 0)
				return false;
			return true;
		}
		public static string getYYYYMMDD(DateTime date)
		{
			return (StringUtil.PadL(StringUtil.Str(Year(date), 4, 0), 4, '0') +
				StringUtil.PadL(StringUtil.Str(Month(date), 2, 0), 2, '0') +
				StringUtil.PadL(StringUtil.Str(Day(date), 2, 0), 2, '0'));
		}
		public static string getYYYYMMDDHHMMSSnosep(DateTime date, bool hasMilliseconds)
		{
			string sDate = getYYYYMMDD(date);
			sDate += StringUtil.PadL(StringUtil.Str(Hour(date), 2, 0), 2, '0');
			sDate += StringUtil.PadL(StringUtil.Str(Minute(date), 2, 0), 2, '0');
			sDate += StringUtil.PadL(StringUtil.Str(Second(date), 2, 0), 2, '0');
			if (hasMilliseconds) sDate += StringUtil.PadL(StringUtil.Str(MilliSecond(date), 3, 0), 3, '0');
			return (sDate);
		}
		private static DateTime ConvertDateTime(DateTime dt, OlsonTimeZone FromTimezone, OlsonTimeZone ToTimezone)
		{
			if (isNullDate(dt))
				return dt;
			DateTime ret;
			TimeSpan offset;
			DateTime dtconverted;
			int milliSeconds;


			// save milliseconds and reset
			milliSeconds = dt.Millisecond;
			dt = DateTimeUtil.ResetMilliseconds(dt);

			ret = toUniversalTime(dt, FromTimezone);

			if (ToTimezone == null)
				dtconverted = CurrentTimeZoneToLocalTime(ret);
			else
			{
				if (ret < OlsonTimeZone.MinTime || ret > OlsonTimeZone.MaxTime)
				{
					offset = CurrentOffset(ToTimezone);
					dtconverted = ret + offset; //UTC to ToTimezone
				}
				else
				{
					try
					{
						dtconverted = ToTimezone.ToLocalTime(ret);
					}
					catch (ArgumentOutOfRangeException)
					{
						//Avoid InSpringForwardGap/InFallBackRange condition
						dtconverted = ToTimezone.ToLocalTime(ret.AddHours(-1)).AddHours(1);
					}
				}
			}
			return dtconverted.AddMilliseconds(milliSeconds);
		}

		public static DateTime Local2DBserver(DateTime dt, OlsonTimeZone ClientTimezone)
		{
			try
			{
				Preferences.StorageTimeZonePty storagePty = Preferences.getStorageTimezonePty();
				if (ClientTimezone == null || isNullDate(dt) || storagePty == Preferences.StorageTimeZonePty.Undefined)
					return dt;
				OlsonTimeZone ToTimezone = (storagePty == Preferences.StorageTimeZonePty.Utc) ? TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Utc.Id) : TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id);
				return ConvertDateTime(dt, ClientTimezone, ToTimezone);

			}
			catch (Exception ex)
			{
				GXLogging.Error(log, ex, "Local2DBserver error");
				throw ex;
			}
		}
		public static DateTime DBserver2local(DateTime dt, OlsonTimeZone ClientTimezone)
		{
			try
			{
				Preferences.StorageTimeZonePty storagePty = Preferences.getStorageTimezonePty();
				if (ClientTimezone == null || isNullDate(dt) || storagePty == Preferences.StorageTimeZonePty.Undefined)
					return dt;
				OlsonTimeZone FromTimezone = (storagePty == Preferences.StorageTimeZonePty.Utc) ? TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Utc.Id) : TimeZoneUtil.GetInstanceFromWin32Id(TimeZoneInfo.Local.Id);
				return ConvertDateTime(dt, FromTimezone, ClientTimezone);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, ex, "DBserver2local error");
				throw ex;
			}
		}
		public static string FormatDateTimeParmMS(DateTime date)
		{
			if (date.Equals(nullDate))
				return "";
			return getYYYYMMDDHHMMSSnosep(date, true);
		}
		public static string FormatDateTimeParm(DateTime date)
		{
			if (date.Equals(nullDate))
				return "";
			return getYYYYMMDDHHMMSSnosep(date, false);
		}
		public static string FormatDateParm(DateTime date)
		{
			if (date.Equals(nullDate))
				return "";

			return getYYYYMMDD(date);
		}
		public DateTime ParseDateParm(string valueString)
		{
#if NETCORE
            valueString = valueString.Trim('"');
#endif

			if (valueString.Replace("/", "").Trim().Length == 0)
				return nullDate;
			if (valueString.IndexOf('/') != -1)
			{
				DateTime dtValue;
				if (DateTime.TryParse(valueString, cultureInfo.DateTimeFormat, DateTimeStyles.None, out dtValue))
					return dtValue;
			}

			return YMDHMSToT((int)NumberUtil.Val(valueString.Substring(0, 4)),
				(int)NumberUtil.Val(valueString.Substring(4, 2)),
				(int)NumberUtil.Val(valueString.Substring(6, 2)),
				0,
				0,
				0, false);
		}
		public DateTime ParseDTimeParm(string valueString)
		{
#if NETCORE
            valueString = valueString.Trim('"');
#endif

			if (valueString.Trim().Length == 0 || isNullDateTime(valueString, 0, 1))
				return nullDate;
			if (valueString.IndexOf('/') != -1)
			{
				DateTime dtValue;
				if (DateTime.TryParse(valueString, cultureInfo.DateTimeFormat, DateTimeStyles.None, out dtValue))
					return dtValue;
			}

			int mil = (valueString.Trim().Length >= 17)? (int)NumberUtil.Val(valueString.Substring(14, 3)):0;

			return YMDHMSMToT((int)NumberUtil.Val(valueString.Substring(0, 4)),
				(int)NumberUtil.Val(valueString.Substring(4, 2)),
				(int)NumberUtil.Val(valueString.Substring(6, 2)),
				(int)NumberUtil.Val(valueString.Substring(8, 2)),
				(int)NumberUtil.Val(valueString.Substring(10, 2)),
				(int)NumberUtil.Val(valueString.Substring(12, 2)), mil, false);

		}
		public DateTime ParseDateOrDTimeParm(string valueString)
		{

			if (valueString.Trim().Length == 0 || isNullDateTime(valueString, 0, 1))
				return nullDate;
			if (valueString.IndexOf('/') != -1)
				return DateTime.Parse(valueString, cultureInfo.DateTimeFormat);

			if (valueString.Trim().Length > 8)
				return ParseDTimeParm(valueString);
			else
				return ParseDateParm(valueString);
		}
		public static long getDateAsTime(System.DateTime dateTime)
		{
			return (long)((dateTime.Ticks - datetTime1970.Ticks) / timeConversionFactor);
		}
		public static DateTime getTimeAsDate(long ticks)
		{
			return new DateTime(datetTime1970.Ticks + ticks * timeConversionFactor);
		}
		static private DateTime fromUniversalTime(DateTime dt, OlsonTimeZone ToTimezone)
		{

			int milliSeconds = 0;

			if (!isNullDate(dt))
			{
				// save milliseconds and reset
				milliSeconds = dt.Millisecond;
				dt = DateTimeUtil.ResetMilliseconds(dt);
			}
			DateTime ret;
			TimeSpan offset;
			if (ToTimezone == null)
				ret = CurrentTimeZoneToLocalTime(dt);
			else
			{
				if (dt < OlsonTimeZone.MinTime || dt > OlsonTimeZone.MaxTime)
				{
					offset = CurrentOffset(ToTimezone);
					ret = dt + offset; //FromTimezone To UTC
				}
				else
				{
					try
					{
						ret = ToTimezone.ToLocalTime(dt);
					}
					catch (ArgumentOutOfRangeException)
					{
						//Avoid InSpringForwardGap/InFallBackRange condition
						ret = ToTimezone.ToLocalTime(dt.AddHours(-1)).AddHours(1);
					}
				}
			}
			return ret.AddMilliseconds(milliSeconds);
		}

		static private DateTime toUniversalTime(DateTime dt, OlsonTimeZone FromTimezone)
		{
			int milliSeconds = 0;

			if (!isNullDate(dt))
			{
				// save milliseconds and reset
				milliSeconds = dt.Millisecond;
				dt = DateTimeUtil.ResetMilliseconds(dt);
			}

			DateTime ret;
			TimeSpan offset;
			if (FromTimezone == null)
				ret = CurrentTimeZoneToUniversalTime(dt);
			else
			{
				if (dt < OlsonTimeZone.MinTime || dt > OlsonTimeZone.MaxTime)
				{
					offset = CurrentOffset(FromTimezone);
					ret = dt - offset; //FromTimezone To UTC
				}
				else
				{
					try
					{
						ret = FromTimezone.ToUniversalTime(dt);
					}
					catch (ArgumentOutOfRangeException)
					{
						//Avoid InSpringForwardGap/InFallBackRange condition
						ret = FromTimezone.ToUniversalTime(dt.AddHours(-1)).AddHours(1);
					}
				}
			}
			return ret.AddMilliseconds(milliSeconds);
		}

		static private bool isNullDateCompatible(DateTime dt)
		{
			if (Config.GetValueOf("TimeInUtcBug", out string value) && value.StartsWith("y"))
				return dt == DateTime.MinValue;
			else
				return isNullDate(dt);
		}

		static public DateTime fromUniversalTime(DateTime dt)
		{
			return isNullDateCompatible(dt) ? dt : fromUniversalTime(dt, GxContext.Current.GetOlsonTimeZone());
		}

		static public DateTime toUniversalTime(DateTime dt)
		{
			return isNullDateCompatible(dt) ? dt : toUniversalTime(dt, GxContext.Current.GetOlsonTimeZone());
		}

		static public DateTime toUniversalTime(DateTime dt, IGxContext context)
		{
			return isNullDateCompatible(dt) ? dt : toUniversalTime(dt, context.GetOlsonTimeZone());
		}

		static public DateTime FromTimeZone(DateTime dt, String sTZ, IGxContext context)
		{
			OlsonTimeZone fromTimeZone = TimeZoneUtil.GetInstanceFromOlsonName(sTZ);
			if (fromTimeZone != null)
				return ConvertDateTime(dt, fromTimeZone, context.GetOlsonTimeZone());
			return dt;
		}

	}

	public class FileUtil
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.FileUtil));
		public static byte DeleteFile(string fileName)
		{
			try
			{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				File.Delete(fileName);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				return (byte)1;
			}
			catch
			{
				return (byte)0;
			}
		}
		public static byte FileExists(string fileName)
		{
			bool fileNameIsURI = fileName.StartsWith(GXUri.UriSchemeHttp);
			if (!fileNameIsURI)
			{
				if (File.Exists(fileName))
					return 1;
			}
			else
			{
				return RemoteFileExists(fileName);
			}
			return 0;
		}

#if !NETCORE
		///
		/// Checks the file exists or not.
		///
		/// The URL of the remote file.
		/// 1 : If the file exits, 0 if file not exists
		private static byte RemoteFileExists(string url)
		{
			byte result = 0;
			using (System.Net.WebClient client = new System.Net.WebClient())
			{
				try
				{
					using (Stream stream = client.OpenRead(url))
					{
						if (stream != null)
						{
							result = 1;
						}
					}
				}
				catch (System.Net.WebException)
				{
				}
			}
			return result;
		}

		public static string GetStandardOutputFile()
		{
			return Path.Combine(FileUtil.GetStartupDirectory(), AppDomain.CurrentDomain.FriendlyName + ".out");
		}
#else
        private static byte RemoteFileExists(string url)
        {
            return (byte)0;
        }
        //In command line, return the base directory, web.
        public static string GetBasePath()
        {
            return Directory.GetParent(FileUtil.GetStartupDirectory()).FullName;
        }
#endif
		public static string GetStartupDirectory()
		{
			string dir = Assembly.GetCallingAssembly().GetName().CodeBase;
			Uri uri = new Uri(dir);
			return Path.GetDirectoryName(uri.LocalPath);
		}

		public static string UriToPath(string uriString)
		{
			try
			{

				Uri uri = new Uri(uriString);
				return uri.AbsolutePath;
			}
			catch (UriFormatException)
			{
				return uriString;
			}
		}
		public static string getTempFileName(string baseDir, string name="", string extension="tmp", GxFileType fileType = GxFileType.Private)
		{
			name = FixFileName(FileUtil.FileNamePrettify(name), string.Empty);
			return tempFileName(baseDir, name, extension);
		}

		private static string tempFileName(string baseDir, string name, string extension)
		{
			string fileName;
			try
			{
				fileName = PathUtil.SafeCombine(baseDir, $"{name}{NumberUtil.RandomGuid()}.{extension}");
			}
			catch (ArgumentException)//Illegal characters in path
			{
				fileName = PathUtil.SafeCombine(baseDir, $"{name}{NumberUtil.RandomGuid()}.{extension}");
			}
			return fileName;
		}

		public static string NormalizeSource(string source, string baseDirectory)
		{
			string file;

			try
			{
				if (!Path.IsPathRooted(source) && baseDirectory.Trim().Length > 0)
					file = Path.Combine(baseDirectory, source);
				else
					file = source;
			}
			catch (ArgumentException ex)
			{
				GXLogging.Error(log, ex, "NormalizeSource error file:", source);
				return source;
			}
			return file;
		}
		public static string GetFileType(string FileName)
		{
			if (FileName.Trim().Length == 0)
				return string.Empty;

			if (GxUploadHelper.IsUpload(FileName))
			{
				return new GxFile(string.Empty, FileName, GxFileType.PrivateAttribute).GetExtension();
			}

			string extension = string.Empty;
			try
			{
				Uri uri;
				if (Uri.TryCreate(FileName, UriKind.RelativeOrAbsolute, out uri) && uri.IsAbsoluteUri)
					FileName = uri.AbsolutePath;
				extension = Path.GetExtension(FileName);//FileNames with URI or local (exist)
			}
			catch
			{
				FileInfo fi = new FileInfo(FileName); //Local file that do not exist
				extension = fi.Extension;
			}
			int extStart = extension.LastIndexOf('.');
			if (extStart == -1)
				return extension;
			else
				return extension.Replace(".", "");
		}

		public static string FileNamePrettify(string s)
		{
			if (!string.IsNullOrEmpty(s))
			{
				string str = GXUtil.RemoveDiacritics(s.Trim().ToLower()); //remove accents
				str = Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space  
				str = Regex.Replace(str, @"\s", "-"); // //Replace spaces by dashes
				return str;
			}
			else
			{
				return s;
			}
		}

		public static string GetFileName(string FileName)
		{
			if (FileName.Trim().Length == 0)
				return "";

			if (GxUploadHelper.IsUpload(FileName))
			{
				FileName = new GxFile(string.Empty, FileName, GxFileType.PrivateAttribute).GetName();
			}
			try
			{
				return Path.GetFileNameWithoutExtension(FileName);//FileNames with URI or local (exist)
			}
			catch
			{
				FileInfo fi = new FileInfo(FileName);//Local file that do not exist
				int extStart = fi.Name.LastIndexOf('.');
				if (extStart == -1)
					return fi.Name;
				else
					return fi.Name.Substring(0, extStart);
			}
		}
		public static string GetCompleteFileName(string name, string type)
		{
			if (String.IsNullOrEmpty(name))
				return "";
			if (String.IsNullOrEmpty(type))
				return name;

			return name + "." + type;
		}
		internal static FileStream FileStream(ref string fileName, FileMode mode, FileAccess access, string baseDir)
		{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
			try
			{
				return new FileStream(fileName, mode, access);
			}
			catch (ArgumentException ex)//Illegal characters in path
			{
				GXLogging.Error(log, ex, "Invalid Filename: ", fileName);
				fileName = FixFileName(fileName, baseDir);//Debe modificar fileName, parametro de inout.
				return new FileStream(fileName, mode, access);
			}
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
		}

		public static string FixFileName(string fileName, string baseDir)
		{
			string regexSearch = new string(Path.GetInvalidFileNameChars());
			Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
			if (!string.IsNullOrEmpty(baseDir))
			{
				string name = fileName;
				if (name.Contains(baseDir))
				{
					name = fileName.Substring(baseDir.Length);
				}
				name = r.Replace(name, String.Empty);
				fileName = Path.Combine(baseDir, name);
			}
			else
			{
				fileName = r.Replace(fileName, String.Empty);
			}
			return fileName;
		}
	}
	public class PathUtil
	{
		const string schemeRegEx = @"^([a-z][a-z0-9+\-.]*):";
		static Regex scheme = new Regex(schemeRegEx, RegexOptions.IgnoreCase);

		public static bool IsAbsoluteUrl(string url)
		{
			Uri result;
			return Uri.TryCreate(url, UriKind.Absolute, out result) && (result.Scheme == GXUri.UriSchemeHttp || result.Scheme == GXUri.UriSchemeHttps || result.Scheme == GXUri.UriSchemeFtp);
		}
		public static bool IsAbsoluteUrlOrAnyScheme(string url)
		{
			return (!String.IsNullOrEmpty(url)) && (IsAbsoluteUrl(url) || scheme.IsMatch(url));
		}

		public static bool HasUrlQueryString(string url)
		{
			return url.Contains("?");
		}

		public static Uri GetBaseUri()
		{
			return new Uri(GxContext.StaticPhysicalPath() + Path.DirectorySeparatorChar.ToString());
		}

		public static string RelativeURL(string blobPath)
		{
			if (IsAbsoluteUrl(blobPath))
			{
				return blobPath;
			}
			else
			{
				string fileName = Path.GetFileNameWithoutExtension(blobPath);
				blobPath = RelativePath(blobPath);
#pragma warning disable SYSLIB0013 // EscapeUriString
				return StringUtil.ReplaceLast(blobPath, fileName, Uri.EscapeUriString(fileName));
#pragma warning disable SYSLIB0013 // EscapeUriString
			}
		}
		public static bool AbsoluteUri(string fileName, out Uri result)
		{
			result = null;
			if (Uri.TryCreate(fileName, UriKind.Absolute, out result) && (result.IsAbsoluteUri))
			{
				return true;
			}
			else
			{
				Uri relative;
				if (Uri.TryCreate(fileName, UriKind.Relative, out relative))
				{
					if (!string.IsNullOrEmpty(Preferences.getBLOB_PATH_SHORT_NAME()))
					{
						int idx = Math.Max(fileName.IndexOf(Preferences.getBLOB_PATH_SHORT_NAME() + '/', StringComparison.OrdinalIgnoreCase), fileName.IndexOf(Preferences.getBLOB_PATH_SHORT_NAME() + '\\', StringComparison.OrdinalIgnoreCase));
						if (idx >= 0)
						{
							fileName = fileName.Substring(idx);
							Uri localRelative;
							if (Uri.TryCreate(fileName, UriKind.Relative, out localRelative))
								relative = localRelative;
						}
					}

					if (Uri.TryCreate(PathUtil.GetBaseUri(), relative, out result))
					{
						return true;
					}
				}
				return false; ;
			}

		}

		public static string RelativePath(string blobPath)
		{
			if (string.IsNullOrEmpty(blobPath))
				return blobPath;
			string basePath = GxContext.StaticPhysicalPath();
			if (blobPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
			{
				blobPath = blobPath.Substring(basePath.Length);
			}
			else if (!string.IsNullOrEmpty(Preferences.getBLOB_PATH_SHORT_NAME()))

			{
				int idx = Math.Max(blobPath.IndexOf(Preferences.getBLOB_PATH_SHORT_NAME() + '/', StringComparison.OrdinalIgnoreCase), blobPath.IndexOf(Preferences.getBLOB_PATH_SHORT_NAME() + '\\', StringComparison.OrdinalIgnoreCase));
				if (idx >= 0)
					blobPath = blobPath.Substring(idx);
			}
			return blobPath.Replace(Path.DirectorySeparatorChar, '/');
		}
		public static GxSimpleCollection<string> RelativePath(GxSimpleCollection<string> blobPaths)
		{
			GxSimpleCollection<string> newBlobPaths = new GxSimpleCollection<string>();
			for (int i = 1; i <= blobPaths.Count; i++)
			{
				string blobPath = (string)blobPaths.Item(i);
				newBlobPaths.Add(RelativePath(blobPath));
			}
			return newBlobPaths;
		}
		[Obsolete("UploadPath in PathUtil is deprecated", false)]
		public static string UploadPath(string filename)
		{
			return filename;
		}
		public static string CompletePath(string pathName, string basePath)
		{
			if (pathName == null)
				return pathName;
			if (basePath.Trim().Length == 0)
				return pathName;
			if (pathName.Trim().Length == 0)
				return pathName;
			string pp;
			if (pathName.ToLower().StartsWith("http:") || pathName.StartsWith("//"))
				return pathName;
			if (pathName.StartsWith("\\\\"))
				return pathName;
			if (pathName[1] == ':')
			{
				if (pathName.Length >= 2)
				{
					if (!isNumber(pathName[2]))
						return pathName;
				}
				else
					return pathName;
			}
			if (pathName[0] == '\\')
			{
				pp = basePath.Substring(0, 2);
				if (pp[1] == ':')
					return pp + pathName;
				else
					return pathName;
			}
			else
				return Path.Combine(basePath, pathName);
		}
		static bool isNumber(char n)
		{
			if (n >= '0' && n <= '9')
				return true;
			return false;
		}
		public static string GetValidPath(string path, string replaceStr)
		{
			return Path.GetInvalidPathChars().Aggregate(path, (current, c) => current.Replace(c.ToString(), replaceStr));
		}
		public static bool IsValidFileName(string path)
		{
			return !String.IsNullOrEmpty(path) && (path.IndexOfAny(Path.GetInvalidFileNameChars()) < 0);
		}
		public static bool IsValidFilePath(string path)
		{
			return !String.IsNullOrEmpty(path) && (path.IndexOfAny(Path.GetInvalidPathChars()) < 0);
		}
		public static string GetValidFileName(string path, string replaceStr)
		{
			string validPath = path;
			if (!IsValidFilePath(path))
				validPath = GetValidPath(path, "_");

			Uri result;
			if (Uri.TryCreate(validPath, UriKind.Absolute, out result))
				validPath = result.LocalPath;

			string fileName = Path.GetFileName(validPath);
			if (!IsValidFileName(fileName))
			{
				return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), replaceStr));
			}
			else
				return fileName;
		}

		internal static string SafeCombine(string basePath, string fileName)
		{
			return Path.Combine(basePath, Path.GetFileName(fileName));
		}
	}

	public class GXUtil
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GXUtil));
#if !NETCORE
		static Hashtable domains;
		static string DOMAINS_FILE;
#endif
		const int UNKNOWN_DBMS_VERSION = 99;

		public static void SystemExit()
		{
#if !NETCORE
			Process.GetCurrentProcess().Kill();
#endif
		}

		public static void WriteLog(string message)
		{
			StackTrace st = new StackTrace(new StackFrame(1, true));
			StackFrame sf = st.GetFrame(0);
			GXLogging.Debug(log, String.Format("At file: {0}, line: {1}, {2}", sf.GetFileName(), sf.GetFileLineNumber(), message));
		}
		public static void WriteLogError(string message)
		{
			StackTrace st = new StackTrace(new StackFrame(1, true));
			StackFrame sf = st.GetFrame(0);
			GXLogging.Error(log, String.Format("At file: {0}, line: {1}, {2}", sf.GetFileName(), sf.GetFileLineNumber(), message));
		}
		public static void WriteLogInfo(string message)
		{
			StackTrace st = new StackTrace(new StackFrame(1, true));
			StackFrame sf = st.GetFrame(0);
			GXLogging.Info(log, String.Format("At file: {0}, line: {1}, {2}", sf.GetFileName(), sf.GetFileLineNumber(), message));
		}
		public static void WriteTLog(string message)
		{
			StackTrace st = new StackTrace(new StackFrame(1, true));
			StackFrame sf = st.GetFrame(0);
			GXLogging.Trace(log, String.Format("At file: {0}, line: {1}, {2}", sf.GetFileName(), sf.GetFileLineNumber(), message));
		}
		public static void WriteLogRaw(string message, Object obj)
		{
			GXLogging.Debug(log, String.Format("{0}{1}", message, obj.ToString()));
		}
#if !NETCORE
		internal static void WinMessage(string errorInfo, string v)
		{
			GXLogging.Error(log, "WinMessage error", errorInfo, v);

			IGxMessageFactory msgFactory = Dialogs.Message;
			if (msgFactory != null && msgFactory.GetMessageDialog() != null)
			{
				msgFactory.GetMessageDialog().Show(errorInfo, v);
			}
		}

		public static string EnumerationDescription(int domainNumber, string domainValue)
		{
			string domainId = domainNumber.ToString();
			string domainDescription;
			if (domains == null)
			{
				domains = new Hashtable();
				DOMAINS_FILE = GxContext.StaticPhysicalPath() + Path.DirectorySeparatorChar + "domains.ini";
			}
			if (domains[domainId] == null)
			{
				domains[domainId] = new Hashtable();
			}
			if ((domainDescription = (string)((Hashtable)domains[domainId])[domainValue]) == null)
			{
				domainDescription = new IniFile(DOMAINS_FILE).IniReadValue(domainId, domainValue);
				((Hashtable)domains[domainId])[domainValue] = domainDescription;
				return domainDescription;
			}
			else return domainDescription;

		}
#else
        public static string EnumerationDescription(int domainNumber, string domainValue){
            return domainValue;
        }
#endif

		public static byte[] readToByteArray(Stream instream)
		{

			byte[] binary;
			using (BinaryReader br = new BinaryReader(instream))
			{
				binary = br.ReadBytes((int)instream.Length);
			}
			return binary;
		}

		static public string GetMessage(string s)
		{
			return s;
		}
		static public string GetMessage(string s, object[] vec)
		{
			return s;
		}
		public static string EncodeJSConstant(string inp)
		{
			return HttpUtility.JavaScriptStringEncode(inp);
		}

		public static string AccessKey(string OCaption)
		{
			string accessKey = "";
			if (OCaption.IndexOf('&') != -1)
			{
				for (int i = 0; i < OCaption.Length - 1; i++)
				{
					if (OCaption[i] == '&' && OCaption[i + 1] != '&')
					{
						accessKey = accessKey + OCaption[i + 1];
						break;
					}
				}
			}
			return accessKey;
		}
		public static string AccessKeyCaption(string OCaption)
		{
			string DCaption = "";
			if (OCaption.IndexOf('&') == -1)
				return OCaption;
			for (int i = 0; i < OCaption.Length - 1; i++)
			{
				if (OCaption[i] == '&' && OCaption[i + 1] != '&')
				{
					DCaption += OCaption.Substring(i + 1);
					break;
				}
				else
					DCaption += OCaption[i];
			}
			return DCaption;
		}


		public static bool CompressResponse()
		{
			string val;
			if (Config.GetValueOf("COMPRESS_HTML", out val))
				if (val == "1")
					return true;
			return false;
		}

		public static void SetGZip(HttpContext httpContext)
		{
#if !NETCORE
			if (httpContext != null)
			{
				string AcceptEncoding = httpContext.Request.Headers["Accept-Encoding"];
				if (!string.IsNullOrEmpty(AcceptEncoding))
				{
					HttpResponse response = httpContext.Response;
					if (AcceptEncoding.Contains("gzip"))
					{
						if (!(response.Filter is GZipStream))
						{
							response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
							response.AppendHeader("Content-Encoding", "gzip");
						}
					}
					else if (AcceptEncoding.Contains("deflate"))
					{
						if (!(response.Filter is DeflateStream))
						{
							response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
							response.AppendHeader("Content-Encoding", "deflate");
						}
					}
				}
			}
#endif
		}

		public static string RemoveDiacritics(string text)
		{
			if (!String.IsNullOrEmpty(text))
			{
				char[] chars = new char[text.Length];
				int charIndex = 0;

				text = text.Normalize(NormalizationForm.FormD);
				for (int i = 0; i < text.Length; i++)
				{
					char c = text[i];
					if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
						chars[charIndex++] = c;
				}
				return new string(chars, 0, charIndex).Normalize(NormalizationForm.FormC);
			}
			return text;
		}

		public static bool ContainsNoAsciiCharacter(string input)
		{
			const int MaxAnsiCode = 127;
			const char DoubleQuote = '"';
			if (!string.IsNullOrEmpty(input))
			{
				foreach (char c in input)
				{
					if (((int)c) > MaxAnsiCode || c == DoubleQuote)
						return true;
				}
			}
			return false;
		}
		static public string UrlEncode(string s)
		{
#if NETCORE
            return WebUtility.UrlEncode(s);
#else
			return HttpUtility.UrlEncode(s);
#endif
		}
		static public string UrlDecode(string s)
		{
#if NETCORE
            return WebUtility.UrlDecode(s);
#else
			return HttpUtility.UrlDecode(s);
#endif
		}

		public static string ParmsEncryptionKey(IGxContext context)
		{
			string keySourceType = string.Empty,
				GXKey = string.Empty;
			if (Config.GetValueOf("USE_ENCRYPTION", out keySourceType))
			{
				GXKey = GetEncryptionKey(context, keySourceType);
			}
			return GXKey;
		}

		public static string GetEncryptionKey(IGxContext context, string keySourceType)
		{
			if (string.IsNullOrEmpty(keySourceType))
			{
				Config.GetValueOf("USE_ENCRYPTION", out keySourceType);
			}
			string GXKey = string.Empty;
			switch (keySourceType)
			{
				case "SESSION":
					GXKey = Crypto.Decrypt64(context.GetCookie("GX_SESSION_ID"), Crypto.GetServerKey());
					break;
				case "SITE":
				default:
					GXKey = Crypto.GetSiteKey();
					break;
			}
			return GXKey;
		}

		public static string DecryptParm(object parm, string gxkey)
		{
			string value = parm.ToString();
			if (!String.IsNullOrEmpty(gxkey))
			{
				string strValue = Crypto.Decrypt64(value.ToString(), gxkey, true);
				if ((String.CompareOrdinal(StringUtil.Right(strValue, 6).TrimEnd(' '),
					Crypto.CheckSum(StringUtil.Left(strValue, (short)(StringUtil.Len(strValue) - 6)), 6).TrimEnd(' ')) == 0))
				{
					value = StringUtil.Left(strValue, (short)(StringUtil.Len(strValue) - 6));
				}
				else
				{
					throw new Exception("Invalid Parameter (403 Forbidden)");
				}
			}
			return value;
		}

		public static string HtmlEncodeInputValue(string sText)
		{
			StringBuilder buffer = new StringBuilder();
			foreach (char character in sText)
			{
				if (character == '<')
				{
					buffer.Append("&lt;");
				}
				else if (character == '>')
				{
					buffer.Append("&gt;");
				}
				else if (character == '\'')
				{
					HtmlEncodeInputValue(39, buffer);
				}
				else if (character == '&')
				{
					buffer.Append("&amp;");
				}
				else
				{
					buffer.Append(character);
				}
			}
			return buffer.ToString();
		}
		private static void HtmlEncodeInputValue(int character, StringBuilder buffer)
		{
			string padding = "";
			if (character <= 9)
			{
				padding = "00";
			}
			else if (character <= 99)
			{
				padding = "0";
			}
			string number = padding + character.ToString();
			buffer.Append("&#" + number + ";");
		}
#if NETCORE
		internal static string HTMLClean(string text)
		{
			HtmlSettings htmlSettings = new HtmlSettings { PrettyPrint = true };
			htmlSettings.RemoveScriptStyleTypeAttribute = false;
			htmlSettings.RemoveOptionalTags = false;
			htmlSettings.AttributeQuoteChar = '\'';
			htmlSettings.RemoveAttributeQuotes = false;
			htmlSettings.MinifyCss = false;
			htmlSettings.MinifyCssAttributes = false;
			htmlSettings.MinifyJsAttributes = false;
			htmlSettings.MinifyJs = false;
			return Uglify.Html(text, htmlSettings).Code;
		}
#endif
		static public string ValueEncode(string sText)
		{
			return ValueEncode(sText, false, false);
		}
		static public string ValueEncode(string sText, bool encodeWhiteSpace, bool encodeEnter)
		{
			bool inStartingSpaces = true;
			bool firstSpace = true;
			if (sText == null)
				return "";
			StringBuilder sb = new StringBuilder();
			string nl = StringUtil.NewLine();
			int nlSize = nl.Length;
			string sTextEncoded = HttpUtility.HtmlEncode(sText);
			for (int i = 0; i < sTextEncoded.Length; i++)
			{
				char currentChar = sTextEncoded[i];
				if (currentChar != ' ' && currentChar != '\t')
				{

					inStartingSpaces = false;
					firstSpace = true;
				}
				switch (currentChar)
				{

					default:
						if (encodeEnter && sTextEncoded.IndexOf(nl, i, Math.Min(nlSize, sTextEncoded.Length - i)) == i)
						{

							sb.Append("<br>");
							i += nlSize - 1;
						}
						else if (encodeEnter && currentChar == nl[1])
						{
							sb.Append("<br>");
						}
						else if (currentChar == nl[0] && i == sTextEncoded.Length - 1)

							sb.Append("%0d");
						else if (encodeWhiteSpace && (currentChar == ' ' || currentChar == '\t'))
						{
							if (firstSpace && !inStartingSpaces)
							{
								sb.Append(currentChar);
								firstSpace = false;
							}
							else
							{
								if (currentChar == '\t')
									sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;");
								else
									sb.Append("&nbsp;");
							}
						}
						else
							sb.Append(currentChar);
						break;
				}
			}
			return sb.ToString();
		}
		static public string ValueEncodeFull(string sText)
		{
			if (sText == null)
				return "";
			StringBuilder sb = new StringBuilder();
			string sTextEncoded = HttpUtility.HtmlEncode(sText);
			for (int i = 0; i < sTextEncoded.Length; i++)
			{
				char currentChar = sTextEncoded[i];
				if (currentChar == ' ')
					sb.Append("&nbsp;");
				else if (currentChar == '\t')
					sb.Append("&#9;");
				else
					sb.Append(currentChar);
			}
			return sb.ToString();
		}
		internal static string AttributeEncode(string sText)
		{
			return HttpUtility.HtmlAttributeEncode(sText);
		}

		public static string HtmlEndTag(HTMLElement element)
		{
			if ((Preferences.DocType == HTMLDocType.HTML4 || Preferences.DocType == HTMLDocType.NONE || Preferences.DocType == HTMLDocType.HTML4S) &&
				(element == HTMLElement.IMG ||
				element == HTMLElement.INPUT ||
				element == HTMLElement.META ||
				element == HTMLElement.LINK))
				return ">";
			else if (element == HTMLElement.OPTION)
				if (Preferences.DocType == HTMLDocType.XHTML1 || Preferences.DocType == HTMLDocType.HTML5)
					return "</option>";
				else
					return "";
			else
				return "/>";
		}
		public static string HtmlDocType()
		{
			switch (Preferences.DocType)
			{
				case HTMLDocType.HTML4:
					if (Preferences.DocTypeDTD)
						return "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
					else
						return "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">";
				case HTMLDocType.HTML4S:
					if (Preferences.DocTypeDTD)
						return "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">";
					else
						return "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\">";
				case HTMLDocType.XHTML1:
					if (Preferences.DocTypeDTD)
						return "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
					else
						return "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\">";
				case HTMLDocType.HTML5: return "<!DOCTYPE html>";
				default: return "";
			}
		}

		static public string ValueDecode(string sText)
		{
			string sRet;
			if (sText == null)
				return "";
			sRet = HttpUtility.HtmlDecode(sText);
			sRet = sRet.Replace("%0d", "\r");
			return sRet;
		}

		static public string ValueDecodeFull(string sText)
		{
			string sRet;
			if (sText == null)
				return "";
			sRet = sText.Replace("&nbsp;", " ");
			sRet = HttpUtility.HtmlDecode(sText);
			return sRet;
		}
		public static string StringCollectionsToJson(GxStringCollection col1, GxStringCollection col2)
		{
			return StringCollectionsToJsonObj(col1, col2).ToString();
		}

		public static GxJsonArray StringCollectionsToJsonObj(GxStringCollection col1, GxStringCollection col2)
		{
			if (col1.getCount() == 0)
				return StringCollectionToJsonObj(col2);

			GxJsonArray jarray = new GxJsonArray();
			int index = 1;
			while (index <= col1.getCount())
			{
				GxJsonArray item = new GxJsonArray();
				item.Add(col1.item(index));
				item.Add(col2.item(index));
				jarray.Add(item);
				index = index + 1;
			}
			return jarray;
		}

		public static GxJsonArray StringCollectionToJsonObj(GxStringCollection col1)
		{
			GxJsonArray jarray = new GxJsonArray();
			int index = 1;
			while (index <= col1.getCount())
			{
				jarray.Add(col1.item(index));
				index = index + 1;
			}
			return jarray;
		}
		public static bool Confirmed;

		[Obsolete("UserId is deprecated", false)]
		public static string UserId()
		{

			return UserId(null);
		}

		private static string UserId(IGxContext cntxt)
		{
			if (cntxt != null && cntxt.GetProperty(GxDefaultProps.USER_ID) != null)
			{
				GXLogging.Debug(log, "UserId= user in properties");
				return cntxt.GetProperty(GxDefaultProps.USER_ID);
			}
			else
			{
				string value, s=null, s1;
				if (Config.GetValueOf("HTTPCONTEXT_USER_AS_USERID", out value) && value.StartsWith("y"))
				{
					//Virtual directory mapped to another machine, returns the user for "connect as" of virtual directory
					if (cntxt.HttpContext != null)
					{
						s = cntxt.HttpContext.User.Identity.Name;
						GXLogging.Debug(log, "UserId= HttpContext.Current.User.Identity.Name: ", s);
					}
					else
					{
						if (IsWindowsPlatform)
						{
							s = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
							GXLogging.Debug(log, "HttpContext.Current is null, UserId= System.Security.Principal.WindowsIdentity.GetCurrent().Name:", s);
						}
					}
				}
				else
				{
#if NETCORE
					BasicAuthenticationHeaderValue basicAuthenticationHeader = GetBasicAuthenticationHeaderValue(cntxt.HttpContext);
					if (basicAuthenticationHeader != null && basicAuthenticationHeader.IsValidBasicAuthenticationHeaderValue)
					{
						s = basicAuthenticationHeader.UserIdentifier;
					}
					else
					{
						if (IsWindowsPlatform)
							s = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
					}
#else
					s = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
					GXLogging.Debug(log, "UserId= System.Security.Principal.WindowsIdentity.GetCurrent().Name: ", s);
#endif
				}

				if (s == null)
					s = string.Empty;
				if (s.LastIndexOf("\\") != -1)
					try
					{
						s1 = s.Substring(s.LastIndexOf("\\") + 1, s.Length - s.LastIndexOf("\\") - 1);
					}
					catch
					{
						s1 = s;
					}
				else
					s1 = s;

				if (s1.Trim().Length == 0)
					// Windows 98
					return Environment.UserName;
				else
					// Windows 2000, XP, ME
					return s1;
			}
		}
#if NETCORE
        private static BasicAuthenticationHeaderValue GetBasicAuthenticationHeaderValue(HttpContext context)
        {
			if (context == null)
				return null;
			else
			{
				var basicAuthenticationHeader = context.Request.Headers["Authorization"]
					.FirstOrDefault(header => header.StartsWith("Basic", StringComparison.OrdinalIgnoreCase));
				var decodedHeader = new BasicAuthenticationHeaderValue(basicAuthenticationHeader);
				return decodedHeader;
			}
        }
#endif
#if NETCORE
		static int windowsPlatform = -1;
#endif
		public static bool IsWindowsPlatform
		{
			get
			{
#if NETCORE
				if (windowsPlatform == -1)
					windowsPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 1 : 0;

				return (windowsPlatform == 1);
#else
				return true;
#endif
			}
#if NETCORE
			set
			{
				windowsPlatform = value ? 1 : 0;
			}
#endif
		}
		static public int DbmsVersion(IGxContext context, string dataSource)
		{
			string version = context.ServerVersion(dataSource);
			int idx = 0;
			if (!String.IsNullOrEmpty(version) && (idx = version.IndexOf('.')) >= 0)
			{
				return Int32.Parse(version.Substring(0, idx));
			}
			else
			{
				return UNKNOWN_DBMS_VERSION;
			}
		}
		static public bool IsSQLSERVER2005(IGxContext context, string dataSource)
		{
			string version = context.ServerVersion(dataSource);
			if (version.StartsWith("8") || version.StartsWith("7") || version.StartsWith("6"))
				return false;
			else
				return true;
		}

		static public String DataBaseName(IGxContext context, string dataSource)
		{
			return context.DataBaseName(dataSource);
		}
		public static string UserId(string key, IGxContext cntxt, IDataStoreProvider dataStore)
		{
			try
			{
				string prop;
				if (key.ToUpper() == "SERVER" && !(Config.GetValueOf("LOGIN_AS_USERID", out prop) && prop.Equals("1")))
				{
					GXLogging.Debug(log, "UserId= user in ConnectionString");
					if (dataStore != null)
						return dataStore.userId();
				}

				return UserId(cntxt);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "UserId Error", ex);
				throw ex;
			}
		}

		[Obsolete("UserId with string dataSource is deprecated, use UserId((string key, IGxContext cntxt, IDataStoreProvider dataStore) instead", false)]
		public static string UserId(string key, IGxContext cntxt, string id)
		{
			try
			{
				string prop;
				if (key.ToUpper() == "SERVER" && !(Config.GetValueOf("LOGIN_AS_USERID", out prop) && prop.Equals("1")))
				{
					GXLogging.Debug(log, "UserId= user in ConnectionString");
					IGxDataStore dstore = cntxt.GetDataStore(id);
					if (dstore != null)
						return (dstore.UserId);
				}

				return UserId(cntxt);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "UserId Error", ex);
				throw ex;
			}

		}
		public static short SetUserId(string value, IGxContext cntxt, string id)
		{
			if (cntxt != null)
			{
				cntxt.SetProperty(GxDefaultProps.USER_ID, value);
				return 1;
			}
			else
			{
				return 0;
			}
		}
		public static int RGB(int r, int g, int b)
		{
			return Color.FromArgb(0, r, g, b).ToArgb();
		}
#if !NETCORE
		[Obsolete("WrkSt without arguments is deprecated", false)]
		public static string WrkSt()
		{

			return WrkSt(null, HttpContext.Current);
		}
#endif
		public static string WrkSt(IGxContext cntxt)
		{
			if (cntxt != null && cntxt.HttpContext != null)
				return WrkSt(cntxt, cntxt.HttpContext);
			else
				return WrkSt(cntxt, null);
		}

		public static string DeviceId(IGxContext cntxt)
		{
			return GX.ClientInformation.Id;
		}

		public static string ApplicationId(IGxContext cntxt)
		{
			string applicationId = null;
			if (cntxt != null && cntxt.HttpContext != null)
				applicationId = cntxt.HttpContext.Request.Headers["GXApplicationIdentifier"];

			if (applicationId != null)
				return applicationId;
			else
				return string.Empty;
		}

		public static GxStringCollection DefaultWebUser(IGxContext cntxt)
		{
			if (Environment.OSVersion.Version.Major >= 6)
			{ //Widnows Vista, Windows 7
				return DefaultApplicationPoolIdentity();
			}
			else
			{
				GxStringCollection users = new GxStringCollection();
				users.Add(GXUtil.WrkSt(cntxt) + @"\ASPNET");
				return users;
			}
		}
		const string APPPOOL_IDENTITY_TYPE_LOCALSYSTEM = "0";//The application pool runs as LocalSystem.
		const string APPPOOL_IDENTITY_TYPE_LOCALSERVICE = "1";//The application pool runs as LocalService.
		const string APPPOOL_IDENTITY_TYPE_NETWORKSERVICE = "2";//The application pool runs as NetworkService.
		const string APPPOOL_IDENTITY_TYPE_SPECIFICUSER = "3";//The application pool runs as a specified user account.
		const string APPPOOL_IDENTITY_TYPE_APPPOOL = "4";//The application pool runs as a Application Pool identity.
#if NETCORE
		const string IDENTITY_NETCORE_APPPOOL = @"IIS AppPool\NetCore";
#else
		const string IDENTITY_INTEGRATED_APPPOOL_FW40 = @"IIS APPPOOL\ASP.NET v4.0";
		const string IDENTITY_INTEGRATED_APPPOOL_FW35 = @"IIS AppPool\DefaultAppPool";
		const string IDENTITY_CLASSIC_APPPOOL = @"IIS AppPool\Classic .NET AppPool";
#endif
		const string IDENTITY_NETWORK_SERVICE = @"NT AUTHORITY\NETWORK SERVICE";
		const string IDENTITY_LOCAL_SERVICE = @"NT AUTHORITY\LOCAL SERVICE";

		public static GxStringCollection DefaultApplicationPoolIdentity()
		{
			GxStringCollection usernames = new GxStringCollection();
			try
			{
				DirectoryEntry Entry = GetAppPoolEntry();
				if (Entry != null)
				{
					PropertyCollection Properties = Entry.Properties;
					string AppPoolIdentityType = Properties["AppPoolIdentityType"][0].ToString().Trim();
					switch (AppPoolIdentityType)
					{
						case APPPOOL_IDENTITY_TYPE_APPPOOL:
#if NETCORE
							usernames.Add(IDENTITY_NETCORE_APPPOOL);
#else
							usernames.Add(IDENTITY_CLASSIC_APPPOOL);
							usernames.Add(IDENTITY_INTEGRATED_APPPOOL_FW35);
							usernames.Add(IDENTITY_INTEGRATED_APPPOOL_FW40);
#endif
							break;
						case APPPOOL_IDENTITY_TYPE_NETWORKSERVICE:
						case APPPOOL_IDENTITY_TYPE_LOCALSYSTEM:
							usernames.Add(IDENTITY_NETWORK_SERVICE);
							break;
						case APPPOOL_IDENTITY_TYPE_LOCALSERVICE:
							usernames.Add(IDENTITY_LOCAL_SERVICE);
							break;
						case APPPOOL_IDENTITY_TYPE_SPECIFICUSER:
							usernames.Add(Properties["WAMUserName"][0].ToString());
							break;
					}
				}

			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Could not get DefaultApplicationPoolIdentity", ex);
			}

			if (usernames.Count == 0)
			{
#if NETCORE
				usernames.Add(IDENTITY_NETCORE_APPPOOL);
#else
				usernames.Add(IDENTITY_INTEGRATED_APPPOOL_FW35);
				usernames.Add(IDENTITY_INTEGRATED_APPPOOL_FW40);
				usernames.Add(IDENTITY_CLASSIC_APPPOOL);
#endif
				usernames.Add(IDENTITY_NETWORK_SERVICE);
				usernames.Add(IDENTITY_LOCAL_SERVICE);
			}
			return usernames;
		}

		private static DirectoryEntry GetAppPoolEntry()
		{
#if NETCORE
			DirectoryEntry Entry = new DirectoryEntry("IIS://localhost/W3SVC/AppPools/NetCore");
#else
			DirectoryEntry Entry = new DirectoryEntry("IIS://localhost/W3SVC/AppPools/ASP.NET v4.0");
			if (Entry == null)
				Entry = new DirectoryEntry("IIS://localhost/W3SVC/AppPools/DefaultAppPool");
#endif
			return Entry;
		}

#if NETCORE
        private static string WrkSt(IGxContext gxContext, object httpContext)

        {
        return string.Empty;
        }
#else


		private static string WrkSt(IGxContext gxContext, HttpContext httpContext)
		{
			try
			{
				string wrkst = string.Empty, prop = string.Empty;

				if (gxContext != null && gxContext.GetProperty(GxDefaultProps.WORKSTATION) != null)
				{
					GXLogging.Debug(log, "WrkSt= GX_WRKST in properties");
					wrkst = gxContext.GetProperty(GxDefaultProps.WORKSTATION);
				}
				else if (httpContext != null)
				{
					if ((Config.GetValueOf("WRKST_COMPATIBILITY", out prop) && prop.Equals("1"))) //WRKST_COMPATIBILITY=YES at config.gx
						wrkst = httpContext.Server.MachineName;
					else
						wrkst = httpContext.Request.UserHostAddress;
					GXLogging.Debug(log, "WrkSt= &HttpRequest.Remoteaddress");
				}
				else //Win
				{
					GXLogging.Debug(log, "WrkSt= Environment.MachineName");
					wrkst = Environment.MachineName;
				}
				return wrkst;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "WrkSt Error", ex);
				throw ex;
			}
		}
#endif

		internal static string NormalizeEncodingName(string enc1)
		{
			string enc = enc1.ToUpper();
			if (enc.Equals("UTF-8 BOM", StringComparison.OrdinalIgnoreCase))
				return "UTF-8";
			else if (enc.Equals("UTF-32 BOM", StringComparison.OrdinalIgnoreCase))
				return "UTF-32";
			else if (enc.Equals("UTF-32BE BOM", StringComparison.OrdinalIgnoreCase))
				return "UTF-32BE";
			else if (enc.Equals("UTF-32LE BOM", StringComparison.OrdinalIgnoreCase))
				return "UTF-32LE";
			else if (enc.StartsWith("ISO8859_") || enc.StartsWith("8859_"))
				return "ISO-8859-" + enc.Substring(8);
			else if (enc.Equals("UTF8"))
				return "UTF-8";
			else return enc1;
		}
		internal static Encoding GxIanaToNetEncoding(string enc, bool exceptionOnError)
		{
			try
			{
				if (string.IsNullOrEmpty(enc) && !exceptionOnError)
					return new UTF8Encoding(); //Do not use Encoding.UTF8 = new UTF8Encoding(true);
				else
				{
					Encoding resEncoding;
					enc = enc.ToLower();
					switch (enc)
					{
						case "utf-8 bom":
							resEncoding = new UTF8Encoding(true);
							break;
						case "utf-8":
							resEncoding = new UTF8Encoding(false);
							break;
#if !NETCORE
						case "ansi":
							resEncoding = Encoding.Default;
							break;
#endif
						case "utf-32":
							resEncoding = new UTF32Encoding(false, false);
							break;
						case "utf-32be":
							resEncoding = new UTF32Encoding(true, false);
							break;
						case "utf-32 bom":
							resEncoding = new UTF32Encoding(false, true);
							break;
						case "utf-32be bom":
							resEncoding = new UTF32Encoding(true, true);
							break;
						case "utf-16be bom":
							resEncoding = Encoding.BigEndianUnicode;
							break;
						case "utf-16le bom":
							resEncoding = Encoding.Unicode;
							break;
						default:
							resEncoding = Encoding.GetEncoding(enc);
							break;
					}
					return resEncoding;
				}
			}
			catch (Exception ex)
			{
				if (exceptionOnError)
					throw ex;
				else
					return new UTF8Encoding(); //Do not use Encoding.UTF8 = new UTF8Encoding(true);
			}
		}


		public static string NormalizeKey(string key)
		{
			return StringUtil.RTrim(StringUtil.Upper(key));
		}

		public static short SetWrkSt(string value, IGxContext cntxt)
		{
			if (cntxt != null)
			{
				cntxt.SetProperty(GxDefaultProps.WORKSTATION, value);
				return 1;
			}
			else
			{
				return 0;
			}
		}

		public static void Msg(string s)
		{

		}
		public static short Sleep(int seconds)
		{
			if (seconds < 0)
				return 0;
			Thread.Sleep(seconds * 1000);
			return 0;
		}

		public static int OpenDocument(string commandString)
		{
			return Shell(commandString, 0);
		}

		public static short OpenPrintDocument(string commandString)
		{
#if !NETCORE
			if (GXProcessHelper.ProcessFactory != null)
			{
				return GXProcessHelper.ProcessFactory.GetProcessHelper().OpenPrintDocument(commandString);
			}
			else
			{
				return 0;
			}
#else
            return 0;
#endif
		}
		public static int Shell(string commandString, int modal)
		{
#if !NETCORE
			if (GXProcessHelper.ProcessFactory != null)
			{
				return GXProcessHelper.ProcessFactory.GetProcessHelper().Shell(commandString, modal);
			}
			else
				return 0;
#else
            return new GxProcess().Shell(commandString, modal);
#endif
		}

		public static string ReadRegKey(string path)
		{
			string[] splitPath = path.Split('\\');
			int pathItems = splitPath.Length;
			if (pathItems < 2)
				return "";
			string partialPath = splitPath[0];
			for (int i = 1; i < pathItems - 1; i++)
				partialPath += "\\" + splitPath[i];

			RegistryKey rKey = findRegKey(partialPath);
			if (rKey != null)
			{
				object oRet = rKey.GetValue(splitPath[pathItems - 1]);
				if (oRet != null)
					return oRet.ToString();
			}
			return "";
		}
		static RegistryKey findRegKey(string path)
		{
			string[] splitPath = path.Split('\\');
			int pathItems = splitPath.Length;
			if (pathItems < 2)
				return null;
			RegistryKey baseKey;
			switch (splitPath[0].ToUpper())
			{
				case "HKEY_CLASSES_ROOT":
					baseKey = Registry.ClassesRoot;
					break;
				case "HKEY_CURRENT_CONFIG":
					baseKey = Registry.CurrentConfig;
					break;
				case "HKEY_PERFORMANCE_DATA":
					baseKey = Registry.PerformanceData;
					break;
#if !NETCORE
				case "HKEY_DYN_DATA":
					baseKey = Registry.DynData;
					break;
#endif
				case "HKEY_CURRENT_USER":
					baseKey = Registry.CurrentUser;
					break;
				case "HKEY_LOCAL_MACHINE":
					baseKey = Registry.LocalMachine;
					break;
				case "HKEY_USERS":
					baseKey = Registry.Users;
					break;
				default:
					return null;
			}
			RegistryKey subKey;
			int i = 2;
			for (subKey = baseKey.OpenSubKey(splitPath[1]);
				(i < pathItems && subKey != null);
				subKey = subKey.OpenSubKey(splitPath[i++])) ;
			return subKey;
		}
		public static short WriteRegKey(string path, string value)
		{
			GXLogging.Debug(log, "WriteRegKey path:" + path + ",value:" + value);
			try
			{
				string[] splitPath = path.Split('\\');
				int pathItems = splitPath.Length;
				if (pathItems < 2)
					return 1;
				RegistryKey baseKey;
				switch (splitPath[0].ToUpper())
				{
					case "HKEY_CLASSES_ROOT":
						baseKey = Registry.ClassesRoot;
						break;
					case "HKEY_CURRENT_CONFIG":
						baseKey = Registry.CurrentConfig;
						break;
					case "HKEY_PERFORMANCE_DATA":
						baseKey = Registry.PerformanceData;
						break;
#if !NETCORE
					case "HKEY_DYN_DATA":
						baseKey = Registry.DynData;
						break;
#endif
					case "HKEY_CURRENT_USER":
						baseKey = Registry.CurrentUser;
						break;
					case "HKEY_LOCAL_MACHINE":
						baseKey = Registry.LocalMachine;
						break;
					case "HKEY_USERS":
						baseKey = Registry.Users;
						break;
					default:
						return 1;
				}
				RegistryKey subKey = null, parentKey = baseKey;
				for (int i = 1; i <= pathItems - 2; i++)
				{
					subKey = parentKey.OpenSubKey(splitPath[i], RegistryKeyPermissionCheck.ReadWriteSubTree);
					if (subKey == null)
					{
						subKey = parentKey.CreateSubKey(splitPath[i]);
					}
					parentKey = subKey;
				}
				if (subKey == null)
					return 1;
				subKey.SetValue(splitPath[pathItems - 1], value);
				return 0;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "WriteRegKey error", ex);
				return 1;
			}
		}


#if !NETCORE
		public static string RUN_X86 = "Runx86.exe";
		public static string REOR = "Reor.exe";
#pragma warning disable SCS0001 // Possible command injection
		public static bool RunAsX86(string executable, string[] args, DataReceivedEventHandler dataReceived, out int exitCode)
		{
			exitCode = 0;
			try
			{
				bool RunningOn64x = IntPtr.Size == 8;

				if (RunningOn64x)
				{
					string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					string RunAsX86Path = Path.Combine(basePath, RUN_X86);
					GXLogging.Debug(log, RunAsX86Path);
					if (File.Exists(RunAsX86Path))
					{
						if (GXProcessHelper.ProcessFactory != null)
						{
							exitCode = GXProcessHelper.ProcessFactory.GetProcessHelper().ExecProcess(RunAsX86Path, args, basePath, executable, dataReceived);
							return true;
						}
						else
						{
							return false;
						}
					}
					else
					{
						if (dataReceived != null)
							dataReceived($"{RunAsX86Path} does not exists.", null);
						return false;
					}
				}
				else
				{

					return false;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "RunAsX86", ex);
				return false;
			}
		}
#pragma warning restore SCS0001 // Possible command injection
#endif
		public static bool IsBadImageFormatException(Exception ex)
		{
			FileNotFoundException fileNotFound = ex as FileNotFoundException;
			Exception e = ex;
			while (e != null && !(e is BadImageFormatException)
				&& !(e is DllNotFoundException) //System.DllNotFoundException p.e. there is a libmysql 32x. Libmysql 64x does not exist.
				&& !(e is TypeInitializationException)//System.TypeInitializationException p.e. there is oracle for 32x in GAC. Oracle x64 does not exist.
				&& !(fileNotFound != null && fileNotFound.FileName != null && fileNotFound.FileName.Equals("Oracle.DataAccess", StringComparison.OrdinalIgnoreCase)))
			{
				e = e.InnerException;
			}
			GXLogging.Debug(log, "IsBadImageFormatException " + (e != null));

			if (e == null)
			{
				GXLogging.Debug(log, ex.Message);
				GXLogging.Debug(log, ex.StackTrace);
				if (ex.InnerException != null)
				{
					GXLogging.Debug(log, "Inner:" + ex.InnerException.Message);
					GXLogging.Debug(log, ex.InnerException.StackTrace);
				}
			}
			return (e != null);
		}
		public static int HandleException(Exception ex, string main, String[] args)
		{
			SaveToEventLog(main, ex);
			GXLogging.Debug(log, "HandleException", ex);
#if !NETCORE
			int exitCode;
			if (IsBadImageFormatException(ex) && !ExecutingRunX86())
			{
				RunAsX86(string.Format("{0}.exe", main), args, proc_DataReceived, out exitCode);
				return exitCode;
			}
			else
#endif
			{
				return 1;
			}
		}
#if !NETCORE

		public static bool ExecutingRunX86()
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			if (!Environment.Is64BitProcess && assembly != null)
			{
				string executingAssembly = new FileInfo(assembly.Location).Name;
				return executingAssembly.Equals(GXUtil.RUN_X86, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				return false;
			}
		}
#endif

		internal static void proc_DataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e != null && e.Data != null)
			{
				Console.WriteLine(e.Data);
				GXLogging.Debug(log, "proc_DataReceived", e.Data);
			}
			else if (sender != null)
			{
				string senderStr = sender as string;
				if (senderStr != null)
				{
					Console.WriteLine(senderStr);
					GXLogging.Debug(log, "proc_DataReceived", senderStr);
				}
			}
		}
#if !NETCORE
		public static void SaveToEventLog(string appName, string message)
		{
			try
			{
				GXLogging.Debug(log, "Start SaveToEventLog, Parameters: appName '" + appName + "',_message '" + message + "'");

				if (!EventLog.SourceExists(appName))
				{
					GXLogging.Debug(log, "EventLog SourceExists: false");
					EventLog.CreateEventSource(appName, "Application");
				}
				else
				{
					GXLogging.Debug(log, "EventLog SourceExists: true");
				}

				EventLog MyLog = new EventLog();
				MyLog.Source = appName;
				GXLogging.Debug(log, "MachineName '" + MyLog.MachineName + "',log '" + MyLog.Log + "', logName '" + MyLog.LogDisplayName);
				MyLog.WriteEntry(message, EventLogEntryType.Error);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "SaveToEventLog Error ", e);
				Console.WriteLine("Could not save error to EventLog ({0}).{1}:{2}", e.Message, appName, message);
			}
		}

		public static void SaveToEventLog(string appName, Exception e)
		{

			SaveToEventLog(appName, e.Message + StringUtil.NewLine() + "in: " +
				GXUtil.CurrentDomainName() + StringUtil.NewLine() + "Stack Trace:" +
				StringUtil.NewLine() + e.StackTrace);
		}
#else
        public static void SaveToEventLog(string appName, Exception e)
        {
            string msg = e.Message;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                msg += "\n" + e.Message;
            }
            GXLogging.Error(log, "SaveToEventLog:" + appName + " " + e.GetType().ToString() + " " + msg);
        }

#endif

		public static string CurrentDomainName()
		{
			return AppDomain.CurrentDomain.FriendlyName;
		}

		public static decimal Calculate(String expression, String vars, out short err, out string errMsg, IGxContext context)
		{
			return ExpressionEvaluator.eval(context, context.handle, expression, out err, out errMsg, vars);
		}



#if !NETCORE
		public static string ProcessorDependantAssembly(string assemblyFile)
		{
			if (Is64X())
				return Path.Combine(FileUtil.GetStartupDirectory(), @"x64\" + assemblyFile);
			else
				return Path.Combine(FileUtil.GetStartupDirectory(), @"x86\" + assemblyFile);
		}

		static bool Is64X()
		{
			return (System.IntPtr.Size == 8);
		}
		public static bool CheckMD5(string hsh, string s)
		{

			GXLogging.Debug(log, "CheckMD5: in_hsh = " + hsh);
			GXLogging.Debug(log, "CheckMD5: in_str = " + s);
			string newHsh = GetMD5Hash(s);
			GXLogging.Debug(log, "CheckMD5: check_hsh = " + newHsh);
			GXLogging.Debug(log, "CheckMD5: check_result = " + (hsh == newHsh).ToString());
			return hsh == newHsh;
		}
#endif
		public static string GetEncryptedHash(string value, string key)
		{
			return Crypto.Encrypt64(GetHash(WebSecurityHelper.StripInvalidChars(value), Cryptography.Constants.SecurityHashAlgorithm), key);
		}

		public static bool CheckEncryptedHash(string value, string hash, string key)
		{
			return GetHash(WebSecurityHelper.StripInvalidChars(value), Cryptography.Constants.SecurityHashAlgorithm) == Crypto.Decrypt64(hash, key);
		}

		[Obsolete("GetMD5Hash is deprecated for security reasons, please use GetHash instead.", false)]
		public static string GetMD5Hash(string s)  //MD5 is NOT FIPS-compilant
		{
			return GetHash(s, "MD5");
		}

		public static string GetHash(string s)
		{
			return GetHash(s, Constants.DefaultHashAlgorithm);
		}

		public static string GetHash(string s, string hashAlgorithm)
		{
			return GXHashing.ComputeHash(s, hashAlgorithm);
		}

		public static bool CheckFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages)
		{
			if (file == null || !file.Exists())
			{
				GXUtil.ErrorToMessages("Invalid File", "File is null or does not exist", Messages);
				return false;
			}
			else
			{
				return true;
			}
		}

		public static void ErrorToMessages(string errorId, string errorDescription, GXBaseCollection<SdtMessages_Message> Messages)
		{
			if (Messages != null)
			{
				SdtMessages_Message msg = new SdtMessages_Message();
				msg.gxTpr_Description = errorDescription;
				msg.gxTpr_Id = errorId;
				msg.gxTpr_Type = 1;
				Messages.Add(msg);
			}
		}
		public static void ErrorToMessages(string errorId, Exception ex, GXBaseCollection<SdtMessages_Message> Messages)
		{
			if (Messages != null && ex != null)
			{
				StringBuilder str = new StringBuilder();
				str.Append(ex.Message);
				while (ex.InnerException != null)
				{
					str.Append(ex.InnerException.Message);
					ex = ex.InnerException;
				}
				ErrorToMessages(errorId, str.ToString(), Messages);
			}
		}

		public static string PagingSelect(string select)
		{
			string pagingSelect = StringUtil.LTrim(select);

			if (pagingSelect.StartsWith("DISTINCT"))
				pagingSelect = pagingSelect.Substring(9);

			pagingSelect = Regex.Replace(pagingSelect, @"T\d+\.", "GX_ICTE.");
			return pagingSelect;
		}

		public static bool Compare(IComparable operand1, string op, IComparable operand2)
		{
			int compareValue;
			if (operand1 is string sop1 && operand2 is string sop2)
			{
				sop1 = sop1.TrimEnd();
				sop2 = sop2.TrimEnd();
				if (op.Equals("like"))
					return StringUtil.Like(sop1, StringUtil.PadR(sop2, sop1.Length, "%"));
				else compareValue = String.CompareOrdinal(sop1, sop2);
			}
			else if (operand1.GetType() == operand2.GetType())
				compareValue = operand1.CompareTo(operand2);
			else compareValue = Convert.ToDecimal(operand1).CompareTo(Convert.ToDecimal(operand2));
			switch (op)
			{
				case "=": return compareValue == 0;
				case ">": return compareValue > 0;
				case ">=": return compareValue >= 0;
				case "<": return compareValue < 0;
				case "<=": return compareValue <= 0;
				case "<>": return compareValue != 0;
				default: goto case "=";
			}
		}
	}
#if !NETCORE
	public class IniFile
	{
		public string path;

		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section,
				 string key, string def, StringBuilder retVal,
			int size, string filePath);

		public IniFile(string INIPath)
		{
			path = INIPath;
		}

		public string IniReadValue(string Section, string Key)
		{
			StringBuilder temp = new StringBuilder(255);
			GetPrivateProfileString(Section, Key, "", temp,
											255, this.path);
			return temp.ToString();
		}
	}
#endif
	public static class GXDbFile
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.GXDbFile));

		private static Regex schemeRegex = new Regex("^" + Scheme + ":", RegexOptions.Compiled);

		public static string Scheme
		{
			get
			{
				return "gxdbfile";
			}
		}

		public static string MultimediaDirectory
		{
			get
			{
				return "multimedia";
			}
		}

		public static string GetFileName(string uri)
		{
			try
			{
				return FileUtil.GetFileName(uri);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Bad File URI " + uri, e);
				return uri;
			}
		}

		public static string GetFileType(string uri)
		{
			try
			{
				return FileUtil.GetFileType(uri);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Bad File URI " + uri, e);
				return uri;
			}
		}

		public static string AddTokenToFileName(string name, string type)
		{
			if (String.IsNullOrEmpty(name))
				return "";
			return String.Format("{0}_{1}{2}{3}", name, Guid.NewGuid().ToString("N"), String.IsNullOrEmpty(type) ? "" : ".", type);
		}

		public static string RemoveTokenFromFileName(string fileName)
		{
			if (String.IsNullOrEmpty(fileName))
				return "";

			string name = GetFileName(fileName);
			string type = GetFileType(fileName);
			string cleanName = name;
			int sepIdx = name.LastIndexOf('_');
			if (sepIdx > 0)
			{
				if (Regex.IsMatch(name.Substring(sepIdx), "(_[0-9a-zA-Z]{32})$", RegexOptions.RightToLeft))
					cleanName = name.Substring(0, sepIdx);
			}

			return (String.IsNullOrEmpty(type)) ? cleanName : String.Format("{0}.{1}", cleanName, type);
		}

		public static bool HasToken(string fileName)
		{
			return Path.GetFileName(fileName) != GXDbFile.RemoveTokenFromFileName(Path.GetFileName(fileName));
		}

		public static string GetFileNameFromUri(string uriString)
		{
			if (String.IsNullOrEmpty(uriString))
				return "";

			if (schemeRegex.IsMatch(uriString))
				return schemeRegex.Replace(uriString, "");

			return uriString;
		}

		public static string ResolveUri(string uriString, IGxContext context = null)
		{
			return ResolveUri(uriString, true, context);
		}

		public static string ResolveUri(string uriString, bool absUrl, IGxContext context = null)
		{
			if (String.IsNullOrEmpty(uriString))
				return string.Empty;

			string providerObjectName;
			if (PathUtil.IsAbsoluteUrl(uriString) && StorageFactory.TryGetProviderObjectName(ServiceFactory.GetExternalProvider(), uriString, out providerObjectName))
			{
				return new GxFile(string.Empty, providerObjectName, GxFileType.DefaultAttribute).GetURI();
			}

			if (schemeRegex.IsMatch(uriString))
			{
				string fileName = schemeRegex.Replace(uriString, "");
				//Same way as getBlobFile, creates a GxFile in order to take into account external storage when enabled.
				string basePath = Path.Combine(Path.Combine(Preferences.getBLOB_PATH(), MultimediaDirectory));
				try
				{
					GxFile file = new GxFile(string.Empty, PathUtil.SafeCombine(basePath, fileName), GxFileType.PrivateAttribute);
					return PathToUrl(file.GetURI(), absUrl, context);
				}
				catch (ArgumentException ex)
				{
					GXLogging.Warn(log, ex, "Invalid Characters in path", uriString);
				}
			}
			return uriString;
		}

		public static string GenerateUri(string file, bool addToken, bool addScheme)
		{
			string name = GetFileName(file);
			string type = GetFileType(file);

			string scheme = addScheme ? $"{GXDbFile.Scheme}:" : string.Empty;

			string typeExt = string.IsNullOrEmpty(type) ? string.Empty : $".{type}";
			string nameAndType = addToken ? GXDbFile.AddTokenToFileName(name, type) : $"{name}{typeExt}";

			return $"{scheme}{nameAndType}";
		}

		public static string GetNameFromURL(string uri)
		{
			return GetFileName(uri) + "." + GetFileType(uri);
		}

		public static bool IsFileExternal(string uriString)
		{
			if (String.IsNullOrEmpty(uriString))
				return true;

			if (schemeRegex.IsMatch(uriString))
				return false;

			return true;
		}

		private static string GetUriFromFile(string name, string type)
		{
			string cleanName = name.Trim();
			string cleanType = type.Trim();
			return (String.IsNullOrEmpty(cleanName) ? "multimedia-file" : cleanName) + "." + (String.IsNullOrEmpty(cleanType) ? "tmp" : cleanType);
		}

		public static string GetUriFromFile(string name, string type, string path)
		{
			if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(type) && !String.IsNullOrEmpty(path))
			{
				if (GxUploadHelper.IsUpload(path))
				{
					return new GxFile(string.Empty, path, GxFileType.PrivateAttribute).GetName();
				}
				string fromPathType = Path.GetExtension(path);
				if (!String.IsNullOrEmpty(fromPathType) && fromPathType != "tmp")
				{
					return Path.GetFileName(path);
				}
			}
			return GetUriFromFile(name, type);
		}

		public static string GetUrlFromUri(string blob, string uri)
		{
			return String.IsNullOrEmpty(blob) ? uri : "";
		}

		public static string PathToUrl(string path, IGxContext context = null)
		{
			return PathToUrl(path, true, context);
		}
		public static string PathToUrl(string path, bool absUrl, IGxContext context = null)
		{
			Uri pathUri;
			// Absolute URLs
			if (Uri.TryCreate(path, UriKind.Absolute, out pathUri) && (pathUri.Scheme == GXUri.UriSchemeHttp || pathUri.Scheme == GXUri.UriSchemeHttps))
				return path;

			if (GxContext.Current == null || GxContext.Current.HttpContext == null)
			{
				// Absolute file system paths
				if (pathUri != null && pathUri.Scheme == GXUri.UriSchemeFile)
					return pathUri.AbsoluteUri;

				// Relative file system paths
				if (path.Contains("\\"))
				{
					pathUri = new Uri(Path.GetFullPath(path));
					return pathUri.AbsoluteUri;
				}

				// Relative URLs
				return path;
			}
			if (context != null)
				if (absUrl)
					return context.PathToUrl(path);
				else
					return context.PathToRelativeUrl(path, false);
			else
				if (absUrl)
				return GxContext.Current.PathToUrl(path);
			else
				return GxContext.Current.PathToRelativeUrl(path, false);
		}
	}

	public static class GxImageUtil
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GxImageUtil));

		private static string ImageAbsolutePath(string originalFileLocation)
		{
			return ImageFile(originalFileLocation).GetAbsoluteName();
		}
		private static GxFile ImageFile(string originalFileLocation)
		{
			return new GxFile(GxContext.StaticPhysicalPath(), originalFileLocation);
		}

		public static string Resize(string imageFile, int width, int height, bool keepAspectRatio)
		{
			try
			{
				int newheight = height;
				string originalFileLocation = ImageAbsolutePath(imageFile);
				using (Image image = Image.FromFile(ImageAbsolutePath(originalFileLocation)))
				{
					// Prevent using images internal thumbnail
					image.RotateFlip(RotateFlipType.Rotate180FlipNone);
					image.RotateFlip(RotateFlipType.Rotate180FlipNone);

					if (keepAspectRatio)
					{
						double resize = (double)image.Width / (double)width;//get the resize vector
						newheight = (int)(image.Height / resize);//  set the new heigth of the current image
					}//return the image resized to the given heigth and width
					image.GetThumbnailImage(width, newheight, null, IntPtr.Zero).Save(originalFileLocation);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Resize {imageFile} failed", ex);
			}
			return imageFile;
		}
		public static string Scale(string imageFile, int percent)
		{
			try
			{
				string originalFileLocation = ImageAbsolutePath(imageFile);
				int width, height;
				using (Image image = Image.FromFile(originalFileLocation))
				{
					width = image.Size.Width * percent / 100;
					height = image.Size.Height * percent / 100;
				}
				return Resize(imageFile, width, height, true);
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Scale {imageFile} failed", ex);
				return imageFile;
			}
		}
		public static string Crop(string imageFile, int X, int Y, int Width, int Height)
		{
			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					string originalFileLocation = ImageAbsolutePath(imageFile);
					using (Image OriginalImage = Image.FromFile(originalFileLocation))
					{
						using (Bitmap bmp = new Bitmap(Width, Height))
						{
							bmp.SetResolution(OriginalImage.HorizontalResolution, OriginalImage.VerticalResolution);
							using (Graphics Graphic = Graphics.FromImage(bmp))
							{
								Graphic.SmoothingMode = SmoothingMode.AntiAlias;
								Graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
								Graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
								Graphic.DrawImage(OriginalImage, new Rectangle(0, 0, Width, Height), X, Y, Width, Height, GraphicsUnit.Pixel);
								bmp.Save(ms, OriginalImage.RawFormat);
							}
						}
					}
					using (FileStream file = new FileStream(originalFileLocation, FileMode.Open, FileAccess.Write))
					{
						ms.WriteTo(file);
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Crop {imageFile} failed", ex);
			}
			return imageFile;
		}
		public static string Rotate(string imageFile, int angle)
		{

			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					string originalFileLocation = ImageAbsolutePath(imageFile);
					using (Image OriginalImage = Image.FromFile(originalFileLocation))
					{
						using (Bitmap rotatedImage = new Bitmap(OriginalImage.Width, OriginalImage.Height))
						{
							rotatedImage.SetResolution(OriginalImage.HorizontalResolution, OriginalImage.VerticalResolution);

							using (Graphics g = Graphics.FromImage(rotatedImage))
							{
								g.TranslateTransform(OriginalImage.Width / 2, OriginalImage.Height / 2);
								g.RotateTransform(angle);
								g.TranslateTransform(-OriginalImage.Width / 2, -OriginalImage.Height / 2);
								g.DrawImage(OriginalImage, new Point(0, 0));
							}
							rotatedImage.Save(ms, OriginalImage.RawFormat);
						}
					}
					using (FileStream file = new FileStream(originalFileLocation, FileMode.Open, FileAccess.Write))
					{
						ms.WriteTo(file);
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Rotate {imageFile} failed", ex);
			}
			return imageFile;
		}
		public static string FlipHorizontally(string imageFile) {

			try
			{
				string originalFileLocation = ImageAbsolutePath(imageFile);
				using (Bitmap bmp = new Bitmap(originalFileLocation))
				{
					bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
					bmp.Save(originalFileLocation);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Flip Horizontally {imageFile} failed", ex);
			}
			return imageFile;
		}
		public static string FlipVertically(string imageFile)
		{
			try
			{
				string originalFileLocation = ImageAbsolutePath(imageFile);
				using (Bitmap bmp = new Bitmap(originalFileLocation))
				{
					bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
					bmp.Save(originalFileLocation);
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"Flip Vertically {imageFile} failed", ex);
			}
			return imageFile;
		}

		public static int GetImageWidth(string imageFile)
		{
			try
			{
				string originalFileLocation = ImageAbsolutePath(imageFile);
				using (Bitmap bmp = new Bitmap(originalFileLocation))
				{
					return bmp.Width;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"GetImageWidth {imageFile} failed", ex);
			}
			return 0;
		}
		public static int GetImageHeight(string imageFile)
		{
			try
			{
				string originalFileLocation = ImageAbsolutePath(imageFile);
				using (Bitmap bmp = new Bitmap(originalFileLocation))
				{
					return bmp.Height;
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"GetImageHeight {imageFile} failed", ex);
			}
			return 0;
		}
		public static long GetFileSize(string imageFile)
		{
			try
			{
				return ImageFile(imageFile).GetLength();
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, $"GetFileSize {imageFile} failed", ex);
			}
			return 0;
		}
	}
	public class StorageUtils
	{
		public const string DELIMITER = "/";

		private static string ReplaceAt(string str, int index, int length, string replace)
		{
			return str.Remove(index, Math.Min(length, str.Length - index))
					.Insert(index, replace);
		}

		[Obsolete("EncodeUrl is deprecated as it would give unexpected results for some urls. Use EncodeUrlPath instead", false)]
		public static string EncodeUrl(string objectName) { 
			if (!Uri.IsWellFormedUriString(objectName, UriKind.RelativeOrAbsolute))
				return HttpUtility.UrlPathEncode(objectName);
			return objectName;
		}

		public static string EncodeUrlPath(string objectNameOrRelativeUrl)
		{
			int idx = objectNameOrRelativeUrl.LastIndexOf(StorageUtils.DELIMITER);
			if (idx > 0)
			{
				string objectName = objectNameOrRelativeUrl.Substring(idx + 1);
				return ReplaceAt(objectNameOrRelativeUrl, idx + 1, objectName.Length, Uri.EscapeDataString(objectName));
			}
			return Uri.EscapeDataString(objectNameOrRelativeUrl);
		}

		public static string DecodeUrl(string url)
		{
			return HttpUtility.UrlDecode(url);
		}

		public static String NormalizeDirectoryName(String directoryName)
		{
			directoryName = directoryName.Replace("\\", DELIMITER);
			if (!directoryName.EndsWith(DELIMITER))
				return directoryName + DELIMITER;
			return directoryName;
		}
		public static string EncodeNonAsciiCharacters(string value)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in value)
			{
				if (c > 127)
				{
					// This character is too big for ASCII
					string encodedValue = "\\u" + ((int)c).ToString("x4");
					sb.Append(encodedValue);
				}
				else
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}
	}

}