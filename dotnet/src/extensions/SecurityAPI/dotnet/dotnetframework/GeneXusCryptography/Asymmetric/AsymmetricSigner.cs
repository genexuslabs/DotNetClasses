
using GeneXusCryptography.AsymmetricUtils;
using GeneXusCryptography.Hash;
using GeneXusCryptography.HashUtils;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.Encoders;
using GeneXusCryptography.Commons;
using System;
using System.Security;
using SecurityAPICommons.Utils;
using System.IO;

namespace GeneXusCryptography.Asymmetric
{
	/// <summary>
	/// Implements Asymmetric Signer engines and methods to sign and verify signatures
	/// </summary>
	[SecuritySafeCritical]
	public class AsymmetricSigner : SecurityAPIObject, IAsymmetricSignerObject
	{


		/// <summary>
		/// AsymmetricSigner class constructor
		/// </summary>
		public AsymmetricSigner() : base()
		{

		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

		[SecuritySafeCritical]
		public String DoSign(PrivateKeyManager key, string hashAlgorithm, string plainText)
		{
			this.error.cleanError();
			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateObjectInput("key", key, this.error);
			SecurityUtils.validateStringInput("hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			if (this.HasError()) { return ""; };
			/*******INPUT VERIFICATION - END*******/


			EncodingUtil eu = new EncodingUtil();
			byte[] inputText = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}
			string result = "";
			using (Stream inputStream = new MemoryStream(inputText))
			{
				result = Sign(key, hashAlgorithm, inputStream);
			}
			return result;
		}

		[SecuritySafeCritical]
		public String DoSignFile(PrivateKeyManager key, string hashAlgorithm, string path)
		{
			this.error.cleanError();


			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateObjectInput("key", key, this.error);
			SecurityUtils.validateStringInput("hashAlgorithm", hashAlgorithm, this.error);
			SecurityUtils.validateStringInput("path", path, this.error);
			if (this.HasError()) { return ""; }
			/*******INPUT VERIFICATION - END*******/

			string result = "";
			using (Stream input = SecurityUtils.getFileStream(path, this.error))
			{
				if (this.HasError())
				{
					return "";
				}

				result = Sign(key, hashAlgorithm, input);
			}
			return result;
		}

		[SecuritySafeCritical]
		public bool DoVerify(CertificateX509 cert, string plainText, string signature)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateObjectInput("cert", cert, this.error);
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			SecurityUtils.validateStringInput("signature", signature, this.error);
			if (this.HasError()) { return false; }
			/*******INPUT VERIFICATION - END*******/


			EncodingUtil eu = new EncodingUtil();
			byte[] inputText = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return false;
			}
			bool result = false;
			using (Stream inputStream = new MemoryStream(inputText))
			{
				result = Verify(cert, inputStream, signature, null);
			}
			return result;
		}

		[SecuritySafeCritical]
		public bool DoVerifyWithPublicKey(PublicKey key, string plainText, string signature, string hash)
		{
			this.error.cleanError();

			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput("key", key, this.error);
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			SecurityUtils.validateStringInput("signature", signature, this.error);
			SecurityUtils.validateStringInput("hashAlgorithm", hash, this.error);
			if (this.HasError())
			{
				return false;
			}
			/******* INPUT VERIFICATION - END *******/

			EncodingUtil eu = new EncodingUtil();
			byte[] inputText = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return false;
			}
			bool result = false;
			using (Stream inputStream = new MemoryStream(inputText))
			{
				result = Verify(key, inputStream, signature, hash);
			}
			return result;
		}

		[SecuritySafeCritical]
		public bool DoVerifyFile(CertificateX509 cert, string path, string signature)
		{
			this.error.cleanError();

			/*******INPUT VERIFICATION - BEGIN*******/
			SecurityUtils.validateObjectInput("cert", cert, this.error);
			SecurityUtils.validateStringInput("path", path, this.error);
			SecurityUtils.validateStringInput("signature", signature, this.error);
			if (this.HasError()) { return false; }
			/*******INPUT VERIFICATION - END*******/

			bool result = false;
			using (Stream input = SecurityUtils.getFileStream(path, this.error))
			{
				if (this.HasError())
				{
					return false;
				}
				result = Verify(cert, input, signature, null);
			}
			return result;
		}

		[SecuritySafeCritical]
		public bool DoVerifyFileWithPublicKey(PublicKey key, string path, string signature, string hash)
		{
			this.error.cleanError();

			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput("key", key, this.error);
			SecurityUtils.validateStringInput("path", path, this.error);
			SecurityUtils.validateStringInput("signature", signature, this.error);
			SecurityUtils.validateStringInput("hashAlgorithm", hash, this.error);
			if (this.HasError())
			{
				return false;
			}
			/******* INPUT VERIFICATION - END *******/

			bool result = false;
			using (Stream input = SecurityUtils.getFileStream(path, this.error))
			{
				if (this.HasError())
				{
					return false;
				}
				result = Verify(key, input, signature, hash);
			}
			return result;
		}

		/********EXTERNAL OBJECT PUBLIC METHODS  - END ********/

		private String Sign(PrivateKey key, string hashAlgorithm, Stream input)
		{
			PrivateKeyManager keyMan = (PrivateKeyManager)key;
			if (keyMan.HasError())
			{
				this.error = keyMan.GetError();
				return "";
			}
			AsymmetricSigningAlgorithm asymmetricSigningAlgorithm = AsymmetricSigningAlgorithmUtils
					.GetAsymmetricSigningAlgorithm(keyMan.getAlgorithm(), this.error);
			if (this.HasError()) return "";
			ISigner signer = AsymmetricSigningAlgorithmUtils.GetSigner(asymmetricSigningAlgorithm, GetHash(hashAlgorithm),
					this.error);
			if (this.HasError()) return "";
			SetUpSigner(signer, input, keyMan.getAsymmetricKeyParameter(), true);
			if (this.HasError()) return "";
			byte[] outputBytes = null;
			try
			{
				outputBytes = signer.GenerateSignature();
			}
			catch (Exception e)
			{
				error.setError("AE01", e.Message);
				return "";
			}
			String result = "";
			try
			{
				result = Base64.ToBase64String(outputBytes);
			}
			catch (Exception e)
			{
				error.setError("AE018", e.Message);
				return "";
			}
			return result;
		}

		private bool Verify(Key key, Stream input, string signature, string hash)
		{
			PublicKey cert = null;
			bool isKey = false;
			if (hash == null)
			{
				cert = (CertificateX509)key;
			}
			else
			{
				cert = (PublicKey)key;
				isKey = true;
			}
			if (cert.HasError())
			{
				this.error = cert.GetError();
				return false;
			}
			string hashAlgorithm = "";
			if (isKey)
			{
				hashAlgorithm = hash;
			}
			else
			{
				if (SecurityUtils.compareStrings(((CertificateX509)cert).getPublicKeyHash(), "ECDSA"))
				{
					hashAlgorithm = "SHA1";
				}
				else
				{
					hashAlgorithm = ((CertificateX509)cert).getPublicKeyHash();
				}
			}
			AsymmetricSigningAlgorithm asymmetricSigningAlgorithm = AsymmetricSigningAlgorithmUtils
					.GetAsymmetricSigningAlgorithm(cert.getAlgorithm(), this.error);
			if (this.HasError()) return false;
			ISigner signer = AsymmetricSigningAlgorithmUtils.GetSigner(asymmetricSigningAlgorithm, GetHash(hashAlgorithm),
					this.error);
			if (this.HasError()) return false;
			SetUpSigner(signer, input, cert.getAsymmetricKeyParameter(), false);
			if (this.HasError()) return false;
			byte[] signatureBytes = null;
			try
			{
				signatureBytes = Base64.Decode(signature);
			}
			catch (Exception e)
			{
				error.setError("AE019", e.Message);
				return false;
			}

			if (signatureBytes == null || signatureBytes.Length == 0)
			{
				this.error.setError("AE020", "Error reading signature");
				return false;
			}
			bool result = false;
			try
			{
				result = signer.VerifySignature(signatureBytes);
			}
			catch (Exception e)
			{
				error.setError("AE021", e.Message);
				return false;
			}
			return result;

		}

		private void SetUpSigner(ISigner signer, Stream input, AsymmetricKeyParameter asymmetricKeyParameter,
			bool toSign)
		{
			try
			{
				signer.Init(toSign, asymmetricKeyParameter);
			}
			catch (Exception e)
			{
				error.setError("AE022", e.Message);
				return;
			}
			byte[] buffer = new byte[8192];
			int n;
			try
			{
				while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					signer.BlockUpdate(buffer, 0, n);
				}
			}
			catch (Exception e)
			{
				error.setError("AE023", e.Message);
				return;
			}
		}

		private IDigest GetHash(string hashAlgorithm)
		{
			HashAlgorithm hash = HashAlgorithmUtils.getHashAlgorithm(hashAlgorithm, this.error);
			if (this.HasError())
			{
				return null;
			}
			Hashing hashing = new Hashing();
			IDigest digest = hashing.createHash(hash);
			if (hashing.HasError())
			{
				this.error = hashing.GetError();
				return null;
			}
			return digest;
		}
	}
}
