using GeneXus.Utils;
using System;


namespace GeneXus.Application
{
	public class msglist : GXBaseCollection<msglistItem>
	{
		const short MESSAGE_TYPE_ERROR = 1;
		const short MESSAGE_TYPE_WARNING = 0;
		const string MESSAGE_SUCADDED = "SuccessfullyAdded";
		const string MESSAGE_UPDATED = "SuccessfullyUpdated";
		const string MESSAGE_DELETED = "SuccessfullyDeleted";

		short _displayMode;
		public msglist()
			: base()
		{
			_containedName = "MessageList";
			_containedXmlNamespace = "GeneXus.Utils";
		}
		public msglist(string containedName, string containedXmlNamespace, string containedType, string containedTypeNamespace)
			: base()
		{
			_containedName = containedName;
			_containedXmlNamespace = containedXmlNamespace;
		}
		public void Append(GXBaseCollection<SdtMessages_Message> list)
		{
			foreach (SdtMessages_Message item in list)
			{
				if (!(item.gxTpr_Type == MESSAGE_TYPE_WARNING && (item.gxTpr_Id == MESSAGE_SUCADDED || item.gxTpr_Id == MESSAGE_UPDATED || item.gxTpr_Id == MESSAGE_DELETED)))
					addItem(item.gxTpr_Description, item.gxTpr_Type, item.gxTpr_Id);
			}
		}
		public short AnyError()
		{
			foreach (msglistItem item in this)
			{
				if (item.gxTpr_Type == MESSAGE_TYPE_ERROR)
					return MESSAGE_TYPE_ERROR;
			}
			return MESSAGE_TYPE_WARNING;
		}
		public void addItem(string s)
		{
			Add(new msglistItem("", s, 0, "", false));
		}
		public void addItem(string s, bool gxMessage)
		{
			Add(new msglistItem("", s, 0, "", gxMessage));
		}
		public void addItem(string s, short type, string att)
		{
			Add(new msglistItem("", s, type, att, false));
		}
		public void addItem(string s, short type, string att, bool gxMessage)
		{
			Add(new msglistItem("", s, type, att, gxMessage));
		}
		public void addItem(string s, string id, short type, string att)
		{
			Add(new msglistItem(id, s, type, att, false));
		}
		public void addItem(string s, string id, short type, string att, bool gxMessage)
		{
			Add(new msglistItem(id, s, type, att, gxMessage));
		}
		public int ItemCount
		{
			get
			{
				return Count;
			}
		}
		public int getItemCount()
		{
			return Count;
		}
		public String getItemAtt(int i)
		{
			return this[i - 1].att;
		}
		public String getItemText(int i)
		{
			return this[i - 1].gxTpr_Description;
		}
		public String getItemValue(int i)
		{
			return this[i - 1].gxTpr_Id;
		}
		public short getItemType(int i)
		{
			return this[i - 1].gxTpr_Type;
		}
		public void removeAllItems()
		{
			Clear();
		}
		public short DisplayMode
		{
			get { return _displayMode; }
			set { _displayMode = value; }
		}
	}
	public class msglistItem : GxUserType
	{
		string _id = "";
		string _description = "";
		short _type = -1;
		string _att = "";
		public msglistItem()
		{
		}
		public msglistItem(string id, string description, short type, string att, bool gxMessage)
		{
			_id = id;
			_description = description;
			_type = type;
			_att = att;
			IsGxMessage = gxMessage;
		}

		public bool IsGxMessage { get; set; }

		public string gxTpr_Id
		{
			get { return _id; }
			set { _id = value; }
		}
		public string gxTpr_Description
		{
			get { return _description; }
			set { _description = value; }
		}
		public short gxTpr_Type
		{
			get { return _type; }
			set { _type = value; }
		}
		public string att
		{
			get { return _att; }
		}
		public override void ToJSON()
		{
			AddObjectProperty("id", gxTpr_Id);
			AddObjectProperty("text", gxTpr_Description);
			AddObjectProperty("type", gxTpr_Type);
			AddObjectProperty("att", att);
		}
	}
}