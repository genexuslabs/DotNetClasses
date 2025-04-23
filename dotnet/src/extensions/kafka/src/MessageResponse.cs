namespace GeneXus.Messaging.Core
{
	public class MessageResponse
	{
		public string Key { set; get; }
		public string Value { set; get; }
		public string Topic { set; get; }
		public MessageResponseError Error { set; get; }
	}

	public class MessageResponseError
	{
		/// <summary>
		///     Initialize a new Error instance from a particular
		///     <see cref="ErrorCode"/> value.
		/// </summary>
		/// <param name="code">
		///     The <see cref="ErrorCode"/> value associated with this Error.
		/// </param>
		/// <remarks>
		///     The reason string associated with this Error will
		///     be a static value associated with the <see cref="ErrorCode"/>.
		/// </remarks>
		/// 
		public MessageResponseError()
		{

		}
		public MessageResponseError(int code)
		{
			Code = code;
			reason = null;
		}

		/// <summary>
		///     Initialize a new Error instance from a particular
		///     <see cref="ErrorCode"/> value and custom <paramref name="reason"/>
		///     string.
		/// </summary>
		/// <param name="code">
		///     The <see cref="ErrorCode"/> value associated with this Error.
		/// </param>
		/// <param name="reason">
		///     A custom reason string associated with the error
		///     (overriding the static string associated with 
		///     <paramref name="code"/>).
		/// </param>
		public MessageResponseError(int code, string reason)
		{
			Code = code;
			this.reason = reason;
		}

		/// <summary>
		///     Gets the <see cref="ErrorCode"/> associated with this Error.
		/// </summary>
		public int Code { get; }

		private string reason;

		/// <summary>
		///     Gets a human readable reason string associated with this error.
		/// </summary>
		public string Reason
		{
			get { return reason; }
		}

		/// <summary>
		///     true if Code != ErrorCode.NoError.
		/// </summary>
		public bool HasError
			=> Code != 0;

		/// <summary>
		///     true if this is error originated locally (within librdkafka), false otherwise.
		/// </summary>
		public bool IsLocalError
			=> (int)Code < -1;

		/// <summary>
		///     true if this error originated on a broker, false otherwise.
		/// </summary>
		public bool IsBrokerError
			=> (int)Code > 0;
	
	}
}
