#region Copyright (c) 2010 Zbigniew Babiej

/* 
 * 
Written by Zbigniew Babiej, zbabiej@yahoo.com.
*/

#endregion

#region Using directives

using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;

#endregion

namespace TZ4Net
{
	/// <summary>
	/// A TimeZone Implementation with Historical Changes and Leapseconds.
	/// Implementation of the local time zone conversion based on Olson database.
	/// This public-domain time zone database contains code and data that represent the history of local time 
	/// for many representative locations around the globe. It is updated periodically to reflect changes made 
	/// by political bodies to UTC offsets and daylight-saving rules. This database (often called tz or zoneinfo) 
	/// is used by several implementations, including the GNU C Library used in GNU/Linux, FreeBSD, NetBSD, 
	/// OpenBSD, Cygwin, DJGPP, HP-UX, IRIX, Mac OS X, OpenVMS, Solaris, Tru64, and UnixWare.
	/// As opposite to Win32/.NET API, it allows to perform convertion in arbitrary time zone
	///
	/// The code is based on Stuart D. Gathman's Java translation of the  Unix "tz" package (formerly known as 
	/// "localtime"). See http://www.twinsun.com/tz/tz-link.htm and http://www.bmsi.com/java/#TZ for more details.
	/// It uses Jon Skeet's EndianBitConverter. See http://www.yoda.arachsys.com/csharp/miscutil/ for more details.
	/// </summary>
	public class ZoneInfo
	{
		#region Nested types

		/// <summary>
		/// Container for time local variables.
		/// </summary>
		public class Time
		{
			#region Fields

			/// <summary>
			/// Hour of day, 0 - 23.
			/// </summary>
			internal int hour;
		
			/// <summary>
			/// Minute of hour, 0 - 59.
			/// </summary>
			internal int min;
		
			/// <summary>
			/// Second of minute, 0 - 60.
			/// Note: that value may be 60 on a leap second. 
			/// </summary>
			internal int sec;
		
			/// <summary>
			/// Day of week, 0 - 6, 0 = Sunday
			/// </summary>
			internal int wday;
		
			/// <summary>
			/// Years since 1900.
			/// </summary>
			internal int year;

			/// <summary>
			/// Day of year, 1 - 366.
			/// </summary>
			internal int yday;

			/// <summary>
			/// Month of year, 0 - 11.
			/// </summary>
			internal int mon;
		
			/// <summary>
			/// Day of month, 1 - 31.
			/// </summary>
			internal int mday;

			/// <summary>
			/// True if time is DST.
			/// </summary>
			internal bool isDst;

			/// <summary>
			/// Timezone name.
			/// </summary>
			internal string zone;

			private const int SECSPERMIN	= 60;
			private const int MINSPERHOUR	= 60;
			private const int HOURSPERDAY	= 24;
			private const int DAYSPERWEEK	= 7;
			private const int SECSPERHOUR	= SECSPERMIN * MINSPERHOUR;
			private const int SECSPERDAY	= SECSPERHOUR * HOURSPERDAY;
			private const int THURSDAY	= 4;
			private const int EPOCH_WDAY	= THURSDAY;
			private const int DAYSADJ = 25203;	// days between 1900 & 1970
			private const int CENT_WDAY = EPOCH_WDAY - DAYSADJ % 7;

			/// <summary>
			/// Min. time represented by signed 32-bits integer.
			/// </summary>
			public static readonly Time MinValue = new ZoneInfo(UtcName).GetUtcTime(MinClock);
			/// <summary>
			/// Max. time represented by signed 32-bits integer.
			/// </summary>
			public static readonly Time MaxValue = new ZoneInfo(UtcName).GetUtcTime(MaxClock);

			#endregion

			#region Constructors

			/// <summary>
			/// Default constructor
			/// </summary>
			internal Time()
			{
			}

			/// <summary>
			/// Initialize a new <see cref="Time"/> object to calendar day and time offset.
			/// </summary>
			/// <param name="year">Years since 1900.</param>
			/// <param name="mon">Month 0-11</param>
			/// <param name="day">Day of month 1-31.</param>
			/// <param name="secs">Seconds in day.</param>
			internal Time(int year, int mon, int day, int secs) 
			{
				this.year = year;
				this.mon = mon;
				this.mday = day;
				SetSecs(secs);
			}

			/// <summary>
			/// Creates the instance from "normalized" values.
			/// </summary>
			/// <param name="year">Year.</param>
			/// <param name="mon">Month of year 1-12.</param>
			/// <param name="day">Day of month 1-31.</param>
			/// <param name="hour">Hour 0-23.</param>
			/// <param name="min">Minute 0-59.</param>
			/// <param name="sec">Second 0-60.</param>
			/// <returns></returns>
			public Time(int year, int mon, int day, int hour, int min, int sec) 
				: this(checked(year - 1900), checked(mon - 1), day, hour*3600 + min*60 + sec)
			{
			}

			/// <summary>
			/// Creates the instance from "normalized" values.
			/// </summary>
			/// <param name="dt">DateTime instance to create the object from.</param>
			/// <returns></returns>
			public Time(DateTime dt) 
				: this(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second)
			{
			}

			#endregion

			/// <summary>
			/// String representation of the object.
			/// </summary>
			/// <returns><see cref="string"></see> representation of the object.</returns>
			public override string ToString()
			{
				return string.Format("{0}-{1:00}-{2:00} {3:00}:{4:00}:{5:00} {6}", year + 1900, mon + 1, mday, hour, min, sec, zone);
			}

			/// <summary>
			/// Gets the equivalent DateTime instance.
			/// </summary>
			public DateTime DateTime 
			{
				get 
				{
					int leapSeconds = sec - 59;
					if (leapSeconds > 0) 
					{
						return new DateTime(year + 1900, mon + 1, mday, hour, min, 59).AddSeconds(leapSeconds);
					} 
					else 
					{
						return new DateTime(year + 1900, mon + 1, mday, hour, min, sec);
					}
				}
			}

			/// <summary>
			/// Determines whether the specified object is equal to the current object.
			/// </summary>
			/// <param name="obj">Object instance to compare to.</param>
			/// <returns><see cref="bool"></see> value indicating if both objects are equal.</returns>
			public override bool Equals(object obj) 
			{
				if (obj == null || !obj.GetType().IsSubclassOf(this.GetType())) 
				{ 
					return false;
				}
				return CompareTo((Time)obj) == 0;
			}

			/// <summary>
			/// Types that override Equals must also override GetHashCode.
			/// </summary>
			/// <returns>Hashcode of the instance.</returns>
			public override int GetHashCode() 
			{
				return (year << 24) + (mon << 20) + (mday << 15) + (hour << 10) + (min << 5) + sec;
			}

			/// <summary>
			/// Set the local time fields from a clock and GMT offset.
			/// </summary>
			/// <param name="clock">Seconds since 1970</param>
			/// <param name="offset">Offset from UT in seconds</param>
			public void SetClock(long clock, int offset) 
			{
				int days = (int)(clock / SECSPERDAY);
				int secs = (int)(clock % SECSPERDAY);
				secs += offset;
				while (secs < 0) 
				{
					secs += SECSPERDAY;
					days--;
				}
				while (secs >= SECSPERDAY) 
				{
					secs -= SECSPERDAY;
					days++;
				}
      
				SetSecs(secs);
      
				int doc = days + DAYSADJ;
				wday = (CENT_WDAY + doc) % DAYSPERWEEK;

				// now compute date from days since EPOCH
				int leapyear = 2;								// not leapyear adj = 2 
				// 1461 days in 4 years 
				year = (doc - doc/1461 + 364) / 365;			// calculate year 
				yday = doc - ((year - 1) * 1461) / 4;			// day of year conversion 
				if (year % 4 == 0)								// is this a leapyear? 
				{							
					leapyear = 1;								// yes - reset adj to 1 
				}
				if (yday > 59 && (yday > 60 || leapyear == 2)) 
				{
					yday += leapyear;							// correct for leapyear 
				}

				mon = (269 + yday * 9) / 275;					// calculate month 
				mday = yday + 30 - 275 * mon / 9;				// calc day of month 
				mon--;											// unix convention
			}


			/// <summary>
			/// Hour of day, 0 - 23.
			/// </summary>
			public int Hour 
			{
				get 
				{
					return hour;
				}
			}
		
			/// <summary>
			/// Minute of hour, 0 - 59.
			/// </summary>
			public int Min 
			{
				get 
				{
					return min;
				}
			}

			/// <summary>
			/// Second of minute, 0 - 60.
			/// Note: that value may be 60 on a leap second. 
			/// </summary>
			public int Sec 
			{
				get 
				{
					return sec;
				}
			}
		
			/// <summary>
			/// Day of week, 0 - 6, 0 = Sunday
			/// </summary>
			public int WDay 
			{
				get 
				{
					return wday;
				}
			}
	
			/// <summary>
			/// Year.
			/// </summary>
			public int Year 
			{
				get 
				{
					return year + 1900;
				}
			}

			/// <summary>
			/// Day of year, 1 - 366.
			/// </summary>
			public int YDay 
			{
				get 
				{
					return yday;
				}
			}

			/// <summary>
			/// Month of year, 1 - 12.
			/// </summary>
			public int Mon 
			{
				get 
				{
					return mon + 1;
				}
			}
		
			/// <summary>
			/// Day of month, 1 - 31.
			/// </summary>
			public int MDay 
			{
				get 
				{
					return mday;
				}
			}

			/// <summary>
			/// True if time is DST.
			/// </summary>
			public bool IsDst
			{
				get 
				{
					return isDst;
				}
			}

			/// <summary>
			/// Timezone name.
			/// </summary>
			public string Zone 
			{
				get 
				{
					return zone;
				}
			}

			/// <summary>
			/// Compares to other instance of <see cref="Time"></see> object.
			/// </summary>
			/// <param name="time">Instance to compare current object to.</param>
			/// <returns>-1, 0 or 1 indicating the comparison result.</returns>
			public int CompareTo(Time time) 
			{
				if (year != time.year) 
				{
					return year - time.year;
				}
				if (mon != time.mon) 
				{
					return mon - time.mon;
				}
				if (mday != time.mday) 
				{
					return mday - time.mday;
				}
				if (hour != time.hour) 
				{
					return hour - time.hour;
				}
				if (min != time.min) 
				{
					return min - time.min;
				}
				return sec - time.sec;
			}

			#region Implementation

			/// <summary>
			/// Calculates time values from the number of seconds.
			/// </summary>
			/// <param name="secs">Number of seconds.</param>
			private void SetSecs(int secs) 
			{
				hour = secs / SECSPERHOUR;
				int rem = secs % SECSPERHOUR;
				min =  rem / SECSPERMIN;
				sec = rem % SECSPERMIN;
			}

			#endregion
		}

		/// <summary>
		/// Summary description for Rule.
		/// </summary>
		public class Rule
		{
			#region Fields

			/// <summary>
			/// Offset from GMT in seconds. 
			/// </summary>
			private readonly int offset;

			/// <summary>
			/// Name of the rule. 
			/// </summary>
			private readonly string name;

			/// <summary>
			/// True if daylight savings time. 
			/// </summary>
			private readonly bool isDst;

			#endregion

			#region Constructors

			/// <summary>
			/// Creates the instance from name, offset and the DST flag.
			/// </summary>
			/// <param name="name">Name of the rule.</param>
			/// <param name="offset">Offset of the rule.</param>
			/// <param name="isDst">Flag indicating if this is DST rule.</param>
			internal Rule(string name, int offset, bool isDst) 
			{
				this.name = name;
				this.offset = offset;
				this.isDst = isDst;
			}

			#endregion

			#region Public interface

			/// <summary>
			/// Gets the offset of the zone.
			/// </summary>
			public int Offset 
			{
				get 
				{
					return offset;
				}
			}

			/// <summary>
			/// Gets the name of the rule. 
			/// </summary>
			public string Name 
			{
				get 
				{
					return name;
				}
			}

			/// <summary>
			/// Gets the True if daylight savings time. 
			/// </summary>
			public bool IsDST 
			{
				get 
				{
					return isDst;
				}
			}

			/// <summary>
			/// Converts the instance to a string.
			/// </summary>
			/// <returns>String representation of the instance.</returns>
			public override string ToString() 
			{
				return string.Format("Type: {0} Offset: {1} DST: {2}", name, offset, isDst);
			}

			#endregion
		}

		#endregion

		#region Fields

		/// <summary>
		/// Number of .NET ticks at unix epoch.
		/// </summary>
		public static readonly long unixEpochTicks = new DateTime(1970, 01, 01, 00, 00, 00, 000).Ticks;
	
		/// <summary>
		/// Transition times.
		/// </summary>
		private int[] transTimes;

		/// <summary>
		/// Rule index for each transition.
		/// </summary>
		private byte[] transTypes;

		/// <summary>
		/// Transition rules.
		/// </summary>
		private Rule[] rules;
		
		/// <summary>
		/// Leapseconds.
		/// </summary>
		private int[] leapSecs;

		/// <summary>
		/// Caches the name of this zoneinfo passed in constructor.
		/// </summary>
		private string name;

		/// <summary>
		/// Caches the directory name of this zoneinfo passed in constructor.
		/// </summary>
		private string dir;

		/// <summary>
		/// Caches the base name of the TZ database resource.
		/// </summary>
		private const string baseName = "zoneinfo";

		/// <summary>
		/// Caches the resource file name of the TZ database resource, like MyResource.resources.
		/// </summary>
		private static readonly string resFileName = string.Format("{0}.resources", baseName);

		/// <summary>
		/// Root name of the TZ database resource, like MyAssembly.MyResource.
		/// </summary>
		private static readonly string resRootName = string.Format("{0}.{1}", Assembly.GetExecutingAssembly().GetName().Name, baseName);


		/// <summary>
		/// Name of the resource containing the names of databases embedded as assembly resources.
		/// </summary>
		private static readonly string resMetaInfoName = "metainfo";

		/// <summary>
		/// Holds the default zoneinfo directory name. Used when consturctor with no
		/// zoneinfo directory name is used.
		/// </summary>
		public const string DefaultDir = "zoneinfo";

		/// <summary>
		/// Holds the default UTC name string. Can be used to find out if default name comes from TZ variable.
		/// </summary>
		public static readonly string DefaultUtcName = "Etc/UTC";

		/// <summary>
		/// Holds the UTC name string.
		/// </summary>
		public const string UtcName = "UTC";

		/// <summary>
		/// Smallest value of unix clock.
		/// </summary>
		public const long MinClock = int.MinValue;

		/// <summary>
		/// Biggest value of unix clock.
		/// </summary>
		public const long MaxClock = int.MaxValue;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the zoneinfo instance from given name and zoneinfo directory.
		/// </summary>
		/// <param name="name">Name of zoneinfo.</param>
		/// <param name="dir">Directory name.</param>
		public ZoneInfo(string name, string dir)
		{
			if (name == null) 
			{
				throw new System.ArgumentNullException("name", "Name can not be null.");
			}
			if (dir == null) 
			{
				throw new System.ArgumentNullException("dir", "Directory name can not be null.");
			}
			if (Array.IndexOf(AllDirs, dir) < 0) 
			{
				throw new System.ArgumentException(string.Format("Directory name '{0}' not found.", dir));
			}
			ResourceManager resourceManager = new ResourceManager(resRootName, Assembly.GetExecutingAssembly());
			byte[] resource = (byte[])resourceManager.GetObject(string.Format("{0}/{1}", dir, name));
			if (resource == null) 
			{
				throw new System.ArgumentException(string.Format("Name '{0}' not found in directory '{1}'.", name, dir));			
			}

			EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Big, new BufferedStream(new MemoryStream(resource, false)));

			try 
			{
				// read header
				reader.Seek(28, SeekOrigin.Begin);
				int leapCount = reader.ReadInt32();
				int timeCount = reader.ReadInt32();
				int ruleCount = reader.ReadInt32();
				int charCount = reader.ReadInt32();

				// load transition data
				transTimes = new int[timeCount];
				for (int i = 0; i < timeCount; i++) 
				{
					transTimes[i] = reader.ReadInt32();
				}
				transTypes = new byte[timeCount];
				reader.Read(transTypes, 0, timeCount);

				// load rule data
				int[] offset = new int[ruleCount];
				byte[] dst = new byte[ruleCount];
				byte[] idx = new byte[ruleCount];
				for (int i = 0; i < ruleCount; i++) 
				{
					offset[i] = reader.ReadInt32();
					dst[i] = reader.ReadByte();
					idx[i] = reader.ReadByte();
				}
				byte[] str = new byte[charCount];
				reader.Read(str, 0, charCount);

				// convert rule data
				rules = new Rule[ruleCount];
				for (int i = 0; i < ruleCount; i++) 
				{
					// find string
					int pos = idx[i];
					int end = pos;
					while (str[end] != 0) end++;
					char[] chars = new char[end - pos];
					int decodedChars = Encoding.ASCII.GetDecoder().GetChars(str, pos, end - pos, chars, 0);
					System.Diagnostics.Debug.Assert(decodedChars == end - pos, "Unexpected number of decoded characters.");
					rules[i] = new Rule(new string(chars), offset[i], dst[i] != 0);
				}

				// load leap seconds table
				leapSecs = new int[leapCount * 2];
				for (int i = 0; leapCount > 0; leapCount--) 
				{
					leapSecs[i++] = reader.ReadInt32();
					leapSecs[i++] = reader.ReadInt32();
				}
			} 
			finally 
			{
				reader.Close();
			}

			this.name = name;
			this.dir = dir;
		}


		/// <summary>
		/// Constructs zoneinfo instance from the given name and default zoneinfo directory.
		/// </summary>
		/// <param name="name">Name of zoneinfo.</param>
		public ZoneInfo(string name) : this(name, DefaultDir)
		{
		}


		/// <summary>
		/// Constructs default zoneinfo instance.
		/// </summary>
		public ZoneInfo() : this(DefaultName, DefaultDir)
		{
		}


		#endregion

		#region Implementation

		/// <summary>
		/// Gets "normaltz" (the default rule) for timezone.
		/// </summary>
		internal Rule DefaultStdRule 
		{
			get 
			{
				int i = 0;
				while (rules[i].IsDST && i < rules.Length) 
				{
					i++;
				}
				return rules[i];
			}
		}

		/// <summary>
		/// Gets default DST for timezone.
		/// </summary>
		internal Rule DefaultDstRule 
		{
			get 
			{
				for (int i = rules.Length - 1; i >= 0; i--) 
				{
					if (rules[i].IsDST) 
					{
						return rules[i];
					}
				}
				return null;
			}
		}

		
		/// <summary>
		/// Finds a DST rule close to the given unix time.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <param name="isDst">Type of the rule to find.</param>
		/// <returns>Instance of <see cref="Rule"/> representing next DST rule.</returns>
		private Rule FindNearestRule(long clock, bool isDst) 
		{
			Rule res = null;
			int iDown = FindTransitionIndex(clock);
			int iUp = iDown + 1;
			int iMax = transTypes.Length;
			while ((iDown >= 0 && iDown < iMax) || (iUp >= 0 && iUp < iMax)) 
			{
				if (iDown >= 0 && iDown < iMax)
				{
					if (rules[transTypes[iDown]].IsDST == isDst) 
					{
						res = rules[transTypes[iDown]];
						break;
					}
				}
				if ((iUp >= 0 && iUp < iMax)) 
				{
					if (rules[transTypes[iUp]].IsDST == isDst) 
					{
						res = rules[transTypes[iUp]];
						break;
					}
				}
				iDown --;
				iUp ++;
			}
			return res;
		}


		/// <summary>
		/// When calculating "normaltz" (the default rule) for timezone,
		/// we originally took the first non-DST rule for the current TZ.
		/// But this produces nonsensical results for areas where historical
		/// non-integer time zones were used, e.g. if GMT-2:33 was used until 1918.
		/// This loop, based on a suggestion by Ophir Bleibergh, tries to find a 
		/// non-DST rule close to the given unix time. This is somewhat of a hack, 
		/// but much better than the previous solution taking first non-DST rule.
		/// Tricky: we need to get either the next or previous non-DST TZ.
		/// We shall take the most recent non-DST value.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <returns>Instance of <see cref="Rule"/> representing most recent non-DST rule.</returns>
		private Rule FindNearestNonDstRule(long clock) 
		{
			return FindNearestRule(clock, false);
		}

		
		/// <summary>
		/// Finds a DST rule close to the given unix time.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <returns>Instance of <see cref="Rule"/> representing next DST rule.</returns>
		private Rule FindNearestDstRule(long clock) 
		{
			return FindNearestRule(clock, true);
		}


		/// <summary>
		/// Gets non-DST rule close to the given unix time.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <returns>Instance of <see cref="Rule"/> representing most recent non-DST rule.</returns>
		private Rule GetNearestNonDstRule(long clock) 
		{
			Rule rule = FindNearestNonDstRule(clock);
			return rule != null ? rule : DefaultStdRule;
		}


		/// <summary>
		/// Gets DST rule close to the given unix time.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <returns>Instance of <see cref="Rule"/> representing most recent non-DST rule.</returns>
		private Rule GetNearestDstRule(long clock) 
		{
			Rule rule = FindNearestDstRule(clock);
			return rule != null ? rule : DefaultDstRule;
		}


		/// <summary>
		/// Returns the "normaltz" (the default rule) for this zoneinfo for a given unix time.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <returns>Instance of <see cref="Rule"/> representing default rule.</returns>
		private Rule GetNormalRule(long clock) 
		{
			return GetNearestNonDstRule(clock);
		}


		/// <summary>
		/// Returns rule for a specified unix time.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <returns>Instance of <see cref="Rule"/> representing rule for a given unix time.</returns>
		public Rule GetRule(long clock) 
		{
			Rule rule = FindRule(clock);
			if (rule == null) 
			{
				rule = DefaultStdRule;
			}
			return rule;
		}


		/// <summary>
		/// Gets the base name of the zoneinfo database.
		/// </summary>
		internal static string BaseName
		{
			get 
			{
				return baseName;
			}
		}


		/// <summary>
		/// Gets the name of the resource containing the names of databases embedded as assembly resources.
		/// </summary>
		internal static string MetaInfoResourceName
		{
			get 
			{
				return resMetaInfoName;
			}
		}


		/// <summary>
		/// Gets the resource file name of the zoneinfo database.
		/// </summary>
		internal static string ResourceFileName
		{
			get 
			{
				return resFileName;
			}
		}


		/// <summary>
		/// Finds transition index the instance should use for given unix time.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <returns>Index in transition tables.</returns>
		internal int FindTransitionIndex(long clock) 
		{
			// FIXME: use binary search
			if (transTimes.Length > 0 && clock >= transTimes[0]) 
			{
				int i = 1;
				for (; i < transTimes.Length; i++) 
				{
					if (clock < transTimes[i]) 
					{
						break;
					}
				}
				return i - 1;
			}
			return -1;
		}


		/// <summary>
		/// Finds a rule the instance should use for given unix time.
		/// </summary>
		/// <param name="clock">Unix time clock i.e number of seconds since epoch.</param>
		/// <returns>Instance of <see cref="Rule"></see> object representing the rule to be used for a given time clock.</returns>
		internal Rule FindRule(long clock) 
		{
			int index = FindTransitionIndex(clock);
			if (index >= 0) 
			{
				return rules[transTypes[index]];
			}
			return null;
		}


		/// <summary>
		/// Applies the rule and leapsecond correction to given unix time.
		/// The result is passed in computed properties of <see cref="Time"/> local time.
		/// </summary>
		/// <param name="clock">Seconds since 1970.</param>
		/// <param name="rule">Rule to apply.</param>
		/// <param name="t">Local time properties to set.</param>
		/// <returns>The offset from GMT including rule, DST, and leap seconds.</returns>
		internal int ApplyRule(long clock, Rule rule, Time t) 
		{
			bool hit = false;
			int offset = (rule == null) ? 0 : rule.Offset;

			for (int i = leapSecs.Length; (i -= 2) >= 0;) 
			{
				int time = leapSecs[i];
				int correction = leapSecs[i + 1];
				if (clock >= time) 
				{
					if (clock == time) 
					{
						hit = ((i == 0 && correction > 0) || correction > leapSecs[i - 1]);
					}
					offset -= correction;
					break;
				}
			}

			t.SetClock(clock, offset);

			// A positive leap second requires a special
			// representation.  This uses "... ??:59:60".
			if (hit) 
			{
				t.sec += 1;
			}

			if (rule != null) 
			{
				t.isDst = rule.IsDST;
				t.zone = rule.Name;
			}
			else 
			{
				t.isDst = false;
				t.zone = UtcName;
			}

			return offset;
		}


		/// <summary>
		/// Calculates seconds since the epoch, the reverse of GetLocalTime() and GetUtcTime(). Unused fields are computed and stored in time.
		/// </summary>
		/// <param name="time"> Time to convert. The Year, Mon, MDay, Hour, Min, Sec fields are used and validated. Other fields are computed.</param>
		/// <param name="local">Flag indicating whether the passed time is a local or utc for the zone.</param>
		/// <returns>Seconds since the epoch.</returns>
		private long GetClock(Time time, bool local) 
		{
			int clock = 0;
			int bits = 31;
			Time t = new Time();
			// use binary search
			// FIXME: make smarter initial guess?
			for (;;) 
			{
				Rule rule = local ? GetRule(clock) : null;
				ApplyRule(clock, rule, t);
				int direction = t.CompareTo(time);
				if (direction == 0) 
				{
					time.wday	= t.wday;
					time.yday	= t.yday;
					time.isDst	= t.isDst;
					time.zone	= t.zone;
					return clock;
				}
				if (bits-- < 0) 
				{
					throw new ArgumentException(string.Format("Bad time: {0}. Binary search failed.", time), "time");
				}
				if (bits < 0) 
				{
					clock--;
				}
				else if (direction > 0) 
				{
					clock -= 1 << bits;
				}
				else 
				{
					clock += 1 << bits;
				}
			}
		}

		#endregion

		#region Public interface

		/// <summary>
		/// Gets all zoneinfo directories stored in zoneinfo database.
		/// </summary>
		public static string[] AllDirs
		{
			get 
			{
#if NETCORE
				string[] dirNames = new string[] { "zoneinfo" };
#else
				ResourceManager resourceManager = new ResourceManager(resRootName, Assembly.GetExecutingAssembly());
				string[] dirNames = (string[])resourceManager.GetObject(resMetaInfoName);
#endif
				Debug.Assert(dirNames != null, string.Format("{0} object not found in {1} resources.", resMetaInfoName, resRootName));
				return dirNames;
			}
		}

		/// <summary>
		/// Returns all zoneinfo names for given zoneinfo directory.
		/// </summary>
		/// <param name="dir">Name of zoneinfo directory.</param>
		/// <returns>List of  all zoneinfo names from given zoneinfo directory.</returns>
		public static string[] GetAllNames(string dir)
		{
			if (dir == null) 
			{
				throw new ArgumentNullException("dir", "Directory name can not be null.");
			}
#if !NETCORE
			ResourceManager rm = new ResourceManager(resRootName, Assembly.GetExecutingAssembly());
			ResourceSet rs = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
			
			ArrayList names = new ArrayList();
			string pattern = string.Format("{0}/", dir);
			foreach (DictionaryEntry entry in rs)
			{
				string key = (string)entry.Key;
				if (key.StartsWith(pattern))
				{
					names.Add(key.Substring(pattern.Length));
				}
			}
			rs.Close();
			return (string[])names.ToArray(typeof(string));
#else
			return ZoneInfoConsts.AllNames;
#endif
		}

		/// <summary>
		/// Gets all zoneinfo names in default zoneinfo directory.
		/// </summary>
		public static string[] AllNames
		{
			get 
			{
				return GetAllNames(DefaultDir);
			}
		}


		/// <summary>
		/// Gets the name of this zoneinfo.
		/// </summary>
		public string Name 
		{
			get 
			{
				return name;
			}
		}


		/// <summary>
		/// Gets the directory name of this zoneinfo.
		/// </summary>
		public string Dir
		{
			get 
			{
				return dir;
			}
		}


		/// <summary>
		/// Gets the ID of this zoneinfo. 
		/// </summary>
		public string ID
		{
			get 
			{
				return NormalRule.Name;
			}
		}
		

		/// <summary>
		/// Gets the amount of time in milliseconds to add to UTC to get standard time in this zoneinfo.
		/// </summary>
		public int RawOffset 
		{
			get 
			{
				return NormalRule.Offset * 1000;
			}
		}


		/// <summary>
		/// Gets the unix times of all transitions for this zoneinfo.
		/// </summary>
		public long[] AllTransitionClocks
		{
			get 
			{
				long[] res = new long[transTimes.Length];
				Array.Copy(transTimes, res, res.Length);
				return res;
			}
		}


		/// <summary>
		/// Gets all transition rules of this zoneinfo.
		/// </summary>
		public Rule[] AllRules
		{
			get 
			{
				return rules;
			}
		}


		/// <summary>
		/// Gets default transition rule for this zoneinfo.
		/// </summary>
		public Rule NormalRule 
		{
			get 
			{
				return GetNormalRule(CurrentClock);
			}
		}


		/// <summary>
		/// Gets the unix times of transitions observed in given year for this zoneinfo.
		/// </summary>
		/// <param name="year">Year for which the transitions were observed.
		/// </param>
		/// <returns>Array of unix times at which the transitions were observed.</returns>
		public long[] GetTransitionClocks(int year) 
		{
			ArrayList resClocks = new ArrayList();
			foreach (long clock in transTimes) 
			{
				int clockYear = GetUtcTime(clock).year;
				if (clockYear == checked(year - 1900)) 
				{
					resClocks.Add(clock);
				}
			}
			return (long[])resClocks.ToArray(typeof(long));
		}


		/// <summary>
		/// Gets the tranzition rule for a given transition time.
		/// </summary>
		/// <param name="transitionClock">Transition time obtained earlier from 
		/// <see cref="AllTransitionClocks"/> property.
		/// </param>
		/// <returns>An instance of <see cref="Rule"/> representing the 
		/// type of transition associated with a given transition time.
		/// </returns>
		public Rule GetTransitionRule(long transitionClock) 
		{
			return rules[transTypes[Array.IndexOf(transTimes, (int)transitionClock)]];
		}


		/// <summary>
		/// Gets the unix times of all leap second corrections for this zoneinfo.
		/// </summary>
		public long[] AllLeapSecondCorrectionClocks
		{
			get 
			{
				ArrayList resClockList = new ArrayList();
				for (int i = -2; (i += 2) < leapSecs.Length; ) 
				{
					resClockList.Add((long)leapSecs[i]);
				}
				return (long[])resClockList.ToArray(typeof(long));
			}
		}

		/// <summary>
		/// Gets the leap second correction value for a given correction time.
		/// </summary>
		/// <param name="correctionClock">Leap second correction time obtained 
		/// earlier from <see cref="AllLeapSecondCorrectionClocks"/> property.</param>
		/// <returns>Integer value representing the leap second correction.</returns>
		public int GetLeapSecondCorrection(long correctionClock) 
		{
			for (int i = -2; (i += 2) < leapSecs.Length; ) 
			{
				if (leapSecs[i] == (int)correctionClock) 
				{
					return leapSecs[i + 1];
				}
			}
			return 0;
		}


		/// <summary>
		/// Returns true if a particular date is considered part of daylight time in this zoneinfo.
		/// </summary>
		/// <param name="localTime">Local time of the given zoneinfo. Note: It is not a local time of the machine !!!</param>
		/// <returns>true if a particular time is considered part of daylight time in this zoneinfo.</returns>
		public bool InDaylightTime(Time localTime) 
		{
			return GetRule(GetClockFromLocal(localTime)).IsDST;
		}


		/// <summary>
		/// Returns true if this zoneinfo has transitions between various offsets
		/// from UT, such as standard time and daylight time.
		/// </summary>
		public bool UsesDaylightTime 
		{ 
			get 
			{
				return rules.Length > 1;
			}
		}


		/// <summary>
		/// Calculates seconds since the epoch, the reverse of GetLocalTime(). Unused fields are computed and stored in localTime.
		/// </summary>
		/// <param name="localTime">Zone's local time to convert. The Year, Mon, MDay, Hour, Min, Sec fields are used and validated. Other fields are computed.</param>
		/// <returns>Seconds since the epoch.</returns>
		public long GetClockFromLocal(Time localTime) 
		{
			return GetClock(localTime, true);
		}


		/// <summary>
		/// Calculates seconds since the epoch, the reverse of GetUtcTime(). Unused fields are computed and stored in localTime.
		/// </summary>
		/// <param name="utcTime">Utc time to convert. The Year, Mon, MDay, Hour, Min, Sec fields are used and validated. Other fields are computed.</param>
		/// <returns>Seconds since the epoch.</returns>
		public long GetClockFromUtc(Time utcTime) 
		{
			return GetClock(utcTime, false);
		}


		/// <summary>
		/// Return the offset of this zoneinfo from UTC for a calendar date and time.
		/// </summary>
		/// <param name="localTime">Zone's local time to convert.</param>
		/// <returns>The offset in milliseconds to add to UTC to get local time.</returns>
		public int GetOffset(Time localTime) 
		{
			long clock = GetClockFromLocal(localTime);
			return ApplyRule(clock, GetRule(clock), new Time()) * 1000;
		}


		/// <summary>
		/// Gets the nearest DST rule of this zoneinfo.
		/// </summary>
		public Rule NearestDstRule
		{
			get 
			{
				return GetNearestDstRule(CurrentClock);
			}
		}

		/// <summary>
		/// Gets the nearest non-DST rule of this zoneinfo.
		/// </summary>
		public Rule NearestNonDstRule
		{
			get 
			{
				return GetNearestNonDstRule(CurrentClock);
			}
		}

		/// <summary>
		/// Calculates local time from seconds since the epoch.
		/// </summary>
		/// <param name="clock">seconds since 1970.</param>
		/// <returns>Local time instance.</returns>
		public Time GetLocalTime(long clock) 
		{
			Time t = new Time();
			ApplyRule(clock, GetRule(clock), t);
			return t;
		} 


		/// <summary>
		/// Compute UTC from clock.  This includes leap second corrections if
		/// compiled into the current zoneinfo file.
		/// </summary>
		/// <param name="clock">Clock seconds since 1970.</param>
		/// <returns>New <see cref="Time"/> object with all time fields computed.</returns>
		public Time GetUtcTime(long clock) 
		{
			Time t = new Time();
			ApplyRule(clock, null, t);
			return t;
		}


		/// <summary>
		/// Gets the current unix time of the host machine;
		/// </summary>
		public static long  CurrentClock 
		{
			get 
			{
				return (long)new TimeSpan(DateTime.UtcNow.Ticks - unixEpochTicks).TotalSeconds;
			}
		}


		/// <summary>
		/// Gets default zoneinfo name.
		/// </summary>
		public static string DefaultName 
		{
			get 
			{
				string name = null;
				try 
				{
					name = Environment.GetEnvironmentVariable("TZ");
				} 
				catch (System.Security.SecurityException)
				{
				}
				if (name == null) 
				{
					name = DefaultUtcName;
				} 
				return name;
			}
		}

		#endregion
	}
}
