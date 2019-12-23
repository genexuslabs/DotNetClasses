using System;
using System.Collections.Generic;
using System.Text;

namespace GeneXus.Search
{
    internal class IndexRecord
    {
        internal const string URIFIELD = "URI";
        internal const string VIEWERFIELD = "VIEWER";
        internal const string TITLEFIELD = "TITLE";
        public static string CONTENTFIELD = "CONTENT";
        internal const string ENTITYFIELD = "ENTITY";
        internal const string TIMESTAMPFIELD = "TIME";
        internal const string KEYFIELDPREFIX = "KEY";

        private string m_uri;
        private string m_entity;
        private string m_content;
        private string m_viewer;
        private string m_title;
        private IList<string> m_keys = new List<string>();

        public IndexRecord() { }
        public IndexRecord(string URI) : this(URI, string.Empty, string.Empty, string.Empty, string.Empty, new List<string>()) { }
        public IndexRecord(string URI, string entity, string content, string title, string viewer, IList<string> keys)
        {
            m_uri = URI;
            m_entity = entity;
            m_content = content;
            m_keys = keys;
            m_title = title;
            m_viewer = viewer;
        }

        public IList<string> Keys
        {
            get { return m_keys; }
            set { m_keys = value; }
        }

        public string Content
        {
            get { return m_content; }
            set { m_content = value; }
        }

        public string Entity
        {
            get { return m_entity; }
            set { m_entity = value; }
        }

        public string Uri
        {
            get { return m_uri; }
            set { m_uri = value; }
        }

        public string Viewer
        {
            get { return m_viewer; }
            set { m_viewer = value; }
        }

        public string Title
        {
            get { return m_title; }
            set { m_title = value; }
        }
        public static string ProcessContent(string content)
        {
            if (Settings.Instance.Analyzer == Settings.WhitespaceAnalyzer)
            {
                return content.ToLowerInvariant();
            }
            else
                return content;
        }
    }
}
