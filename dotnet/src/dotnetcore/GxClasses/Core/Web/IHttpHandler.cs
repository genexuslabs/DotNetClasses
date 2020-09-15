using Microsoft.AspNetCore.Http;

namespace GeneXus.Utils
{
	public interface IHttpHandler
	{
		void ProcessRequest(HttpContext context);
		void sendAdditionalHeaders();
		HtmlTextWriter ControlOutputWriter { get; set; }

	}
}