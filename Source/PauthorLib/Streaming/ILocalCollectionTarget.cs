//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
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
