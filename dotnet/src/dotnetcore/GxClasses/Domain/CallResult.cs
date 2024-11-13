using GeneXus.Application;

namespace GeneXus.Utils
{
	public class CallResult
	{
		public CallResult() { }
		public bool Success()
		{
			return true;
		}
		public bool Fail()
		{
			return false;
		}
		public msglist GetMessages()
		{
			return new msglist();
		}
		public string ToJson()
		{
			return "{}";
		}
	}
}
