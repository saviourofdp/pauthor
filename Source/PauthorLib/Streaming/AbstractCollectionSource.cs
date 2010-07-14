//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// AbstractCollectionSource provides a simple base class suitable for most implementations of
    /// <see cref="ICollectionSource"/>.
    /// </summary>
    /// <remarks>
    /// For most collection sources, the items represent the vast majority of the data for the collection, and all other
    /// pieces of data (e.g., facet categories, name, icon, etc.) constitute a much smaller portion of the data. This
    /// makes it reasonable to want to cache the facet categories and other header data, and to re-read the item data
    /// each time it is requested. This class provides a very simple set of methods for collection source implementators
    /// to override, and implements all the correct caching behavior for you.
    /// </remarks>
    public abstract class AbstractCollectionSource : IPivotCollectionSource
    {
        public AbstractCollectionSource(String basePath)
        {
            m_basePath = basePath;
        }

        /// <summary>
        /// The user-facing title of this collection.
        /// </summary>
        /// <see cref="PivotCollection.Name"/>
        public virtual String Name
        {
            get { return this.CachedCollectionData.Name; }
        }

        /// <summary>
        /// The favicon associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Icon"/>
        public virtual PivotImage Icon
        {
            get { return this.CachedCollectionData.Icon; }
        }

        /// <summary>
        /// The branding image associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.BrandImage"/>
        public virtual PivotImage BrandImage
        {
            get { return this.CachedCollectionData.BrandImage; }
        }

        /// <summary>
        /// The additional text to be appended to an item's name when requesting search results from Bing.
        /// </summary>
        /// <seealso cref="PivotCollection.AdditionalSearchText"/>
        public virtual String AdditionalSearchText
        {
            get { return this.CachedCollectionData.AdditionalSearchText; }
        }

        /// <summary>
        /// The version number of the Pivot schema used to represent this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.SchemaVersion"/>
        public virtual String SchemaVersion
        {
            get { return this.CachedCollectionData.SchemaVersion; }
        }

        /// <summary>
        /// The copyright link for the content of this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Copyright"/>
        public virtual PivotLink Copyright
        {
            get { return this.CachedCollectionData.Copyright; }
        }

        /// <summary>
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.ImageBase"/>
        public virtual String ImageBase
        {
            get { return this.CachedCollectionData.ImageBase; }
        }

        /// <summary>
        /// The facet categories defined for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.FacetCategories"/>
        public virtual IReadOnlyPivotList<String, PivotFacetCategory> FacetCategories
        {
            get { return this.CachedCollectionData.FacetCategories; }
        }

        /// <summary>
        /// An enumeration of all the items in this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Items"/>
        public virtual IEnumerable<PivotItem> Items
        {
            get { return this.LoadItems(); }
        }

        /// <summary>
        /// A string describing where this collection's data resides.
        /// </summary>
        /// <seealso cref="PivotCollection.BasePath"/>
        public virtual String BasePath
        {
            get { return this.CachedCollectionData.BasePath; }

            protected set { this.CachedCollectionData.BasePath = value; }
        }

        /// <summary>
        /// Disposes of the resources used by this collection source.
        /// </summary>
        public virtual void Dispose()
        {
            // Do nothing.
        }

        /// <summary>
        /// Subclasses must override this method to populate the <see cref="CachedCollectionData"/> property with all
        /// of the collection's data except the items.
        /// </summary>
        protected abstract void LoadHeaderData();

        /// <summary>
        /// Subclasses must override this method to return an enumeration of the collection's items.
        /// </summary>
        /// <remarks>
        /// In order to comply with the implicit contract of the <see cref="ICollectionSource"/> interface, subclasses
        /// should implement this method to <code>yield return</code> each item as it is created. They should not, for
        /// example, simply create a list of items and return it.
        /// </remarks>
        /// <returns>an enumeration of the items in this collection</returns>
        protected abstract IEnumerable<PivotItem> LoadItems();

        /// <summary>
        /// The <see cref="Microsoft.LiveLabs.Pauthor.Core.PivotCollection"/> which caches all the non-item data for
        /// this collection.
        /// </summary>
        /// <remarks>
        /// When this property is first read, it will call the <see cref="LoadHeaderData()"/> method. That method should
        /// fully populate all the non-item data in this collection.
        /// </remarks>
        protected PivotCollection CachedCollectionData
        {
            get
            {
                if (m_cachedCollectionData == null)
                {
                    m_cachedCollectionData = new PivotCollection();
                    m_cachedCollectionData.BasePath = m_basePath;
                    this.LoadHeaderData();
                }
                return m_cachedCollectionData;
            }
        }

        /// <summary>
        /// Clears all the cached header data for this collection.
        /// </summary>
        protected void ClearCache()
        {
            m_cachedCollectionData = null;
        }

        private PivotCollection m_cachedCollectionData;

        private String m_basePath;
    }
}
