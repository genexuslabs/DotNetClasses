using System;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;

namespace GeneXus.Encryption
{
	/**
	 * This class acts as a central repository for an algorithm specific
	 * properties. It reads an (algorithm).properties file containing algorithm-
	 * specific properties. When using the AES-Kit, this (algorithm).properties
	 * file is located in the (algorithm).jar file produced by the "jarit" batch/
	 * script command.<p>
	 *
	 * <b>Copyright</b> &copy; 1997, 1998
	 * <a href="http://www.systemics.com/">Systemics Ltd</a> on behalf of the
	 * <a href="http://www.systemics.com/docs/cryptix/">Cryptix Development Team</a>.
	 * <br>All rights reserved.<p>
	 *
	 * <b>$Revision: 1.1 $</b>
	 * @author  David Hopwood
	 * @author  Jill Baker
	 * @author  Raif S. Naffah
	 */
	internal class Twofish_Properties // implicit no-argument constructor
	{
		// Constants and variables with relevant static code
		//...........................................................................

		public static bool GLOBAL_DEBUG = false;

		public static String ALGORITHM = "Twofish";
		public static double VERSION = 0.2;
		public static String FULL_NAME = ALGORITHM + " ver. " + VERSION;
		public static String NAME = "Twofish_Properties";

		/** Default properties in case .properties file was not found. */
		private static string[][] DEFAULT_PROPERTIES = new string[][]{
		new string[]{"Trace.Twofish_Algorithm",       "true"},
		new string[]{"Debug.Level.*",             "1"},
		new string[]{"Debug.Level.Twofish_Algorithm", "9"},
	};
		public static ConcurrentDictionary<string, string> properties = InitializeProperties();

		static ConcurrentDictionary<string, string> InitializeProperties()
		{
			ConcurrentDictionary<string, string> props = new ConcurrentDictionary<string, string>();
			if (GLOBAL_DEBUG) Console.WriteLine(">>> " + NAME + ": Looking for " + ALGORITHM + " properties");
			String it = ALGORITHM + ".properties";
			bool ok = false;
			if (!ok)
			{
				if (GLOBAL_DEBUG) Console.WriteLine(">>> " + NAME + ": WARNING: Unable to load \"" + it + "\" from CLASSPATH.");
				if (GLOBAL_DEBUG) Console.WriteLine(">>> " + NAME + ":          Will use default values instead...");
				int n = DEFAULT_PROPERTIES.Length;
				for (int i = 0; i < n; i++)
					props.TryAdd(DEFAULT_PROPERTIES[i][0], DEFAULT_PROPERTIES[i][1]);
				if (GLOBAL_DEBUG) Console.WriteLine(">>> " + NAME + ": Default properties now set...");
			}
			return props;
		}


		// Properties methods (excluding load and save, which are deliberately not
		// supported).
		//...........................................................................

		/** Get the value of a property for this algorithm. */
		public static string getProperty(string key)
		{
			return (string)properties[key];
		}

		/**
		 * Get the value of a property for this algorithm, or return
		 * <i>value</i> if the property was not set.
		 */
		public static string getProperty(string key, string value)
		{
			if (properties.ContainsKey(key)) return (string)properties[key];
			else return value;
		}

		/** List algorithm properties to the PrintStream <i>out</i>. */
		//    public static void list (PrintStream out) {
		//        list(new PrintWriter(out, true));
		//    }

		/** List algorithm properties to the PrintWriter <i>out</i>. */
		public static void list(TextWriter _out)
		{
			_out.WriteLine("#");
			_out.WriteLine("# ----- Begin " + ALGORITHM + " properties -----");
			_out.WriteLine("#");
			String key, _value;
			IEnumerator _enum = properties.Keys.GetEnumerator();
			while (_enum.MoveNext())
			{
				key = (string)_enum.Current;
				_value = getProperty(key);
				_out.WriteLine(key + " = " + _value);
			}
			_out.WriteLine("#");
			_out.WriteLine("# ----- End " + ALGORITHM + " properties -----");
		}

		//    public synchronized void load(InputStream in) throws IOException {}

		public static IEnumerator propertyNames()
		{
			return properties.Keys.GetEnumerator();
		}

		//    public void save (OutputStream os, String comment) {}


		// Developer support: Tracing and debugging enquiry methods (package-private)
		//...........................................................................

		/**
		 * Return true if tracing is requested for a given class.<p>
		 *
		 * User indicates this by setting the tracing <code>bool</code>
		 * property for <i>label</i> in the <code>(algorithm).properties</code>
		 * file. The property's key is "<code>Trace.<i>label</i></code>".<p>
		 *
		 * @param label  The name of a class.
		 * @return True iff a bool true value is set for a property with
		 *      the key <code>Trace.<i>label</i></code>.
		 */
		public static bool isTraceable(String label)
		{
			String s = getProperty("Trace." + label);
			if (s == null)
				return false;
			return label.ToLower().Equals("true");
		}

		/**
		 * Return the debug level for a given class.<p>
		 *
		 * User indicates this by setting the numeric property with key
		 * "<code>Debug.Level.<i>label</i></code>".<p>
		 *
		 * If this property is not set, "<code>Debug.Level.*</code>" is looked up
		 * next. If neither property is set, or if the first property found is
		 * not a valid decimal integer, then this method returns 0.
		 *
		 * @param label  The name of a class.
		 * @return  The required debugging level for the designated class.
		 */
		public static int getLevel(String label)
		{
			String s = getProperty("Debug.Level." + label);
			if (s == null)
			{
				s = getProperty("Debug.Level.*");
				if (s == null)
					return 0;
			}
			return Int32.Parse(s);
		}

		/**
		 * Return the PrintWriter to which tracing and debugging output is to
		 * be sent.<p>
		 *
		 * User indicates this by setting the property with key <code>Output</code>
		 * to the literal <code>out</code> or <code>err</code>.<p>
		 *
		 * By default or if the set value is not allowed, <code>System.err</code>
		 * will be used.
		 */
		public static TextWriter getOutput()
		{
			String name = getProperty("Output");
			if (name != null && name.Equals("out"))
				return Console.Out;
			else
				return Console.Error;
		}
	}
}
