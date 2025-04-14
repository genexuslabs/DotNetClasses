using System;
using GeneXus.Application;
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
		public void MontevideoTimeZoneConversion_offset3()
		{
			DateTime dt = new DateTime(1967, 10, 28, 7, 10, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(1967, 10, 28, 4, 10, 0, DateTimeKind.Local);

			#region NodaTime
			DateTime result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

			#region TZ4Net
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone mdeoTimezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			result = DateTimeUtil.DBserver2local(dt, mdeoTimezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result);
			#endregion

			dt = new DateTime(1976, 4, 8, 22, 31, 0, DateTimeKind.Utc);
			expected = new DateTime(1976, 4, 8, 19, 31, 0, DateTimeKind.Local);

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

			#region TZ4Net
#pragma warning disable CS0618 // Type or member is obsolete
			result = DateTimeUtil.DBserver2local(dt, mdeoTimezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result);
			#endregion

		}
#if NETCORE
		[Fact]
		public void MontevideoTimeZoneConversion_offset2()
		{
			DateTime dt = new DateTime(2012, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(2012, 1, 1, 12, 0, 0, DateTimeKind.Local);


			#region TZ4Net
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone mdeoTimezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.Local2DBserver(dt, mdeoTimezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result);
			#endregion
			
			#region NodaTime
			result = DateTimeUtil.Local2DBserver(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion
		}
#endif
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
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(GUADALAJARA_IANA_TIMEZONE_ID);
			result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
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
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone parisTimezone = TimeZoneUtil.GetInstanceFromOlsonName(PARIS_IANA_TIMEZONE_ID);
			result1 = DateTimeUtil.DBserver2local(dt1, parisTimezone);
			result2 = DateTimeUtil.DBserver2local(dt2, parisTimezone);
			result3 = DateTimeUtil.DBserver2local(dt3, parisTimezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected1, result1);
			Assert.Equal(expected2, result2);
			Assert.Equal(expected3, result3);
			#endregion

		}
		[Fact]
		public void Year1753Conversion()
		{
			DateTime dt = new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(1752, 12, 31, 21, 0, 0, DateTimeKind.Unspecified);

			#region T4ZNet
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result); 
			#endregion

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void Year1901MidDayConversion()
		{
			
			DateTime dt = new DateTime(1901, 12, 13, 12, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(1901, 12, 13, 9, 0, 0, DateTimeKind.Unspecified);

			#region T4ZNet
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result);
			#endregion

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void Year1901Conversion()
		{

			DateTime dt = new DateTime(1901, 12, 30, 12, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(1901, 12, 30, 8, 15, 9, DateTimeKind.Unspecified);

			#region T4ZNet
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result);
			#endregion

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void Year1902Conversion()
		{

			DateTime dt = new DateTime(1902, 12, 30, 12, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(1902, 12, 30, 8, 15, 9, DateTimeKind.Unspecified);

			#region T4ZNet
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result);
			#endregion

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void Year1901MorningConversion()
		{

			DateTime dt = new DateTime(1901, 12, 13, 11, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(1901, 12, 13, 8, 0, 0, DateTimeKind.Unspecified);

			#region T4ZNet
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result); 
			#endregion

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void Year1901AfternoonConversion()
		{

			DateTime dt = new DateTime(1901, 12, 13, 15, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(1901, 12, 13, 12, 0, 0, DateTimeKind.Unspecified);

			#region T4ZNet
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result); 
			#endregion

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void Year2039Conversion()
		{
			DateTime dt = new DateTime(2039, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(2038, 12, 31, 21, 0, 0, DateTimeKind.Unspecified);

			#region T4ZNet
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result); 
			#endregion

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void Year2050Conversion()
		{
			DateTime dt = new DateTime(2050, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime expected = new DateTime(2049, 12, 31, 21, 0, 0, DateTimeKind.Unspecified);

			#region T4ZNet
#pragma warning disable CS0618 // Type or member is obsolete
			OlsonTimeZone timezone = TimeZoneUtil.GetInstanceFromOlsonName(MONTEVIDEO_IANA_TIMEZONE_ID);
			DateTime result = DateTimeUtil.DBserver2local(dt, timezone);
#pragma warning restore CS0618 // Type or member is obsolete
			Assert.Equal(expected, result);
			#endregion

			#region NodaTime
			result = DateTimeUtil.DBserver2local(dt, MONTEVIDEO_IANA_TIMEZONE_ID);
			Assert.Equal(expected, result);
			#endregion

		}
		[Fact]
		public void TimeZoneInJsonTime()
		{
			DateTime value = DateTimeUtil.CToT3("19:00:00");
			DateTime expected = DateTime.MinValue.AddHours(19);
			Assert.Equal(expected, value);

			value = DateTimeUtil.CToT3("19:00:00.000");
			Assert.Equal(expected, value);

			value = DateTimeUtil.CToT3("1899-12-31T19:00:00.000");
			Assert.Equal(expected, value);

			value = DateTimeUtil.CToT3("0000-00-00T19:00:00.000");
			Assert.Equal(expected, value);

			value = DateTimeUtil.CToT3("0001-01-01T19:00:00.000");
			Assert.Equal(expected, value);

			value = DateTimeUtil.CToT3("2025-04-14T19:00:00.000");
			Assert.Equal(expected, value);
		}
	}
}
