using System;
using System.Collections;
using System.Reflection;
using GeneXus.Application;
using GX;
using System.Collections.Generic;
using GeneXus.Utils;
using System.IO;
using System.Globalization;

namespace GeneXus.MapServices
{
	public class LocationInfo : GxUserType
	{
		public Geospatial Location;
		public String Description;
		public DateTime Time;
		public Double Precision;
		public Double Heading;
		public Double Speed;

#region Json
		private static Hashtable mapper;
		public override String JsonMap(String value)
		{
			if (mapper == null)
			{
				mapper = new Hashtable();
			}
			return (String)mapper[value]; ;
		}

		public override void ToJSON()
		{
			ToJSON(true);
			return;
		}

		public override void ToJSON(bool includeState)
		{
			AddObjectProperty("Location", Location, false);
			AddObjectProperty("Description", Description, false);
			String STime = GeneXus.Utils.DateTimeUtil.TToC2(Time, false);
			AddObjectProperty("Time", STime, false);
			AddObjectProperty("Precision", Precision, false);
			AddObjectProperty("Heading", Heading, false);
			AddObjectProperty("Speed", Speed, false);
			return;
		}

#endregion
	}

	public class Maps
    {
		private const string GENEXUS_CORE_DLL_PATH = @"GeneXus.dll";
		private const string NAMESPACE = @"GeneXus.Core.genexus.common";
		private static Assembly assembly;

		private const string DIRECTIONS_PARAMETERS_SDT_CLASS_NAME = @"SdtDirectionsRequestParameters";
		
		public static dynamic CalculateDirections(Geospatial a, Geospatial b, string transportType = "", bool requestAlternateRoutes = false)
		{
			LoadAssemblyIfNeeded();

			Type classType = assembly.GetType(NAMESPACE + "." + DIRECTIONS_PARAMETERS_SDT_CLASS_NAME, false, ignoreCase: true);
			if (classType != null && Activator.CreateInstance(classType) is GxUserType parametersSDT)
			{
				classType.GetProperty("gxTpr_Sourcelocation").SetValue(parametersSDT, a);
				classType.GetProperty("gxTpr_Destinationlocation").SetValue(parametersSDT, b);
				classType.GetProperty("gxTpr_Transporttype").SetValue(parametersSDT, transportType);
				classType.GetProperty("gxTpr_Requestalternateroutes").SetValue(parametersSDT, requestAlternateRoutes);

				return CalculateDirections(parametersSDT);
			}

			return null;
		}

		private const string DIRECTIONS_SERVICE_INTERNAL_PROCEDURE_CLASS_NAME = @"googlemapsdirectionsserviceinternal";

		public static dynamic CalculateDirections(GxUserType directionsParameters)
		{
			LoadAssemblyIfNeeded();

			Type procedureClassType = assembly.GetType(NAMESPACE + "." + DIRECTIONS_SERVICE_INTERNAL_PROCEDURE_CLASS_NAME, false, true);
			Type sdtClassType = assembly.GetType(NAMESPACE + "." + DIRECTIONS_PARAMETERS_SDT_CLASS_NAME, false, ignoreCase: true);
			if (procedureClassType != null && sdtClassType != null)
			{
				MethodBase method = procedureClassType.GetMethod("executeUdp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				object gxProdecure = Activator.CreateInstance(procedureClassType, new object[] { GxContext.Current });
				return (GxUserType)method.Invoke(gxProdecure, new object[] { directionsParameters });
			}

			return null;
		}

		private static void LoadAssemblyIfNeeded() {
			if (assembly == null)
			{
				assembly = LoadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), GENEXUS_CORE_DLL_PATH));
				if (assembly == null)
					assembly = LoadAssembly(Path.Combine(GxContext.StaticPhysicalPath(), "bin", GENEXUS_CORE_DLL_PATH));
			}
		}

		private static Assembly LoadAssembly(string fileName)
		{
			if (File.Exists(fileName))
			{
				Assembly ass = Assembly.LoadFrom(fileName);
				return ass;				
			}
			else
				return null;
		}

		public static LocationInfo GetCurrentLocation(int minAccuracy, int timeout, bool includeHAndS, bool ignoreErrors)
		{
			if (Application.GxContext.IsHttpContext)
			{
				String ip = Application.GxContext.Current.GetRemoteAddress();
				string info = new System.Net.WebClient().DownloadString("http://ipinfo.io/" + ip);
				return new LocationInfo { };
			}
			return new LocationInfo { };
		}

		public static LocationInfo GetCurrentLocation(int minAccuracy, int timeout, bool includeHAndS)
		{
			return new LocationInfo { };
		}

		public static double GetLatitude(Geospatial Point) {
			return Point.Latitude;
		}

		public static double GetLongitude(Geospatial Point)
		{
			return Point.Longitude;
		}

		public static double GetDistance(Geospatial Start, Geospatial Destination)
		{
			return Start.STDistance(Destination);
		}

		public static GxSimpleCollection<String> ReverseGeocode(Geospatial coordinate) {

			GxSimpleCollection<String> addresses = new GxSimpleCollection<string>();
#if !NETCORE
			String LatLong = coordinate.Latitude.ToString(CultureInfo.InvariantCulture) + "," + coordinate.Longitude.ToString(CultureInfo.InvariantCulture);
			List<String> locationAddresses = GXGeolocation.GetAddress(LatLong);
			addresses.AddRange(locationAddresses);

#endif
			return addresses;
		}
		public static GxSimpleCollection<Geospatial> GeocodeAddress(String address)
		{
			GxSimpleCollection<Geospatial> loclist = new GxSimpleCollection<Geospatial>();
#if !NETCORE
			List<Geospatial> locations = GXGeolocation.GetLocationGeography(address);
			loclist.AddRange(locations);

#endif
			return loclist;
		}
	}	
}
