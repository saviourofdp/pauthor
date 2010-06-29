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
    /// ICollectionDefinition summarizes the definition properties of a Pivot collection.
    /// </summary>
    /// <remarks>
    /// This class allows both an actual <see cref="PivotCollection"/> object as well as a <see
    /// cref="Microsoft.LiveLabs.Pauthor.Streaming.IPivotCollectionSource"/> to be considered as defining collections.
    /// </remarks>
    public interface ICollectionDefinition
    {
        /// <summary>
        /// The facet categories which define this collection.
        /// </summary>
        /// <see cref="PivotCollection.FacetCategories"/>
        IReadablePivotList<String, PivotFacetCategory> FacetCategories { get; }
    }
}
