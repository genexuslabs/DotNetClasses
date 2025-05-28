using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GeneXus.AI.Chat
{
	public class ChatMessage
	{
		[JsonPropertyName("role")]
		public string Role { get; set; }

		[JsonPropertyName("content")]
		public string Content { get; set; }

		[JsonPropertyName("tool_calls")]
		public List<ToolCall> ToolCalls { get; set; }

		[JsonPropertyName("tool_call_id")]
		public string ToolCallId { get; set; }

	}
	public class ToolCall
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("type")]
		public string Type { get; set; }

		[JsonPropertyName("function")]
		public Function Function { get; set; }

	}
	public class Function
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("arguments")]
		public string Arguments { get; set; }
	}
}
