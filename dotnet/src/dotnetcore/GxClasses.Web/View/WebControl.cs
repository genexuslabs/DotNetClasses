using GeneXus.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GeneXus.WebControls
{
	public class WebControl
	{
		public string ID { get; internal set; }
		public Color ForeColor { get; internal set; }
		public Color BackColor { get; internal set; }
		public Unit BorderWidth { get; internal set; }
		public Unit Width { get; internal set; }
		public Unit Height { get; internal set; }
		public FontInfo Font { get; internal set; }
		public bool Enabled { get; internal set; }
		public bool Visible { get; internal set; }
		public string ToolTip { get; internal set; }
		public string CssClass { get; internal set; }
		public Dictionary<string, string> Attributes { get; internal set; }
		public void RenderControl(HtmlTextWriter controlOutputWriter)
		{
		}
		public string Text { get; internal set; }
		public Dictionary<string,string> Style { get; set; }
	}
	public class TextBox : WebControl
	{
		public int MaxLength { get; internal set; }
		public int Columns { get; internal set; }
		public object TextMode { get; internal set; }
		public int Rows { get; internal set; }
	}
	public enum TextBoxMode
	{
		//
		// Summary:
		//     Represents single-line entry mode.
		SingleLine = 0,
		//
		// Summary:
		//     Represents multiline entry mode.
		MultiLine = 1,
		//
		// Summary:
		//     Represents password entry mode.
		Password = 2,
		//
		// Summary:
		//     Represents color entry mode.
		Color = 3,
		//
		// Summary:
		//     Represents date entry mode.
		Date = 4,
		//
		// Summary:
		//     Represents date-time entry mode.
		DateTime = 5,
		//
		// Summary:
		//     Represents local date-time entry mode.
		DateTimeLocal = 6,
		//
		// Summary:
		//     Represents email address entry mode.
		Email = 7,
		//
		// Summary:
		//     Represents month entry mode.
		Month = 8,
		//
		// Summary:
		//     Represents number entry mode.
		Number = 9,
		//
		// Summary:
		//     Represents numeric range entry mode.
		Range = 10,
		//
		// Summary:
		//     Represents search string entry mode.
		Search = 11,
		//
		// Summary:
		//     Represents phone number entry mode.
		Phone = 12,
		//
		// Summary:
		//     Represents time entry mode.
		Time = 13,
		//
		// Summary:
		//     Represents URL entry mode.
		Url = 14,
		//
		// Summary:
		//     Represents week entry mode.
		Week = 15
	}
	public class HyperLink : WebControl
	{
		public string NavigateUrl { get; internal set; }
		public string Target { get; internal set; }
	}
	public class LiteralControl
	{
		public string ID { get; internal set; }
		public string Text { get; internal set; }
		public bool Visible { get; internal set; }

		internal void RenderControl(HtmlTextWriter controlOutputWriter)
		{
			throw new NotImplementedException();
		}
	}
}