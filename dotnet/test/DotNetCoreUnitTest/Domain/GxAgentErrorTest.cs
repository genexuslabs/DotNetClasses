using System.Text.Json;
using GeneXus.AI.Chat;
using GeneXus.Utils;
using Xunit;

namespace DotNetCoreUnitTest.Domain
{
	public class GxAgentErrorTest
	{
		[Fact]
		public void ErrorResponseSetsHasError()
		{
			string errorJson = @"{""error"":{""code"":500,""message"":""Oops! It looks like I'm not fully awake and can't process your request right now. Please try again later.""},""success"":false,""status"":""failed""}";

			ChatCompletionResult result = JsonSerializer.Deserialize<ChatCompletionResult>(errorJson);

			Assert.NotNull(result);
			Assert.True(result.HasError);
			Assert.NotNull(result.Error);
			Assert.Equal(500, result.Error.Code);
			Assert.Contains("not fully awake", result.ErrorMessage, System.StringComparison.OrdinalIgnoreCase);
			Assert.Null(result.Choices);
		}

		[Fact]
		public void SuccessResponseDoesNotSetHasError()
		{
			string successJson = @"{""id"":""chatcmpl-123"",""choices"":[{""index"":0,""message"":{""role"":""assistant"",""content"":""Hello!""},""finish_reason"":""stop""}]}";

			ChatCompletionResult result = JsonSerializer.Deserialize<ChatCompletionResult>(successJson);

			Assert.NotNull(result);
			Assert.False(result.HasError);
			Assert.Null(result.Error);
			Assert.NotNull(result.Choices);
			Assert.Single(result.Choices);
		}

		[Fact]
		public void SuccessFalseWithoutErrorObjectSetsHasError()
		{
			string json = @"{""success"":false,""status"":""failed""}";

			ChatCompletionResult result = JsonSerializer.Deserialize<ChatCompletionResult>(json);

			Assert.NotNull(result);
			Assert.True(result.HasError);
			Assert.Contains("failed", result.ErrorMessage, System.StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void CallResultReflectsFailState()
		{
			CallResult callResult = new CallResult();

			Assert.True(callResult.Success());
			Assert.False(callResult.Fail());

			callResult.IsFail = true;
			callResult.AddMessage("Test error");

			Assert.False(callResult.Success());
			Assert.True(callResult.Fail());
		}
	}
}
