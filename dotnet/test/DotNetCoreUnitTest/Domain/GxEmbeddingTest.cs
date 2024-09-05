using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneXus.Utils;
using NPOI.SS.Formula.Functions;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class GxEmbeddingTest
	{
		//[Fact]
		public async Task EmbeddingTest()
		{
			EmbeddingService embeddingService = EmbeddingService.Instance;
			var embedding = await embeddingService.GenerateEmbeddingAsync("openai/text-embedding-3-small", "Hello World");
			Assert.NotNull(embedding);
		}
	}
}
