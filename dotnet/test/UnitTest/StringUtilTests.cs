using System;
using System.Diagnostics;
using GeneXus.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTesting
{
	[TestClass]
	public class StringUtilTests
	{
		[TestMethod]
		public void TestNToCPerformance()
		{
			StringUtil.CachePictures = false;
			Stopwatch w = new Stopwatch();
			w.Start();
			for (int i = 0; i < 1000000; i++)
			{
				string value = StringUtil.NToC(19, 9, 0, ".", ",");
			}
			w.Stop();
			long elapsed = w.ElapsedMilliseconds;

			StringUtil.CachePictures = true;
			Stopwatch w2 = new Stopwatch();
			w2.Start();
			for (int i = 0; i < 1000000; i++)
			{
				string value = StringUtil.NToC(19, 9, 0, ".", ",");
			}
			w2.Stop();
			long elapsed2 = w2.ElapsedMilliseconds;

			Assert.IsTrue(elapsed2 < elapsed);
		}

		[TestMethod]
		public void TestTrunc()
		{
			decimal d = NumberUtil.Trunc((decimal)-7561.23, 1);
			Assert.IsTrue(d == (decimal) -7561.2);

			double doub = NumberUtil.Trunc(-7561.23, 1);
			Assert.IsTrue(doub == -7561.2);

			doub = NumberUtil.Trunc(1.5, 0);
			Assert.IsTrue(doub == 1);

			doub = NumberUtil.Trunc(1.4, 0);
			Assert.IsTrue(doub == 1);

			doub = NumberUtil.Trunc(1.25, 1);
			Assert.IsTrue(doub == 1.2);

			doub = NumberUtil.Trunc(1.24, 1);
			Assert.IsTrue(doub == 1.2);

			doub = NumberUtil.Trunc(1.24, 2);
			Assert.IsTrue(doub == 1.24);

			doub = NumberUtil.Trunc(1.24, 3);
			Assert.IsTrue(doub == 1.24);
		}

		[TestMethod]
		public void TestLike()
		{
			string str1 = "hello";
#pragma warning disable CA1303 // Do not pass literals as localized parameters
			Assert.IsTrue(StringUtil.Like(str1, StringUtil.PadR("he", str1.Length, "%")));
			Assert.IsFalse(StringUtil.Like(str1, StringUtil.PadR("el", str1.Length, "%")));
			Assert.IsTrue(StringUtil.Like(str1, StringUtil.PadR("_el", str1.Length, "%")));
#pragma warning restore CA1303 // Do not pass literals as localized parameters
		}
	}
}
