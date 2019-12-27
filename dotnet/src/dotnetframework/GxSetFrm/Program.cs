using GeneXus.Printer;

namespace GeneXus.GxSetFrm
{
	class Program
	{
		public static void Main(string[] args)
		{
			new GxReportBuilderDll("").GxPrnCfg("GXPRN.INI");
		}
	}
}
