//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.LiveLabs.Pauthor.Imaging
{
    /// <summary>
    /// ImageCreator is an abstract parent class for objects which deal with creating images.
    /// </summary>
    /// <remarks>
    /// This class defines a number of functions for managing the working directory for those objects.
    /// </remarks>
    public abstract class ImageCreator : IDisposable
    {
        /// <summary>
        /// Creates a new image creator.
        /// </summary>
        public ImageCreator()
        {
            this.WorkingDirectory = Path.Combine(Path.GetTempPath(),
                this.GetType().Name + "-" + Guid.NewGuid().ToString());
            this.ShouldDeleteWorkingDirectory = true;
        }

        /// <summary>
        /// The working directory for this image creator.
        /// </summary>
        /// <remarks>
        /// Subclasses should keep whatever temporary results, cached files, or other working files they need in this
        /// directory. Any time this property is changed, the  <see cref="ShouldDeleteWorkingDirectory"/> property is
        /// set to false. By default, this property is set to a new, uniquely named directory within the current user's
        /// temp directory.
        /// </remarks>
        public String WorkingDirectory
        {
            get { return m_workingDirectory; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("WorkingDirectory cannot be null");
                if ((m_workingDirectory != null) && (m_shouldDeleteWorkingDirectory))
                {
                    Directory.Delete(m_workingDirectory, true);
                }

                if (Directory.Exists(value) == false)
                {
                    Directory.CreateDirectory(value);
                }
                m_workingDirectory = value;
                this.ShouldDeleteWorkingDirectory = false;
            }
        }

        /// <summary>
        /// Whether this image creator should remove its working directory when it is disposed.
        /// </summary>
        /// <remarks>
        /// By default, this is set to true, but any time the <see cref="WorkingDirectory"/> is changed, this property
        /// will be reset to false.
        /// </remarks>
        public bool ShouldDeleteWorkingDirectory
        {
            get { return m_shouldDeleteWorkingDirectory; }

            set { m_shouldDeleteWorkingDirectory = value; }
        }

        /// <summary>
        /// Destroys the <see cref="WorkingDirectory"/> if the <see cref="ShouldDeleteWorkingDirectory"/> is true.
        /// </summary>
        public virtual void Dispose()
        {
            if (this.ShouldDeleteWorkingDirectory == false) return;
            Directory.Delete(this.WorkingDirectory, true);
            this.ShouldDeleteWorkingDirectory = false;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of resources used by this image creator.
        /// </summary>
        ~ImageCreator()
        {
            this.Dispose();
        }

        /// <summary>
        /// Constant for Bmp ImageCodecInfo index.
        /// </summary>
        protected const int BmpEncoderIndex = 0;

        /// <summary>
        /// Constant for Jpeg ImageCodecInfo index.
        /// </summary>
        protected const int JpegEncoderIndex = 1;

        /// <summary>
        /// Constant for Gif ImageCodecInfo index.
        /// </summary>
        protected const int GifEncoderIndex = 2;

        /// <summary>
        /// Constant for Tiff ImageCodecInfo index.
        /// </summary>
        protected const int TiffEncoderIndex = 3;

        /// <summary>
        /// Constant for Png ImageCodecInfo index.
        /// </summary>
        protected const int PngEncoderIndex = 4;

        /// <summary>
        /// Constant for Image file ImageCodecInfo index.
        /// </summary>
        protected const int StandardImageEncoder = PngEncoderIndex;
        
        /// <summary>
        /// Constant for the Image file format extension
        /// </summary>
        protected const String StandardImageFormatExtension = ".png";

        private String m_workingDirectory;

        private bool m_shouldDeleteWorkingDirectory;
    }
}
