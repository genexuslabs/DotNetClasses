using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Security;


namespace SecurityAPICommons.Encoders
{
	/// <summary>
	/// Implements hexadecimal encoding and decoding functions
	/// </summary>
	[SecuritySafeCritical]
	public class HexaEncoder : SecurityAPIObject
	{


		/// <summary>
		/// Hexa class contructor
		/// </summary>

		[SecuritySafeCritical]
		public HexaEncoder() : base()
		{

		}

		/// <summary>
		/// string Hexadecimal encoded representation of UTF-8 input plain text
		/// </summary>
		/// <param name="plainText">string UTF-8 plain text</param>
		/// <returns>string Hexa hexadecimal representation of plainText</returns>
		[SecuritySafeCritical]
		public string toHexa(string plainText)
		{
			this.error.cleanError();
			EncodingUtil eu = new EncodingUtil();
			byte[] stringBytes = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}
			string hexa = "";
			try
			{
				hexa = Hex.ToHexString(stringBytes, 0, stringBytes.Length);
			}
			catch (Exception e)
			{
				this.error.setError("HE001", e.Message);
				return "";
			}
			return hexa.ToUpper();

		}
		/// <summary>
		/// string UTF-8 representation of the STring hexadecimal encoded text
		/// </summary>
		/// <param name="stringHexa">string hexadecimal representation of a text</param>
		/// <returns>string UTF-8 plain text from stringHexa</returns>
		[SecuritySafeCritical]
		public string fromHexa(string stringHexa)
		{
			this.error.cleanError();
			byte[] resBytes;
			try
			{
				resBytes = Hex.Decode(fixString(stringHexa));
			}
			catch (Exception e)
			{
				this.error.setError("HE002", e.Message);
				return "";
			}
			EncodingUtil eu = new EncodingUtil();
			String result = eu.getString(resBytes);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}
			return result;
		}

		[SecuritySafeCritical]
		public bool isHexa(string input)
		{
			this.error.cleanError();
			try
			{
				Hex.Decode(fixString(input));
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public static string fixString(String input)
		{
			if (!input.Contains("-"))
			{
				return input;
			}
			else
			{
				string inputStr = input.Replace("-", "");
				return inputStr;
			}
		}
	}
}