namespace GeneXus.Metadata
{
	using System;
	using System.Reflection;
	using System.Collections;
	using System.IO;
	using GeneXus.Utils;
	using GeneXus.Configuration;
	using System.Runtime.Serialization;
#if NETCORE
	using GxClasses.Helpers;
	using System.Runtime.Loader;
	using System.Linq;
#endif
	using GeneXus.Application;
	using System.Collections.Concurrent;

	public class ClassLoader
	{
		private static readonly IGXLogger log = GXLoggerFactory.GetLogger<ClassLoader>();

#if NETCORE
		private const string GXWEBPROCEDURE_TYPE = "GeneXus.Procedure.GXWebProcedure";
#endif
		static public Object GetInstance(string assemblyName, string fullClassName, Object[] constructorArgs)
		{

			GXLogging.Debug(log, "GetInstance '" + fullClassName + "', assembly '" + assemblyName + "'");
			try{


#if NETCORE
				Assembly assem = AssemblyLoader.LoadAssembly(new AssemblyName(assemblyName));
				Type classType = assem.GetType(fullClassName, true, true);
#else
				Assembly assem = Assembly.Load(new AssemblyName(assemblyName));
				Type classType = assem.GetType(fullClassName, false, true);
#endif
				return Activator.CreateInstance(classType, constructorArgs);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error in invoke GetInstance", e);
#if !NETCORE
				if (!GxContext.isReorganization)
				{
					GXUtil.WinMessage(e.Message, string.Empty);
				}
#endif
				throw GxClassLoaderException.ProcessException(e);
			}
		}
		static public void Execute(object instance, string mthd, Object[] args)
		{
			GXLogging.Debug(log, "Execute instance '" + instance + "', mthd '" + mthd + "'");
			MethodInfo mth;
			try
			{
				int count = args != null ? args.Length : 0;
				Type[] types = new Type[count];
				for (int i = 0; i < count; i++)
					types[i] = args[i].GetType();
				mth = instance.GetType().GetMethod(mthd, types);
				mth.Invoke(instance, args);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error in invoke ", e);
				throw GxClassLoaderException.ProcessException(e);
			}
		}
        static ConcurrentDictionary<string, Type> loadedAssemblies = new ConcurrentDictionary<string, Type>();
		static public Type FindType(string defaultAssemblyName, string clss, Assembly defaultAssembly, bool ignoreCase = false, bool ignoreError=false)
		{
			return FindType(defaultAssemblyName, string.Empty, clss, defaultAssembly, ignoreCase, ignoreError);
		}
		//ns = kb Namespace
		//clssWithoutNamespace = fullname genexus object (includes module), p.e. genexus.sd.synchronization.offlineeventreplicator
		static public Type FindType(string defaultAssemblyName, string ns, string clssWithoutNamespace, Assembly defaultAssembly, bool ignoreCase = false, bool ignoreError = false)
		{

			string clss = string.IsNullOrEmpty(ns) ? clssWithoutNamespace : string.Format("{0}.{1}", ns, clssWithoutNamespace);
			Type objType = null;
			string appNS;
			if (!loadedAssemblies.TryGetValue(clss, out objType))
			{
				if (defaultAssembly != null)
                {
					try
					{
						objType = ignoreCase? defaultAssembly.GetType(clss, false, ignoreCase): defaultAssembly.GetType(clss, false);
					}
					catch
					{
						GXLogging.Warn(log, "Failed to load type: " + clss + ", assembly: " + defaultAssembly.FullName);
					}
                }
                
				try
				{
					AssemblyName defaultAssemblyNameObj = new AssemblyName(defaultAssemblyName);
					if (objType == null)
					{
#if NETCORE
						Assembly assem = AssemblyLoader.LoadAssembly(defaultAssemblyNameObj);
						objType = ignoreCase ? assem.GetType(clss, false): assem.GetType(clss, false, ignoreCase);
#else
						objType = Assembly.Load(defaultAssemblyNameObj).GetType(clss, false);
#endif
					}
				}
				catch(FileNotFoundException)
				{
					GXLogging.Warn(log, "Assembly: ", defaultAssemblyName, "not found");
				}
				catch(Exception ex)
				{
					GXLogging.Warn(log, "Failed to load type: " + clss + ", assembly: " + defaultAssemblyName, ex);
				}
				try
				{
					if (objType == null)
						
						if (Assembly.GetEntryAssembly() != null)
							objType = ignoreCase ? Assembly.GetEntryAssembly().GetType(clss, false, ignoreCase) : Assembly.GetEntryAssembly().GetType(clss, false);
				}
				catch
				{
					GXLogging.Warn(log, "Failed to load type: " + clss + " from entryAssembly");
				}
				try
				{
					if (objType == null)
						
						objType = ignoreCase ? Assembly.GetCallingAssembly().GetType(clss, false, ignoreCase) : Assembly.GetCallingAssembly().GetType(clss, false);
				}
				catch
				{
					GXLogging.Warn(log, "Failed to load type: " + clss + " from callingAssembly");
				}

				if (objType == null && !string.IsNullOrEmpty(ns) && Config.GetValueOf("AppMainNamespace", out appNS))
				{
					if (ns != appNS) 
					{
						return FindType(defaultAssemblyName, appNS, clssWithoutNamespace, defaultAssembly, ignoreCase, ignoreError);
					}
				}
				if (objType == null)
                {
					GXLogging.Warn(log, "Find Instance in CurrentDomain");
                    
                    foreach (Assembly asby in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        objType = ignoreCase ? asby.GetType(clss, false, ignoreCase) : asby.GetType(clss, false);
                        if (objType != null)
                            break;
                    }
                }
				if (objType == null && !string.IsNullOrEmpty(ns))
				{
					if (defaultAssemblyName.Contains(","))
					{
						
						string[] parts = defaultAssemblyName.Split(',');
						if (parts.Length == 2)
						{
							defaultAssemblyName = parts[1];
							clss = parts[0];
						}
						return FindType(defaultAssemblyName, string.Empty, clss, defaultAssembly, ignoreCase, ignoreError);
					}
				}

				if (objType == null)
                {
					GXLogging.Debug(log, "Find class in assemblies in directory " + FileUtil.GetStartupDirectory());
                    
                    ArrayList files = new ArrayList();
                    files.AddRange(Directory.GetFiles(FileUtil.GetStartupDirectory(), "*.dll"));
#if !NETCORE
					files.AddRange(Directory.GetFiles(FileUtil.GetStartupDirectory(), "*.exe"));
#endif
                    foreach (string file in files)
                    {
						GXLogging.Debug(log, "Find class " + clss + ", assembly: " + file);
                        try
                        {
#if !NETCORE
                            objType = Assembly.LoadFrom(file).GetType(clss, false);
#else
							Assembly assem = AssemblyLoader.LoadAssembly(new AssemblyName(Path.GetFileNameWithoutExtension(file)));
							objType = ignoreCase ? assem.GetType(clss, false, ignoreCase) : assem.GetType(clss, false);
#endif
							if (objType != null)
                                break;
                        }
                        catch (BadImageFormatException)
                        {
                            // It is not an .Net assembly
                        }
                    }

                }

				loadedAssemblies[clss] = objType;
            }
			if (objType == null)
			{
				if (ignoreError)
					GXLogging.Warn(log, "Failed to load type: " + clss + " from currentdomain");
				else
				{
					GXLogging.Error(log, "Failed to load type: " + clss + " from currentdomain");
					throw new GxClassLoaderException("Failed to load type: " + clss);
				}
			}
			return objType;

		}
		//[Obsolete("FindInstance with 4 arguments is deprecated", false)]
		static public object FindInstance(string defaultAssemblyName, string clss, Object[] constructorArgs, Assembly defaultAssembly)
		{
			return FindInstance(defaultAssemblyName, string.Empty, clss, constructorArgs, defaultAssembly);
		}
		static public object FindInstance(string defaultAssemblyName, string nspace, string clss, Object[] constructorArgs, Assembly defaultAssembly, bool ignoreCase=false)
		{
			Type objType = FindType( defaultAssemblyName, nspace, clss, defaultAssembly, ignoreCase);
			GXLogging.Debug(log, "CreateInstance, Args ", ConstructorArgsString(constructorArgs));
			return Activator.CreateInstance(objType, constructorArgs);
		}
		internal static string ConstructorArgsString(Object[] constructorArgs)
		{
			string argsConstr = "";
			for (int i = 0; constructorArgs != null && i < constructorArgs.Length; i++)
			{
				argsConstr += "'" + constructorArgs[i] + "' ";
			}
			return argsConstr;
		}

		static public void ExecuteVoidRef(object o, string mthd, Object[] args)
		{
			try
			{
				MemberInfo[] mth = o.GetType().GetMember(mthd);
				
				MethodInfo mi = null;

				if (mth != null && mth.Length > 0)
				{
					if (mth.Length > 1)
						for (int i = 0; i < mth.Length; i++)
						{
							if (((MethodInfo)mth[i]).ReturnType.ToString() == "System.Void")
							{
								mi = (MethodInfo)mth[i];
								break;
							}
						}
					else
					{
						mi = (MethodInfo)mth[0];
					}

					if (mi != null)
					{
						ParameterModifier[] pms = new ParameterModifier[args.Length];
						ParameterInfo[] pis = mi.GetParameters();
						for (int i = 0; i < pis.Length; i++)
						{
							ParameterInfo pi = pis[i];
							ParameterModifier pm = new ParameterModifier(3);
							pm[0] = ((pi.Attributes & ParameterAttributes.In) != ParameterAttributes.None);
							pm[1] = ((pi.Attributes & ParameterAttributes.Out) != ParameterAttributes.None);
							pm[2] = pi.ParameterType.IsByRef;
							pms[i] = pm;
						}
						try
						{
							o.GetType().InvokeMember(mthd, BindingFlags.InvokeMethod, null, o, args, pms, null, null);

						}catch(MissingMethodException)
						{
							throw new GxClassLoaderException("Method " + mi.DeclaringType.FullName + "." + mi.Name + " for " + args.Length + " parameters ("+ String.Join(",", args) + ") not found");
						}
					}
					else
					{
						throw new GxClassLoaderException("Method " + mthd + " with " + args.Length + " parameters not found");
					}
				}
				else
					throw new GxClassLoaderException("Method " + mthd + " not found");
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error in invoke ", e);
				throw GxClassLoaderException.ProcessException(e);
			}
		}
		static public void ExecuteRef(object o, string mthd, Object[] args)
		{
			GXLogging.Debug(log, "ExecuteRef '" + "class '" + o + "' mthd '" + mthd + "'");
			try
			{
				MethodInfo mth = o.GetType().GetMethod(mthd);
				mth.Invoke(o, args);
			}
			catch (Exception e)
			{
				GXLogging.Error(log, "Error in ExecuteRef ", e);
				throw GxClassLoaderException.ProcessException(e);
			}
		}
        static public object ExecuteUdp(Assembly callingAssembly, string clss, Object[] constructorArgs, string mthd, Object[] args)
        {
            Object o = FindInstance("", clss, constructorArgs, callingAssembly);

            MethodInfo mth = o.GetType().GetMethod(mthd);
            return mth.Invoke(o, args);
        }

		static public void Execute(string assmbly, string ns, string clss, Object[] constructorArgs, string mthd, Object[] args)
		{
			GXLogging.Debug(log, "Execute assembly '" + assmbly + "', class '" + clss + "' mthd '" + mthd + "'");
			Object o = FindInstance(assmbly, ns, clss, constructorArgs, Assembly.GetCallingAssembly());
			ExecuteVoidRef(o, mthd, args);
		}
		[Obsolete("Execute with 5 arguments is deprecated", false)]
		static public void Execute(string assmbly, string clss, Object[] constructorArgs, string mthd, Object[] args)
		{
			Execute(assmbly, string.Empty, clss, constructorArgs, mthd, args);
		}
		
		static public void WebExecute(string assmbly, string nspace, string className, Object[] constructorArgs, string mthd, Object[] args)
		{
			GXLogging.Debug(log, "Execute assembly '" + assmbly + "', namespace '" + nspace + "'  class '" + className + "' mthd '" + mthd + "'");

			Type objType = FindType(assmbly, nspace, className, Assembly.GetCallingAssembly());

#if NETCORE
			if (typeof(IHttpHandler).IsAssignableFrom(objType) && (objType.BaseType.FullName!=GXWEBPROCEDURE_TYPE))
#else
			if (objType.IsSubclassOf(typeof(GeneXus.Http.GXHttpHandler)) && !objType.IsSubclassOf(typeof(Procedure.GXWebProcedure)))
#endif
			throw new GxClassLoaderException(": (" + assmbly + "). Loading of web object not allowed in WebExecute.");
			Object o = Activator.CreateInstance(objType, constructorArgs);

			ExecuteVoidRef(o, mthd, args);
		}
		[Obsolete("WebExecute with 5 arguments is deprecated", false)]
		static public void WebExecute(string assmbly, string className, Object[] constructorArgs, string mthd, Object[] args)
		{
			WebExecute(assmbly, string.Empty, className, constructorArgs, mthd, args);
		}
		public static object CreateInstance(Assembly assemblyInstance, string typeName, object[] parms)
		{
			try
			{
				Type t = assemblyInstance.GetType(typeName);
				object instance = Activator.CreateInstance(t, parms);
				return instance;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log, "Exception in CreateInstance " + ex.GetType());
				while (ex.InnerException!=null)
					ex = ex.InnerException;
				throw ex;
			}
		}
		public static object CreateInstance(Assembly assemblyInstance, string typeName)
		{
			return CreateInstance(assemblyInstance, typeName,  null);
		}

		public static object GetPropValue(object instante, string property)
		{
			PropertyInfo prop = instante.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
			return (prop != null) ? prop.GetValue(instante, null) : null;
		}
		public static object GetStaticPropValue(Assembly assemblyInstance, string typeName, string property)
		{
			PropertyInfo prop = assemblyInstance.GetType(typeName).GetProperty(property, BindingFlags.Public | BindingFlags.Static);
			return (prop != null) ? prop.GetValue(null, null) : null;
		}
		public static object GetPropValue(object instante, string property, Type type)
		{
			PropertyInfo prop = instante.GetType().GetProperty(property, type);
			return (prop != null) ? prop.GetValue(instante, null) : null;
		}
		public static object GetConstantValue(Assembly ass, string className, string field)
		{
			Type prn1 = ass.GetType(className);
			FieldInfo f = prn1.GetField(field);
			return (f!=null) ? f.GetValue(null): null;
		}

		public static void SetPropValue(object instante, string property, object value)
		{
			PropertyInfo prop = instante.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
			if (prop!=null)
				prop.SetValue(instante, value, null);
		}

		public static void SetPropValue(object instante, string property, Type returnType, object value)
		{
			try
			{
				PropertyInfo prop = instante.GetType().GetProperty(property, returnType);
				if (prop != null)
					prop.SetValue(instante, value, null);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw ex;
			}
		}

		public static object Invoke(object instance, string methodName, object[] parms)
		{
			try
			{
				return instance.GetType().InvokeMember(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, instance, parms);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw ex;
			}
		}

		public static object InvokeStatic(Assembly ass, string className, string methodName, object[] parms)
		{
			try {
				return ass.GetType(className, false, true).InvokeMember(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, parms);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw ex;
			}
		}

		public static object GetEnumValue(Assembly ass, string enumClassName, string enumField)
		{
			Type e = ass.GetType(enumClassName);
			FieldInfo fi = e.GetField(enumField);
			return (fi != null) ? fi.GetValue(null) : null;
		}

	}
	[Serializable()]
	public class GxClassLoaderException : Exception
	{
		private string _errorInfo;
#if !NETCORE
		public GxClassLoaderException(SerializationInfo info, StreamingContext ctx)
			: base(info, ctx)
		{
			
		}
#endif
		public GxClassLoaderException(string msg): base("GeneXus Class Loader Exception "+msg)
		{
			_errorInfo = msg;
		}
		public GxClassLoaderException(string msg, Exception innerException): base("GeneXus Class Loader Exception "+msg, innerException)
		{
			_errorInfo = msg;
		}

		public static Exception ProcessException(Exception e)
		{
			while (e.InnerException != null)
				e = e.InnerException;
			return e;
		}

		public override string Message
		{
			get
			{
				return _errorInfo;
			}
		}
		public override string ToString()
		{
			return "GeneXus Class Loader Exception "+ _errorInfo;
		}
	}
}