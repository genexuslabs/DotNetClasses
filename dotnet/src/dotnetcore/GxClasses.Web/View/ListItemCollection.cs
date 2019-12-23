using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace GeneXus.WebControls
{
	public class ListItemCollection : List<ListItem>
	{
		public ListItemCollection()
		{
		}

		internal ListItem FindByValue(string key)
		{
			return base.Find(x => x.Value == key);
		}
	}
	public class ListItem
	{
		public ListItem(string text, string value)
		{
			Text = text;
			Value = value;
		}

		[DefaultValue(true)]
		public bool Enabled { get; set; }
		//
		// Summary:
		//     Gets or sets a value indicating whether the item is selected.
		//
		// Returns:
		//     true if the item is selected; otherwise, false. The default is false.
		[DefaultValue(false)]
		public bool Selected { get; set; }
		//
		// Summary:
		//     Gets or sets the text displayed in a list control for the item represented by
		//     the System.Web.UI.WebControls.ListItem.
		//
		// Returns:
		//     The text displayed in a list control for the item represented by the System.Web.UI.WebControls.ListItem
		//     control. The default value is System.String.Empty.
		[DefaultValue("")]
		[Localizable(true)]
		public string Text { get; set; }
		//
		// Summary:
		//     Gets or sets the value associated with the System.Web.UI.WebControls.ListItem.
		//
		// Returns:
		//     The value associated with the System.Web.UI.WebControls.ListItem. The default
		//     is System.String.Empty.
		[DefaultValue("")]
		[Localizable(true)]
		public string Value { get; set; }
	}
}