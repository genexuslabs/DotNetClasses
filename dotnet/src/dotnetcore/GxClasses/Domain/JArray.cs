#region License, Terms and Conditions
//
// Jayrock - A JSON-RPC implementation for the Microsoft .NET Framework
// Written by Atif Aziz (atif.aziz@skybow.com)
// Copyright (c) Atif Aziz. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 2.1 of the License, or (at your option)
// any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
// details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation, Inc.,
// 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA 
//
#endregion

namespace GeneXus.Application
{
    #region Imports

    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
	using GeneXus.Utils;

	#endregion

	/// <summary>
	/// An ordered sequence of values. This class also provides a number of
	/// methods that can be found on a JavaScript Array for sake of parity.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Public Domain 2002 JSON.org, ported to C# by Are Bjolseth (teleplan.no)
	/// and re-adapted by Atif Aziz (www.raboof.com)</para>
	/// </remarks>

	[ Serializable ]
    internal class JArray : CollectionBase
    {
        public JArray() {}

        public JArray(IEnumerable collection)
        {
            foreach (object item in collection)
                List.Add(item);
        }

        public virtual object this[int index]
        {
            get { return InnerList[index]; }
            set { List[index] = value; }
        }

        public int Length
        {
            get { return Count; }
        }

        public JArray Put(object value)
        {
            Add(value);
            return this;
        }

        public virtual void Add(object value)
        {
            List.Add(value);
        }
        public virtual void Add(int index, object value)
        {
            InnerList.Insert(index, value);
        }

        public virtual void Remove(object value)
        {
            List.Remove(value);
        }

        public virtual bool Contains(object value)
        {
            return List.Contains(value);
        }

        public virtual int IndexOf(object value)
        {
            return List.IndexOf(value);
        }

        public virtual bool HasValueAt(int index)
        {
            return this[index] != null;
        }

        public virtual object GetValue(int index)
        {
            return GetValue(index, null);
        }

        public virtual object GetValue(int index, object defaultValue)
        {
            object value = this[index];
            return value != null ? value : defaultValue;
        }

        public virtual bool GetBoolean(int index)
        {
            return GetBoolean(index, false);
        }

        public virtual bool GetBoolean(int index, bool defaultValue)
        {
            object value = GetValue(index);
            if (value == null) return defaultValue;
            return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        public virtual double GetDouble(int index)
        {
            return GetDouble(index, float.NaN);
        }

        public virtual double GetDouble(int index, float defaultValue)
        {
            object value = GetValue(index);
            if (value == null) return defaultValue;
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        public virtual int GetInt32(int index)
        {
            return GetInt32(index, 0);
        }

        public virtual int GetInt32(int index, int defaultValue)
        {
            object value = GetValue(index);
            if (value == null) return defaultValue;
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        public virtual string GetString(int index)
        {
            return GetString(index, string.Empty);
        }

        public virtual string GetString(int index, string defaultValue)
        {
            object value = GetValue(index);
            if (value == null) return defaultValue;
            return value.ToString();
        }

        public virtual JArray GetArray(int index)
        {
            return (JArray) GetValue(index);
        }

        public virtual JObject GetObject(int index)
        {
            return (JObject) GetValue(index);
        }

        protected override void OnValidate(object value)
        {
            //
            // Null values are allowed in a JSON array so don't delegate
            // to the base class (CollectionBase) implementation since that
            // disallows null entries by default.
            //
        }

        /// <summary>
        /// Make an JSON external form string of this JsonArray. For
        /// compactness, no unnecessary whitespace is added.
        /// </summary>
        /// <remarks>
        /// This method assumes that the data structure is acyclical.
        /// </remarks>

        public override string ToString()
        {
			return JSONHelper.WriteJSON(this);
		}

      
        /// <summary>
        /// Copies the elements to a new object array.
        /// </summary>

        public virtual object[] ToArray()
        {
            return (object[]) ToArray(typeof(object));
        }

        /// <summary>
        /// Copies the elements to a new array of the specified type.
        /// </summary>

        public virtual Array ToArray(Type elementType)
        {
            return InnerList.ToArray(elementType);
        }

        public virtual void Reverse()
        {
            InnerList.Reverse();
        }

        //
        // Methods that imitate the JavaScript array methods.
        //

        /// <summary>
        /// Appends new elements to an array.
        /// </summary>
        /// <returns>
        /// The new length of the array.
        /// </returns>
        /// <remarks>
        /// This method appends elements in the order in which they appear. If
        /// one of the arguments is an array, it is added as a single element.
        /// Use the <see cref="Concat"/> method to join the elements from two or
        /// more arrays.
        /// </remarks>
        
        public int Push(object value)
        {
            Add(value);
            return Count;
        }

        /// <summary>
        /// Appends new elements to an array.
        /// </summary>
        /// <returns>
        /// The new length of the array.
        /// </returns>
        /// <remarks>
        /// This method appends elements in the order in which they appear. If
        /// one of the arguments is an array, it is added as a single element.
        /// Use the <see cref="Concat"/> method to join the elements from two or
        /// more arrays.
        /// </remarks>

        public int Push(params object[] values)
        {
            if (values != null)
            {
                foreach (object value in values)
                    Push(value);
            }

            return Count;
        }

        /// <summary>
        /// Removes the last element from an array and returns it.
        /// </summary>
        /// <remarks>
        /// If the array is empty, null is returned.
        /// </remarks>

        public object Pop()
        {
            if (Count == 0)
                return null;

            object lastValue = InnerList[Count - 1];
            RemoveAt(Count - 1);
            return lastValue;
        }

        /// <summary>
        /// Returns a new array consisting of a combination of two or more
        /// arrays.
        /// </summary>

        public JArray Concat(params object[] values)
        {
            JArray newArray = new JArray(this);

            if (values != null)
            {
                foreach (object value in values)
                {
                    JArray arrayValue = value as JArray;
                    
                    if (arrayValue != null)
                    {
                        foreach (object arrayValueValue in arrayValue)
                            newArray.Push(arrayValueValue);
                    }
                    else
                    {
                        newArray.Push(value);
                    }
                }
            }

            return newArray;
        }

        /// <summary>
        /// Removes the first element from an array and returns it.
        /// </summary>

        public object Shift()
        {
            if (Count == 0)
                return null;

            object firstValue = InnerList[0];
            RemoveAt(0);
            return firstValue;
        }

        /// <summary>
        /// Returns an array with specified elements inserted at the beginning.
        /// </summary>
        /// <remarks>
        /// The unshift method inserts elements into the start of an array, so
        /// they appear in the same order in which they appear in the argument
        /// list.
        /// </remarks>

        public JArray Unshift(object value)
        {
            List.Insert(0, value);
            return this;
        }

        /// <summary>
        /// Returns an array with specified elements inserted at the beginning.
        /// </summary>
        /// <remarks>
        /// The unshift method inserts elements into the start of an array, so
        /// they appear in the same order in which they appear in the argument
        /// list.
        /// </remarks>

        public JArray Unshift(params object[] values)
        {
            if (values != null)
            {
                foreach (object value in values)
                    Unshift(value);
            }

            return this;
        }
    }
}
