using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using GeneXus.Utils;
using System.Linq;
#if !NETCORE
using Jayrock.Json;
#endif

using Type = System.Type;

namespace GeneXus.Application
{
	public class ReflectionHelper
	{
		public static void CallBCMethod(object instance, String methodName, IList<string> inParametersValues)
		{
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			object[] parametersForInvocation = ProcessParametersForInvoke(methodInfo, inParametersValues);
			methodInfo.Invoke(instance, parametersForInvocation);
		}
		public static bool HasMethod(object instance, String methodName, IGxContext context = null)
		{
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName);
			if (methodInfo != null)
				return true;
			else
				return false;
		}
		public static bool SearchMethod(MemberInfo info, object obj)
		{
			return info.Name.StartsWith(obj.ToString(), StringComparison.OrdinalIgnoreCase);
		}
		public static Dictionary<string, object> CallMethodPattern(object instance, String methodPattern, IDictionary<string, object> parameters, IGxContext context = null)
		{
			Type instanceType = instance.GetType();
			MemberFilter memberFilter = new MemberFilter(SearchMethod);
			MemberInfo memberInfo = instanceType.FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase, memberFilter, methodPattern)[0];
			MethodInfo methodInfo = instanceType.GetMethod(memberInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
			return CallMethodImpl(instance, methodInfo, parameters, context);
		}

		public static Dictionary<string, object> CallMethod(object instance, String methodName, IDictionary<string, object> parameters, IGxContext context = null)
		{
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
			return CallMethodImpl(instance, methodInfo, parameters, context);
		}
		public static IList<object> CallMethod(object instanceOrType, String methodName, IList<object> parameters, bool isStatic = false, IGxContext context = null)
		{
			MethodInfo methodInfo;
			object instance = null;
			if (isStatic)
			{
				methodInfo = ((Type)instanceOrType).GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
			}
			else
			{

				methodInfo = instanceOrType.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
				instance = instanceOrType;
			}
			return CallMethodImpl(instance, methodInfo, parameters, context);
		}
		static Dictionary<string, object> CallMethodImpl(object instance, MethodInfo methodInfo, IDictionary<string, object> parameters, IGxContext context)
		{
			object[] parametersForInvocation = ProcessParametersForInvoke(methodInfo, parameters, context);
			object returnParm = methodInfo.Invoke(instance, parametersForInvocation);
			return ProcessParametersAfterInvoke(methodInfo, parametersForInvocation, returnParm);
		}
		static IList<object> CallMethodImpl(object instance, MethodInfo methodInfo, IList<object> parameters, IGxContext context)
		{
			object[] parametersForInvocation = ProcessParametersForInvoke(methodInfo, parameters, context);
			object returnParm = methodInfo.Invoke(instance, parametersForInvocation);
			IList<object> parametersAfterInvoke = parametersForInvocation.ToList<object>();
			parametersAfterInvoke.Add(returnParm);
			return parametersAfterInvoke;
		}
		public static bool MethodHasInputParameters(object instance, String methodName)
		{
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName);
			var methodParameters = methodInfo.GetParameters();
			foreach (var methodParameter in methodParameters)
			{
				if (!methodParameter.IsOut)
				{
					return true;
				}
			}
			return false;
		}

		public static Dictionary<string, object> GetWrappedParameter(object instance, String methodName, Dictionary<string, object> bodyParameters)
		{			
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName);
			var methodParameters = methodInfo.GetParameters();
			List<ParameterInfo> inputParameters = new List<ParameterInfo>();
			foreach (var methodParameter in methodParameters)
			{
				if (!methodParameter.IsOut)
				{
					inputParameters.Add(methodParameter);
				}
			}
			if (inputParameters.Count == 1)
			{
				ParameterInfo pInfo = inputParameters[0];				
				if (pInfo.ParameterType.IsSubclassOf(typeof(GxUserType)) && bodyParameters.Count > 1)
				{
					string gxParameterName = GxParameterName(pInfo.Name).ToLower();
					Dictionary<string, object> parameters = new Dictionary<string, object>();
					JObject jparms = new JObject(bodyParameters);
					parameters.Add(gxParameterName, jparms);
					return parameters;

				}
				if (typeof(IGxCollection).IsAssignableFrom(pInfo.ParameterType) &&  bodyParameters.Count == 1 && bodyParameters.ContainsKey(string.Empty) && bodyParameters[string.Empty] is JArray)
				{
					string gxParameterName = GxParameterName(pInfo.Name).ToLower();
					Dictionary<string, object> parameters = new Dictionary<string, object>();
					parameters.Add(gxParameterName, bodyParameters[string.Empty]);
					return parameters;
				}
				
			}
			return bodyParameters;
		}

		private static object ConvertSingleJsonItem(object value, Type newType, IGxContext context)
		{
			if (value != null && value.GetType() == newType)
			{
				return value;
			}
			else if (typeof(IGxJSONAble).IsAssignableFrom(newType))
			{
				object TObject;
				if (typeof(GxSilentTrnSdt).IsAssignableFrom(newType) && context != null)
				{
					TObject = Activator.CreateInstance(newType, new object[] { context });
				}
				else
				{
					TObject = Activator.CreateInstance(newType);
				}
				((IGxJSONAble)TObject).FromJSONObject(value);
				return TObject;
			}
			else if (newType == typeof(DateTime))
			{
				string jsonDate = value as string;
				return DateTimeUtil.CToDT2(jsonDate, context);
			}
			else if (newType == typeof(Geospatial))
			{
				return new Geospatial(value);
			}
			else if (typeof(IConvertible).IsAssignableFrom(newType))
			{
				return Convert.ChangeType(value, newType, CultureInfo.InvariantCulture);
			}
			else if (newType == typeof(Guid) && Guid.TryParse(value.ToString(), out Guid guidResult))
			{
				return guidResult;
			}
			else
			{
				return value;
			}
		}

		private static object ConvertStringToNewNonNullableType(object value, Type newType, IGxContext context = null)
		{

			if (newType.IsArray)
			{
				// For comma separated list
				Type singleItemType = newType.GetElementType();

				var elements = new ArrayList();
				foreach (string element in value.ToString().Split(','))
				{
					object convertedSingleItem = ConvertSingleJsonItem(element, singleItemType, context);
					elements.Add(convertedSingleItem);
				}
				return elements.ToArray(singleItemType);
			}
			return ConvertSingleJsonItem(value, newType, context);
		}

		internal static object ConvertStringToNewType(object value, Type newType, IGxContext context = null)
		{
			// If it's not a nullable type, just pass through the parameters to Convert.ChangeType
			if (newType.GetTypeInfo().IsGenericType && newType.GetGenericTypeDefinition() != null && newType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				if (value == null)
				{
					return null;
				}
				return ConvertStringToNewNonNullableType(value, new NullableConverter(newType).UnderlyingType, context);
			}
			return ConvertStringToNewNonNullableType(value, newType, context);
		}

		public static Dictionary<string, string> ParametersFormat(object instance, string methodName)
		{
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName);

			Dictionary<string, string> formatList = new Dictionary<string, string>();
			var methodParameters = methodInfo.GetParameters();
			foreach (var methodParameter in methodParameters)
			{
				string gxParameterName = GxParameterName(methodParameter.Name);
				if (IsByRefParameter(methodParameter))
				{
					string fmt = "";
					object[] attributes = methodParameter.GetCustomAttributes(true);
					GxJsonFormatAttribute attFmt = (GxJsonFormatAttribute)attributes.Where(a => a.GetType() == typeof(GxJsonFormatAttribute)).FirstOrDefault();
					if (attFmt != null)
						fmt = attFmt.JsonFormat;
					formatList.Add(gxParameterName, fmt);
				}
			}
			return formatList;
		}


		private static Dictionary<string, object> ProcessParametersAfterInvoke(MethodInfo methodInfo, object[] parametersForInvocation, object returnParm)
		{
			Dictionary<string, object> outputParameters = new Dictionary<string, object>();
			var methodParameters = methodInfo.GetParameters();
			int idx = 0;
			foreach (var methodParameter in methodParameters)
			{
				string gxParameterName = GxParameterName(methodParameter);
				if (IsByRefParameter(methodParameter))
				{
					outputParameters.Add(gxParameterName, parametersForInvocation[idx]);
				}
				idx++;
			}
			if (returnParm != null)
				outputParameters.Add(string.Empty, returnParm);
			return outputParameters;
		}


		internal static object[] ProcessParametersForInvoke(MethodInfo methodInfo, IDictionary<string, object> parameters, IGxContext context = null)
		{
			var methodParameters = methodInfo.GetParameters();
			object[] parametersForInvocation = new object[methodParameters.Length];
			int idx = 0;
			foreach (var methodParameter in methodParameters)
			{
				object value;

				string gxParameterName = GxParameterName(methodParameter.Name).ToLower();
				Type parmType = methodParameter.ParameterType;
				string jsontypename = "";
				object[] attributes = parmType.GetCustomAttributes(true);
				GxJsonName jsonName = (GxJsonName)attributes.Where(a => a.GetType() == typeof(GxJsonName)).FirstOrDefault();
				if (jsonName != null)
					jsontypename = jsonName.Name.ToLower();
				else
					jsontypename = gxParameterName;
				if (IsByRefParameter(methodParameter))
				{
					parmType = parmType.GetElementType();
				}
				if (parameters != null && parameters.TryGetValue(jsontypename, out value))
				{
					if (value == null || JSONHelper.IsJsonNull(value))
					{
						parametersForInvocation[idx] = null;
					}
					else
					{
						object convertedValue = ConvertStringToNewType(value, parmType, context);
						parametersForInvocation[idx] = convertedValue;
					}
				}
				else
				{
					object defaultValue = CreateInstance(parmType);
					parametersForInvocation[idx] = defaultValue;
				}
				idx++;
			}
			return parametersForInvocation;
		}

		internal static object[] ProcessParametersForInvoke(MethodInfo methodInfo, IList<object> parameters, IGxContext context = null)
		{
			var methodParameters = methodInfo.GetParameters();
			object[] parametersForInvocation = new object[methodParameters.Length];
			int idx = 0;
			foreach (var methodParameter in methodParameters)
			{
				Type parmType = methodParameter.ParameterType;
				if (IsByRefParameter(methodParameter))
				{
					parmType = parmType.GetElementType();
				}
				object value = parameters.ElementAt(idx);
				if (!value.GetType().Equals(parmType))
				{
					//To avoid convertion from string type
					if (value.GetType() != typeof(string))
					{
						object convertedValue = ConvertStringToNewType(value, parmType, context);
						parametersForInvocation[idx] = convertedValue;
					}
					else
					{
						throw new ArgumentException("Type does not match", methodParameter.Name);
					}
				}
				else
				{
					parametersForInvocation[idx] = value;
				}
				idx++;
			}
			return parametersForInvocation;
		}
		private static Regex attVar = new Regex(@"^AV?\d{1,}", RegexOptions.Compiled);

		private static string GxParameterName(ParameterInfo methodParameter)
		{
			int idx = methodParameter.Name.IndexOf('_');
			if (idx >= 0)
			{
				string mparm = methodParameter.Name.Substring(idx + 1);
				string PName = methodParameter.ParameterType.FullName;
				// The root element name should be in the metadata of the SDT 
				if (mparm.StartsWith("Gx") && mparm.EndsWith("rootcol") && PName.Contains("_"))
				{
					int Pos = PName.IndexOf("Sdt") + 3;
					mparm = PName.Substring(Pos, PName.IndexOf("_") - Pos);
				}
				return mparm;
			}
			else
			{
				return attVar.Replace(methodParameter.Name, string.Empty);
			}
		}

		private static string GxParameterName(string methodParameterName)
		{
			int idx = methodParameterName.IndexOf('_');
			if (idx >= 0)
			{
				return  methodParameterName.Substring(idx + 1);				
			}
			else
			{
				return attVar.Replace(methodParameterName, string.Empty);
			}
		}

		private static object[] ProcessParametersForInvoke(MethodInfo methodInfo, IList<string> parametersValues)
		{
			var methodParameters = methodInfo.GetParameters();
			object[] parametersForInvocation = new object[methodParameters.Length];
			int idx = 0;
			string pattern = @"(AV\d+)(.*)";
			string replacement = "$2";
			Regex rgx = new Regex(pattern);

			foreach (var methodParameter in methodParameters)
			{
				rgx.Replace(methodParameter.Name, replacement);
				Type parmType = methodParameter.ParameterType;
				object convertedValue = ConvertStringToNewType(parametersValues[idx], parmType);
				parametersForInvocation[idx] = convertedValue;
				idx++;
			}
			return parametersForInvocation;
		}
		private static bool IsByRefParameter(ParameterInfo methodParameter)
		{
			return methodParameter.IsOut || methodParameter.ParameterType.IsByRef;
		}
		private static object CreateInstance(Type targetType)
		{
			if (Type.GetTypeCode(targetType) == TypeCode.String)
			{
				return string.Empty;
			}
			else
				return Activator.CreateInstance(targetType);
		}

	}
}
