using GeneXus.Application;
using GeneXus.Programs;
using GeneXus.UserControls;
using GeneXus.UserControls.Implementation;
using GeneXus.Utils;
using GeneXus.WebControls;
#if !NETCORE
using System.Web.UI;
#endif
using Xunit;
using System.IO;
using System.Reflection;
using System.Text;

namespace UnitTesting
{
	public class UserControlRender
	{
		[Fact]
		public void GenerateSimpleUC()
		{
			GXUserControl ucMycontrol1 = new GXUserControl();

			UserControlFactoryImpl factory = new UserControlFactoryImpl();
			GxDictionary propbag = new GxDictionary();

			string GetTemplateFile(string type)
			{
				return "TestModule.MyControl.view";
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
			ucMycontrol1.SetProperty("Boolean", true);
			ucMycontrol1.Render(context, "testmodule.mycontrol", "internalName", "MYCONTROL1Container");

			string output = sb.ToString();

			Assert.Contains(ONE, output, System.StringComparison.InvariantCulture);
			Assert.Contains(TWO, output, System.StringComparison.InvariantCulture);
			Assert.Contains(HELLO, output, System.StringComparison.InvariantCulture);
			Assert.Contains("Boolean:true", output, System.StringComparison.InvariantCulture);
		}

	}
}
