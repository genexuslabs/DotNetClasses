using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Genexus.DynamicCall
{
    public class GxDynamicCall
    {
		private const string defaultMethod = "execute";
		private string _assemblyName;
		private Assembly _assembly;
		private string _namespace;
		private string _externalName;
		private GxUserType _properties;
		private object _object;
		public string ObjectName { get; set; }
		public GxUserType Properties     
		{
			get => _properties;
			set
			{
				_properties = Properties;
				_assemblyName = (string) _properties.GetType().GetProperty("gxTpr_Assemblyname").GetValue(_properties);
				_namespace = (string) _properties.GetType().GetProperty("gxTpr_Namespace").GetValue(_properties);
				_externalName = (string)_externalName.GetType().GetProperty("gxTpr_Externalname").GetValue(_properties);
			}
		}

		public GxDynamicCall()
		{
			_assemblyName= null;
			_assembly= null;
			_namespace= null;
			_properties= null;
			_externalName = null;
			_object = null;
		}

		private void VerifyDefaultProperties() {
			_namespace = string.IsNullOrEmpty(_namespace) ? "GeneXus.Programs" : _namespace;
			
			if (_assembly is null)
			{
				if (string.IsNullOrEmpty(_assemblyName))
				{
					_assembly = Assembly.GetCallingAssembly();
				}
				else
				{
					try
					{
						_assembly = Assembly.LoadFrom(_assemblyName);
					}
					catch
					{
						throw;
					}
				}
			}
		}

		public void Execute(ref IList<object> parameters, out IList<SdtMessages_Message> errors)
		{
			System.Diagnostics.Debugger.Launch();
			Create(null, out errors);
			if (errors.Count == 0)
			{
				try
				{
					IList<object> outParms = ReflectionHelper.CallMethod(_object, defaultMethod, parameters);
					parameters = outParms;
				}
				catch (Exception e)
				{
					GXUtil.ErrorToMessages("CallMethod Error", e.Message, (GXBaseCollection<SdtMessages_Message>)errors);
				}
			}
		}

		public void Create( IList<object> constructParms, out IList<SdtMessages_Message> errors)
		{
			errors = new GXBaseCollection<SdtMessages_Message>();
			string objectNameToInvoke;
			try
			{
				VerifyDefaultProperties();
				if (constructParms is null)
				{
					objectNameToInvoke = ObjectName;
				}
				else
				{
					objectNameToInvoke = _externalName;
				}
				try
				{
					Type objType = ClassLoader.FindType(objectNameToInvoke, _namespace, objectNameToInvoke.ToLower().Trim(), _assembly);
					object[] constructorParameters;
					if (constructParms != null && constructParms.Count > 0)
					{
						constructorParameters = new object[constructParms.Count];
						constructParms.CopyTo(constructorParameters, 0);
					}
					else
					{
						constructorParameters = Array.Empty<object>();
					}
					_object = Activator.CreateInstance(objType, constructorParameters);
				}
				catch (Exception e)
				{
					GXUtil.ErrorToMessages("CreateInstance Error", e.Message, (GXBaseCollection<SdtMessages_Message>) errors);
				}
			}
			catch (Exception e)
			{
				GXUtil.ErrorToMessages("VerifyProperties Error", e.Message, (GXBaseCollection<SdtMessages_Message>)errors);
			}
		}
		public object Execute(ref IList<object> parameters, GxUserType methodconfiguration , out IList<SdtMessages_Message> errors)
		{
			object result;
			errors = new GXBaseCollection<SdtMessages_Message>();
			IList<object> outParms= new List<object>();
#if NET462_OR_GREATER
			GxUserType methodPlatformSubLevel=(GxUserType)methodconfiguration.GetType().GetProperty("gxTpr_Netframework").GetValue(methodconfiguration);
#elif NET5_0_OR_GREATER
			GxUserType methodPlatformSubLevel=(GxUserType)methodconfiguration.GetType().GetProperty("gxTpr_Net").GetValue(methodconfiguration);
#endif
			string methodName = (string)methodPlatformSubLevel.GetType().GetProperty("gxTpr_Methodname").GetValue(methodconfiguration);
			bool isStatic = (bool)methodconfiguration.GetType().GetProperty("gxTpr_Methodisstatic").GetValue(methodconfiguration);
			if (!isStatic)
			{
				if (_object != null)
				{
					try
					{
						outParms = ReflectionHelper.CallMethod(_object, (string.IsNullOrEmpty(methodName) ? defaultMethod : methodName), parameters);
					}
					catch (Exception e)
					{
						GXUtil.ErrorToMessages("CallMethod Error", e.Message, (GXBaseCollection<SdtMessages_Message>)errors);
					}
				}
				else
				{
					GXUtil.ErrorToMessages("NullInstance Error", "You must invoke create method before execute a non static one", (GXBaseCollection<SdtMessages_Message>)errors);
				}
			}
			else
			{
				VerifyDefaultProperties();
				Type objType = ClassLoader.FindType(_externalName, _namespace, _externalName.ToLower().Trim(), _assembly);
				outParms = ReflectionHelper.CallMethod(objType, (string.IsNullOrEmpty(methodName) ? defaultMethod : methodName), parameters, isStatic);
			}
			if (outParms.Count > parameters.Count)
			{
				result = outParms[parameters.Count];
				outParms.RemoveAt(parameters.Count);

			}
			else
			{
				result = null;
			}
			parameters = outParms;
			return result;
		}

	}
}

