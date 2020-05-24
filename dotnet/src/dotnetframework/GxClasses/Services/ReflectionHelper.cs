using GeneXus.Configuration;
using GeneXus.Metadata;
using GeneXus.Utils;
using Jayrock.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

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
		public static Dictionary<string, object> CallMethod(object instance, String methodName, IDictionary<string, object> parameters)
		{
			MethodInfo methodInfo = instance.GetType().GetMethod(methodName);
			List<int> outputParameterIndex;
			object[] parametersForInvocation = ProcessParametersForInvoke(methodInfo, parameters, out outputParameterIndex);
			methodInfo.Invoke(instance, parametersForInvocation);

			return ProcessParametersAfterInvoke(methodInfo, parametersForInvocation);
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

		private static object ConvertSingleJsonItem(object value, Type newType)
		{
			if (typeof(IGxJSONAble).IsAssignableFrom(newType))
			{
				var TObject = Activator.CreateInstance(newType);
				((IGxJSONAble)TObject).FromJSONObject((IJsonFormattable)value);
				return TObject;
			}
			else if (newType == typeof(DateTime))
			{
				return DateTimeUtil.CToT2(value as string);
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

		private static object ConvertStringToNewNonNullableType(object value, Type newType)
		{
			
			if (newType.IsArray)
			{
				// For comma separated list
				Type singleItemType = newType.GetElementType();

				var elements = new ArrayList();
				foreach (var element in value.ToString().Split(','))
				{
					var convertedSingleItem = ConvertSingleJsonItem(element, singleItemType);
					elements.Add(convertedSingleItem);
				}
				return elements.ToArray(singleItemType);
			}
			return ConvertSingleJsonItem(value, newType);
		}

		private static object ConvertStringToNewType(object value, Type newType)
		{
			// If it's not a nullable type, just pass through the parameters to Convert.ChangeType
			if (newType.GetTypeInfo().IsGenericType && newType.GetGenericTypeDefinition() != null && newType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				if (value == null)
				{
					return null;
				}
				return ConvertStringToNewNonNullableType(value, new NullableConverter(newType).UnderlyingType);
			}
			return ConvertStringToNewNonNullableType(value, newType);
		}

		private static Dictionary<string, object> ProcessParametersAfterInvoke(MethodInfo methodInfo, object[] parametersForInvocation)
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
			return outputParameters;
		}
		private static object[] ProcessParametersForInvoke(MethodInfo methodInfo, IDictionary<string, object> parameters, out List<int> outputParametersIndex)
		{
			var methodParameters = methodInfo.GetParameters();
			outputParametersIndex = new List<int>();
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
					outputParametersIndex.Add(idx);
				}
				if (parameters!=null && parameters.TryGetValue(gxParameterName, out value))
				{
					var convertedValue = ConvertStringToNewType(value, parmType);
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
