using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using GeneXus.Utils;
using System.Linq;
using Jayrock.Json;

using Type = System.Type;

namespace GeneXus.Application
{
	public class ReflectionHelper
	{
		const string ISO_8601_TIME_SEPARATOR = "T";
		const string ISO_8601_TIME_SEPARATOR_1 = ":";
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
		static Dictionary<string, object> CallMethodImpl(object instance, MethodInfo methodInfo, IDictionary<string, object> parameters, IGxContext context)
		{
			object[] parametersForInvocation = ProcessParametersForInvoke(methodInfo, parameters, context);
			object returnParm = methodInfo.Invoke(instance, parametersForInvocation);
			return ProcessParametersAfterInvoke(methodInfo, parametersForInvocation, returnParm);
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
				if ( (pInfo.ParameterType.Name.StartsWith("GXBaseCollection") || pInfo.ParameterType.Name.StartsWith("GXSimpleCollection"))
					  &&  bodyParameters.Count == 1 && bodyParameters.ContainsKey(string.Empty) && bodyParameters[string.Empty] is JArray)
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
			if (value!= null && value.GetType() == newType)
			{
				return value;
			}
			else if (typeof(IGxJSONAble).IsAssignableFrom(newType))
			{
				object TObject;
				if (typeof(GxSilentTrnSdt).IsAssignableFrom(newType) && context!=null)
				{
					TObject = Activator.CreateInstance(newType, new object[] { context});
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
				if (!string.IsNullOrEmpty(jsonDate) && (jsonDate.Contains(ISO_8601_TIME_SEPARATOR) || jsonDate.Contains(ISO_8601_TIME_SEPARATOR_1)))
				{
					return DateTimeUtil.CToT2(jsonDate);
				}
				else
				{
					return DateTimeUtil.CToD2(jsonDate);
				}
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

		private static object ConvertStringToNewNonNullableType(object value, Type newType, IGxContext context=null)
		{
			
			if (newType.IsArray)
			{
				// For comma separated list
				Type singleItemType = newType.GetElementType();

				var elements = new ArrayList();
				foreach (var element in value.ToString().Split(','))
				{
					var convertedSingleItem = ConvertSingleJsonItem(element, singleItemType, context);
					elements.Add(convertedSingleItem);
				}
				return elements.ToArray(singleItemType);
			}
			return ConvertSingleJsonItem(value, newType, context);
		}

		internal static object ConvertStringToNewType(object value, Type newType, IGxContext context=null)
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
				var gxParameterName = GxParameterName(methodParameter.Name);
				if (IsByRefParameter(methodParameter))
				{
					string fmt = "";
					var attributes = methodParameter.GetCustomAttributes(true);
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
				var gxParameterName = GxParameterName(methodParameter.Name);
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


		internal static object[] ProcessParametersForInvoke(MethodInfo methodInfo, IDictionary<string, object> parameters, IGxContext context=null)
		{
			var methodParameters = methodInfo.GetParameters();
			object[] parametersForInvocation = new object[methodParameters.Length];
			var idx = 0;
			foreach (var methodParameter in methodParameters)
			{
				object value;
				
				var gxParameterName = GxParameterName(methodParameter.Name).ToLower();
				Type parmType = methodParameter.ParameterType;
				if (IsByRefParameter(methodParameter))
				{
					parmType = parmType.GetElementType();
				}
				if (parameters!=null && parameters.TryGetValue(gxParameterName, out value))
				{
					if (value == null || JSONHelper.IsJsonNull(value))
					{
						parametersForInvocation[idx] = null;
					}
					else
					{
						var convertedValue = ConvertStringToNewType(value, parmType, context);
						parametersForInvocation[idx] = convertedValue;
					}
				}
				else
				{
					var defaultValue = CreateInstance(parmType);
					parametersForInvocation[idx] = defaultValue;
				}
				idx++;
			}
			return parametersForInvocation;
		}
		private static Regex attVar = new Regex(@"^AV?\d{1,}", RegexOptions.Compiled);
		private static string GxParameterName(string methodParameterName)
		{
			int idx = methodParameterName.IndexOf('_');
			if (idx >= 0)
			{
				return methodParameterName.Substring(idx + 1);
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
			var idx = 0;
			string pattern = @"(AV\d+)(.*)";
			string replacement = "$2";
			Regex rgx = new Regex(pattern);

			foreach (var methodParameter in methodParameters)
			{
				rgx.Replace(methodParameter.Name, replacement);
				Type parmType = methodParameter.ParameterType;
				var convertedValue = ConvertStringToNewType(parametersValues[idx], parmType);
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
