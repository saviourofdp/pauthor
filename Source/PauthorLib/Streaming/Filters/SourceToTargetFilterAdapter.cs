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

using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    /// <summary>
    /// SourceToTargetFilterAdapter adapts a <see cref="PivotCollectionSourceFilter"/> to a
    /// <see cref="PivotCollectionTargetFilter"/> interface.
    /// </summary>
    /// <remarks>
    /// Since all filters alter a collection stream, it is often the case that the order in which filters are applied is
    /// important. Sometimes, one would like to use a source filter in between two target filters. Unfortunately, this
    /// isn't possible because all source filters are applied by the time the first target filter sees the data. This
    /// class allows a source filter to be inserted (as a target filter) in between multiple target filters.
    /// 
    /// <para>This is done in the <see cref="Write"/> method by wrapping the given collection source passed to that
    /// method with this filter's source filter. The source filter is then passed along to this filter's delegate
    /// collection target. As the delegate reads from what it thinks is the collection source, the actual collection
    /// source is altered by the source filter which has been interposed in between.</para>
    /// </remarks>
    public class SourceToTargetFilterAdapter : PivotCollectionTargetFilter
    {
        /// <summary>
        /// Creates a new source-to-target filter adapter with a given source filter, and a stub delegate collection
        /// target.
        /// </summary>
        /// <remarks>
        /// The stub should be replaced with a real collection target before using the new filter.
        /// </remarks>
        /// <param name="sourceFilter">the source filter to be applied to collections written to this filter</param>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        public SourceToTargetFilterAdapter(PivotCollectionSourceFilter sourceFilter)
            : this(NullCollectionTarget.Instance, sourceFilter)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new source-to-target filter adapter using a given source filter, and a given delegate collection
        /// target.
        /// </summary>
        /// <param name="target">the collection target to which a collection stream is written after the source filter
        /// has been applied</param>
        /// <param name="sourceFilter">the source filter to be applied to a given collection stream</param>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        public SourceToTargetFilterAdapter(IPivotCollectionTarget target, PivotCollectionSourceFilter sourceFilter)
            : base(target)
        {
            this.SourceFilter = sourceFilter;
        }

        /// <summary>
        /// The source filter to be applied to any collection source written to this filter. The initial value for
        /// this property is set by the constructor.
        /// </summary>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        public PivotCollectionSourceFilter SourceFilter
        {
            get { return m_sourceFilter; }

            set
            {
                if (value == null) throw new ArgumentNullException("SourceFilter cannot be null");
                m_sourceFilter = value;
            }
        }

        /// <summary>
        /// Applies this adapter's source filter and writes the resulting collection stream into the delegate collection
        /// target.
        /// </summary>
        /// <param name="source">the collection to be written</param>
        public override void Write(IPivotCollectionSource source)
        {
            this.SourceFilter.Source = source;
            this.Target.Write(this.SourceFilter);
        }

        private PivotCollectionSourceFilter m_sourceFilter;
    }
}
