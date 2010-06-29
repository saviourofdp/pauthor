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
    /// PivotList is a readonly interface for a <see cref="PivotList&lt;K,T&gt;"/>.
    /// </summary>
    /// <typeparam name="K">the type of the unique identifier for each element</typeparam>
    /// <typeparam name="T">the type of each element</typeparam>
    public interface IReadablePivotList<K, T> : IEnumerable<T>, IEnumerable
    {
        /// <summary>
        /// Returns or changes the item at the given index.
        /// </summary>
        /// <param name="index">the index of the desired item</param>
        /// <returns>the item at the requested index</returns>
        /// <exception cref="ArgumentException">if given a <c>null</c> value</exception>
        /// <exception cref="ArgumentOutOfRangeException">if the given index is not within this list</exception>
        /// <exception cref="InvalidOperationException">if this list <see cref="IsReadOnly"/></exception>
        T this[int index] { get; }

        /// <summary>
        /// Returns or changes the item with a given key.
        /// </summary>
        /// <param name="key">the unique identifier of the desired item</param>
        /// <returns>the item with the requested key</returns>
        /// <exception cref="ArgumentException">if set to a <c>null</c> value or given a <c>null</c> key</exception>
        /// <exception cref="InvalidOperationException">if this list <see cref="IsReadOnly"/></exception>
        T this[K key] { get; }

        /// <summary>
        /// Whether this list is read-only.
        /// </summary>
        /// <remarks>
        /// If this is is read-only, then calling any method which mutates the list will thrown an exception.
        /// </remarks>
        bool IsReadOnly { get; }

        /// <summary>
        /// Returns whether this list contains a given item.
        /// </summary>
        /// <param name="item">the item to test</param>
        /// <returns><c>true</c> if this list contains the given item</returns>
        bool Contains(T item);

        /// <summary>
        /// Returns whether this list has an item with the given key.
        /// </summary>
        /// <param name="key">the unique identifier to test</param>
        /// <returns><c>true</c> if this list contains an item with the given key</returns>
        bool Contains(K key);

        /// <summary>
        /// Copies items in this list to the given array.
        /// </summary>
        /// <param name="array">the array into which values will be copied</param>
        /// <param name="arrayIndex">the index at which the first item should be placed</param>
        /// <see cref="System.Collections.Generic.ICollection&lt;T&gt;.CopyTo"/>
        void CopyTo(T[] array, int arrayIndex);

        /// <summary>
        /// The number of items currently in this list.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Determines the index at which a given item occurs in this list.
        /// </summary>
        /// <param name="item">the item whose index is desired</param>
        /// <returns>the index of the desired item or <c>-1</c> if the given item is not in this list</returns>
        int IndexOf(T item);

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
        bool TryGetValue(int index, out T item);

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
        bool TryGetValue(K key, out T item);
    }
}
