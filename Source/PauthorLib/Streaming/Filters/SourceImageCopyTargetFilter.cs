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

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    /// <summary>
    /// SourceImageCopyTargetFilter alters a collection stream by copying the images from their current location to a
    /// directory adjcent to the collection target.
    /// </summary>
    /// <remarks>
    /// This is useful when copying a full collection from one place to another, or when using the <see
    /// cref="HtmlImageCreationSourceFilter"/> to generate images for a collection.
    /// 
    /// <para>Each image from the collection source will be placed in a "<i>name</i>_images" directory adjcent to the
    /// base path of the collection target (where <i>name</i> is the file name (without extension) of the base path).
    /// </para>
    /// 
    /// <para>This filter only copies files which actually exist in the file system. In particular, this means that 
    /// DeepZoom images cannot be copied using this filter (since the image is merely an index into the DZC).</para>
    /// 
    /// <para>This filter may also be used to copy only the collection-level images (i.e., the
    /// <see cref="PivotCollection.Icon"/> and <see cref="PivotCollection.BrandImage"/>) by setting the
    /// <see cref="OnlyCopyCollectionImages"/> property to true.</para>
    /// </remarks>
    public class SourceImageCopyTargetFilter : PivotCollectionTargetFilter
    {
        /// <summary>
        /// Creates a source image copy target filter using a stub delegate collection source.
        /// </summary>
        /// <remarks>
        /// The stub should be replaced with a real collection source before using the new filter.
        /// </remarks>
        public SourceImageCopyTargetFilter()
            : this(NullCollectionTarget.Instance)
        {
            this.OnlyCopyCollectionImages = false;
        }

        /// <summary>
        /// Creates a source image copy target filter with a given delegate collection target.
        /// </summary>
        /// <param name="target">the collection target to which the collection should be written after the source images
        /// have been copied</param>
        public SourceImageCopyTargetFilter(IPivotCollectionTarget target)
            : base(target)
        {
            // Do nothing.
        }

        /// <summary>
        /// Whether only collection-level images should be copied (i.e., icon and brand image), or all images should be
        /// copied (i.e., the image for each item as well).
        /// </summary>
        /// <remarks>
        /// When set to true, only collection-level images will be copied; when set to false, all images will be copied.
        /// By default, this property is set to false (i.e., all images are copied).
        /// </remarks>
        public bool OnlyCopyCollectionImages
        {
            get { return m_onlyCopyCollectionImages; }

            set { m_onlyCopyCollectionImages = value; }
        }

        /// <summary>
        /// Copies the collection sources images (as determined by the <see cref="OnlyCopyCollectionImages"/> property),
        /// and then writes the collection to the delegate collection target.
        /// </summary>
        /// <remarks>
        /// Once the image files are copied, the appropriate changes are also made to the collection (and images, if
        /// appropriate) to refer to the new image files instead of those specified in the collection source.
        /// </remarks>
        /// <param name="source">the collection source to be written</param>
        public override void Write(IPivotCollectionSource source)
        {
            SourceImageCopySourceFilter sourceFilter = new SourceImageCopySourceFilter(source, this.GetLocalTarget());
            sourceFilter.OnlyCopyCollectionImages = this.OnlyCopyCollectionImages;
            base.Write(sourceFilter);
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

            return (ILocalCollectionTarget)deepestTarget;
        }

        private bool m_onlyCopyCollectionImages;

        private class SourceImageCopySourceFilter : PivotCollectionSourceFilter
        {
            public SourceImageCopySourceFilter(IPivotCollectionSource source, ILocalCollectionTarget localTarget)
                : base(source)
            {
                m_localTarget = localTarget;
                m_onlyCopyCollectionImages = false;
            }

            public bool OnlyCopyCollectionImages
            {
                get { return m_onlyCopyCollectionImages; }

                set { m_onlyCopyCollectionImages = value; }
            }

            public override PivotImage Icon
            {
                get { return this.CopyCollectionImage(this.Source.Icon, "_icon"); }
            }

            public override PivotImage BrandImage
            {
                get { return this.CopyCollectionImage(this.Source.BrandImage, "_brand"); }
            }

            public override IEnumerable<PivotItem> Items
            {
                get
                {
                    if (this.OnlyCopyCollectionImages) return this.Source.Items;
                    return this.CopyImages(this.Source.Items);
                }
            }

            private IEnumerable<PivotItem> CopyImages(IEnumerable<PivotItem> items)
            {
                String newImageDirectoryName = Path.GetFileNameWithoutExtension(m_localTarget.BasePath) + "_images";
                String newImageDirectoryPath = Path.Combine(
                    Directory.GetParent(m_localTarget.BasePath).FullName, newImageDirectoryName);
                if (Directory.Exists(newImageDirectoryPath) == false)
                {
                    Directory.CreateDirectory(newImageDirectoryPath);
                }

                foreach (PivotItem item in this.Source.Items)
                {
                    if (item.Image != null)
                    {
                        String imageFileName = Path.GetFileName(item.Image.SourcePath);
                        String newImagePath = Path.Combine(newImageDirectoryPath, imageFileName);
                        File.Copy(item.Image.SourcePath, newImagePath, true);

                        String newRelativeFileName = Path.Combine(newImageDirectoryName, imageFileName);
                        item.Image.SourcePath = newRelativeFileName;
                    }
                    yield return item;
                }
            }

            private PivotImage CopyCollectionImage(PivotImage sourceImage, String suffix)
            {
                if (sourceImage == null) return null;

                String sourceFilePath = sourceImage.SourcePath;
                String targetFileName = Path.GetFileNameWithoutExtension(m_localTarget.BasePath);
                targetFileName += suffix + Path.GetExtension(sourceFilePath);
                String targetDirectoryPath = Directory.GetParent(m_localTarget.BasePath).FullName;
                String targetFilePath = Path.Combine(targetDirectoryPath, targetFileName);
                if (File.Exists(targetFilePath) == false)
                {
                    File.Copy(sourceFilePath, targetFilePath);
                }

                PivotImage newImage = new PivotImage(targetFileName);
                return newImage;
            }

            private ILocalCollectionTarget m_localTarget;

            private bool m_onlyCopyCollectionImages;
        }
    }
}
