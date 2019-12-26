using System;


namespace GeneXus.Application
{
	[Serializable]
	public class UrlItem
	{
		public string url;
		public bool redirect;
	}
	public sealed class GeneXusCommonAssemblyAttribute : Attribute
	{
	}
	public class GxHttpContextVars
	{
		public string wjLoc { get; set; }
		public int wjLocDisableFrm { get; set; }
	}

}
