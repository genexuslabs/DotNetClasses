using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Encryption;
using GeneXus.Utils;
using Jose;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using static GeneXus.Web.Security.SecureTokenHelper;

namespace GeneXus.Web.Security
{
	[SecuritySafeCritical]
	public static class WebSecurityHelper
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(GeneXus.Web.Security.WebSecurityHelper));
    		const int SecretKeyMinimumLength = 16;
        public static string StripInvalidChars(string input)
        {
			if (string.IsNullOrEmpty(input))
				return input;
            var output = new string(input.Where(c => !char.IsControl(c)).ToArray());
            return output.Trim();
        }

        public static string Sign(string pgmName, string issuer, string value, SecurityMode mode, IGxContext context)
        {            
            return SecureTokenHelper.Sign(new WebSecureToken { ProgramName = pgmName, Issuer = issuer, Value = string.IsNullOrEmpty(value) ? string.Empty: StripInvalidChars(value) }, mode, GetSecretKey(context));
        }

        private static string GetSecretKey(IGxContext context)
        {
			string hashSalt = string.Empty;
			Config.GetValueOf("VER_STAMP", out hashSalt); //Some random SALT that is different in every GX App installation. Better if changes over time
			string secretKey = GXUtil.GetEncryptionKey(context, string.Empty) + hashSalt;
			if (secretKey.Length < SecretKeyMinimumLength)
				return StringUtil.PadL(secretKey, SecretKeyMinimumLength, '0');
			else
				return secretKey;
        }

        public static bool Verify(string pgmName, string issuer, string value, string jwtToken, IGxContext context)
        {
            WebSecureToken token;
            return WebSecurityHelper.Verify(pgmName, issuer, value, jwtToken, out token, context);
        }
        public static bool Verify(string pgmName, string issuer, string value, string jwtToken, out WebSecureToken token, IGxContext context)
        {
            token = new WebSecureToken();
            bool ok = SecureTokenHelper.Verify(jwtToken, token, GetSecretKey(context));			
            bool ret = ok && !string.IsNullOrEmpty(pgmName) && token.ProgramName == pgmName && issuer == token.Issuer &&
                StripInvalidChars(token.Value) == StripInvalidChars(value) && token.Expiration >= DateTime.Now;

            if (!ret)
            {

                if (!ok)
                {
                    GXLogging.Error(_log, "verify: Invalid token");
                }
                if (token.ProgramName != pgmName)
                {
                    GXLogging.Error(_log, "verify: pgmName mismatch " + "'" + token.ProgramName + "' <> '" + pgmName + "'");
                }
                if (issuer != token.Issuer)
                {
                    GXLogging.Error(_log, "verify: issuer mismatch " + "'" + token.Issuer + "' <> '" + issuer + "'");
                }
                if (StripInvalidChars(token.Value) != StripInvalidChars(value))
                {
                    GXLogging.Error(_log, "verify: value mismatch " + "'" + token.Value + "'" + " <> '" + value + "'");
                }
                if (token.Expiration < DateTime.Now)
                {
                    GXLogging.Error(_log, "verify: token expired ");
                }
            }
            return ret;
        }

		internal static bool VerifySecureSignedSDTToken(string cmpCtx, IGxCollection value, string signedToken, IGxContext context)
		{
			WebSecureToken Token = SecureTokenHelper.getWebSecureToken(signedToken, GetSecretKey(context));
			if (Token == null)
				return false;
			IGxCollection PayloadObject = (IGxCollection)value.Clone();
			PayloadObject.FromJSonString(Token.Value);
			return GxUserType.IsEqual(value, PayloadObject);
		}

		internal static bool VerifySecureSignedSDTToken(string cmpCtx, GxUserType value, string signedToken, IGxContext context)
		{
			WebSecureToken Token = SecureTokenHelper.getWebSecureToken(signedToken, GetSecretKey(context));
			if (Token == null)
				return false;
			GxUserType PayloadObject = (GxUserType)value.Clone();
			PayloadObject.FromJSonString(Token.Value);
			return GxUserType.IsEqual(value, PayloadObject);
		}


	}
	[SecuritySafeCritical]
	public static class SecureTokenHelper
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(GeneXus.Web.Security.SecureTokenHelper));

        public enum SecurityMode
        {
            Sign,
            SignEncrypt,           
            None 
        }

		internal static WebSecureToken getWebSecureToken(string signedToken, string secretKey)
		{
			byte[] bSecretKey = Encoding.ASCII.GetBytes(secretKey);
			if (string.IsNullOrEmpty(signedToken))
				return null;
			string payload = Jose.JWT.Decode(signedToken, bSecretKey);
			WebSecureToken Token = new WebSecureToken();
			Token.FromJSonString(payload);
			return Token;
		}
		public static string Sign(SecureToken token, SecurityMode mode, string secretKey)
        {
			byte[] bSecretKey = Encoding.ASCII.GetBytes(secretKey);
			string payload = token.ToJSonString();
			string encoded = string.Empty;
            switch (mode)
            {
                case SecurityMode.Sign:
                    encoded = JWT.Encode(payload, bSecretKey, JwsAlgorithm.HS256);
                    break;
                case SecurityMode.SignEncrypt:
                    encoded = JWT.Encode(payload, bSecretKey, JweAlgorithm.PBES2_HS256_A128KW, JweEncryption.A128CBC_HS256);
                    break;
                case SecurityMode.None:
                    encoded = JWT.Encode(payload, null, JwsAlgorithm.none);
                    break;
            }
            return encoded;
		}

		internal static bool Verify(string jwtToken, SecureToken outToken, string secretKey)
		{
			bool ok = false;
			byte[] bSecretKey = Encoding.ASCII.GetBytes(secretKey);
            if (!string.IsNullOrEmpty(jwtToken))
			{				
				try
				{
					string payload = Jose.JWT.Decode(jwtToken, bSecretKey);
                    ok = outToken.FromJSonString(payload);
                }
                catch (EncryptionException e)
                {
                    GXLogging.Error(_log, string.Format("Web Token Encryption Exception for Token '{0}'", jwtToken), e);
                }
                catch (IntegrityException e)
                {
                    GXLogging.Error(_log, string.Format("Web Token Integrity Exception for Token '{0}'", jwtToken), e);
                }
                catch (Exception e)
				{
					GXLogging.Error(_log, string.Format("Web Token verify failed for Token '{0}'", jwtToken), e);
				}
			}
			return ok;
		}          
    }

    [DataContract]
    public abstract class SecureToken : IGxJSONSerializable
    {
        public abstract string ToJSonString();
        public abstract bool FromJSonString(string s);

        public virtual bool FromJSonFile(GxFile file)
        {
            throw new NotImplementedException();
        }

        public virtual bool FromJSonFile(GxFile file, GXBaseCollection<SdtMessages_Message> Messages)
        {
            throw new NotImplementedException();
        }
        
        public virtual bool FromJSonString(string s, GXBaseCollection<SdtMessages_Message> Messages)
        {
            throw new NotImplementedException();
        }

    }

    [DataContract]
    public class WebSecureToken: SecureToken
    {
        [DataMember(Name = "gx-issuer", IsRequired = true, EmitDefaultValue = false)]
        public string Issuer { get; set; }

		[DataMember(Name = "gx-pgm", IsRequired = true, EmitDefaultValue = false)]
        public string ProgramName { get; set; }

        [DataMember(Name = "gx-val", EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember(Name = "gx-exp", EmitDefaultValue = false)]
        public DateTime Expiration { get; set; }

        public WebSecureToken()
        {
            Expiration = DateTime.Now.AddDays(15);			
		}

        public override bool FromJSonString(string s)
        {
            try {
                WebSecureToken wt = JSONHelper.Deserialize<WebSecureToken>(s, Encoding.UTF8);
                this.Expiration = wt.Expiration;
                this.Value = wt.Value;
                this.ProgramName = wt.ProgramName;
                this.Issuer = wt.Issuer;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override string ToJSonString()
        {
            return JSONHelper.Serialize<WebSecureToken>(this, Encoding.UTF8);
        }
    } 
}
