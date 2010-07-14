//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    /// <summary>
    /// DeepZoomTargetFilter alters a collection stream being written through it to include DeepZoom artifacts instead
    /// of raw images.
    /// </summary>
    /// <remarks>
    /// The DeepZoom artifacts are written to the file system adjcent to the base path of the underlying target. This
    /// target filter will create a DeepZoom collection (DZC) for the collection as a whole, and change the collection
    /// source's original <see cref="PivotCollection.ImageBase"/> to refer to the DZC file. This target filter will also
    /// alter the <see cref="PivotItem.Image"/> for each item to refer to the appropriate index in the DZC file.
    /// <para><b>NOTE:</b> This class can be <i>extremely</i> memory intensive for several reasons. First, the memory
    /// usage when creating DeepZoom artifacts inherently scales with the number of images in the collection. Second,
    /// this class attempts to speed up the process of creating DZIs for each item by running several conversion
    /// processes in parallel. Each of these also uses quite a bit of memory. If the application starts to run low on
    /// memory, the class will automatically start shutting down these background threads. If you would like to
    /// proactively control this memory usage, set the <see cref="ThreadCount"/> property.  Naturally, higher numbers
    /// complete the conversion process faster, while lower numbers use less memory.</para>
    /// </remarks>
    public class DeepZoomTargetFilter : PivotCollectionTargetFilter, ILocalCollectionTarget
    {
        /// <summary>
        /// Creates a new DeepZoom target filter using a stub collection target.
        /// </summary>
        /// <remarks>
        /// This stub should be replaced before actually using the new filter.
        /// </remarks>
        public DeepZoomTargetFilter()
            : this(NullCollectionTarget.Instance)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new DeepZoom target filter using the given delegate collection target.
        /// </summary>
        /// <param name="target">the delegate target to which a collection source will be written after it has been
        /// transformed to using DeepZoom artifacts for images</param>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        public DeepZoomTargetFilter(IPivotCollectionTarget target)
            : base(target)
        {
            m_sourceFilter = new DeepZoomSourceFilter(NullCollectionSource.Instance, this.BasePath);
        }

        /// <summary>
        /// The number of background DZI conversion threads used by this filter.
        /// </summary>
        /// <remarks>
        /// Setting this to a higher value will cause DZI creation to go faster (limited by I/O throughput). Setting
        /// this to a lower value will use less memory. By default, this property is set to five threads per processor.
        /// </remarks>
        public int ThreadCount
        {
            get { return m_sourceFilter.ThreadCount; }

            set { m_sourceFilter.ThreadCount = value; }
        }

        /// <summary>
        /// The base path of the underlying delegate collection target.
        /// </summary>
        /// <remarks>
        /// Changing this property will write through to the underlying delegate.
        /// </remarks>
        public String BasePath
        {
            get { return this.GetLocalTarget().BasePath; }

            set { this.GetLocalTarget().BasePath = value; }
        }

        /// <summary>
        /// Transforms the given collection to use DeepZoom artifacts for its images, and then writes the resulting
        /// collection to the underlying collection target delegate.
        /// </summary>
        /// <remarks>
        /// All of the DeepZoom artifacts will be written into a directory named "foo_deepzoom" where "foo" is the file
        /// name (without extension) of the collection target delegate's base path.
        /// </remarks>
        /// <param name="source">the collection source to write</param>
        public override void Write(IPivotCollectionSource source)
        {
            m_sourceFilter.Source = source;
            this.Target.Write(m_sourceFilter);
        }

        private ILocalCollectionTarget GetLocalTarget()
        {
            IPivotCollectionTarget deepestTarget = this.Target;
            while (deepestTarget is PivotCollectionTargetFilter)
            {
                deepestTarget = ((PivotCollectionTargetFilter)deepestTarget).Target;
            }
            if ((deepestTarget is ILocalCollectionTarget) == false)
            {
                throw new ArgumentException("Target chain must end with a ILocalCollectionTarget");
            }
            return (ILocalCollectionTarget) deepestTarget;
        }

        private DeepZoomSourceFilter m_sourceFilter;
    }
}
