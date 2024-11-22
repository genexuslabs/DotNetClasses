using System;
using System.Collections.Generic;
using System.Reflection;
using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Utils;

namespace GeneXus.DynamicCall
{
	public class GxDynamicCall
	{
		private const string defaultMethod = "execute";
		private Assembly _assembly;
		private readonly IGxContext _context;
		private GXProperties _properties;
		private object _object;
		public string ExternalName { get; set; }
		public GXProperties Properties
		{
			get => _properties;
			set
			{
				_properties = Properties;
			}
		}

		public GxDynamicCall(IGxContext context)
		{
			_context = context;
			_assembly = null;
			_properties = new GXProperties();
			_object = null;
		}

		private void VerifyDefaultProperties()
		{

			if (_assembly is null)
			{
				if (string.IsNullOrEmpty(_properties.Get("AssemblyName")))
				{
					_assembly = Assembly.GetCallingAssembly();
				}
				else
				{
					try
					{
						_assembly = Assembly.LoadFrom(_properties.Get("AssemblyName"));
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

		public void Create(IList<object> constructParms, out IList<SdtMessages_Message> errors)
		{
			errors = new GXBaseCollection<SdtMessages_Message>();
			string objectNameToInvoke;
			try
			{
				VerifyDefaultProperties();

				objectNameToInvoke = ExternalName;

				try
				{
					Type objType = ClassLoader.FindType(objectNameToInvoke, objectNameToInvoke.ToLower().Trim(), _assembly);
					object[] constructorParameters;
					if (constructParms != null && constructParms.Count > 0)
					{
						constructorParameters = new object[constructParms.Count];
						constructParms.CopyTo(constructorParameters, 0);
					}
					else
					{
						constructorParameters = new object[] { _context };
					}
					_object = Activator.CreateInstance(objType, constructorParameters);
				}
				catch (Exception e)
				{
					GXUtil.ErrorToMessages("CreateInstance Error", e.Message, (GXBaseCollection<SdtMessages_Message>)errors);
				}
			}
			catch (Exception e)
			{
				GXUtil.ErrorToMessages("VerifyProperties Error", e.Message, (GXBaseCollection<SdtMessages_Message>)errors);
			}
		}
		public object Execute(ref IList<object> parameters, GxDynCallMethodConf methodconfiguration, out IList<SdtMessages_Message> errors)
		{
			object result;
			errors = new GXBaseCollection<SdtMessages_Message>();
			IList<object> outParms = new List<object>();

			string methodName = methodconfiguration.MethodName;
			bool isStatic = methodconfiguration.IsStatic;

			if (!isStatic)
			{
				if (_object != null)
				{
					try
					{
						outParms = ReflectionHelper.CallMethod(_object, string.IsNullOrEmpty(methodName) ? defaultMethod : methodName, parameters);
					}
					catch (Exception e)
					{
						GXUtil.ErrorToMessages("CallMethod Error", e.Message, (GXBaseCollection<SdtMessages_Message>)errors);
					}
				}
				else
				{
					GXUtil.ErrorToMessages("NullInstance Error", "You must invoke create method before executing a non-static one", (GXBaseCollection<SdtMessages_Message>)errors);
				}
			}
			else
			{
				VerifyDefaultProperties();
				Type objType = ClassLoader.FindType(ExternalName, ExternalName.ToLower().Trim(), _assembly);
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
	public class GxDynCallMethodConf
	{
		public bool IsStatic
		{
			get; set;
		}
		public string MethodName
		{
			get; set;
		}

		public GxDynCallMethodConf()
		{
			IsStatic = false;
			MethodName = "execute";
		}

	}
}