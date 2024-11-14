using System;
using System.Threading.Tasks;
using GeneXus.AI;
using GeneXus.Utils;
using OpenAI.Chat;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class GxEmbeddingTest
	{
		[Fact(Skip ="Local test")]
		public async Task EmbeddingTest()
		{
			IEmbeddingService embeddingService = AIEmbeddingFactory.Instance;
			ReadOnlyMemory<float> embedding = await embeddingService.GenerateEmbeddingAsync("openai/text-embedding-3-small", 512, "Hello World");
			Assert.False(embedding.IsEmpty);
		}
		[Fact(Skip ="Local test")]
		public async Task AssistantTest()
		{
			AgentService agentService = AgentService.AgentHandlerInstance;
			string userMessage =  "What's the weather like in Buenos Aires today?";
			string modelId = "e4e7a837-b8ad-4d25-b2db-431dda9af0af";
			GXProperties properties = new GXProperties();
			properties.Set("$context", "context for reference");
			ChatCompletion embedding = await agentService.Assistant(modelId, userMessage, properties);
			Assert.NotNull(embedding.Content);
		}
	}
}
