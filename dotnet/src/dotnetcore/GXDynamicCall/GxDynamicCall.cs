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
		private GxDynCallProperties _properties;
		private object _object;
		public string ObjectName { get; set; }
		public GxDynCallProperties Properties     
		{
			get => _properties;
			set
			{
				_properties = Properties;
				_assemblyName = Properties.AssemblyName;
				_namespace = Properties.NameSpace;
				_externalName = Properties.ExternalName; 
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
			Create(null, out errors);
			if (errors.Count == 0)
			{
				GxDynCallMethodConf methodConf = new GxDynCallMethodConf();
				Execute(ref parameters, methodConf, out errors);
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
		public object Execute(ref IList<object> parameters, GxDynCallMethodConf methodconfiguration , out IList<SdtMessages_Message> errors)
		{
			object result;
			errors = new GXBaseCollection<SdtMessages_Message>();
			IList<object> outParms= new List<object>();

			string methodName = methodconfiguration.MethodName; 
			bool isStatic = methodconfiguration.IsStatic;

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

