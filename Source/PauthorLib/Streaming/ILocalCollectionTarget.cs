//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// ILocalCollectionTarget defines a collection target whose data resides locally.
    /// </summary>
    /// <remarks>
    /// Certain Pivot collection target filters transform files on the local disk, and therefore require their delegates
    /// implement this interface.
    /// </remarks>
    public interface ILocalCollectionTarget : IPivotCollectionTarget
    {
        /// <summary>
        /// The path on the local disk (relative or absolute) where this collection target's data resides.
        /// </summary>
        String BasePath { get; set; }
    }
}
