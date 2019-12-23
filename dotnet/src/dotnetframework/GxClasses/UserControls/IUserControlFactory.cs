using GeneXus.Utils;

namespace GeneXus.UserControls
{
	public interface IUserControlFactory
	{
		string RenderControl(string controlType, string internalName, GxDictionary propbag);
	}
}