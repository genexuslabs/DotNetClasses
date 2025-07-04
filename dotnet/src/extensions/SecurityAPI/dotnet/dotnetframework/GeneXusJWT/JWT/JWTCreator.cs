
using GeneXusJWT.GenexusComons;
using GeneXusJWT.GenexusJWTClaims;
using GeneXusJWT.GenexusJWTUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using GeneXusJWT.JWTClaims;
using log4net;

namespace GeneXusJWT.GenexusJWT
{
	[SecuritySafeCritical]
	public class JWTCreator : SecurityAPIObject, IJWTObject
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(JWTCreator));

		private int counter;


		[SecuritySafeCritical]
		public JWTCreator() : base()
		{


			EncodingUtil eu = new EncodingUtil();
			eu.setEncoding("UTF8");
			this.counter = 0;
			/***Config to Debug - Delete on Release version!!!***/
			Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/

		[SecuritySafeCritical]
		public string DoCreate(string algorithm, PrivateClaims privateClaims, JWTOptions options)
		{
			logger.Debug("DoCreate");
			this.error.cleanError();
			return Create_Aux(algorithm, privateClaims, options, null, true);
		}

		[SecuritySafeCritical]
		public string DoCreateFromJSON(string algorithm, string json, JWTOptions options)
		{
			logger.Debug("DoCreateFromJSON");
			this.error.cleanError();
			return Create_Aux(algorithm, null, options, json, false);
		}

		[SecuritySafeCritical]
		public bool DoVerify(String token, String expectedAlgorithm, PrivateClaims privateClaims, JWTOptions options)
		{
			logger.Debug("DoVerify");
			this.error.cleanError();
			return DoVerify(token, expectedAlgorithm, privateClaims, options, true, true);
		}

		[SecuritySafeCritical]
		public bool DoVerifyJustSignature(String token, String expectedAlgorithm, JWTOptions options)
		{
			logger.Debug("DoVerifyJustSignature");
			this.error.cleanError();
			return DoVerify(token, expectedAlgorithm, null, options, false, false);
		}

		[SecuritySafeCritical]
		public bool DoVerifySignature(String token, String expectedAlgorithm, JWTOptions options)
		{
			logger.Debug("DoVerifySignature");
			this.error.cleanError();
			return DoVerify(token, expectedAlgorithm, null, options, false, true);
		}

		[SecuritySafeCritical]
		public string GetPayload(string token)
		{
			string method = "GetPayload";
			logger.Debug(method);
			this.error.cleanError();
			string res = "";
			try
			{
				res = getTokenPart(token, "payload");
			}
			catch (Exception e)
			{
				this.error.setError("JW001", e.Message);
				logger.Error(method, e);
				return "";
			}
			return res;

		}

		[SecuritySafeCritical]
		public string GetHeader(string token)
		{
			string method = "GetHeader";
			logger.Debug(method);
			this.error.cleanError();
			string res = "";
			try
			{
				res = getTokenPart(token, "header");
			}
			catch (Exception e)
			{
				this.error.setError("JW002", e.Message);
				logger.Error(method, e);
				return "";
			}
			return res;
		}

		[SecuritySafeCritical]
		public string GetTokenID(string token)
		{
			string method = "GetTokenID";
			logger.Debug(method);
			this.error.cleanError();
			string res = "";
			try
			{

				res = getTokenPart(token, "id");
			}
			catch (Exception e)
			{
				this.error.setError("JW003", e.Message);
				logger.Error(method, e);
				return "";
			}
			return res;
		}


		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		[SecuritySafeCritical]
		private string Create_Aux(string algorithm, PrivateClaims privateClaims, JWTOptions options, string payloadString, bool hasClaims)
		{
			string method = "Create_Aux";
			logger.Debug(method);
			if (options == null)
			{
				this.error.setError("JW004", "Options parameter is null");
				logger.Error("Options parameter is null");
				return "";
			}
			JWTAlgorithm alg = JWTAlgorithmUtils.getJWTAlgorithm(algorithm, this.error);
			if (this.HasError())
			{
				return "";
			}
			/***Hack to support 1024 RSA key lengths - BEGIN***/
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForSigningMap["RS256"] = 1024;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForSigningMap["RS512"] = 1024;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForSigningMap["RS384"] = 1024;
			/***Hack to support 1024 RSA key lengths - END***/

			/***Hack to support 192 ECDSA key lengths - BEGIN***/
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForSigningMap["ES256"] = 112;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForSigningMap["ES512"] = 112;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForSigningMap["ES384"] = 112;
			/***Hack to support 192 ECDSA key lengths - END***/
			JwtPayload payload = null;
			if (hasClaims)
			{
				if (privateClaims == null)
				{
					this.error.setError("JW005", "PrivateClaims parameter is null");
					logger.Error("PrivateClaims parameter is null");
					return "";
				}
				payload = doBuildPayload(privateClaims, options);
			}
			else
			{
				try
				{
					payload = JwtPayload.Deserialize(payloadString);
				}
				catch (Exception ex)
				{
					this.error.setError("", ex.Message);
					logger.Error(method, ex);
					return "";
				}
			}


			SecurityKey genericKey = null;
			if (JWTAlgorithmUtils.isPrivate(alg))
			{

				PrivateKeyManager key = options.GetPrivateKey();
				if(key == null)
				{
					this.error.setError("JW018", "Add the private key using JWTOptions.SetPrivateKey function");
					return "";
				}
				if (key.HasError())
				{
					this.error = key.GetError();
					return "";
				}
				try
				{
					switch (key.getAlgorithm())
					{
						case "RSA":
							genericKey = new RsaSecurityKey((RSA)key.getAsymmetricAlgorithm());
							break;
						case "ECDSA":
							genericKey = new ECDsaSecurityKey((ECDsa)key.getAsymmetricAlgorithm());
							break;
						default:
							this.error.setError("JW019", "Not recognized key algorithm");
							logger.Error("Not recognized key algorithm");
							return "";
					}
				}catch(Exception e)
				{
					this.error = key.HasError() ? key.GetError() : new Error("JW020", e.Message);
					return "";
				}
			}
			else
			{
				if(options.getSecret() == null)
				{
					this.error.setError("JW021", "Set the secret using JWTOptions.SetSecret function");
					logger.Error("Set the secret using JWTOptions.SetSecret function");
					return "";
				}
				SymmetricSecurityKey symKey = new SymmetricSecurityKey(options.getSecret());
				genericKey = symKey;
			}

			SigningCredentials signingCredentials = JWTAlgorithmUtils.getSigningCredentials(alg, genericKey, this.error);
			if (this.HasError())
			{

				return "";
			}

			string signedJwt = "";
			try
			{

				JwtHeader header = new JwtHeader(signingCredentials);
				if (!options.GetHeaderParameters().IsEmpty())
				{
					AddHeaderParameters(header, options);
				}

				JwtSecurityToken secToken = new JwtSecurityToken(header, payload);
				JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
				signedJwt = handler.WriteToken(secToken);
			}
			catch (Exception e)
			{

				this.error.setError("JW006", e.Message);
				logger.Error(method, e);

				return "";
			}

			return signedJwt;
		}

		[SecuritySafeCritical]
		private bool DoVerify(string token, string expectedAlgorithm, PrivateClaims privateClaims, JWTOptions options, bool verifyClaims, bool verifyRegClaims)
		{
			string method = "DoVerify";
			logger.Debug(method);
			if (options == null)
			{
				this.error.setError("JW007", "Options parameter is null");
				logger.Error("Options parameter is null");
				return false;
			}
			JWTAlgorithm expectedJWTAlgorithm = JWTAlgorithmUtils.getJWTAlgorithm(expectedAlgorithm, this.error);
			if (this.HasError())
			{
				return false;
			}

			/***Hack to support 1024 RSA key lengths - BEGIN***/
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["RS256"] = 1024;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["RS512"] = 1024;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["RS384"] = 1024;
			/***Hack to support 1024 RSA key lengths - END***/

			/***Hack to support 192 ECDSA key lengths - BEGIN***/
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["EcdsaSha256"] = 112;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["EcdsaSha512"] = 112;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["EcdsaSha384"] = 112;
			/***Hack to support 192 ECDSA key lengths - END***/

			/***Hack to support 192 ECDSA key lengths - BEGIN***/
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["ES256"] = 112;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["ES512"] = 112;
			AsymmetricSignatureProvider.DefaultMinimumAsymmetricKeySizeInBitsForVerifyingMap["ES384"] = 112;
			/***Hack to support 192 ECDSA key lengths - END***/


			JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
			JwtSecurityToken jwtToken = new JwtSecurityToken(token);
			if (isRevoqued(jwtToken, options))
			{
				return false;
			}
			if (verifyRegClaims)
			{
				if (!validateRegisteredClaims(jwtToken, options))
				{
					return false;
				}
			}
			if (verifyClaims)
			{
				if (!verifyPrivateClaims(jwtToken, privateClaims, options) || !VerifyHeader(jwtToken, options))
				{
					return false;
				}
			}
			//if validates all registered claims and it is not on revocation list
			TokenValidationParameters parms = new TokenValidationParameters();
			parms.ValidateLifetime = false;
			parms.ValidateAudience = false;
			parms.ValidateIssuer = false;
			parms.ValidateActor = false;
			JWTAlgorithm alg = JWTAlgorithmUtils.getJWTAlgorithm_forVerification(jwtToken.Header.Alg, this.error);
			if (this.HasError())
			{
				return false;
			}
			if (JWTAlgorithmUtils.getJWTAlgorithm(jwtToken.Header.Alg, this.error) != expectedJWTAlgorithm || this.HasError())
			{
				this.error.setError("JW009", "Expected algorithm does not match token algorithm");
				logger.Error("Expected algorithm does not match token algorithm");
				return false;
			}
			SecurityKey genericKey = null;
			if (JWTAlgorithmUtils.isPrivate(alg))
			{
				PublicKey cert = options.GetPublicKey();
				if(cert == null)
				{
					this.error.setError("JW022", "Public key or certificate not loaded for verification");
					logger.Error("Public key or certificate not loaded for verification");
					return false;
				}
				if (cert.HasError())
				{
					this.error = cert.GetError();
					return false;
				}
				try
				{
					switch (cert.getAlgorithm())
					{
						case "RSA":
							genericKey = new RsaSecurityKey((RSA)cert.getAsymmetricAlgorithm());
							break;
						case "ECDSA":
							genericKey = new ECDsaSecurityKey((ECDsa)cert.getAsymmetricAlgorithm());
							break;
						default:
							this.error.setError("JW019", "Not recognized key algorithm");
							logger.Error("Not recognized key algorithm");
							return false;
					}
				}catch(Exception e)
				{
					this.error = cert.HasError() ? cert.GetError(): new Error("JW020", e.Message);
					return false;
				}
			}
			else
			{
				if(options.getSecret() == null)
				{
					this.error.setError("JW022", "Symmetric key not loaded for verification");
					logger.Error("Symmetric key not loaded for verification");
					return false;
				}	
				SymmetricSecurityKey symKey = new SymmetricSecurityKey(options.getSecret());
				genericKey = symKey;
			}
			genericKey.KeyId = "256";

			SigningCredentials signingCredentials = JWTAlgorithmUtils.getSigningCredentials(alg, genericKey, this.error);
			parms.IssuerSigningKey = genericKey;
			SecurityToken validatedToken;
			try
			{
				handler.ValidateToken(token, parms, out validatedToken);
			}
			catch (Exception e)
			{
				this.error.setError("JW008", e.Message);
				logger.Error(method, e);

				return false;
			}
			return true;



		}


		private JwtPayload doBuildPayload(PrivateClaims privateClaims, JWTOptions options)
		{
			logger.Debug("doBuildPayload");
			JwtPayload payload = new JwtPayload();
			// ****START BUILD PAYLOAD****//
			// Adding private claims
			List<Claim> privateC = privateClaims.getAllClaims();
			foreach (Claim privateClaim in privateC)
			{

				if (privateClaim.getNestedClaims() != null)
				{

					payload.Add(privateClaim.getKey(), privateClaim.getNestedClaims().getNestedMap());
				}
				else
				{
					System.Security.Claims.Claim netPrivateClaim = null;
					object obj = privateClaim.getValue();
					if (obj.GetType() == typeof(string))
					{
						netPrivateClaim = new System.Security.Claims.Claim(privateClaim.getKey(), (string)privateClaim.getValue());
					}
					else if (obj.GetType() == typeof(int))
					{
						int value = (int)obj;
						netPrivateClaim = new System.Security.Claims.Claim(privateClaim.getKey(), value.ToString(System.Globalization.CultureInfo.InvariantCulture), System.Security.Claims.ClaimValueTypes.Integer32);
					}
					else if (obj.GetType() == typeof(long))
					{
						long value = (long)obj;
						netPrivateClaim = new System.Security.Claims.Claim(privateClaim.getKey(), value.ToString(System.Globalization.CultureInfo.InvariantCulture), System.Security.Claims.ClaimValueTypes.Integer64);
					}
					else if (obj.GetType() == typeof(double))
					{
						double value = (double)obj;
						netPrivateClaim = new System.Security.Claims.Claim(privateClaim.getKey(), value.ToString(System.Globalization.CultureInfo.InvariantCulture), System.Security.Claims.ClaimValueTypes.Double);
					}
					else if (obj.GetType() == typeof(bool))
					{
						bool value = (bool)obj;
						netPrivateClaim = new System.Security.Claims.Claim(privateClaim.getKey(), value.ToString(System.Globalization.CultureInfo.InvariantCulture), System.Security.Claims.ClaimValueTypes.Boolean);
					}
					else
					{
						this.error.setError("JW014", "Unrecognized data type");
						logger.Error("Unrecognized data type");
					}

					//System.Security.Claims.Claim netPrivateClaim = new System.Security.Claims.Claim(privateClaim.getKey(), privateClaim.getValue());

					payload.AddClaim(netPrivateClaim);
				}

			}
			// Adding public claims
			if (options.hasPublicClaims())
			{
				PublicClaims publicClaims = options.getAllPublicClaims();
				List<Claim> publicC = publicClaims.getAllClaims();
				foreach (Claim publicClaim in publicC)
				{
					System.Security.Claims.Claim netPublicClaim = new System.Security.Claims.Claim(publicClaim.getKey(), (string)publicClaim.getValue());
					payload.AddClaim(netPublicClaim);
				}

			}
			// Adding registered claims
			if (options.hasRegisteredClaims())
			{
				RegisteredClaims registeredClaims = options.getAllRegisteredClaims();
				List<Claim> registeredC = registeredClaims.getAllClaims();
				foreach (Claim registeredClaim in registeredC)
				{
					System.Security.Claims.Claim netRegisteredClaim;

					if (RegisteredClaimUtils.isTimeValidatingClaim(registeredClaim.getKey()))
					{

						netRegisteredClaim = new System.Security.Claims.Claim(registeredClaim.getKey(), (string)registeredClaim.getValue(), System.Security.Claims.ClaimValueTypes.Integer32);
					}
					else
					{

						netRegisteredClaim = new System.Security.Claims.Claim(registeredClaim.getKey(), (string)registeredClaim.getValue());
					}

					payload.AddClaim(netRegisteredClaim);
				}
			}
			// ****END BUILD PAYLOAD****//
			return payload;
		}

		private bool validateRegisteredClaims(JwtSecurityToken jwtToken, JWTOptions options)
		{

			logger.Debug("validateRegisteredClaims");
			// Adding registered claims
			if (options.hasRegisteredClaims())
			{
				RegisteredClaims registeredClaims = options.getAllRegisteredClaims();
				List<Claim> registeredC = registeredClaims.getAllClaims();
				foreach (Claim registeredClaim in registeredC)
				{
					string registeredClaimKey = registeredClaim.getKey();
					object registeredClaimValue = registeredClaim.getValue();
					if (RegisteredClaimUtils.exists(registeredClaimKey))
					{
						if (!RegisteredClaimUtils.isTimeValidatingClaim(registeredClaimKey))
						{
							if (!RegisteredClaimUtils.validateClaim(registeredClaimKey, (string)registeredClaimValue, 0, jwtToken, this.error))
							{
								return false;
							}
						}
						else
						{
							long customValidationTime = registeredClaims.getClaimCustomValidationTime(registeredClaimKey);
							//int value = (int)registeredClaimValue;
							if (!RegisteredClaimUtils.validateClaim(registeredClaimKey, (string)registeredClaimValue, customValidationTime, jwtToken, this.error))
							{
								return false;
							}
						}
						if (this.HasError())
						{
							return false;
						}


					}
					else
					{
						error.setError("JW017", String.Format("{0} wrong registered claim key", registeredClaimKey));
						logger.Error(String.Format("{0} wrong registered claim key", registeredClaimKey));
						return false;
					}
				}
			}
			return true;
		}
		private static bool isRevoqued(JwtSecurityToken jwtToken, JWTOptions options)
		{
			RevocationList rList = options.getRevocationList();
			return rList.isInRevocationList(jwtToken.Payload.Jti);
		}

		private string getTokenPart(string token, String part)
		{
			logger.Debug("getTokenPart");
			JwtSecurityToken jwtToken = new JwtSecurityToken(token);

			switch (part)
			{
				case "payload":
					return jwtToken.Payload.SerializeToJson();
				case "header":
					return jwtToken.Header.SerializeToJson();
				case "id":
					return jwtToken.Payload.Jti;
				default:
					error.setError("JW012", "Unknown token segment");
					logger.Error("Unknown token segment");
					return "";
			}

		}

		private bool verifyPrivateClaims(JwtSecurityToken jwtToken, PrivateClaims privateClaims, JWTOptions options)
		{
			string method = "verifyPrivateClaims";
			logger.Debug(method);
			RegisteredClaims registeredClaims = options.getAllRegisteredClaims();
			PublicClaims publicClaims = options.getAllPublicClaims();
			if (privateClaims == null || privateClaims.isEmpty())
			{
				return true;
			}
			string jsonPayload = jwtToken.Payload.SerializeToJson();
			Dictionary<string, object> map = null;
			try
			{
				map = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonPayload);
			}
			catch (Exception e)
			{
				this.error.setError("JW018", e.Message);
				logger.Error(method, e);
				return false;
			}
			this.counter = 0;
			bool validation = verifyNestedClaims(privateClaims.getNestedMap(), map, registeredClaims, publicClaims);
			int pClaimsCount = countingPrivateClaims(privateClaims.getNestedMap(), 0);
			if (validation && !(this.counter == pClaimsCount))
			{
				return false;
			}
			return validation;
		}

		private bool verifyNestedClaims(Dictionary<string, object> pclaimMap, Dictionary<string, object> map,
					RegisteredClaims registeredClaims, PublicClaims publicClaims)
		{
			logger.Debug("verifyNestedClaims");
			List<string> mapClaimKeyList = new List<string>(map.Keys);
			List<string> pClaimKeyList = new List<string>(pclaimMap.Keys);
			if (pClaimKeyList.Count > pClaimKeyList.Count)
			{
				return false;
			}
			foreach (string mapKey in mapClaimKeyList)
			{

				if (!isRegistered(mapKey, registeredClaims) && !isPublic(mapKey, publicClaims))
				{
					this.counter++;
					if (!pclaimMap.ContainsKey(mapKey))
					{
						return false;
					}

					object op = pclaimMap[mapKey];
					object ot = map[mapKey];
					Type opt = op.GetType();
					Type ott = ot.GetType();
					Type int16 = Type.GetType("System.Int16", false, true);
					Type int32 = Type.GetType("System.Int32", false, true);
					Type int64 = Type.GetType("System.Int64", false, true);
					if ((opt == typeof(string)) && (ott == typeof(string)))
					{

						if (!SecurityUtils.compareStrings(((string)op).Trim(), ((string)ot).Trim()))
						{
							return false;
						}
					}

					else if (((opt == int16) || (opt == int32) || (opt == int64)) && ((ott == int16) || (ott == int32) || (ott == int64)))
					{
						if (Convert.ToInt32(op, System.Globalization.CultureInfo.InvariantCulture) != Convert.ToInt32(ot, System.Globalization.CultureInfo.InvariantCulture))
						{
							return false;
						}
					}
					else if (opt == typeof(bool))
					{
						if (Convert.ToBoolean(op, System.Globalization.CultureInfo.InvariantCulture) != Convert.ToBoolean(ot, System.Globalization.CultureInfo.InvariantCulture))
						{
							return false;
						}

					}
					else if ((op.GetType() == typeof(Dictionary<string, object>)) && (ot.GetType() == typeof(JObject)))
					{


						bool flag = verifyNestedClaims((Dictionary<string, object>)op, ((JObject)ot).ToObject<Dictionary<string, object>>(),
								registeredClaims, publicClaims);
						if (!flag)
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
			}
			return true;
		}

		private void AddHeaderParameters(JwtHeader header, JWTOptions options)
		{
			logger.Debug("AddHeaderParameters");
			HeaderParameters parameters = options.GetHeaderParameters();
			List<string> list = parameters.GetAll();
			Dictionary<string, object> map = parameters.GetMap();
			foreach (string s in list)
			{
				header.Add(s.Trim(), ((string)map[s]).Trim());
			}
		}

		private static bool VerifyHeader(JwtSecurityToken jwtToken, JWTOptions options)
		{
			logger.Debug("VerifyHeader");
			int claimsNumber = jwtToken.Header.Count;
			HeaderParameters parameters = options.GetHeaderParameters();
			if (parameters.IsEmpty() && claimsNumber == 2)
			{
				return true;
			}
			if (parameters.IsEmpty() && claimsNumber > 2)
			{
				return false;
			}

			List<string> allParms = parameters.GetAll();
			if (claimsNumber != allParms.Count + 2)
			{
				return false;
			}
			Dictionary<string, Object> map = parameters.GetMap();


			foreach (string s in allParms)
			{

				if (!jwtToken.Header.ContainsKey(s.Trim()))
				{
					return false;
				}


				string claimValue = null;
				try
				{
					claimValue = (string)jwtToken.Header[s.Trim()];
				}
				catch (Exception)
				{
					return false;
				}
				string optionsValue = ((string)map[s]).Trim();
				if (!SecurityUtils.compareStrings(claimValue, optionsValue.Trim()))
				{
					return false;
				}
			}
			return true;

		}

		private static bool isRegistered(string claimKey, RegisteredClaims registeredClaims)
		{

			List<Claim> registeredClaimsList = registeredClaims.getAllClaims();
			foreach (Claim s in registeredClaimsList)
			{
				if (SecurityUtils.compareStrings(s.getKey().Trim(), claimKey.Trim()))
				{
					return true;
				}
			}
			return false;
		}

		private static bool isPublic(string claimKey, PublicClaims publicClaims)
		{
			List<Claim> publicClaimsList = publicClaims.getAllClaims();
			foreach (Claim s in publicClaimsList)
			{
				if (SecurityUtils.compareStrings(s.getKey().Trim(), claimKey.Trim()))
				{
					return true;
				}
			}
			return false;
		}

		private int countingPrivateClaims(Dictionary<string, object> map, int counter)
		{
			List<string> list = new List<string>(map.Keys);
			foreach (string s in list)
			{
				counter++;
				object obj = map[s];
				if (obj.GetType() == typeof(Dictionary<string, object>))
				{
					counter = countingPrivateClaims((Dictionary<string, object>)obj, counter);
				}
			}
			return counter;
		}
	}
}


