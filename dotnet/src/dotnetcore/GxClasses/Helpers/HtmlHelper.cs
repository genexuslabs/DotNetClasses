using Microsoft.AspNetCore.Http;
using System;
using System.Text;

namespace GeneXus.Utils
{
	public class HtmlTextWriter
	{
		StringBuilder _stream = new StringBuilder();
		HttpContext _context;
		public HtmlTextWriter(HttpContext context) {
			_context = context;
		}
		public void WriteLine(string s) {
			Write(s + Environment.NewLine);
		}
		public void Write(string s) {
			_stream.Append(s);
		}
		public void Flush()
		{
			//Response.WriteAsync makes ResponseCompressor throws exception for big stream, use Response.Body.Write instead
			_context.Response.Body.Write(Encoding.UTF8.GetBytes(_stream.ToString()));
			_context.Response.Body.FlushAsync();
		}
	}

}