namespace GeneXus.Http
{
	using System;
	using System.Collections.Specialized;
	using System.IO;
	using System.Text;
#if !NETCORE
	using System.Web.UI;
#else
	using GeneXus.Utils;
#endif

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

	


