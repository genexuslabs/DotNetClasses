using System;
using GeneXus.Utils;
using TZ4Net;
using Xunit;

namespace xUnitTesting
{

	public class TimeZoneTest
	{
		const string MONTEVIDEO_IANA_TIMEZONE_ID = "America/Montevideo";
		const string GUADALAJARA_IANA_TIMEZONE_ID = "America/Mexico_City";
		const string PARIS_IANA_TIMEZONE_ID = "Europe/Paris";
		[Fact]
		public void TimeZoneConversion()
		{
			DateTime dt = new DateTime(1967, 10, 28, 7, 10, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(1967, 10, 28, 4, 10, 0, DateTimeKind.Local);

			#region NodaTime
			DateTime result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

			#region TZ4Net
			OlsonTimeZone mdeoTimezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			result = DateTimeUtil.DBserver2local(dt, mdeoTimezone);
			Assert.Equal(expected, result);
			#endregion

			dt = new DateTime(1976, 4, 8, 22, 31, 0, DateTimeKind.Utc);
			expected = new DateTime(1976, 4, 8, 19, 31, 0, DateTimeKind.Local);

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

			#region TZ4Net
			result = DateTimeUtil.DBserver2local(dt, mdeoTimezone);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void MexicoTimeZoneConversion()
		{
			DateTime dt = new DateTime(2023, 5, 28, 7, 10, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(2023, 5, 28, 1, 10, 0, DateTimeKind.Unspecified);

			#region NodaTime
			DateTime result = DateTimeUtil.DBserver2local(dt, GUADALAJARA_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

			#region T4ZNet
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(GUADALAJARA_IANA_TIMEZONE_ID);
			result = DateTimeUtil.DBserver2local(dt, timezone);
			Assert.Equal(expected.AddHours(1), result); //Olson has a mistake with this timezone
			#endregion
		}
		[Fact]
		public void EuropeTimeZone()
		{
			DateTime dt1 = new DateTime(2015, 3, 29, 2, 30, 0, DateTimeKind.Utc);
			DateTime expected1 = new DateTime(2015, 3, 29, 4, 30, 0, DateTimeKind.Unspecified);

			DateTime dt2 = new DateTime(2015, 6, 19, 2, 30, 0, DateTimeKind.Utc);
			DateTime expected2 = new DateTime(2015, 6, 19, 4, 30, 0, DateTimeKind.Unspecified);

			DateTime dt3 = new DateTime(2015, 10, 25, 2, 30, 0, DateTimeKind.Utc);
			DateTime expected3 = new DateTime(2015, 10, 25, 3, 30, 0, DateTimeKind.Unspecified);

			#region NodaTime
			DateTime result1 = DateTimeUtil.DBserver2local(dt1, PARIS_IANA_TIMEZONE_ID);
			DateTime result2 = DateTimeUtil.DBserver2local(dt2, PARIS_IANA_TIMEZONE_ID);
			DateTime result3 = DateTimeUtil.DBserver2local(dt3, PARIS_IANA_TIMEZONE_ID);
			Assert.Equal(expected1, result1);
			Assert.Equal(expected2, result2);
			Assert.Equal(expected3, result3);
			#endregion

			#region T4ZNet
			OlsonTimeZone parisTimezone = TimeZoneUtil.GetInstanceFromOlsonName(PARIS_IANA_TIMEZONE_ID);
			result1 = DateTimeUtil.DBserver2local(dt1, parisTimezone);
			result2 = DateTimeUtil.DBserver2local(dt2, parisTimezone);
			result3 = DateTimeUtil.DBserver2local(dt3, parisTimezone);
			Assert.Equal(expected1, result1);
			Assert.Equal(expected2, result2);
			Assert.Equal(expected3, result3);
			#endregion

		}
	}
}
