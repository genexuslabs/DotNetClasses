namespace GeneXus.Http
{
	using System;
	using GeneXus.Application;
	using System.Web;
	using System.Web.Hosting;
	using System.IO;
	using GeneXus.Http;
	using System.Web.UI;
	using System.Text;
	using System.Collections.Specialized;

	public class GxWebWrapper
	{
		string _baseUrl;
		GXHttpHandler _source;

		public string BaseURL
		{
			get { return _baseUrl;}
			set { _baseUrl = value;}
		}
		public GXHttpHandler Source
		{
			get { return _source;}
			set { _source = value;}
		}
		public string GetResponse()
		{
			StringBuilder output = new StringBuilder();
			StringWriter outputWriter = new StringWriter( output);
			_source.ControlOutputWriter = new HtmlTextWriter( outputWriter);
			_source.FormVars = new NameValueCollection();
			_source.InitPrivates();
			_source.context.BaseUrl = _baseUrl;
			try
			{
				_source.getresponse(_baseUrl);
			}
			catch (Exception e)
			{
				try
				{
					_source.context.CloseConnections();
				}
				catch{}
				throw new Exception( "GXApplication exception", e);
			}
			return output.ToString();
		}
	}
}

	


