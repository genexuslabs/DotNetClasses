using GeneXus.Mock;
using GeneXus.Programs;
using MockDBAccess;
using NUnit.Framework;

namespace TestMockDBAccess
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestServerDateWithMockDB()
        {
			//Connect to sample DB from https://www.sqlitetutorial.net/sqlite-sample-database/
			new aabmtest().executeCmdLine(null) ;

        }
		[Test]
		public void TestMockProcedure()
		{
			IGxMock mock= new GXMockProcedure();
			GxMockProvider.Provider = mock;
			aprocmain test = new aprocmain();
			string message = "";
			short number = 2;
			test.execute(1, ref number, out message);

			string msg = test.GX_msglist.getItemText(1);
			Assert.AreEqual("Mocking aprocmain Parameters:[AV8clientid,Int16,in,value:1] [AV9Number,Int16&,ref,value:2] [AV10Message,String&,ref,value:] ", msg);

		}
	}
}