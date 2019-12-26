#if !NETCORE
using GeneXus.UserControls.Implementation;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneXus.UserControls	
{
	public class UserControlFactory
	{
#if !NETCORE
		public static IUserControlFactory Instance = new UserControlFactoryImpl();
#else
		public static IUserControlFactory Instance;
#endif

	}
}
