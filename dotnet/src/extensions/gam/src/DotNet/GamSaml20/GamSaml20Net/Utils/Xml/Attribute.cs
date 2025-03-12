using System;
using log4net;
using System.Xml;
using GeneXus;
using System.Collections.Generic;

namespace GamSaml20Net.Utils.Xml
{
	internal class Attribute : XmlTypes
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Attribute));

		private Element element;

		private string _atTag;
		internal string atTag
		{
			get { return _atTag; }
		}

		public Attribute(List<string> elementTags, string tag)
		{
			element = new Element(elementTags);
			_atTag = tag;
		}

		private string FindValue(XmlDocument xmlDoc)
		{
			logger.Trace("FindValue");
			XmlNodeList nodeList = element.GetNodeListForTags(xmlDoc);
			try
			{
				return nodeList[0].Attributes.GetNamedItem(_atTag).Value;
			}
			catch (Exception e)
			{
				logger.Debug($"FindValue -- Could not found value for {_atTag} attribute", e);
				return null;
			}
		}

		override
		internal string PrintJson(XmlDocument xmlDoc)
		{
			string value = FindValue(xmlDoc);
			return value == null ? null : $"\"{_atTag}\": \"{value}\"";
		}
	}
}
