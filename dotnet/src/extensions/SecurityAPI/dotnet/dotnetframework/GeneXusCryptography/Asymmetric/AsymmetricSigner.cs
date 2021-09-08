
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
			/******** INPUT VERIFICATION - BEGIN ********/
			if (key == null)
			{
				error.setError("AE001", "Private key cannot be null");
				return "";
			}
			if (hashAlgorithm == null || hashAlgorithm.Length == 0 || SecurityUtils.compareStrings("", hashAlgorithm))
			{
				error.setError("AE002", "HashAlgorithm cannot be empty value; use HashAlgorithm domain");
				return "";
			}
			if (plainText == null || plainText.Length == 0 || SecurityUtils.compareStrings("", plainText))
			{
				error.setError("AE003", "The plainText value to sign cannot be empty");
				return "";
			}
			/******** INPUT VERIFICATION - END ********/


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
			/******** INPUT VERIFICATION - BEGIN ********/
			if (key == null)
			{
				error.setError("AE005", "Private key cannot be null");
				return "";
			}
			if (hashAlgorithm == null || hashAlgorithm.Length == 0 || SecurityUtils.compareStrings("", hashAlgorithm))
			{
				error.setError("AE006", "HashAlgorithm cannot be empty value; use HashAlgorithm domain");
				return "";
			}
			if (path == null || path.Length == 0 || SecurityUtils.compareStrings("", path))
			{
				error.setError("AE007", "The path value of the file to sign cannot be empty");
				return "";
			}
			/******** INPUT VERIFICATION - END ********/

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
			/******** INPUT VERIFICATION - BEGIN ********/
			if (cert == null)
			{
				error.setError("AE009", "Certificate cannot be null");
				return false;
			}
			if (plainText == null || plainText.Length == 0 || SecurityUtils.compareStrings("", plainText))
			{
				error.setError("AE010", "The plainText value to verify cannot be empty");
				return false;
			}
			if (signature == null || signature.Length == 0 || SecurityUtils.compareStrings("", signature))
			{
				error.setError("AE011", "The signature value to verify cannot be empty");
				return false;
			}
			/******** INPUT VERIFICATION - END ********/


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
				result = Verify(cert, inputStream, signature);
			}
			return result;
		}

		[SecuritySafeCritical]
		public bool DoVerifyFile(CertificateX509 cert, string path, string signature)
		{
			/******** INPUT VERIFICATION - BEGIN ********/
			if (cert == null)
			{
				error.setError("AE013", "Certificate cannot be null");
				return false;
			}
			if (path == null || path.Length == 0 || SecurityUtils.compareStrings("", path))
			{
				error.setError("AE014", "The path value of the faile to verify cannot be empty");
				return false;
			}
			if (signature == null || signature.Length == 0 || SecurityUtils.compareStrings("", signature))
			{
				error.setError("AE015", "The signature value to verify cannot be empty");
				return false;
			}
			/******** INPUT VERIFICATION - END ********/

			bool result = false;
			using (Stream input = SecurityUtils.getFileStream(path, this.error))
			{
				if (this.HasError())
				{
					return false;
				}
				result = Verify(cert, input, signature);
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
					.GetAsymmetricSigningAlgorithm(keyMan.getPrivateKeyAlgorithm(), this.error);
			if (this.HasError()) return "";
			ISigner signer = AsymmetricSigningAlgorithmUtils.GetSigner(asymmetricSigningAlgorithm, GetHash(hashAlgorithm),
					this.error);
			if (this.HasError()) return "";
			SetUpSigner(signer, input, keyMan.getPrivateKeyParameterForSigning(), true);
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

		private bool Verify(Certificate certificate, Stream input, string signature)
		{
			CertificateX509 cert = (CertificateX509)certificate;
			if (cert.HasError())
			{
				this.error = cert.GetError();
				return false;
			}
			string hashAlgorithm = "";
			if (SecurityUtils.compareStrings(cert.getPublicKeyHash(), "ECDSA"))
			{
				hashAlgorithm = "SHA1";
			}
			else
			{
				hashAlgorithm = cert.getPublicKeyHash();
			}
			AsymmetricSigningAlgorithm asymmetricSigningAlgorithm = AsymmetricSigningAlgorithmUtils
					.GetAsymmetricSigningAlgorithm(cert.getPublicKeyAlgorithm(), this.error);
			if (this.HasError()) return false;
			ISigner signer = AsymmetricSigningAlgorithmUtils.GetSigner(asymmetricSigningAlgorithm, GetHash(hashAlgorithm),
					this.error);
			if (this.HasError()) return false;
			SetUpSigner(signer, input, cert.getPublicKeyParameterForSigning(), false);
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
