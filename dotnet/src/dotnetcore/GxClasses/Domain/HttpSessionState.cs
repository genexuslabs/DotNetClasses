using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.AspNetCore.Http;

namespace GeneXus.Http
{
	internal class HttpSessionState 
	{
		ISession session;

		public HttpSessionState(ISession session)
		{
			this.session = session;
		}

		public string SessionID { get { return session.Id; } internal set { } }
		
		public object this[string name] { get { return ByteArrayToObject(session.Get(name)); } set { session.Set(name, ObjectToByteArray(value)); } }

		private byte[] ObjectToByteArray(Object obj)
		{
			if (obj == null)
				return null;

			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, obj);

			return ms.ToArray();
		}

		// Convert a byte array to an Object
		private Object ByteArrayToObject(byte[] arrBytes)
		{
			if (arrBytes == null)
				return null;
			MemoryStream memStream = new MemoryStream();
			BinaryFormatter binForm = new BinaryFormatter();
			memStream.Write(arrBytes, 0, arrBytes.Length);
			memStream.Seek(0, SeekOrigin.Begin);
			Object obj = (Object)binForm.Deserialize(memStream);

			return obj;
		}
		internal void Remove(string key)
		{
			session.Remove(key);
		}

		internal void RemoveAll()
		{
			session.Clear();
		}

		internal void Clear()
		{
			session.Clear();
		}

		internal void Abandon()
		{
		}
	}
}