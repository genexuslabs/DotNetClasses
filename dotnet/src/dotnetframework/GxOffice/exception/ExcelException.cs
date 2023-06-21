using System;

namespace GeneXus.MSOffice.Excel.exception
{
	public class ExcelException : Exception
	{
		private int _errorCode;
		private string _errDsc;

		public ExcelException(int errCode, string errDsc, Exception e) : base(errDsc, e)
		{
			_errorCode = errCode;
			_errDsc = errDsc;
		}

		public ExcelException(int errCode, string errDsc) : base(errDsc)
		{
			_errorCode = errCode;
			_errDsc = errDsc;
		}

		public int ErrorCode
		{
			get { return _errorCode; }
		}

		public string ErrorDescription
		{
			get { return _errDsc; }
		}
	}
}
