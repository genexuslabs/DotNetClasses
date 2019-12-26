using Microsoft.AspNetCore.Http;
using System;

namespace GeneXus.Utils
{
	public class HtmlTextWriter
	{
		HttpContext _context;
		public HtmlTextWriter(HttpContext context) {
			_context = context;
		}
		public void WriteLine(string s) {
			Write(s + Environment.NewLine);
		}
		public void Write(string s) {
			
			_context.Response.WriteAsync(s);
		}
	}

}