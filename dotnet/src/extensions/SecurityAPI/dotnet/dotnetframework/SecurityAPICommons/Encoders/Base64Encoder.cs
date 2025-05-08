using System;
using System.Security;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Utilities.Encoders;
using log4net;

namespace SecurityAPICommons.Encoders
{
	/// <summary>
	/// Base64Encoder class
	/// </summary>
	[SecuritySafeCritical]
	public class Base64Encoder : SecurityAPIObject
	{

		private static readonly ILog logger = LogManager.GetLogger(typeof(Base64Encoder));

		/// <summary>
		/// Base64Encoder constructor
		/// </summary>
		[SecuritySafeCritical]
		public Base64Encoder() : base()
		{

		}

		/// <summary>
		/// string to Base64 encoded string
		/// </summary>
		/// <param name="text">string UTF-8 plain text to encode</param>
		/// <returns>Base64 string text encoded</returns>
		[SecuritySafeCritical]
		public string toBase64(string text)
		{
			logger.Debug("toBase64");
			this.error.cleanError();
			EncodingUtil eu = new EncodingUtil();
			byte[] textBytes = eu.getBytes(text);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}
			string result = "";
			try
			{
				result = Base64.ToBase64String(textBytes);
			}
			catch (Exception e)
			{
				this.error.setError("BS001", e.Message);
				logger.Error("toBase64", e);
				return "";
			}
			return result;
		}
		/// <summary>
		/// string Base64 encoded to string plain text
		/// </summary>
		/// <param name="base64Text">string Base64 encoded</param>
		/// <returns>string UTF-8 plain text from Base64</returns>
		[SecuritySafeCritical]
		public string toPlainText(string base64Text)
		{
			logger.Debug("toPlainText");
			this.error.cleanError();
			byte[] bytes;
			try
			{
				bytes = Base64.Decode(base64Text);
			}
			catch (Exception e)
			{
				this.error.setError("BS002", e.Message);
				logger.Error("toPlainText", e);
				return "";
			}
			EncodingUtil eu = new EncodingUtil();
			string result = eu.getString(bytes);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}
			return result;
		}
		/// <summary>
		/// string Base64 encoded text to string hexadecimal encoded text
		/// </summary>
		/// <param name="base64Text">string Base64 encoded</param>
		/// <returns>string Hexa representation of base64Text</returns>
		[SecuritySafeCritical]
		public string toStringHexa(string base64Text)
		{
			logger.Debug("toStringHexa");
			this.error.cleanError();
			byte[] bytes;
			try
			{
				bytes = Base64.Decode(base64Text);
			}
			catch (Exception e)
			{
				this.error.setError("BS003", e.Message);
				logger.Error("toStringHexa", e);
				return "";
			}
			string result = "";
			try
			{
				result = Hex.ToHexString(bytes).ToUpper();
			}
			catch (Exception e)
			{
				this.error.setError("BS004", e.Message);
				logger.Error("toStringHexa", e);
				return "";
			}
			return result;

		}
		/// <summary>
		/// string hexadecimal encoded text to string Base64 encoded text
		/// </summary>
		/// <param name="stringHexa">string Hexa</param>
		/// <returns>string Base64 encoded of stringHexa</returns>
		[SecuritySafeCritical]
		public string fromStringHexaToBase64(string stringHexa)
		{
			logger.Debug("fromStringHexaToBase64");
			this.error.cleanError();
			byte[] stringBytes;
			try
			{
				stringBytes = Hex.Decode(stringHexa);
			}
			catch (Exception e)
			{
				this.error.setError("BS005", e.Message);
				logger.Error("fromStringHexaToBase64", e);
				return "";
			}
			string result = "";
			try
			{
				result = Base64.ToBase64String(stringBytes);
			}
			catch (Exception e)
			{
				this.error.setError("BS006", e.Message);
				logger.Error("fromStringHexaToBase64", e);
				return "";
			}
			return result;
		}
	}
}
