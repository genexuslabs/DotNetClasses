using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;

namespace GeneXus.Application
{
	public class GxObjectProperties
	{
		private GxLocation location = null;

		public GxLocation Location { get => location; set => location = value; }
	}

	public class GxObjectsConfiguration {
		
		private  Dictionary<string, GxObjectProperties> _properties = new Dictionary<string, GxObjectProperties>();	
		public GxObjectProperties PropertiesFor(string objName)
		{
			if (!_properties.ContainsKey(objName))
				_properties[objName] = new GxObjectProperties();
			return _properties[objName];
		}
	}
}
