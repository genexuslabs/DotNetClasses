using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Collections;
using Jayrock.Json;
using log4net;
using System.Reflection;
using GeneXus.Metadata;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using GxClasses.Helpers;
using GeographicLib;

namespace GeneXus.Utils
{

	public interface IGeographicNative
	{

		Object InnerValue
		{
			get;
			set;
		}
		int Srid
		{
			get;
			set;
		}
		void FromString(String s);
		String ToStringSQL();
		String ToStringSQL(String defaultString);
	}

	class NTSGeographyWrapper
	{

		private const double EARTH_RADIUS = 6371008.8;
		private const double DEG_TO_RAD = Math.PI / 180;

		internal static object Parse(String geoText)
		{
			WKTReader reader = new WKTReader();
			var geodata = reader.Read(geoText);
			if (geodata.IsValid)
			{
				return geodata;
			}
			else
				return null;
		}

		internal static object GeometryParse(String geoText)
		{
			return NTSGeographyWrapper.Parse(geoText);
		}

		internal static object STGeomFromText(string geoText, int sRID)
		{
			return NTSGeographyWrapper.Parse("SRID=" + sRID.ToString() + ";" + geoText);
		}

		internal static object NullSQLGeography
		{
			get
			{
				return null;
			}
		}

		internal static object STGeometryType(object instance)
		{
			return (instance !=null)?((Geometry)instance).GeometryType: "";
		}
		internal static double Long(object instance)
		{
			if (STGeometryType(instance).Equals("Point"))
			{
				return ((Geometry)instance).InteriorPoint.X;
			}
			return 0;
		}
		internal static double Lat(object instance)
		{
			if (STGeometryType(instance).Equals("Point"))
			{
				return ((Geometry)instance).InteriorPoint.Y;
			}
			return 0;
		}

		internal static void SetSrid(object instance, int srid)
		{
			((Geometry)instance).SRID = srid;
		}
		internal static int Srid(object instance)
		{
			return ((Geometry)instance).SRID;
		}

		internal static bool IsValid(object instance)
		{
			if (instance != null)
				return ((Geometry)instance).IsValid;
			return false;
		}

		internal static double STArea(object instance)
		{
			PolygonResult r = STPolygonResult((Geometry)instance, false);
			return r.Area;
		}

		static PolygonResult STPolygonResult(Geometry instance, bool IsLine)
		{
			Geodesic g = Geodesic.WGS84;
			var line = new GeographicLib.PolygonArea(g, IsLine);
			int points = NTSGeographyWrapper.STNumPoinst(instance);
			for (int i = 1; i < points; i++)
			{
				Coordinate currentPoint = NTSGeographyWrapper.STPoints(instance)[i];
				line.AddPoint(currentPoint.Y, currentPoint.X);
			}
			PolygonResult r = line.Compute();
			return r;
		}

		internal static double STDistance(object instanceA, object instanceB)
		{
			NetTopologySuite.Geometries.Point pointA = null;
			NetTopologySuite.Geometries.Point pointB = null;
			if (!STGeometryType(instanceA).Equals("Point"))
			{
				pointA = ((Geometry)instanceA).InteriorPoint;
			}
			else
			{
				pointA = (Point)instanceA;
			}
			if (!STGeometryType(instanceB).Equals("Point"))
			{
				pointB = ((Geometry)instanceB).InteriorPoint;
			}
			else
			{
				pointB = (Point)instanceB;
			}
			double LatA = pointA.Y;
			double LonA = pointA.X;
			double LatB = pointB.Y;
			double LonB = pointB.X;
			Geodesic g = Geodesic.WGS84;
			var dt = g.Inverse(LatA, LonA, LatB, LonB);
			return dt.s12;
		}

		/*public static double DistanceSLC(double Lat1, double Lon1, double Lat2, double Lon2)
		{
			try
			{
				double radLat1 = Lat1 * DEG_TO_RAD;
				double radLat2 = Lat2 * DEG_TO_RAD;
				double radLon1 = Lon1 * DEG_TO_RAD;
				double radLon2 = Lon2 * DEG_TO_RAD;

				// central angle, aka arc segment angular distance
				double centralAngle = Math.Acos(Math.Sin(radLat1) * Math.Sin(radLat2) +
						Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Cos(radLon2 - radLon1));

				// great-circle (orthodromic) distance on Earth between 2 points
				return EARTH_RADIUS * centralAngle;
			}
			catch {
					throw;
				}
		}*/

		internal static bool STIntersects(object instanceA, object instanceB)
		{
			return ((Geometry)instanceA).Intersects((Geometry)instanceB);
		}

		internal static int STNumPoinst(object instance)
		{
			return ((Geometry)instance).NumPoints;
		}

		internal static Coordinate[] STPoints(object instance)
		{
			return ((Geometry)instance).Coordinates;
		}

		internal static int STNumGeometries(object instance)
		{
			return ((Geometry)instance).NumGeometries;
		}

		internal static object STGeometryN(object instance, int i)
		{
			return ((Geometry)instance).GetGeometryN(i);
		}

		internal static int STNumRings(object instance)
		{
			if (STGeometryType(instance).Equals("Polygon"))
				return ((NetTopologySuite.Geometries.Polygon)instance).NumInteriorRings + 1; // add exterior Ring
			else
				return 0;
		}

		internal static object STRingN(object instance, int i)
		{
			if (STGeometryType(instance).Equals("Polygon"))
			{
				if (i == 0)
					return ((NetTopologySuite.Geometries.Polygon)instance).ExteriorRing;
				else
					return ((NetTopologySuite.Geometries.Polygon)instance).GetInteriorRingN(i-1);
			}
			else
				return null;
		}
	}

	[KnownType(typeof(System.Double[]))]
	[KnownType(typeof(System.Collections.ArrayList))]
	[DataContract]
	public class Geospatial : IGeographicNative
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.Geospatial));

		const string EMPTY_GEOMETRY = "GEOMETRYCOLLECTION EMPTY";
		const string EMPTY_GEOGRAPHY = "GEOGRAPHY EMPTY";
		const string EMPTY_POINT = "POINT EMPTY";
		const string EMPTY_LINE = "LINESTRING EMPTY";
		const string EMPTY_POLY = "POLYGON EMPTY";

		const string ALT_EMPTY_POINT = "POINT(0 0)";
		const string ALT_EMPTY_LINE = "LINESTRING( 0 0,0 1)";
		const string ALT_EMPTY_POLY = "POLYGON((0 0, 0 1, 1 0,0 0))";
		
		public enum GeoGraphicTypeValue { Point, MultiPoint, Line, MultiLine, Polygon, MultiPolygon, Other };

		public static explicit operator Geospatial(String s)
		{
			return new Geospatial(s);
		}

		public static explicit operator String(Geospatial g)
		{
			return g.ToString();
		}

		public Geospatial(String value)
		{
			String s = value.ToString();
			initInstanceVars();
			this.FromString(s);
			// serttype
			this.setGXGeoType(NTSGeographyWrapper.STGeometryType(_innerValue).ToString());
		}
		public Geospatial(Decimal latitude, Decimal longitude)
		{
			initInstanceVars();
			String wktBuffer = "POINT";
			wktBuffer += "(" + longitude.ToString("G17", CultureInfo.InvariantCulture) + " " + latitude.ToString("G17", CultureInfo.InvariantCulture) + ")";
			this.FromString(wktBuffer);
		}

		public Geospatial(Double latitude, Double longitude)
		{
			initInstanceVars();
			String wktBuffer = "POINT";
			wktBuffer += "(" + ((Double)longitude).ToString("G17", CultureInfo.InvariantCulture) + " " + ((Double)latitude).ToString("G17", CultureInfo.InvariantCulture) + ")";
			this.FromString(wktBuffer);
		}

		public Geospatial(String value, String format)
		{
			if (format == "wkt")
			{
				new Geospatial(value);
			}
			else
			{
				initInstanceVars();
				this.FromJSON(value.ToString());
			}
		}


		public Geospatial(object value)
		{
			Geospatial geo = value as Geospatial;
			if (geo != null)
			{
				this.InnerValue = geo.InnerValue;
				// settype
				this.setGXGeoType(NTSGeographyWrapper.STGeometryType(_innerValue).ToString());
				this.srid = geo.srid;
			}
			else
			{
				String s = value.ToString();
				new Geospatial(s);
			}
		}

		public Geospatial()
		{
			initInstanceVars();
		}

		void initInstanceVars()
		{
			this.PointList = new PointT[] { new PointT() };
			this.srid = 4326;
			// set value
			this.InnerValue = NTSGeographyWrapper.NullSQLGeography;
		}

		public int srid;
		public int Srid
		{
			get
			{
				if (this.InnerValue != null)
					if (_innerValue == NTSGeographyWrapper.NullSQLGeography)
						return 0;
					else
						return NTSGeographyWrapper.Srid(_innerValue);
				else
					return srid;
			}

			set
			{
				if (this.InnerValue != null)
					NTSGeographyWrapper.SetSrid(_innerValue, Srid);
				else
					srid = value;
			}
		}

		public GeoGraphicTypeValue GeographicType = GeoGraphicTypeValue.Point;

		public PointT[] PointList;

		private object _innerValue;

		public object InnerValue
		{
			get
			{
				return _innerValue;
			}

			set
			{
				_innerValue = value;
			}

		}

		void setGXGeoType(String GeographicTypeString)
		{
			switch (GeographicTypeString)
			{
				case "Point":
					GeographicType = GeoGraphicTypeValue.Point;
					break;
				case "LineString":
					GeographicType = GeoGraphicTypeValue.Line;
					break;
				case "CircularString":
					GeographicType = GeoGraphicTypeValue.Other;
					break;
				case "CompoundCurve":
					GeographicType = GeoGraphicTypeValue.Other;
					break;
				case "Polygon":
					GeographicType = GeoGraphicTypeValue.Polygon;
					break;
				case "CurvePolygon":
					GeographicType = GeoGraphicTypeValue.Other;
					break;
				case "GeometryCollection":
					GeographicType = GeoGraphicTypeValue.Other;
					break;
				case "MultiPoint":
					GeographicType = GeoGraphicTypeValue.MultiPoint;
					break;
				case "MultiLineString":
					GeographicType = GeoGraphicTypeValue.MultiLine;
					break;
				case "MultiPolygon":
					GeographicType = GeoGraphicTypeValue.MultiPolygon;
					break;
				default:
					GeographicType = GeoGraphicTypeValue.Point;
					break;
			}
		}

		public PointT Point
		{
			get
			{
				return this.PointList[0];
			}
		}


		public double Longitude
		{
			get
			{
				return this.PointList[0].Longitude;
			}
		}

		public double Latitude
		{
			get
			{
				return this.PointList[0].Latitude;
			}
		}

		public String JSONPointToWKT(JArray coords)
		{
			String[] jbuffer = new String[] { "", "" };
			jbuffer[0] = "";
			jbuffer[1] = "";
			if (coords[0].GetType().Equals(typeof(String)))
			{
				jbuffer[0] = (String)coords[0];
				jbuffer[0] = jbuffer[0].Replace(',', '.');
			}
			else if (coords[0].GetType().Equals(typeof(int)) || coords[0].GetType().Equals(typeof(long)))
			{
				jbuffer[0] = ((int)coords[0]).ToString("G17", CultureInfo.InvariantCulture);
			}
			else
				jbuffer[0] = Convert.ToDouble(coords[0]).ToString("G17", CultureInfo.InvariantCulture);

			if (coords[1].GetType().Equals(typeof(String)))
			{
				jbuffer[1] = (String)coords[1];
				jbuffer[1] = jbuffer[1].Replace(',', '.');
			}
			else if (coords[1].GetType().Equals(typeof(int)) || coords[1].GetType().Equals(typeof(long)))
			{
				jbuffer[1] = ((int)coords[1]).ToString("G17", CultureInfo.InvariantCulture);
			}
			else
				jbuffer[1] = Convert.ToDouble(coords[1]).ToString("G17", CultureInfo.InvariantCulture);
			return (jbuffer[0] + " " + jbuffer[1]);
		}

		public void FromJSON(String s)
		{
			JObject geoObject = JSONHelper.ReadJSON<JObject>(s);
			if (geoObject != null)
			{
				JObject geometry = (JObject)geoObject["geometry"];
				String featuretype = "";
				JArray coords;
				String wktBuffer = "";
				if (geometry == null)
				{
					featuretype = (String)geoObject["type"];
					coords = (JArray)geoObject["coordinates"];
				}
				else
				{
					featuretype = (String)geometry["type"];
					coords = (JArray)geometry["coordinates"];
				}
				if (featuretype == null)
				{
					String locationString = (String)geoObject["Location"];
					String[] coordinates = locationString.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);
					wktBuffer = "POINT(" + coordinates[1].ToString() + " " + coordinates[0].ToString() + ")";
				}
				else
				{
					setGXGeoType(featuretype);
					String sep = "";
					String sep1 = "";
					String sep2 = "";
					switch (GeographicType)
					{
						case GeoGraphicTypeValue.Point:
							wktBuffer = "POINT";
							wktBuffer += "(" + JSONPointToWKT(coords) + ")";
							break;
						case GeoGraphicTypeValue.Line:
							wktBuffer = "LINESTRING";
							wktBuffer += "(";
							sep = "";
							foreach (JArray jp in coords)
							{
								wktBuffer += sep + " " + JSONPointToWKT(jp);
								sep = ",";
							}
							wktBuffer += ")";
							break;
						case GeoGraphicTypeValue.Polygon:
							wktBuffer = "POLYGON";
							wktBuffer += "(";
							sep1 = "";
							foreach (JArray jl in coords)
							{
								wktBuffer += sep1 + "(";
								sep = "";
								String firstPoint = JSONPointToWKT(jl.GetArray(0));
								foreach (JArray jp in jl)
								{
									wktBuffer += sep + " " + JSONPointToWKT(jp);
									sep = ",";
								}
								wktBuffer += sep + firstPoint;
								wktBuffer += ")";
								sep1 = ",";

							}
							wktBuffer += ")";
							break;
						case GeoGraphicTypeValue.MultiPoint:
							wktBuffer = "MULTIPOINT";
							wktBuffer += "(";
							sep = "";
							foreach (JArray jp in coords)
							{
								wktBuffer += sep + " " + JSONPointToWKT(jp);
								sep = ",";
							}
							wktBuffer += ")";
							break;
						case GeoGraphicTypeValue.MultiLine:
							wktBuffer = "MULTILINESTRING";
							wktBuffer += "(";
							sep1 = "";
							foreach (JArray jl in coords)
							{
								wktBuffer += sep1 + "(";
								sep = "";
								foreach (JArray jp in jl)
								{
									wktBuffer += sep + "" + JSONPointToWKT(jp);
									sep = ",";
								}
								wktBuffer += ")";
								sep1 = ",";
							}
							wktBuffer += ")";
							break;
						case GeoGraphicTypeValue.MultiPolygon:
							wktBuffer = "MULTIPOLYGON";
							wktBuffer += "(";
							sep2 = "";
							foreach (JArray jm in coords)
							{
								wktBuffer += sep2 + "(";
								sep1 = "";
								foreach (JArray jl in jm)
								{
									wktBuffer += sep1 + "(";
									sep = "";
									String firstPoint = JSONPointToWKT(jl.GetArray(0));
									foreach (JArray jp in jl)
									{
										wktBuffer += sep + "" + JSONPointToWKT(jp);
										sep = ",";
									}
									wktBuffer += sep + firstPoint;
									wktBuffer += ")";
									sep1 = ",";
								}
								wktBuffer += ")";
								sep2 = ",";
							}
							wktBuffer += ")";
							break;
						default:
							wktBuffer = "";
							break;

					}
				}
				if (wktBuffer.Length > 0)
				{
					this.FromString(wktBuffer);
				}
			}
		}

		public static Boolean IsNullOrEmpty(Geospatial g)
		{
			return (g.InnerValue == null ||
						g.InnerValue.ToString().Equals(EMPTY_POINT) ||
						g.InnerValue.ToString().Equals(EMPTY_LINE) ||
						g.InnerValue.ToString().Equals(EMPTY_POLY) ||
						g.InnerValue.ToString().Equals(EMPTY_GEOGRAPHY) ||
						g.InnerValue.ToString().Equals(EMPTY_GEOMETRY)
					);
		}


		public static Geospatial FromGXLocation(String geoText)
		{
			Geospatial geo = new Geospatial();
			if (!String.IsNullOrEmpty(geoText))
			{
				if (geoText.Contains("."))
				{
					// has . as decimal separator and "," as value sep
					geoText = geoText.Replace(',', ' ');
				}
				else
				{
					// has comma as DS and space as value sep
					geoText = geoText.Replace(',', '.');
				}
				try
				{
					String[] coord = geoText.Split(new char[] { ' ' }, 2);

					geo.Point.Longitude = Convert.ToDouble(coord[1].Trim(), CultureInfo.InvariantCulture.NumberFormat);
					geo.Point.Latitude = Convert.ToDouble(coord[0].Trim(), CultureInfo.InvariantCulture.NumberFormat);
					geo.srid = 4326;
					// Latitude and Longitud parameters are reversed in the 'Point' constructor:
					// construct a point object
					String wkt = $"POINT({geo.Point.Longitude.ToString("F6")} {geo.Point.Latitude.ToString("F6")})";
					geo.InnerValue = NTSGeographyWrapper.STGeomFromText(wkt, geo.Srid);

				}
				catch (Exception)
				{
					// Can't convert to geography set as null.
					// set innerval
					geo.InnerValue = NTSGeographyWrapper.NullSQLGeography;
					geo.geoText = "";
					geo.Point.Longitude = 0;
					geo.Point.Latitude = 0;
				}

			}
			return geo;
		}

		private bool IsGeoNull(String s)
		{
			if (String.IsNullOrEmpty(s) || s.Equals(ALT_EMPTY_POINT) || s.Equals(ALT_EMPTY_LINE)
				|| s.Equals(ALT_EMPTY_POLY) || s.Equals(EMPTY_GEOMETRY) || s.Equals(EMPTY_GEOGRAPHY) ||
				s.Equals(EMPTY_POINT) || s.Equals(EMPTY_LINE) || s.Equals(EMPTY_POLY))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void FromString(String s)
		{
			if (IsGeoNull(s))
			{
				geoText = EMPTY_GEOMETRY;
			}
			else
			{
				geoText = s.Trim();
			}
			try
			{
				// Parse
				_innerValue = NTSGeographyWrapper.Parse(geoText);
				// SRID, Type & Points X, Y
				if ((!NTSGeographyWrapper.IsValid(_innerValue)) && _innerValue != null)
				{
					//_innerValue = NTSGeographyWrapper.MakeValid(_innerValue);
				}

				this.srid = NTSGeographyWrapper.Srid(_innerValue);

				this.setGXGeoType(NTSGeographyWrapper.STGeometryType(_innerValue).ToString());
				if (GeographicType == GeoGraphicTypeValue.Point)
				{
					this.Point.Longitude = NTSGeographyWrapper.Long(_innerValue);
					this.Point.Latitude = NTSGeographyWrapper.Lat(_innerValue);
				}
			}
			catch (Exception ex)
			{
				if (!String.IsNullOrEmpty(ex.ToString()) && ex.HResult == -2146232832 && ex.Message.Contains("Unknown Type"))
				{ 
					if (GeographicType == GeoGraphicTypeValue.Point && !String.IsNullOrEmpty(geoText))
					{
						_innerValue = Geospatial.FromGXLocation(geoText);
					}
					else
					{
						// Cannot parse value
						_innerValue = NTSGeographyWrapper.NullSQLGeography;
						this.geoText = "";
						this.Point.Longitude = 0;
						this.Point.Latitude = 0;
					}
				}
				else
				{
					// Cannot parse value
					_innerValue = NTSGeographyWrapper.NullSQLGeography;
					this.geoText = "";
					this.Point.Longitude = 0;
					this.Point.Latitude = 0;
				}
			}
		}

		override public String ToString()
		{
			return this.ToStringSQL("");
		}

		public String ToStringESQL()
		{
			String wktText = this.ToStringSQL(EMPTY_GEOMETRY);
			if (!wktText.Equals(EMPTY_GEOMETRY))
			{
				wktText = "SRID=" + this.Srid.ToString() + ";" + wktText;
			}
			return wktText;
		}

		public String ToStringSQL(String defaultValue)
		{
			// serialize to wkt ?
			if (this.InnerValue != null && this.InnerValue != NTSGeographyWrapper.NullSQLGeography && (!IsGeoNull(this.InnerValue.ToString())))
			{
				return this.InnerValue.ToString();
			}
			else
			{
				return defaultValue;
			}
		}

		public String ToStringSQL()
		{
			return this.ToStringSQL(EMPTY_GEOGRAPHY);
		}

		public String ToGeoJSON()
		{
			return JSONHelper.Serialize<GeneXus.Utils.Geospatial>(this);

		}
		public void FromGeoJSON(String s)
		{
			FromJSON(s);
		}

		public static int IsEqual(Geospatial geoA, Geospatial geoB)
		{
			return String.Equals(geoA.ToString(), geoB.ToString()) ? 0 : 1;
		}

		String geoText = "";

		public double STDistance(Geospatial x)
		{

			if (_innerValue != null && x.InnerValue != null &&
				!_innerValue.ToString().Equals(EMPTY_GEOGRAPHY) &&
				!x.InnerValue.ToString().Equals(EMPTY_GEOGRAPHY))
			{
				try
				{
					return NTSGeographyWrapper.STDistance(_innerValue, x.InnerValue);
				}
				catch (Exception ex)
				{
					GXLogging.Debug(log, "Error calling Distance() exception:");
					GXLogging.Debug(log, ex.ToString());
					return 0;
				}
			}
			else
			{
				if (_innerValue == null)
					GXLogging.Debug(log, "STDistance: _innerValue is not valid");
				else
					GXLogging.Debug(log, "STDistance: x.InnerValue is not valid");

				return 0;
			}
		}

		public double STArea()
		{

			// HARD calculo area
			if (_innerValue != null &&
				!_innerValue.ToString().Equals(EMPTY_GEOGRAPHY) &&
				GeographicType == GeoGraphicTypeValue.Polygon)
			{
				try
				{
					return NTSGeographyWrapper.STArea(_innerValue);
				}
				catch (Exception ex)
				{
					GXLogging.Debug(log, "Error calling Area() exception:");
					GXLogging.Debug(log, ex.ToString());
					return 0;
				}
			}
			else
			{
				GXLogging.Debug(log, "STArea: _innerValue is not valid");
				return 0;
			}
		}

		public Boolean STIntersect(Geospatial x)
		{
			if (_innerValue != null && x.InnerValue != null &&
				!_innerValue.ToString().Equals(EMPTY_GEOGRAPHY) &&
				!x.InnerValue.ToString().Equals(EMPTY_GEOGRAPHY))
			{
				try
				{
					return NTSGeographyWrapper.STIntersects(_innerValue, x.InnerValue);
				}
				catch (Exception ex)
				{
					GXLogging.Debug(log, "Error calling Intersect() exception:");
					GXLogging.Debug(log, ex.ToString());
					return false;
				}
			}
			else
			{
				if (_innerValue == null)
					GXLogging.Debug(log, "STIntersect: _innerValue is not valid");
				else
					GXLogging.Debug(log, "STIntersect: x.InnerValue is not valid");
				return false;
			}
		}

		[DataMember(Name = "type", Order = 0)]
		public String SJ_GeoType
		{
			get
			{
				// return GeographicType
				/*
				  Point, LineString, CircularString, CompoundCurve, Polygon, CurvePolygon,
				  GeometryCollection, Multi	Point, MultiLineString, MultiPolygon, and FullGlobe.
				 */
				return NTSGeographyWrapper.STGeometryType(_innerValue).ToString();
			}
			set
			{
				if (_innerValue == null)
					initInstanceVars();

			}
		}

		[DataMember(Name = "coordinates", Order = 1)]
		public ArrayList SJ_GeoSets
		{
			get
			{
				if (_innerValue != null)
				{
					// get type
					String geoType = NTSGeographyWrapper.STGeometryType(_innerValue).ToString();
					return geoToArray(geoType, _innerValue);
				}
				else
				{
					return new ArrayList();
				}
			}
			set
			{

			}
		}

		ArrayList geoToArray(String geoType, Object geoInstance)
		{

			ArrayList ArrayOfPoints = new ArrayList();
			if (geoType.Equals("MultiPoint") || geoType.Equals("LineString"))
			{
				int points = NTSGeographyWrapper.STNumPoinst(geoInstance);
				for (int i = 1; i < points; i++)
				{
					double[] CurrentPoint = new double[2];
					Coordinate currentPoint = NTSGeographyWrapper.STPoints(geoInstance)[i];
					// lat y long
					CurrentPoint[0] = currentPoint.X;
					CurrentPoint[1] = currentPoint.Y;
					ArrayOfPoints.Add(CurrentPoint);
				}
			}
			else if (geoType.Equals("Point"))
			{
				ArrayOfPoints.Add(NTSGeographyWrapper.Long(geoInstance));
				ArrayOfPoints.Add(NTSGeographyWrapper.Lat(geoInstance));
			}
			else if (geoType.Equals("Polygon"))
			{
				int rings = NTSGeographyWrapper.STNumRings(geoInstance);
				for (int j = 0; j < rings; j++)
				{
					ArrayList RingArray = new ArrayList();
					object currentRing = NTSGeographyWrapper.STRingN(geoInstance, j);
					int p = NTSGeographyWrapper.STNumPoinst(currentRing);

					for (int i = 0; i < p; i++)
					{
						double[] CurrentPoint = new double[2];
						Coordinate currentPoint = NTSGeographyWrapper.STPoints(currentRing)[i];
						CurrentPoint[0] = currentPoint.X;
						CurrentPoint[1] = currentPoint.Y;
						RingArray.Add(CurrentPoint);
					}
					ArrayOfPoints.Add(RingArray);
				}
			}
			else if (geoType.Equals("MultiLineString"))
			{
				int geoms = NTSGeographyWrapper.STNumGeometries(geoInstance);
				for (int j = 0; j < geoms; j++)
				{
					object currentgeom = NTSGeographyWrapper.STGeometryN(geoInstance, j);
					ArrayOfPoints.Add(geoToArray("LineString", currentgeom));
				}
			}
			else if (geoType.Equals("MultiPolygon"))
			{
				int geoms = NTSGeographyWrapper.STNumGeometries(geoInstance);
				for (int j = 1; j < geoms; j++)
				{
					object currentgeom = NTSGeographyWrapper.STGeometryN(geoInstance, j);
					ArrayOfPoints.Add(geoToArray("Polygon", currentgeom));
				}
			}
			return ArrayOfPoints;
		}

	}

	public class PointT
	{
		public double Latitude = 0;
		public double Longitude = 0;
	}

	public class Line
	{
		public PointT[] Points;
	}

	public class Polygon
	{
		public Line[] Points;
	}


}
