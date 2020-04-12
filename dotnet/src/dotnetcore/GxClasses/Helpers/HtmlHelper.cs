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
			_context.Response.WriteAsync(_stream.ToString());
		}
	}

}