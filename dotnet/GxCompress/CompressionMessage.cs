namespace Genexus.Compression
{
	public class CompressionMessage
	{
		private readonly bool successfulOperation;
		private readonly string message;

		public CompressionMessage(bool successfulOperation, string message)
		{
			this.successfulOperation = successfulOperation;
			this.message = message;
		}

		public bool IsSuccessfulOperation
		{
			get { return successfulOperation; }
		}

		public string Message
		{
			get { return message; }
		}
	}
}
