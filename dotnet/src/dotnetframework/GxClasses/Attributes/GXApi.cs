using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneXus.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class GXApi : Attribute
	{
	}
}
