//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.Net;

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Crawling;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.RssCrawler
{
    public class RssCollectionSource : AbstractCollectionSource
    {
        public RssCollectionSource(String rssFeedUrl)
            : base(rssFeedUrl)
        {
            // Do nothing.
        }

        protected override void LoadHeaderData()
        {
            this.CachedCollectionData.FacetCategories.Add(new PivotFacetCategory("Author", PivotFacetType.String));
            this.CachedCollectionData.FacetCategories.Add(new PivotFacetCategory("Category", PivotFacetType.String));
            this.CachedCollectionData.FacetCategories.Add(new PivotFacetCategory("Date", PivotFacetType.DateTime));

            XPathHelper document = null;
            using (WebClient webClient = new WebClient())
            {
                document = new XPathHelper(webClient.DownloadString(this.BasePath));
            }

            String value = null;
            if (document.TryFindString("//channel/title", out value))
            {
                this.CachedCollectionData.Name = value;
            }

            if (document.TryFindString("//channel/link", out value))
            {
                this.CachedCollectionData.Copyright = new PivotLink("Source", value);
            }
        }

        protected override IEnumerable<PivotItem> LoadItems()
        {
            XPathHelper document = null;
            using (WebClient webClient = new WebClient())
            {
                document = new XPathHelper(webClient.DownloadString(this.BasePath));
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
