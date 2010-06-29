//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// NullCollectionSource is a stub implementation of <see cref="IPivotCollectionSource"/> which returns null (or an
    /// empty enumeration) for all properties.
    /// </summary>
    /// <remarks>
    /// This class can be used as a placeholder source whereever one is required but the actual source is not yet
    /// available.
    /// </remarks>
    public class NullCollectionSource : IPivotCollectionSource
    {
        /// <summary>
        /// A shared instance of this collection source.
        /// </summary>
        /// <remarks>
        /// Since this collection source is immutable, there is no need to create multiple instances. Users of this
        /// class should refer this this single instance instead whereever possible.
        /// </remarks>
        public static readonly NullCollectionSource Instance = new NullCollectionSource();

        /// <summary>
        /// The user-facing title of this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Name"/>
        public string Name
        {
            get { return null; }
        }

        /// <summary>
        /// The favicon associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Icon"/>
        public PivotImage Icon
        {
            get { return null; }
        }

        /// <summary>
        /// The branding image associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.BrandImage"/>
        public PivotImage BrandImage
        {
            get { return null; }
        }

        /// <summary>
        /// The additional text to be appended to an item's name when requesting search results from Bing.
        /// </summary>
        /// <seealso cref="PivotCollection.AdditionalSearchText"/>
        public String AdditionalSearchText
        {
            get { return null; }
        }

        /// <summary>
        /// The version number of the Pivot schema used to represent this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.SchemaVersion"/>
        public String SchemaVersion
        {
            get { return null; }
        }

        /// <summary>
        /// The copyright link for the content of this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Copyright"/>
        public PivotLink Copyright
        {
            get { return null; }
        }

        /// <summary>
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.ImageBase"/>
        public String ImageBase
        {
            get { return null; }
        }

        /// <summary>
        /// The facet categories defined for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.FacetCategories"/>
        public IReadablePivotList<String, PivotFacetCategory> FacetCategories
        {
            get { return new PivotList<String, PivotFacetCategory>(); }
        }

        /// <summary>
        /// An enumeration of all the items in this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Items"/>
        public IEnumerable<PivotItem> Items
        {
            get { yield break; }
        }

        /// <summary>
        /// Releases any resources used by this collection source.
        /// </summary>
        public void Dispose()
        {
            // Do nothing.
        }
    }
}
