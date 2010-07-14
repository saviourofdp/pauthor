//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.IO;
using System.Net;

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
                String sourcePath = null;
                Uri sourceUri = null;

                this.IsDeepZoomIndex = false;
                int index = 0;
                if (value.StartsWith("#") && (value.Length >= 2) && (int.TryParse(value.Substring(1), out index)))
                {
                    this.IsDeepZoomIndex = true;
                    this.IsLocalFile = false;
                    this.IsRemoteFile = false;
                    sourcePath = value;
                }

                if (sourcePath == null)
                {
                    if (Uri.TryCreate(value, UriKind.Absolute, out sourceUri))
                    {
                        this.IsLocalFile = sourceUri.IsFile;
                        this.IsRemoteFile = !this.IsLocalFile;
                        sourcePath = value;
                    }
                }

                if (sourcePath == null)
                {
                    if (Uri.TryCreate(value, UriKind.Relative, out sourceUri))
                    {
                        this.IsLocalFile = File.Exists(value);
                        this.IsRemoteFile = !this.IsLocalFile;
                        sourcePath = value;
                    }
                }

                if (this.IsLocalFile)
                {
                    sourcePath = sourceUri.LocalPath;
                }

                if (sourcePath == null)
                {
                    throw new ArgumentException("Could not parse SourcePath: " + value);
                }

                m_sourcePath = sourcePath;
                m_sourceUri = sourceUri;
            }
        }

        public bool IsLocalFile
        {
            get;
            private set;
        }

        public bool IsRemoteFile
        {
            get;
            private set;
        }

        public bool IsDeepZoomIndex
        {
            get;
            private set;
        }

        public void EnsureLocal()
        {
            if (this.IsLocalFile) return;

            String tempPath = Path.GetTempFileName();
            this.Save(tempPath);
            m_shouldDelete = true;
            m_sourcePath = tempPath;
        }

        public void Save(String targetPath)
        {
            try
            {
                if (this.IsLocalFile)
                {
                    File.Copy(m_sourcePath, targetPath, true);
                }
                else if (this.IsRemoteFile)
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.DownloadFile(m_sourcePath, targetPath);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot save image to " + targetPath);
                }
            }
            catch (Exception e)
            {
                throw new IOException("Could not save image to " + targetPath, e);
            }
        }

        /// <summary>
        /// Determines whether the source path is an actual file on disk which is readable.
        /// </summary>
        /// <returns>true if the source path can be read, false otherwise</returns>
        [Obsolete("Use IsLocalFile, IsRemoteFile, or IsDeepZoomIndex instead")]
        public bool IsSourcePathReadable()
        {
            return File.Exists(this.SourcePath);
        }

        ~PivotImage()
        {
            if (m_shouldDelete == false) return;
            if (File.Exists(m_sourcePath) == false) return;
            File.Delete(m_sourcePath);
        }

        private Uri m_sourceUri;

        private String m_sourcePath;

        private bool m_shouldDelete;
    }
}
