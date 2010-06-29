//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.LiveLabs.Pauthor.Core
{
    /// <summary>
    /// PivotItem represents a single item in a Pivot collection.
    /// </summary>
    /// <remarks>
    /// It contains a few attributes of its own, but the bulk of its data is represented in its list of facets. Each
    /// item may have a single facet for each facet category in the collection, and each facet may have multiple values.
    /// Items may exist apart from a specific collection, but when they are finally added to a collection, their facets
    /// must be compatible with collection's facet categories.
    /// </remarks>
    public class PivotItem
    {
        /// <summary>
        /// Creates a new item with a given identifier.
        /// </summary>
        /// <remarks>
        /// While the identifier may have any value (except null or the empty string), it must be unique within any
        /// collection to which the item is added.
        /// </remarks>
        /// <param name="id">the unique identifier for the new item</param>
        /// <param name="collectionDefinition">the definition of the collection this item should use to validate
        /// itself</param>
        /// <exception cref="ArgumentException">if the given identifier is null or empty</exception>
        public PivotItem(String id, ICollectionDefinition collectionDefinition)
        {
            m_facets = new SortedDictionary<String, List<IComparable>>();
            m_relatedLinks = new List<PivotLink>();
            this.Id = id;
            this.CollectionDefinition = collectionDefinition;
        }

        /// <summary>
        /// The identifier for this item.
        /// </summary>
        /// <remarks>
        /// This identifier may not be null or empty, and must be unique within the context of any collection the item
        /// is added to.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if given a <c>null</c> value</exception>
        public String Id
        {
            get { return m_id; }

            private set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("Id");
                m_id = value;
            }
        }

        /// <summary>
        /// The user-facing name of this item.
        /// </summary>
        /// <remarks>
        /// This property may have any value, although an empty string will automatically be converted to null.
        /// By default, this property is null.
        /// </remarks>
        public String Name
        {
            get { return m_name; }

            set
            {
                m_name = String.IsNullOrEmpty(value) ? null : value;
            }
        }

        /// <summary>
        /// The image associated with this item.
        /// </summary>
        /// <remarks>
        /// While various formats are supported by different platforms, it is best to stick with JPEG or PNG images for
        /// greatest compatibility (PNG is the preferred format). This may also contain a reference to a specific item
        /// in a Deep Zoom Collection (DZC) file.  By default, this property is null.
        /// </remarks>
        public PivotImage Image
        {
            get { return m_image; }

            set { m_image = value; }
        }

        /// <summary>
        /// A textual description of this item.
        /// </summary>
        /// <remarks>
        /// This text may be of any length, but must be plain text (i.e., any markup will be rendered literally).
        /// Whitespace will be preserved, however an empty string will automatically be converted to null. By default,
        /// this property is null.
        /// </remarks>
        public String Description
        {
            get { return m_description; }

            set
            {
                m_description = String.IsNullOrEmpty(value) ? null : value;
            }
        }

        /// <summary>
        /// A URL which provides more details about this item.
        /// </summary>
        /// <remarks>
        /// This URL will be activated when the user either clicks the item's "Open" button, or double-clicks on the
        /// item itself. The URL is <b>not</b> verified to be a working URL, but empty values are automatically
        /// converted to null. By default, this property is null.
        /// </remarks>
        public String Href
        {
            get { return m_href; }

            set
            {
                m_href = String.IsNullOrEmpty(value) ? null : value;
            }
        }

        /// <summary>
        /// Returns the value of a given facet category for this item.
        /// </summary>
        /// <remarks>
        /// If this item does not have a value for the given facet category, then it will return <c>null</c>. Otherwise,
        /// if it has multiple values for that facet category, then the first value will be returned.
        /// </remarks>
        /// <param name="facetCategoryName">the name of the facet category whose value is desired</param>
        /// <returns>
        /// the first value associated with the given facet category, or <c>null</c> if this item has no values for the
        /// given facet category
        /// </returns>
        /// <seealso cref="GetFacetValueAsString"/>
        /// <seealso cref="GetAllFacetValues"/>
        public IComparable GetFacetValue(String facetCategoryName)
        {
            if (m_facets.ContainsKey(facetCategoryName) == false) return null;
            if (m_facets[facetCategoryName].Count == 0) return null;
            return m_facets[facetCategoryName][0];
        }

        /// <summary>
        /// Returns the value of the given facet category for this item as a string.
        /// </summary>
        /// <remarks>
        /// If this item does not have a value for the given facet category, then it will return <c>null</c>. Otherwise,
        /// the value will be formatted using the standard format for the facet category's type and returned. In case
        /// there are multiple values, only the first value will be returned.
        /// </remarks>
        /// <param name="facetCategoryName">the name of the facet category whose value is desired</param>
        /// <returns>
        /// a formatted string taken from the first value associated with the given facet category, or <c>null</c> if
        /// this item has no values for the given facet category
        /// </returns>
        /// <seealso cref="GetFacetValue"/>
        /// <seealso cref="GetAllFacetValuesAsString"/>
        public String GetFacetValueAsString(String facetCategoryName)
        {
            return this.ConvertFacetValueToString(facetCategoryName, this.GetFacetValue(facetCategoryName));
        }

        /// <summary>
        /// Returns all the facet values associated with a given facet category.
        /// </summary>
        /// <remarks>
        /// If this item does not have any values for the given facet category, then it will return <c>null</c>.
        /// </remarks>
        /// <param name="facetCategoryName">the name of the facet category whose value is desired</param>
        /// <returns>
        /// a new list containing the values this item has for the given facet category (i.e., altering the returned
        /// list does not alter this item)
        /// </returns>
        /// <seealso cref="GetFacetValue"/>
        /// <seealso cref="GetAllFacetValuesAsString"/>
        public List<IComparable> GetAllFacetValues(String facetCategoryName)
        {
            if (m_facets.ContainsKey(facetCategoryName) == false) return null;
            return new List<IComparable>(m_facets[facetCategoryName]);
        }

        /// <summary>
        /// Returns the value of the given facet category for this item as a string.
        /// </summary>
        /// <remarks>
        /// If this item does not have a value for the given facet category, then it will return <c>null</c>. Otherwise,
        /// the value will be formatted using the standard format for the facet category's type and returned. In case
        /// there are multiple values, only the first value will be returned.
        /// </remarks>
        /// <param name="facetCategoryName">the name of the facet category whose value is desired</param>
        /// <param name="delimiter">the text to be used to separate individual values</param>
        /// <returns>
        /// a formatted string taken from the first value associated with the given facet category, or <c>null</c> if
        /// this item has no values for the given facet category
        /// </returns>
        /// <seealso cref="GetFacetValue"/>
        /// <seealso cref="GetAllFacetValuesAsString"/>
        public String GetAllFacetValuesAsString(String facetCategoryName, String delimiter)
        {
            if (m_facets.ContainsKey(facetCategoryName) == false) return "";

            List<IComparable> facetValues = m_facets[facetCategoryName];
            String[] values = new String[facetValues.Count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = this.ConvertFacetValueToString(facetCategoryName, facetValues[i]);
            }
            return String.Join(delimiter, values);
        }

        /// <summary>
        /// Adds a set of values to this item for a given facet category.
        /// </summary>
        /// <remarks>
        /// When a new value is added, it is verified against this item's collection definition. If the facet category
        /// does not exist, then an exception is thrown. If the item is already part of a collection with
        /// <see cref="PivotCollection.InferFacetCategories"/> turned on, then instead of throwing an exception, a new
        /// facet category will be added to the collection. If a given value matches an existing facet category, but is
        /// is not a compatible <c>Type</c>, then an exception will be thrown. If given value is invalid, then none of
        /// the given values will be added.
        /// </remarks>
        /// <param name="facetCategoryName">the name of the facet category to which the values should be added</param>
        /// <param name="values">the values to add</param>
        /// <exception cref="ArgumentException">if the given facet category does not exist in this item's collection
        /// definition (and it's not part of a collection which infers facet categories), or if the given value is of an
        /// incompatible type</exception>
        public void AddFacetValues(String facetCategoryName, params IComparable[] values)
        {
            List<IComparable> actualValues = new List<IComparable>(values.Length);
            List<IComparable> facetValues = null;

            foreach (IComparable value in values)
            {
                actualValues.Add(this.ValidateFacetValue(this.CollectionDefinition, facetCategoryName, value));
            }

            foreach (IComparable actualValue in actualValues)
            {
                if (m_facets.TryGetValue(facetCategoryName, out facetValues) == false)
                {
                    facetValues = new List<IComparable>();
                    m_facets[facetCategoryName] = facetValues;
                }
                facetValues.Add(actualValue);
            }
        }

        /// <summary>
        /// Removes a set of facet values from this item for a named facet category.
        /// </summary>
        /// <remarks>
        /// If any values are provided, then only those values will be removed (if they exist in the name facet
        /// category). If the given values are not present for the given facet category, then the method call is
        /// ignored.
        /// </remarks>
        /// <param name="facetCategoryName">the name of the facet catgory to be affected</param>
        /// <param name="values">
        /// the values to remove, or omit any values to remove the facet category from this item altogether
        /// </param>
        public void RemoveFacetValues(String facetCategoryName, params IComparable[] values)
        {
            if (facetCategoryName == null) return;
            if (m_facets.ContainsKey(facetCategoryName) == false) return;
            if (values.Length == 0) throw new ArgumentException("Must specify at least one value to remove");

            List<IComparable> facetValues = m_facets[facetCategoryName];
            foreach (IComparable value in values)
            {
                facetValues.Remove(value);
            }

            if (facetValues.Count == 0)
            {
                m_facets.Remove(facetCategoryName);
            }
        }

        /// <summary>
        /// Removes all facet values from this item for a named facet category.
        /// </summary>
        /// <param name="facetCategoryName">the name of the facet catgory to be removed</param>
        public void RemoveAllFacetValues(String facetCategoryName)
        {
            if (facetCategoryName == null) return;
            if (m_facets.ContainsKey(facetCategoryName) == false) return;
            m_facets.Remove(facetCategoryName);
        }

        /// <summary>
        /// A list of the facet categories defined for this item.
        /// </summary>
        /// <remarks>
        /// Only the names of those facet categories for which at least one value is defined will be returned. A new
        /// list object is returned each time this property is accessed, and modifications to that list do not affect
        /// this object.
        /// </remarks>
        public List<String> FacetCategories
        {
            get { return new List<String>(m_facets.Keys); }
        }

        /// <summary>
        /// An enumeration of hyperlinks which should be displayed in the Related Links section of the info pane for
        /// this item.
        /// </summary>
        /// <remarks>
        /// By setting this property, you can replace the existing set of related links for this item. By setting this
        /// property to an empty enumeration, you can remove the existing related links. By default, this property is an
        /// empty enumeration.
        /// </remarks>
        /// <exception cref="ArgumentException">if any value given is null</exception>
        public IEnumerable<PivotLink> RelatedLinks
        {
            get { return m_relatedLinks; }

            set
            {
                m_relatedLinks.Clear();
                if (value == null) return;

                foreach (PivotLink link in value)
                {
                    this.AddRelatedLink(link);
                }
            }
        }

        /// <summary>
        /// Whether or not this item has any related links.
        /// </summary>
        /// <remarks>
        /// This property reflects the current state of the <see cref="RelatedLinks"/> property, and is therefore
        /// read-only.
        /// </remarks>
        public bool HasRelatedLinks
        {
            get { return m_relatedLinks.Count > 0; }
        }

        /// <summary>
        /// Adds a new related link to this item.
        /// </summary>
        /// <param name="link">the link to be added</param>
        /// <exception cref="ArgumentNullException">if the given value is null</exception>
        public void AddRelatedLink(PivotLink link)
        {
            if (link == null) throw new ArgumentNullException("Link");
            m_relatedLinks.Add(link);
        }

        /// <summary>
        /// The definition of the collection to which this item belongs.
        /// </summary>
        /// <remarks>
        /// This value is set via the item's constructor, and by adding the item to a collection. Changing this value
        /// will cause all facet values to be re-evaluted according to the definition of the new collection, and an
        /// exception will be thrown if any are found to be incompatible. If the item is added to a collection, then
        /// the same validation occurs, and the same possible exceptions in case of incompatible definitions.
        /// </remarks>
        public ICollectionDefinition CollectionDefinition
        {
            get { return m_collectionDefinition; }

            set
            {
                if (value == null) throw new ArgumentNullException("CollectionDefinition");

                foreach (String facetCategoryName in this.FacetCategories)
                {
                    List<IComparable> facetValues = m_facets[facetCategoryName];
                    for (int i = 0; i < facetValues.Count; i++)
                    {
                        facetValues[i] = this.ValidateFacetValue(value, facetCategoryName, facetValues[i]);
                    }
                }
                m_collectionDefinition = value;

                if (m_containingCollection != m_collectionDefinition)
                {
                    m_containingCollection = null;
                }
            }
        }

        /// <summary>
        /// Formats a given value as a string based upon the default formatting of the named facet category.
        /// </summary>
        /// <remarks>
        /// This method is used principally by the <see cref="GetFacetValueAsString"/> and <see
        /// cref="GetAllFacetValuesAsString"/> methods, but any compatible value may be formatted using this method.
        /// </remarks>
        /// <param name="facetCategoryName">the name of the facet category whose default format should be used</param>
        /// <param name="value">the value to format</param>
        /// <returns>a formatted version of the given value</returns>
        /// <exception cref="ArgumentException">if given an incompatible value</exception>
        public String ConvertFacetValueToString(String facetCategoryName, IComparable value)
        {
            PivotFacetCategory facetCategory = this.CollectionDefinition.FacetCategories[facetCategoryName];
            return facetCategory.Type.FormatValue(value);
        }

        internal PivotCollection ContainingCollection
        {
            get { return m_containingCollection; }

            set
            {
                m_containingCollection = value;
                if (value != null)
                {
                    this.CollectionDefinition = value;
                }
            }
        }

        internal IComparable ValidateFacetValue(ICollectionDefinition definition, String facetCategoryName, IComparable value)
        {
            IComparable actualValue = value;

            if (definition.FacetCategories.Contains(facetCategoryName))
            {
                PivotFacetCategory existingFacetCategory = definition.FacetCategories[facetCategoryName];

                if ((existingFacetCategory.Type.IsValidValue(value) == false))
                {
                    actualValue = null;
                    if (value is String)
                    {
                        try
                        {
                            actualValue = existingFacetCategory.Type.ParseValue((String)value);
                        }
                        catch (FormatException)
                        {
                            // Do nothing.
                        }
                    }

                    if (actualValue == null)
                    {
                        throw new ArgumentException("Item Id " + this.Id + " has an incompatible value (type: " +
                            value.GetType().Name + ") for facet category " + facetCategoryName);
                    }
                }
            }
            else if ((this.ContainingCollection != null) && (this.ContainingCollection.InferFacetCategories))
            {
                PivotFacetType facetType = PivotFacetType.IdentifyType(value);
                this.ContainingCollection.FacetCategories.Add(new PivotFacetCategory(facetCategoryName, facetType));
            }
            else
            {
                throw new ArgumentException("Item Id " + this.Id + " has an incompatible value (type: " +
                    value.GetType().Name + ") for facet category " + facetCategoryName);
            }

            return actualValue;
        }

        private String m_id;

        private String m_name;

        private PivotImage m_image;

        private String m_description;

        private String m_href;

        private SortedDictionary<String, List<IComparable>> m_facets;

        private List<PivotLink> m_relatedLinks;

        private ICollectionDefinition m_collectionDefinition;

        private PivotCollection m_containingCollection;
    }
}
