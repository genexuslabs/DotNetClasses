using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Collections;
using Jayrock.Json;
using log4net;
using System.Reflection;
using GeneXus.Metadata;
using System.Data.SqlTypes;
using System.IO;
#if NETCORE
using GxClasses.Helpers;
#endif
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

	class SQLGeographyWrapper
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.SQLGeographyWrapper));
		static Assembly _geoAssembly;
		public const string SqlGeographyClass = "Microsoft.SqlServer.Types.SqlGeography";
		public const string SqlGeometryClass = "Microsoft.SqlServer.Types.SqlGeometry";

		public const string SqlGeographyAssemby = "Microsoft.SqlServer.Types";
		static object _nullSQLGeography;
		private SQLGeographyWrapper() { }
		internal static Assembly GeoAssembly
		{
			get
			{
				try
				{
					if (_geoAssembly == null)
					{
						GXLogging.Debug(log, "Loading ", SqlGeographyAssemby, " from GAC");
#if NETCORE
						var asl = new AssemblyLoader(FileUtil.GetStartupDirectory());
						_geoAssembly = asl.LoadFromAssemblyPath(Path.Combine(FileUtil.GetStartupDirectory(), SqlGeographyAssemby + ".dll"));
#else
						_geoAssembly = Assembly.LoadWithPartialName(SqlGeographyAssemby);
#endif

						GXLogging.Debug(log, SqlGeographyAssemby, " Loaded from GAC: " + _geoAssembly.FullName);
					}

				}
				catch (Exception ex)
				{
					GXLogging.Error(log, "Error loading " + SqlGeographyAssemby + " from GAC", ex);
				}
				if (_geoAssembly == null)
				{
					_geoAssembly = Assembly.Load(SqlGeographyAssemby);
				}
				return _geoAssembly;
			}
		}
		internal static object NullSQLGeography
		{
			get {
				if (_nullSQLGeography == null)
					_nullSQLGeography = ClassLoader.GetStaticPropValue(GeoAssembly, SqlGeographyClass, "Null");
				return _nullSQLGeography;
			}
		}
		internal static bool IsNull(object instance)
		{
			return (bool)ClassLoader.GetPropValue(instance, "IsNull");
		}

		internal static object STGeometryType(object instance)
		{
			return ClassLoader.Invoke(instance, "STGeometryType", null);
		}
		internal static double Long(object instance)
		{
			return ((SqlDouble)ClassLoader.GetPropValue(instance, "Long")).Value;
		}
		internal static double Lat(object instance)
		{
			return ((SqlDouble)ClassLoader.GetPropValue(instance, "Lat")).Value;
		}
		internal static object CreateInstance()
		{
			return ClassLoader.CreateInstance(GeoAssembly, SqlGeographyClass, null);
		}

        internal static bool IsValid(object instance)
        {
            try
            {			
                return ((SqlBoolean)ClassLoader.Invoke(instance, "STIsValid", null)).Value;
            }
            catch (MissingMethodException ex) {
                GXLogging.Debug(log, "IsValid not Found: " + ex.Message);
                return true;
            }
        }

        internal static object MakeValid(object instance)
        {
            return (ClassLoader.Invoke(instance, "MakeValid", null));
        }

        internal static object Parse(String geoText)
		{
			SqlString sqlGeoText = new SqlString(geoText);
			return ClassLoader.InvokeStatic(GeoAssembly, SqlGeographyClass, "Parse", new object[] { sqlGeoText });
		}

		internal static object GeometryParse(String geoText)
		{
			return ClassLoader.InvokeStatic(GeoAssembly, SqlGeometryClass, "Parse", new object[] { geoText });
		}

		internal static object Deserialize(SqlBytes bytes)
		{
			return ClassLoader.InvokeStatic(GeoAssembly, SqlGeographyClass, "Deserialize", new object[] { bytes });
		}

		internal static object STGeomFromText(string geoText, int sRID)
		{
			object cwPolygon = SQLGeographyWrapper.GeometryParse(geoText);
			object stStartPoint = ClassLoader.Invoke(cwPolygon, "STStartPoint", null);
			object validPolygon = ClassLoader.Invoke(cwPolygon, "MakeValid", null);
			cwPolygon = ClassLoader.Invoke(validPolygon, "STUnion", new object[] { stStartPoint });
			object stAsText = ClassLoader.Invoke(cwPolygon, "STAsText", null);
			return ClassLoader.InvokeStatic(SQLGeographyWrapper.GeoAssembly, SQLGeographyWrapper.SqlGeographyClass, "STGeomFromText", new object[] { stAsText, sRID });

		}
	}

	[KnownType(typeof(System.Double[]))]
    [KnownType(typeof(System.Collections.ArrayList))]
	[DataContract]
	public class Geospatial : IGeographicNative
	{
		static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Utils.Geospatial));

		internal const string EMPTY_GEOMETRY = "GEOMETRYCOLLECTION EMPTY";
		const string EMPTY_GEOGRAPHY = "GEOGRAPHY EMPTY";
		const string EMPTY_POINT = "POINT EMPTY";
		const string EMPTY_LINE = "LINESTRING EMPTY";
		const string EMPTY_POLY = "POLYGON EMPTY";

		internal const string ALT_EMPTY_POINT = "POINT(0 0)";
		internal const string ALT_EMPTY_LINE = "LINESTRING( 0 0,0 1)";
		internal const string ALT_EMPTY_POLY = "POLYGON((0 0, 0 1, 1 0,0 0))";

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
			this.setGXGeoType(SQLGeographyWrapper.STGeometryType(_innerValue).ToString());
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
                this.setGXGeoType(SQLGeographyWrapper.STGeometryType(_innerValue).ToString());
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
			this.InnerValue = SQLGeographyWrapper.CreateInstance();
		}

        public int srid;
		public int Srid {
            get
            {
				if (this.InnerValue != null)
					if (_innerValue == SQLGeographyWrapper.NullSQLGeography)
						return 0;
					else
	                    return ((SqlInt32)ClassLoader.GetPropValue(_innerValue, "STSrid")).Value;
                else
                    return srid;
            }

            set
            {
                if (this.InnerValue != null)
                   ClassLoader.SetPropValue(_innerValue, "STSrid", (SqlInt32)Srid);
                else
                   srid=value;
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
				if (_innerValue == null)
					return 0;
				else
					return SQLGeographyWrapper.Long(_innerValue);
			}

		}

		public double Latitude
		{
			get
			{
				if (_innerValue == null)
					return 0;
				else
					return SQLGeographyWrapper.Lat(_innerValue);
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
                    String[] coordinates  = locationString.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    wktBuffer = "POINT(" + coordinates[1].ToString()  + " " + coordinates[0].ToString() + ")";

                }
                else
                { 
                    setGXGeoType(featuretype);              
                    String sep =  "";
                    String sep1 = "";
                    String sep2 = "";
                    switch (GeographicType)
                    {
                        case GeoGraphicTypeValue.Point:
                            wktBuffer = "POINT";
                            wktBuffer += "(" + JSONPointToWKT(coords)  + ")";
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
									String jpS = JSONPointToWKT(jp);
									wktBuffer += sep + " " + jpS;
									sep = ",";
								}
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
                                wktBuffer += sep +  " " + JSONPointToWKT(jp);
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
									//wktBuffer += sep + firstPoint;
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
			return  (	g.InnerValue == null ||
                        g.InnerValue == SQLGeographyWrapper.NullSQLGeography ||
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
            if ( !String.IsNullOrEmpty(geoText))
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
					geo.InnerValue = ClassLoader.InvokeStatic(SQLGeographyWrapper.GeoAssembly, SQLGeographyWrapper.SqlGeographyClass, "Point", new object[] { geo.Point.Latitude, geo.Point.Longitude, geo.Srid });


				}
				catch (Exception)
                {
                    // Can't convert to geography set as null.
                    geo.InnerValue = SQLGeographyWrapper.NullSQLGeography;
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
				|| s.Equals(ALT_EMPTY_POLY) || s.Equals(EMPTY_GEOMETRY)  || s.Equals(EMPTY_GEOGRAPHY) ||
				s.Equals(EMPTY_POINT) || s.Equals(EMPTY_LINE) || s.Equals(EMPTY_POLY) )
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
				geoText = EMPTY_GEOMETRY ;
            }
            else
            {
                geoText = s.Trim();
            }
            
            
			try
			{			
				// Sql Server Text
				_innerValue = SQLGeographyWrapper.Parse(geoText);

				if (_innerValue != null && (!SQLGeographyWrapper.IsNull(_innerValue)) && (!SQLGeographyWrapper.IsValid(_innerValue)))
				{
					_innerValue = SQLGeographyWrapper.MakeValid(_innerValue);

					this.srid = ((SqlInt32)ClassLoader.GetPropValue(_innerValue, "STSrid")).Value;

					this.setGXGeoType(SQLGeographyWrapper.STGeometryType(_innerValue).ToString());
					if (GeographicType == GeoGraphicTypeValue.Point)
					{
						this.Point.Longitude = SQLGeographyWrapper.Long(_innerValue);
						this.Point.Latitude = SQLGeographyWrapper.Lat(_innerValue);
					}
				}
				else
				{					
						setNullGeography();
				}
				
			}
			catch (ArgumentException ex)
			{
                if (ex.ToString().Contains("24144")) // makevalid didn´t work
                {
                    _innerValue = null;
                }
				if (ex.ToString().Contains("24200")) // Error code for invalid Geo.
				{
					_innerValue = SQLGeographyWrapper.STGeomFromText(geoText, srid);
				}
				else
				{
					setNullGeography();
				}
			}
			catch (FormatException ex)
			{
				String exText = ex.ToString();
				if (!String.IsNullOrEmpty(exText) && (ex.ToString().Contains("24114") || ex.ToString().Contains("24141")))
				{
					if (GeographicType == GeoGraphicTypeValue.Point && !String.IsNullOrEmpty(geoText))
					{
                        int commas =  geoText.Split(',').Length - 1;
                        if ( (commas == 1) && !geoText.Contains(" "))
					    {
							// has . as decimal separator and "," as value sep
							geoText = geoText.Replace(',', ' ');
						}
						else
						{
                            if (geoText.Contains(",") && geoText.Contains(" ")) {
                                geoText = geoText.Replace(',', '.');
                            }
						}
						try
						{
							String[] coord = geoText.Split(new char[] { ' ' }, 2);
							this.Point.Longitude = Convert.ToDouble(coord[1].Trim(), CultureInfo.InvariantCulture.NumberFormat);
							this.Point.Latitude = Convert.ToDouble(coord[0].Trim(), CultureInfo.InvariantCulture.NumberFormat);
							this.srid = 4326;
							// Latitude and Longitud parameters are reversed in the 'Point' constructor:
							_innerValue = ClassLoader.InvokeStatic(SQLGeographyWrapper.GeoAssembly, SQLGeographyWrapper.SqlGeographyClass, "Point", new object[] { this.Point.Latitude, this.Point.Longitude, this.Srid });
						}
						catch (Exception)
						{
							setNullGeography();
						}
					}
					else
					{
						setNullGeography();
					}
				}
				else
				{
					setNullGeography();
				}
			}
		}

		override public String ToString()
        {
			return this.ToStringSQL("");
        }

		void setNullGeography()
		{
			// Cannot parse value
			_innerValue = SQLGeographyWrapper.NullSQLGeography;
			this.geoText = "";
			this.Point.Longitude = 0;
			this.Point.Latitude = 0;
		}

        public String ToStringESQL()
        {
            String wktText =  this.ToStringSQL(EMPTY_GEOMETRY);
            if (!wktText.Equals(EMPTY_GEOMETRY)) {
                wktText = "SRID=" + this.Srid.ToString() + ";" + wktText;
            }
            return wktText;
        }

        public String ToStringSQL(String defaultValue)
        {
            if (this.InnerValue != null && this.InnerValue != SQLGeographyWrapper.NullSQLGeography && (!IsGeoNull(this.InnerValue.ToString())) )
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
		
			if (_innerValue != null && x.InnerValue != null  &&
				! _innerValue.ToString().Equals(EMPTY_GEOGRAPHY) &&
				! x.InnerValue.ToString().Equals(EMPTY_GEOGRAPHY))
			{
				try
				{
					return ((SqlDouble)(ClassLoader.Invoke(_innerValue, "STDistance", new object[] { x.InnerValue }))).Value;
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
			if (_innerValue != null &&
				! _innerValue.ToString().Equals(EMPTY_GEOGRAPHY) &&
				GeographicType == GeoGraphicTypeValue.Polygon)
			{
				try { 
					return ((SqlDouble)(ClassLoader.Invoke(_innerValue, "STArea", Array.Empty<object>()))).Value;
				}
				catch (Exception ex) {
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
			if (_innerValue != null && x.InnerValue != null  &&
				!_innerValue.ToString().Equals(EMPTY_GEOGRAPHY) &&
				! x.InnerValue.ToString().Equals(EMPTY_GEOGRAPHY))				
			{
				try
				{
					return ((SqlBoolean)(ClassLoader.Invoke(_innerValue, "STIntersects", new object[] { x.InnerValue }))).Equals(SqlBoolean.True);
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
				if (_innerValue==null)
					GXLogging.Debug(log, "STIntersect: _innerValue is not valid");
				else
					GXLogging.Debug(log, "STIntersect: x.InnerValue is not valid");
				return false;
			}
		}
        
        [DataMember(Name ="type", Order = 0)]
        public String SJ_GeoType {
            get
            {
                return SQLGeographyWrapper.STGeometryType(_innerValue).ToString();
            }
            set {
				if (_innerValue == null)
					initInstanceVars();

			}
        }
        
        [DataMember(Name = "coordinates",Order = 1)]
        public ArrayList SJ_GeoSets
        {
            get
            {
				if (_innerValue != null) { 
                String geoType = SQLGeographyWrapper.STGeometryType(_innerValue).ToString();
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
                SqlInt32 points = (SqlInt32)ClassLoader.Invoke(geoInstance, "STNumPoints", null);
                for (int i = 1; i <= points.Value; i++)
                {
                    double[] CurrentPoint = new double[2];
                    object currentPoint = ClassLoader.Invoke(geoInstance, "STPointN", new object[] { i });
                    CurrentPoint[0] = SQLGeographyWrapper.Long(currentPoint);
                    CurrentPoint[1] = SQLGeographyWrapper.Lat(currentPoint);
                    ArrayOfPoints.Add(CurrentPoint);
                }
            }
            else if (geoType.Equals("Point"))
            {
                object currentPoint = ClassLoader.Invoke(geoInstance, "STPointN", new object[] { 1 });
                ArrayOfPoints.Add(SQLGeographyWrapper.Long(currentPoint));
                ArrayOfPoints.Add(SQLGeographyWrapper.Lat(currentPoint));
            }
            else if (geoType.Equals("Polygon"))
            {
                SqlInt32 rings = (SqlInt32)ClassLoader.Invoke(geoInstance, "NumRings", null);
                for (int j = 1; j <= rings.Value; j++)
                {
                    ArrayList RingArray = new ArrayList();
                    object currentRing = ClassLoader.Invoke(geoInstance, "RingN", new object[] { j });
                    SqlInt32 p = (SqlInt32)ClassLoader.Invoke(currentRing, "STNumPoints", null);

                    for (int i = 1; i < p.Value; i++)
                    {

                        double[] CurrentPoint = new double[2];
                        object currentPoint = ClassLoader.Invoke(currentRing, "STPointN", new object[] { i });
                        CurrentPoint[0] = SQLGeographyWrapper.Long(currentPoint);
                        CurrentPoint[1] = SQLGeographyWrapper.Lat(currentPoint);
                        RingArray.Add(CurrentPoint);
                    }
                    ArrayOfPoints.Add(RingArray);
                }
            }
            else if (geoType.Equals("MultiLineString"))
            {
                SqlInt32 geoms = (SqlInt32)ClassLoader.Invoke(geoInstance, "STNumGeometries", null);
                for (int j = 1; j <= geoms.Value; j++)
                {
                    object currentgeom = ClassLoader.Invoke(geoInstance, "STGeometryN", new object[] { j });
                    ArrayOfPoints.Add(geoToArray("LineString", currentgeom));
                }
            }
            else if ( geoType.Equals("MultiPolygon"))
            {
                SqlInt32 geoms = (SqlInt32)ClassLoader.Invoke(geoInstance, "STNumGeometries", null);
                for (int j = 1; j <= geoms.Value; j++)
                {
                    object currentgeom = ClassLoader.Invoke(geoInstance, "STGeometryN", new object[] { j });
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
