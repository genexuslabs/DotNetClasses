using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using GeneXus.AI.Chat;
using GeneXus.Application;
using GeneXus.Utils;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class ChatMessageTest
	{
		[Fact]
		public void ChatMessageCollectionTest()
		{
			GxContext gxContext = new GxContext();
			GXExternalCollection<SdtChatMessage> chatMessages = new GXExternalCollection<SdtChatMessage>(gxContext, "SdtChatMessage", "GeneXus.Programs");
			ChatMessage msg = new ChatMessage();
			msg.Role = "user";
			chatMessages.Add(msg);
			Assert.NotEmpty(chatMessages.ToJSonString());
		}


	}
	[Serializable]
	public class SdtChatMessage : GxUserType, IGxExternalObject
	{
		private static Hashtable mapper;
		[XmlIgnore]
		private static GXTypeInfo _typeProps;
		protected ChatMessage GeneXus_ArtificialIntelligence_ChatMessage_externalReference;

		public SdtChatMessage()
		{
		}

		public SdtChatMessage(IGxContext context)
		{
			this.context = context;
			this.initialize();
		}

		public override string JsonMap(string value)
		{
			if (SdtChatMessage.mapper == null)
				SdtChatMessage.mapper = new Hashtable();
			return (string)SdtChatMessage.mapper[(object)value];
		}

		public string gxTpr_Role
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				return this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference.Role;
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference.Role = value;
				this.SetDirty("Role");
			}
		}

		public string gxTpr_Content
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				return this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference.Content;
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference.Content = value;
				this.SetDirty("Content");
			}
		}

		public GXExternalCollection<SdtToolCall> gxTpr_Toolcalls
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				return new GXExternalCollection<SdtToolCall>(this.context, "GeneXus.Core.genexus.artificialintelligence.SdtToolCall", "GeneXus.Core")
				{
					ExternalInstance = (IList)CollectionUtils.ConvertToInternal(typeof(List<ToolCall>), (object)this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference.ToolCalls)
				};
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference.ToolCalls = (List<ToolCall>)CollectionUtils.ConvertToExternal(typeof(List<ToolCall>), (object)value.ExternalInstance);
				this.SetDirty("Toolcalls");
			}
		}

		public string gxTpr_Toolcallid
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				return this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference.ToolCallId;
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference.ToolCallId = value;
				this.SetDirty("Toolcallid");
			}
		}

		public object ExternalInstance
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = new ChatMessage();
				return (object)this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference;
			}
			set
			{
				this.GeneXus_ArtificialIntelligence_ChatMessage_externalReference = (ChatMessage)value;
			}
		}

		protected override GXTypeInfo TypeInfo
		{
			get => SdtChatMessage._typeProps;
			set => SdtChatMessage._typeProps = value;
		}

		public void initialize()
		{
		}
	}

	[Serializable]
	public class SdtToolCall : GxUserType, IGxExternalObject
	{
		private static Hashtable mapper;
		[XmlIgnore]
		private static GXTypeInfo _typeProps;
		protected ToolCall GeneXus_ArtificialIntelligence_ToolCall_externalReference;

		public SdtToolCall()
		{
		}

		public SdtToolCall(IGxContext context)
		{
			this.context = context;
			this.initialize();
		}

		public override string JsonMap(string value)
		{
			if (SdtToolCall.mapper == null)
				SdtToolCall.mapper = new Hashtable();
			return (string)SdtToolCall.mapper[(object)value];
		}

		public string gxTpr_Id
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ToolCall_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ToolCall_externalReference = new ToolCall();
				return this.GeneXus_ArtificialIntelligence_ToolCall_externalReference.Id;
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_ToolCall_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ToolCall_externalReference = new ToolCall();
				this.GeneXus_ArtificialIntelligence_ToolCall_externalReference.Id = value;
				this.SetDirty("Id");
			}
		}

		public string gxTpr_Type
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ToolCall_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ToolCall_externalReference = new ToolCall();
				return this.GeneXus_ArtificialIntelligence_ToolCall_externalReference.Type;
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_ToolCall_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ToolCall_externalReference = new ToolCall();
				this.GeneXus_ArtificialIntelligence_ToolCall_externalReference.Type = value;
				this.SetDirty("Type");
			}
		}

		public SdtFunction gxTpr_Function
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ToolCall_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ToolCall_externalReference = new ToolCall();
				return new SdtFunction(this.context)
				{
					ExternalInstance = (object)this.GeneXus_ArtificialIntelligence_ToolCall_externalReference.Function
				};
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_ToolCall_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ToolCall_externalReference = new ToolCall();
				this.GeneXus_ArtificialIntelligence_ToolCall_externalReference.Function = (Function)value.ExternalInstance;
				this.SetDirty("Function");
			}
		}

		public object ExternalInstance
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_ToolCall_externalReference == null)
					this.GeneXus_ArtificialIntelligence_ToolCall_externalReference = new ToolCall();
				return (object)this.GeneXus_ArtificialIntelligence_ToolCall_externalReference;
			}
			set => this.GeneXus_ArtificialIntelligence_ToolCall_externalReference = (ToolCall)value;
		}

		protected override GXTypeInfo TypeInfo
		{
			get => SdtToolCall._typeProps;
			set => SdtToolCall._typeProps = value;
		}

		public void initialize()
		{
		}
	}
	[Serializable]
	public class SdtFunction : GxUserType, IGxExternalObject
	{
		private static Hashtable mapper;
		[XmlIgnore]
		private static GXTypeInfo _typeProps;
		protected Function GeneXus_ArtificialIntelligence_Function_externalReference;

		public SdtFunction()
		{
		}

		public SdtFunction(IGxContext context)
		{
			this.context = context;
			this.initialize();
		}

		public override string JsonMap(string value)
		{
			if (SdtFunction.mapper == null)
				SdtFunction.mapper = new Hashtable();
			return (string)SdtFunction.mapper[(object)value];
		}

		public string gxTpr_Name
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_Function_externalReference == null)
					this.GeneXus_ArtificialIntelligence_Function_externalReference = new Function();
				return this.GeneXus_ArtificialIntelligence_Function_externalReference.Name;
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_Function_externalReference == null)
					this.GeneXus_ArtificialIntelligence_Function_externalReference = new Function();
				this.GeneXus_ArtificialIntelligence_Function_externalReference.Name = value;
				this.SetDirty("Name");
			}
		}

		public string gxTpr_Arguments
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_Function_externalReference == null)
					this.GeneXus_ArtificialIntelligence_Function_externalReference = new Function();
				return this.GeneXus_ArtificialIntelligence_Function_externalReference.Arguments;
			}
			set
			{
				if (this.GeneXus_ArtificialIntelligence_Function_externalReference == null)
					this.GeneXus_ArtificialIntelligence_Function_externalReference = new Function();
				this.GeneXus_ArtificialIntelligence_Function_externalReference.Arguments = value;
				this.SetDirty("Arguments");
			}
		}

		public object ExternalInstance
		{
			get
			{
				if (this.GeneXus_ArtificialIntelligence_Function_externalReference == null)
					this.GeneXus_ArtificialIntelligence_Function_externalReference = new Function();
				return (object)this.GeneXus_ArtificialIntelligence_Function_externalReference;
			}
			set => this.GeneXus_ArtificialIntelligence_Function_externalReference = (Function)value;
		}

		protected override GXTypeInfo TypeInfo
		{
			get => SdtFunction._typeProps;
			set => SdtFunction._typeProps = value;
		}

		public void initialize()
		{
		}
	}

}
