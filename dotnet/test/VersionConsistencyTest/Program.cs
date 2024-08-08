using System;
using GeneXus.Mail;
using GeneXus.Office.ExcelGXEPPlus;

public class TestVersionConsistency
{
	public static void Main(string[] args)
	{
		ExcelDocument eppPlusDocument = new ExcelDocument();
		Console.WriteLine($"ExcelDocument default path:{eppPlusDocument.DefaultPath}");

		GXPOP3Session mailkitSession = new GXPOP3Session();
		mailkitSession.Host = "localhost";
		mailkitSession.Logout();


	}
}
