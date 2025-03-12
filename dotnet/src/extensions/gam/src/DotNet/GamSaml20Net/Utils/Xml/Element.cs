using System;
using log4net;
using System.Xml;
using System.Collections.Generic;
using GeneXus.Data;

namespace GamSaml20Net.Utils.Xml
{
	internal class Element : XmlTypes
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Element));

		private List<string> _tags;
		internal List<string> Tags
		{
			get { return _tags; }
		}



		public Element(List<string> tags)
		{
			_tags = tags;
		}

		private string FindValue(XmlDocument xmlDoc)
		{
			XmlNodeList nodeList = GetNodeListForTags(xmlDoc);
			if (nodeList == null)
			{
				return string.Empty;
			}
			try
			{
				return nodeList[0].InnerText;
			}
			catch (Exception e)
			{
				logger.Error("FindValue", e);
				return string.Empty;
			}
		}

		internal XmlNodeList GetNodeListForTags(XmlDocument xmlDoc)
		{
			foreach (string tag in _tags)
			{
				XmlNodeList nodeList = xmlDoc.GetElementsByTagName(tag);
				if (nodeList.Count > 0)
				{
					return nodeList;
				}
			}
			logger.Error($"Could not find value for {_tags[0]} element");
			return null;
		}

		override
		internal string PrintJson(XmlDocument xmlDoc)
		{
			string value = FindValue(xmlDoc);
			return value == null ? null : $"\"{_tags[0]}\": \"{value}\"";
		}
	}
}
