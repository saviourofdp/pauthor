//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Imaging;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    /// <summary>
    /// HtmlImageCreationSourceFilter alters a collection by adding new images for each item based upon an HTML
    /// template.
    /// </summary>
    /// <remarks>
    /// This class simply wraps an <see cref="Microsoft.LiveLabs.Pauthor.Imaging.HtmlImageCreator"/> which does the
    /// actual work of creating images. For full details on the HTML template, or the image creation process, see the
    /// documentation for that class.
    /// </remarks>
    public class HtmlImageCreationSourceFilter : PivotCollectionSourceFilter
    {
        /// <summary>
        /// Creates a new HTML image creation source filter with a stub collection source delegate.
        /// </summary>
        /// <remarks>
        /// The stub should be replaced with an actual collection source before using the new filter.
        /// </remarks>
        public HtmlImageCreationSourceFilter()
            : this(NullCollectionSource.Instance)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates an HTML image creation source filter with a given collection source delegate.
        /// </summary>
        /// <param name="source">the collection source from which this filter's data will be drawn</param>
        public HtmlImageCreationSourceFilter(IPivotCollectionSource source)
            : base(source)
        {
            m_imageCreator = new HtmlImageCreator();
        }

        /// <summary>
        /// The directory into which the generated images will be placed.
        /// </summary>
        /// <remarks>
        /// By default, this directory will be deleted (along with all the generated images) when this class's
        /// <see cref="Dispose"/> method is called. This may be prevented in one of two ways: change the working
        /// directory to one of your own choosing, or use a target or target filter which creates a copy of the images
        /// (e.g., <see cref="DeepZoomTargetFilter"/> or <see cref="SourceImageCopyTargetFilter"/>). By default, this
        /// property is set to a uniquely named directory in the user's temp directory.
        /// </remarks>
        public String WorkingDirectory
        {
            get { return m_imageCreator.WorkingDirectory; }

            set { m_imageCreator.WorkingDirectory = value; }
        }

        /// <summary>
        /// The HTML template used to create images.
        /// </summary>
        /// <seealso cref="Microsoft.LiveLabs.Pauthor.Imaging.HtmlImageCreator.HtmlTemplate"/>
        public String HtmlTemplate
        {
            get { return m_imageCreator.HtmlTemplate; }

            set { m_imageCreator.HtmlTemplate = value; }
        }


        /// <summary>
        /// The path to an HTML file which should be used as the template for this image creator.
        /// </summary>
        /// <seealso cref="Microsoft.LiveLabs.Pauthor.Imaging.HtmlImageCreator.HtmlTemplatePath"/>
        public String HtmlTemplatePath
        {
            get { return m_imageCreator.HtmlTemplatePath; }

            set { m_imageCreator.HtmlTemplatePath = value; }
        }

        /// <summary>
        /// The height (in pixels) at which to render images.
        /// </summary>
        /// <seealso cref="Microsoft.LiveLabs.Pauthor.Imaging.HtmlImageCreator.Height"/>
        public int Height
        {
            get { return m_imageCreator.Height; }

            set { m_imageCreator.Height = value; }
        }

        /// <summary>
        /// The width (in pixels) at which to render images.
        /// </summary>
        /// <seealso cref="Microsoft.LiveLabs.Pauthor.Imaging.HtmlImageCreator.Width"/>
        public int Width
        {
            get { return m_imageCreator.Width; }

            set { m_imageCreator.Width = value; }
        }

        /// <summary>
        /// Whether images should always be created, or if they should only be created if the item is missing an image.
        /// </summary>
        /// <remarks>
        /// If this property is set to true, then items which already have images will be left as-is. If set to false,
        /// all items will receive new images based upon this filter's HTML template. By default, this property is set
        /// to false (i.e., all items will receive new images).
        /// </remarks>
        public bool OnlyMissingImage
        {
            get { return m_onlyMissingImage; }

            set { m_onlyMissingImage = value; }
        }

        /// <summary>
        /// An enumeration of all the items in this collection.
        /// </summary>
        /// <remarks>
        /// Each item will have its current image replaced with a new image based upon the HTML template assigned to
        /// this class (depending upon the <see cref="OnlyMissingImage"/> property).
        /// </remarks>
        /// <seealso cref="PivotCollection.Items"/>
        public override IEnumerable<PivotItem> Items
        {
            get
            {
                foreach (PivotItem item in this.Source.Items)
                {
                    if ((m_onlyMissingImage == false) || (item.Image == null))
                    {
                        m_imageCreator.AssignImage(item);
                    }
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Disposes of resources used by this source filter.
        /// </summary>
        public override void Dispose()
        {
            if (m_imageCreator != null)
            {
                m_imageCreator.Dispose();
                m_imageCreator = null;
            }
        }

        private HtmlImageCreator m_imageCreator;

        private bool m_onlyMissingImage;
    }
}
