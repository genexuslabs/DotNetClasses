using System.Collections.Generic;

namespace GeneXus.AI
{
	public class ChatMessage
	{
		public string Role { get; set; }
		public string Content { get; set; }
		public List<ToolCall> ToolCalls { get; set; }
		public string ToolCallId{ get; set; }
	}
	public class ToolCall
	{
		public string Id { get; set; }
		public string Type { get; set; }

		public Function Function { get; set; }
	}
	public class Function
	{
		public string Name { get; set; }
		public string Arguments { get; set; }
	}
}
