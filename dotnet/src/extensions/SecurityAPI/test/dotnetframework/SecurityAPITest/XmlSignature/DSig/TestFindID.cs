using GeneXusXmlSignature.GeneXusCommons;
using GeneXusXmlSignature.GeneXusDSig;
using NUnit.Framework;
using SecurityAPICommons.Keys;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.XmlSignature.DSig
{
	[TestFixture]
	public class TestFindID : SecurityAPITestObject
	{
		private CertificateX509 cert;
		private PrivateKeyManager key;
		private XmlDSigSigner signer;
		private DSigOptions options;
		private string xmlInput;
		private string xPath;

		[SetUp]
		public virtual void SetUp()
		{
			cert = new CertificateX509();
			cert.Load(BASE_PATH + "dummycerts\\RSA_sha256_1024\\sha256_cert.crt");
			key = new PrivateKeyManager();
			key.Load(BASE_PATH + "dummycerts\\RSA_sha256_1024\\sha256d_key.pem");

			signer = new XmlDSigSigner();
			options = new DSigOptions();

			options.IdentifierAttribute = "Id";

			xmlInput = "<envEvento xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"1.00\"><idLote>1</idLote><evento versao=\"1.00\"><infEvento Id=\"ID2102103521011431017000298855005000016601157405784801\"><cOrgao>91</cOrgao><tpAmb>1</tpAmb><CNPJ>31102046000145</CNPJ><chNFe>35210114310170002988550050000166011574057848</chNFe><dhEvento>2021-01-26T11:12:34-03:00</dhEvento><tpEvento>210210</tpEvento><nSeqEvento>1</nSeqEvento><verEvento>1.00</verEvento><detEvento versao=\"1.00\"><descEvento>Ciencia da Operacao</descEvento></detEvento></infEvento></evento></envEvento>";
			xPath = "#ID2102103521011431017000298855005000016601157405784801";
		}

		[Test]
		public void TestFindID1()
		{
			//System.Diagnostics.Debugger.Launch();
			string signed = signer.DoSignElement(xmlInput, xPath, key, cert, options);
			Assert.IsFalse(SecurityUtils.compareStrings(signed, ""));
			Assert.IsFalse(signer.HasError());
		}

	}
}
