using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeneXus.Utils
{
	public class GxMessageFactory : IGxMessageFactory
	{
		static GxMessageFactory()
		{
			Dialogs.Message = new GxMessageFactory();
		}

		public IGxMessage GetMessageDialog()
		{
			return new GxMessage();
		}
	}
	public class GxMessage: IGxMessage
	{
		public void Show(string text, string caption)
		{
			MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}
	}
}
