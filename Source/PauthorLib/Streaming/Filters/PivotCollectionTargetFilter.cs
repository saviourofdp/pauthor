//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    /// <summary>
    /// PivotCollectionTargetFilter provides an abstract base class for classes which wish to alter collection streams
    /// as they are written into a collection target.
    /// </summary>
    /// <remarks>
    /// Each collection target filter requires a delegate collection target into which the transformed collection is
    /// written. This implementation writes the data from the source collection exactly as-is. Subclasses should
    /// override the <see cref="Write"/> method to make whatever modifications they would like to the source collection,
    /// and then write the collection into the delegate collection target.
    /// </remarks>
    public class PivotCollectionTargetFilter : IPivotCollectionTarget
    {
        /// <summary>
        /// Creates a new collection target filter with a stub delegate collection target.
        /// </summary>
        /// <remarks>
        /// The stub is actually an instance of <see cref="NullCollectionTarget"/> which simply does nothing when its
        /// <see cref="Write"/> method is called. This stub delegate should be replaced with a real collection target
        /// before the new filter is used.
        /// </remarks>
        public PivotCollectionTargetFilter()
            : this(NullCollectionTarget.Instance)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new collection target filter with a given delegate collection target.
        /// </summary>
        /// <param name="target">the collection target to which the transformed source collection should be
        /// written</param>
        public PivotCollectionTargetFilter(IPivotCollectionTarget target)
        {
            this.Target = target;
        }

        /// <summary>
        /// The delegate collection target to which the transformed source collection is written.
        /// </summary>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        public IPivotCollectionTarget Target
        {
            get { return m_target; }

            private set
            {
                if (value == null) throw new ArgumentNullException("Target");
                m_target = value;
            }
        }

        /// <summary>
        /// Writes a given collection source to the delegate collection target.
        /// </summary>
        /// <remarks>
        /// Subclasses should override this method to make whatever transformations are needed, and then write the
        /// collection through to the delegate collection target.
        /// </remarks>
        /// <param name="source">the source collection to be written</param>
        public virtual void Write(IPivotCollectionSource source)
        {
            this.Target.Write(source);
        }

        /// <summary>
        /// Disposes of any resources used by this collection target filter.
        /// </summary>
        /// <remarks>
        /// This method is also echoed through to the delegate collection target.
        /// </remarks>
        public virtual void Dispose()
        {
            this.Target.Dispose();
        }

        private IPivotCollectionTarget m_target;
    }
}
