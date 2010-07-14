//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;

namespace Microsoft.LiveLabs.Pauthor.Core
{
    /// <summary>
    /// FacetSortOrder defines a custom sort order made available for a specific facet category.
    /// </summary>
    /// <remarks>
    /// A custom sort order is composed of a name and an ordered set of values. The Pivot client will display the
    /// name in the sort order selector in the facet pane. The sort values will determine: the ordering of the
    /// values within this facet category's section in the facet pane, the ordering of items containing those values
    /// in the view pane while in grid view, the buckets displayed in histogram view, as well as the order of items
    /// within each bucket. Any values not included in the sort values will be shown in alphabetical order after
    /// those which are.
    /// </remarks>
    public class PivotFacetSortOrder
    {
        /// <summary>
        /// Creates a new sort order with a given name.
        /// </summary>
        /// <param name="name">the user-facing name of the custom sort order</param>
        /// <exception cref="ArgumentException">if the given name is empty or null</exception>
        public PivotFacetSortOrder(String name)
        {
            m_values = new List<String>();
            this.Name = name;
        }

        /// <summary>
        /// The user-facing name of this sort order.
        /// </summary>
        /// <remarks>
        /// It is displayed in Pivot in the sort selector associated with the individual facet category to which it
        /// is assigned. This property cannot be empty or null. The initial value is assigned by the constructor.
        /// </remarks>
        /// <exception cref="ArgumentException">if the given name is empty or null</exception>
        public String Name
        {
            get { return m_name; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("Name cannot be null or empty");
                m_name = value;
            }
        }

        /// <summary>
        /// An ordered sequence of values which make up the custom sort order.
        /// </summary>
        /// <remarks>
        /// This property is read-only; see the <see cref="AddValue"/>, <see cref="AddAllValues"/>, and
        /// <see cref="RemoveValue"/> methods in order to modify the list. By default, this property contains an
        /// empty enumeration.
        /// </remarks>
        public IEnumerable<String> Values
        {
            get { return m_values; }
        }

        /// <summary>
        /// Adds a value to this sort order.
        /// </summary>
        /// <param name="value">the value to add</param>
        /// <exception cref="ArgumentNullException">if a null value is given</exception>
        /// <exception cref="ArgumentException">if a duplicate value is given</exception>
        public void AddValue(String value)
        {
            if (value == null) throw new ArgumentNullException("Sort Order Value");
            if (m_values.Contains(value))
            {
                throw new ArgumentException("Cannot have duplicate sort order values: " + value);
            }
            m_values.Add(value);
        }

        /// <summary>
        /// Adds all of the given values to this sort order in the order specified.
        /// </summary>
        /// <remarks>
        /// This method is equivalent to repeatedly calling the <see cref="AddValue"/> method.
        /// </remarks>
        /// <param name="values">the values to add</param>
        /// <exception cref="ArgumentNullException">if a null value is given</exception>
        /// <exception cref="ArgumentException">if a duplicate value is given</exception>
        public void AddAllValues(IEnumerable<String> values)
        {
            foreach (String value in values)
            {
                this.AddValue(value);
            }
        }

        /// <summary>
        /// Removes a given value from the sort order.
        /// </summary>
        /// <remarks>
        /// If the given value was not in the sort order, then nothing happens.
        /// </remarks>
        /// <param name="value">the value to remove</param>
        public void RemoveValue(String value)
        {
            if (value == null) return;
            m_values.Remove(value);
        }

        private String m_name;

        private List<String> m_values;
    }
}
