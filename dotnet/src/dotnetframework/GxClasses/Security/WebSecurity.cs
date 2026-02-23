using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Claims;
using System.Text;
using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.Utils;
using Microsoft.IdentityModel.Tokens;
using static GeneXus.Web.Security.SecureTokenHelper;

namespace GeneXus.Web.Security
{
	[SecuritySafeCritical]
	public static class WebSecurityHelper
    {
		static readonly IGXLogger _log = GXLoggerFactory.GetLogger(typeof(WebSecurityHelper).FullName);

		const int SecretKeyMinimumLength = 32;

        public static string StripInvalidChars(string input)
        {
			if (string.IsNullOrEmpty(input))
				return input;
			string output = new string(input.Where(c => !char.IsControl(c)).ToArray());
            return output.Trim();
        }

        public static string Sign(string pgmName, string issuer, string value, SecurityMode mode, IGxContext context)
        {            
            return SecureTokenHelper.Sign(new WebSecureToken { ProgramName = pgmName, Issuer = issuer, Value = string.IsNullOrEmpty(value) ? string.Empty: StripInvalidChars(value) }, mode, GetSecretKey(context));
        }
		internal static string Sign(string pgmName, string issuer, TokenValue tokenValue, SecurityMode mode, IGxContext context)
		{
			return SecureTokenHelper.Sign(new WebSecureToken {
				ProgramName = pgmName,
				Issuer = issuer,
				ValueType = tokenValue.ValueType,
				Value = string.IsNullOrEmpty(tokenValue.Value) ? string.Empty : StripInvalidChars(tokenValue.Value) },
				mode, GetSecretKey(context));
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
			WebSecureToken jwtTokenObj = SecureTokenHelper.getWebSecureToken(jwtToken, GetSecretKey(context), false);
			if (jwtTokenObj != null && jwtTokenObj.ValueType == ValueTypeHash)
			{
				return Verify(pgmName, issuer, GetHash(value), jwtToken, out token, context);
			}
			else
			{
				return Verify(pgmName, issuer, value, jwtToken, out token, context);
			}
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
			WebSecureToken token = SecureTokenHelper.getWebSecureToken(signedToken, GetSecretKey(context));
			if (token == null)
				return false;
			if (token.ValueType == SecureTokenHelper.ValueTypeHash) 
			{
				return VerifyTokenHash(value.ToJSonString(), token);
			}
			else
			{
				IGxCollection PayloadObject = (IGxCollection)value.Clone();
				PayloadObject.FromJSonString(token.Value);
				return GxUserType.IsEqual(value, PayloadObject);
			}
		}

		internal static bool VerifySecureSignedSDTToken(string cmpCtx, GxUserType value, string signedToken, IGxContext context)
		{
			WebSecureToken token = SecureTokenHelper.getWebSecureToken(signedToken, GetSecretKey(context));
			if (token == null)
				return false;
			if (token.ValueType == ValueTypeHash) 
			{
				return VerifyTokenHash(value.ToJSonString(), token); 
			}
			else
			{
				GxUserType PayloadObject = (GxUserType)value.Clone();
				PayloadObject.FromJSonString(token.Value);
				return GxUserType.IsEqual(value, PayloadObject);
			}

		}

		private static bool VerifyTokenHash(string payloadJsonString, WebSecureToken token)
		{
			string hash = GetHash(payloadJsonString);
			if (hash != token.Value)
			{
				GXLogging.Error(_log, $"WebSecurity Token Verification error - Hash mismatch '{hash}' <> '{token.Value}'");
				GXLogging.Debug(_log, "Payload TokenOriginalValue: " + payloadJsonString);
				return false;
			}
			return true;
		}
	}
	internal class TokenValue
	{
		internal string Value { get; set; }
		internal string ValueType { get; set; }
	}

	[SecuritySafeCritical]
	public static class SecureTokenHelper
    {
		static readonly IGXLogger _log = GXLoggerFactory.GetLogger(typeof(SecureTokenHelper).FullName);
		internal const string ValueTypeHash = "hash";
		const int MaxTokenValueLength = 1024;

        public enum SecurityMode
        {
            Sign,
            SignEncrypt,           
            None 
        }
		internal static WebSecureToken getWebSecureToken(string signedToken, string secretKey, bool validate=true)
		{
			byte[] bSecretKey = Encoding.ASCII.GetBytes(secretKey);
			if (string.IsNullOrEmpty(signedToken))
				return null;

			using (var hmac = new System.Security.Cryptography.HMACSHA256(bSecretKey))
			{
				var handler = new JwtSecurityTokenHandler();
				if (signedToken.Length >= handler.MaximumTokenSizeInBytes)
				{
					handler.MaximumTokenSizeInBytes = signedToken.Length + 1;
				}
				WebSecureToken outToken = new WebSecureToken();
				if (validate)
				{
					var validationParameters = new TokenValidationParameters
					{
						ClockSkew = TimeSpan.FromMinutes(1),
						ValidateAudience = false,
						ValidateIssuer = false,
						IssuerSigningKey = new SymmetricSecurityKey(hmac.Key),
					};
					SecurityToken securityToken;
					var claims = handler.ValidateToken(signedToken, validationParameters, out securityToken);
					outToken.Value = claims.Identities.First().Claims.First(c => c.Type == WebSecureToken.GXVALUE).Value;
					outToken.ValueType = claims.Identities.First().Claims.First(c => c.Type == WebSecureToken.GXVALUE_TYPE)?.Value ?? string.Empty;
				}
				else
				{
					JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(signedToken);
					var claims = jwtSecurityToken.Claims;
					outToken.Value = claims.First(c => c.Type == WebSecureToken.GXVALUE).Value;
					outToken.ValueType = claims.First(c => c.Type == WebSecureToken.GXVALUE_TYPE)?.Value ?? string.Empty;
				}
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
							new Claim(WebSecureToken.GXEXPIRATION, token.Expiration.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString()),
							new Claim(WebSecureToken.GXVALUE_TYPE, token.ValueType ?? string.Empty)
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

						outToken.Expiration = new DateTime(1970, 1, 1).AddSeconds(Double.Parse(jwtSecurityToken.Claims.First(c => c.Type == WebSecureToken.GXEXPIRATION).Value));
						outToken.ProgramName = jwtSecurityToken.Claims.First(c => c.Type == WebSecureToken.GXPROGRAM).Value;
						outToken.Issuer = jwtSecurityToken.Claims.First(c => c.Type == WebSecureToken.GXISSUER).Value;
						outToken.Value = jwtSecurityToken.Claims.First(c => c.Type == WebSecureToken.GXVALUE).Value;
						ok = true;
					}
                }
                catch (Exception e)
				{
					GXLogging.Error(_log, string.Format("Web Token verify failed for Token '{0}'", jwtToken), e);
				}
			}
			return ok;
		}
		internal static TokenValue GetTokenValue(IGxJSONSerializable obj)
		{
			return GetTokenValue(obj.ToJSonString());
		}
		internal static TokenValue GetTokenValue(string value)
		{

			if (value!=null && value.Length > MaxTokenValueLength)
			{
				string hash = GetHash(value);
				GXLogging.Debug(_log, $"GetTokenValue: TokenValue is too long, using hash: {hash} instead of original value.");
				GXLogging.Debug(_log, $"Server TokenOriginalValue:" + value);
				return new TokenValue() { Value = hash, ValueType = ValueTypeHash };
			}
			else
			{
				GXLogging.Debug(_log, $"GetTokenValue:" + value);
				return new TokenValue() { Value = value };
			}
		}
		internal static string GetHash(string jsonString)
		{
			using (var sha256 = System.Security.Cryptography.SHA256.Create())
			{
				byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(jsonString));
				jsonString = Convert.ToBase64String(hashBytes);
				return jsonString;
			}
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
		internal const string GXVALUE_TYPE = "gx-val-type";

		[DataMember(Name = GXISSUER, IsRequired = true, EmitDefaultValue = false)]
        public string Issuer { get; set; }

		[DataMember(Name = GXPROGRAM, IsRequired = true, EmitDefaultValue = false)]
        public string ProgramName { get; set; }

        [DataMember(Name = GXVALUE, EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember(Name = GXEXPIRATION, EmitDefaultValue = false)]
        public DateTime Expiration { get; set; }

		[DataMember(Name = GXVALUE_TYPE, EmitDefaultValue = false)]
		public string ValueType { get; set; }
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
				this.ValueType = wt.ValueType;
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
