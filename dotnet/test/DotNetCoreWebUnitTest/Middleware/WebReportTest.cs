using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Codeuctivity;
using GeneXus.Metadata;
using Spire.Pdf;
using Xunit;
namespace xUnitTesting
{
	public class WebReportTest : MiddlewareTest
	{
		public WebReportTest():base()
		{
			ClassLoader.FindType("apdfwebbasictest", "GeneXus.Programs", "apdfwebbasictest", Assembly.GetExecutingAssembly(), true);//Force loading assembly for webhook procedure
			server.AllowSynchronousIO=true;
		}
		[Fact]
		public async Task TestPDFA()
		{
			HttpClient client = server.CreateClient();

			HttpResponseMessage response = await client.GetAsync("apdfwebbasictest.aspx");
			response.EnsureSuccessStatusCode();
			Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
			String fileName = response.Content.Headers.ContentDisposition.FileName;
			//Assert.Equal("Report.pdf", fileName);
			using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				await response.Content.CopyToAsync(fs);
			}
			PdfDocument pdf = new PdfDocument();
			pdf.LoadFromFile(fileName);
			PdfConformanceLevel conformance = pdf.Conformance;

			Assert.Equal(PdfConformanceLevel.Pdf_A1A, conformance);

			PdfAValidator pdfAValidator = new PdfAValidator();
			Report result = await pdfAValidator.ValidateWithDetailedReportAsync(fileName);
			bool isValid = await pdfAValidator.ValidateAsync(fileName);
			Assert.True(isValid, result.RawOutput);



		}

	}

}
