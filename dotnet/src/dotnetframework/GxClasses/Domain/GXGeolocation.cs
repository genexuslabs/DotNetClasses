using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Jayrock.Json;
using log4net;
using GeneXus.Utils;
using System.Globalization;
using GeneXus;
using GeneXus.Configuration;

namespace GX
{
    public class GXGeolocation
    {
		private const String MAPS_URI = "https://maps.google.com/maps/api/";
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GX.GXGeolocation));

        private static double GetComponent(String geolocation, int item)
        {
            if (!string.IsNullOrEmpty(geolocation))
            {
                string[] st = geolocation.Split(',');
                if (st.Length == 2 && item < 2)
                {

                    return Convert.ToDouble(st[item], CultureInfo.InvariantCulture);
                }
            }
            return 0;
        }
        public static double GetLatitude(String geolocation)
        {
            return GXGeolocation.GetComponent(geolocation, 0);
        }

        public static double GetLongitude(String geolocation)
        {
            return GXGeolocation.GetComponent(geolocation, 1);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static int GetDistance(String location1, String location2)
        {
            // Haversine formula:
            // a = sin²(delta_lat/2) + cos(lat1).cos(lat2).sin²(delta_long/2)
            // c = 2.atan2(sqrt(a), sqrt(1-a))
            // d = R.c
            //   where R is earth’s radius (mean radius = 6,371km);
            // note that angles need to be in radians to pass to trig functions!

            double lat1, lon1, lat2, lon2, d_lat, d_lon, a1, a2, a3, a, c, distance;

            lat1 = GXGeolocation.GetLatitude(location1);
            lon1 = GXGeolocation.GetLongitude(location1);
            lat2 = GXGeolocation.GetLatitude(location2);
            lon2 = GXGeolocation.GetLongitude(location2);

            d_lat = GXGeolocation.DegreesToRadians(lat2 - lat1);
            d_lon = GXGeolocation.DegreesToRadians(lon2 - lon1);

            lat1 = GXGeolocation.DegreesToRadians(lat1);
            lat2 = GXGeolocation.DegreesToRadians(lat2);

            a1 = Math.Pow(Math.Sin(d_lat / 2), 2);
            a2 = Math.Pow(Math.Sin(d_lon / 2), 2);
            a3 = Math.Cos(lat1) * Math.Cos(lat2);
            a = a1 + a2 * a3;

            c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            distance = 6371 * c * 1000; 

            return (int)distance;
        }
#if !NETCORE
		private static String GetContentFromURL(String urlString)
        {
            HttpWebResponse resp = null;
            String result = null;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(urlString);
                req.Method = "GET";
                resp = (HttpWebResponse)req.GetResponse();

                Stream rStream = resp.GetResponseStream();
                using (StreamReader readStream = new StreamReader(rStream, Encoding.UTF8))
                {
                    result = readStream.ReadToEnd();
                }
                rStream.Close();
            }
            catch (WebException ex)
            {
                GXLogging.Error(log, "getContentFromURL error url:" + urlString, ex);
            }
            finally
            {
                
                if (resp != null) 
                    resp.Close();
            }

            return result;
        }
#else
		private static String GetContentFromURL(String urlString)
		{
			HttpWebResponse resp = null;
			String result = null;

			try
			{
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(urlString);
				req.Method = "GET";
				using (resp = (HttpWebResponse)req.GetResponseAsync().Result)
				{

					using (Stream rStream = resp.GetResponseStream())
					{
						using (StreamReader readStream = new StreamReader(rStream, Encoding.UTF8))
						{
							result = readStream.ReadToEnd();
						}
					}
				}
			}
			catch (WebException ex)
			{
				GXLogging.Error(log, "getContentFromURL error url:" + urlString, ex);
			}
			finally
			{
				
				if (resp != null)
					resp.Dispose();
			}

			return result;
		}
#endif
			public static List<string> GetAddress(String location)
        {
            String urlString = MAPS_URI + "geocode/json?latlng=" + location + "&sensor=false";
			String ApiKey = "";
			if (Config.GetValueOf("GoogleApiKey", out ApiKey))
			{
				urlString += "&key=" + ApiKey;
			}
			String response = GXGeolocation.GetContentFromURL(urlString);

            List<string> result = new List<string>();

            try
            {
                if (!string.IsNullOrEmpty(response))
                {
                    StringReader sr = new StringReader(response);
                    JsonTextReader tr = new JsonTextReader(sr);
                    JObject json = (JObject)(tr.DeserializeNext());
                    if (json.Contains("results"))
                    {
                        JArray results = (JArray)json["results"];
                        for (int i = 0; i < results.Length; i++)
                        {
                            JObject jo = (JObject)results[i];
                            if (jo.Contains("formatted_address"))
                            {
                                result.Add((string)jo["formatted_address"]);
                            }
                        }
                    }
                }
            }
            catch (JsonException ex) 
            {
                GXLogging.Error(log, "getAddress error json:" + response, ex);
            }

            return result;
        }


		public static List<string> GetLocation(String address)
		{
			List<Geospatial> locations = GetLocationGeography(address);
			List<string> result = new List<String>();
			foreach (Geospatial location in locations) {
				String geoloc = Convert.ToString(location.Latitude, CultureInfo.InvariantCulture) + "," + Convert.ToString(location.Longitude, CultureInfo.InvariantCulture);
				result.Add(geoloc);
			}
			return result;
		}

		public static List<Geospatial> GetLocationGeography(String address)
        {
            String urlString = MAPS_URI + "geocode/json?address=" + GXUtil.UrlEncode(address) + "&sensor=false";
			String ApiKey = "";
			if (Config.GetValueOf("GoogleApiKey", out ApiKey))
			{
				urlString += "&key=" + ApiKey;
			}

			String response = GXGeolocation.GetContentFromURL(urlString);

            List<Geospatial> result = new List<Geospatial>();

            try
            {
                if (!string.IsNullOrEmpty(response))
                {
                    StringReader sr = new StringReader(response);
                    JsonTextReader tr = new JsonTextReader(sr);
                    JObject json = (JObject)(tr.DeserializeNext());
                    if (json.Contains("results"))
                    {
                        JArray results = (JArray)json["results"];
                        for (int i = 0; i < results.Length; i++)
                        {
                            JObject jo = (JObject)results[i];
                            if (jo.Contains("geometry"))
                            {
                                JObject geometry = (JObject)jo["geometry"];
                                if (geometry.Contains("location"))
                                {
                                    JObject location = (JObject)geometry["location"];
                                    if (location!=null && (location.Contains("lat")) && (location.Contains("lng")))
                                    {										
										Geospatial point = new Geospatial(Convert.ToDecimal(location["lat"]), Convert.ToDecimal(location["lng"]));
                                        result.Add(point);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (JsonException ex) 
            {
                GXLogging.Error(log, "getLocation error json:" + response, ex);
            }

            return result;
        }

		public static short AuthorizationStatus{get;set;}
    }
}
