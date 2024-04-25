using System;
using GeneXus.Application;
using Xunit;
namespace UnitTesting
{
	public class LanguageCmdTest
	{

		[Fact]

		public void TestLangugae()
		{
			GxContext context = new GxContext();
			context.SetLanguage("Arabic");
			DateTime arabicDate = context.localUtil.YMDToD(2025, 4, 1);
			Assert.Equal(2025, arabicDate.Year);
			string msg = context.localUtil.Format(arabicDate, "99/99/99");
			Assert.Equal("01/04/25", msg);
			msg = context.GetMessage("hello", "");
			Assert.Equal("مرحبا", msg);
		}

	}

}
