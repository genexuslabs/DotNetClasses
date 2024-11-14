using System;
using System.Threading.Tasks;
using GeneXus.Utils;
using NPOI.SS.Formula.Functions;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class GxEmbeddingTest
	{
		[Fact(Skip ="Local test")]
		public async Task EmbeddingTest()
		{
			IEmbeddingService embeddingService = AIEmbeddingactory.Instance;
			ReadOnlyMemory<float> embedding = await embeddingService.GenerateEmbeddingAsync("openai/text-embedding-3-small", 512, "Hello World");
			Assert.False(embedding.IsEmpty);
		}
		/*[Fact]
		public async Task AssistantTest()
		{
			EmbeddingService embeddingService = EmbeddingService.Instance;
			string userMessage =  "What's the weather like in Buenos Aires today?";
			string context = "context for reference";
			var embedding = await embeddingService.Assistant(userMessage, context);
			Assert.NotNull(embedding);
		}
		*/
	}
}
