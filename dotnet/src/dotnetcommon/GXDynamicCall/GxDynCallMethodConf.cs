using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genexus.DynamicCall
{
	public class GxDynCallMethodConf
	{
		public bool IsStatic
		{
			get; set;
		}
		public string MethodName
		{
			get; set;
		}

		public GxDynCallMethodConf()
		{
			IsStatic = false;
			MethodName = "execute";
		}

	}
}
