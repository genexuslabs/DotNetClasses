using GeneXus.Application;
using GeneXus.Metadata;
using GeneXus.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Genexus.DynamicCall
{
    public class GxDynamicCall
    {
        static public void Invoke(string objectToInvoke, ref IList parameters,string nameS)
        {
            if (string.IsNullOrEmpty(nameS))
            {
                nameS = "GeneXus.Programs";
            }
            try
            {
                Dictionary<string, object> parms = new Dictionary<string, object>();
                foreach ( GxUserType item in parameters)
                {
                    string value = (string)item.GetType().GetProperty("gxTpr_Value").GetValue(item);
                    string name = (string)item.GetType().GetProperty("gxTpr_Name").GetValue(item);
                    parms.Add(name, value);
                }

                Type objType = ClassLoader.FindType(objectToInvoke, nameS, objectToInvoke.ToLower().Trim(), Assembly.GetCallingAssembly());

                Object o = Activator.CreateInstance(objType, Array.Empty<object>());
                Dictionary<string, object> outParms = ReflectionHelper.CallMethod(o, "execute", parms);

                GxUserType parameter = new GxUserType();

                for (int i = 0; i < parms.Count; i++)
                {
                    parameter = (GxUserType)parameters[i];
                    string name = (string)parameter.GetType().GetProperty("gxTpr_Name").GetValue(parameter);
                    Object objAux;
                    if (outParms.TryGetValue(name, out objAux))
                    {
                        parameter.GetType().GetProperty("gxTpr_Value").SetValue(parameter, objAux.ToString());
                    }

                }
            }
            catch (Exception) {
                // ver que hacer
            }
        }
    }
}

