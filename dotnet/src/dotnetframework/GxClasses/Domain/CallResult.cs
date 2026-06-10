using GeneXus.Application;

namespace GeneXus.Utils
{
	public class CallResult
	{
		msglist m_Messages;
		public CallResult()
		{
			m_Messages = new msglist(); 
		}
		internal bool IsFail { get; set; }

		public bool Success()
		{
			return !IsFail;
		}
		public bool Fail()
		{
			return IsFail;
		}
		public msglist GetMessages()
		{
			return m_Messages;
		}
		public string ToJson()
		{
			return m_Messages.ToJSonString();
		}

		internal void AddMessage(string v)
		{
			m_Messages.Add(v);
		}
	}
}
