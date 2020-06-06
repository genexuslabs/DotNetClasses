using GeneXus.UserControls.Implementation;

namespace GeneXus.UserControls	
{
	public class UserControlFactory
	{

		public static IUserControlFactory Instance = new UserControlFactoryImpl();

	}
}
