using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GeneXus.Programs
{
	public class RequestParameters
    {
        public string message { get; set; }
    }

    [ApiController]
    [Route("dummy.aspx")]

    public class AccessTokenControllerDummy : ControllerBase
    {
		[HttpPost]
		public async Task<ActionResult<string>> Post([FromForm] string message)
		{
			return await Task.FromResult(message);
		}
	}

}