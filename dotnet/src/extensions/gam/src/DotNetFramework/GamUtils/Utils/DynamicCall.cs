using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;
using GeneXus.Application;
using log4net;
using System.Security;


#if NETCORE
using GxClasses.Helpers;
using GeneXus.Utils;
#endif

namespace GamUtils.Utils
{
	[SecuritySafeCritical]
	public class DynamicCall
	{

		private IGxContext mGxContext;
		private static readonly ILog logger = LogManager.GetLogger(typeof(DynamicCall));

		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/

		[SecuritySafeCritical]
		public DynamicCall(IGxContext context)
		{
			mGxContext = context;
		}

		[SecuritySafeCritical]
		public bool Execute(string assembly, string typeName, bool useContext, string method, string jsonParms, out string jsonOutput)
		{
			logger.Debug("Execute");
			return DoCall(assembly, typeName, useContext, method, false, "", jsonParms, out jsonOutput);
		}

		[SecuritySafeCritical]
		public bool ExecuteEventHandler(string assembly, string typeName, bool useContext, string method, string eventType, string jsonInput, out string jsonOutput)
		{
			logger.Debug("ExecuteEventHandler");
			return DoCall(assembly, typeName, useContext, method, true, eventType, jsonInput, out jsonOutput);
		}


		/********EXTERNAL OBJECT PUBLIC METHODS  - BEGIN ********/


		private bool DoCall(string assembly, string typeName, bool useContext, string method, bool isEventHandler, string input1, string input2, out string jsonOutput)
		{
			logger.Debug("DoCall");
			try
			{
				jsonOutput = "{}";
				Assembly assm;
				if (Path.IsPathRooted(assembly))
					assm = Assembly.LoadFrom(assembly);
				else
					assm = Assembly.LoadFrom(Path.Combine(GetStartupDirectory(), assembly));

				if (assm != null)
				{
					Type type = assm.GetType(typeName);
					if (type != null)
					{
						object instance = null;
						if (useContext && (mGxContext != null))
						{
							ConstructorInfo constructorWithContext = type.GetConstructor(new Type[] { typeof(IGxContext) });
							if (constructorWithContext != null)
							{
								instance = constructorWithContext.Invoke(new object[] { mGxContext });
							}
						}
						if (instance == null)
							instance = assm.CreateInstance(typeName);

						if (instance != null)
						{
							MethodInfo methodInfo;
							object[] parameters;

							if (isEventHandler)
							{
								methodInfo = instance.GetType().GetMethod(method, new Type[] { input1.GetType(), input2.GetType(), jsonOutput.GetType().MakeByRefType() });
								parameters = new object[] { input1, input2, jsonOutput };
							}
							else
							{
								methodInfo = instance.GetType().GetMethod(method, new Type[] { input2.GetType(), jsonOutput.GetType().MakeByRefType() });
								parameters = new object[] { input2, jsonOutput };
							}

							if (methodInfo != null)
							{
								object result = methodInfo.Invoke(instance, parameters);
								if (methodInfo.ReturnType == typeof(void) && (parameters.Length > 0))
									jsonOutput = (string)parameters[parameters.Length - 1];
								else
									jsonOutput = result.ToString();

								return true;
							}
							else
							{
								logger.Error("error: " + method + "(String, out String)" + " in " + assembly + " not found");
								jsonOutput = "{\"error\":\"" + method + "(String, out String)" + " in " + assembly + " not found" + "\"}";
								return false;
							}
						}
						else
						{
							logger.Error("error: " + "constructor for " + typeName + " in " + assembly + " not found");
							jsonOutput = "{\"error\":\"" + "constructor for " + typeName + " in " + assembly + " not found" + "\"}";
							return false;
						}
					}
					else
					{
						logger.Error("error: " + typeName + " in " + assembly + " not found");
						jsonOutput = "{\"error\":\"" + typeName + " in " + assembly + " not found" + "\"}";
						return false;
					}
				}
				else
				{
					logger.Error("error: " + assembly + " not found");
					jsonOutput = "{\"error\":\"" + assembly + " not found" + "\"}";
					return false;
				}
			}
			catch (Exception ex)
			{
				StringBuilder str = new StringBuilder();
				str.Append(ex.Message);
				while (ex.InnerException != null)
				{
					str.Append(ex.InnerException.Message);
					ex = ex.InnerException;
				}
				logger.Error("error: " + Enquote(str.ToString()));
				jsonOutput = "{\"error\":" + Enquote(str.ToString()) + "}";
				return false;
			}
		}

		private static string GetStartupDirectory()
		{
			logger.Debug("GetStartupDirectory");
#if NETCORE
			return FileUtil.GetStartupDirectory();
#else
			string dir = Path.GetDirectoryName(Assembly.GetCallingAssembly().GetName().CodeBase); ;
			if (dir.StartsWith("file:\\"))
				dir = dir.Substring(6);
			return dir;
#endif
		}


		private static string Enquote(string s)
		{
			logger.Debug("Enquote)");
			if (s == null || s.Length == 0)
				return "\"\"";

			int length = s.Length;
			StringBuilder sb = new StringBuilder(length + 4);

			sb.Append('"');

			for (int index = 0; index < length; index++)
			{
				char ch = s[index];

				if ((ch == '\\') || (ch == '"') || (ch == '>'))
				{
					sb.Append('\\');
					sb.Append(ch);
				}
				else if (ch == '\b')
					sb.Append("\\b");
				else if (ch == '\t')
					sb.Append("\\t");
				else if (ch == '\n')
					sb.Append("\\n");
				else if (ch == '\f')
					sb.Append("\\f");
				else if (ch == '\r')
					sb.Append("\\r");
				else
				{
					if (ch < ' ')
					{
						//t = "000" + Integer.toHexString(c);
						//string tmp = new string(ch, 1);
						string t = "000" + ((int)ch).ToString(CultureInfo.InvariantCulture);
						sb.Append("\\u" + t.Substring(t.Length - 4));
					}
					else
					{
						sb.Append(ch);
					}
				}
			}

			sb.Append('"');
			return sb.ToString();
		}
	}
}
