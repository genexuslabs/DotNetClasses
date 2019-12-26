using System.Collections.Generic;

namespace GeneXus.WebControls
{
	public class ListControl : WebControl
	{
		public List<ListItem> Items { get; internal set; }
		public int SelectedIndex { get; internal set; }
	}
}