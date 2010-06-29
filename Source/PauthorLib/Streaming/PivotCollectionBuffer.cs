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
    /// PivotCollectionBuffer wraps a in-memory PivotCollection and allows it to serve as either a collection source or
    /// collection target.
    /// </summary>
    /// <remarks>
    /// This allows collection streams to be converted into in-memory collections, and vice-versa. <b>NOTE:</b> large
    /// collections can rapidly consume all available memory, so this class should only be used for collections which
    /// are known to be sufficiently small to fit completely in memory.
    /// </remarks>
    public class PivotCollectionBuffer : IPivotCollectionSource, IPivotCollectionTarget
    {
        /// <summary>
        /// Creates a new collection buffer with a new, emtpy collection as its basis.
        /// </summary>
        public PivotCollectionBuffer()
            : this(null)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new collection buffer with an existing collection as its basis.
        /// </summary>
        /// <param name="collection">the collection to serve as the basis for the new buffer</param>
        public PivotCollectionBuffer(PivotCollection collection)
        {
            this.Collection = collection;
        }

        /// <summary>
        /// The in-memory collection which serves as the basis for this buffer.
        /// </summary>
        /// <remarks>
        /// Any properties read from this buffer will be taken directly from this collection. Any changes to properties
        /// on this buffer will likewise be written through directly to this collection. Assigning a null value to this
        /// property will automatically set this property to a new, empty collection. The initial value of this property
        /// is set by the constructor.
        /// </remarks>
        public PivotCollection Collection
        {
            get { return m_collection; }

            set
            {
                m_collection = (value == null) ? new PivotCollection() : value;
            }
        }

        /// <summary>
        /// The user-facing title of this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Name"/>
        public String Name
        {
            get { return this.Collection.Name; }
        }

        /// <summary>
        /// The favicon associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Icon"/>
        public PivotImage Icon
        {
            get { return this.Collection.Icon; }
        }

        /// <summary>
        /// The branding image associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.BrandImage"/>
        public PivotImage BrandImage
        {
            get { return this.Collection.BrandImage; }
        }

        /// <summary>
        /// The additional text to be appended to an item's name when requesting search results from Bing.
        /// </summary>
        /// <seealso cref="PivotCollection.AdditionalSearchText"/>
        public String AdditionalSearchText
        {
            get { return this.Collection.AdditionalSearchText; }
        }

        /// <summary>
        /// The version number of the Pivot schema used to represent this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.SchemaVersion"/>
        public String SchemaVersion
        {
            get { return this.Collection.SchemaVersion; }
        }

        /// <summary>
        /// The copyright link for the content of this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Copyright"/>
        public PivotLink Copyright
        {
            get { return this.Collection.Copyright; }
        }

        /// <summary>
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.ImageBase"/>
        public String ImageBase
        {
            get { return this.Collection.ImageBase; }
        }

        /// <summary>
        /// The facet categories defined for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.FacetCategories"/>
        public IReadablePivotList<String, PivotFacetCategory> FacetCategories
        {
            get { return this.Collection.FacetCategories; }
        }

        /// <summary>
        /// An enumeration of all the items in this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Items"/>
        public IEnumerable<PivotItem> Items
        {
            get { return this.Collection.Items; }
        }

        /// <summary>
        /// Overwrites the collection currently contained by this buffer with the collection in the given collection
        /// source.
        /// </summary>
        /// <param name="source">the collection source to copy into this buffer</param>
        public void Write(IPivotCollectionSource source)
        {
            this.Collection = new PivotCollection();

            this.Collection.Name = source.Name;
            this.Collection.Icon = source.Icon;
            this.Collection.BrandImage = source.BrandImage;
            this.Collection.AdditionalSearchText = source.AdditionalSearchText;
            this.Collection.SchemaVersion = source.SchemaVersion;
            this.Collection.Copyright = source.Copyright;
            this.Collection.ImageBase = source.ImageBase;
            this.Collection.FacetCategories.AddRange(source.FacetCategories);
            this.Collection.Items.AddRange(source.Items);
        }

        /// <summary>
        /// Disposes of any resources used by this collection buffer.
        /// </summary>
        public void Dispose()
        {
            // Do nothing.
        }

        private PivotCollection m_collection;
    }
}
