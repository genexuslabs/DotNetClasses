using System;
using System.Collections;
using System.Collections.Generic;

using GeneXus.Utils;
using System.IO;
using System.Globalization;
using GeneXus.Configuration;
using GX;
using Jayrock.Json;
using System.Net.Http;

namespace GeneXus.MapServices
{
	public class Directions : GxUserType
	{
		public GxSimpleCollection<Route> Routes;
		public SdtMessages_Message Messages;

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
			AddObjectProperty("Routes", Routes, false);
			AddObjectProperty("Messages", Messages, false);
			return;
		}

		#endregion

	}
	public class Route:GxUserType
	{
		public String name;
		public Double distance;
		public GxSimpleCollection<String> advisoryNotices;
		public Double expectedTravelTime;
		public String transportType;
		public Geospatial geoline;

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
			AddObjectProperty("name", name, false);
			AddObjectProperty("distance", distance, false);
			AddObjectProperty("advisoryNotices", advisoryNotices, false);
			AddObjectProperty("expectedTravelTime", expectedTravelTime, false);
			AddObjectProperty("transportType", transportType, false);
			AddObjectProperty("geoline", geoline, false);
			return;
		}

		#endregion
	}

	public class LocationInfo:  GxUserType {
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
		public static Directions CalculateDirections(Geospatial a, Geospatial b) {
			return CalculateDirections(a, b, "", false);
		}

		public static Directions CalculateDirections(Geospatial a, Geospatial b, String transportType ,
									   bool requestAlternateRoutes)
		{
			Directions directionsCalculated = new Directions();
			String ApiKey = "";
			if (Config.GetValueOf("GoogleApiKey", out ApiKey))
			{
				String queryString = $"json?key={ApiKey}&origin={a.Latitude.ToString(CultureInfo.InvariantCulture)},{a.Longitude.ToString(CultureInfo.InvariantCulture)}&destination={b.Latitude.ToString(CultureInfo.InvariantCulture)},{b.Longitude.ToString(CultureInfo.InvariantCulture)}";
				String transportMode = TransportMode(transportType);
				if (!String.IsNullOrEmpty(transportMode))
				{
					queryString += $"&mode={transportMode}";
				}
				if (requestAlternateRoutes)
				{
					queryString += "&alternatives=true";
				}
				Http.Client.GxHttpClient http = new Http.Client.GxHttpClient();
				http.Host = "maps.googleapis.com";
				http.Secure = 1;
				http.BaseURL = "maps/api/directions/";
				http.Execute("GET", queryString);
				if (http.StatusCode == 200)
				{
					String result = http.ToString();
					directionsCalculated = ParseResponse(result);
				}
			}
			return directionsCalculated;
		}

		private static String DecodePolyLine(String EncodedPolyLine)
		{

			int len = EncodedPolyLine.Length;

			System.Collections.ArrayList path = new System.Collections.ArrayList(len / 2);
			int index = 0;
			int lat = 0;
			int lng = 0;

			while (index < len)
			{
				int result = 1;
				int shift = 0;
				int b;
				do
				{
					b = EncodedPolyLine[index++] - 63 - 1;
					result += b << shift;
					shift += 5;
				} while (b >= 0x1f);
				lat += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

				result = 1;
				shift = 0;
				do
				{
					b = EncodedPolyLine[index++] - 63 - 1;
					result += b << shift;
					shift += 5;
				} while (b >= 0x1f);
				lng += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

				path.Add((lng * 1e-5).ToString(CultureInfo.InvariantCulture) + " " + (lat * 1e-5).ToString(CultureInfo.InvariantCulture));
			}
			return String.Join(",", path.ToArray());
		}

		static Directions ParseResponse(String response)
		{
			Directions directionsCalculated = new Directions();
			directionsCalculated.Routes = new GxSimpleCollection<Route>();
			JObject objResponse = JSONHelper.ReadJSON<JObject>(response);
			if (objResponse != null)
			{
				JArray routes = (JArray) objResponse["routes"];
				foreach (JObject r in routes)
				{
					
					int routeDistance = 0;
					int routeDuration = 0;
					List<string> polyLineData = new List<string>();
					String travelMode = "";
					JArray legs = (JArray)(r["legs"]);
					foreach (JObject l in legs)
					{
						routeDistance += (int)((JObject)(l["distance"]))["value"];
						routeDuration += (int)((JObject)(l["duration"]))["value"];
						JArray steps = (JArray)l["steps"];
						foreach (JObject s in steps)
						{
							String encodedLine = (String)((JObject)(s["polyline"]))["points"];
							String decodedLine = DecodePolyLine(encodedLine);
							if (decodedLine.Length > 0)
							{
								polyLineData.Add(decodedLine);
							}
							travelMode = (String)s["travel_mode"];
						}
					}
					string lineStringPoints = string.Join(",", polyLineData);
					Route currentRoute = new Route();
					currentRoute.name = (String)r["summary"];
					currentRoute.transportType = travelMode;
					currentRoute.distance = routeDistance;
					currentRoute.expectedTravelTime = routeDuration;
					currentRoute.geoline = new Geospatial("LINESTRING(" + lineStringPoints + ")");
					directionsCalculated.Routes.Add(currentRoute);
				}
			}
			return directionsCalculated;
		}

		private static String TransportMode(String transportType)
		{
			switch (transportType)
			{
				case "GXM_Driving":
					return "driving";					
				case "GXM_Walking":
					return "walking";
				case "GXM_Transit":
					return "transit";
				case "GXM_Bicycling":
					return "bicycling";
				 default:
					return "";
			}
		}

		public static LocationInfo GetCurrentLocation(int minAccuracy, int timeout, bool includeHAndS, bool ignoreErrors)
		{
			if (Application.GxContext.IsHttpContext)
			{
				string ip = Application.GxContext.Current.GetRemoteAddress();
				using (HttpClient client = new HttpClient())
				{
					using (HttpResponseMessage response = client.GetAsync(new Uri("http://ipinfo.io/" + ip)).Result)
					{
						using (HttpContent content = response.Content)
						{
							string info = content.ReadAsStringAsync().Result;
						}
					}
				}
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
