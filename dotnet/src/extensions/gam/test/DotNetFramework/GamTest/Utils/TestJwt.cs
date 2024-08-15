using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using GamUtils;
using Microsoft.IdentityModel.Tokens;




namespace GamTest.Utils
{
	[TestFixture]
	public class TestJwt
	{
#pragma warning disable CS0414
		private static string resources;
		private static string header;
		private static string payload;
		private static string path_RSA_sha256_2048;
		private static string alias;
		private static string password;
#pragma warning disable IDE0044
		private static string BASE_PATH;
#pragma warning restore IDE0044
#pragma warning restore CS0414



		[SetUp]
		public virtual void SetUp()
		{
			BASE_PATH = GetStartupDirectory();
			resources = Path.Combine(BASE_PATH, "Resources", "dummycerts");

			string kid = Guid.NewGuid().ToString();
			
			header = "{\n" +
				"  \"alg\": \"RS256\",\n" +
				"  \"kid\": \"" + kid + "\",\n" +
				"  \"typ\": \"JWT\"\n" +
				"}";
			payload = "{\n" +
				"  \"sub\": \"1234567890\",\n" +
				"  \"name\": \"John Doe\",\n" +
				"  \"iat\": 1516239022\n" +
				"}";
			
			alias = "1";
			password = "dummy";
			path_RSA_sha256_2048 = Path.Combine(resources, "RSA_sha256_2048");
			
		}

		[Test]
		public void Test_pkcs8_pem()
		{
			string token = GamUtilsEO.CreateJwt(Path.Combine(path_RSA_sha256_2048, "sha256d_key.pem"), "", "", payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "Test_pkcs8_pem");
			bool result = GamUtilsEO.VerifyJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.cer"), "", "", token);
			Assert.IsTrue(result, "test_pkcs8 verify cer");
		}

		[Test]
		public void Test_get()
		{
			string token = GamUtilsEO.CreateJwt(Path.Combine(path_RSA_sha256_2048, "sha256d_key.pem"), "", "", payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_get create");
			string header_get = GamUtilsEO.GetJwtHeader(token);
			Assert.IsFalse(header_get.IsNullOrEmpty(), "test_get getHeader");
			string payload_get = GamUtilsEO.GetJwtPayload(token);
			Assert.IsFalse(payload_get.IsNullOrEmpty(), "test_get getPayload");
		}

		[Test]
		public void Test_pkcs8_key()
		{
			string token = GamUtilsEO.CreateJwt(Path.Combine(path_RSA_sha256_2048, "sha256d_key.key"), "", "", payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_pkcs8 create");
			bool result = GamUtilsEO.VerifyJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.crt"), "", "", token);
			Assert.IsTrue(result, "test_pkcs8 verify crt");
		}

		[Test]
		public void Test_pkcs12_p12()
		{
			string token = GamUtilsEO.CreateJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12"), alias, password, payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_pkcs12_p12 create");
			bool result = GamUtilsEO.VerifyJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12"), alias, password, token);
			Assert.IsTrue(result, "test_pkcs12_p12 verify");
		}

		[Test]
		public void Test_pkcs12_pkcs12()
		{
			string token = GamUtilsEO.CreateJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.pkcs12"), alias, password, payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_pkcs12_pkcs12 create");
			bool result = GamUtilsEO.VerifyJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.pkcs12"), alias, password, token);
			Assert.IsTrue(result, "test_pkcs12_pkcs12 verify");
		}

		[Test]
		[Ignore("issues with pfx extension in .Net")]
		public void Test_pkcs12_pfx()
		{
			string token = GamUtilsEO.CreateJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.pfx"), alias, password, payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_pkcs12_pfx create");
			bool result = GamUtilsEO.VerifyJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.pfx"), alias, password, token);
			Assert.IsTrue(result, "test_pkcs12_pfx verify");
		}

		[Test]
		public void Test_pkcs12_noalias()
		{
			string token = GamUtilsEO.CreateJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12"), "", password, payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_pkcs12_noalias jks create");
			bool result = GamUtilsEO.VerifyJwt(Path.Combine(path_RSA_sha256_2048, "sha256_cert.p12"), "", password, token);
			Assert.IsTrue(result, "test_pkcs12_noalias jks verify");
		}

		[Test]
		public void Test_b64()
		{
			string publicKey = "MIIEATCCAumgAwIBAgIJAIAqvKHZ+gFhMA0GCSqGSIb3DQEBCwUAMIGWMQswCQYDVQQGEwJVWTETMBEGA1UECAwKTW9udGV2aWRlbzETMBEGA1UEBwwKTW9udGV2aWRlbzEQMA4GA1UECgwHR2VuZVh1czERMA8GA1UECwwIU2VjdXJpdHkxEjAQBgNVBAMMCXNncmFtcG9uZTEkMCIGCSqGSIb3DQEJARYVc2dyYW1wb25lQGdlbmV4dXMuY29tMB4XDTIwMDcwODE4NTcxN1oXDTI1MDcwNzE4NTcxN1owgZYxCzAJBgNVBAYTAlVZMRMwEQYDVQQIDApNb250ZXZpZGVvMRMwEQYDVQQHDApNb250ZXZpZGVvMRAwDgYDVQQKDAdHZW5lWHVzMREwDwYDVQQLDAhTZWN1cml0eTESMBAGA1UEAwwJc2dyYW1wb25lMSQwIgYJKoZIhvcNAQkBFhVzZ3JhbXBvbmVAZ2VuZXh1cy5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC1zgaU+Wh63p9DNWoAy64252EvZjN49AY3x0QCnAa8JO9Pk7znQwrxEFUKgZzv0GHEYW7+X+uyJr7BW4TA6fuJJ8agE/bmZRZyjdJjoue0FML6fbmCZ9Tsxpxe4pzispyWQ8jYT4Kl4I3fdZNUSn4XSidnDKBISeC05mrcchDKhInpiYDJ481lsB4JTEti3S4Xy/ToKwY4t6attw6z5QDhBc+Yro+YUqruliOAKqcfybe9k07jwMCvFVM1hrYYJ7hwHDSFo3MKwZ0y2gw0w6SgVBxLFo+KYP3q63b5wVhD8lzaSh+8UcyiHM2/yjEej7EnRFzdclTSNXRFNaiLnEVdAgMBAAGjUDBOMB0GA1UdDgQWBBQtQAWJRWNr/OswPSAdwCQh0Eei/DAfBgNVHSMEGDAWgBQtQAWJRWNr/OswPSAdwCQh0Eei/DAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQCjHe3JbNKv0Ywc1zlLacUNWcjLbmzvnjs8Wq5oxtf5wG5PUlhLSYZ9MPhuf95PlibnrO/xVY292P5lo4NKhS7VOonpbPQ/PrCMO84Pz1LGfM/wCWQIowh6VHq18PiZka9zbwl6So0tgClKkFSRk4wpKrWX3+M3+Y+D0brd8sEtA6dXeYHDtqV0YgjKdZIIOx0vDT4alCoVQrQ1yAIq5INT3cSLgJezIhEadDv3Tc7bMxMFeL+81qHm9Z/9/KE6Z+JB0ZEOkF/2NSQJd+Z7MBR8CxOdTQis3ltMoXDatNkjZ2Env40sw4NICB8YYhsWMIarew5uNT+RS28YHNlbmogh";
			string privateKey = "MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQC1zgaU+Wh63p9DNWoAy64252EvZjN49AY3x0QCnAa8JO9Pk7znQwrxEFUKgZzv0GHEYW7+X+uyJr7BW4TA6fuJJ8agE/bmZRZyjdJjoue0FML6fbmCZ9Tsxpxe4pzispyWQ8jYT4Kl4I3fdZNUSn4XSidnDKBISeC05mrcchDKhInpiYDJ481lsB4JTEti3S4Xy/ToKwY4t6attw6z5QDhBc+Yro+YUqruliOAKqcfybe9k07jwMCvFVM1hrYYJ7hwHDSFo3MKwZ0y2gw0w6SgVBxLFo+KYP3q63b5wVhD8lzaSh+8UcyiHM2/yjEej7EnRFzdclTSNXRFNaiLnEVdAgMBAAECggEBAJP8ajslcThisjzg47JWGS8z1FXi2Q8hg1Yv61o8avcHEY0y8tdEKUnkQ3TT4E0M0CgsL078ATz4cNmvhzYIv+j66aEv3w/XRRhl/NWBqx1YsQV5BWHy5sz9Nhe+WnnlbbSa5Ie+4NfpG1LDv/Mi19RZVg15p5ZwHGrkDCP47VYKgFXw51ZPxq/l3IIeq4PyueC/EPSAp4e9qei7p85k3i2yiWsHgZaHwHgDTx2Hgq1y/+/E5+HNxL2OlPr5lzlN2uIPZ9Rix2LDh0FriuCEjrXFsTJHw4LTK04rkeGledMtw6/bOTxibFbgeuQtY1XzG/M0+xlP2niBbAEA4Z6vTsECgYEA6k7LSsh6azsk0W9+dE6pbc3HisOoKI76rXi38gEQdCuF04OKt46WttQh4r1+dseO4OgjXtRMS0+5Hmx2jFXjPJexMgLftvrbwaVqg9WHenKL/qj5imCn4kVaa4Jo1VHFaIY+1b+iv+6WY/leFxGntAki9u4PRogRrUrWLRH9keUCgYEAxqLisgMHQGcpJDHJtI2N+HUgLDN065PtKlEP9o6WBwAb86/InVaTo2gmEvmslNQDYH16zdTczWMHnnBx1B012KpUD+t5CWIvMZdsTnMRDjWhdgm5ylN9NT89t5X8GPvo36WjuXAKWWjcRodzRgo57z9achCyMKhGU5yDOxh8jhkCgYAx6rtwoSlDcwQzAjfEe4Wo+PAL5gcLLPrGvjMiAYwJ08Pc/ectl9kP9j2J2qj4kSclTw9KApyGZuOfUagn2Zxhqkd7yhTzHJp4tM7uay1DrueYR1NyYYkisXfD87J1z8forsDwNLVtglzTy6p56674sgGa7bifZBmv+4OJco286QKBgQC4dGXDHGDNg36G590A1zpw8ILxyM7YPEPOOfxy3rGeypEqV6AZy13KLlq84DFM+xwvrBYvsW1hJIbcsFpjuMRZ8MGjDu0Us6JTkOO4bc32vgKzlBB9O85XdeSf6J1zrenwVOaWut5BbMiwjfOTpMdrzg71QV/XI0w7NGoApJp1cQKBgERfI6AfJTaKtEpfX3udR1B3zra1Y42ppU2TvGI5J2/cItENoyRmtyKYDp2I036/Pe63nuIzs31i6q/hCr9Tv3AGoSVKuPLpCWv5xVO/BPhGs5dwx81nUo0/P+H2X8dx7g57PQY4uf4F9+EIXeAdbPqfB8GBW7RX3FDx5NpB+Hh/";
			string token = GamUtilsEO.CreateJwt(privateKey, "", "", payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_b64 create");
			bool result = GamUtilsEO.VerifyJwt(publicKey, "", "", token);
			Assert.IsTrue(result, "test_b64 verify");
		}

		[Test]
		public void Test_json_jwk()
		{
			string keyPair = GamUtilsEO.GenerateKeyPair();
			string token = GamUtilsEO.CreateJwt(keyPair, "", "", payload, header);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_json_jwk create");
			string publicJwk = GamUtilsEO.GetPublicJwk(keyPair);
			bool result = GamUtilsEO.VerifyJwt(publicJwk, "", "", token);
			Assert.IsTrue(result, "test_json_jwk verify");
		}

		[Test]
		public void Test_json_jwks()
		{
			string keyPair = GamUtilsEO.GenerateKeyPair();
			string publicJwk = GamUtilsEO.GetPublicJwk(keyPair);
			string header_jwks = MakeHeader(publicJwk);
			string token = GamUtilsEO.CreateJwt(keyPair, "", "", payload, header_jwks);
			Assert.IsFalse(token.IsNullOrEmpty(), "test_json_jwks create");
			string publicJwks = "{\"keys\": [" + publicJwk + "]}";
			bool result = GamUtilsEO.VerifyJwt(publicJwks, "", "", token);
			Assert.IsTrue(result, "test_json_jwks verify");
		}

		
		private static string GetStartupDirectory()
		{
#pragma warning disable SYSLIB0044
			string dir = Assembly.GetCallingAssembly().GetName().CodeBase;
#pragma warning restore SYSLIB0044
			Uri uri = new Uri(dir);
			return Path.GetDirectoryName(uri.LocalPath);
		}

		private static string MakeHeader(string publicJwk)
		{
			try
			{
				Jose.Jwk jwk = Jose.Jwk.FromJson(publicJwk);
				return "{\n" +
					"  \"alg\": \"RS256\",\n" +
					"  \"kid\": \"" + jwk.KeyId + "\",\n" +
					"  \"typ\": \"JWT\"\n" +
					"}";

			}
			catch (Exception e)
			{
				Console.WriteLine(e.StackTrace);
				return "";
			}
		}


	}
}
