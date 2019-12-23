using System;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Search
{
	public class SearchException : Exception
	{
		public SearchException(int errCode) : base(GetMessage(errCode)) { m_errCode = errCode; }

		private int m_errCode;

		public int ErrCode
		{
			get { return m_errCode; }
			set { m_errCode = value; }
		}

		static string GetMessage(int errCode)
		{
			switch (errCode)
			{
				case NO_ERROR:
					return "No Error";

				case COULDNOTCONNECT:
					return "Could not connect to index files";

				case IOEXCEPTION:
					return "Could not open index files";

				case PARSEERROR:
					return "Error parsing query string";

				case INDEXERROR:
					return "Invalid collection index";

				default:
					return "Unknow";
			}
		}

		public const int NO_ERROR = 0; 
		public const int COULDNOTCONNECT = 1; //Could not connect to index
		public const int PARSEERROR = 2;
		public const int IOEXCEPTION = 3;
		public const int INDEXERROR = 4;
	}
}
