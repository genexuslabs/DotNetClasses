using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXusJWT.Utils;
using NUnit.Framework;
using SecurityAPITest.SecurityAPICommons.commons;

namespace SecurityAPITest.Jwt.Features
{
	[TestFixture]
	public class TestUnixTimeStampCreator: SecurityAPITestObject
	{
		protected static string date;
		protected static UnixTimeStampCreator creator;
		protected static string expected;

		[SetUp]
		public virtual void SetUp()
		{
			date = "2023/07/19 11:41:00";
			creator = new UnixTimeStampCreator();
			expected = "1689766860";

		}

		[Test]
		public void TestCreate()
		{
			string obtained = creator.Create(date);
			//Console.WriteLine("obt:" + obtained);
			Equals(expected, obtained, creator);
		}
	}
}
