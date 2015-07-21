using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.LiveLabs.Pauthor.Streaming.Filters;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Kantar.DataSolutions.Pauthor.Streaming.Filters
{
    public class SourceImageInPlaceTargetFilter : PivotCollectionTargetFilter
    {
        /// <summary>
        /// Creates a source image copy target filter using a stub delegate collection source.
        /// </summary>
        /// <remarks>
        /// The stub should be replaced with a real collection source before using the new filter.
        /// </remarks>
        public SourceImageInPlaceTargetFilter()
            : this(NullCollectionTarget.Instance)
        {
            // nothing
        }

        /// <summary>
        /// Creates a source target filter with a given delegate collection target where the image paths
        /// are copied accross verbatim. No images are copied
        /// </summary>
        /// <param name="target">the collection target to which the collection should be written 
        /// </param>
        public SourceImageInPlaceTargetFilter(IPivotCollectionTarget target)
            : base(target)
        {
            // Do nothing.
        }
    }
}
