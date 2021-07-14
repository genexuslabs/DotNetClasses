using System;
using System.Threading.Tasks;
using GeneXus.Application;
using Microsoft.AspNetCore.Http;

namespace GxClasses.Web
{
	public interface IGXRouting
	{
		public Task ProcessRestRequest(HttpContext context);
		public bool ServiceInPath(String path, out String actualPath);
		public GxRestWrapper GetController(HttpContext context, string controller, string methodName);
		public void ServicesGroupSetting();
		public void ServicesFunctionsMetadata();

	}
}
