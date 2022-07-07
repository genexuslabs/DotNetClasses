using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeneXus.Application;
using Microsoft.AspNetCore.Http;

namespace GxClasses.Web
{
	public interface IGXRouting
	{
		public Task ProcessRestRequest(HttpContext context);
		public bool ServiceInPath(String path, out String actualPath);
		public GxRestWrapper GetController(HttpContext context, ControllerInfo controllerInfo);
		public GxRestWrapper GetController(HttpContext context, string controller, string methodName, Dictionary<string, string> variableAlias);
		public void ServicesGroupSetting();
		public void ServicesFunctionsMetadata();

	}
}
