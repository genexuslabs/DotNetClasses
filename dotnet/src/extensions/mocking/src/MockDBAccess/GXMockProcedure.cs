using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Application;
using GeneXus.Mock;

namespace MockDBAccess
{
	public class GXMockProcedure : IGxMock
	{
		public bool Handle<T>(IGxContext context, T objectInstance, List<GxObjectParameter> parameters) where T : GXBaseObject
		{
			StringBuilder builder = new StringBuilder("Mocking ");
			builder.Append(objectInstance.GetType().Name);
			if (parameters != null && parameters.Count>0)
			{
				builder.Append(" Parameters:");
				foreach (var parameter in parameters)
				{
					builder.Append("[");
					builder.Append($"{parameter.ParmName},{parameter.ParmInfo.ParameterType.Name}");
					if (parameter.ParmInfo.ParameterType.IsByRef)
					{
						builder.Append(",ref");
					}
					else if (parameter.ParmInfo.IsOut)
					{
						builder.Append(",out");
					}
					else
					{
						builder.Append(",in");
					}
					builder.Append(",value:" + objectInstance.GetType().GetField(parameter.ParmName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(objectInstance));
					builder.Append("] ");

					//objectInstance.GetType().GetField(parameter.ParmName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(objectInstance, NEWVALUE) //SET
				}
			}
			objectInstance.context.GX_msglist.addItem(builder.ToString());
			return true;
		}
	}
}
