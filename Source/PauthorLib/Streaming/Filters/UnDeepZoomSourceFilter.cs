//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Imaging;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    /// <summary>
    /// UnDeepZoomSourceFilter alters a collection source by converting DeepZoom images back into normal JPEG images.
    /// </summary>
    /// <remarks>
    /// This class is actually just a wrapper around the <see cref="UnDeepZoomImageCreator"/> class. Refer to the
    /// documentation for class for full details on the process.
    /// </remarks>
    public class UnDeepZoomSourceFilter : PivotCollectionSourceFilter
    {
        /// <summary>
        /// Creates a new un-DeepZoom source filter with a stub delegate collection source.
        /// </summary>
        /// <remarks>
        /// The stub should be replaced with a real collection source before using the new filter.
        /// </remarks>
        public UnDeepZoomSourceFilter()
            : this(NullCollectionSource.Instance)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new un-DeepZoom source filter with a given delegate collection source.
        /// </summary>
        /// <param name="source">the collection source from which the collection data should be read</param>
        public UnDeepZoomSourceFilter(IPivotCollectionSource source)
            : base(source)
        {
            m_imageCreator = new UnDeepZoomImageCreator();
        }

        /// <summary>
        /// The directory into which the un-DeepZoom'd images will be placed.
        /// </summary>
        /// <remarks>
        /// By default, this directory will be deleted (along with all the images) when this class's
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
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection.
        /// </summary>
        /// <remarks>
        /// This filter deliberately sets this value to null since the collections images will no longer be based upon a
        /// DZC after passing through this filter. See: <see cref="PivotCollection.ImageBase"/>
        /// </remarks>
        public override String ImageBase
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Converts the delegate collection source's images from DeepZoom images to ordinary JPEGs before returning
        /// them.
        /// </summary>
        /// <remarks>
        /// In order to do this, this class must read through the DZC to find the actual image associated with each
        /// item. So long as the order of items in the CXML matches the order in the DZC, this is an O(1) operation for
        /// each item. However, if the order of items in each file differs, this operation degenrates to an O(n)
        /// operation for each item (i.e., O(n^2) overall). If the <see cref="ImageBase"/> of the collection source is
        /// not a file with a "dzc" extension, then the items are returned as-is.
        /// </remarks>
        /// <seealso cref="Microsoft.LiveLabs.Pauthor.Core.PivotCollection.Items"/>
        public override IEnumerable<PivotItem> Items
        {
            get
            {
                String imageBase = this.Source.ImageBase;
                if (imageBase == null) return this.Source.Items;

                imageBase = imageBase.ToLowerInvariant();
                if (imageBase.EndsWith("dzc") == false) return this.Source.Items;

                return this.ProcessItems(imageBase);
            }
        }

        /// <summary>
        /// Disposes of the resources used by this filter.
        /// </summary>
        public override void Dispose()
        {
            if (m_imageCreator != null) m_imageCreator.Dispose();
            m_imageCreator = null;

            if (m_dzcReader != null) m_dzcReader.Close();
            m_dzcReader = null;
        }

        /// <summary>
        /// Disposes of resources used by this filter.
        /// </summary>
        ~UnDeepZoomSourceFilter()
        {
            this.Dispose();
        }

        private IEnumerable<PivotItem> ProcessItems(String imageBase)
        {
            String deepZoomDirectory = Directory.GetParent(imageBase).FullName;

            foreach (PivotItem item in this.Source.Items)
            {
                if ((item.Image == null) || (item.Image.SourcePath.StartsWith("#") == false))
                {
                    yield return item;
                    continue;
                }

                String dzcIndex = item.Image.SourcePath.Substring(1);
                String imageName = this.GetImageName(imageBase, dzcIndex);
                if (imageName == null)
                {
                    item.Image = null;
                }
                else if (imageName.EndsWith("dzi"))
                {
                    String imagePath = Path.Combine(deepZoomDirectory, imageName);
                    item.Image.SourcePath = m_imageCreator.UnDeepZoomImage(imagePath);
                }
                yield return item;
            }
        }

        private String GetImageName(String imageBase, String dzcIndex)
        {
            if (m_dzcReader == null)
            {
                m_dzcReader = XmlReader.Create(imageBase);
                m_dzcReader.MoveToContent();
            }

            int attempts = 0;
            while (attempts <= 1)
            {
                while (m_dzcReader.Read())
                {
                    if (m_dzcReader.NodeType != XmlNodeType.Element) continue;
                    if (m_dzcReader.LocalName != "I") continue;
                    if (m_dzcReader.GetAttribute("Id") != dzcIndex) continue;
                    return m_dzcReader.GetAttribute("Source");
                }

                m_dzcReader.Close();
                m_dzcReader = XmlReader.Create(imageBase);
                m_dzcReader.MoveToContent();
                attempts++;
            }

            return null;
        }

        private UnDeepZoomImageCreator m_imageCreator;

        private XmlReader m_dzcReader;
    }
}
