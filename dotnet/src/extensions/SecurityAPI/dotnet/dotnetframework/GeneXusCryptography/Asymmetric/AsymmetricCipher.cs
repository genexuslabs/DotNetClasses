
using GeneXusCryptography.AsymmetricUtils;
using GeneXusCryptography.Commons;
using GeneXusCryptography.Hash;
using GeneXusCryptography.HashUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Utilities.Encoders;
using System.Security;
using SecurityAPICommons.Utils;
using log4net;

namespace GeneXusCryptography.Asymmetric
{
	/// <summary>
	/// Implements Asymmetric Block Cipher Engines and methods to encrypt and decrypt
	/// </summary>
	[SecuritySafeCritical]
	public class AsymmetricCipher : SecurityAPIObject, IAsymmetricCipherObject
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AsymmetricCipher));
		private readonly string className = typeof(AsymmetricCipher).Name;

		/// <summary>
		/// AsymmetricCipher class constructor
		/// </summary>
		public AsymmetricCipher() : base()
		{

		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

		[SecuritySafeCritical]
		public string DoEncrypt_WithPrivateKey(string hashAlgorithm, string asymmetricEncryptionPadding, PrivateKeyManager key, string plainText)
		{
			string method = "DoEncrypt_WithPrivateKey";
			logger.Debug(method);
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput(className, method, "hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput(className, method, "asymmetricEncryptionPadding", asymmetricEncryptionPadding, this.error);
			SecurityUtils.validateStringInput(className, method, "plainText", plainText, this.error);
			SecurityUtils.validateObjectInput(className, method, "key", key, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/
			return DoEncryptInternal(hashAlgorithm, asymmetricEncryptionPadding, key, true, plainText, false);
		}

		[SecuritySafeCritical]
#pragma warning disable CA1707 // Identifiers should not contain underscores
		public string DoEncrypt_WithPublicKey(string hashAlgorithm, string asymmetricEncryptionPadding, PublicKey key, string plainText)
#pragma warning restore CA1707 // Identifiers should not contain underscores
		{
			string method = "DoEncrypt_WithPublicKey";
			logger.Debug(method);
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput(className, method, "hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput(className, method, "asymmetricEncryptionPadding", asymmetricEncryptionPadding, this.error);
			SecurityUtils.validateStringInput(className, method, "plainText", plainText, this.error);
			SecurityUtils.validateObjectInput(className, method, "key", key, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/

			return DoEncryptInternal(hashAlgorithm, asymmetricEncryptionPadding, key, false, plainText, true);
		}

		[SecuritySafeCritical]
		public string DoEncrypt_WithCertificate(string hashAlgorithm, string asymmetricEncryptionPadding, CertificateX509 certificate, string plainText)
		{
			string method = "DoEncrypt_WithCertificate";
			logger.Debug("DoEncrypt_WithCertificate");
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput(className, method, "hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput(className, method, "asymmetricEncryptionPadding", asymmetricEncryptionPadding, this.error);
			SecurityUtils.validateStringInput(className, method, "plainText", plainText, this.error);
			SecurityUtils.validateObjectInput(className, method, "certificate", certificate, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/

			return DoEncryptInternal(hashAlgorithm, asymmetricEncryptionPadding, certificate, false, plainText, false);
		}

		[SecuritySafeCritical]
		public string DoDecrypt_WithPrivateKey(string hashAlgorithm, string asymmetricEncryptionPadding, PrivateKeyManager key, string encryptedInput)
		{
			string method = "DoDecrypt_WithPrivateKey";
			logger.Debug(method);
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput(className, method, "hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput(className, method, "asymmetricEncryptionPadding", asymmetricEncryptionPadding, this.error);
			SecurityUtils.validateStringInput(className, method, "encryptedInput", encryptedInput, this.error);
			SecurityUtils.validateObjectInput(className, method, "key", key, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/

			return DoDecryptInternal(hashAlgorithm, asymmetricEncryptionPadding, key, true, encryptedInput, false);
		}

		[SecuritySafeCritical]
		public string DoDecrypt_WithCertificate(string hashAlgorithm, string asymmetricEncryptionPadding, CertificateX509 certificate, string encryptedInput)
		{
			string method = "DoDecrypt_WithCertificate";
			logger.Debug(method);
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput(className, method, "hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput(className, method, "asymmetricEncryptionPadding", asymmetricEncryptionPadding, this.error);
			SecurityUtils.validateStringInput(className, method, "encryptedInput", encryptedInput, this.error);
			SecurityUtils.validateObjectInput(className, method, "certificate", certificate, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/

			return DoDecryptInternal(hashAlgorithm, asymmetricEncryptionPadding, certificate, false, encryptedInput, false);
		}

		[SecuritySafeCritical]
#pragma warning disable CA1707 // Identifiers should not contain underscores
		public string DoDecrypt_WithPublicKey(string hashAlgorithm, string asymmetricEncryptionPadding, PublicKey key, string encryptedInput)
#pragma warning restore CA1707 // Identifiers should not contain underscores
		{
			string method = "DoDecrypt_WithPublicKey";
			logger.Debug(method);
			this.error.cleanError();
			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput(className, method, "hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput(className, method, "asymmetricEncryptionPadding", asymmetricEncryptionPadding, this.error);
			SecurityUtils.validateStringInput(className, method, "encryptedInput", encryptedInput, this.error);
			SecurityUtils.validateObjectInput(className, method, "key", key, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/

			return DoDecryptInternal(hashAlgorithm, asymmetricEncryptionPadding, key, false, encryptedInput, true);
		}


		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/


		/// <summary>
		/// Encrypts the string encoded plain text
		/// </summary>
		/// <param name="asymmetricEncryptionAlgorithm">string AsymmetricEncryptionAlgorithm enum, algorithm name</param>
		/// <param name="hashAlgorithm">string HashAlgorithm enum, algorithm name</param>
		/// <param name="asymmetricEncryptionPadding">string AsymmetricEncryptionPadding enum, padding name</param>
		/// <param name="keyPath">string path to key/certificate</param>
		/// <param name="isPrivate">boolean true if key is private, false if it is public</param>
		/// <param name="alias">string keystore/certificate pkcs12 format alias</param>
		/// <param name="password">Srting keysore/certificate pkcs12 format alias</param>
		/// <param name="plainText">string to encrypt</param>
		/// <returns>string Base64 encrypted plainText text</returns>
		private string DoEncryptInternal(string hashAlgorithm, string asymmetricEncryptionPadding, Key key, bool isPrivate, string plainText, bool isPublicKey)
		{
			string method = "DoEncryptInternal";
			logger.Debug(method);
			this.error.cleanError();

			HashAlgorithm hash = HashAlgorithmUtils.getHashAlgorithm(hashAlgorithm, this.error);
			AsymmetricEncryptionPadding padding = AsymmetricEncryptionPaddingUtils.getAsymmetricEncryptionPadding(asymmetricEncryptionPadding, this.error);
			if (this.error.existsError())
			{
				return "";
			}

			string asymmetricEncryptionAlgorithm = "";
			AsymmetricKeyParameter asymKey = null;
			if (isPrivate)
			{
				PrivateKeyManager keyMan = (PrivateKeyManager)key;
				if (!keyMan.HasPrivateKey || keyMan.HasError())
				{
					this.error = keyMan.GetError();
					return "";
				}
				asymmetricEncryptionAlgorithm = keyMan.getAlgorithm();

				asymKey = keyMan.getAsymmetricKeyParameter();
				if (keyMan.HasError())
				{
					this.error = keyMan.GetError();
					return "";
				}
			}
			else
			{
				PublicKey cert = isPublicKey ? (PublicKey)key : (CertificateX509)key;
				if (cert.HasError())
				{
					this.error = cert.GetError();
					return "";
				}
				if (cert.HasError())
				{
					this.error = cert.GetError();
					return "";
				}
				asymmetricEncryptionAlgorithm = cert.getAlgorithm();
				asymKey = cert.getAsymmetricKeyParameter();
				if (cert.HasError())
				{
					this.error = cert.GetError();
					return "";
				}
			}

			AsymmetricEncryptionAlgorithm algorithm = AsymmetricEncryptionAlgorithmUtils
					.getAsymmetricEncryptionAlgorithm(asymmetricEncryptionAlgorithm, this.error);
			try
			{
				return doEncrypt(algorithm, hash, padding, asymKey, plainText);
			}
			catch (InvalidCipherTextException e)
			{
				this.error.setError("AE036", string.Format("Algoritmo inválido {0}", algorithm));
				logger.Error(method, e);

				return "";
			}
		}


		/// <summary>
		/// Decrypts the base64 encoded encrypted text
		/// </summary>
		/// <param name="asymmetricEncryptionAlgorithm">string AsymmetricEncryptionAlgorithm enum, algorithm name</param>
		/// <param name="hashAlgorithm">string HashAlgorithm enum, algorithm name</param>
		/// <param name="asymmetricEncryptionPadding">string AsymmetricEncryptionPadding enum, padding name</param>
		/// <param name="keyPath">string path to key/certificate</param>
		/// <param name="isPrivate">boolean true if key is private, false if it is public</param>
		/// <param name="alias">string keystore/certificate pkcs12 format alias</param>
		/// <param name="password">Srting keysore/certificate pkcs12 format alias</param>
		/// <param name="encryptedInput"></param>
		/// <returns>string decypted encryptedInput text</returns>
		private string DoDecryptInternal(string hashAlgorithm, string asymmetricEncryptionPadding, Key key, bool isPrivate, string encryptedInput, bool isPublicKey)
		{
			string method = "DoDecryptInternal";
			logger.Debug(method);
			this.error.cleanError();

			HashAlgorithm hash = HashAlgorithmUtils.getHashAlgorithm(hashAlgorithm, this.error);
			AsymmetricEncryptionPadding padding = AsymmetricEncryptionPaddingUtils.getAsymmetricEncryptionPadding(asymmetricEncryptionPadding, this.error);
			if (this.error.existsError())
			{
				return "";
			}
			string asymmetricEncryptionAlgorithm = "";
			AsymmetricKeyParameter asymKey = null;

			if (isPrivate)
			{
				PrivateKeyManager keyMan = (PrivateKeyManager)key;
				if (!keyMan.HasPrivateKey || keyMan.HasError())
				{
					this.error = keyMan.GetError();
					return "";
				}
				asymmetricEncryptionAlgorithm = keyMan.getAlgorithm();

				asymKey = keyMan.getAsymmetricKeyParameter();
				if (keyMan.HasError())
				{
					this.error = keyMan.GetError();
					return "";
				}
			}
			else
			{
				PublicKey cert = isPublicKey ? (PublicKey)key : (CertificateX509)key;
				if (cert.HasError())
				{
					this.error = cert.GetError();
					return "";
				}
				asymmetricEncryptionAlgorithm = cert.getAlgorithm();
				asymKey = cert.getAsymmetricKeyParameter();
				if (cert.HasError())
				{
					this.error = cert.GetError();
					return "";
				}
			}

			AsymmetricEncryptionAlgorithm algorithm = AsymmetricEncryptionAlgorithmUtils
					.getAsymmetricEncryptionAlgorithm(asymmetricEncryptionAlgorithm, this.error);


			try
			{
				this.error.cleanError();
				return doDecrypt(algorithm, hash, padding, asymKey, encryptedInput);
			}
			catch (InvalidCipherTextException e)
			{
				this.error.setError("AE039", string.Format("Algoritmo inválido {0} ", algorithm));
				logger.Error(method, e);
				throw new InvalidCipherTextException(string.Format("Algoritmo inválido {0} ", algorithm), e);
			}
		}

		/// <summary>
		/// Decrypts the base64 encoded encrypted text 
		/// </summary>
		/// <param name="asymmetricEncryptionAlgorithm">string AsymmetricEncryptionAlgorithm enum, algorithm name</param>
		/// <param name="hashAlgorithm">string HashAlgorithm enum, algorithm name</param>
		/// <param name="asymmetricEncryptionPadding">string AsymmetricEncryptionPadding enum, padding name</param>
		/// <param name="asymmetricKeyParameter">AsymmetricKeyParameter with loaded key for specified algorithm</param>
		/// <param name="encryptedInput">string Base64 to decrypt</param>
		/// <returns>string decypted encryptedInput text</returns>
		private string doDecrypt(AsymmetricEncryptionAlgorithm asymmetricEncryptionAlgorithm, HashAlgorithm hashAlgorithm, AsymmetricEncryptionPadding asymmetricEncryptionPadding, AsymmetricKeyParameter asymmetricKeyParameter, string encryptedInput)
		{
			logger.Debug("doDecrypt");
			IAsymmetricBlockCipher asymEngine = getEngine(asymmetricEncryptionAlgorithm);
			IDigest hash = getDigest(hashAlgorithm);
			IAsymmetricBlockCipher cipher = getPadding(asymEngine, hash, asymmetricEncryptionPadding);
			BufferedAsymmetricBlockCipher bufferedCipher = new BufferedAsymmetricBlockCipher(cipher);
			if (this.error.existsError())
			{
				return "";
			}
			bufferedCipher.Init(false, asymmetricKeyParameter);
			byte[] inputBytes = Base64.Decode(encryptedInput);
			bufferedCipher.ProcessBytes(inputBytes, 0, inputBytes.Length);
			byte[] outputBytes = bufferedCipher.DoFinal();
			if (outputBytes == null || outputBytes.Length == 0)
			{
				this.error.setError("AE040", "Asymmetric decryption error");
				logger.Error("Asymmetric decryption error");
				return "";
			}
			EncodingUtil eu = new EncodingUtil();
			this.error = eu.GetError();
			return eu.getString(outputBytes);


		}
		/// <summary>
		/// Encrypts the string encoded plain text
		/// </summary>
		/// <param name="asymmetricEncryptionAlgorithm"></param>
		/// <param name="hashAlgorithm"></param>
		/// <param name="asymmetricEncryptionPadding"></param>
		/// <param name="asymmetricKeyParameter"></param>
		/// <param name="plainText"></param>
		/// <returns>Base64 encrypted encryptedInput text</returns>
		private string doEncrypt(AsymmetricEncryptionAlgorithm asymmetricEncryptionAlgorithm, HashAlgorithm hashAlgorithm, AsymmetricEncryptionPadding asymmetricEncryptionPadding, AsymmetricKeyParameter asymmetricKeyParameter, string plainText)
		{
			logger.Debug("doEncrypt");
			IAsymmetricBlockCipher asymEngine = getEngine(asymmetricEncryptionAlgorithm);
			IDigest hash = getDigest(hashAlgorithm);
			IAsymmetricBlockCipher cipher = getPadding(asymEngine, hash, asymmetricEncryptionPadding);
			BufferedAsymmetricBlockCipher bufferedCipher = new BufferedAsymmetricBlockCipher(cipher);
			if (this.error.existsError())
			{
				return "";
			}
			bufferedCipher.Init(true, asymmetricKeyParameter);
			EncodingUtil eu = new EncodingUtil();
			byte[] inputBytes = eu.getBytes(plainText);
			if (eu.GetError().existsError())
			{
				this.error = eu.GetError();
				return "";
			}
			bufferedCipher.ProcessBytes(inputBytes, 0, inputBytes.Length);
			byte[] outputBytes = bufferedCipher.DoFinal();
			if (outputBytes == null || outputBytes.Length == 0)
			{
				this.error.setError("AE041", "Asymmetric encryption error");
				logger.Error("Asymmetric encryption error");
				return "";
			}
			this.error.cleanError();
			return Base64.ToBase64String(outputBytes);

		}
		/// <summary>
		/// Build asymmetric block cipher engine
		/// </summary>
		/// <param name="asymmetricEncryptionAlgorithm">AsymmetricEncryptionAlgorithm enum, algorithm name</param>
		/// <returns>IAsymmetricBlockCipher Engine for the specified algorithm</returns>
		private IAsymmetricBlockCipher getEngine(AsymmetricEncryptionAlgorithm asymmetricEncryptionAlgorithm)
		{
			logger.Debug("getEngine");
			switch (asymmetricEncryptionAlgorithm)
			{
				case AsymmetricEncryptionAlgorithm.RSA:
					return new RsaEngine();
				default:
					this.error.setError("AE042", "Unrecognized algorithm");
					logger.Error("Unrecognized algorithm");
					return null;
			}

		}
		/// <summary>
		/// Build Digest engine for asymmetric block cipher and signing
		/// </summary>
		/// <param name="hashAlgorithm">HashAlgorithm enum, algorithm name</param>
		/// <returns>IDigest Engine for the specified algorithm</returns>
		private IDigest getDigest(HashAlgorithm hashAlgorithm)
		{
			logger.Debug("getDigest");
			Hashing hash = new Hashing();
			IDigest digest = hash.createHash(hashAlgorithm);
			if (digest == null)
			{
				this.error.setError("AE043", "Unrecognized HashAlgorithm");
				logger.Error("Unrecognized HashAlgorithm");
				return null;
			}
			return digest;
		}
		/// <summary>
		/// Buils Asymmetric Block Cipher engine
		/// </summary>
		/// <param name="asymBlockCipher">AsymmetricBlockCipher enum, algorithm name</param>
		/// <param name="hash">Digest Engine for hashing</param>
		/// <param name="asymmetricEncryptionPadding">AsymmetricEncryptionPadding enum, padding name</param>
		/// <returns>AsymmetricBlockCipher Engine specific for the algoritm, hash and padding</returns>
		private IAsymmetricBlockCipher getPadding(IAsymmetricBlockCipher asymBlockCipher, IDigest hash, AsymmetricEncryptionPadding asymmetricEncryptionPadding)
		{
			logger.Debug("getPadding");
			switch (asymmetricEncryptionPadding)
			{
				case AsymmetricEncryptionPadding.NOPADDING:
					return null;
				case AsymmetricEncryptionPadding.OAEPPADDING:
					if (hash != null)
					{
						return new OaepEncoding(asymBlockCipher, hash);
					}
					else
					{
						return new OaepEncoding(asymBlockCipher);
					}
				case AsymmetricEncryptionPadding.PCKS1PADDING:
					return new Pkcs1Encoding(asymBlockCipher);
				case AsymmetricEncryptionPadding.ISO97961PADDING:
					return new ISO9796d1Encoding(asymBlockCipher);
				default:
					error.setError("AE044", "Unrecognized AsymmetricEncryptionPadding");
					logger.Error("Unrecognized AsymmetricEncryptionPadding");
					return null;
			}
		}
	}
}
