using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;


namespace GeneXus.UserControls.Implementation
{
	public class UserControlFactoryImpl : IUserControlFactory
	{
		private ConcurrentDictionary<string, UserControlGenerator> m_Generators = new ConcurrentDictionary<string, UserControlGenerator>();
		
		public string RenderControl(string controlType, string internalName, GxDictionary propbag)
		{
			if (!m_Generators.TryGetValue(controlType, out UserControlGenerator userControlGenerator))
			{
				userControlGenerator = new UserControlGenerator(controlType);
				m_Generators[controlType] = userControlGenerator;
			}
			return userControlGenerator.Render(internalName, propbag);
		}
	}
}
