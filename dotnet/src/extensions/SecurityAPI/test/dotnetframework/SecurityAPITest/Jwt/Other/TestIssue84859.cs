using GeneXusJWT.GenexusJWT;
using NUnit.Framework;
using SecurityAPICommons.Utils;
using SecurityAPITest.SecurityAPICommons.commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityAPITest.Jwt.Other
{
	[TestFixture]
	public class TestIssue84859 : SecurityAPITestObject
	{
		protected static string token;
		protected static JWTCreator jwt;

		[SetUp]
		public virtual void SetUp()
		{
			token = "dummy";
			jwt = new JWTCreator();
		}

		[Test]
		public void TestMalformedPayload()
		{
			string res = jwt.GetPayload(token);
			Assert.IsTrue(SecurityUtils.compareStrings(res, ""));
			Assert.IsTrue(jwt.HasError());
		}

		[Test]
		public void TestMalformedHeader()
		{
			string res = jwt.GetHeader(token);
			Assert.IsTrue(SecurityUtils.compareStrings(res, ""));
			Assert.IsTrue(jwt.HasError());
		}

		[Test]
		public void TestMalformedID()
		{
			string res = jwt.GetTokenID(token);
			Assert.IsTrue(SecurityUtils.compareStrings(res, ""));
			Assert.IsTrue(jwt.HasError());
		}
	}
}
