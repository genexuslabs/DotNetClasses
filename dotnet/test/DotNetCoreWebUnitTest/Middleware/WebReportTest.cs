using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Codeuctivity;
using com.genexus.reports;
using GeneXus.Metadata;
using GeneXus.Programs;
using Spire.Pdf;
using Xunit;
namespace xUnitTesting
{
	public class WebReportTest : MiddlewareTest
	{
		public WebReportTest() : base()
		{
			ClassLoader.FindType("apdfwebbasictest", "GeneXus.Programs", "apdfwebbasictest", Assembly.GetExecutingAssembly(), true);//Force loading assembly for webhook procedure
			ClassLoader.FindType("apdfwebwoutimage", "GeneXus.Programs", "apdfwebwoutimage", Assembly.GetExecutingAssembly(), true); 
			server.AllowSynchronousIO = true;
		}
		[Fact(Skip = "temporary turned off due to timeout error")]
		public void TestPDFA()
		{
			HttpClient client = server.CreateClient();
			TestPDFA_1AB(client, "apdfwebbasictest.aspx", Spire.Pdf.PdfConformanceLevel.Pdf_A1A).GetAwaiter().GetResult();
			TestPDFA_1AB(client, "apdfwebwoutimage.aspx", Spire.Pdf.PdfConformanceLevel.Pdf_A1A).GetAwaiter().GetResult();
			PDFReportItextSharp.SetDefaultComplianceLevel(com.genexus.reports.PdfConformanceLevel.Pdf_A1B);
			TestPDFA_1AB(client, "apdfwebbasictest.aspx", Spire.Pdf.PdfConformanceLevel.Pdf_A1B).GetAwaiter().GetResult();
			TestPDFA_1AB(client, "apdfwebwoutimage.aspx", Spire.Pdf.PdfConformanceLevel.Pdf_A1B).GetAwaiter().GetResult();
			
		}
		async Task TestPDFA_1AB(HttpClient client, string serviceName, Spire.Pdf.PdfConformanceLevel expectedLevel)
		{
			HttpResponseMessage response = await client.GetAsync(serviceName);
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
			Spire.Pdf.PdfConformanceLevel conformance = pdf.Conformance;

			Assert.True(expectedLevel == conformance, $"Conformance level is {conformance} but {expectedLevel} was expected");

			PdfAValidator pdfAValidator = new PdfAValidator();
			Report result = await pdfAValidator.ValidateWithDetailedReportAsync(fileName);
			bool isValid = await pdfAValidator.ValidateAsync(fileName);
			Assert.True(isValid, result.RawOutput);



		}

	}

}
