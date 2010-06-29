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

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// IPivotCollectionSource defines an interface for a writing a Pivot collection stream.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are free to write the collection in any form they like, but they must not hold
    /// the entire list of items in memory at once. Instead, they should enumerate through the list of items and write
    /// them into the final form one-by-one and release them immediately. Because it does not load all items into memory
    /// this interface may be used to process collections of any size.
    /// </remarks>
    public interface IPivotCollectionTarget : IDisposable
    {
        /// <summary>
        /// Writes the given collection into the specific form dictated by the implementation of this interface.
        /// </summary>
        /// <param name="source">a Pivot collection source to write</param>
        void Write(IPivotCollectionSource source);
    }
}
