#region Copyright (c) 2010 Zbigniew Babiej

/*
 * 
Written by Zbigniew Babiej, zbabiej@yahoo.com.
*/

#endregion

#region Using

using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;
using System.Globalization;
using System.Resources;
using System.Diagnostics;
using Microsoft.Win32;

#endregion

namespace TZ4Net
{
	/// <summary>
	/// Olson database contains also offset change information in addition to daylight-saving periods.
	/// Offset changes manifest themselves as two consecutive non-DST rules. This information
	/// might be useful, for example for validating the input dates. 
	/// We can subclass <see cref="System.Globalization.DaylightTime"/> type to represent that data.
	/// </summary>
	public class StandardTime : DaylightTime 
	{
		#region Constructors

		/// <summary>
		/// Constructs non-DST period of time.
		/// </summary>
		/// <param name="start">The <see cref="System.DateTime"/> that represents the date and time 
		/// when the daylight-saving period begins. The value must be in local time. </param>
		/// <param name="end">The <see cref="System.DateTime"/> that represents the date and time when the daylight-saving 
		/// period ends. The value must be in local time. </param>
		/// <param name="delta">The <see cref="System.TimeSpan"/> that represents the difference 
		/// between the standard time and the daylight-saving time in ticks.</param>
		public StandardTime(DateTime start, DateTime end, TimeSpan delta) : base(start, end, delta)
		{
		}

		#endregion
	}

	/// <summary>
	/// Enumeration that represents the result of time validation.
	/// </summary>
	public enum TimeCheckResult
	{
		/// <summary>
		/// Value less than minimum value supported by Unix.
		/// </summary>
		LessThanUnixMin,
		/// <summary>
		/// Value greater than maximum value supported by Unix.
		/// </summary>
		GreaterThanUnixMax,
		/// <summary>
		/// Value is valid.
		/// </summary>
		Valid,
		/// <summary>
		/// Value in fall-back range.
		/// </summary>
		InFallBackRange,
		/// <summary>
		/// Value in spring-forward range.
		/// </summary>
		InSpringForwardGap,
	}

#pragma warning disable CS0618 // Type or member is obsolete
	/// <summary>
	/// Thin wrapper around <see cref="ZoneInfo"/> which conforms to <see cref="System.TimeZone"/> interface.
	/// Uses "zoneinfo" directory by default, as <see cref="System.DateTime"/> does not support leap seconds.
	/// Implements caching.
	/// </summary>
	public class OlsonTimeZone : System.TimeZone
#pragma warning restore CS0618 // Type or member is obsolete
	{
		#region Nested types

		/// <summary>
		/// Value data of military/NATO letter to Olson map.
		/// </summary>
		private sealed class MilitaryMapValue
		{
			/// <summary>
			/// Caches military/NATO letter.
			/// </summary>
			private string letter;

			/// <summary>
			/// Caches military/NATO name.
			/// </summary>
			private string militaryName;
						
			/// <summary>
			/// Caches e-mail time zone indicator.
			/// </summary>
			private string emailIndicator;

			/// <summary>
			/// Caches Olson name corresponding to military/NATO letter.
			/// </summary>
			private string olsonName;

			/// <summary>
			/// Creates an instance of military map value.
			/// </summary>
			/// <param name="letter">Military/NATO timezone letter.</param>
			/// <param name="militaryName">Military/NATO timezone name.</param>
			/// <param name="emailIndicator">Military/NATO timezone email indicator.</param>
			/// <param name="olsonName">Corresponding Olson timezone name.</param>
			public MilitaryMapValue(string letter, string militaryName, string emailIndicator, string olsonName)
			{
				this.letter = letter;
				this.militaryName = militaryName;
				this.emailIndicator = emailIndicator;
				this.olsonName = olsonName;
			}

			/// <summary>
			/// Gets military timezone letter.
			/// </summary>
			public string Letter
			{
				get
				{
					return letter;
				}
			}

			/// <summary>
			/// Gets Military timezone name.
			/// </summary>
			public string MilitaryName
			{
				get
				{
					return militaryName;
				}
			}

			/// <summary>
			/// Gets corresponding Olson name.
			/// </summary>
			public string OlsonName
			{
				get
				{
					return olsonName;
				}
			}
		}

		// Value data of CLDR's Win32 to Olson map
		private sealed class UnicodeWin32MapValue
		{
			/// <summary>
			/// Caches Win32 Id.
			/// </summary>
			private string win32Id;

			/// <summary>
			/// Caches Olson name corresponding to Win32 Id.
			/// </summary>
			private string olsonName;

			/// <summary>
			/// Caches Win32 prefix corresponding to Win32 Id.
			/// </summary>
			private string win32Prefix;

			/// <summary>
			/// Caches Win32 name corresponding to Win32 Id.
			/// </summary>
			private string win32Name;

			/// <summary>
			/// Caches information whether timezone is obsoleted.
			/// </summary>
			private bool isObsoleted;

			/// <summary>
			/// Creates the instance of CLDR map value.
			/// </summary>
			/// <param name="win32Id">Win32 timezone id.</param>
			/// <param name="olsonName">Olson timezone name.</param>			
			public UnicodeWin32MapValue(string win32Id, string olsonName) 
			{
				if (win32Id == null) 
				{
					throw new ArgumentNullException("win32Id");
				}
				if (olsonName == null) 
				{
					throw new ArgumentNullException("olsonName");
				}
				this.win32Id = win32Id;
				this.olsonName = olsonName;
			}

			/// <summary>
			/// Gets Win32 timezone Id.
			/// </summary>
			public string Win32Id
			{
				get 
				{
					return win32Id;
				}
			}

			/// <summary>
			/// Gets Olson timezone name.
			/// </summary>
			public string OlsonName
			{
				get 
				{
					return olsonName;
				}
			}

			/// <summary>
			/// Gets Win32 tinezone prefix.
			/// </summary>
			public string Win32Prefix
			{
				get 
				{
					return win32Prefix;
				}
				set 
				{
					win32Prefix = value;
				}
			}

			/// <summary>
			/// Gets Win32 timezone name.
			/// </summary>
			public string Win32Name
			{
				get 
				{
					return win32Name;
				}
				set 
				{
					win32Name = value;
				}
			}

			/// <summary>
			/// Gets information whether timezone is obsoleted.
			/// </summary>
			public bool IsObsoleted 
			{
				get 
				{
					return isObsoleted;
				}
				set 
				{
					isObsoleted = true;
				}
			}
		}

		/// <summary>
		/// Value data of registry map.
		/// </summary>
		private sealed class RegistryMapValue
		{
			/// <summary>
			/// Caches registry subkey name.
			/// </summary>
			private string win32Id;

			/// <summary>
			/// Caches 'Std' value.
			/// </summary>
			private string localWin32Id;

			/// <summary>
			/// Caches 'Display' value.
			/// </summary>
			private string localWin32Name;

			/// <summary>
			/// Creates the instance of registry map value.
			/// </summary>
			/// <param name="win32Id">Name of subkey.</param>
			/// <param name="localWin32Id">'Std' value.</param>
			/// <param name="localWin32Name">'Display' value.</param>
			public RegistryMapValue(string win32Id, string localWin32Id, string localWin32Name)
			{
				this.win32Id = win32Id;
				this.localWin32Id = localWin32Id;
				this.localWin32Name = localWin32Name;
			}

			/// <summary>
			/// Gets the subkey name.
			/// </summary>
			public string Win32Id 
			{
				get 
				{
					return win32Id;
				}
			}

			/// <summary>
			/// Gets the 'Std' value.
			/// </summary>
			public string LocalWin32Id
			{
				get
				{
					return localWin32Id;
				}
			}

			/// <summary>
			/// Gets the 'Display' value.
			/// </summary>
			public string LocalWin32Name
			{
				get
				{
					return localWin32Name;
				}
			}
		}

		/// <summary>
		/// Value data of timezone rank map
		/// </summary>
		private sealed class RankValue : IComparable	
		{
			/// <summary>
			/// Stores the Olson name of timezone.
			/// </summary>
			private string olsonName;

			/// <summary>
			/// Holds the coverage of the timezone by the rule.
			/// </summary>
			private double coverage;

			/// <summary>
			/// Denotes the raw number of future transitions into that rule.
			/// </summary>
			private int futureTransitionCount;

			/// <summary>
			/// Denotes the number of all transitions into that rule.
			/// </summary>
			private int allTransitionCount;

			/// <summary>
			/// If the transition is the last transition of timezone.
			/// </summary>
			private bool isLastTransition;

			/// <summary>
			/// Creates the instance for given time zone.
			/// </summary>
			/// <param name="olsonName">Olson name of timezone.</param>
			public RankValue(string olsonName) 
			{
				this.olsonName = olsonName;
			}

			/// <summary>
			/// Gets Olson name of timezone.
			/// </summary>
			public string OlsonName 
			{
				get 
				{
					return olsonName;
				}
			}

			/// <summary>
			/// Gets the coverage of the rule.
			/// </summary>
			public double Coverage 
			{
				get 
				{
					return coverage;
				}
				set 
				{
					coverage = value;
				}
			}

			/// <summary>
			/// Gets and sets the raw number of future transitions.
			/// </summary>
			public int FutureTransitionCount
			{
				get 
				{
					return futureTransitionCount;
				}
				set 
				{
					futureTransitionCount = value;
				}
			}

			/// <summary>
			/// Gets the number of all transitions.
			/// </summary>
			public int AllTransitionCount
			{
				get 
				{
					return allTransitionCount;
				}
				set 
				{
					allTransitionCount = value;
				}
			}

			/// <summary>
			/// Indicates if the transition is the last transition of timezone.
			/// </summary>
			public bool IsLastTransition 
			{
				get 
				{
					return isLastTransition;
				}
				set 
				{
					isLastTransition = value;
				}
			}

			/// <summary>
			/// Compares to other instance of the same type.
			/// </summary>
			/// <param name="that">Instance to compare to.</param>
			/// <returns></returns>
			public int CompareTo(RankValue that) 
			{
				if (Math.Min(this.FutureTransitionCount, maxFutureTransitions) > Math.Min(that.FutureTransitionCount, maxFutureTransitions)) 
				{
					return 1;
				} 
				if (Math.Min(this.FutureTransitionCount, maxFutureTransitions) < Math.Min(that.FutureTransitionCount, maxFutureTransitions)) 
				{
					return -1;
				}

				if (this.IsLastTransition && !that.IsLastTransition) 
				{
					return 1;
				} 
				else if (!this.IsLastTransition && that.IsLastTransition) 
				{
					return -1;
				}
				
				if (this.AllTransitionCount > that.AllTransitionCount) 
				{
					return 1;
				} 
				else if (this.AllTransitionCount < that.AllTransitionCount) 
				{
					return -1;
				}
				
				if (this.Coverage > that.Coverage) 
				{
					return 1;
				} 
				else if (this.Coverage < that.Coverage)
				{
					return -1;
				}

				if (this.OlsonName.IndexOf("/") >= 0 && that.OlsonName.IndexOf("/") < 0 ) 
				{
					return 1;
				} 
				else if (this.OlsonName.IndexOf("/") < 0 && that.OlsonName.IndexOf("/") >= 0 ) 
				{
					return -1;
				}

				return -this.OlsonName.CompareTo(that.OlsonName);
			}

			#region IComparable Members

			/// <summary>
			/// Compares the current instance with another object of the same type.
			/// </summary>
			/// <param name="obj">An object to compare with this instance.</param>
			/// <returns>A 32-bit signed integer that indicates the relative order of the comparands.</returns>
			int IComparable.CompareTo(object obj)
			{
				if (obj is RankValue) 
				{
					return CompareTo((RankValue) obj);
				}
				else 
				{
					throw new ArgumentException("object is not a RankValue");
				}
			}

			#endregion
		}

		#endregion

		#region Fields

		/// <summary>
		/// Caches the base name of the CLDR database resource.
		/// </summary>
		private const string CldrBaseName = "cldrinfo";

		/// <summary>
		/// Caches the resource file name of the CLDR database resource, like MyResource.resources.
		/// </summary>
		internal static readonly string CldrResourceFileName = string.Format("{0}.resources", CldrBaseName);

		/// <summary>
		/// Root name of the CLDR database resource, like MyAssembly.MyResource.
		/// </summary>
		private static readonly string CldrResouceRootName = string.Format("{0}.{1}", Assembly.GetExecutingAssembly().GetName().Name, CldrBaseName);

		/// <summary>
		/// CLDR windows zones supplemental file and resource name;
		/// </summary>
		private const string CldrSupplementalFileName = "windowsZones.xml";

		/// <summary>
		/// CLDR zone log file and resource name;
		/// </summary>
		private const string CldrZoneLogFileName = "zone_log.html";

		/// <summary>
		/// Min. value of time as constrained by 32-bit Unix clock;
		/// </summary>
		public static readonly DateTime MinTime = ZoneInfo.Time.MinValue.DateTime;

		/// <summary>
		/// Max. value of time as constrained by 32-bit Unix clock;
		/// </summary>
		public static readonly DateTime MaxTime = ZoneInfo.Time.MaxValue.DateTime;

		/// <summary>
		/// Storage for underlying <see cref="ZoneInfo"/> object.
		/// </summary>
		private ZoneInfo zoneInfo;

		/// <summary>
		/// Caches all time changes for later reuse. It contains both daylight and standard changes.
		/// </summary>
		private DaylightTime[] allTimeChanges;

		/// <summary>
		/// Caches all timezone names for later reuse.
		/// </summary>
		private static string[] allNames;

		/// <summary>
		/// Caches primary timezone names for later reuse.
		/// </summary>
		private static string[] primaryNames;

		/// <summary>
		/// Holds the current timezone defined by the client. If not null, 
		/// it overrides the the setting from TZ environment variable. 
		/// </summary>
		private static OlsonTimeZone currentTimeZone;

		/// <summary>
		/// Returned if current timezone is not defined by the client and both TZ
		/// environment variable and registry default values are not defined.
		/// </summary>
		private static readonly OlsonTimeZone defaultUtcTimeZone = new OlsonTimeZone(ZoneInfo.DefaultUtcName);

		/// <summary>
		/// Caches the instances of constructed timezones.
		/// </summary>
		private static Hashtable cache = new Hashtable();

		/// <summary>
		/// Common format for time check error message.
		/// </summary>
		private const string TimeOutOfRangeMsgFormat = "Time value out of valid range. Check code is '{0}'.";

		/// <summary>
		/// Caches the map of military/NATO letters to Olson name.
		/// string letter -> string[4] {letter, militaryName, emailIndicator, olsonName}
		/// </summary>
		private static SortedList militaryMap;

		/// <summary>
		/// Caches the map of Win32 Id to Olson name and Win32 name as defined in CLDR supplemental data.
		/// string win32Id -> string[3] {olsonName, win32Prefix, win32Name}
		/// </summary>
		private static Hashtable unicodeWin32Map;

		/// <summary>
		/// Caches the map of Olson name to Win32 Id based on Unicode's CLDR supplemental data.
		/// </summary>
		private static Hashtable olsonToWin32IdMap;

		/// <summary>
		/// Caches the map of Olson name to Win32 name based on Unicode's CLDR supplemental data.
		/// </summary>
		private static Hashtable olsonToWin32NameMap;

		/// <summary>
		/// Caches the map of Win32 name to Olson Name based on Unicode's CLDR supplemental data.
		/// </summary>
		private static Hashtable win32NameToOlsonMap;

		/// <summary>
		/// Caches the map of Unicode alias alternate name to Olson main name as proposed in 'Time Zone Localization' draft document.
		/// See http://www.unicode.org/cldr/data/docs/design/formatting/time_zone_localization.html
		/// </summary>
		private static Hashtable unicodeAliasMap;

		/// <summary>
		/// Caches the map of Win32 Id to Win32 name as defined in registry.
		/// </summary>
		private static Hashtable registryMap;

		/// <summary>
		/// Caches the map of registry Win32 name to Olson name based on registry and Unicode definitions.
		/// </summary>
		private static Hashtable registryWin32NameToOlsonMap;

		/// <summary>
		/// Caches the map of abrreviation to Olson name;
		/// </summary>
		private static Hashtable abbreviationToOlsonMap;

		/// <summary>
		/// Caches the map of Olson name to standard abbreviation;
		/// </summary>
		private static Hashtable olsonToStdAbbreviationMap;

		/// <summary>
		/// The clock boundary from which the future transitions are calculated.
		/// </summary>
		private static readonly long currentClock = ZoneInfo.CurrentClock;

		/// <summary>
		/// Max number of future transitions to be taken into consideration.
		/// </summary>
		private static readonly int maxFutureTransitions = ZoneInfo.Time.MaxValue.DateTime.Year - new ZoneInfo(ZoneInfo.UtcName).GetUtcTime(currentClock).DateTime.Year - 1;

		/// <summary>
		/// Synchronization root object to be used in multithreading environment.
		/// As factory method and instant methods use 'lazy evaluation' techniques
		/// to initialize the static data, it is recommended to synchronize
		/// against this single root object for both static and instance invocations.
		/// </summary>
		private static object  syncRoot = new object();

		#endregion
		
		#region Constructors

		private OlsonTimeZone(string name) : base()
		{
			zoneInfo = new ZoneInfo(name, ZoneInfoDir);
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Gets full Military/NATO letter to Olson name map;
		/// </summary>
		/// <returns></returns>
		private static Hashtable FullMilitaryMap
		{
			get
			{
				Hashtable res = new Hashtable();
				res.Add("Y", new MilitaryMapValue("Y", "Yankee Time Zone",	"-1200", "Etc/GMT+12"));
				res.Add("X", new MilitaryMapValue("X", "X-ray Time Zone",	"-1100", "Etc/GMT+11"));
				res.Add("W", new MilitaryMapValue("W", "Whiskey Time Zone", "-1000", "Etc/GMT+10"));
				res.Add("V", new MilitaryMapValue("V", "Victor Time Zone",	"-0900", "Etc/GMT+9"));
				res.Add("U", new MilitaryMapValue("U", "Uniform Time Zone", "-0800", "Etc/GMT+8"));
				res.Add("T", new MilitaryMapValue("T", "Tango Time Zone",	"-0700", "Etc/GMT+7"));
				res.Add("S", new MilitaryMapValue("S", "Sierra Time Zone",	"-0600", "Etc/GMT+6"));
				res.Add("R", new MilitaryMapValue("R", "Romeo Time Zone",	"-0500", "Etc/GMT+5"));
				res.Add("Q", new MilitaryMapValue("Q", "Quebec Time Zone",	"-0400", "Etc/GMT+4"));
				res.Add("P", new MilitaryMapValue("P", "Papa Time Zone",	"-0300", "Etc/GMT+3"));
				res.Add("O", new MilitaryMapValue("O", "Oscar Time Zone",	"-0200", "Etc/GMT+2"));
				res.Add("N", new MilitaryMapValue("N", "November Time Zone","-0100", "Etc/GMT+1"));
				res.Add("Z", new MilitaryMapValue("Z", "Zulu Time Zone",	"+0000", "Etc/GMT+0"));
				res.Add("A", new MilitaryMapValue("A", "Alpha Time Zone",	"+0100", "Etc/GMT-1"));
				res.Add("B", new MilitaryMapValue("B", "Bravo Time Zone",	"+0200", "Etc/GMT-2"));
				res.Add("C", new MilitaryMapValue("C", "Charlie Time Zone", "+0300", "Etc/GMT-3"));
				res.Add("D", new MilitaryMapValue("D", "Delta Time Zone",	"+0400", "Etc/GMT-4"));
				res.Add("E", new MilitaryMapValue("E", "Echo Time Zone",	"+0500", "Etc/GMT-5"));
				res.Add("F", new MilitaryMapValue("F", "Foxtrot Time Zone", "+0600", "Etc/GMT-6"));
				res.Add("G", new MilitaryMapValue("G", "Golf Time Zone",	"+0700", "Etc/GMT-7"));
				res.Add("H", new MilitaryMapValue("H", "Hotel Time Zone",	"+0800", "Etc/GMT-8"));
				res.Add("I", new MilitaryMapValue("I", "India Time Zone",	"+0900", "Etc/GMT-9"));
				res.Add("K", new MilitaryMapValue("K", "Kilo Time Zone",	"+1000", "Etc/GMT-10"));
				res.Add("L", new MilitaryMapValue("L", "Lima Time Zone",	"+1100", "Etc/GMT-11"));
				res.Add("M", new MilitaryMapValue("M", "Mike Time Zone",	"+1200", "Etc/GMT-12"));
				return res;
			}
		}

		/// <summary>
		/// Creates the Military/NATO letter to Olson name map;
		/// </summary>
		/// <returns></returns>
		private static SortedList CreateMilitaryMap() 
		{
			SortedList res = new SortedList();
			foreach(MilitaryMapValue mapValue in FullMilitaryMap.Values)
			{
				if (Array.IndexOf(AllNames, mapValue.OlsonName) >= 0)
				{	// add only if value exists in the list of all Olson names
					res.Add(mapValue.Letter, mapValue);
				}
			}

			return res;
		}

		/// <summary>
		/// Creates the Win32 Id to Olson name map;
		/// </summary>
		/// <returns></returns>
		private static Hashtable CreateUnicodeWin32Map() 
		{
			Hashtable res = new Hashtable();
			ResourceManager resourceManager = new ResourceManager(CldrResouceRootName, Assembly.GetExecutingAssembly());
			byte[] resource = (byte[])resourceManager.GetObject(CldrSupplementalFileName);
			if (resource == null) 
			{
				throw new System.ArgumentException(string.Format("Resource '{0}' not found in assembly '{1}'.", CldrSupplementalFileName, Assembly.GetExecutingAssembly().GetName().Name));			
			}
			Stream stream = null;
			try 
			{
				stream = new BufferedStream(new MemoryStream(resource, false));
				XmlDocument doc = new XmlDocument();
				doc.XmlResolver = null;
				doc.Load(stream);
				XmlElement mapTimezonesElement = doc.DocumentElement.SelectSingleNode("//supplementalData/windowsZones/mapTimezones") as XmlElement;
				if (mapTimezonesElement != null)
				{
					XmlNodeList children = mapTimezonesElement.ChildNodes;
					int i = -1, iMax = children.Count - 1;
					while (++i < iMax)
					{
						XmlComment comment = children[i] as XmlComment;
						if (comment != null)
						{
							XmlElement element = children[++i] as XmlElement;
							if (element != null)
							{
								Debug.Assert(element.GetAttribute("territory") == "001");
								UnicodeWin32MapValue mapValue = new UnicodeWin32MapValue(element.GetAttribute("other"), element.GetAttribute("type"));
								mapValue.Win32Name = comment.Value.Trim();

								// end of 'mapZone' element, ready to add to the map
								if (Array.IndexOf(AllNames, mapValue.OlsonName) >= 0)
								{   // add only if value exists in the list of all Olson names
									res.Add(mapValue.Win32Id, mapValue);
								}
							}
						}
					}
				}
			} 
			finally 
			{
				if (stream != null) 
				{
					stream.Close();
				}
			}
			return res;
		}


		/// <summary>
		/// Creates the registry Win32 Id to Win32 name map;
		/// </summary>
		/// <returns></returns>
		private static Hashtable CreateRegistryMap()
		{
			Hashtable res = new Hashtable();
#if NETCORE
			foreach (TimeZoneInfo z in TimeZoneInfo.GetSystemTimeZones())
			{
				res[z.Id] = new RegistryMapValue(z.StandardName, z.Id, z.DisplayName);
			}
			return res;
#else
			RegistryKey regKey = null;
			
			try 
			{
				regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones");
				Debug.Assert(regKey != null);
				string[] subKeyNames = regKey.GetSubKeyNames();
				foreach (string subKeyName in subKeyNames) 
				{
					RegistryKey subKey = null;
					try 
					{
						subKey = regKey.OpenSubKey(subKeyName);
						Debug.Assert(subKey != null);
						string localWin32Id = (string)subKey.GetValue(@"Std");
						string localWin32Name = (string)subKey.GetValue(@"Display");
						if (localWin32Id != null && localWin32Name != null) 
						{
							res.Add(subKeyName, new RegistryMapValue(subKeyName, localWin32Id, localWin32Name));
						}
					} 
					finally 
					{
						if (subKey != null) 
						{
							subKey.Close();
						}
					}
				}
			}
			catch (System.Security.SecurityException) 
			{
			}
			finally 
			{
				if (regKey != null) 
				{
					regKey.Close();
				}
			}
			return res;
#endif
		}

		/// <summary>
		/// Creates the Unicode alias to Olson name map;
		/// We should have used the html parser here, but to avoid the dependency
		/// on third-party components we use a simple manual parsing instead.
		/// </summary>
		/// <returns></returns>
		private static Hashtable CreateUnicodeAliasMap() 
		{
			Hashtable res = new Hashtable();
			ResourceManager resourceManager = new ResourceManager(CldrResouceRootName, Assembly.GetExecutingAssembly());
			byte[] resource = (byte[])resourceManager.GetObject(CldrZoneLogFileName);
			if (resource == null) 
			{
				throw new System.ArgumentException(string.Format("Resource '{0}' not found in assembly '{1}'.", CldrZoneLogFileName, Assembly.GetExecutingAssembly().GetName().Name));			
			}
			string html = new ASCIIEncoding().GetString(resource);
			Debug.Assert(html.StartsWith("<html>"));
			int aliasesAnchorIndex = html.IndexOf("<a name='aliases' href='#aliases'>");
			Debug.Assert(aliasesAnchorIndex > 0);
			int tableStartIndex = html.IndexOf("<table>", aliasesAnchorIndex) + "<table>".Length + 1;
			Debug.Assert(tableStartIndex > 0);
			int tableEndIndex = html.IndexOf("</table>", tableStartIndex);
			Debug.Assert(tableEndIndex > 0);
			string table = html.Substring(tableStartIndex, tableEndIndex - tableStartIndex);
			Regex r = new Regex(@"\s*</tr>\n\s*"); 
			string[] rows = r.Split(table);
			Debug.Assert(rows != null && rows.Length > 0);
			r = new Regex(@"\s*</td>\s*");
			for (int i = 1; i < rows.Length - 1; i++) 
			{
				string row = rows[i];
				Debug.Assert(row.StartsWith("<tr><td>"));
				row = row.Substring("<tr><td>".Length);
				string[] columns = r.Split(row);
				Debug.Assert(columns.Length == 4);
				for(int j = 1; j < columns.Length - 1; j++) 
				{
					Debug.Assert(columns[j].StartsWith("<td>"));
					columns[j] = columns[j].Substring("<td>".Length);
				}
				string alternateName = columns[1];
				string mainName = columns[2];
				Debug.Assert(alternateName != null);
				Debug.Assert(mainName != null);
				if (Array.IndexOf(AllNames, mainName) >= 0) 
				{	// add only if mainName exists in the list of all Olson names
					res.Add(alternateName, mainName);
				}
			}
			return res;
		}


		/// <summary>
		/// Gets the map of Win32 Id to Olson name and Win32 name as defined in CLDR supplemental data.
		/// </summary>
		private static SortedList MilitaryMap 
		{
			get 
			{
				if (militaryMap == null) 
				{
					militaryMap = CreateMilitaryMap();
				}
				return militaryMap;
			}
		}


		/// <summary>
		/// Gets the map of Win32 Id to Olson name and Win32 name as defined in CLDR supplemental data.
		/// </summary>
		private static Hashtable UnicodeWin32Map 
		{
			get 
			{
				if (unicodeWin32Map == null) 
				{
					unicodeWin32Map = CreateUnicodeWin32Map();
				}
				return unicodeWin32Map;
			}
		}


		/// <summary>
		/// Gets the map of Unicode alternate name to Olson main name as proposed in 'Time Zone Localization' draft document.
		/// See http://www.unicode.org/cldr/data/docs/design/formatting/time_zone_localization.html
		/// </summary>
		private static Hashtable UnicodeAliasMap 
		{
			get 
			{
				if (unicodeAliasMap == null) 
				{
					unicodeAliasMap = CreateUnicodeAliasMap();
				}
				return unicodeAliasMap;
			}
		}


		/// <summary>
		/// Gets the map of Win32 Id to Win32 name as defined in registry.
		/// </summary>
		private static Hashtable RegistryMap
		{
			get 
			{
				if (registryMap == null) 
				{
					registryMap = CreateRegistryMap();
				}
				return registryMap;
			}
		}


		/// <summary>
		/// Creates the Olson name to Win32 Id map;
		/// </summary>
		/// <returns>Hashtable mapping the Olson name to Win32 Id.</returns>
		private static Hashtable CreateOlsonToWin32IdMap() 
		{
			Debug.Assert(UnicodeWin32Map != null);
			IDictionaryEnumerator e = UnicodeWin32Map.GetEnumerator();
			Hashtable res = new Hashtable();
			while (e.MoveNext()) 
			{
				UnicodeWin32MapValue mapValue = e.Value as UnicodeWin32MapValue;
				Debug.Assert(mapValue != null);
				if (mapValue != null)
				{
					if (res.ContainsKey(mapValue.OlsonName))
					{
						if (!mapValue.IsObsoleted)
						{
							res[mapValue.OlsonName] = e.Key;
						}
					}
					else
					{
						res.Add(mapValue.OlsonName, e.Key);
					}
				}
			}
			return res;
		}


		/// <summary>
		/// Creates the registry Win32 name to Olson name map;
		/// </summary>
		/// <returns>Hashtable mapping the registry Win32 name to Olson name.</returns>
		private static Hashtable CreateRegistryWin32NameToOlsonMap() 
		{
			Debug.Assert(RegistryMap != null);
			Hashtable res = new Hashtable();
			foreach(RegistryMapValue mapValue in RegistryMap.Values)
			{
				Debug.Assert(mapValue.LocalWin32Name != null);
				Debug.Assert(mapValue.Win32Id != null);
				if (UnicodeWin32Map.ContainsKey(mapValue.Win32Id)) 
				{
					UnicodeWin32MapValue value = (UnicodeWin32MapValue)UnicodeWin32Map[mapValue.Win32Id];
					Debug.Assert(value != null);
					res.Add(mapValue.LocalWin32Name, value.OlsonName);
				}
			}
			return res;
		}


		/// <summary>
		/// Creates the Olson name to Win32 Id map;
		/// </summary>
		/// <returns></returns>
		private static Hashtable CreateOlsonToWin32NameMap() 
		{
			Debug.Assert(UnicodeWin32Map != null);
			IDictionaryEnumerator e = UnicodeWin32Map.GetEnumerator();
			Hashtable res = new Hashtable();
			while (e.MoveNext()) 
			{
				UnicodeWin32MapValue mapValue = e.Value as UnicodeWin32MapValue;
				Debug.Assert(mapValue != null);
				if (mapValue != null)
				{
					if (res.ContainsKey(mapValue.OlsonName))
					{
						if (!mapValue.IsObsoleted)
						{
							res[mapValue.OlsonName] = mapValue.Win32Name;
						}
					}
					else
					{
						res.Add(mapValue.OlsonName, mapValue.Win32Name);
					}
				}
			}
			return res;
		}


		/// <summary>
		/// Creates the Win32 name to Olson name map;
		/// </summary>
		/// <returns></returns>
		private static Hashtable CreateWin32NameToOlsonMap() 
		{
			Debug.Assert(UnicodeWin32Map != null);
			IDictionaryEnumerator e = UnicodeWin32Map.GetEnumerator();
			Hashtable res = new Hashtable();
			while (e.MoveNext()) 
			{
				UnicodeWin32MapValue mapValue = e.Value as UnicodeWin32MapValue;
				Debug.Assert(mapValue != null);
				if (mapValue != null)
				{
					if (res.ContainsKey(mapValue.Win32Name))
					{
						if (!mapValue.IsObsoleted)
						{
							res[mapValue.Win32Name] = mapValue.OlsonName;
						}
					}
					else
					{
						res.Add(mapValue.Win32Name, mapValue.OlsonName);
					}
				}
			}
			return res;
		}


		/// <summary>
		/// Gets the map of Olson name to Win32 Id.
		/// </summary>
		private static Hashtable OlsonToWin32IdMap
		{
			get 
			{
				if (olsonToWin32IdMap == null) 
				{
					olsonToWin32IdMap = CreateOlsonToWin32IdMap();
				}
				return olsonToWin32IdMap;
			}
		}

		/// <summary>
		/// Gets the map of Olson name to Win32 name. 
		/// </summary>
		private static Hashtable OlsonToWin32NameMap
		{
			get 
			{
				if (olsonToWin32NameMap == null) 
				{
					olsonToWin32NameMap = CreateOlsonToWin32NameMap();
				}
				return olsonToWin32NameMap;
			}
		}


		/// <summary>
		/// Gets the map of Win32 name to Olson name. 
		/// </summary>
		private static Hashtable Win32NameToOlsonMap
		{
			get 
			{
				if (win32NameToOlsonMap == null) 
				{
					win32NameToOlsonMap = CreateWin32NameToOlsonMap();
				}
				return win32NameToOlsonMap;
			}
		}


		/// <summary>
		/// Gets the map of reigistry Win32 name to Olson name. 
		/// </summary>
		private static Hashtable RegistryWin32NameToOlsonMap
		{
			get 
			{
				if (registryWin32NameToOlsonMap == null) 
				{
					registryWin32NameToOlsonMap = CreateRegistryWin32NameToOlsonMap();
				}
				return registryWin32NameToOlsonMap;
			}
		}


		#endregion

		#region TimeZone Implementation

		/// <summary>
		/// Gets the current timezone's registry Win32 Id.
		/// </summary>
		private static string RegistryWin32Id
		{
			get 
			{ 
#pragma warning disable CS0618 // Type or member is obsolete
				string win32Id = TimeZone.CurrentTimeZone.StandardName;
#pragma warning restore CS0618 // Type or member is obsolete
				// above value is in local language, hence mapping to language neutral version
				foreach (RegistryMapValue mapValue in RegistryMap.Values)
				{
					if (mapValue.LocalWin32Id == win32Id)
					{
						return mapValue.Win32Id;
					}
				}
				// if permission not granted to read the registry, i.e. RegistryMap is empty,
				// then it should still work for english versions
				return win32Id;
			}
		}

		/// <summary>
		/// Gets the timezone of the current computer system.
		/// </summary>
		public static new OlsonTimeZone CurrentTimeZone 
		{
			get 
			{
				if (currentTimeZone == null) 
				{
					// if timezone not set explicitly, get the default name from TZ variable
					string currentOlsonName = ZoneInfo.DefaultName;
					if (object.ReferenceEquals(currentOlsonName, ZoneInfo.DefaultUtcName) || Array.IndexOf(AllNames, currentOlsonName) < 0)
					{
						// if TZ variable not set or set to unknown name, try to lookup in registry
						if (RegistryWin32Id != null) 
						{
							// if permission granted to read the registry, convert the id to olson name
							currentOlsonName = GetNameFromWin32Id(RegistryWin32Id);
							if (currentOlsonName == null) 
							{
								// it is possible to get the win32Id which is not supported by current version of CLDR.
								// For example CLDR 1.4.1 does not support some new zones introduced in Microsoft 2007 
								// time zone update like 'Central Standard Time (Mexico)'
								return defaultUtcTimeZone;
							}
						} 
						else 
						{
							// no permission to read registry or registry corrupted
							return defaultUtcTimeZone;
						}
						
					} 
					return GetInstanceFromOlsonName(currentOlsonName);					
				} 
				else 
				{
					return currentTimeZone;
				}
			}
			set 
			{
				currentTimeZone = value;
			}
		}


		/// <summary>
		/// Gets the daylight saving timezone name.
		/// </summary>
		public override string DaylightName 
		{
			get 
			{
				return string.Format("{0} Daylight Time", zoneInfo.Name);
			}
		}


		/// <summary>
		/// Gets the standard timezone name.
		/// </summary>
		public override string StandardName 
		{
			get 
			{
				return string.Format("{0} Standard Time", zoneInfo.Name);
			} 
		}
			
	
		/// <summary>
		///  Returns the daylight saving time period for a particular year.  
		/// </summary>
		/// <param name="year">The year to which the daylight saving time period applies.</param>
		/// <returns> A <see cref="System.Globalization.DaylightTime"/> instance containing the start and end date for daylight saving time in year.</returns>
		public override DaylightTime GetDaylightChanges(int year) 
		{
			foreach (DaylightTime time in AllTimeChanges) 
			{
				if (time.Start.Year == year && !(time is StandardTime)) 
				{
					return time;
				}
			}
			return new DaylightTime(DateTime.MinValue, DateTime.MinValue, new TimeSpan(0));
		}


		/// <summary>
		/// Returns a value indicating wheter the specified date and time is within a daylight saving time period. 
		/// </summary>
		/// <param name="time">A date and time.</param>
		/// <returns> true if time is in a daylight saving time period; false otherwise, or if time is null.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when time value is invalid within current timezone.</exception>
		public override bool IsDaylightSavingTime(DateTime time) 
		{
			TimeCheckResult res = CheckLocalTime(time);
			if (res != TimeCheckResult.Valid) 
			{
				throw new ArgumentOutOfRangeException("time", time, string.Format(TimeOutOfRangeMsgFormat, res));
			}

			return zoneInfo.InDaylightTime(new ZoneInfo.Time(time));
		}


		/// <summary>
		/// Converts the UTC time to the local time of timezone.
		/// </summary>
		/// <param name="time">UTC time./// </param>
		/// <returns>
		/// Local time of converter's timezone corresponding to UTC time.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when time value is invalid within current timezone.</exception>
		public override DateTime ToLocalTime(DateTime time)
		{
			TimeCheckResult res = CheckLocalTime(time);
			if (res < TimeCheckResult.Valid) 
			{
				throw new ArgumentOutOfRangeException("time", time, string.Format(TimeOutOfRangeMsgFormat, res));
			}

			return zoneInfo.GetLocalTime(zoneInfo.GetClockFromUtc(new ZoneInfo.Time(time))).DateTime;
		}


		/// <summary>
		/// Converts the local time of timezone to the UTC time.
		/// </summary>
		/// <param name="time">
		/// Local time of the given timezone. Note: It is not a local time of the machine !!!
		/// </param>
		/// <returns>
		/// UTC time corresponding to the local time of timezone.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when time value is invalid within current timezone.</exception>
		public override DateTime ToUniversalTime(DateTime time)
		{
			TimeCheckResult res = CheckLocalTime(time);
			if (res != TimeCheckResult.Valid) 
			{
				throw new ArgumentOutOfRangeException("time", time, string.Format(TimeOutOfRangeMsgFormat, res));
			}

			return zoneInfo.GetUtcTime(zoneInfo.GetClockFromLocal(new ZoneInfo.Time(time))).DateTime;
		}
					   

		/// <summary>
		/// Returns the coordinated universal time (UTC) offset for the specified local time.
		/// </summary>
		/// <param name="time">The local date and time.</param>
		/// <returns>The UTC offset from time, measured in ticks.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when time value is invalid within current timezone.</exception>
		public override TimeSpan GetUtcOffset(DateTime time)
		{
			TimeCheckResult res = CheckLocalTime(time);
			if (res != TimeCheckResult.Valid) 
			{
				throw new ArgumentOutOfRangeException("time", time, string.Format(TimeOutOfRangeMsgFormat, res));
			}
			int offset = zoneInfo.GetOffset(new ZoneInfo.Time(time));
			return new TimeSpan((long)offset * 10000);
		}

		#endregion

		#region Public Interface

		/// <summary>
		/// Gets the object to synchronize the static and instance calls
		/// coming  from different threads.
		/// </summary>
		public static object SyncRoot 
		{
			get 
			{
				return syncRoot;
			}
		}

		/// <summary>
		/// Factory, that provides the client with instance of timezone with a given Olson name.
		/// </summary>
		/// <param name="olsonName">Olson name of the timezone to be provided.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstanceFromOlsonName(string olsonName) 
		{
			OlsonTimeZone timeZone = (OlsonTimeZone)cache[olsonName];
			if (timeZone == null) 
			{
				timeZone = new OlsonTimeZone(olsonName);
				cache.Add(olsonName, timeZone);
			}
			return timeZone;
		}


		/// <summary>
		/// Helper factory, that provides the client with instance of timezone from given Win32 Id.
		/// </summary>
		/// <param name="win32Id">Win32 Id of the timezone to be provided.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstanceFromWin32Id(string win32Id) 
		{
			if (win32Id == null) 
			{
				throw new System.ArgumentNullException("win32Id", "Win32 Id of timezone can not be null.");
			}
			string olsonName = GetNameFromWin32Id(win32Id);
			if (olsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Win32 Id '{0}' not supported. Corresponding Olson name not found.", win32Id), "win32Id");
			}
			return GetInstanceFromOlsonName(olsonName);
		}


		/// <summary>
		/// Helper factory, that provides the client with instance of timezone from given Win32 Name.
		/// </summary>
		/// <param name="win32Name">Win32 name of the timezone to be provided.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstanceFromWin32Name(string win32Name) 
		{
			if (win32Name == null) 
			{
				throw new System.ArgumentNullException("win32Name", "Win32 name of timezone can not be null.");
			}
			string olsonName = GetNameFromWin32Name(win32Name);
			if (olsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Win32 name '{0}' not supported. Corresponding Olson name not found.", win32Name), "win32Id");
			}
			return GetInstanceFromOlsonName(olsonName);
		}


		/// <summary>
		/// Helper factory, that provides the client with instance of timezone from given registry Win32 name.
		/// </summary>
		/// <param name="registryWin32Name">Registry Win32 name of the timezone to be provided.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstanceFromRegistryWin32Name(string registryWin32Name) 
		{
			if (registryWin32Name == null) 
			{
				throw new System.ArgumentNullException("registryWin32Name", "Registry Win32 name of timezone can not be null.");
			}
			string olsonName = GetNameFromRegistryWin32Name(registryWin32Name);
			if (olsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Registry Win32name '{0}' not supported. Corresponding Olson name not found.", registryWin32Name), "registryDisplayName");
			}
			return GetInstanceFromOlsonName(olsonName);
		}


		/// <summary>
		/// Helper factory, that provides the client with instance of timezone from given Unicode alias.
		/// </summary>
		/// <param name="alias">Unicode alias of the timezone to be provided.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstanceFromAlias(string alias) 
		{
			if (alias == null) 
			{
				throw new System.ArgumentNullException("alias", "Unicode alias of timezone can not be null.");
			}
			string olsonName = FindNameFromAlias(alias);
			if (olsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Unicode alias '{0}' not supported. Corresponding Olson name not found.", alias), "alias");
			}
			return GetInstanceFromOlsonName(olsonName);
		}

		/// <summary>
		/// Helper factory, that provides the client with instance of timezone from given abbreviation.
		/// </summary>
		/// <param name="abbreviation">Abbreviation of the timezone.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstanceFromAbbreviation(string abbreviation) 
		{
			if (abbreviation == null) 
			{
				throw new System.ArgumentNullException("abbreviation", "Abbreviation can not be null.");
			}
			string olsonName = GetNameFromAbbreviation(abbreviation);
			if (olsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Abbreviation '{0}' not supported. Corresponding Olson name not found.", abbreviation), "abbreviation");
			}
			return GetInstanceFromOlsonName(olsonName);
		}


		/// <summary>
		/// Helper factory, that provides the client with instance of timezone from given military/NATO timezone letter.
		/// </summary>
		/// <param name="letter">Military/NATO timezone letter.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstanceFromMilitaryLetter(string letter) 
		{
			if (letter == null) 
			{
				throw new System.ArgumentNullException("letter", "Military/NATO timezone letter can not be null.");
			}
			string olsonName = GetNameFromMilitaryLetter(letter);
			if (olsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Military/NATO letter '{0}' not supported. Corresponding Olson name not found.", letter), "letter");
			}
			return GetInstanceFromOlsonName(olsonName);
		}


		/// <summary>
		/// Helper factory, that provides the client with instance of timezone from given military/NATO timezone name.
		/// </summary>
		/// <param name="militaryName">Military/NATO timezone name.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstanceFromMilitaryName(string militaryName) 
		{
			if (militaryName == null) 
			{
				throw new System.ArgumentNullException("militaryName", "Military/NATO timezone name can not be null.");
			}
			string olsonName = GetNameFromMilitaryName(militaryName);
			if (olsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Military/NATO name '{0}' not supported. Corresponding Olson name not found.", militaryName), "militaryName");
			}
			return GetInstanceFromOlsonName(olsonName);
		}


		/// <summary>
		/// Helper factory, that provides the client with instance of timezone from given arbitrary name (Olson name, Win32 id, Win32 name, Unicode alias).
		/// </summary>
		/// <param name="name">Arbitrary name of the timezone to be provided.</param>
		/// <returns>Instance of <see cref="OlsonTimeZone"/> type.</returns>
		public static OlsonTimeZone GetInstance(string name) 
		{
			if (name == null) 
			{
				throw new System.ArgumentNullException("name", "Name of timezone can not be null.");
			}
			string olsonName = LookupName(name);
			if (olsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Name '{0}' not supported. Corresponding Olson name not found.", name), "name");
			}
			return GetInstanceFromOlsonName(olsonName);
		}


		/// <summary>
		/// Gets the underlying <see cref="ZoneInfo"/> object.
		/// </summary>
		public ZoneInfo ZoneInfo 
		{
			get 
			{
				return zoneInfo;
			}
		}
		

		/// <summary>
		/// Gets all Olson timezone names.
		/// </summary>
		private static string[] AllNames
		{
			get 
			{
				if (allNames == null) 
				{
					allNames = ZoneInfo.AllNames;
				}
				return allNames;
			}
		}


		/// <summary>
		/// Gets primary Olson timezone names obtained by mapping all Unicode aliased defined in 'Time Zone Localization' draft document.
		/// See http://www.unicode.org/cldr/data/docs/design/formatting/time_zone_localization.html for more details.
		/// </summary>
		private static string[] AllPrimaryNames
		{
			get 
			{
				if (primaryNames == null) 
				{
					ArrayList res = new ArrayList();
					foreach(string name in AllNames) 
					{
						if (!UnicodeAliasMap.Contains(name)) 
						{
							res.Add(name);
						} 
						else 
						{
							Debug.Assert(Array.IndexOf(AllNames, UnicodeAliasMap[name]) >= 0);
						}
					}
					primaryNames = (string[])res.ToArray(typeof(string));
				}
				return primaryNames;
			}
		}


		/// <summary>
		/// Gets the short name of the timezone.
		/// </summary>
		public string Name 
		{
			get 
			{
				return zoneInfo.Name;
			}
		}


		/// <summary>
		/// Gets the amount of time, measured in ticks, to add to UTC to get standard time in this timezone.
		/// </summary>
		public TimeSpan RawUtcOffset 
		{
			get 
			{
				return new TimeSpan((long)zoneInfo.RawOffset * 10000);
			}
		}


		/// <summary>
		///  Gets zone's all daylight saving time periods.  
		/// </summary>
		private DaylightTime[] AllTimeChanges 
		{
			get 
			{
				if (allTimeChanges == null) 
				{
					ArrayList changes = new ArrayList();
					long startClock = ZoneInfo.MinClock;
					ZoneInfo.Rule startRule = ZoneInfo.DefaultStdRule;
					long[] clocks = zoneInfo.AllTransitionClocks;

					// Default change
					DateTime start = MinTime;
					DateTime end = clocks.Length == 0 ? MaxTime : zoneInfo.GetUtcTime(clocks[0]).DateTime.Add(new TimeSpan(0, 0, startRule.Offset));
					TimeSpan delta = new TimeSpan(0, 0, startRule.Offset);
					DaylightTime time = new StandardTime(start, end, delta);
					changes.Add(time);
				
					// Changes based on zone tranzitions
					for (int i = 0; i < clocks.Length; i++) 
					{
						long clock = clocks[i];
						ZoneInfo.Rule rule = zoneInfo.GetRule(clock);
						start = zoneInfo.GetUtcTime(clock).DateTime.Add(new TimeSpan(0, 0, startRule.Offset));
						end = (i < clocks.Length - 1) ? zoneInfo.GetUtcTime(clocks[i + 1]).DateTime.Add(new TimeSpan(0, 0, rule.Offset)) : MaxTime;
						delta = new TimeSpan(0, 0, rule.Offset - startRule.Offset);
						time = rule.IsDST ? new DaylightTime(start, end, delta) : new StandardTime(start, end, delta);
						changes.Add(time);
						startClock = clock;
						startRule = rule;
					}
					allTimeChanges = (DaylightTime[])changes.ToArray(typeof(DaylightTime));
				}
				return allTimeChanges;
			}
		}


		/// <summary>
		/// Local times can be ambiguous and should be used carefully. This method allows to check
		/// the time value within the current timezone.
		/// </summary>
		/// <param name="time">Time to check.</param>
		/// <returns>Value of type <see cref="TimeCheckResult"/>indicating the result of the check.</returns>
		public TimeCheckResult CheckLocalTime(DateTime time) 
		{
			if (time < MinTime) 
			{
				return TimeCheckResult.LessThanUnixMin;
			} 
			else if (time > MaxTime)
			{
				return TimeCheckResult.GreaterThanUnixMax;
			} 
			else if (time == MaxTime) 
			{				
				return zoneInfo.GetRule(ZoneInfo.MaxClock).Offset < 0 ? TimeCheckResult.InSpringForwardGap : TimeCheckResult.Valid;
			}

			Debug.Assert(AllTimeChanges.Length > 0);

			DaylightTime currentChange = null;
			DaylightTime nextChange = null;
		
			for (int i = 0; i < AllTimeChanges.Length; i++) 
			{
				if (time >= AllTimeChanges[i].Start && time < AllTimeChanges[i].End) 
				{
					currentChange = AllTimeChanges[i];
					if (i < AllTimeChanges.Length - 1) 
					{
						nextChange = AllTimeChanges[i + 1];
					}
					break;
				}
			}

			Debug.Assert(currentChange != null);
			if (currentChange != null)
			{
				if (time >= currentChange.Start && time < currentChange.Start.Add(currentChange.Delta))
				{
					return TimeCheckResult.InSpringForwardGap;
				}

				if (nextChange != null)
				{
					if (time >= currentChange.End.Add(nextChange.Delta) && time <= currentChange.End)
					{
						return TimeCheckResult.InFallBackRange;
					}
				}
				else
				{
					Debug.Assert(currentChange.End == MaxTime);
					if (time > currentChange.End.Add(new TimeSpan(0, 0, zoneInfo.GetRule(ZoneInfo.MaxClock).Offset)) && time <= currentChange.End)
					{
						return TimeCheckResult.InSpringForwardGap;
					}
				}
			}
			return TimeCheckResult.Valid;
		}


		/// <summary>
		/// Returns true if this timezone has transitions between various offsets
		/// from universal time, such as standard time and daylight time.
		/// </summary>
		public bool UsesDaylightTime 
		{ 
			get 
			{
				return zoneInfo.UsesDaylightTime;
			}
		}


		/// <summary>
		/// Helper method that converts between arbitrary timezones.
		/// </summary>
		/// <param name="fromZoneName">Arbitrary name of the source timezone.</param>
		/// <param name="fromTime">Time to convert.</param>
		/// <param name="toZoneName">Arbitrary name of the destintation timezone.</param>
		/// <returns></returns>
		public static DateTime Convert(string fromZoneName, DateTime fromTime, string toZoneName) 
		{
			string fromOlsonName = LookupName(fromZoneName);
			if (fromOlsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Name '{0}' not supported. Corresponding Olson name not found.", fromZoneName), "fromZoneName");
			}
			string toOlsonName = LookupName(toZoneName);
			if (toOlsonName == null) 
			{
				throw new System.ArgumentException(string.Format("Name '{0}' not supported. Corresponding Olson name not found.", toOlsonName), "toOlsonName");
			}
			return GetInstanceFromOlsonName(toOlsonName).ToLocalTime(GetInstanceFromOlsonName(fromOlsonName).ToUniversalTime(fromTime));
		}


		/// <summary>
		/// Gets the default time zone.
		/// </summary>
		public static OlsonTimeZone DefaultUtcTimeZone 
		{
			get 
			{
				return defaultUtcTimeZone;
			}
		}


		/// <summary>
		/// Gets the zoneinfo directory name used to instantiate the undelrlying <see cref="ZoneInfo"/> 
		/// objects. By design this is leapsecond free directory, which is a default directory
		/// of <see cref="ZoneInfo"/> class.
		/// </summary>
		public static string ZoneInfoDir 
		{
			get 
			{
				return ZoneInfo.DefaultDir;
			}
		}


		/// <summary>
		/// Gets all supported Unicode aliases defined in 'Time Zone Localization' draft document.
		/// See http://www.unicode.org/cldr/data/docs/design/formatting/time_zone_localization.html
		/// </summary>
		private static string[] AllAliases
		{
			get 
			{
				Debug.Assert(UnicodeAliasMap != null);
				return (string[])new ArrayList(UnicodeAliasMap.Keys).ToArray(typeof(string));
			}
		}


		/// <summary>
		/// Gets the corresponding Olson name for specified Win32 Id based on Unicode CLDR supplemental data.
		/// </summary>
		/// <param name="win32Id">Win32 Id for which the corresponding Olson name is requested. Supports both Unicode (short) and Registry (long) formats.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string GetNameFromWin32Id(string win32Id) 
		{
			if (win32Id == null) 
			{
				throw new System.ArgumentNullException("win32Id", "Win32 Id of timezone can not be null.");
			}
			Debug.Assert(UnicodeWin32Map != null);
			UnicodeWin32MapValue value = (UnicodeWin32MapValue)UnicodeWin32Map[win32Id];
			return value != null ? value.OlsonName : null;
		}


		/// <summary>
		/// Gets the corresponding Olson name for specified Win32 name based on Unicode CLDR supplemental data.
		/// </summary>
		/// <param name="win32Name">Win32 name for which the corresponding Olson name is requested.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string GetNameFromWin32Name(string win32Name) 
		{
			if (win32Name == null) 
			{
				throw new System.ArgumentNullException("win32Name", "Win32 name of timezone can not be null.");
			}
			Debug.Assert(Win32NameToOlsonMap != null);
			return (string)Win32NameToOlsonMap[win32Name];
		}


		/// <summary>
		/// Gets the corresponding Olson name for specified registry Win32 name.
		/// </summary>
		/// <param name="registryWin32Name">Registry Win32 name for which the corresponding Olson name is requested.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string GetNameFromRegistryWin32Name(string registryWin32Name) 
		{
			if (registryWin32Name == null) 
			{
				throw new System.ArgumentNullException("registryWin32Name", "Registry Win32 name of timezone can not be null.");
			}
			Debug.Assert(RegistryWin32NameToOlsonMap != null);
			return (string)RegistryWin32NameToOlsonMap[registryWin32Name];
		}


		/// <summary>
		/// Gets the corresponding Olson name for specified Unicode alias as defined in 'Time Zone Localization' draft document.
		/// See http://www.unicode.org/cldr/data/docs/design/formatting/time_zone_localization.html
		/// </summary>
		/// <param name="alias">Unicode alias for which the corresponding Olson name is requested.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string GetNameFromAlias(string alias) 
		{
			if (alias == null) 
			{
				throw new System.ArgumentNullException("alias", "Unicode alias of timezone can not be null.");
			}
			Debug.Assert(UnicodeAliasMap != null);
			return (string)UnicodeAliasMap[alias];
		}


		/// <summary>
		/// Finds the corresponding leaf Olson name for specified Unicode alias as defined in 'Time Zone Localization' draft document.
		/// See http://www.unicode.org/cldr/data/docs/design/formatting/time_zone_localization.html
		/// </summary>
		/// <param name="alias">Unicode alias for which the corresponding Olson name is requested.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string FindNameFromAlias(string alias) 
		{
			if (alias == null) 
			{
				throw new System.ArgumentNullException("alias", "Unicode alias of timezone can not be null.");
			}
			Debug.Assert(UnicodeAliasMap != null);
			while (UnicodeAliasMap[alias] != null) 
			{
				if (UnicodeAliasMap[UnicodeAliasMap[alias]] != null) 
				{
					alias = (string)UnicodeAliasMap[alias];
				} 
				else 
				{
					break;
				}
			}
			return (string)UnicodeAliasMap[alias];
		}


		/// <summary>
		/// Finds the corresponding Olson name for specified arbitrary name (Olson name, Win32 id, Win32 name, Unicode alias).
		/// </summary>
		/// <param name="name">Arbitrary name for which the corresponding Olson name is requested.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string LookupName(string name) 
		{
			if (name == null) 
			{
				throw new System.ArgumentNullException("name", "Name or Id of timezone can not be null.");
			}
			string res = null;
			if (Array.IndexOf(AllNames, name) >= 0) 
			{
				res = name;
			} 
			else 
			{
				res = FindNameFromAlias(name);
				if (res == null) 
				{
					res = GetNameFromWin32Id(name);
					if (res == null) 
					{
						res = GetNameFromWin32Name(name);
						if (res == null) 
						{
							res = GetNameFromRegistryWin32Name(name);
							if (res == null) 
							{
								res = GetNameFromAbbreviation(name);
								if (res == null)
								{
									res = GetNameFromMilitaryLetter(name);
									if (res == null)
									{
										res = GetNameFromMilitaryName(name);
									}
								}
							}							
						}
					}
				}
			}
			return res;
		}


		/// <summary>
		/// Gets the corresponding Win32 name for specified Win32 Id based on Unicode CLDR supplemental data.
		/// </summary>
		/// <param name="win32Id">Win32 Id for which the corresponding Olson name is required.</param>
		/// <returns>Corresponding Win32 name.</returns>
		public static string GetWin32NameFromWin32Id(string win32Id) 
		{
			if (win32Id == null) 
			{
				throw new System.ArgumentNullException("win32Id", "Win32 Id of timezone can not be null.");
			}
			Debug.Assert(UnicodeWin32Map != null);
			UnicodeWin32MapValue value = (UnicodeWin32MapValue)UnicodeWin32Map[win32Id];
			return value != null ? value.Win32Name : null;
		}


		/// <summary>
		/// Gets corresponding Win32 Id as defined in Unicode CLDR supplemental data.
		/// </summary>
		public string Win32Id 
		{
			get 
			{
				Debug.Assert(OlsonToWin32IdMap != null);
				return (string)OlsonToWin32IdMap[Name];
			}
		}


		/// <summary>
		/// Gets corresponding Win32 name as defined in Unicode CLDR supplemental data.
		/// </summary>
		public string Win32Name
		{
			get 
			{
				Debug.Assert(OlsonToWin32NameMap != null);
				return (string)OlsonToWin32NameMap[Name];
			}
		}


		/// <summary>
		/// Calculates the coverage of the timezone by the rule.
		/// </summary>
		/// <param name="startClock">Start unix clock of the rule.</param>
		/// <param name="endClock">End unix clock of the rule.</param>
		/// <returns>Decimal number within range (0-1] indicating the coverage value.</returns>
		private static double CalculateRuleCoverage(long startClock, long endClock) 
		{
			Debug.Assert(startClock <= endClock);
			double x1 = - ZoneInfo.MinClock + startClock;
			double x2 = - ZoneInfo.MinClock + endClock;
			return (Math.Pow(x2, 2) - Math.Pow(x1, 2)) / (4 * Math.Pow(ZoneInfo.MinClock, 2));
		}


		/// <summary>
		/// Gets the area part from Olson name. Olson name is in the form Area/Location.
		/// </summary>
		/// <param name="olsonName">Olson name to get the area for.</param>
		/// <returns>Area of Olson name.</returns>
		private static string GetArea(string olsonName) 
		{
			int pos = olsonName.IndexOf("/");
			return pos >= 0 ? olsonName.Substring(0, pos) : string.Empty;
		}


		/// <summary>
		/// Finds the most numerous timezone name areas i.e. Europe, US, America, Africa etc.
		/// </summary>
		/// <param name="values">List of rank values to select the timezone from.</param>
		/// <returns>List of most numerous timezone name areas.</returns>
		private static string[] FindMostNumerousArea(ArrayList values) 
		{
			Hashtable prefixMap = new Hashtable();
			foreach(RankValue rankValue in values) 
			{
				string area = GetArea(rankValue.OlsonName);
				if (prefixMap.ContainsKey(area)) 
				{
					prefixMap[area] = ((int)prefixMap[area]) + 1;
				} 
				else 
				{
					prefixMap.Add(area, 1);
				}
			}

			int max = 0;
			foreach(int count in prefixMap.Values)
			{
				if (count >= max) 
				{
					max = count;
				}
			}

			ArrayList res = new ArrayList();

			foreach(string prefix in prefixMap.Keys) 
			{
				double ratio = (double)((int)prefixMap[prefix])/max;
				if (ratio >= 0.40) 
				{
					res.Add(prefix);
				}
			}
			return (string[])res.ToArray(typeof(string));
		}


		/// <summary>
		/// Finds the most significant rank value.
		/// </summary>
		/// <param name="values">Collection of rank values to select the timezone name from.</param>
		/// <returns>Instance of most significant rank value.</returns>
		private static RankValue FindMostSignificantRankValue(ICollection values) 
		{
			ArrayList rankValues = new ArrayList(values);
			rankValues.Sort();
			rankValues.Reverse();
			string[] areas = FindMostNumerousArea(rankValues);
			foreach(RankValue rankValue in rankValues) 
			{
				if (Array.IndexOf(areas, GetArea(rankValue.OlsonName)) >= 0)
				{
					return rankValue;
				}
			}
			return null;
		}


		/// <summary>
		/// Finds the name of most significant timezone.
		/// </summary>
		/// <param name="values">Collection of rank values to select the timezone name from.</param>
		/// <returns>Name of most significant timezone.</returns>
		private static string FindMostSignificantName(ICollection values) 
		{
			RankValue rankValue = FindMostSignificantRankValue(values);
			return rankValue != null ? rankValue.OlsonName : null;
		}


		/// <summary>
		/// Finds the rules's offset with the highest number of references.
		/// </summary>
		/// <param name="offsetMap">Rule's map of offset string to rank value collection.</param>
		/// <returns>List of most numerous offsets.</returns>
		private static string[] FindMostNumerousRuleOffset(Hashtable offsetMap)
		{
			int max = 0;
			foreach(Hashtable rankMap in offsetMap.Values)
			{
				if (rankMap.Count >= max) 
				{
					max = rankMap.Count;
				}
			}

			ArrayList res = new ArrayList();

			foreach(string offsetString in offsetMap.Keys)
			{
				if (((Hashtable)offsetMap[offsetString]).Count == max) 
				{
					res.Add(offsetString);
				}
			}
			return (string[])res.ToArray(typeof(string));
		}


		/// <summary>
		/// Finds the most significant offset for the offset map of given rule.
		/// </summary>
		/// <param name="offsetMap">Rule's map of offset string to rank value collection.</param>
		/// <returns>Offset string of most significant offset within offsetMap.</returns>
		private static string FindMostSignificantRuleOffset(Hashtable offsetMap) 
		{
			string[] offsets = FindMostNumerousRuleOffset(offsetMap);
			if (offsets.Length == 1) 
			{
				return offsets[0];
			} 
			else if (offsets.Length > 1)
			{
				string maxOffset = null;
				RankValue maxRankValue = null;
				foreach(string offset in offsets) 
				{
					Hashtable rankMap = (Hashtable)offsetMap[offset];
					RankValue rankValue = FindMostSignificantRankValue(rankMap.Values);
					if (maxRankValue == null || maxRankValue.CompareTo(rankValue) <= 0) 
					{
						maxOffset = offset;
					}
				}
				Debug.Assert(maxOffset != null);
				return maxOffset;
			} 
			else 
			{
				return null;
			}
		}


		/// <summary>
		/// Adds the rule to the map.
		/// </summary>
		/// <param name="ruleMap">Temporary map to process the rule against.</param>
		/// <param name="name">Olson name of timezone referenced by the rule.</param>
		/// <param name="rule">Rule to process.</param>
		/// <param name="startClock">Unix time of entering the rule by timezone.</param>
		/// <param name="endClock">Unix time of exiting the rule by timezone.</param>
		/// <returns>Instance of the object denoting the rank of rule within timezone.</returns>
		private static RankValue AddRuleToMap(Hashtable ruleMap, string name, ZoneInfo.Rule rule, long startClock, long endClock) 
		{
			Debug.Assert(ruleMap != null);
			Debug.Assert(name != null);
			Debug.Assert(rule != null);
			Hashtable offsetMap = (Hashtable)ruleMap[rule.Name];
			if (offsetMap == null) 
			{
				offsetMap = new Hashtable();
				ruleMap.Add(rule.Name, offsetMap);
			}
			int roundedOffset = RoundOffset(rule.Offset);
			Hashtable rankMap = (Hashtable)offsetMap[GetOffsetString(roundedOffset)];
			if (rankMap == null) 
			{
				rankMap = new Hashtable();
				offsetMap.Add(GetOffsetString(roundedOffset), rankMap);
			}
					
			RankValue rankValue = (RankValue)rankMap[name];
			if (rankValue  == null)
			{
				rankValue = new RankValue(name);
				rankMap.Add(name, rankValue);
			}
			rankValue.Coverage += CalculateRuleCoverage(startClock, endClock);
			rankValue.AllTransitionCount += 1;
			if (startClock >= currentClock) 
			{
				rankValue.FutureTransitionCount += 1;
			}

			Debug.Assert(rankValue != null);
			return rankValue;
		}


		/// <summary>
		/// Creates the map of rule name to map of offset to RuleMapValue.
		/// <param name="olsonNames">Olson names to create the map for.</param>
		/// <param name="includeDST">Flag indicating wheter to create full map i.e. including DST rules.</param>
		/// </summary>
		/// <returns>Instance of map of rule name to map of offset to RuleMapValue.</returns>
		private static Hashtable CreateRuleMap(string[] olsonNames, bool includeDST) 
		{
			Hashtable res = new Hashtable();
			foreach (string name in olsonNames) 
			{
				OlsonTimeZone tz = GetInstanceFromOlsonName(name);
				int imax = tz.ZoneInfo.AllTransitionClocks.Length - 1;
				for (int i = 0; i <= imax; i++) 
				{
					long startClock = tz.ZoneInfo.AllTransitionClocks[i];
					long endClock = i < imax ? tz.ZoneInfo.AllTransitionClocks[i + 1] : ZoneInfo.MaxClock;
					ZoneInfo.Rule rule = tz.ZoneInfo.GetTransitionRule(startClock);
					if (includeDST || !rule.IsDST) 
					{
						RankValue rankValue = AddRuleToMap(res, name, rule, startClock, endClock);
						rankValue.IsLastTransition = (startClock <= currentClock);
					}
				}
				// if timezone has no transitions, iterate through all defined rules
				if (res.Count == 0)
				{
					imax = tz.ZoneInfo.AllRules.Length - 1;
					for (int i = 0; i <= imax; i++) 
					{
						ZoneInfo.Rule rule = tz.ZoneInfo.AllRules[i];
						if (includeDST || !rule.IsDST) 
						{
							RankValue rankValue = AddRuleToMap(res, name, rule, ZoneInfo.MinClock, ZoneInfo.MaxClock);
							rankValue.IsLastTransition = true;
						}
					}
				}
			}
			return res;
		}


		/// <summary>
		/// Creates the map of abbreviation to Olson name.
		/// </summary>
		/// <returns>Instance of map of abbreviation name to Olson name.</returns>
		private static Hashtable CreateAbbreviationToOlsonMap() 
		{
			Hashtable res = new Hashtable();
			Hashtable ruleMap = CreateRuleMap(AllNames, true);
			foreach (string ruleName in ruleMap.Keys) 
			{
				Hashtable offsetMap = (Hashtable)ruleMap[ruleName];
				Debug.Assert(offsetMap != null);

				string mostSignificantOffset = FindMostSignificantRuleOffset(offsetMap);
				Debug.Assert(mostSignificantOffset != null);
				Hashtable mostSignificantRankMap = (Hashtable)offsetMap[mostSignificantOffset];
				Debug.Assert(mostSignificantRankMap != null);
				string ruleDefaultName = FindMostSignificantName(mostSignificantRankMap.Values);
				Debug.Assert(ruleDefaultName != null);
				res.Add(ruleName, ruleDefaultName);

				foreach(string offsetString in offsetMap.Keys) 
				{
					Hashtable rankMap = (Hashtable)offsetMap[offsetString];
					Debug.Assert(rankMap != null);
					string offsetDefaultName = FindMostSignificantName(rankMap.Values);
					Debug.Assert(offsetDefaultName != null);
					res.Add(string.Format("{0}{1}", ruleName, offsetString), offsetDefaultName);
				}
				
			}
			return res;
		}


		/// <summary>
		/// Gets the map of abbreviation name to Olson name.
		/// </summary>
		private static Hashtable AbbreviationToOlsonMap 
		{
			get 
			{
				if (abbreviationToOlsonMap == null)  
				{
					abbreviationToOlsonMap = CreateAbbreviationToOlsonMap();
				}
				return abbreviationToOlsonMap;
			}
		}


		/// <summary>
		/// Gets the list of all primary abbreviations.
		/// </summary>
		private static string[] AllPrimaryAbbreviations 
		{
			get 
			{
				ArrayList res = new ArrayList();
				Regex r = new Regex("[+|-][0-9][0-9]:[0-9][0-9]$");
				foreach(string abbreviation in AbbreviationToOlsonMap.Keys) 
				{
					if (!r.IsMatch(abbreviation)) 
					{
						res.Add(abbreviation);
					}
				}
				return (string[])res.ToArray(typeof(string));
			}
		}


		/// <summary>
		/// Gets the list of all abbreviations.
		/// </summary>
		private static string[] AllAbbreviations 
		{
			get 
			{
				return (string[])(new ArrayList(AbbreviationToOlsonMap.Keys).ToArray(typeof(string)));
			}
		}


		/// <summary>
		/// Gets the corresponding Olson name for specified abbreviation.
		/// </summary>
		/// <param name="abbreviation">Abbreviation for which the corresponding Olson name is requested.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string GetNameFromAbbreviation(string abbreviation) 
		{
			if (abbreviation == null) 
			{
				throw new System.ArgumentNullException("abbreviation", "Abbreviation of timezone can not be null.");
			}
			return (string)AbbreviationToOlsonMap[abbreviation];
		}


		/// <summary>
		/// Gets the corresponding Olson name for specified militart/NATO letter.
		/// </summary>
		/// <param name="letter">Militart/NATO letter for which the corresponding Olson name is requested.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string GetNameFromMilitaryLetter(string letter) 
		{
			if (letter == null) 
			{
				throw new System.ArgumentNullException("letter", "Letter of military/NATO timezone can not be null.");
			}
			Debug.Assert(MilitaryMap != null);
			if (MilitaryMap.ContainsKey(letter))
			{
				MilitaryMapValue value = (MilitaryMapValue)MilitaryMap[letter];
				Debug.Assert(value != null);
				return value.OlsonName;
			}
			else
			{
				return null;
			}
		}


		/// <summary>
		/// Gets the corresponding Olson name for specified militart/NATO name.
		/// </summary>
		/// <param militaryName="letter">Militart/NATO name for which the corresponding Olson name is requested.</param>
		/// <returns>Corresponding Olson name.</returns>
		public static string GetNameFromMilitaryName(string militaryName) 
		{
			if (militaryName == null) 
			{
				throw new System.ArgumentNullException("militaryName", "Name of military/NATO timezone can not be null.");
			}
			Debug.Assert(MilitaryMap != null);
			foreach(MilitaryMapValue value in MilitaryMap.Values)
			{
				if (value.MilitaryName == militaryName)
				{
					return value.OlsonName;
				}
			}
			return null;
		}


		/// <summary>
		/// Gets the corresponding military/NATO name for specified militart/NATO letter.
		/// </summary>
		/// <param name="letter">Militart/NATO letter for which the corresponding military/NATO name is requested.</param>
		/// <returns>Corresponding military/NATO name.</returns>
		public static string GetMilitaryNameFromLetter(string letter) 
		{
			if (letter == null) 
			{
				throw new System.ArgumentNullException("letter", "Letter of military/NATO timezone can not be null.");
			}
			Debug.Assert(MilitaryMap != null);
			if (MilitaryMap.ContainsKey(letter))
			{
				return ((MilitaryMapValue)MilitaryMap[letter]).MilitaryName;
			}
			else
			{
				return null;
			}
		}


		/// <summary>
		/// Creates the map of Olson name to abbreviation.
		/// </summary>
		/// <returns>Instance of map of Olson name to standard abbreviation.</returns>
		private static Hashtable CreateOlsonToStdAbbreviationMap()
		{
			Hashtable res = new Hashtable();
			foreach(string olsonName in AllNames) 
			{
				string maxAbbreviation = null;
				RankValue maxRankValue = null;

				// find the strongest rule for given timezone
				Hashtable ruleMap = CreateRuleMap(new string[]{olsonName}, false);
				Debug.Assert(ruleMap != null);
				foreach (string ruleName in ruleMap.Keys) 
				{
					Hashtable offsetMap = (Hashtable)ruleMap[ruleName];
					Debug.Assert(offsetMap != null);
					foreach(Hashtable rankMap in offsetMap.Values) 
					{
						Debug.Assert(rankMap.Count == 1);
						RankValue rankValue = (RankValue)new ArrayList(rankMap.Values)[0];
						if (maxRankValue == null || rankValue.CompareTo(maxRankValue) > 0) 
						{
							maxAbbreviation = ruleName;
							maxRankValue = rankValue;
						}
					}
				}
				Debug.Assert(maxAbbreviation != null && maxRankValue != null);
				Debug.Assert(maxRankValue.OlsonName == olsonName);
				res.Add(olsonName, maxAbbreviation);
			}
			return res;
		}


		/// <summary>
		/// Gets the map of Olson name to standard abbreviation.
		/// </summary>
		private static Hashtable OlsonToStdAbbreviationMap
		{
			get 
			{
				if (olsonToStdAbbreviationMap == null)  
				{
					olsonToStdAbbreviationMap = CreateOlsonToStdAbbreviationMap();
				}
				return olsonToStdAbbreviationMap;
			}
		}


		/// <summary>
		/// Gets the standard abbreviation of timezone.
		/// </summary>
		public string StandardAbbreviation 
		{
			get 
			{
				return (string)OlsonToStdAbbreviationMap[Name];
			}
		}


		/// <summary>
		/// Rounds the offset value to hours and minutes. Assumes offset less than 24h.
		/// </summary>
		/// <param name="offset">Offset to round in seconds.</param>
		/// <returns>Rounded offset in seconds.</returns>
		private static int RoundOffset(int offset) 
		{
			Debug.Assert(offset < 24*3600, "Offset value out of range.");
			TimeSpan ts = new TimeSpan(offset * TimeSpan.TicksPerSecond);
			return ts.Hours * 3600 + ts.Minutes * 60;
		}


		/// <summary>
		/// Generates the string representation of the offset.
		/// </summary>
		/// <param name="offset">Rule offset.</param>
		/// <returns>String representation of the rule offset.</returns>
		private static string GetOffsetString(int offset) 
		{
			TimeSpan ts = new TimeSpan(offset * TimeSpan.TicksPerSecond);
			return string.Format("{0}{1:00}:{2:00}", offset >= 0 ? "+" : "-", Math.Abs(ts.Hours), Math.Abs(ts.Minutes));
		}

		#endregion
	}
}
