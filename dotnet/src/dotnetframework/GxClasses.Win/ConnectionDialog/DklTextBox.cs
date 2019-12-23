using System;
using System.Windows.Forms;

namespace ConnectionBuilder
{
	/// <summary>
	/// Summary description for DklTextBox.
	/// </summary>
	public class DklTextBox : TextBox
	{
		public DklTextBox()
		{
			//
			// TODO: Add constructor logic here
			//
			this.Enter += new System.EventHandler(this.OnGotFocus);
		}

		private void OnGotFocus(object sender, System.EventArgs e)
		{
			this.Select(0, Text.Length);
		}

	}
}
