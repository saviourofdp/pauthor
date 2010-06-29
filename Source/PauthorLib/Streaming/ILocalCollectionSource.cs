//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// ILocalCollectionSource defines a collection source whose data resides locally.
    /// </summary>
    /// <remarks>
    /// Certain Pivot collection source filters transform files on the local disk, and therefore require their delegates
    /// implement this interface.
    /// </remarks>
    public interface ILocalCollectionSource : IPivotCollectionSource
    {
        /// <summary>
        /// The path on the local disk (relative or absolute) where this collection source's data resides.
        /// </summary>
        String BasePath { get; set; }
    }
}
