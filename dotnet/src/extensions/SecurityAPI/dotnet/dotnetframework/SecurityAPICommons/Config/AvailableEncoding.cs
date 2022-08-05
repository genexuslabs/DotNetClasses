using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using SecurityAPICommons.Commons;

namespace SecurityAPICommons.Config
{
    [SecuritySafeCritical]
    public enum AvailableEncoding
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
		NONE, UTF_8, UTF_16, UTF_16BE, UTF_16LE, UTF_32, UTF_32BE, UTF_32LE, SJIS, GB2312
#pragma warning restore CA1707 // Identifiers should not contain underscores
	}

    [SecuritySafeCritical]
    public static class AvailableEncodingUtils
    {
        public static AvailableEncoding getAvailableEncoding(string encoding, Error error)
        {
			if(error == null) return AvailableEncoding.NONE;
			if (encoding == null)
			{
				error.setError("AE001", "Unknown encoding or not available");
				return AvailableEncoding.NONE;
			}
            encoding = encoding.Replace("-", "_");
            encoding = encoding.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            switch (encoding.Trim())
            {
                case "UTF_8":
                    return AvailableEncoding.UTF_8;
                case "UTF_16":
                    return AvailableEncoding.UTF_16;
                case "UTF_16BE":
                    return AvailableEncoding.UTF_16BE;
                case "UTF_16LE":
                    return AvailableEncoding.UTF_16LE;
                case "UTF_32":
                    return AvailableEncoding.UTF_32;
                case "UTF_32BE":
                    return AvailableEncoding.UTF_32BE;
                case "UTF_32LE":
                    return AvailableEncoding.UTF_32LE;
                case "SJIS":
                    return AvailableEncoding.SJIS;
                case "GB2312":
                    return AvailableEncoding.GB2312;
                default:
                    error.setError("AE001", "Unknown encoding or not available");
                    return AvailableEncoding.NONE;
            }
        }

        public static bool existsEncoding(string encoding)
        {
			if(encoding == null) return  false;
            encoding = encoding.Replace("-", "_");
            encoding = encoding.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            switch (encoding)
            {
                case "UTF_8":
                case "UTF_16":
                case "UTF_16BE":
                case "UTF_16LE":
                case "UTF_32":
                case "UTF_32BE":
                case "UTF_32LE":
                case "SJIS":
                case "GB2312":
                    return true;
                default:
                    return false;
            }
        }

        public static string valueOf(AvailableEncoding availableEncoding)
        {
            switch (availableEncoding)
            {
                case AvailableEncoding.UTF_8:
                    return "UTF-8";
                case AvailableEncoding.UTF_16:
                    return "UTF-16";
                case AvailableEncoding.UTF_16BE:
                    return "UTF-16BE";
                case AvailableEncoding.UTF_16LE:
                    return "UTF-16LE";
                case AvailableEncoding.UTF_32:
                    return "UTF-32";
                case AvailableEncoding.UTF_32BE:
                    return "UTF-32BE";
                case AvailableEncoding.UTF_32LE:
                    return "UTF-32LE";
                case AvailableEncoding.SJIS:
                    return "Shift_JIS";
                case AvailableEncoding.GB2312:
                    return "GB2312";
                default:
                    return "";
            }
        }

        public static string encapsulateGetString(byte[] input, AvailableEncoding availableEncoding, Error error)
        {
			if (error == null) return "";
            const string strUniRepChr = "�"; //Unicode Character 'REPLACEMENT CHARACTER' (U+FFFD)
            switch (availableEncoding)
            {
                case AvailableEncoding.UTF_8:
                    return Encoding.UTF8.GetString(input);
#if NETCORE
                case AvailableEncoding.UTF_16:
                    Encoding utf16 = Encoding.GetEncoding(1201, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    string utf16_string = utf16.GetString(input);
                    return utf16_string.Replace(strUniRepChr, String.Empty);

                case AvailableEncoding.UTF_16BE:
                    Encoding utf16_be = Encoding.GetEncoding(1201, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    string utf16_beString = utf16_be.GetString(input);
                    return utf16_beString.Replace(strUniRepChr, String.Empty);

                case AvailableEncoding.UTF_16LE:
                    Encoding utf16_le = Encoding.GetEncoding(1200, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    string utf16_leString = utf16_le.GetString(input);
                    return utf16_leString.Replace(strUniRepChr, String.Empty);

#else
                case AvailableEncoding.UTF_16:
                    return Encoding.BigEndianUnicode.GetString(input);

                case AvailableEncoding.UTF_16BE:
                    return Encoding.BigEndianUnicode.GetString(input);

                case AvailableEncoding.UTF_16LE:
                    return Encoding.Unicode.GetString(input);
#endif
                case AvailableEncoding.UTF_32:



                    Encoding cpUTF32_1 = Encoding.GetEncoding(12001, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    string cpUTF32_1String = cpUTF32_1.GetString(input);
                    return cpUTF32_1String.Replace(strUniRepChr, String.Empty);
                //return cpUTF32_1String.Remove(cpUTF32_1String.Length - 1);



                case AvailableEncoding.UTF_32BE:




                    Encoding cpUTF32_2 = Encoding.GetEncoding(12001, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    string cpUTF32_2String = cpUTF32_2.GetString(input);
                    return cpUTF32_2String.Replace(strUniRepChr, String.Empty);
                case AvailableEncoding.UTF_32LE:

                    Encoding cpUTF32_3 = Encoding.GetEncoding(12000, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    string cpUTF32_3String = cpUTF32_3.GetString(input);
                    return cpUTF32_3String.Replace(strUniRepChr, String.Empty);


                case AvailableEncoding.SJIS:
#if NETCORE
                    Encoding sjis = CodePagesEncodingProvider.Instance.GetEncoding(932);
                    return sjis.GetString(input);

#else

                    Encoding sjis = Encoding.GetEncoding(932);
                    return sjis.GetString(input);
#endif

                case AvailableEncoding.GB2312:
#if NETCORE
                    Encoding gb2312 = CodePagesEncodingProvider.Instance.GetEncoding(936, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return gb2312.GetString(input);
#else
                    Encoding gb2312 = Encoding.GetEncoding(936, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return gb2312.GetString(input);
#endif


                default:
                    error.setError("AE001", "Unknown encoding");
                    return "";
            }
        }

        public static byte[] encapsulateeGetBytes(string input, AvailableEncoding availableEncoding, Error error)
        {
			if (error == null) return null;
            const string strUniRepChr = "�"; //Unicode Character 'REPLACEMENT CHARACTER' (U+FFFD)
            switch (availableEncoding)
            {
                case AvailableEncoding.UTF_8:
                    return Encoding.UTF8.GetBytes(input);
#if NETCORE
                case AvailableEncoding.UTF_16:
                    Encoding utf16 = Encoding.GetEncoding(1201, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return utf16.GetBytes(input);

                case AvailableEncoding.UTF_16BE:
                    Encoding utf16_be = Encoding.GetEncoding(1201, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return utf16_be.GetBytes(input);

                case AvailableEncoding.UTF_16LE:
                    Encoding utf16_le = Encoding.GetEncoding(1200, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return utf16_le.GetBytes(input);

#else
                case AvailableEncoding.UTF_16:
                    return Encoding.BigEndianUnicode.GetBytes(input);

                case AvailableEncoding.UTF_16BE:
                    return Encoding.BigEndianUnicode.GetBytes(input);

                case AvailableEncoding.UTF_16LE:
                    return Encoding.Unicode.GetBytes(input);
#endif
                case AvailableEncoding.UTF_32:

                    Encoding cpUTF32_1 = Encoding.GetEncoding(12001, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return cpUTF32_1.GetBytes(input);

                case AvailableEncoding.UTF_32BE:

                    Encoding cpUTF32_2 = Encoding.GetEncoding(12001, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return cpUTF32_2.GetBytes(input);

                case AvailableEncoding.UTF_32LE:
                    Encoding cpUTF32_3 = Encoding.GetEncoding(12000, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return cpUTF32_3.GetBytes(input);

                case AvailableEncoding.SJIS:
#if NETCORE
                    Encoding sjis = CodePagesEncodingProvider.Instance.GetEncoding(932);
                    return sjis.GetBytes(input);
#else
                    Encoding sjis = Encoding.GetEncoding(932);
                    return sjis.GetBytes(input);
#endif

                case AvailableEncoding.GB2312:
#if NETCORE
                    Encoding gb2312 = CodePagesEncodingProvider.Instance.GetEncoding(936, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return gb2312.GetBytes(input);
#else
                    Encoding gb2312 = Encoding.GetEncoding(936, new EncoderReplacementFallback(strUniRepChr), new DecoderReplacementFallback(strUniRepChr));
                    return gb2312.GetBytes(input);
#endif

                default:
                    error.setError("AE001", "Unknown encoding");
                    return null;
            }
        }

    }
}
