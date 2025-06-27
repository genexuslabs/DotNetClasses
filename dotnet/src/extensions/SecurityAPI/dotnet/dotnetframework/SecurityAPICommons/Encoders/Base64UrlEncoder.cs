using System;
using System.Security;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using Org.BouncyCastle.Utilities.Encoders;
using System.Text;
using log4net;
using System.Web;

namespace SecurityAPICommons.Encoders
{

	[SecuritySafeCritical]
	public class Base64UrlEncoder: SecurityAPIObject
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Base64UrlEncoder));

		[SecuritySafeCritical]
		public Base64UrlEncoder() : base()
		{

		}

		[SecuritySafeCritical]
		public string toBase64(string text)
		{
			string method = "toBase64";
			logger.Debug(method);
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
				byte[] resultBytes = UrlBase64.Encode(textBytes);
				result = Encoding.UTF8.GetString(resultBytes);
			}
			catch (Exception e)
			{
				this.error.setError("BS001", e.Message);
				logger.Error(method, e);
				return "";
			}
			return result;
		}

		[SecuritySafeCritical]
		public string toPlainText(string base64Text)
		{
			string method = "toPlainText";
			logger.Debug(method);
			this.error.cleanError();
			byte[] bytes;
			try
			{
				bytes = UrlBase64.Decode(base64Text);
			}
			catch (Exception e)
			{
				this.error.setError("BS002", e.Message);
				logger.Error(method, e);
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

		[SecuritySafeCritical]
		public string toStringHexa(string base64Text)
		{
			string method = "toStringHexa";
			logger.Debug(method);
			this.error.cleanError();
			byte[] bytes;
			try
			{
				bytes = UrlBase64.Decode(base64Text);
			}
			catch (Exception e)
			{
				this.error.setError("BS003", e.Message);
				logger.Error(method, e);
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
				logger.Error(method, e);
				return "";
			}
			return result;
		}

		[SecuritySafeCritical]
		public string fromStringHexaToBase64(string stringHexa)
		{
			string method = "fromStringHexaToBase64";
			logger.Debug(method);
			this.error.cleanError();
			byte[] stringBytes;
			try
			{
				stringBytes = Hex.Decode(stringHexa);
			}
			catch (Exception e)
			{
				this.error.setError("BS005", e.Message);
				logger.Error(method, e);
				return "";
			}
			string result = "";
			try
			{
				byte[] resultBytes = UrlBase64.Encode(stringBytes);
				result = Encoding.UTF8.GetString(resultBytes);
			}
			catch (Exception e)
			{
				this.error.setError("BS006", e.Message);
				logger.Error(method, e);
				return "";
			}
			return result;
		}

		[SecuritySafeCritical]
		public string base64ToBase64Url(string base64Text)
		{
			string method = "base64ToBase64Url";
			logger.Debug(method);
			this.error.cleanError();
			string result = "";
			try
			{
				byte[] b64bytes = Base64.Decode(base64Text);
				byte[] bytes = UrlBase64.Encode(b64bytes);
				result = Encoding.UTF8.GetString(bytes);
			}
			catch (Exception e)
			{
				this.error.setError("BS007", e.Message);
				logger.Error(method, e);
				return "";
			}
			return result;
		}

		[SecuritySafeCritical]
		public string base64UrlToBase64(string base64UrlText)
		{
			string method = "base64UrlToBase64";
			logger.Debug(method);
			this.error.cleanError();
			string result = "";
			try
			{
				byte[] b64bytes = UrlBase64.Decode(base64UrlText);
				byte[] bytes = Base64.Encode(b64bytes);
				result = Encoding.UTF8.GetString(bytes);
			}
			catch (Exception e)
			{
				this.error.setError("BS008", e.Message);
				logger.Error(method, e);
				return "";
			}
			return result;
		}
	}
}
