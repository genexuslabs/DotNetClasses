using System;
using System.IO;
using NUnit.Framework;
using GamUtils;
using Microsoft.IdentityModel.Tokens;

namespace GamTest.Utils
{
	[TestFixture]
	public class TestPrivateKey
	{
#pragma warning disable CS0414
		private static string resources;
		private static string path_RSA_sha256_2048;
		private static string alias;
		private static string password;
#pragma warning disable IDE0044
		private static string BASE_PATH;
#pragma warning restore IDE0044
#pragma warning restore CS0414
		private static string header;
		private static string payload;

		[SetUp]
		public virtual void SetUp()
		{
			BASE_PATH = TestJwt.GetStartupDirectory();
			resources = Path.Combine(BASE_PATH, "Resources", "dummycerts");
			path_RSA_sha256_2048 = Path.Combine(resources, "RSA_sha256_2048");
			string kid = Guid.NewGuid().ToString();
			alias = "1";
			password = "dummy";
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

		}


		[Test]
		[Ignore("issues with pfx extension in .Net")]
		public void TestLoadPfx()
		{
			string result = GamUtilsEO.CreateJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.pfx", "", password, payload, header );
			Assert.IsFalse(result.IsNullOrEmpty(), "TestLoadPfx");
		}

		[Test]
		public void TestLoadPkcs12()
		{
			string result = GamUtilsEO.CreateJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.pkcs12", "", password, payload, header);
			Assert.IsFalse(result.IsNullOrEmpty(), "TestLoadPkcs12");
		}

		[Test]
		public void TestLoadP12()
		{
			string result = GamUtilsEO.CreateJWTWithFile(path_RSA_sha256_2048 + "\\sha256_cert.p12", "", password, payload, header);
			Assert.IsFalse(result.IsNullOrEmpty(), "TestLoadP12");
		}

		[Test]
		public void TestLoadPem()
		{
			string result = GamUtilsEO.CreateJWTWithFile(path_RSA_sha256_2048 + "\\sha256d_key.pem", "", password, payload, header);
			Assert.IsFalse(result.IsNullOrEmpty(), "TestLoadPem");
		}

		[Test]
		public void TestLoadKey()
		{
			string result = GamUtilsEO.CreateJWTWithFile(path_RSA_sha256_2048 + "\\sha256d_key.key", "", password, payload, header);
			Assert.IsFalse(result.IsNullOrEmpty(), "TestLoadKey");
		}

		[Test]
		public void TestLoadBase64()
		{
			string base64 = "MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQC1zgaU+Wh63p9DNWoAy64252EvZjN49AY3x0QCnAa8JO9Pk7znQwrxEFUKgZzv0GHEYW7+X+uyJr7BW4TA6fuJJ8agE/bmZRZyjdJjoue0FML6fbmCZ9Tsxpxe4pzispyWQ8jYT4Kl4I3fdZNUSn4XSidnDKBISeC05mrcchDKhInpiYDJ481lsB4JTEti3S4Xy/ToKwY4t6attw6z5QDhBc+Yro+YUqruliOAKqcfybe9k07jwMCvFVM1hrYYJ7hwHDSFo3MKwZ0y2gw0w6SgVBxLFo+KYP3q63b5wVhD8lzaSh+8UcyiHM2/yjEej7EnRFzdclTSNXRFNaiLnEVdAgMBAAECggEBAJP8ajslcThisjzg47JWGS8z1FXi2Q8hg1Yv61o8avcHEY0y8tdEKUnkQ3TT4E0M0CgsL078ATz4cNmvhzYIv+j66aEv3w/XRRhl/NWBqx1YsQV5BWHy5sz9Nhe+WnnlbbSa5Ie+4NfpG1LDv/Mi19RZVg15p5ZwHGrkDCP47VYKgFXw51ZPxq/l3IIeq4PyueC/EPSAp4e9qei7p85k3i2yiWsHgZaHwHgDTx2Hgq1y/+/E5+HNxL2OlPr5lzlN2uIPZ9Rix2LDh0FriuCEjrXFsTJHw4LTK04rkeGledMtw6/bOTxibFbgeuQtY1XzG/M0+xlP2niBbAEA4Z6vTsECgYEA6k7LSsh6azsk0W9+dE6pbc3HisOoKI76rXi38gEQdCuF04OKt46WttQh4r1+dseO4OgjXtRMS0+5Hmx2jFXjPJexMgLftvrbwaVqg9WHenKL/qj5imCn4kVaa4Jo1VHFaIY+1b+iv+6WY/leFxGntAki9u4PRogRrUrWLRH9keUCgYEAxqLisgMHQGcpJDHJtI2N+HUgLDN065PtKlEP9o6WBwAb86/InVaTo2gmEvmslNQDYH16zdTczWMHnnBx1B012KpUD+t5CWIvMZdsTnMRDjWhdgm5ylN9NT89t5X8GPvo36WjuXAKWWjcRodzRgo57z9achCyMKhGU5yDOxh8jhkCgYAx6rtwoSlDcwQzAjfEe4Wo+PAL5gcLLPrGvjMiAYwJ08Pc/ectl9kP9j2J2qj4kSclTw9KApyGZuOfUagn2Zxhqkd7yhTzHJp4tM7uay1DrueYR1NyYYkisXfD87J1z8forsDwNLVtglzTy6p56674sgGa7bifZBmv+4OJco286QKBgQC4dGXDHGDNg36G590A1zpw8ILxyM7YPEPOOfxy3rGeypEqV6AZy13KLlq84DFM+xwvrBYvsW1hJIbcsFpjuMRZ8MGjDu0Us6JTkOO4bc32vgKzlBB9O85XdeSf6J1zrenwVOaWut5BbMiwjfOTpMdrzg71QV/XI0w7NGoApJp1cQKBgERfI6AfJTaKtEpfX3udR1B3zra1Y42ppU2TvGI5J2/cItENoyRmtyKYDp2I036/Pe63nuIzs31i6q/hCr9Tv3AGoSVKuPLpCWv5xVO/BPhGs5dwx81nUo0/P+H2X8dx7g57PQY4uf4F9+EIXeAdbPqfB8GBW7RX3FDx5NpB+Hh/";
			string result = GamUtilsEO.CreateJWTWithFile(base64 , "", "", payload, header);
			Assert.IsFalse(result.IsNullOrEmpty(), "TestLoadKey");
		}
	}
}
