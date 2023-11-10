// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 2.1 of the License, or (at your option)
// any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
using System;
using System.Collections;
using System.Collections.Specialized;
using GeneXus.Utils;
namespace GeneXus.Application
{
	/// the implementation does internally try to remember the order in which 
	/// the keys were added in order facilitate human-readability as in when
	/// an instance is rendered as text.</para>

	[ Serializable ]
    internal class JObject : OrderedDictionary, IJayrockCompatible
	{
        public JObject() {}

        /// <summary>
        /// Construct a JObject from a IDictionary
        /// </summary>

        public JObject(IDictionary members)
        {
            foreach (DictionaryEntry entry in members)
            {
                if (entry.Key == null)
                    throw new Exception("InvalidMemberException");

				base.Add(entry.Key.ToString(), entry.Value);
            }

         }

        public virtual bool HasMembers
        {
            get { return Count > 0; }
        }


        /// <summary>
        /// Accumulate values under a key. It is similar to the Put method except
        /// that if there is already an object stored under the key then a
        /// JArray is stored under the key to hold all of the accumulated values.
        /// If there is already a JArray, then the new value is appended to it.
        /// In contrast, the Put method replaces the previous value.
        /// </summary>

        public virtual JObject Accumulate(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            object current = base[name];

            if (current == null)
            {
                Put(name, value);
            }
            else 
            {
                IList values = current as IList;
                
                if (values != null)
                {
                    values.Add(value);
                }
                else
                {
					values = new JArray
					{
						current,
						value
					};
					Put(name, values);
                }
            }

            return this;
        }

        /// <summary>
        /// Put a key/value pair in the JObject. If the value is null,
        /// then the key will be removed from the JObject if it is present.
        /// </summary>

        public virtual JObject Put(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (value != null)
                this[name] = value;
            else 
                Remove(name);
			
            return this;
        }

        public virtual ICollection Names
        {
            get
            {
				return base.Keys;
            }
        }

        /// <summary>
        /// Overridden to return a JSON formatted object as a string.
        /// </summary>
        
        public override string ToString()
        {
			return TextJsonSerializer.SerializeToJayrockCompatibleJson(this);
		}


    }
}
