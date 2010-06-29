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
    /// IPivotCollectionSource defines an interface for a readable stream of a Pivot collection.
    /// </summary>
    /// <remarks>
    /// Implementations are encouraged to read (and possibly cache) all of the properties of the collection at once, but
    /// they must read the items into memory one-by-one as they are enumerated, and release them immediately. Because it
    /// does not load all items into memory this interface may be used to process collections of any size.
    /// </remarks>
    public interface IPivotCollectionSource : ICollectionDefinition, IDisposable
    {
        /// <summary>
        /// The user-facing title of this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Name"/>
        String Name { get; }

        /// <summary>
        /// The favicon associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Icon"/>
        PivotImage Icon { get; }

        /// <summary>
        /// The branding image associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.BrandImage"/>
        PivotImage BrandImage { get; }

        /// <summary>
        /// The additional text to be appended to an item's name when requesting search results from Bing.
        /// </summary>
        /// <seealso cref="PivotCollection.AdditionalSearchText"/>
        String AdditionalSearchText { get; }

        /// <summary>
        /// The version number of the Pivot schema used to represent this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.SchemaVersion"/>
        String SchemaVersion { get; }

        /// <summary>
        /// The copyright link for the content of this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Copyright"/>
        PivotLink Copyright { get; }

        /// <summary>
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.ImageBase"/>
        String ImageBase { get; }

        /// <summary>
        /// An enumeration of all the items in this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Items"/>
        IEnumerable<PivotItem> Items { get; }
    }
}
