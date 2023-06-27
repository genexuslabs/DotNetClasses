using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GeneXus.Data
{
	[Serializable()]
	public class GxNotImplementedException : Exception
	{
		public GxNotImplementedException(string message)
			: base(message)
		{
		}

		public GxNotImplementedException()
		{
		}
	}

}
