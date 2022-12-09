using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Utils;
using log4net;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Claims;
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
			bool jwtVerifyOk = SecureTokenHelper.Verify(jwtToken, token, GetSecretKey(context));
			bool contentVerifyOk = jwtVerifyOk && !string.IsNullOrEmpty(pgmName) && token.ProgramName == pgmName && issuer == token.Issuer &&
				StripInvalidChars(token.Value) == StripInvalidChars(value) && token.Expiration >= DateTime.Now;

			if (!contentVerifyOk && _log.IsErrorEnabled)
			{
				StringBuilder errMessage = new StringBuilder("WebSecurity Token Verification error");
				if (!jwtVerifyOk)
				{
					errMessage.Append($" - JWT Signature Verification failed");
				}
				if (token.ProgramName != pgmName)
				{
					errMessage.Append($" - ProgramName mismatch '{token.ProgramName}' <> '{pgmName}'");
				}
				if (StripInvalidChars(token.Value) != StripInvalidChars(value))
				{
					errMessage.Append($" - Value mismatch '{StripInvalidChars(token.Value)}' <> '{StripInvalidChars(value)}'");
				}
				else if (issuer != token.Issuer)
				{
					errMessage.Append($" - Issuer mismatch '{token.Issuer}' <> '{issuer}'");
				}

				if (token.Expiration < DateTime.Now)
				{
					errMessage.Append(" - Token expired ");
				}
				GXLogging.Error(_log, errMessage.ToString());
			}
			return contentVerifyOk;
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

			using (var hmac = new System.Security.Cryptography.HMACSHA256(bSecretKey))
			{
				var handler = new JwtSecurityTokenHandler();
				var validationParameters = new TokenValidationParameters
				{
					ClockSkew = TimeSpan.FromMinutes(1),
					ValidateAudience = false,
					ValidateIssuer = false,
					IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
				};
				SecurityToken securityToken;
				WebSecureToken outToken = new WebSecureToken();
				var claims = handler.ValidateToken(signedToken, validationParameters, out securityToken);
				outToken.Value = claims.Identities.First().Claims.First(c => c.Type == WebSecureToken.GXVALUE).Value;
				return outToken;
			}
		}
        public static string Sign(WebSecureToken token, SecurityMode mode, string secretKey)
        {
			byte[] bSecretKey = Encoding.ASCII.GetBytes(secretKey);
			string encoded = string.Empty;
			switch (mode)
			{
				case SecurityMode.Sign:
					var tokenHandler = new JwtSecurityTokenHandler();

					var jwtoken = tokenHandler.CreateJwtSecurityToken(issuer: token.Issuer, audience: null, 
                           subject: new ClaimsIdentity(new[] {
							new Claim(WebSecureToken.GXISSUER, token.Issuer),
							new Claim(WebSecureToken.GXPROGRAM, token.ProgramName),
							new Claim(WebSecureToken.GXVALUE, token.Value),
							new Claim(WebSecureToken.GXEXPIRATION, token.Expiration.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString())
							}),
						notBefore: DateTime.UtcNow,
						expires: token.Expiration,
						signingCredentials: new SigningCredentials(new SymmetricSecurityKey(bSecretKey), SecurityAlgorithms.HmacSha256));

					return tokenHandler.WriteToken(jwtoken);
			}
            return encoded;
		}

		internal static bool Verify(string jwtToken, WebSecureToken outToken, string secretKey)
		{
			bool ok = false;
			string payload = "";
			byte[] bSecretKey = Encoding.ASCII.GetBytes(secretKey);
            if (!string.IsNullOrEmpty(jwtToken))
			{				
				try
				{
					using (var hmac = new System.Security.Cryptography.HMACSHA256(bSecretKey))
					{
						var handler = new JwtSecurityTokenHandler();
						var validationParameters = new TokenValidationParameters
						{
							ClockSkew = TimeSpan.FromMinutes(1),
							ValidateAudience = false,
							ValidateIssuer = false,
							IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
						};
						//Avoid handler.ValidateToken which does not work in medium trust environment
						JwtSecurityToken jwtSecurityToken = (JwtSecurityToken)handler.GetType().InvokeMember("ValidateSignature",
							System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, handler,
							new object[] { jwtToken, validationParameters });
						Validators.ValidateIssuerSecurityKey(jwtSecurityToken.SigningKey, jwtSecurityToken, validationParameters);
						payload = jwtSecurityToken.EncodedPayload;
						outToken.Expiration = new DateTime(1970, 1, 1).AddSeconds(Double.Parse(jwtSecurityToken.Claims.First(c => c.Type == WebSecureToken.GXEXPIRATION).Value));
						outToken.ProgramName = jwtSecurityToken.Claims.First(c => c.Type == WebSecureToken.GXPROGRAM).Value;
						outToken.Issuer = jwtSecurityToken.Claims.First(c => c.Type == WebSecureToken.GXISSUER).Value;
						outToken.Value = jwtSecurityToken.Claims.First(c => c.Type == WebSecureToken.GXVALUE).Value;
						ok = true;
					}
                }
                catch (Exception e)
				{
					string json = System.Text.Json.JsonSerializer.Serialize(new { Property = payload });
					GXLogging.Error(_log, string.Format("Web Token verify failed for Token '{0}'",json), e);
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
        internal const string GXISSUER = "gx-issuer";
        internal const string GXPROGRAM = "gx-pgm";
        internal const string GXVALUE = "gx-val";
        internal const string GXEXPIRATION = "gx-exp";

        [DataMember(Name = GXISSUER, IsRequired = true, EmitDefaultValue = false)]
        public string Issuer { get; set; }

		[DataMember(Name = GXPROGRAM, IsRequired = true, EmitDefaultValue = false)]
        public string ProgramName { get; set; }

        [DataMember(Name = GXVALUE, EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember(Name = GXEXPIRATION, EmitDefaultValue = false)]
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
