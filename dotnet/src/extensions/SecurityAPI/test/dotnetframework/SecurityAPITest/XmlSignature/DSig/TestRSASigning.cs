using SecurityAPITest.SecurityAPICommons.commons;
using NUnit.Framework;
using GeneXusXmlSignature.GeneXusCommons;
using SecurityAPICommons.Commons;
using SecurityAPICommons.Keys;
using GeneXusXmlSignature.GeneXusUtils;
using GeneXusXmlSignature.GeneXusDSig;
using SecurityAPICommons.Config;
using System.IO;

namespace SecurityAPITest.XmlSignature.DSig
{
    [TestFixture]
    public class TestRSASigning: SecurityAPITestObject

    {
		private static string path_RSA_sha1_1024;
		private static string path_RSA_sha256_1024;
		private static string path_RSA_sha256_2048;
		private static string path_RSA_sha512_2048;

		private static string xmlUnsigned;
		private static string xmlUnsignedPath;
		private static string xmlSignedPathRoot;

		private static string alias;
		private static string password;

		private static string dSigType;

		private static string[] arrayCanonicalization;
		private static string[] arrayKeyInfoType;

		private static DSigOptions options;
		private static DSigOptions optionsXPath;
		private static DSigOptions optionsID;

		private static Error error;

#if !NETCORE
		private static string xmlUnsignedXPathFile;
		private static string xPath;
		private static string xmlUnsignedXPath;
#endif

		private static string xmlUnsignedIDPathFile;
		private static string identifierAttribute;
		private static string id;
		private static string xmlUnsignedID;
		private static string xmlIDSchemaPath;

		private static EncodingUtil eu;

		[SetUp]
		public virtual void SetUp()
		{
			eu = new EncodingUtil();
			eu.setEncoding("UTF_8");
			path_RSA_sha1_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha1_1024");
			path_RSA_sha256_1024 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_1024");
			path_RSA_sha256_2048 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha256_2048");
			path_RSA_sha512_2048 = Path.Combine(BASE_PATH, "dummycerts", "RSA_sha512_2048");

			alias = "1";
			password = "dummy";

			xmlUnsigned = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + "<Envelope xmlns=\"http://example.org/envelope\">"
					+ "  <Body>" + "    Ola mundo" + "  </Body>" + "</Envelope>";

			xmlUnsignedPath = Path.Combine(BASE_PATH, "Temp", "toSign.xml");
			xmlSignedPathRoot = Path.Combine(BASE_PATH, "Temp", "outputTestFilesC");

			dSigType = "ENVELOPED";
			arrayCanonicalization = new string[] { "C14n_WITH_COMMENTS", "C14n_OMIT_COMMENTS", "exc_C14n_OMIT_COMMENTS",
				"exc_C14N_WITH_COMMENTS" };
			arrayKeyInfoType = new string[] { "KeyValue", "X509Certificate", "NONE" };

			options = new DSigOptions();

			optionsXPath = new DSigOptions();

			error = new Error();
#if !NETCORE
			xmlUnsignedXPathFile = Path.Combine(BASE_PATH, "Temp", "bookSample.xml");
			xPath = "/bookstore/book[1]";
#endif

			xmlUnsignedIDPathFile = Path.Combine(BASE_PATH, "Temp", "xmlID.xml");
			identifierAttribute = "id";
			id = "#tag1";
			xmlIDSchemaPath = Path.Combine(BASE_PATH, "Temp", "xmlIDSchema.xsd");

			optionsID = new DSigOptions();
			optionsID.IdentifierAttribute = identifierAttribute;

			xmlUnsignedID = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" + "<messages>\r\n" + "  <note id='tag1'>\r\n"
					+ "    <to>Tove</to>\r\n" + "    <from>Jani</from>\r\n" + "    <heading>Reminder</heading>\r\n"
					+ "    <body>Don't forget me this weekend!</body>\r\n" + "  </note>\r\n" + "  <note id='tag2'>\r\n"
					+ "    <to>Jani</to>\r\n" + "    <from>Tove</from>\r\n" + "    <heading>Re: Reminder</heading>\r\n"
					+ "    <body>I will not</body>\r\n" + "  </note>\r\n" + "</messages>";
#if !NETCORE
			xmlUnsignedXPath = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" + "<bookstore>\r\n"
					+ "<book category=\"cooking\">\r\n" + "  <title lang=\"en\">Everyday Italian</title>\r\n"
					+ "  <author>Giada De Laurentiis</author>\r\n" + "  <year>2005</year>\r\n"
					+ "  <price>30.00</price>\r\n" + "</book>\r\n" + "<book category=\"children\">\r\n"
					+ "  <title lang=\"en\">Harry Potter</title>\r\n" + "  <author>J K. Rowling</author>\r\n"
					+ "  <year>2005</year>\r\n" + "  <price>29.99</price>\r\n" + "</book>\r\n"
					+ "<book category=\"web\">\r\n" + "  <title lang=\"en\">XQuery Kick Start</title>\r\n"
					+ "  <author>James McGovern</author>\r\n" + "  <author>Per Bothner</author>\r\n"
					+ "  <author>Kurt Cagle</author>\r\n" + "  <author>James Linn</author>\r\n"
					+ "  <author>Vaidyanathan Nagarajan</author>\r\n" + "  <year>2003</year>\r\n"
					+ "  <price>49.99</price>\r\n" + "</book>\r\n" + "<book category=\"web\">\r\n"
					+ "  <title lang=\"en\">Learning XML</title>\r\n" + "  <author>Erik T. Ray</author>\r\n"
					+ "  <year>2003</year>\r\n" + "  <price>39.95</price>\r\n" + "</book>\r\n" + "</bookstore>";
#endif
		}

		[Test]
		public void Test_sha1_1024_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.crt");
			string pathSigned = "Test_sha1_1024_DER";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.Load(pathKey);
			bulkTest(cert, privateKey, pathSigned, false, false, false, null);

		}

		[Test]
		public void Test_sha1_1024_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.pem");
			string pathSigned = "Test_sha1_1024_PEM";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.Load(pathKey);
			bulkTest(cert, privateKey, pathSigned, false, false, false, null);
		}



		[Test]
		public void Test_sha1_1024_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.p12");
			string pathSigned = "Test_sha1_1024_PKCS12";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.LoadPKCS12(pathCert, alias, password);
			privateKey.LoadPKCS12(pathKey, alias, password);
			bulkTest(cert, privateKey, pathSigned, true, false, false, null);
		}

		[Test]
		public void Test_sha256_1024_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.crt");
			string pathSigned = "Test_sha256_1024_DER";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.Load(pathKey);
			bulkTest(cert, privateKey, pathSigned, false, false, false, null);
		}

		[Test]
		public void Test_sha256_1024_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.pem");
			string pathSigned = "Test_sha256_1024_PEM";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.Load(pathKey);
			bulkTest(cert, privateKey, pathSigned, false, false, false, null);
		}

		

		[Test]
		public void Test_sha256_1024_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.p12");
			string pathSigned = "Test_sha256_1024_PKCS12";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.LoadPKCS12(pathCert, alias, password);
			privateKey.LoadPKCS12(pathKey, alias, password);
			bulkTest(cert, privateKey, pathSigned, true, false, false, null);
		}

		[Test]
		public void Test_sha256_2048_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.crt");
			string pathSigned = "Test_sha256_2048_DER";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.Load(pathKey);
			bulkTest(cert, privateKey, pathSigned, false, false, false, null);
		}

		[Test]
		public void Test_sha256_2048_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.pem");
			string pathSigned = "Test_sha256_2048_PEM";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.Load(pathKey);
			bulkTest(cert, privateKey, pathSigned, false, false, false, null);
		}



		[Test]
		public void Test_sha256_2048_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12");
			string pathSigned = "Test_sha256_2048_PKCS12";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.LoadPKCS12(pathCert, alias, password);
			privateKey.LoadPKCS12(pathKey, alias, password);
			bulkTest(cert, privateKey, pathSigned, true, false, false, null);
		}

		[Test]
		public void Test_sha512_2048_DER()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.crt");
			string pathSigned = "Test_sha512_2048_DER";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.Load(pathKey);
			bulkTest(cert, privateKey, pathSigned, false, false, false, null);
		}

		[Test]
		public void Test_sha512_2048_PEM()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512d_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.pem");
			string pathSigned = "Test_sha512_2048_PEM";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.Load(pathKey);
			bulkTest(cert, privateKey, pathSigned, false, false, false, null);
		}

		

		[Test]
		public void Test_sha512_2048_PKCS12()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512_cert.p12");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.p12");
			string pathSigned = "Test_sha512_2048_PKCS12";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.LoadPKCS12(pathCert, alias, password);
			privateKey.LoadPKCS12(pathKey, alias, password);
			bulkTest(cert, privateKey, pathSigned, true, false, false, null);
		}




		private void bulkTest(Key key, PrivateKeyManager privateKey, string pathSigned, bool ispkcs12, bool encrypted, bool isPublicKey, string hash)
		{
			for (int k = 0; k < arrayKeyInfoType.Length; k++)
			{
				options.KeyInfoType = arrayKeyInfoType[k];
				optionsXPath.KeyInfoType = arrayKeyInfoType[k];
				optionsID.KeyInfoType = arrayKeyInfoType[k];
				

				bulkTestWithKeyInfo(key, privateKey, pathSigned, isPublicKey, hash);
				bulkTestWithKeyInfoXPath(key, privateKey, pathSigned, isPublicKey, hash);
				bulkTestWithKeyInfoID(key, privateKey, pathSigned, isPublicKey, hash);
			}

		}

		private void bulkTestWithKeyInfoXPath(Key certificate, PrivateKeyManager key, string pathSigned, bool isPublickey, string hash)
		{
#if NETCORE
			//******Net Core no tiene habilitado usar la transform xpath******//
			Assert.IsTrue(true);
#else
			XmlDSigSigner signer = new XmlDSigSigner();
			if (!(KeyInfoTypeUtils.getKeyInfoType(options.KeyInfoType, new Error()) == KeyInfoType.X509Certificate && isPublickey))
			{

				string pathSignedXPath = pathSigned + "_xPAth";
				for (int c = 0; c < arrayCanonicalization.Length; c++)
				{

					/**** TEST FILES ****/
					optionsXPath.DSigSignatureType = dSigType;
					optionsXPath.Canonicalization = arrayCanonicalization[c];
					bool signedFile = isPublickey ? signer.DoSignFileElementWithPublicKey(xmlUnsignedXPathFile, xPath, key, (PublicKey)certificate,
							Path.Combine(xmlSignedPathRoot, pathSignedXPath + ".xml"), optionsXPath, hash) : signer.DoSignFileElement(xmlUnsignedXPathFile, xPath, key, (CertificateX509)certificate,
							Path.Combine(xmlSignedPathRoot, pathSignedXPath + ".xml"), optionsXPath);
					Assert.IsTrue(signedFile);
					True(signedFile, signer);

					bool verifyFile = false;
					KeyInfoType keyInfo = KeyInfoTypeUtils.getKeyInfoType(optionsXPath.KeyInfoType, error);
					if (keyInfo != KeyInfoType.NONE)
					{
						verifyFile = signer.DoVerifyFile(Path.Combine(xmlSignedPathRoot, pathSignedXPath + ".xml"), optionsXPath);
					}
					else
					{
						verifyFile = isPublickey ? signer.DoVerifyFileWithPublicKey(Path.Combine(xmlSignedPathRoot, pathSignedXPath + ".xml"), (PublicKey)certificate,
								optionsXPath) : signer.DoVerifyFileWithCert(Path.Combine(xmlSignedPathRoot, pathSignedXPath + ".xml"), (CertificateX509)certificate,
								optionsXPath);
					}
					//True(verifyFile, signer);

					/**** TEST STRINGS ****/

					string signedString = isPublickey ? signer.DoSignElementWithPublicKey(xmlUnsignedXPath, xPath, key, (PublicKey)certificate, optionsXPath, hash) : signer.DoSignElement(xmlUnsignedXPath, xPath, key, (CertificateX509)certificate, optionsXPath);
					bool resultSignString = false;
					if (keyInfo != KeyInfoType.NONE)
					{
						resultSignString = signer.DoVerify(signedString, optionsXPath);
					}
					else
					{
						resultSignString = isPublickey ? signer.DoVerifyWithPublicKey(signedString, (PublicKey)certificate, optionsXPath) : signer.DoVerifyWithCert(signedString, (CertificateX509)certificate, optionsXPath);

					}
					//True(resultSignString, signer);

				}
			}
#endif
		}

		private void bulkTestWithKeyInfo(Key certificate, PrivateKeyManager key, string pathSigned, bool isPublickey, string hash)
		{
			XmlDSigSigner signer = new XmlDSigSigner();
			if (!(KeyInfoTypeUtils.getKeyInfoType(options.KeyInfoType, new Error()) == KeyInfoType.X509Certificate && isPublickey))
			{
				for (int c = 0; c < arrayCanonicalization.Length; c++)
				{

					/**** TEST FILES ****/
					options.DSigSignatureType = dSigType;
					options.Canonicalization = arrayCanonicalization[c];

					bool signedFile = isPublickey ? signer.DoSignFileWithPublicKey(xmlUnsignedPath, key, (PublicKey)certificate,
							Path.Combine(xmlSignedPathRoot, pathSigned + ".xml"), options, hash) : signer.DoSignFile(xmlUnsignedPath, key, (CertificateX509)certificate,
							Path.Combine(xmlSignedPathRoot, pathSigned + ".xml"), options);

					Assert.IsTrue(signedFile);
					True(signedFile, signer);

					bool verifyFile = false;
					KeyInfoType keyInfo = KeyInfoTypeUtils.getKeyInfoType(options.KeyInfoType, error);
					if (keyInfo != KeyInfoType.NONE)
					{
						verifyFile = signer.DoVerifyFile(Path.Combine(xmlSignedPathRoot, pathSigned + ".xml"), options);
					}
					else
					{
						verifyFile = isPublickey ? signer.DoVerifyFileWithPublicKey(Path.Combine(xmlSignedPathRoot, pathSigned + ".xml"), (PublicKey)certificate, options) : signer.DoVerifyFileWithCert(Path.Combine(xmlSignedPathRoot, pathSigned + ".xml"), (CertificateX509)certificate, options);
					}
					True(verifyFile, signer);

					/**** TEST STRINGS ****/

					string signedString = isPublickey ? signer.DoSignWithPublicKey(xmlUnsigned, key, (PublicKey)certificate, options, hash) : signer.DoSign(xmlUnsigned, key, (CertificateX509)certificate, options);
					bool resultSignString = false;
					if (keyInfo != KeyInfoType.NONE)
					{
						resultSignString = signer.DoVerify(signedString, options);
					}
					else
					{
						resultSignString = isPublickey ? signer.DoVerifyWithPublicKey(signedString, (PublicKey)certificate, options) : signer.DoVerifyWithCert(signedString, (CertificateX509)certificate, options);

					}
					True(resultSignString, signer);

				}
			}
		}

		private void bulkTestWithKeyInfoID(Key certificate, PrivateKeyManager key, string pathSigned, bool isPublickey, string hash)
		{
			XmlDSigSigner signer = new XmlDSigSigner();
			if (!(KeyInfoTypeUtils.getKeyInfoType(options.KeyInfoType, new Error()) == KeyInfoType.X509Certificate && isPublickey))
			{
				string pathSignedID = pathSigned + "_id";
				for (int c = 0; c < arrayCanonicalization.Length; c++)
				{

					/**** TEST FILES ****/
					optionsID.DSigSignatureType = dSigType;
					optionsID.Canonicalization = arrayCanonicalization[c];

					optionsID.XmlSchemaPath = xmlIDSchemaPath;
					bool signedFile = isPublickey ? signer.DoSignFileElementWithPublicKey(xmlUnsignedIDPathFile, id, key, (PublicKey)certificate,
							Path.Combine(xmlSignedPathRoot, pathSignedID + ".xml"), optionsID, hash) : signer.DoSignFileElement(xmlUnsignedIDPathFile, id, key, (CertificateX509)certificate,
							Path.Combine(xmlSignedPathRoot, pathSignedID + ".xml"), optionsID);
					Assert.IsTrue(signedFile);
					True(signedFile, signer);

					bool verifyFile = false;
					optionsID.XmlSchemaPath = "";
					KeyInfoType keyInfo = KeyInfoTypeUtils.getKeyInfoType(optionsID.KeyInfoType, error);
					if (keyInfo != KeyInfoType.NONE)
					{

						verifyFile = signer.DoVerifyFile(Path.Combine(xmlSignedPathRoot, pathSignedID + ".xml"), optionsID);
					}
					else
					{
						verifyFile = isPublickey ? signer.DoVerifyFileWithPublicKey(Path.Combine(xmlSignedPathRoot, pathSignedID + ".xml"), (PublicKey)certificate,
								optionsID) : signer.DoVerifyFileWithCert(Path.Combine(xmlSignedPathRoot, pathSignedID + ".xml"), (CertificateX509)certificate,
								optionsID);
					}
					True(verifyFile, signer);

					/**** TEST STRINGS ****/
					optionsID.XmlSchemaPath = xmlIDSchemaPath;
					string signedString = isPublickey ? signer.DoSignElementWithPublicKey(xmlUnsignedID, id, key, (PublicKey)certificate, optionsID, hash) : signer.DoSignElement(xmlUnsignedID, id, key, (CertificateX509)certificate, optionsID);
					bool resultSignString = false;

					optionsID.XmlSchemaPath = "";
					if (keyInfo != KeyInfoType.NONE)
					{
						resultSignString = signer.DoVerify(signedString, optionsID);
					}
					else
					{
						resultSignString = isPublickey ? signer.DoVerifyWithPublicKey(signedString, (PublicKey)certificate, optionsID) : signer.DoVerifyWithCert(signedString, (CertificateX509)certificate, optionsID);

					}
					Assert.IsTrue(resultSignString);
					True(resultSignString, signer);

				}
			}
		}

		[Test]
		public void Test_sha256_1024_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha256_1024, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_1024, "sha256_cert.pem");
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.LoadEncrypted(pathKey, password);
			string pathSigned = "Test_sha256_1024_PEM";
			bulkTest(cert, privateKey, pathSigned, false, true, false, null);
		}

		[Test]
		public void Test_sha1_1024_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1_key.pem");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_cert.pem");
			string pathSigned = "Test_sha1_1024_PEM";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.LoadEncrypted(pathKey, password);
			bulkTest(cert, privateKey, pathSigned, false, true, false, null);
		}

		[Test]
		public void Test_sha256_2048_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_cert.pem");
			string pathSigned = "Test_sha256_2048_PEM";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.LoadEncrypted(pathKey, password);
			bulkTest(cert, privateKey, pathSigned, false, true, false, null);
		}

		[Test]
		public void Test_sha512_2048_PEM_Encrypted()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_cert.pem");
			string pathSigned = "Test_sha512_2048_PEM";
			CertificateX509 cert = new CertificateX509();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.LoadEncrypted(pathKey, password);
			bulkTest(cert, privateKey, pathSigned, false, true, false, null);
		}

		[Test]
		public void Test_sha1_1024_PEM_PublicKey()
		{
			string pathKey = Path.Combine(path_RSA_sha1_1024, "sha1_key.pem");
			string pathCert = Path.Combine(path_RSA_sha1_1024, "sha1_pubkey.pem");
			string pathSigned = "Test_sha1_1024_PublicKey";
			PublicKey publicKey = new PublicKey();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			publicKey.Load(pathCert);
			privateKey.LoadEncrypted(pathKey, password);
			bulkTest(publicKey, privateKey, pathSigned, false, true, true, "SHA1");
		}

		[Test]
		public void Test_sha256_2048_PEM_PublicKey()
		{
			string pathKey = Path.Combine(path_RSA_sha256_2048, "sha256_key.pem");
			string pathCert = Path.Combine(path_RSA_sha256_2048, "sha256_pubkey.pem");
			string pathSigned = "Test_sha256_2048_PublicKey";
			PublicKey cert = new PublicKey();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.LoadEncrypted(pathKey, password);
			bulkTest(cert, privateKey, pathSigned, false, true, true, "SHA256");
		}

		[Test]
		public void Test_sha512_2048_PEM_PublicKey()
		{
			string pathKey = Path.Combine(path_RSA_sha512_2048, "sha512_key.pem");
			string pathCert = Path.Combine(path_RSA_sha512_2048, "sha512_pubkey.pem");
			string pathSigned = "Test_sha512_2048_PublicKey";
			PublicKey cert = new PublicKey();
			PrivateKeyManager privateKey = new PrivateKeyManager();
			cert.Load(pathCert);
			privateKey.LoadEncrypted(pathKey, password);
			bulkTest(cert, privateKey, pathSigned, false, true, true, "SHA512");
		}


	}
}
