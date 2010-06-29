//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    /// <summary>
    /// PivotCollectionSourceFilter provides an abstract base class for classes which wish to alter collection streams.
    /// </summary>
    /// <remarks>
    /// Every source filter requires a delegate collection source from which all the actual data is taken. This
    /// implementation returns data from the underlying delegate collection source unchanged. Subclasses should override
    /// those properties pertaining to the data they wish to alter, and return the new values for those properties
    /// instead of the original values.
    /// </remarks>
    public abstract class PivotCollectionSourceFilter : IPivotCollectionSource
    {
        /// <summary>
        /// Creates a new collection source filter with a stub source collection delegate.
        /// </summary>
        /// <remarks>
        /// This delegate is an instance of <see cref="NullCollectionSource"/> which returns null (or an empty
        /// enumeration) for all properties. This source delegate should be replaced before actually using the new
        /// filter.
        /// </remarks>
        public PivotCollectionSourceFilter()
            : this(NullCollectionSource.Instance)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new collection source filter with the given collection source delegate.
        /// </summary>
        /// <param name="source">the collection source from which this filter should draw its data</param>
        public PivotCollectionSourceFilter(IPivotCollectionSource source)
        {
            this.Source = source;
        }

        /// <summary>
        /// The collection source from which this filter draws its data.
        /// </summary>
        /// <exception cref="ArgumentNullException">if the given value was null</exception>
        public IPivotCollectionSource Source
        {
            get { return m_source; }

            set
            {
                if (value == null) throw new ArgumentNullException("Source");
                m_source = value;
            }
        }

        /// <summary>
        /// The user-facing title of this collection. See: <see cref="PivotCollection.Name"/>
        /// </summary>
        public virtual string Name
        {
            get { return this.Source.Name; }
        }

        /// <summary>
        /// The favicon associated with this collection. See: <see cref="PivotCollection.Icon"/>
        /// </summary>
        public virtual PivotImage Icon
        {
            get { return this.Source.Icon; }
        }

        /// <summary>
        /// The branding image associated with this collection. See: <see cref="PivotCollection.BrandImage"/>
        /// </summary>
        public virtual PivotImage BrandImage
        {
            get { return this.Source.BrandImage; }
        }

        /// <summary>
        /// The additional text to be appended to an item's name when requesting search results from Bing. See:
        /// <see cref="PivotCollection.AdditionalSearchText"/>
        /// </summary>
        public virtual string AdditionalSearchText
        {
            get { return this.Source.AdditionalSearchText; }
        }

        /// <summary>
        /// The version number of the Pivot schema used to represent this collection. See:
        /// <see cref="PivotCollection.SchemaVersion"/>
        /// </summary>
        public virtual string SchemaVersion
        {
            get { return this.Source.SchemaVersion; }
        }

        /// <summary>
        /// The copyright link for the content of this collection. See: <see cref="PivotCollection.Copyright"/>
        /// </summary>
        public virtual PivotLink Copyright
        {
            get { return this.Source.Copyright; }
        }

        /// <summary>
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection. See:
        /// <see cref="PivotCollection.ImageBase"/>
        /// </summary>
        public virtual string ImageBase
        {
            get { return this.Source.ImageBase; }
        }

        /// <summary>
        /// The facet categories defined for this collection. See: <see cref="PivotCollection.FacetCategories"/>
        /// </summary>
        public virtual IReadablePivotList<String, PivotFacetCategory> FacetCategories
        {
            get { return this.Source.FacetCategories; }
        }

        /// <summary>
        /// An enumeration of all the items in this collection. See: <see cref="PivotCollection.Items"/>
        /// </summary>
        public virtual IEnumerable<PivotItem> Items
        {
            get { return this.Source.Items; }
        }

        /// <summary>
        /// Disposes of the resources used by this filter. This call is echoed to the underlying collection source as
        /// well.
        /// </summary>
        public virtual void Dispose()
        {
            this.Source.Dispose();
        }

        private IPivotCollectionSource m_source;
    }
}
