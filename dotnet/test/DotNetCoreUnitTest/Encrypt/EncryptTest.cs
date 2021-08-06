using GeneXus.Application;
using GeneXus.Encryption;
using GeneXus.Http;
using GeneXus.Utils;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace UnitTesting
{
	public class EncryptTest
	{
		[Fact]
		public void UriEncrypt64Test()
		{
			GxContext context = new GxContext();
			var httpctx = new DefaultHttpContext();
			context.HttpContext = httpctx;
			TestHandler testHandler = new TestHandler();
			testHandler.ProcessRequest(httpctx);
		}
	}
	public class TestHandler : GXHttpHandler
	{
		const string GXKey = "395CA6376B75526C8B6FF4010307A47D";
		const string pgmName = "protocolo.wpconfirmardados.aspx";
		public TestHandler()
		{
			context = new GxContext();
			IsMain = true;
		}
		public override void webExecute()
		{
			string ecryptionTmp = pgmName + UrlEncode(StringUtil.LTrimStr(1, 1, 0)) + "," + UrlEncode(StringUtil.LTrimStr(0, 1, 0)) + "," + UrlEncode(StringUtil.RTrim("jhon@4rtecnology.com")) + "," + UrlEncode(StringUtil.RTrim("John Paul")) + "," + UrlEncode(StringUtil.RTrim("424b25")) + "," + UrlEncode(StringUtil.RTrim("Request"));
			string encrypted = Encrypt64(ecryptionTmp + Crypto.CheckSum(ecryptionTmp, 6), GXKey);
			Assert.Contains("//", encrypted);

			string uriEncrypted = UriEncrypt64(ecryptionTmp + Crypto.CheckSum(ecryptionTmp, 6), GXKey);
			Assert.DoesNotContain("//", uriEncrypted);

			string GXDecQS = UriDecrypt64(encrypted, GXKey);
			bool isMatch = (StringUtil.StrCmp(StringUtil.Right(GXDecQS, 6), Crypto.CheckSum(StringUtil.Left(GXDecQS, (short)(StringUtil.Len(GXDecQS) - 6)), 6)) == 0) && (StringUtil.StrCmp(StringUtil.Substring(GXDecQS, 1, StringUtil.Len(pgmName)), pgmName) == 0);
			Assert.True(isMatch);

		}
	}
}
