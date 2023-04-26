using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class SdtMd5hashTest
	{
		[Fact]
		public void Md5HashCultureIndependant()
		{
			string expectedHash = "F9A2FB37DA0D6F5B62BC0316B1BDC120";
			string json = "{\"InvoiceId\":5,\"InvoiceDate\":\"2005-06-25\",\"InvoiceDescription\":\"Inv.: 06/25/05\",\"ClientId\":2,\"ClientFirstName\":\"Juan                \",\"ClientBalance\":4000.00000000000000,\"ClientAddress\":\"BsAs                     \",\"InvoiceLatestLine\":0,\"Line\":[{\"InvoiceLineId\":1,\"ProductId\":1,\"ProductName\":\"Computer            \",\"ProductStock\":200,\"ProductPrice\":720.0000,\"InvoiceLineQty\":3,\"InvoiceLineAmount\":2160.0000,\"Mode\":\"UPD\",\"Modified\":0,\"Initialized\":0,\"InvoiceLineId_Z\":1,\"ProductId_Z\":1,\"ProductName_Z\":\"Computer            \",\"ProductStock_Z\":200,\"ProductPrice_Z\":720.0000,\"InvoiceLineQty_Z\":3,\"InvoiceLineAmount_Z\":2160.0000},{\"InvoiceLineId\":2,\"ProductId\":10,\"ProductName\":\"Impresoras          \",\"ProductStock\":25,\"ProductPrice\":630.0000,\"InvoiceLineQty\":1,\"InvoiceLineAmount\":630.0000,\"Mode\":\"UPD\",\"Modified\":0,\"Initialized\":0,\"InvoiceLineId_Z\":2,\"ProductId_Z\":10,\"ProductName_Z\":\"Impresoras          \",\"ProductStock_Z\":25,\"ProductPrice_Z\":630.0000,\"InvoiceLineQty_Z\":1,\"InvoiceLineAmount_Z\":630.0000},{\"InvoiceLineId\":3,\"ProductId\":7,\"ProductName\":\"Carteles Publicidad \",\"ProductStock\":1250,\"ProductPrice\":150.0000,\"InvoiceLineQty\":2,\"InvoiceLineAmount\":300.0000,\"Mode\":\"UPD\",\"Modified\":0,\"Initialized\":0,\"InvoiceLineId_Z\":3,\"ProductId_Z\":7,\"ProductName_Z\":\"Carteles Publicidad \",\"ProductStock_Z\":1250,\"ProductPrice_Z\":150.0000,\"InvoiceLineQty_Z\":2,\"InvoiceLineAmount_Z\":300.0000}],\"InvoiceSubTotal\":3090.000000000000,\"InvoiceSubTotal_N\":0,\"InvoiceTaxes\":679.80000000000000,\"InvoiceTotal\":3769.80000000000000,\"Mode\":\"UPD\",\"Initialized\":0,\"InvoiceId_Z\":5,\"InvoiceDate_Z\":\"2005-06-25\",\"InvoiceDescription_Z\":\"\",\"ClientId_Z\":2,\"ClientFirstName_Z\":\"Juan                \",\"ClientBalance_Z\":4000.0000,\"ClientAddress_Z\":\"BsAs                     \",\"InvoiceLatestLine_Z\":0,\"InvoiceSubTotal_Z\":3090.000000000000,\"InvoiceTaxes_Z\":0,\"InvoiceTotal_Z\":0}";
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			ComputeHash(expectedHash, json);
			Thread.CurrentThread.CurrentCulture = new CultureInfo("es-UY");
			ComputeHash(expectedHash, json);
		}

		private void ComputeHash(string expectedHash, string json)
		{
			SdtInvoice worker = new SdtInvoice();
			worker.FromJSonString(json);
			SdtInvoice_RESTInterface worker_interface = new SdtInvoice_RESTInterface(worker);
			string md5Hash = worker_interface.Hash;
			string s = worker_interface.ToString();
			Console.WriteLine(s);
			Assert.True(expectedHash == md5Hash, $"MD5 Hash on Culture {Thread.CurrentThread.CurrentCulture.Name}: {md5Hash} does not match expected hash: {expectedHash}");
		}
	}
}
