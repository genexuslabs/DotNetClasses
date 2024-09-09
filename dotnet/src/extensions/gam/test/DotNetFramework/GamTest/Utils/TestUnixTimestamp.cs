using System;
using NUnit.Framework;
using System.Globalization;
using GamUtils;

namespace GamTest.Utils
{ 
	[TestFixture]
	public class TestUnixTimestamp
	{

		[Test]
		public void TestCreate()
		{
			DateTime one = CreateDate("2024/02/02 02:02:02"); //1706839322
			DateTime two = CreateDate("2023/03/03 03:03:03"); //1677812583
			DateTime three = CreateDate("2022/04/04 04:04:04"); //1649045044
			DateTime four = CreateDate("2020/02/02 02:22:22"); //1580610142
			DateTime five = CreateDate("2010/05/05 05:05:05"); //1273035905
			DateTime six = CreateDate("2000/05/05 05:05:05"); //957503105

			DateTime[] arrayDates = new DateTime[] { one, two, three, four, five, six };
			long[] arrayStamps = new long[] { 1706839322L, 1677812583L, 1649045044L, 1580610142L, 1273035905L, 957503105L};

			for (int i = 0; i < arrayDates.Length; i++)
			{
				Assert.AreEqual(GamUtilsEO.CreateUnixTimestamp(arrayDates[i]), arrayStamps[i], "testCreate");
			}
		}

		private static DateTime CreateDate(string date)
		{
			return DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
		}

	}
}
