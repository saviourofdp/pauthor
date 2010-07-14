//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;

namespace Microsoft.LiveLabs.Pauthor
{
    /// <summary>
    /// PauthorException is a base class for exceptions thrown by Pauthor.
    /// </summary>
    public class PauthorException : Exception
    {
        /// <summary>
        /// Creates a new Pauthor exception.
        /// </summary>
        public PauthorException()
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new Pauthor exception with an explainatory message.
        /// </summary>
        /// <param name="message">a string describing the nature of the problem</param>
        public PauthorException(String message)
            : base(message)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new Pauthor exception wrapping an underlying exception with an explainatory message.
        /// </summary>
        /// <param name="message">a string describing the nature of the problem</param>
        /// <param name="innerException">the underlying exception which caused the problem</param>
        public PauthorException(String message, Exception innerException)
            : base(message, innerException)
        {
            // Do nothing.
        }
    }
}
