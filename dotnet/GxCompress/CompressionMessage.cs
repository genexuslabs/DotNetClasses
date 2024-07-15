namespace Genexus.Compression
{
	public class CompressionMessage
	{
		private bool successfulOperation;
		private string message;

		public CompressionMessage(bool successfulOperation, string message)
		{
			this.successfulOperation = successfulOperation;
			this.message = message;
		}

		public bool IsSuccessfulOperation
		{
			get { return successfulOperation; }
			set { successfulOperation = value; }
		}

		public string Message
		{
			get { return message; }
			set { message = value; }
		}
	}
}
