using System;
using System.IO;
using System.Text;

using log4net;

using Lucene.Net.Index;
using Lucene.Net.Store;
using SpellChecker.Net.Search.Spell;

namespace GeneXus.Search
{
	public sealed class Spelling
	{
		private static readonly ILog log = log4net.LogManager.GetLogger(typeof(GeneXus.Search.Indexer));
		#region Singleton

		private static Spelling m_instance = new Spelling();
		private Spelling() { }
		public static Spelling Instance { get { return m_instance; } }

		 #endregion

		public bool BuildDictionary()
		{
			try
			{
				IndexReader my_luceneReader = IndexReader.Open(Settings.Instance.StoreFolder, true);

				SpellChecker.Net.Search.Spell.SpellChecker spell = GetSpelling(true);
				if (spell != null)
					spell.IndexDictionary(new LuceneDictionary(my_luceneReader, IndexRecord.CONTENTFIELD));
				my_luceneReader.Dispose();

				return true;
			}
			catch (Exception ex)
			{
				GXLogging.Error(log,"BuildDictionary Error", ex);
				return false;
			}
		}
		public string Suggest(string phrase)
		{
			StringBuilder res = new StringBuilder();
			try
			{
				String[] words = phrase.Split(new char[] { ' ' });
				SpellChecker.Net.Search.Spell.SpellChecker spell = GetSpelling(false);
				if (spell != null)
				{
					for (int i = 0; i < words.Length; i++)
					{
						string[] similar = spell.SuggestSimilar(words[i], 1);
						if (similar != null && similar.Length > 0)
						{
							res.Append(similar[0]);
							if (i != words.Length - 1) res.Append(' ');
						}
					}
				}
			}
			catch (Exception ex)
			{
				GXLogging.Error(log,"Suggest Error", ex);
			}
			return res.ToString();

		}

#region Internal Operations

		internal SpellChecker.Net.Search.Spell.SpellChecker GetSpelling(bool build)
		{
			try
			{
				string dictionaryFolder = Path.Combine(Settings.Instance.IndexFolder, "Dictionary");
				if (!System.IO.Directory.Exists(dictionaryFolder))
					System.IO.Directory.CreateDirectory(dictionaryFolder);
				return new SpellChecker.Net.Search.Spell.SpellChecker(new Lucene.Net.Store.MMapDirectory(new DirectoryInfo(dictionaryFolder)));
			}
			catch (Exception)
			{
				return null;
			}
		}

#endregion
	}
}