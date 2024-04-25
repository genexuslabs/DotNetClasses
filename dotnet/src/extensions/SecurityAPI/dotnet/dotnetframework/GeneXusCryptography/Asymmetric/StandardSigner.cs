using System;
using System.Collections.Generic;
using System.Security;
using GeneXusCryptography.AsymmetricUtils;
using GeneXusCryptography.Commons;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Config;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
using Org.BouncyCastle.Utilities.Collections;


namespace GeneXusCryptography.Asymmetric
{
	[SecuritySafeCritical]
	public class StandardSigner : SecurityAPIObject, IStandardSignerObject
	{

		public StandardSigner() : base()
		{

		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - BEGIN ********/

		[SecuritySafeCritical]
		public string Sign(string plainText, SignatureStandardOptions options)
		{
			this.error.cleanError();

			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput("signatureStandardOptions", options, this.error);
			SecurityUtils.validateObjectInput("private key", options.GetPrivateKey(), this.error);
			SecurityUtils.validateObjectInput("certificate", options.GetCertificate(), this.error);
			SecurityUtils.validateStringInput("plainText", plainText, this.error);
			if (this.HasError())
			{
				return "";
			}

			/******* INPUT VERIFICATION - END *******/

			EncodingUtil eu = new EncodingUtil();
			byte[] inputText = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return "";
			}

			string result = "";
			try
			{
				result = Sign_internal(inputText, options.GetPrivateKey(), options.GetCertificate(), options.GetSignatureStandard(), options.GetEncapsulated());
			}
			catch (Exception e)
			{
				error.setError("SS002", e.Message);
				result = "";
			}

			return result;
		}

		[SecuritySafeCritical]

		public bool Verify(string signed, string plainText, SignatureStandardOptions options)
		{
			this.error.cleanError();

			/******* INPUT VERIFICATION - BEGIN *******/
			SecurityUtils.validateObjectInput("signatureStandardOptions", options, this.error);
			//SecurityUtils.validateStringInput("plainText", plainText, this.error);
			SecurityUtils.validateStringInput("signed", signed, this.error);
			if (this.HasError())
			{
				return false;
			}


			/******* INPUT VERIFICATION - END *******/

			EncodingUtil eu = new EncodingUtil();
			byte[] plainText_bytes = eu.getBytes(plainText);
			if (eu.HasError())
			{
				this.error = eu.GetError();
				return false;
			}

			bool result = false;
			try
			{
				result = Verify_internal(Base64.Decode(signed), plainText_bytes, options.GetEncapsulated());
			}
			catch (Exception e)
			{
				error.setError("SS002", e.Message);
				result = false;
			}

			return result;
		}

		/******** EXTERNAL OBJECT PUBLIC METHODS - END ********/

		private string Sign_internal(byte[] input, PrivateKeyManager key, CertificateX509 cert, SignatureStandard signatureStandard, bool encapsulated)
		{

			PrivateKeyManager keyMan = (PrivateKeyManager)key;
			if (keyMan.HasError())
			{
				this.error = keyMan.GetError();
				return "";
			}
			CertificateX509 certificate = (CertificateX509)cert;
			if (certificate.HasError())
			{

				this.error = certificate.GetError();
				return "";
			}
			AsymmetricSigningAlgorithm asymmetricSigningAlgorithm = AsymmetricSigningAlgorithmUtils
					.GetAsymmetricSigningAlgorithm(keyMan.getAlgorithm(), this.error);
			string encryptAlg = AsymmetricSigningAlgorithmUtils.GetCMSSigningAlgortithm(asymmetricSigningAlgorithm, this.error);
			if (this.HasError()) { return ""; }

			Org.BouncyCastle.X509.X509Certificate cert2 = DotNetUtilities.FromX509Certificate(certificate.Cert);

			CmsSignedDataGenerator generator = new CmsSignedDataGenerator();
			string digest = asymmetricSigningAlgorithm == AsymmetricSigningAlgorithm.ECDSA ? CmsSignedGenerator.DigestSha1 : DigestCalculator(certificate);

			generator.AddSigner(keyMan.getAsymmetricKeyParameter(), cert2, encryptAlg, digest);

			List<Org.BouncyCastle.X509.X509Certificate> certList = new List<Org.BouncyCastle.X509.X509Certificate>();
			certList.Add(cert2);

			IStore<Org.BouncyCastle.X509.X509Certificate> certStore = CollectionUtilities.CreateStore(certList);

			generator.AddCertificates(certStore);

			CmsSignedData signedData = generator.Generate(new CmsProcessableByteArray(input), encapsulated);


			return Base64.ToBase64String(signedData.GetEncoded());

		}


		private bool Verify_internal(byte[] cmsSignedData, byte[] data, bool encapsulated)
		{
			CmsSignedData cms = encapsulated ? new CmsSignedData(cmsSignedData) : new CmsSignedData(new CmsProcessableByteArray(data), cmsSignedData);

			SignerInformationStore signers = cms.GetSignerInfos();

			IStore<Org.BouncyCastle.X509.X509Certificate> certificates = cms.GetCertificates();
			var signerInfos = signers.GetSigners();
			foreach (SignerInformation signer in signerInfos)
			{
				var certCollection = certificates.EnumerateMatches(signer.SignerID);
				var certEnum = certCollection.GetEnumerator();

				certEnum.MoveNext();
				Org.BouncyCastle.X509.X509Certificate cert = certEnum.Current;
				var publicKey = cert.GetPublicKey();
				bool res = false;

				res = signer.Verify(publicKey);

				if (!res)
				{
					return false;
				}
			}
			return true;
		}



		private string DigestCalculator(CertificateX509 cert)
		{
			string value = cert.getPublicKeyHash();
			switch (value)
			{
				case "SHA1":
					return CmsSignedGenerator.DigestSha1;
				case "SHA256":
					return CmsSignedGenerator.DigestSha256;
				case "SHA512":
					return CmsSignedGenerator.DigestSha512;
				default:
					this.error.setError("SS003", "Unrecognizable certificate hash algorithm");
					return "";
			}

		}

	}
}
