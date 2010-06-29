//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.IO;

namespace Microsoft.LiveLabs.Pauthor.Core
{
    /// <summary>
    /// PivotImage is an image associated with a Pivot collection.
    /// </summary>
    /// <remarks>
    /// Images are associated with both the collection as a whole by the <see cref="PivotCollection.Icon"/> and
    /// <see cref="PivotCollection.BrandImage"/> properties, and with individual items by the <see
    /// cref="PivotItem.Image"/> property.
    /// </remarks>
    public class PivotImage
    {
        /// <summary>
        /// Creates a new image with a given source path.
        /// </summary>
        /// <param name="sourcePath">the location on disk where the image data is stored</param>
        /// <exception cref="ArgumentException">if the given value is null or empty</exception>
        public PivotImage(String sourcePath)
        {
            this.SourcePath = sourcePath;
        }

        /// <summary>
        /// A string which specifies where to find the image.
        /// </summary>
        /// <remarks>
        /// This may be an absolute path to a file on disk, a path relative to the CXML or Excel file, or even an index
        /// into a DZC file depending upon the context in which the image is used.
        /// </remarks>
        /// <exception cref="ArgumentException">if the given value is null or empty</exception>
        public String SourcePath
        {
            get { return m_sourcePath; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("SourcePath cannot be null or empty");
                m_sourcePath = value;
            }
        }

        /// <summary>
        /// Determines whether the source path is an actual file on disk which is readable.
        /// </summary>
        /// <returns>true if the source path can be read, false otherwise</returns>
        public bool IsSourcePathReadable()
        {
            return File.Exists(this.SourcePath);
        }

        private String m_sourcePath;
    }
}
