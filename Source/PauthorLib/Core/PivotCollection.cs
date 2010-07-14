//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;

namespace Microsoft.LiveLabs.Pauthor.Core
{
    /// <summary>
    /// PivotCollection is an in-memory representation of an entire Pivot collection.
    /// </summary>
    /// <remarks>
    /// All items, facet categories, and other information is accessible and editable from this class. While this is
    /// very convenient for small collections, it can be quite a memory burden for larger collections. In such cases,
    /// consider using the various classes defined in the <see cref="Microsoft.LiveLabs.Pauthor.Streaming"/> namespace
    /// instead.
    /// </remarks>
    public class PivotCollection : ICollectionDefinition
    {
        /// <summary>
        /// Set the <see cref="AdditionalSearchText" /> property to this value to prevent Bing search results from being
        /// displayed for this collection.
        /// </summary>
        public const String BlockAdditionalSearches = "__block";

        /// <summary>
        /// Creates a new, empty Pivot collection.
        /// </summary>
        public PivotCollection()
        {
            m_facetCategories = new PivotList<String, PivotFacetCategory>();
            m_facetCategories.OnAddItem = this.OnAddFacetCategory;
            m_facetCategories.OnRemoveItem = this.OnRemoveFacetCategory;
            m_facetCategories.GetKeyForItem = (facetCategory => facetCategory.Name);

            m_items = new PivotList<String, PivotItem>();
            m_items.OnAddItem = this.OnAddItem;
            m_items.OnRemoveItem = this.OnRemoveItem;
            m_items.GetKeyForItem = (item => item.Id);

            this.SchemaVersion = "1.0";
            this.BasePath = ".";
        }

        /// <summary>
        /// The user-facing title of this collection.
        /// </summary>
        /// <remarks>
        /// This text will appear in the top-left corner of the Pivot client, just above the facet pane. If this
        /// property is null, then no title will be displayed. This property may not contain an empty string. If one is
        /// assigned, it will be converted to null. By default, this property is null.
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
        /// The favicon associated with this collection.</summary>
        /// <remarks>This icon will appear to the left of the collection's title
        /// just above the facet pane. If a <see cref="BrandImage"/> is provided, that will be displayed instead. The
        /// favicon is also displayed if the user pins the collection to the toolbar on the bottom of the screen. If
        /// this property is null, then no icon will be displayed. By default, this property is null.
        /// </remarks>
        public PivotImage Icon
        {
            get { return m_icon; }

            set { m_icon = value; }
        }

        /// <summary>
        /// The branding image associated with this collection.
        /// </summary>
        /// <remarks>
        /// This image will appear to the left of the collection's <see cref="Name"/> above the facet pane. If this
        /// property is null, then no branding image will be displayed. By default, this property is null.
        /// </remarks>
        public PivotImage BrandImage
        {
            get { return m_brandImage; }

            set { m_brandImage = value; }
        }

        /// <summary>
        /// The additional text to be appended to an item's name when requesting search results from Bing.
        /// </summary>
        /// <remarks>
        /// These results are displayed in the bottom portion of the info pane when a user has selected a single item.
        /// In order to suppress all search results, assign <see cref="BlockAdditionalSearches"/> to this property. If
        /// this property is set to null, then the item's name will be used as the search string. By default, this
        /// property is null.
        /// </remarks>
        public String AdditionalSearchText
        {
            get { return m_additionalSearchText; }

            set { m_additionalSearchText = value; }
        }

        /// <summary>
        /// The version number of the Pivot schema used to represent this collection.
        /// </summary>
        /// <remarks>
        /// By default, the version is "1.0" and should not be changed unless you are working with a pre-release version
        /// of Pivot. In such a clase, ask your contact on the Pivot team what value you should use.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if given a <c>null</c> value</exception>
        public String SchemaVersion
        {
            get { return m_schemaVersion; }

            set 
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("SchemaVersion");
                m_schemaVersion = value;
            }
        }

        /// <summary>
        /// The copyright link for the content of this collection.
        /// </summary>
        /// <remarks>
        /// This hyperlink will appear in small text at the very bottom of the info pane. Despite being called a
        /// "Copyright" link, it may actually be used to link to any resource you would like (the most common purpose
        /// being to link to an HTML page acknowledging the ownership of part of the content in the collection). If this
        /// property is null, then no copyright link will be displayed. By default, this property is null.
        /// </remarks>
        public PivotLink Copyright
        {
            get { return m_copyright; }

            set { m_copyright = value; }
        }

        /// <summary>
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection.
        /// </summary>
        /// <remarks>
        /// If this collection does not use a DeepZoom Collection for images, this property should be null. By default,
        /// this property is null.
        /// </remarks>
        public String ImageBase
        {
            get { return m_imageBase; }

            set { m_imageBase = value; }
        }

        /// <summary>
        /// The facet categories defined for this collection.
        /// </summary>
        /// <remarks>
        /// This is an ordered list of facet categories, given in the same order in which they were first added to the
        /// collection. By assigning a new list to this property, all existing facet categories will be removed and the
        /// new ones added (i.e., the new list instance is not literally assigned). By default, this property is an
        /// empty list.
        /// <para/>
        /// Since this property is a <see cref="PivotList&lt;K,T&gt;"/>, individual facet categories may be accessed in
        /// a variety of ways. For example, using the [] operator with a string will return the facet category with the
        /// given name. If used with an <c>int</c>, the facet category at the given index will be returned.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// if assigned any value after items have been added to the collection
        /// </exception>
        public PivotList<String, PivotFacetCategory> FacetCategories
        {
            get { return m_facetCategories; }
        }

        /// <summary>
        /// A read-only list of the facet categories defined for this collection.
        /// </summary>
        /// <see cref="FacetCategories"/>
        IReadOnlyPivotList<String, PivotFacetCategory> ICollectionDefinition.FacetCategories
        {
            get { return this.FacetCategories; }
        }

        /// <summary>
        /// All the items in this collection.
        /// </summary>
        /// <remarks>
        /// This is an ordered list of items, given in the same order in which they were first added to the collection.
        /// By assigning a new list to this property, all existing items will be removed and the new ones added (i.e.,
        /// the new list instance is not literally assigned). By default, this property is an empty list.
        /// <para/>
        /// Since this property is a <see cref="PivotList&lt;K,T&gt;"/>, individual items may be accessed in a variety
        /// of ways. For example, using the [] operator with a string will return the item with the given ID. If used
        /// with an <c>int</c>, the facet category at the given index will be returned.
        /// </remarks>
        public PivotList<String, PivotItem> Items
        {
            get { return m_items; }
        }

        /// <summary>
        /// Whether this collection should add missing facet categories used by items added to the collection.
        /// </summary>
        /// <remarks>
        /// If set to <c>true</c>, then whenever an item is added which uses a facet category not present in this
        /// collection, a new facet category will be created whose type is inferred from the type of the value the item
        /// has for that facet category.
        /// <para/>
        /// For example, suppose an item has a facet value called "Source" of type <see cref="PivotLink"/>. If this item
        /// were added to a collection which does not have a "Source" facet category already, and this property is
        /// <c>true</c>, then a new facet category called "Source" of type "Link" would be added to this collection.
        /// </remarks>
        public bool InferFacetCategories
        {
            get;
            set;
        }

        /// <summary>
        /// The path to which any relative paths in this collection are defined.
        /// </summary>
        /// <remarks>
        /// There are a variety of places where collection data refers to paths relative to the collection. For example,
        /// the <see cref="Icon"/> and <see cref="ImageBase"/> properties on this object, as well as the <see
        /// cref="PivotImage.SourcePath"/> property can all refer to relative paths. All such relative paths may be
        /// resolved with reference to this property.
        /// <para/>
        /// All paths in collections can either be local file paths or remote URIs. Generally, assuming that all paths
        /// are URIs will work best since .NET will automatically convert local file paths into URIs, but not
        /// vice-versa.
        /// </remarks>
        public String BasePath
        {
            get { return m_basePath; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BasePath");
                m_basePath = value;
            }
        }

        private PivotFacetCategory OnAddFacetCategory(PivotFacetCategory facetCategory)
        {
            if (this.FacetCategories.Contains(facetCategory.Name))
            {
                throw new ArgumentException("Cannot add a duplicate facet category name (Letters already exists)");
            }
            return facetCategory;
        }

        private void OnRemoveFacetCategory(PivotFacetCategory facetCategory)
        {
            foreach (PivotItem item in this.Items)
            {
                item.RemoveAllFacetValues(facetCategory.Name);
            }
        }

        private PivotItem OnAddItem(PivotItem item)
        {
            try
            {
                item.ContainingCollection = this;
            }
            catch
            {
                item.ContainingCollection = null;
                throw;
            }

            return item;
        }

        private void OnRemoveItem(PivotItem item)
        {
            item.ContainingCollection = null;
        }

        private String m_name;

        private PivotImage m_icon;

        private PivotImage m_brandImage;

        private String m_additionalSearchText;

        private String m_schemaVersion;

        private PivotLink m_copyright;

        private String m_imageBase;

        private PivotList<String, PivotFacetCategory> m_facetCategories;

        private PivotList<String, PivotItem> m_items;

        private String m_basePath;
    }
}
