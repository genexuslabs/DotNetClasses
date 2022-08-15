using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;


namespace GeneXus.Application
{
	public class GXObjectProperties
	{

		private GxLocation location = null;

		public GxLocation Location { get => location; set => location = value; }
	}

	public class GXObjectsConfiguration {
		
		private  Dictionary<string, GXObjectProperties> _properties = new Dictionary<string, GXObjectProperties>();	
		public GXObjectProperties PropertiesFor(string objName)
		{
			if (!_properties.ContainsKey(objName))
				_properties[objName] = new GXObjectProperties();
			return _properties[objName];
		}

	}

}
