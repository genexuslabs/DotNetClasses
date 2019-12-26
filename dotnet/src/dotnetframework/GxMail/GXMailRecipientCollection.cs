using System;
using System.Collections;

namespace GeneXus.Mail
{
	
	public class GXMailRecipientCollection : CollectionBase
	{
		public void Add(GXMailRecipient recipient)
		{
			List.Add(new GXMailRecipient(recipient.Name, recipient.Address));
		}

		public void New(string name, string address)
		{
			Add(new GXMailRecipient(name, address));
		}

		public GXMailRecipient Item(int index)
		{
			return (GXMailRecipient)List[index-1];
		}
	}
}
