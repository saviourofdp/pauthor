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

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Imaging;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    public class DeepZoomSourceFilter : PivotCollectionSourceFilter
    {
        public DeepZoomSourceFilter(IPivotCollectionSource source, String targetBasePath)
            : base(source)
        {
            String basePath = Directory.GetParent(targetBasePath).FullName;
            String baseFileName = Path.GetFileNameWithoutExtension(targetBasePath);
            String deepZoomDirectoryName = String.Format(DeepZoomDirectoryTemplate, baseFileName);
            m_dzcRelativePath = Path.Combine(deepZoomDirectoryName,
                String.Format(DzcFileNameTemplate, baseFileName));
            String dzcPath = Path.Combine(basePath, m_dzcRelativePath);

            m_imageCreator = new ParallelDeepZoomCreator(dzcPath);
        }

        public override String ImageBase
        {
            get { return m_dzcRelativePath; }
        }

        public override IEnumerable<PivotItem> Items
        {
            get
            {
                m_imageCreator.Start();

                foreach (PivotItem item in this.Source.Items)
                {
                    if (item.Image != null)
                    {
                        int index = m_imageCreator.Submit(item.Image);
                        item.Image.SourcePath = "#" + index;
                    }

                    yield return item;
                }

                m_imageCreator.Join();
            }
        }

        public int ThreadCount
        {
            get { return m_imageCreator.ThreadCount; }

            set { m_imageCreator.ThreadCount = value; }
        }

        private const String DeepZoomDirectoryTemplate = "{0}_deepzoom";

        private const String DzcFileNameTemplate = "{0}.dzc";

        private String m_dzcRelativePath;

        private ParallelDeepZoomCreator m_imageCreator;
    }
}
