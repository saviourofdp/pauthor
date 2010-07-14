//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// NullCollectionTarget is a stub implementation of <see cref="IPivotCollectionTarget"/>.
    /// </summary>
    /// <remarks>
    /// Instead of writing a collection stream to a particular target, it simply does nothing. This class can be used as
    /// a placeholder target whereever one is required but the actual target is not yet available.
    /// </remarks>
    public class NullCollectionTarget : IPivotCollectionTarget
    {
        /// <summary>
        /// A shared instance of this collection target.
        /// </summary>
        /// <remarks>
        /// Since this collection target is immutable, there is no need to create multiple instances. Users of this
        /// class should refer this this single instance instead whereever possible.
        /// </remarks>
        public static readonly NullCollectionTarget Instance = new NullCollectionTarget();

        /// <summary>
        /// Abstains from writing the given collection source anywhere.
        /// </summary>
        /// <param name="source">the source which will not be written anywhere</param>
        public void Write(IPivotCollectionSource source)
        {
            // Do nothing.
        }

        /// <summary>
        /// Disposes of the resources used by this collection target.
        /// </summary>
        public void Dispose()
        {
            // Do nothing.
        }
    }
}
