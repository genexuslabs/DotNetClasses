using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using GeneXus.Utils;
using Jayrock.Json;
using Type = System.Type;

namespace GeneXus.Application
{
	public class ReflectionHelper
    {
		const string ISO_8601_TIME_SEPARATOR= "T";
		const string ISO_8601_TIME_SEPARATOR_1 = ":";
		public static void CallBCMethod(object instance, String methodName, IList<string> inParametersValues)
		{
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			object[] parametersForInvocation = ProcessParametersForInvoke(methodInfo, inParametersValues);
			methodInfo.Invoke(instance, parametersForInvocation);
		}
		public static Dictionary<string, object> CallMethod(object instance, String methodName, IDictionary<string, object> parameters, IGxContext context=null)
		{
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName);
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
				((IGxJSONAble)TObject).FromJSONObject((IJsonFormattable)value);
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
				return Convert.ChangeType(value, newType);
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

		private static Dictionary<string, object> ProcessParametersAfterInvoke(MethodInfo methodInfo, object[] parametersForInvocation, object returnParm)
		{
			Dictionary<string, object> outputParameters = new Dictionary<string, object>();
			var methodParameters = methodInfo.GetParameters();
			int idx = 0;
			foreach (var methodParameter in methodParameters)
			{
				var gxParameterName = methodParameter.Name.Substring(methodParameter.Name.IndexOf('_') + 1);
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
				
				var gxParameterName = methodParameter.Name.Substring(methodParameter.Name.IndexOf('_') + 1).ToLower();
				Type parmType = methodParameter.ParameterType;
				if (IsByRefParameter(methodParameter))
				{
					parmType = parmType.GetElementType();
				}
				if (parameters!=null && parameters.TryGetValue(gxParameterName, out value))
				{
					var convertedValue = ConvertStringToNewType(value, parmType, context);
					parametersForInvocation[idx] = convertedValue;
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
