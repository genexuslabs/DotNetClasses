using System;

namespace GeneXus.Mail
{
	
	public class GXMailRecipient
	{
		private string address;
		private string name;

		public GXMailRecipient()
		{
			address = string.Empty;
			name = string.Empty;
		}

		public GXMailRecipient(string name, string address)
		{
			this.name = name;
			this.address = address;
		}

		public string Address
		{
			get
			{
				return address;
			}
			set
			{
				address = value;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public string FullName
		{
			get
			{
				return name + " <" + address + ">";
			}
		}

	}
}
