using System;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Schema;
using GeneXus.Application;
using GeneXus.Http.Client;
using GeneXus.Http.Server;
using GeneXus.Utils;
using log4net;

using System.Xml.Xsl;
using System.Xml.XPath;

namespace GeneXus.XML
{

	public interface IGxXmlFunctions
	{
		short XmlStart( );
		short XmlStart( string FileName);
		short XmlEnd();
		short XmlBeginElement( string ElementName);
		short XmlEndElement();
		short XmlValue(	string ValueName, string ElementValue);
		short XmlText( string TextValue);
		short XmlRaw( string RawText);
		short XmlAtt( string AttriName, string AttriValue);
	}
	public class GXXMLReader : IDisposable
	{
		public const short ElementType					= 1;
		public const short EndTagType					= 2;
		public const short TextType						= 4;
		public const short CommentType					= 8;
		public const short WhiteSpaceType				= 16;
		public const short CDataType					= 32;
		public const short ProcessingInstructionType	= 64;
		public const short DoctypeType					= 128;
		
		public const int ValidationNone				= 0;
		public const int ValidationAuto				= 1;
		public const int ValidationDTD				= 2;
		public const int ValidationSchema			= 3;
		public const int ValidationXDR				= 4;

		private XmlTextReader treader;
		private XmlValidatingReader vreader;

	
		private const int QUEUE_SIZE = 2;
		private int nextElementNode;
		private int nextValuedNode;

		private ElementNode[]	elementNode = new ElementNode[QUEUE_SIZE];
		private ValuedNode[]	valuedNode  = new ValuedNode[QUEUE_SIZE];
		private EndTagNode		endTagNode  = new EndTagNode();
		private PINode			piNode		= new PINode();

		private ArrayList		NodesQueue;
		private Node			CurrentNode;

		UnparsedEntitiesContainer EntitiesContainer = new UnparsedEntitiesContainer();
		private GXResolver		Resolver;
		private Encoding		encoding;
		
		private bool simpleElements = true;
		private bool nextSimpleElements = true;
		private bool removeWhiteSpaces = true;
		private bool removeWhiteNodes = true;
        private bool ignoreComments = false;
		private bool UTF8NodesEncoding;
		private Encoding documentEncoding;
		private int validationType;
		private int linesNormalization = 1;
		private NameValueCollection schemas = new NameValueCollection();

		private short errorCode;
		private string errorDescription = "";
		private int errorLineNumber;
		private int errorLinePos;   
		private bool inContent;
		private bool stopOnInvalid = true;
		string _basePath;
		GXSOAPContext soapContext;

		public GXXMLReader(string basePath) : this()
		{
			_basePath = basePath;
		}
		public GXXMLReader()
		{
			int i;
			Resolver = new GXResolver(this, EntitiesContainer);
			NodesQueue = new ArrayList(2 * QUEUE_SIZE);
			for (i = 0; i < QUEUE_SIZE; i++)
			{
				elementNode[i] = new ElementNode();
				valuedNode[i] = new ValuedNode();
			}
			SimpleElements = 1;
			RemoveWhiteNodes = 1;
			RemoveWhiteSpaces = 1;
			ReadExternalEntities = 1;
			_basePath = "";

		}
		public XmlReader GetBaseReader()
		{
			return vreader;
		}
		public void ValidationCallBack( object sender, ValidationEventArgs args )
		{
			errorCode = 2;
			errorDescription = "Validation error: " + args.Message;
			if (treader != null)
			{
				errorLineNumber = treader.LineNumber;
				errorLinePos = treader.LinePosition;
			}
			else
			{
				errorLineNumber = 0;
				errorLinePos = 0;
			}
		}
		private XmlTextReader CreateXmlTextReader (string URL)
		{		
			try
			{
				if (documentEncoding == null) 
				{
					XmlTextReader auxReader = new XmlTextReader (URL);
					Resolver.Myself = Resolver.ResolveUri(new Uri(auxReader.BaseURI), URL);
					return auxReader;
				}
				
				if (URL.ToLower().StartsWith("http:"))
				{
					HttpWebRequest request = (HttpWebRequest) WebRequest.Create(URL);
					WebResponse response = request.GetResponse();
					StreamReader sr = new StreamReader(response.GetResponseStream(), documentEncoding);
					Resolver.Myself = new Uri(URL);
					return new XmlTextReader (sr);
				}
				else
				{
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					StreamReader sr = new StreamReader(URL, documentEncoding);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
					string sBaseDirectory = GxContext.StaticPhysicalPath();

					if (!sBaseDirectory.EndsWith("\\")) sBaseDirectory += '\\';
					Uri baseUri = new Uri( sBaseDirectory );
					Resolver.Myself = new Uri(baseUri, URL);
					return new XmlTextReader (sr);
				}
			}
			
			catch (NotSupportedException )
			{
				errorCode = 1;
				errorDescription = "Bad URL: " + URL;
				errorLineNumber = 0;
				errorLinePos = 0;
				return null;
			}
			catch (ArgumentNullException )
			{
				errorCode = 1;
				errorDescription = "Bad URL";
				errorLineNumber = 0;
				errorLinePos = 0;
				return null;
			}
			catch (ArgumentException )
			{
				errorCode = 1;
				errorDescription = "Bad URL";
				errorLineNumber = 0;
				errorLinePos = 0;
				return null;
			}
			catch (Exception e)
			{
				errorCode = 1;
				errorDescription = e.Message;
				errorLineNumber = 0;
				errorLinePos = 0;
				return null;
			}

		}

		public short Open(string URL1)
		{
			string URL = PathUtil.CompletePath(URL1, _basePath);
			if (treader != null) Close();
			EntitiesContainer.Reset();
			treader = CreateXmlTextReader(URL);
			if (treader != null)
			{
				SetDtdProcessing(treader, Resolver, validationType);
				inContent = false;
				encoding = null;

				vreader = new XmlValidatingReader( treader );
				vreader.XmlResolver = Resolver;
				ValidationType = validationType;
				AddSchemasToReader();
				vreader.ValidationEventHandler += new ValidationEventHandler( ValidationCallBack );

				LinesNormalization = linesNormalization;
				RemoveWhiteNodes = (removeWhiteNodes) ? 1 : 0;
			}
			return 0;
		}

		private void SetDtdProcessing(XmlTextReader treader, GXResolver resolver, int validationType)
		{
			if (treader!=null && !resolver.ReadExternalEntities && validationType == ValidationNone)
				treader.DtdProcessing = DtdProcessing.Ignore;
		}

		public short OpenResponse(IGxHttpClient httpClient)
		{
			if (treader != null) Close();
			EntitiesContainer.Reset();
			treader = new XmlTextReader(httpClient.ReceiveStream);
			if (treader != null)
			{
				SetDtdProcessing(treader, Resolver, validationType);
				inContent = false;
				encoding = null;

				vreader = new XmlValidatingReader( treader );
				vreader.XmlResolver = Resolver;
				ValidationType = validationType;
				AddSchemasToReader();
				vreader.ValidationEventHandler += new ValidationEventHandler( ValidationCallBack );

				LinesNormalization = linesNormalization;
				RemoveWhiteNodes = (removeWhiteNodes) ? 1 : 0;
			}
			return 0;
		}

		public void OpenRequest(GxSoapRequest httpReq)
		{
			GXSOAPContext soapCtx = httpReq.SoapContext as GXSOAPContext;
			if (soapCtx == null)
			{
				soapCtx = new GXSOAPContext();
				httpReq.SoapContext = soapCtx;
			}
			this.soapContext = soapCtx;
			this.OpenRequest(httpReq as GxHttpRequest);
		}
		public void OpenRequest(GxHttpRequest httpReq)
		{
			if (treader != null) Close();
			EntitiesContainer.Reset();
			treader = new XmlTextReader (httpReq.Request.InputStream);
			if (treader != null)
			{
				SetDtdProcessing(treader, Resolver, validationType);
				inContent = false;
				encoding = null;
				vreader = new XmlValidatingReader( treader );
				vreader.XmlResolver = Resolver;
				ValidationType = validationType;
                AddSchemasToReader();
				vreader.ValidationEventHandler += new ValidationEventHandler( ValidationCallBack );
				LinesNormalization = linesNormalization;
				RemoveWhiteNodes = (removeWhiteNodes) ? 1 : 0;
			}
		}

		public void OpenFromString(string s)
		{
			inContent = false;
			EntitiesContainer.Reset();

			string sBaseDirectory = GxContext.StaticPhysicalPath();

			if (!sBaseDirectory.EndsWith("\\")) sBaseDirectory += '\\';
			Uri baseUri = new Uri( sBaseDirectory );
			Resolver.Myself = baseUri;
			treader = null;
			try
			{
				if (File.Exists(s))
					treader = new XmlTextReader(s);
			}
			catch { }
			if (treader==null)
				treader = new XmlTextReader(new StringReader(s));
			SetDtdProcessing(treader, Resolver, validationType);
			vreader = new XmlValidatingReader( treader );
			vreader.XmlResolver = Resolver;
			ValidationType = validationType;
            AddSchemasToReader();
			vreader.ValidationEventHandler += new ValidationEventHandler( ValidationCallBack );

			LinesNormalization = linesNormalization;
			RemoveWhiteNodes = (removeWhiteNodes) ? 1 : 0;
		}

		public Encoding Encoding
		{
			get
			{
				if (encoding != null) 
					return encoding;
				else if (documentEncoding != null)
					return documentEncoding;
				else if (vreader != null) 
					return vreader.Encoding;
				else
					return null;

			}
		}

		public bool InContent
		{
			get
			{
				return inContent;
			}
		}

		private void AddSchemasToReader()
		{
			int i;
			string uri;
			string nameSpace;
			for (i = 0; i < schemas.Count; i++)
			{
				try
				{
					nameSpace = schemas.Keys[i];
					if (String.IsNullOrEmpty(nameSpace)) nameSpace = null;
					uri = schemas[i];
					if (!String.IsNullOrEmpty(uri))
						vreader.Schemas.Add(nameSpace, uri);
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
					errorLineNumber = 0;
					errorLinePos = 0;
				}
			}
		}


		public void AddSchema(string uri, string nameSpace)
		{
			schemas.Add(nameSpace, uri);
			if (vreader != null && !String.IsNullOrEmpty(uri))
			{
				try
				{
                    if (String.IsNullOrEmpty(nameSpace))
						vreader.Schemas.Add(null, uri);
					else
						vreader.Schemas.Add(nameSpace, uri);
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
					errorLineNumber = 0;
					errorLinePos = 0;
				}
			}
		}

		public void AddSchema(string uri)
		{
			AddSchema(uri, "");
		}

		public short ErrCode
		{
			get
			{
				return errorCode;
			}
		}

		public string ErrDescription
		{
			get
			{
				return errorDescription;
			}
		}

		public short ErrLineNumber
		{
			get
			{
				return (short) errorLineNumber;
			}
		}

		public short ErrLinePos
		{
			get
			{
				return (short) errorLinePos;
			}
		}

		public short StopOnInvalid
		{
			get
			{
				return (short)(stopOnInvalid? 1 : 0);
			}
			set
			{
				stopOnInvalid = value != 0;
			}
		}
		public bool EOF
		{
			get
			{
                if (errorCode == 1)
                    return true;

				if (vreader == null) return true;
				return vreader.EOF;
			}
		}
		public short IsSimple
		{
			get
			{
				return (short)((getNodeExtent() > 1) ? 1 : 0);
			}
		}

		public short Read()
		{
			int extent;
			int i;
			short nRet=0;
			
			if (StoppingError) return 0;
			errorCode = 0;
			errorDescription = "";
			
			extent = getNodeExtent();
			for (i = 0; i < extent; i++)
				NodesQueue.RemoveAt(0);
			
			simpleElements = nextSimpleElements;
			nRet = fillQueue();
			
			if (nRet > 0 && (extent = getNodeExtent()) > 0)
			{
				CurrentNode = (Node)NodesQueue[0];
				if (extent > 2) 
					CurrentNode.Value = ((Node)NodesQueue[1]).Value;
			}

			if (this.soapContext != null)
				AppendNode(this.soapContext, CurrentNode, extent);
			return nRet;
		}

		
		public short ReadType(int NodeTypeConstr)
		{
			return ReadType(NodeTypeConstr, "");
		}

		public short ReadType(int NodeTypeConstr, string NameConstr)
		{
			string AuxName = NameConstr.Trim();
			short ret;
			
			while ( (ret = Read()) > 0)
			{
				if	( (NodeType & NodeTypeConstr) > 0 && (AuxName.Length == 0 || Name == NameConstr)) 
				{
					break;
				}
			}
			return ret;
		}

		private string SkipAndReadRaw(bool GenerateXML)
		{
			int Level;
			string sRet = null;
			string AttName = null;
			int OldRemoveWhiteSpaces = 0;
			char []trimChars = {'\t', ' '};

			StringWriter	sWriter = null;
			GXXMLWriter		xmlWriter = null;
			
			if (NodeType != ElementType) return "";

			if (GenerateXML)
			{
				sWriter = new StringWriter();
				xmlWriter = new GXXMLWriter();
				xmlWriter.Open(sWriter);
				OldRemoveWhiteSpaces = RemoveWhiteSpaces;
				RemoveWhiteSpaces = 0;
			}


			Level = 0;
			do
			{	
				switch (NodeType)
				{
					case ElementType:

						if (GenerateXML)
						{
							if (IsSimple > 0)
								xmlWriter.WriteElement(Name, Value);
							else
							{
								Level++;
								xmlWriter.WriteStartElement(Name);
							}
											
							for (int i = 1; xmlWriter.ErrCode == 0 && (AttName = GetAttributeName(i)).Length > 0; i++)
							{
								xmlWriter.WriteAttribute(AttName, GetAttributeByIndex(i));
							}
						}
						else
						{
							if (! (IsSimple > 0)) Level++;
						}
						break;

					case EndTagType:
						if (GenerateXML)
							xmlWriter.WriteEndElement();
						Level--;
						break;

					case WhiteSpaceType:
						break;
					case TextType:
						if (GenerateXML)
						{
							string sValue = Value;
							if (sValue.StartsWith("\r\n"))
								sValue = sValue.Substring(2);
							else if (sValue.StartsWith("\n"))
								sValue = sValue.Substring(1);
							xmlWriter.WriteText(sValue.TrimEnd(trimChars));
						}
						break;

					case CommentType:
						if (GenerateXML)
						{
							xmlWriter.WriteComment(Value);
						}
						break;
				
					case CDataType:
						if (GenerateXML)
						{
							xmlWriter.WriteCData(Value);
						}
						break;

					case ProcessingInstructionType:
						if (GenerateXML)
						{
						
							xmlWriter.WriteProcessingInstruction(Name, Value);
						}
						break;

				}
			}
			while (Level > 0 && Read() > 0); //First Check for Level and then if Level>0 Move cursor to the next ElementNode.


			if (GenerateXML)
			{
				xmlWriter.Close();
				if (!StoppingError)
					sRet = sWriter.ToString();
				else
					sRet = "";
				sWriter.Close();
				if (sRet == null) sRet = "";
				RemoveWhiteSpaces = OldRemoveWhiteSpaces;
			}
			
			return sRet;
		}

		public void Skip()
		{
			SkipAndReadRaw(false);
		}

		public string ReadRawXML()
		{
			return SkipAndReadRaw(true);
		}


		public short Close()
		{
			if ( treader != null )
			{
				treader.Close();
				treader = null;
			}
			if ( vreader != null )
			{
				vreader.Close();
				vreader = null;
			}
			CurrentNode = null;
			documentEncoding = null;

			if (this.soapContext != null)
				this.soapContext.EndMessage();
			return 0;
		}

		public short AttributeCount
		{
			get
			{
				if (CurrentNode != null)
					return CurrentNode.AttributeCount;
				else
					return 0;
			}
		}

		public short NodeType
		{
			get
			{
				if (CurrentNode != null)
					return CurrentNode.NodeType;
				else
					return 0;
			}
		}

		public string Name
		{
			get
			{
				if (CurrentNode != null)
					return CurrentNode.Name;
				else
					return "";
			}
		}

		public string Prefix
		{
			get
			{
				if (CurrentNode != null)
					return CurrentNode.Prefix;
				else
					return "";
			}
		}

		public string LocalName
		{
			get
			{
				if (CurrentNode != null)
					return CurrentNode.LocalName;
				else
					return "";
			}
		}

		public string NamespaceURI
		{
			get
			{
				if (CurrentNode != null)
					return CurrentNode.NameSpaceURI;
				else
					return "";
			}
		}

		public void SetNodeEncoding(string sEncoding)
		{
			if (sEncoding == "UTF-8")
				UTF8NodesEncoding = true;
			else if (sEncoding == "ANSI")
				UTF8NodesEncoding = false;
		}

		public void SetDocEncoding(string sEncoding)
		{
			SetDocumentEncoding(sEncoding);
		}

		public void SetDocumentEncoding(string sEncoding)
		{
            documentEncoding = GXUtil.GxIanaToNetEncoding(sEncoding, false);
		}

		public string Value
		{
			get
			{
				try
				{
					if (CurrentNode != null)
						if (!UTF8NodesEncoding)
							return (RemoveWhiteSpaces > 0) ? CurrentNode.Value.Trim() : CurrentNode.Value;
						else
						{
							UTF8Encoding utf8Encoding = new UTF8Encoding();
							byte[] utf8Bytes = utf8Encoding.GetBytes((RemoveWhiteSpaces > 0) ? CurrentNode.Value.Trim() : CurrentNode.Value);
							return Encoding.Unicode.GetString(utf8Bytes);
						}
					else
						return "";
				}
				catch (Exception )
				{
					return "";
				}
			}
		}


		public string GetAttributeByIndex(int Index)
		{
			if (CurrentNode != null)
				return CurrentNode.GetAttributeByIndex(Index - 1);
			else
				return "";
		}

		public string GetAttributeByName(string Name)
		{
			if (CurrentNode != null)
				return CurrentNode.GetAttributeByName(Name);
			else
				return "";
		}

		public short ExistsAttribute(string Name)
		{
			if (CurrentNode != null)
				return CurrentNode.ExistsAttribute(Name);
			else
				return 0;
		}

		public string GetAttributeName(int Index)
		{
			if (CurrentNode != null)
				return CurrentNode.GetAttributeName(Index - 1);
			else
				return "";
		}

		public string GetAttributePrefix(int Index)
		{
			if (CurrentNode != null)
				return CurrentNode.GetAttributePrefix(Index - 1);
			else
				return "";
		}

		public string GetAttributeLocalName(int Index)
		{
			if (CurrentNode != null)
				return CurrentNode.GetAttributeLocalName(Index - 1);
			else
				return "";
		}

		public string GetAttributeURI(int Index)
		{
			if (CurrentNode != null)
				return CurrentNode.GetAttributeURI(Index - 1);
			else
				return "";
		}

		public string GetAttEntityValueByIndex(int Index)
		{
			string sVal = GetAttributeByIndex(Index);
			if (String.IsNullOrEmpty(sVal)) return string.Empty;
			return EntitiesContainer.GetEntityValue(sVal);
		}
		public string GetAttEntityValueByName(string Name)
		{
			string sVal = GetAttributeByName(Name);
			if (String.IsNullOrEmpty(sVal)) return String.Empty;
			return EntitiesContainer.GetEntityValue(sVal);
		}
		public string GetAttEntityNotationByIndex(int Index)
		{
			string sVal = GetAttributeByIndex(Index);
			if (String.IsNullOrEmpty(sVal)) return string.Empty;
			return EntitiesContainer.GetEntityNotation(sVal);
		}

		public string GetAttEntityNotationByName(string Name)
		{
			string sVal = GetAttributeByName(Name);
			if (String.IsNullOrEmpty(sVal)) return String.Empty;
			return EntitiesContainer.GetEntityNotation(sVal);
		}



		public int ReadExternalEntities
		{
			get
			{
				return (Resolver.ReadExternalEntities) ? 1 : 0;
			}
			set
			{
				Resolver.ReadExternalEntities = value > 0;
				SetDtdProcessing(treader, Resolver, validationType);
			}
		}

		public int RemoveWhiteSpaces
		{
			get
			{
				return (removeWhiteSpaces) ? 1 : 0;
			}
			set
			{
				removeWhiteSpaces = (value > 0);
			}
		}

		public int RemoveWhiteNodes
		{
			get
			{
				return (removeWhiteNodes) ? 1 : 0;
			}
			set
			{
				removeWhiteNodes = (value > 0);
				if (treader != null)
					treader.WhitespaceHandling = (value > 0)? WhitespaceHandling.None : WhitespaceHandling.All;
			}
		}
		public int ValidationType
		{
			get
			{
				return validationType;
			}
			set
			{
				if (value >= ValidationNone && value <= ValidationXDR)
				{
					validationType = value;
					if (vreader != null)
						switch (validationType)
						{
							case ValidationNone:
								vreader.ValidationType = System.Xml.ValidationType.None;
								break;
							case ValidationAuto:
								vreader.ValidationType = System.Xml.ValidationType.Auto;
								break;
							case ValidationDTD:
								vreader.ValidationType = System.Xml.ValidationType.DTD;
								break;
							case ValidationSchema:
								vreader.ValidationType = System.Xml.ValidationType.Schema;
								break;
							case ValidationXDR:
								vreader.ValidationType = System.Xml.ValidationType.XDR;
								break;
						}
				}
			}
		}
		public int LinesNormalization
		{
			get
			{
				return linesNormalization;
			}
			set
			{				
				switch (value)
				{
					case 0:
						linesNormalization = value;
						if (treader != null)
							treader.Normalization = false;
						break;
					case 1:
						linesNormalization = value;
						if (treader != null)
							treader.Normalization = true;
						break;
					case 2:
						linesNormalization = value;
						if (treader != null)
							treader.Normalization = true;
						break;
				}	
			}
		}

		public int SimpleElements
		{
			get
			{
				return (nextSimpleElements) ? 1 : 0;
			}
			set
			{
				nextSimpleElements = (value > 0) ;
			}
		}

        public int IgnoreComments
        {
            get
            {
                return (ignoreComments) ? 1 : 0;
            }
            set
            {
                ignoreComments = (value > 0);
            }
        }

		private int getNodeExtent()
		{
			int size, i;
			Node auxNode;
			
			size = NodesQueue.Count;
			if (size == 0) return 0;
			
			if (!simpleElements) return 1;
			
			i = 0;
			auxNode = (Node)NodesQueue[i];
			if (auxNode.NodeType != ElementType) return 1;
			
			if (++i >= size) return 0;
			auxNode = (Node)NodesQueue[i];
			if (auxNode.NodeType == EndTagType) 
				return i + 1;
			
			if (auxNode.NodeType != TextType && auxNode.NodeType != WhiteSpaceType) 
				return 1;
			
			if (++i >= size) return 0;
			auxNode = (Node)NodesQueue[i];
			if (auxNode.NodeType == EndTagType) 
				return i + 1;
			else
				return 1;
		}


		private bool StoppingError
		{
			get
			{
				return errorCode == 1 || errorCode == 2 && stopOnInvalid;
			}
		}

		private short fillQueue()
		{
			bool nRet = true;
			while (nRet && !StoppingError && getNodeExtent() == 0)
			{
					
				try
				{
					nRet = vreader.Read();
					if (vreader.Encoding != null) encoding = vreader.Encoding;
				}
				catch (XmlException e)
				{
					errorCode = 1;   
					errorDescription = e.Message;
					errorLineNumber = treader.LineNumber;
					errorLinePos = treader.LinePosition;
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
					errorLineNumber = treader.LineNumber;
					errorLinePos = treader.LinePosition;
				}
				
				if (nRet && !StoppingError)
				{
					if (vreader.NodeType == XmlNodeType.Element) 
					{	
						inContent = true;
						elementNode[nextElementNode].Name = vreader.Name;
						elementNode[nextElementNode].LocalName = vreader.LocalName;
						elementNode[nextElementNode].Prefix = vreader.Prefix;
						elementNode[nextElementNode].NameSpaceURI = vreader.NamespaceURI;
						elementNode[nextElementNode].Value = "";
						elementNode[nextElementNode].ClearAttributes();

						for (bool exists = vreader.MoveToFirstAttribute(); exists; exists = vreader.MoveToNextAttribute())
							elementNode[nextElementNode].AddAttribute(vreader.Name, vreader.Prefix, vreader.LocalName, vreader.NamespaceURI, vreader.Value);
							
						vreader.MoveToElement();

						NodesQueue.Add(elementNode[nextElementNode]);
						nextElementNode = (nextElementNode + 1) % QUEUE_SIZE;
						if (vreader.IsEmptyElement) 
						{
							endTagNode.Name = vreader.Name;
							endTagNode.LocalName = vreader.LocalName;
							endTagNode.Prefix = vreader.Prefix;
							endTagNode.NameSpaceURI = vreader.NamespaceURI;
							NodesQueue.Add(endTagNode);
						}
					} 
					else if (vreader.NodeType == XmlNodeType.EndElement) 
					{
						endTagNode.Name = vreader.Name;
						endTagNode.LocalName = vreader.LocalName;
						endTagNode.Prefix = vreader.Prefix;
						endTagNode.NameSpaceURI = vreader.NamespaceURI;
						NodesQueue.Add(endTagNode);
					}
					else if (vreader.NodeType == XmlNodeType.ProcessingInstruction) 
					{
						piNode.Name = vreader.Name;
						piNode.Value = vreader.Value;
						NodesQueue.Add(piNode);
						
					}
					else if (vreader.NodeType == XmlNodeType.CDATA) 
					{
						valuedNode[nextValuedNode].NodeType = CDataType;
						valuedNode[nextValuedNode].Value = vreader.Value;
						NodesQueue.Add(valuedNode[nextValuedNode]);
						nextValuedNode = (nextValuedNode + 1) % QUEUE_SIZE;
					}
					else if (vreader.NodeType == XmlNodeType.DocumentType) 
					{
						valuedNode[nextValuedNode].NodeType = DoctypeType;
						valuedNode[nextValuedNode].Value = vreader.Name;
						NodesQueue.Add(valuedNode[nextValuedNode]);
						nextValuedNode = (nextValuedNode + 1) % QUEUE_SIZE;
						if (!String.IsNullOrEmpty(vreader.Value))
							EntitiesContainer.addString(vreader.Value);
					}
					else if (vreader.NodeType == XmlNodeType.Comment && !ignoreComments) 
					{
						valuedNode[nextValuedNode].NodeType = CommentType;
						valuedNode[nextValuedNode].Value = vreader.Value;
						NodesQueue.Add(valuedNode[nextValuedNode]);
						nextValuedNode = (nextValuedNode + 1) % QUEUE_SIZE;
					}
					else if (vreader.NodeType == XmlNodeType.Text)
					{
						bool Empty = String.IsNullOrEmpty(vreader.Value.Trim());
						if (RemoveWhiteNodes == 0 || !Empty)
						{
							valuedNode[nextValuedNode].NodeType = (!Empty) ? TextType : WhiteSpaceType;
							valuedNode[nextValuedNode].Value = vreader.Value;
							NodesQueue.Add(valuedNode[nextValuedNode]);
							nextValuedNode = (nextValuedNode + 1) % QUEUE_SIZE;
						}
					}
					else if (vreader.NodeType == XmlNodeType.SignificantWhitespace || vreader.NodeType == XmlNodeType.Whitespace)
					{
						if (RemoveWhiteNodes == 0)
						{
							valuedNode[nextValuedNode].NodeType = WhiteSpaceType;
							valuedNode[nextValuedNode].Value = vreader.Value;
							NodesQueue.Add(valuedNode[nextValuedNode]);
							nextValuedNode = (nextValuedNode + 1) % QUEUE_SIZE;
						}
					}
				}
			}
				if (getNodeExtent() > 0)
					return 1;
				else
					return 0;
		}

		static bool isSimpleExtent(int extent)
		{
			return extent > 1;
		}

		internal void AppendNode(GXSOAPContext ctx, Node node, int extent)
		{
			GXXMLWriter xmlWriter = ctx.XmlWriter;

			char[] trimChars = { '\t', ' ' };

			if (node.NodeType != ElementType) return;
			switch (node.NodeType)
			{
				case ElementType:

					if (isSimpleExtent(extent))
						xmlWriter.WriteElement(node.Name, node.Value);
					else
					{
						xmlWriter.WriteStartElement(node.Name);
					}

					string AttName = null;
					for (int i = 1; xmlWriter.ErrCode == 0 && (AttName = node.GetAttributeName(i - 1)).Length > 0; i++)
					{
						xmlWriter.WriteAttribute(AttName, node.GetAttributeByIndex(i - 1));
					}
					break;

				case EndTagType:
					xmlWriter.WriteEndElement();
					break;

				case WhiteSpaceType:
					break;

				case TextType:
					string sValue = node.Value;
					if (sValue.StartsWith("\r\n"))
						sValue = sValue.Substring(2);
					else if (sValue.StartsWith("\n"))
						sValue = sValue.Substring(1);
					xmlWriter.WriteText(sValue.TrimEnd(trimChars));
					break;

				case CommentType:
					xmlWriter.WriteComment(node.Value);
					break;

				case CDataType:
					xmlWriter.WriteCData(node.Value);
					break;

				case ProcessingInstructionType:
					xmlWriter.WriteProcessingInstruction(node.Name, node.Value);
					break;
			}
		}

		////////////////////////////////////////////////////////////////////////////////////

		private class GXResolver: XmlUrlResolver
		{

			private Uri myself;
			private bool readExternalEntities = true;
			private GXXMLReader xmlreader;
			private UnparsedEntitiesContainer entities;

			public Uri Myself
			{
				get
				{
					return myself;
				}
				set
				{
					myself = value;
				}
			}

			public bool ReadExternalEntities
			{
				get
				{
					return readExternalEntities;
				}
				set
				{
					readExternalEntities = value;
				}
			}

			public GXResolver(GXXMLReader reader, UnparsedEntitiesContainer EntitiesContainer)
			{
				xmlreader = reader;
				entities = EntitiesContainer;
			}
			
			public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)	
			{
				Stream baseStream = (absoluteUri.ToString() == Myself.ToString() || ReadExternalEntities) ? (Stream)base.GetEntity(absoluteUri, role, ofObjectToReturn) : new MemoryStream();
				return absoluteUri.AbsoluteUri.Equals(Myself.AbsoluteUri) || xmlreader.InContent? baseStream : new DTDParsingStream(baseStream, entities, xmlreader);
				//compare AbsoluteUri instead of ToString because it can happen that 
				//absoluteUri.ToString="file://\\output.XML" y Myself.ToString="file://\output.XML", they are different but the Uri is the same
			}
		}

		
		private class DTDParsingStream : Stream
		{
			private Stream baseStream;
			private UnparsedEntitiesContainer entities;
			private GXXMLReader xmlreader;

			public DTDParsingStream(Stream BaseStream, UnparsedEntitiesContainer EntitiesContainer, GXXMLReader reader)
			{
				baseStream  = BaseStream;
				entities = EntitiesContainer;
				xmlreader = reader;
			}

			public override  int Read(byte[] buffer, int offset, int count)
			{
				int res = baseStream.Read(buffer, offset, count);
				if (res > 0)
				{
					Encoding encoding = xmlreader.Encoding;
					if (encoding == null) encoding = Encoding.UTF8;
					entities.addString(encoding.GetString(buffer, offset, res));
				}
				return res;
			}

			public override  int ReadByte()
			{
				int res = baseStream.ReadByte();
				if (res >= 0)
				{
					Encoding encoding = xmlreader.Encoding;
					if (encoding == null) encoding = Encoding.UTF8;
					byte[] buffer = new Byte[1];
					entities.addString(encoding.GetString(buffer, 0, 1));
				}
				return res;
			}

			public override bool CanRead {get{return baseStream.CanRead;}}
			public override bool CanSeek {get{return baseStream.CanSeek;}}
			public override bool CanWrite {get{return baseStream.CanWrite;}}
			public override long Length {get{return baseStream.Length;}}
			public override long Position {get{return baseStream.Position;} set{baseStream.Position = value;}}
			public override void Close(){baseStream.Close();}
			public override void Flush(){baseStream.Flush();}
			public override long Seek(long offset, SeekOrigin origin){return baseStream.Seek(offset, origin);}
			public override void SetLength(long value){baseStream.SetLength(value);}
			public override void Write(byte[] buffer, int offset, int count){baseStream.Write(buffer, offset, count);}
			public override void WriteByte(byte value){baseStream.WriteByte(value);}

		}

		private class UnparsedEntitiesContainer
		{
			private Hashtable entities = new Hashtable();
			
			private string parsingBuffer = "";
			private enum parsingStates
			{
				Initial,
				seenOpen,
				seenOpen2,
				InEntity,
				InNotation
			};
			private parsingStates state = parsingStates.Initial;


			public void Reset()
			{
				state = parsingStates.Initial;
				parsingBuffer = "";
				entities.Clear();
			}

			private bool loadEntity(string s)
			{
				int index, index2;
				int length = s.Length;
				string sName, sPubid, sSystem, sNotation;

				//Firsth character must be white space
				if (!char.IsWhiteSpace(s[0])) return false;

				//Extract entity name
				for (index = 1; index < length && !char.IsWhiteSpace(s[index]); index++);
				if (index == length || index == 1) return false;
				sName = s.Substring(1, index - 1);
				index++;
				
				//Extract SYSTEM/PUBLIC key word
				for (index2 = index; index2 < length && !char.IsWhiteSpace(s[index2]); index2++);
				if (index2 == length || index2 == index) return false;
				string sKeyword = s.Substring(index, index2 - index);
				index = index2 + 1;

				//Determine bounderies of SystemLiteral or PubidLiteral;
				if (index >= length -1) return false;
				if (s[index] == '"')
					index2 = s.IndexOf('"', index + 1);
				else if (s[index] == '\'')
					index2 = s.IndexOf('\'', index + 1);
				else
					return false;
				if (index2 <= 0) return false;

				if (sKeyword == "SYSTEM")
				{
					sSystem = s.Substring(index+1, index2 - index -1);
					sPubid = "";
				}
				else if (sKeyword == "PUBLIC")
				{
					sPubid = s.Substring(index+1, index2 - index -1);
					index = index2 + 1;
					if (!char.IsWhiteSpace(s[index])) return false;

					//Determine bounderies of SystemLiteral
					index++;
					if (index >= length -1) return false;
					if (s[index] == '"')
						index2 = s.IndexOf('"', index + 1);
					else if (s[index] == '\'')
						index2 = s.IndexOf('\'', index + 1);
					else
						return false;
					if (index2 <= 0) return false;
					sSystem = s.Substring(index+1, index2 - index -1);
				}
				else 
					return false;
				index = index2 + 1;

				//NData declaration
				if (index + 7 < length && 
					char.IsWhiteSpace(s[index]) && 
					s.Substring(index + 1, 5).Equals("NDATA") &&
					char.IsWhiteSpace(s[index+6]))
				{
					sNotation = s.Substring(index+7, length - index - 7).Trim();
				}
				else
					sNotation = "";

				try
				{
					entities.Add(sName, new EntityDeclaration(sName, sPubid, sSystem, sNotation));
				}
				catch (Exception)
				{
					return false;
				}
				return true;
			}


			private bool loadNotation(string s)
			{
				return false;
			}

			public string GetEntityValue(string sName)
			{
				object obj = entities[sName];
				if (obj == null) return "";
				EntityDeclaration ed = (EntityDeclaration)obj;
				return ed.System;
			}

			public string GetEntityNotation(string sName)
			{
				object obj = entities[sName];
				if (obj == null) return "";
				EntityDeclaration ed = (EntityDeclaration)obj;
				return ed.Notation;
			}

			public void addString(string s)
			{
				int index, index2;
				for (index = 0; index >= 0 && index < s.Length;)
				{
					switch (state)
					{
						case parsingStates.Initial:
							if ((index = s.IndexOf("<", index)) >= 0)
							{
								state = parsingStates.seenOpen;
								index++;
								parsingBuffer = "";
							}
							break;
						case parsingStates.seenOpen:
							if (s[index] == '!')
							{
								state = parsingStates.seenOpen2;
								index++;
							}
							else
								state = parsingStates.Initial;
							break;
						case parsingStates.seenOpen2:
							if (char.IsLetter(s[index]))
								parsingBuffer += s[index++];
							else
							{
								if (parsingBuffer.Equals("ENTITY"))
									state = parsingStates.InEntity;
								else if (parsingBuffer.Equals("NOTATION"))
									state = parsingStates.InNotation;
								else
									state = parsingStates.Initial;
								parsingBuffer = "";
							}
							break;
						case parsingStates.InEntity:
							if ((index2 = s.IndexOf(">", index)) >= 0)
							{
								state = parsingStates.Initial;
								parsingBuffer += s.Substring(index, index2 - index);
								loadEntity(parsingBuffer);
							}
							else
								parsingBuffer += s.Substring(index);
							index = index2;
							break;
						case parsingStates.InNotation:
							if ((index2 = s.IndexOf(">", index)) >= 0)
							{
								state = parsingStates.Initial;
								parsingBuffer += s.Substring(index, index2 - index);
								loadNotation(parsingBuffer);
							}
							else
								parsingBuffer += s.Substring(index);
							index = index2;
							break;
					}
				}
			}

			private class EntityDeclaration
			{
				private string system, notation;
				public EntityDeclaration(string sName, string sPubid, string sSytem, string sNotation)
				{
					
					system = sSytem;
					notation = sNotation;
				}

				public string System
				{
					get {return system;}
				}
				public string Notation
				{
					get {return notation;}
				}
			}

		}



		internal abstract class Node
		{
			abstract public short NodeType {get;set;}

			virtual public string Name 
			{
				get	{return "";}
				set {;}
			}
			virtual public string Value 
			{
				get	{return "";}
				set {;}
			}
			virtual public short AttributeCount
			{
				get {return 0;}
				set {;}
			}
			virtual public string GetAttributeByIndex(int Index)
			{
				return "";
			}
			virtual public string GetAttributeByName(string name)
			{
				return "";
			}
			virtual public short ExistsAttribute(string name)
			{
				return 0;
			}

			virtual public string GetAttributeName(int Index)
			{
				return "";
			}

			virtual public string GetAttributePrefix(int Index)
			{
				return "";
			}

			virtual public string GetAttributeLocalName(int Index)
			{
				return "";
			}

			virtual public string GetAttributeURI(int Index)
			{
				return "";
			}

			virtual public string Prefix 
			{
				get	{return "";}
				set {;}
			}
			virtual public string LocalName 
			{
				get	{return "";}
				set {;}
			}
			virtual public string NameSpaceURI 
			{
				get	{return "";}
				set {;}
			}
		}

		
		private abstract class NamedBasic: Node
		{
			private string m_Name;
			private string m_Prefix;
			private string m_LocalName;
			private string m_NamespaceURI;

			public NamedBasic(string name, string prefix, string local, string uri)
			{
				Name			= name;
				Prefix			= prefix;
				LocalName		= local;
				NameSpaceURI	= uri;
			}

			override public string Name 
			{
				get
				{
					return m_Name;
				}
				set
				{
					m_Name = value;
				}
			}

			override public string Prefix 
			{
				get
				{
					return m_Prefix;
				}
				set
				{
					m_Prefix = value;
				}
			}

			override public string LocalName 
			{
				get
				{
					return m_LocalName;
				}
				set
				{
					m_LocalName = value;
				}
			}

			override public string NameSpaceURI 
			{
				get
				{
					return m_NamespaceURI;
				}
				set
				{
					m_NamespaceURI = value;
				}
			}

		}

		private class AttributeNode: NamedBasic
		{
			private string m_Value;

			public AttributeNode(string name, string prefix, string local, string uri, string val)
				: base (name, prefix, local, uri)
			{
				Value = val;
			}

			public AttributeNode()
				: base ("", "", "", "")
			{
				Value = "";
			}

			override public string Value 
			{
				get
				{
					return m_Value;
				}
				set
				{
					m_Value = value;
				}
			}

			override public short NodeType 
			{
				get	{return 0;}
				set {;}
			}
		}

		
		private class ElementNode: NamedBasic
		{
			private string m_Value;
			private ArrayList Attributes = new ArrayList();
			private ArrayList AttributesPool = new ArrayList();
			private int AttributesPoolUsed;

			public ElementNode()
				: base ("", "", "", "")
			{
				m_Value = "";
			}

			override public short NodeType 
			{
				get
				{
					return GXXMLReader.ElementType;
				}
				set {;}
			}

			override public string Value 
			{
				get
				{
					return m_Value;
				}
				set
				{
					m_Value = value;
				}
			}

			public override short AttributeCount
			{
				get
				{
					return  (short)Attributes.Count;
				}
			}
			
			private int GetAttributeIndex(string name)
			{
				int i, Max;

				Max = Attributes.Count;
				for (i = 0; i < Max; i++)
					if ( ((AttributeNode)Attributes[i]).Name == name)
						return i;
				return -1;
			}

			override public string GetAttributeByIndex(int Index)
			{
				return (Index >= 0 && Index < Attributes.Count) ? ((AttributeNode)Attributes[Index]).Value : "";
			}

			override public string GetAttributeByName(string name)
			{
				int Index;

				if ((Index = GetAttributeIndex(name)) >= 0)
					return ((AttributeNode)Attributes[Index]).Value;
				else
					return "";
			}

			override public string GetAttributeName(int Index)
			{
				return (Index >= 0 && Index < Attributes.Count) ? ((AttributeNode)Attributes[Index]).Name : "";
			}

			override public string GetAttributePrefix(int Index)
			{
				return (Index >= 0 && Index < Attributes.Count) ? ((AttributeNode)Attributes[Index]).Prefix : "";
			}

			override public string GetAttributeLocalName(int Index)
			{
				return (Index >= 0 && Index < Attributes.Count) ? ((AttributeNode)Attributes[Index]).LocalName : "";
			}

			override public string GetAttributeURI(int Index)
			{
				return (Index >= 0 && Index < Attributes.Count) ? ((AttributeNode)Attributes[Index]).NameSpaceURI : "";
			}

			override public short ExistsAttribute(string name)
			{
				return (short)((GetAttributeIndex(name) >= 0) ? 1 : 0);
			}

			public void AddAttribute (string name, string prefix, string local, string uri, string val)
			{
				AttributeNode Attribute = null;

				if (AttributesPoolUsed >= AttributesPool.Count)
				{
					Attribute = new AttributeNode(name, prefix, local, uri, val);
					AttributesPool.Add(Attribute);
				}
				else
				{
					Attribute = (AttributeNode)AttributesPool[AttributesPoolUsed];
					Attribute.Name = name;
					Attribute.Prefix = prefix;
					Attribute.LocalName = local;
					Attribute.NameSpaceURI = uri;
					Attribute.Value = val;
					
				}

				AttributesPoolUsed++;
				Attributes.Add(Attribute);
			}

			public void ClearAttributes()
			{
				Attributes.Clear();
				AttributesPoolUsed = 0;
			}
		}

		private class EndTagNode: NamedBasic
		{

			public EndTagNode()
				: base ("", "", "", "")
			{
			}

			override public short NodeType 
			{
				get
				{
					return GXXMLReader.EndTagType;
				}
				set {;}
			}
		}
			
		private class ValuedNode: Node
		{
			private short m_NodeType;
			private string m_Value;

			override public short NodeType 
			{
				get
				{
					return m_NodeType;
				}
				set
				{
					m_NodeType = value;
				}
			}

			override public string Value 
			{
				get
				{
					return m_Value;
				}
				set
				{
					m_Value = value;
				}
			}

		}

		private class PINode: Node
		{
			private string m_Value;
			private string m_Name;

			override public short NodeType 
			{
				get
				{
					return GXXMLReader.ProcessingInstructionType;
				}
				set {;}
			}

			override public string Value 
			{
				get
				{
					return m_Value;
				}
				set
				{
					m_Value = value;
				}
			}

			override public string Name
			{
				get
				{
					return m_Name;
				}
				set
				{
					m_Name = value;
				}
			}
		}

        #region IDisposable Members

        public void Dispose()
        {
			if ( treader != null || vreader != null )
                this.Close();
        }

        #endregion
    }

	public class GXXMLWriter: IDisposable
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GXXMLWriter));
		private XmlTextWriter writer;

		private short errorCode;
		private string errorDescription;

		private int indentation = 1;
		private string indentChar = "\t";
		private int level;
		private string valueBuffer;
		private bool StartTagFlag;
		private bool CloseUnderlyingWriter = true;
		private bool TextFlag;
		
		private string strEncoding = "";
		private Encoding encoding;
		private StringWriter strWriter;
		private Stream stream;
		string _basePath;

		public XmlWriter GetBaseWriter()
		{
			return writer;
		}
		public GXXMLWriter(string basePath) : this()
		{
			_basePath = basePath;
		}
		public GXXMLWriter()
		{
			_basePath = "";
		}
		private void checkStream()
		{
			if (writer == null && stream != null)
			{
				Encoding enc = getEncoding();
				if( enc == null || enc.EncodingName.IndexOf("UTF-8") != -1) 
					writer = new XmlTextWriter(stream, null);
				else
					writer = new XmlTextWriter(stream, enc);
				
			}
		}

		public short Open(string File1)
		{
			string File = PathUtil.CompletePath(File1, _basePath);
			strWriter = null;
			if (File.Trim().Length == 0) 
			{
				Open(Console.Out);
			}
			else
			{
				if (writer != null) Close();
				
                try
                {
                    string path = Path.GetDirectoryName(File);
                    if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
	                GXLogging.Error(log, "Error creating basePath " + _basePath, ex);
                }
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				stream = new FileStream(File, FileMode.Create);
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
				
				valueBuffer = null;
				StartTagFlag = false;
				TextFlag = false;
				CloseUnderlyingWriter = true;
				level = 0;
			}
			return 0;
		}

		public short Open(TextWriter twriter)
		{
			if (writer != null) Close();
			writer = new XmlTextWriter(twriter);
			writer.Formatting = Formatting.Indented;
			valueBuffer = null;
			StartTagFlag = false;
			TextFlag = false;
			CloseUnderlyingWriter = false;
			level = 0;
			return 0;

		}

		
		public void OpenRequest(IGxHttpClient httpClient)
		{
			if (writer != null) Close();
			
			stream = httpClient.SendStream;

			valueBuffer = null;
			StartTagFlag = false;
			TextFlag = false;
			CloseUnderlyingWriter = false;
			level = 0;
		}
		public short OpenResponse(GxHttpResponse httpRes)
		{
			if (writer != null) Close();
			
			stream = httpRes.Response.OutputStream;

			valueBuffer = null;
			StartTagFlag = false;
			TextFlag = false;
			CloseUnderlyingWriter = false;
			level = 0;
			return 0;
		}
		public void OpenToString()
		{
			strWriter = new StringWriter();
			Open(strWriter);
		}

		public string ResultingString
		{
			get
			{
				if (strWriter != null)
				{
					return strWriter.ToString();
				}
				else return "";
			}
		}

		public short Close()
		{
			if (writer != null)
			{
				FlushValueBuffer();
				try
				{
					while (level > 0)
						WriteEndElement();
					if (CloseUnderlyingWriter)
						writer.Close();
					else
						writer.Flush();
				}
				catch (Exception)
				{
				}
				writer = null;
			}
			stream = null;
			return 0;
		}
		
		public short WriteStartElement (string Name)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				try
				{
					WriteIndentation();
					level++;
					writer.WriteStartElement(Name);
					StartTagFlag = true;
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
			return 0;
		}
		
		public short WriteEndElement()
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				level--;
				try
				{
					WriteIndentation();
					writer.WriteEndElement();
					writer.WriteString("\n");
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
			return 0;
		}

		private void FlushValueBuffer()
		{
			try
			{
				if (valueBuffer != null && writer != null && errorCode == 0)
				{
					string stringToSend = removeUnallowedChars( valueBuffer );
					if (stringToSend.Length > 0)
						writer.WriteString(stringToSend);
					level--;
					writer.WriteEndElement();
				}
				if (StartTagFlag) writer.WriteString("\n");
				StartTagFlag = false;
			}
			catch (Exception e)
			{
				errorCode = 1;
				errorDescription = e.Message;
			}
			valueBuffer = null;
		}

		string removeUnallowedChars( string s)
		{
			// Remove unsupported characters (it is documented not to send invalid characters).
			char[] toRemove = new char[] { '\x4', '\x1a' };
			string s1 = s;
			if (s1.IndexOfAny(toRemove) != -1)
			{
				foreach (char c in toRemove)
					s1 = s1.Replace( c, ' ');
			}
			return s1;
		}
		public short WriteElement (string Name, string Value)
		{
			WriteStartElement(Name);
			valueBuffer = Value;
			return 0;
		}

		public short WriteElement (string Name, object Value)
		{
			WriteStartElement(Name);
			valueBuffer = Value.ToString();
			return 0;
		}

		public short WriteElement (string Name)
		{
			WriteElement (Name, "");
			return 0;
		}

		public short WriteAttribute(string Name, string Value)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				try
				{
					writer.WriteAttributeString(Name, Value);
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
			return 0;
		}
		
		public short WriteText(string Text)
		{
			int i = 0;
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				try
				{
					writer.WriteString(Text);
					for (i = Text.Length - 1; i >= 0 && (Text[i] == '\t' || Text[i] == ' '); i-- );
					TextFlag = i < 0 || Text[i] != '\n';
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
			return 0;
		}

		public short WriteRawText(string Text)
		{
			int i = 0;
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				try
				{
					GXLogging.Debug(log, "WriteRaw , text: '" + Text + "'"); 
					writer.WriteRaw(Text);
					for (i = Text.Length - 1; i >= 0 && (Text[i] == '\t' || Text[i] == ' '); i--) ;
					TextFlag = i < 0 || Text[i] != '\n';
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
			return 0;
		}

		public void WriteComment(string Comment)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				try
				{
					WriteIndentation();
					writer.WriteComment(Comment);
					writer.WriteString("\n");
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteEntityReference(string ER)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				try
				{
					writer.WriteEntityRef(ER);
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteCData(string CData)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				try
				{
					int p, EndChar;
					string Fragment;

					for (p = 0; p >= 0; p = EndChar)
					{
						EndChar = CData.IndexOf("]]>", p);
						if (EndChar >= 0)
						{	
							Fragment = CData.Substring(p, EndChar - p + 1);
						}
						else
						{
							Fragment = CData.Substring(p);
						}
						WriteIndentation();
						writer.WriteCData(Fragment);
						writer.WriteString("\n");
						if (EndChar >= 0) EndChar++;
					}
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteProcessingInstruction(string Target, string Value)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				try
				{
					WriteIndentation();
					writer.WriteProcessingInstruction(Target, Value);
					writer.WriteString("\n");
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteProcessingInstruction(string Target)
		{
			WriteProcessingInstruction(Target, "");
		}


		Encoding getEncoding()
		{
			return encoding;
		}

		public string getEncodingName()
		{
			return strEncoding;
		}

        public void setEncoding(string enc)
        {
            try
            {
                encoding = GXUtil.GxIanaToNetEncoding(GXUtil.NormalizeEncodingName(enc), true);
                strEncoding = GXUtil.NormalizeEncodingName(enc);
            }
            catch
            {
                string encodingName = "ISO-8859-1";

                encoding = Encoding.GetEncoding(encodingName);
                strEncoding = encodingName;
            }
        }

		public void WriteStartDocument(string encod, int Standalone)
		{
			string Text = "";
			setEncoding( encod);
			checkStream();
			if (writer != null && errorCode == 0)
			{
				try
				{
                    Text = "<?xml version = \"1.0\" encoding = \"";
					Text += getEncodingName() + "\"";
					if (Standalone > 0) 
							Text += " standalone = \"yes\"";
					Text += "?>\n";
					writer.WriteRaw(Text);
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteStartDocument( string encod)
		{
			WriteStartDocument(encod, 0);
		}

		public void WriteStartDocument()
		{
			WriteStartDocument("", 0);
		}

		public void WriteDocType(string Name, string Subset)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				try
				{
					writer.WriteDocType(Name, null, null, (Subset.Length > 0) ? Subset : null );
					writer.WriteString("\n");
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteDocType(string Name)
		{
			WriteDocType(Name, "");
		}

		public void WriteDocTypeSystem(string Name, string uri, string Subset)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				try
				{
					writer.WriteDocType(Name, null, uri, (Subset.Length > 0) ? Subset : null );
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteDocTypeSystem(string Name, string uri)
		{
			WriteDocTypeSystem(Name, uri, "");
		}

		public void WriteDocTypePublic(string Name, string PubId, string uri, string Subset)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				try
				{
					writer.WriteDocType(Name, PubId, uri, (Subset.Length > 0) ? Subset : null );
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteDocTypePublic(string Name, string PubId, string uri)
		{
			WriteDocTypePublic(Name, PubId, uri, "");
		}

		private void WriteIndentation()
		{
			int i;

			if (TextFlag)
			{
				writer.WriteString("\n");
				TextFlag = false;
			}
			for (i = 0; i < level; i++)
				writer.WriteString(new string(indentChar[0], indentation));
		}

		public int Indentation
		{
			get
			{
				return indentation;
			}
			set
			{
				indentation = value;
			}
		}

		public string IndentChar
		{
			get
			{
				return indentChar;
			}
			set
			{
				if (value.Length == 1)
				{
					indentChar = value;
				}
			}
		}
		public short ErrCode
		{
			get
			{
				return errorCode;
			}
			
		}

		public string ErrDescription
		{
			get
			{
				return errorDescription;
			}
		}


		public void WriteNSStartElement (string Name, string Prefix, string NameSpaceURI)
		{
			checkStream();
			if (writer != null && errorCode == 0)
			{
				FlushValueBuffer();
				try
				{
					writer.WriteStartElement(Prefix, Name, NameSpaceURI);
					StartTagFlag = true;
					level++;
				}
				catch (Exception e)
				{
					errorCode = 1;
					errorDescription = e.Message;
				}
			}
		}

		public void WriteNSStartElement (string Name)
		{
			WriteNSStartElement (Name, "", "");
		}


		public void WriteNSElement (string Name, string NameSpaceURI, string Value)
		{
			checkStream();
			FlushValueBuffer();
			if (writer != null && errorCode == 0)
			{
				string prefix = writer.LookupPrefix(NameSpaceURI);
				if (prefix == null) prefix = "";
				WriteNSStartElement(Name, prefix, NameSpaceURI);
				valueBuffer = Value;
			}
		}

		public void WriteNSElement (string Name)
		{
			WriteNSElement (Name, "", "");
		}

		public void WriteNSElement (string Name, string NameSpaceURI)
		{
			WriteNSElement (Name, NameSpaceURI, "");
		}

		bool _xmlOutputStarted;
		
		public short XmlStart( )
		{
			if (_xmlOutputStarted )
				return 1;
			Open( (string) null);
			_xmlOutputStarted = true;
			return 0;
		}
		public short XmlStart( string FileName)
		{
			if (_xmlOutputStarted )
				return 1;
			Open( FileName);
			_xmlOutputStarted = true;
			return 0;
		}
		public short XmlEnd()
		{
			Close();
			if (! _xmlOutputStarted )
				return 1;
			_xmlOutputStarted = false;
			return 0;
		}
		public short XmlBeginElement( string ElementName)
		{
			if (! _xmlOutputStarted )
				return 1;
			WriteStartElement(ElementName);
			return 0;
		}
		public short XmlEndElement()
		{
			if ( level <= 0)
				return 2;
			if (! _xmlOutputStarted )
				return 1;
			WriteEndElement();
			return 0;
		}
		public short XmlValue(	string ValueName, string ElementValue)
		{
			if ( level <= 0)
				return 2;
			if (! _xmlOutputStarted )
				return 1;
			WriteElement( ValueName, ElementValue);
			return 0;

		}	
		public short XmlText( string TextValue)
		{
			if ( level <= 0)
				return 2;
			if (! _xmlOutputStarted )
				return 1;
			WriteText( TextValue);
			if (errorCode != 0)
				return 2;
			return 0;
		}
		public short XmlRaw( string RawText)
		{
			if ( level <= 0)
				return 2;
			if (! _xmlOutputStarted )
				return 1;
			WriteText( RawText);
			if (errorCode != 0)
				return 2;
			return 0;
		}
		public short XmlAtt( string AttriName, string AttriValue)
		{
			if ( level <= 0)
				return 2;
			if (! _xmlOutputStarted )
				return 1;
			WriteAttribute( AttriName, AttriValue);
			return 0;
		}

        #region IDisposable Members

        public void Dispose()
        {
            if (writer != null)
                this.Close();
        }

        #endregion
    }
	public class GxXslt
	{
		public static string Apply( string xml, string xslFileName)
		{
			return GxXsltImpl.ApplyToString(xml, xslFileName);
		}
	}
}
