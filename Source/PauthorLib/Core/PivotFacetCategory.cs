//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;

namespace Microsoft.LiveLabs.Pauthor.Core
{
    /// <summary>
    /// PivotFacetCategory describes a single aspect of an item in a Pivot collection.
    /// </summary>
    /// <remarks>
    /// For example, in a collection of US Senators, there may be facet categories for "Party", "Year Elected", and
    /// "Home State". The collection has a list of all the supported facet categories, and each item contains multiple
    /// facets. Each of those facets is defined by one of the facet categories belonging to the collection.
    /// </remarks>
    public class PivotFacetCategory
    {
        /// <summary>
        /// Creates a new facet category with a given name and type.
        /// </summary>
        /// <remarks>
        /// See the <see cref="Name"/> and <see cref="Type"/> properties for details.
        /// </remarks>
        /// <param name="name">the desired name for the new facet category</param>
        /// <param name="type">the type of the new facet category</param>
        public PivotFacetCategory(String name, PivotFacetType type)
        {
            this.Name = name;
            this.Type = type;
            this.IsFilterVisible = type.IsValidInFilterPane();
            this.IsMetaDataVisible = true;
            this.IsWordWheelVisible = type.IsValidInWordWheel();
        }

        /// <summary>
        /// The user-facing name for this facet category.
        /// </summary>
        /// <remarks>
        /// While it's technically permissable to use any string as a name, it's a good idea to keep these under 25 or
        /// so characters, and to avoid using punctuation characters (e.g., ".", ",", "[", "]", etc.). In any case, the
        /// name cannot be null or empty. The initial value for this property is set by the constructor.
        /// </remarks>
        public String Name
        {
            get { return m_name; }

            private set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("Name cannot be null or empty");
                m_name = value;
            }
        }

        /// <summary>
        /// The type of data represented by this facet.
        /// </summary>
        /// <remarks>
        /// This property must be one of the constants defined by the <see cref="PivotFacetType"/> class, and may not be
        /// null. The initial value for this property is set by the constructor.
        /// </remarks>
        public PivotFacetType Type
        {
            get { return m_type; }

            private set
            {
                if (value == null) throw new ArgumentNullException("Type cannot be null");
                m_type = value;
            }
        }

        /// <summary>
        /// The formatting string which is used to present values pertaining to this facet category.
        /// </summary>
        /// <remarks>
        /// This is only used for DateTime and Number facet category types, and should be set to one of the appropriate
        /// .NET formatting strings for each type. If this property is set to null, then the default formatting will be
        /// used for each value (i.e., the result of the appropriate ToString method). By default, this property is set
        /// to null.
        /// </remarks>
        public String Format
        {
            get { return m_format; }

            set { m_format = value; }
        }

        /// <summary>
        /// Whether this facet category appears in the filter pane.
        /// </summary>
        /// <remarks>
        /// Only facet categories whose <see cref="Type"/> is: DateTime, Number, or String may have this property set to
        /// true. For those types, this property is set to true by default; for all others, it is set to false.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// if this property is set to true when this facet category's type does not support it
        /// </exception>
        public bool IsFilterVisible
        {
            get { return m_facetVisible; }

            set
            {
                if ((this.Type.IsValidInFilterPane() == false) && (value == true))
                {
                    throw new ArgumentException("A facet can only be filter visible if it is a DateTime, Number or " +
                        "String facet type (currently: " + this.Type + ")");
                }

                m_facetVisible = value;
            }
        }

        /// <summary>
        /// Whether this facet category appears in the info pane.
        /// </summary>
        public bool IsMetaDataVisible
        {
            get { return m_metaDataVisible; }
            
            set { m_metaDataVisible = value; }
        }

        /// <summary>
        /// Whether this facet category may be used during a text search.
        /// </summary>
        /// <remarks>
        /// Only facet categories whose <see cref="Type"/> is: Link, LongString, or String may have this property set to
        /// true. For those types, this property is set to true by default; for all others, it is set to false.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// if set to a value not supported by this facet category's type
        /// </exception>
        public bool IsWordWheelVisible
        {
            get { return m_wordWheelVisible; }
            
            set
            {
                if ((this.Type.IsValidInWordWheel() == false) && (value == true))
                {
                    throw new ArgumentException("A facet can only be word wheel visible if it is a Link, LongString, " +
                        "or String facet type (currently: " + this.Type + ")");
                }

                m_wordWheelVisible = value;
            }
        }

        /// <summary>
        /// The custom sort order made available for String facet categories in the facet pane, as well as the sort
        /// order used when laying out items.
        /// </summary>
        /// <remarks>
        /// If this property is defined, then an additional sort order will be offered in the facet pane for this facet
        /// category. In addition, when items are sorted in the view pane are sorted by this facet category, they will
        /// obey this sort order. If this property is null, then only the default sort orders are presented. See
        /// <see cref="PivotFacetSortOrder"/> for details.
        /// </remarks>
        public PivotFacetSortOrder SortOrder
        {
            get { return m_sortOrder; }

            set
            {
                if ((this.Type.IsValidInSortOrder() == false) && (value != null))
                {
                    throw new ArgumentException("A facet can only have a sort order if it is a String");
                }
                m_sortOrder = value;
            }
        }

        private String m_name;

        private PivotFacetType m_type;

        private String m_format;

        private bool m_facetVisible;

        private bool m_metaDataVisible;

        private bool m_wordWheelVisible;

        private PivotFacetSortOrder m_sortOrder;
    }
}
