using GeneXus.Application;
using GeneXus.Programs;
using GeneXus.UserControls;
using GeneXus.UserControls.Implementation;
using GeneXus.Utils;
using GeneXus.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;

namespace UnitTesting
{
	[TestClass]
	public class UserControlRender
	{
		[TestMethod]
		public void TestGenerateSimpleUC()
		{

			GXUserControl ucMycontrol1 = new GXUserControl();

			UserControlFactoryImpl factory = new UserControlFactoryImpl();
			GxDictionary propbag = new GxDictionary();

			string GetTemplateFile(string type)
			{
				return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestModule.MyControl.view");
			}
			var context = new GxContext();

			StringBuilder sb = new StringBuilder();
			context.OutputWriter = new HtmlTextWriter(new StringWriter(sb));
			UserControlGenerator.GetTemplateAction = GetTemplateFile;

			// Initialize Data in SDT Collection
			var AV6ItemCollection = new GXBaseCollection<SdtItem>(context, "Item", "testuc");
			string HELLO = "Hello Test UC";
			string ONE = "One";
			string TWO = "Two";
			var AV7Item = new SdtItem(context)
			{
				gxTpr_Name = ONE
			};
			AV6ItemCollection.Add(AV7Item, 0);
			AV7Item = new SdtItem(context)
			{
				gxTpr_Name = TWO
			};
			AV6ItemCollection.Add(AV7Item, 0);
			ucMycontrol1.SetProperty("Items", AV6ItemCollection);

			ucMycontrol1.SetProperty("Message", HELLO);
			ucMycontrol1.Render(context, "testmodule.mycontrol", "internalName", "MYCONTROL1Container");

			Assert.IsTrue(sb.ToString().Contains(ONE) && sb.ToString().Contains(TWO) && sb.ToString().Contains(HELLO));
		}

	}
}
