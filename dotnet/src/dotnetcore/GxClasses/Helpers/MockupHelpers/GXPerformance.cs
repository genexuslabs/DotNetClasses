using GeneXus.Data;
using GeneXus.Data.NTier;
using System;

namespace GeneXus.Performance
{
	public class WMIDataStoreProviders
	{
		public static WMIDataStoreProviders Instance()
		{
			throw new NotImplementedException();
		}
		public WMIDataStoreProvider AddDataStoreProvider(string datastoreName)
		{
			throw new NotImplementedException();
		}
	}
	public class WMIDataStoreProvider
	{
		public WMIDataStoreProvider(string datastoreName)
		{
		}

		internal void IncSentencesCount(ICursor oCur)
		{
			throw new NotImplementedException();
		}

		internal void BeginExecute(ICursor oCur, IGxConnection connection)
		{
			throw new NotImplementedException();
		}

		internal void EndExecute(ICursor oCur, IGxConnection connection)
		{
			throw new NotImplementedException();
		}
	}

}
