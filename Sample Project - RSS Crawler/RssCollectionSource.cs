//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Crawling;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.RssCrawler
{
    public class RssCollectionSource : IPivotCollectionSource
    {
        public RssCollectionSource(String rssFeedUrl)
        {
            this.RssFeedUrl = rssFeedUrl;
        }

        public String RssFeedUrl
        {
            get { return m_rssFeedUrl; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("RssFeedUrl");
                m_rssFeedUrl = value;
            }
        }

        public String AdditionalSearchText
        {
            get { return this.CachedCollectionData.AdditionalSearchText; }
        }

        public PivotImage BrandImage
        {
            get { return this.CachedCollectionData.BrandImage; }
        }

        public PivotLink Copyright
        {
            get { return this.CachedCollectionData.Copyright; }
        }

        public IReadablePivotList<String, PivotFacetCategory> FacetCategories
        {
            get { return this.CachedCollectionData.FacetCategories; }
        }

        public PivotImage Icon
        {
            get { return this.CachedCollectionData.Icon; }
        }

        public String ImageBase
        {
            get { return this.CachedCollectionData.ImageBase; }
        }

        public IEnumerable<PivotItem> Items
        {
            get { return this.DownloadItems(); }
        }

        public String Name
        {
            get { return this.CachedCollectionData.Name; }
        }

        public String SchemaVersion
        {
            get { return this.CachedCollectionData.SchemaVersion; }
        }

        public void Dispose()
        {
            // Do nothing.
        }

        private PivotCollection CachedCollectionData
        {
            get
            {
                if (m_cachedCollectionData == null)
                {
                    m_cachedCollectionData = this.DownloadCollectionData();
                }
                return m_cachedCollectionData;
            }
        }

        private PivotCollection DownloadCollectionData()
        {
            PivotCollection collection = new PivotCollection();
            collection.FacetCategories.Add(new PivotFacetCategory("Author", PivotFacetType.String));
            collection.FacetCategories.Add(new PivotFacetCategory("Category", PivotFacetType.String));
            collection.FacetCategories.Add(new PivotFacetCategory("Date", PivotFacetType.DateTime));

            XPathHelper document = null;
            using (WebClient webClient = new WebClient())
            {
                document = new XPathHelper(webClient.DownloadString(this.RssFeedUrl));
            }

            String value = null;
            if (document.TryFindString("//channel/title", out value))
            {
                collection.Name = value;
            }

            if (document.TryFindString("//channel/link", out value))
            {
                collection.Copyright = new PivotLink("Source", value);
            }

            return collection;
        }

        private IEnumerable<PivotItem> DownloadItems()
        {
            XPathHelper document = null;
            using (WebClient webClient = new WebClient())
            {
                document = new XPathHelper(webClient.DownloadString(this.RssFeedUrl));
            }

            int index = 0;
            foreach (XPathHelper itemNode in document.FindNodes("//item"))
            {
                PivotItem item = new PivotItem(index.ToString(), this);

                String value = null;
                if (itemNode.TryFindString("title", out value))
                {
                    item.Name = value;
                }

                if (itemNode.TryFindString("description", out value))
                {
                    item.Description = value;
                }

                if (itemNode.TryFindString("link", out value))
                {
                    item.Href = value;
                }

                if (itemNode.TryFindString("author", out value))
                {
                    item.AddFacetValues("Author", value);
                }

                foreach (XPathHelper categoryNode in itemNode.FindNodes("category"))
                {
                    item.AddFacetValues("Category", categoryNode.FindString("."));
                }

                if (itemNode.TryFindString("pubDate", out value))
                {
                    DateTime dateValue = DateTime.Now;
                    if (DateTime.TryParse(value, out dateValue))
                    {
                        item.AddFacetValues("Date", dateValue);
                    }
                }

                yield return item;
                index++;
            }
        }

        private String m_rssFeedUrl;

        private PivotCollection m_cachedCollectionData;
    }
}
