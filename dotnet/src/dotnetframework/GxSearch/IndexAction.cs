using System;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Search
{
	internal class IndexAction
	{
		private Action m_action;
		private IndexRecord m_record;

		internal IndexAction() { }

		internal IndexAction(Action action, IndexRecord record)
		{
			m_action = action;
			m_record = record;
		}

		public IndexRecord Record
		{
			get { return m_record; }
			set { m_record = value; }
		}

		public Action Action
		{
			get { return m_action; }
			set { m_action = value; }
		}
	}

	internal enum Action { Insert, Update, Delete };
}
