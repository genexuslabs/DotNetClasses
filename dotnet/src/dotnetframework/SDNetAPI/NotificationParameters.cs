using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json;

namespace Artech.Genexus.SDAPI
{
    public class NotificationParameters
    {
        private Dictionary<string, string> m_data = new Dictionary<string, string>();

        public void Add(string name, string value)
        {
            lock(m_data)
                m_data[name] = value;
        }

        public List<string> Names { get { lock(m_data) return new List<string>(m_data.Keys); } }

        public string ValueOf(string name)
        {
            lock(m_data)
                if (m_data.ContainsKey(name))
                    return m_data[name];

            return string.Empty;
        }

        public void SetParameter(string name, string value)
        {
            m_data[name] = value;
        }

		internal JObject ToJObject()
		{
			JObject ar = new JObject();
			foreach (KeyValuePair<string, string> p in m_data)
			{
				ar.Put(p.Key, p.Value);
			}
			return ar;
		}

		internal int Count
		{
			get{
				return m_data.Count;
			}
		}

		internal string ToJson()
        {
            List<string> parms = new List<string>();
            foreach (string k in m_data.Keys)
                parms.Add(string.Format("\"{0}\":\"{1}\"", k, m_data[k]));

            return "[{" + string.Join(",", parms.ToArray()) + "}]";
        }
                
    }
}
