using GeneXus.Programs;
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
    }
}