using GeneXus.Application;
using GeneXus.Configuration;
using GeneXus.XML;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GeneXus.Utils
{
	public class XMLPrefixes
	{

		[System.Diagnostics.CodeAnalysis.SuppressMessage("GxFxCopRules", "CR1000:EnforceThreadSafeType")]
		Dictionary<string, string> namespacePrefixes = new Dictionary<string, string>();
		void AddNamespacePrefix(string p, string ns)
		{
			if (!namespacePrefixes.ContainsKey(p))
				namespacePrefixes.Add(p, ns);
			else
				namespacePrefixes[p] = ns;
		}
		public void SetPrefixesFromReader(GXXMLReader rdr)
		{
			for (int i = 1; i <= rdr.AttributeCount; i++)
			{
				string attName = rdr.GetAttributeName(i);
				if (attName.ToLower().StartsWith("xmlns:"))
				{
					AddNamespacePrefix(attName.Substring(6), rdr.GetAttributeByIndex(i));
				}
				else if (attName.ToLower() == "xmlns")
				{
					AddNamespacePrefix("", rdr.GetAttributeByIndex(i));
				}
			}
		}
		public Dictionary<string, string> GetPrefixes()
		{
			return namespacePrefixes;
		}

		public void SetPrefixes(Dictionary<string, string> pfxs)
		{
			namespacePrefixes = pfxs;
		}
	}
	public class GXXmlSerializer
	{
		private GXXmlSerializer() { }

		public static T Deserialize<T>(Type TargetType, string serialized, string sName, string sNameSpace, out List<string> serializationErrors)
		{
			T deserialized;
			if (!string.IsNullOrEmpty(serialized))
			{
				XmlSerializer xmls;
				GxSerializationErrorManager serErr = new GxSerializationErrorManager();
				if (string.IsNullOrEmpty(sNameSpace) && string.IsNullOrEmpty(sName))
				{
#pragma warning disable SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
					xmls = new XmlSerializer(TargetType);
#pragma warning restore SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
				}
				else
				{
					xmls = GetSerializer(TargetType, null, sName, sNameSpace);
					xmls.UnknownNode += serErr.unknownNode;
				}
				try
				{
					using (StringReader sr = new StringReader(serialized))
					{
						deserialized = (T)(xmls.Deserialize(sr));
					}
				}
				catch (InvalidOperationException ex)
				{
					if (ex.InnerException != null && ex.InnerException is FormatException) //Input String was not in correct format
					{
						serialized = GxRegex.Replace(serialized, "(<(\\w+)[^<]*?)/>", string.Empty); //Replace empty tags
						using (StringReader sr = new StringReader(serialized))
						{
							deserialized = (T)(xmls.Deserialize(sr));
						}
					}
					else
					{
						throw ex;
					}
				}

				serializationErrors = serErr.GetErrors();
				return deserialized;
			}
			serializationErrors = new List<string>();
			return default(T);
		}
		private static ConcurrentDictionary<string, XmlSerializer> serializers = new ConcurrentDictionary<string, XmlSerializer>();

		private static XmlSerializer GetSerializer(Type type, XmlAttributeOverrides ovAttrs, string rootName, string sNameSpace)
		{
			string key = string.Format("{0},{1},{2},{3}", type.FullName, rootName, sNameSpace, ovAttrs == null);
			XmlRootAttribute root = new XmlRootAttribute(rootName);
			XmlSerializer serializer;
			if (serializers.TryGetValue(key, out serializer))
			{
				return serializer;
			}
			else
			{
				return CreateSerializer(key, type, ovAttrs, root, sNameSpace);
			}
		}

		private static XmlSerializer CreateSerializer(string key, Type type, XmlAttributeOverrides ovAttrs, XmlRootAttribute root, string sNameSpace)
		{
#pragma warning disable SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
			var s = new XmlSerializer(type, ovAttrs, Array.Empty<Type>(), root, sNameSpace);
#pragma warning restore SCS0028 // Unsafe deserialization possible from {1} argument passed to '{0}'
			serializers.TryAdd(key, s);
			return s;
		}

		internal static XmlAttributeOverrides IgnoredAttributes(Dictionary<Type, GxStringCollection> stateAttrs)
		{
			XmlAttributeOverrides ov = new XmlAttributeOverrides();
			XmlAttributes attrs = new XmlAttributes();
			attrs.XmlIgnore = true;

			foreach (Type t in stateAttrs.Keys)
			{
				foreach (string attr in stateAttrs[t])
				{
					ov.Add(t, attr, attrs);
				}
			}
			return ov;
		}
		internal static string Serialize(string rootName, string sNameSpace, XmlAttributeOverrides ovAttrs, bool includeHeader, IGxXMLSerializable instance)
		{
			bool indentElements = true;
			if (Config.GetValueOf("XmlSerializationIndent", out string xmlIndent ))
			{
				indentElements = xmlIndent.Trim().ToLower() == "true";
			}
			XmlSerializer xmls = GetSerializer(instance.GetType(), ovAttrs, rootName, sNameSpace);
			string s;
			using (MemoryStream stream = new MemoryStream())
			{
				XmlWriter xmlw = XmlWriter.Create(stream, new System.Xml.XmlWriterSettings()
				{
					OmitXmlDeclaration = !includeHeader,
					Indent = indentElements,
					IndentChars = "\t",
					Encoding = Encoding.UTF8
				});
				XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
				xmlns.Add(string.Empty, sNameSpace);
				xmls.Serialize(xmlw, instance, xmlns);
				stream.Seek(0L, SeekOrigin.Begin);
				using (StreamReader sr = new StreamReader(stream))
				{
					s = sr.ReadToEnd();
				}
			}
			return s;

		}

		public static void SetSoapError(IGxContext context, string err)
		{
			context.sSOAPErrMsg += err + StringUtil.NewLine();
		}

		public static void SetSoapError(IGxContext context, List<string> errs)
		{
			foreach (var err in errs)
				SetSoapError(context, err);
		}

	}

	public class GxSerializationErrorManager
	{
		List<string> errors = new List<string>();
		int errorCount = 0;
		int maxError = 10;

		public GxSerializationErrorManager()
		{
			if (Config.GetValueOf("XmlSerialization-MaxErrorReporting", out string sMaxErr))
			{
				Int32.TryParse(sMaxErr, out this.maxError);
			}
		}
		public void unknownNode(object sender, XmlNodeEventArgs e)
		{
			if (this.errorCount <= this.maxError)
			{
				this.errorCount++;
				this.Add(string.Format("Unexpected node found ({0}): {1} (name: {2}, namespace: {3}", this.errorCount, e.Name, e.LocalName, e.NamespaceURI));
			}
		}
		public void Add( string s)
		{
			this.errors.Add(s);
		}
		public List<string> GetErrors()
		{
			return this.errors;
		}
	}
}
