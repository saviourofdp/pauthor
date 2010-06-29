//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.LiveLabs.Pauthor.Core
{
    /// <summary>
    /// PivotList is a mix of a <see cref="System.Collections.Generic.List&lt;T&gt;"/> and <see
    /// cref="System.Collections.Generic.Dictionary&lt;K,V&gt;"/>. The two main properties of <see
    /// cref="PivotCollection"/> are implemented using this class to allow fast random, sequential, and keyed access to
    /// lists of elements.
    /// </summary>
    /// <typeparam name="K">the type of the unique identifier for each element</typeparam>
    /// <typeparam name="T">the type of each element</typeparam>
    public class PivotList<K, T> : IList<T>, IReadablePivotList<K,T>
    {
        /// <summary>
        /// Creates a new, empty list.
        /// </summary>
        public PivotList()
        {
            m_dataList = new List<T>();
            m_dataDictionary = new Dictionary<K, T>();

            this.OnAddItem = null;
            this.OnRemoveItem = null;
            this.OnReplaceItem = null;
            this.OnClear = null;
            this.GetKeyForItem = null;
        }

        /// <summary>
        /// Creates a new list containing the given items.
        /// </summary>
        /// <param name="items">the items to add to the new list</param>
        /// <exception cref="ArgumentException">
        /// if any of the given items is <c>null</c>, or has the same key as another item
        /// </exception>
        public PivotList(IEnumerable<T> items) : this()
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Returns or changes the item at the given index.
        /// </summary>
        /// <param name="index">the index of the desired item</param>
        /// <returns>the item at the requested index</returns>
        /// <exception cref="ArgumentException">if given a <c>null</c> value</exception>
        /// <exception cref="ArgumentOutOfRangeException">if the given index is not within this list</exception>
        /// <exception cref="InvalidOperationException">if this list <see cref="IsReadOnly"/></exception>
        public T this[int index]
        {
            get { return m_dataList[index]; }

            set
            {
                T newItem = this.OnReplaceItem(m_dataList[index], value);

                if (this.IsReadOnly) throw new InvalidOperationException("Cannot replace values in a read-only list");
                if (value == null) throw new ArgumentException("Cannot add nulls to a PivotList");
                if (index < 0) throw new ArgumentOutOfRangeException("Index out of range: " + index);
                if (index >= m_dataList.Count) throw new ArgumentOutOfRangeException("Index out of range: " + index);

                m_dataDictionary[this.GetKeyForItem(newItem)] = newItem;
                m_dataList[index] = newItem;
            }
        }

        /// <summary>
        /// Returns or changes the item with a given key.
        /// </summary>
        /// <param name="key">the unique identifier of the desired item</param>
        /// <returns>the item with the requested key</returns>
        /// <exception cref="ArgumentException">if set to a <c>null</c> value or given a <c>null</c> key</exception>
        /// <exception cref="InvalidOperationException">if this list <see cref="IsReadOnly"/></exception>
        public T this[K key]
        {
            get { return m_dataDictionary[key]; }

            set
            {
                T existingItem = m_dataDictionary[key];
                T newItem = this.OnReplaceItem(existingItem, value);

                if (this.IsReadOnly) throw new InvalidOperationException("Cannot replace values in a read-only list");
                if (value == null) throw new ArgumentException("Cannot add nulls to a PivotList");

                m_dataDictionary[key] = newItem;
                m_dataList.Remove(existingItem);
                m_dataList.Add(newItem);
            }
        }

        /// <summary>
        /// Whether this list is read-only.
        /// </summary>
        /// <remarks>
        /// If this is is read-only, then calling any method which mutates the list will thrown an exception.
        /// </remarks>
        public bool IsReadOnly
        {
            get { return m_readOnly; }

            internal set { m_readOnly = value; }
        }

        /// <summary>
        /// Adds a new item to this list.
        /// </summary>
        /// <param name="item">the item to add</param>
        /// <exception cref="ArgumentException">if given a <c>null</c> value</exception>
        /// <exception cref="InvalidOperationException">if called when the list is <see cref="IsReadOnly"/></exception>
        public void Add(T item)
        {
            T newItem = this.OnAddItem(item);

            if (this.IsReadOnly) throw new InvalidOperationException("Cannot add to a read-only list");
            if (item == null) throw new ArgumentException("Cannot add nulls to a PivotList");

            K key = this.GetKeyForItem(item);
            if (m_dataDictionary.ContainsKey(key))
            {
                throw new ArgumentException("Cannot add a second item with key: " + key);
            }

            m_dataDictionary[key] = newItem;
            m_dataList.Add(newItem);
        }

        /// <summary>
        /// Adds multiple values to this list.
        /// </summary>
        /// <param name="items">an enumeration of the items to be added</param>
        /// <exception cref="ArgumentException">if given a <c>null</c> value</exception>
        /// <exception cref="InvalidOperationException">if called when the list is <see cref="IsReadOnly"/></exception>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                this.Add(item);
            }
        }

        /// <summary>
        /// Removes all items from this list.
        /// </summary>
        /// <exception cref="InvalidOperationException">if called when the list is <see cref="IsReadOnly"/></exception>
        public void Clear()
        {
            this.OnClear();

            if (this.IsReadOnly) throw new InvalidOperationException("Cannot clear a read-only list");

            m_dataDictionary.Clear();
            m_dataList.Clear();
        }

        /// <summary>
        /// Returns whether this list contains a given item.
        /// </summary>
        /// <param name="item">the item to test</param>
        /// <returns><c>true</c> if this list contains the given item</returns>
        public bool Contains(T item)
        {
            return this.Contains(this.GetKeyForItem(item));
        }

        /// <summary>
        /// Returns whether this list has an item with the given key.
        /// </summary>
        /// <param name="key">the unique identifier to test</param>
        /// <returns><c>true</c> if this list contains an item with the given key</returns>
        public bool Contains(K key)
        {
            return m_dataDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Copies items in this list to the given array.
        /// </summary>
        /// <param name="array">the array into which values will be copied</param>
        /// <param name="arrayIndex">the index at which the first item should be placed</param>
        /// <see cref="System.Collections.Generic.ICollection&lt;T&gt;.CopyTo"/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            m_dataList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// The number of items currently in this list.
        /// </summary>
        public int Count
        {
            get { return m_dataList.Count; }
        }

        /// <summary>
        /// Returns an enumerator across all the items in this collection.
        /// </summary>
        /// <returns>an enumerator across all the items in this collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) m_dataList.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator across all the items in this collection.
        /// </summary>
        /// <returns>an enumerator across all the items in this collection</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return m_dataList.GetEnumerator();
        }

        /// <summary>
        /// Determines the index at which a given item occurs in this list.
        /// </summary>
        /// <param name="item">the item whose index is desired</param>
        /// <returns>the index of the desired item or <c>-1</c> if the given item is not in this list</returns>
        public int IndexOf(T item)
        {
            return m_dataList.IndexOf(item);
        }

        /// <summary>
        /// Inserts a new item into this collection at a specific index.
        /// </summary>
        /// <param name="index">the index into which the new item should be inserted</param>
        /// <param name="item">the item to be inserted</param>
        /// <exception cref="ArgumentOutOfRangeException">if the given index is not within this collection</exception>
        /// <exception cref="InvalidOperationException">if this list <see cref="IsReadOnly"/></exception>
        /// <see cref="System.Collections.Generic.IList&lt;T&gt;.Insert"/>
        public void Insert(int index, T item)
        {
            T newItem = this.OnAddItem(item);

            if (this.IsReadOnly) throw new InvalidOperationException("Cannot insert values in a read-only list");
            if (index < 0) throw new ArgumentOutOfRangeException("Index out of range: " + index);
            if (index > m_dataList.Count) throw new ArgumentOutOfRangeException("Index out of range: " + index);

            m_dataDictionary[this.GetKeyForItem(newItem)] = newItem;
            m_dataList.Insert(index, newItem);
        }

        /// <summary>
        /// Removes the item at a given index.
        /// </summary>
        /// <param name="index">the index of the item to remove</param>
        /// <exception cref="ArgumentOutOfRangeException">if the given index is not within this collection</exception>
        /// <exception cref="InvalidOperationException">if this list <see cref="IsReadOnly"/></exception>
        public void RemoveAt(int index)
        {
            T item = m_dataList[index];
            this.OnRemoveItem(item);

            if (this.IsReadOnly) throw new InvalidOperationException("Cannot remove from a read-only list");
            if (index < 0) throw new ArgumentOutOfRangeException("Index out of range: " + index);
            if (index >= m_dataList.Count) throw new ArgumentOutOfRangeException("Index out of range: " + index);

            m_dataList.RemoveAt(index);
            m_dataDictionary.Remove(this.GetKeyForItem(item));
        }

        /// <summary>
        /// Removes the given item from the collection.
        /// </summary>
        /// <param name="item">the item to be removed</param>
        /// <returns>
        /// <c>true</c> if the item was found and removed, <c>false</c> if the item was not in this list
        /// </returns>
        public bool Remove(T item)
        {
            this.OnRemoveItem(item);
            if (this.IsReadOnly) throw new InvalidOperationException("Cannot remove from a read-only list");

            m_dataDictionary.Remove(this.GetKeyForItem(item));
            return m_dataList.Remove(item);
        }

        /// <summary>
        /// Attempts to fetch an item from this list at a specific index.
        /// </summary>
        /// <remarks>
        /// If this list does not contain the specified index, then this method will return <c>false</c> and set the
        /// <paramref name="item"/> parameter to <c>null</c>. Otherwise, the out parameter will be set to the requested
        /// value and <c>true</c> will be returned.
        /// </remarks>
        /// <param name="index">the index of the desired item</param>
        /// <param name="item">the item at the given index or <c>null</c> if the given index is not in this list</param>
        /// <returns>
        /// Returns <c>true</c> if this list contains the given index, and <c>false</c> otherwise
        /// </returns>
        public bool TryGetValue(int index, out T item)
        {
            item = default(T);
            if (index < 0) return false;
            if (index >= m_dataList.Count) return false;
            item = m_dataList[index];
            return true;
        }

        /// <summary>
        /// Attempts to fetch an item from this which has a specific key.
        /// </summary>
        /// <remarks>
        /// If this list does not contain an item with the specified key, then this method will return <c>false</c> and
        /// set the <paramref name="item"/> parameter to <c>null</c>. Otherwise, the out parameter will be set to the
        /// requested value and <c>true</c> will be returned.
        /// </remarks>
        /// <param name="key">the unique identifier of the desired item</param>
        /// <param name="item">the item at the given index or <c>null</c> if the given key is not in this list</param>
        /// <returns>
        /// Returns <c>true</c> if this list contains the given index, and <c>false</c> otherwise
        /// </returns>
        public bool TryGetValue(K key, out T item)
        {
            item = default(T);
            if (this.Contains(key) == false) return false;
            item = m_dataDictionary[key];
            return true;
        }

        internal delegate T OnAddItemDelegate(T item);

        internal delegate void OnRemoveItemDelegate(T item);

        internal delegate T OnReplaceItemDelegate(T existingItem, T newItem);

        internal delegate void OnClearDelegate();

        internal delegate K GetKeyForItemDelegate(T item);

        internal OnAddItemDelegate OnAddItem
        {
            get { return m_onAddItem; }

            set { m_onAddItem = (value == null) ? (item => item) : value; }
        }

        internal OnRemoveItemDelegate OnRemoveItem
        {
            get { return m_onRemoveItem; }

            set { m_onRemoveItem = (value == null) ? (item => { }) : value; }
        }

        internal OnReplaceItemDelegate OnReplaceItem
        {
            get { return m_onReplaceItem; }

            set {
                m_onReplaceItem = (value != null) ? value : (OnReplaceItemDelegate)delegate(T existingItem, T newItem)
                {
                    this.OnRemoveItem(existingItem);
                    return this.OnAddItem(newItem);
                };
            }
        }

        internal OnClearDelegate OnClear
        {
            get { return m_onClear; }

            set { m_onClear = (value == null) ? (() => { }) : value; }
        }

        internal GetKeyForItemDelegate GetKeyForItem
        {
            get { return m_getKeyForItem; }

            set { m_getKeyForItem = (value == null) ? (item => default(K)) : value; }
        }

        private List<T> m_dataList;

        private Dictionary<K, T> m_dataDictionary;

        private bool m_readOnly;

        private OnAddItemDelegate m_onAddItem;

        private OnRemoveItemDelegate m_onRemoveItem;

        private OnReplaceItemDelegate m_onReplaceItem;

        private OnClearDelegate m_onClear;

        private GetKeyForItemDelegate m_getKeyForItem;
    }
}
