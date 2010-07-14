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
using System.Xml;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// CxmlCollectionSource provides a streaming collection read from a CXML file.
    /// </summary>
    /// <remarks>
    /// The attributes, facet categories, and items are exactly as provided in the CXML file.
    /// </remarks>
    public class CxmlCollectionSource : AbstractCollectionSource
    {
        /// <summary>
        /// Creates a new CXML collection source given a URI to a CXML file.
        /// </summary>
        /// <param name="baseUri">the URI to a CXML file</param>
        /// <exception cref="ArgumentException">if the given value is null or empty</exception>
        public CxmlCollectionSource(String basePath)
            : base(basePath)
        {
            // Do nothing.
        }

        protected override void LoadHeaderData()
        {
            PivotCollection cachedData = this.CachedCollectionData;

            using (FileStream fileStream = new FileStream(this.BasePath, FileMode.Open, FileAccess.Read))
            {
                using (XmlReader xmlReader = XmlReader.Create(fileStream))
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType != XmlNodeType.Element) continue;

                        if (xmlReader.LocalName == "Collection")
                        {
                            cachedData.Name = xmlReader.GetAttribute("Name");
                            cachedData.AdditionalSearchText = xmlReader.GetAttribute("AdditionalSearchText");
                            cachedData.SchemaVersion = xmlReader.GetAttribute("SchemaVersion");

                            String value = null;
                            if ((value = xmlReader.GetAttribute("Icon", PivotNamespace)) != null)
                            {
                                cachedData.Icon = new PivotImage(new Uri(this.BasePathAsUri, value).ToString());
                            }
                            if ((value = xmlReader.GetAttribute("BrandImage", PivotNamespace)) != null)
                            {
                                cachedData.BrandImage = new PivotImage(new Uri(this.BasePathAsUri, value).ToString());
                            }
                        }
                        else if (xmlReader.LocalName == "Items")
                        {
                            String imageBase = xmlReader.GetAttribute("ImgBase");
                            if (imageBase != null)
                            {
                                cachedData.ImageBase = new Uri(this.BasePathAsUri, imageBase).ToString();
                                xmlReader.Skip();
                            }
                        }
                        else if (xmlReader.LocalName == "Copyright")
                        {
                            cachedData.Copyright =
                                new PivotLink(xmlReader.GetAttribute("Name"), xmlReader.GetAttribute("Href"));
                        }
                        else if (xmlReader.LocalName == "FacetCategory")
                        {
                            PivotFacetCategory facetCategory = this.ParseFacetCategory(xmlReader.ReadSubtree());
                            cachedData.FacetCategories.Add(facetCategory);
                        }
                    }
                }
            }
        }

        protected override IEnumerable<PivotItem> LoadItems()
        {
            using (FileStream fileStream = new FileStream(this.BasePath, FileMode.Open, FileAccess.Read))
            {
                using (XmlReader xmlReader = XmlReader.Create(fileStream))
                {
                    xmlReader.MoveToContent();
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType != XmlNodeType.Element) continue;

                        if (xmlReader.LocalName == "Item")
                        {
                            PivotItem item = this.ParseItem(xmlReader.ReadSubtree());
                            yield return item;
                        }
                    }
                }
            }
        }

        private PivotFacetCategory ParseFacetCategory(XmlReader xmlReader)
        {
            PivotFacetCategory facetCategory = null;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element) continue;

                if (xmlReader.LocalName == "FacetCategory")
                {
                    String name = xmlReader.GetAttribute("Name");
                    String type = xmlReader.GetAttribute("Type");
                    facetCategory = new PivotFacetCategory(name, PivotFacetType.Parse(type));

                    String value = null;
                    if ((value = xmlReader.GetAttribute("Format")) != null)
                    {
                        facetCategory.Format = value;
                    }
                    if ((value = xmlReader.GetAttribute("IsFilterVisible", PivotNamespace)) != null)
                    {
                        facetCategory.IsFilterVisible = Boolean.Parse(value);
                    }
                    if ((value = xmlReader.GetAttribute("IsMetaDataVisible", PivotNamespace)) != null)
                    {
                        facetCategory.IsMetaDataVisible = Boolean.Parse(value);
                    }
                    if ((value = xmlReader.GetAttribute("IsWordWheelVisible", PivotNamespace)) != null)
                    {
                        facetCategory.IsWordWheelVisible = Boolean.Parse(value);
                    }
                }
                else if (xmlReader.LocalName == "SortOrder")
                {
                    if (facetCategory == null) continue;

                    String name = xmlReader.GetAttribute("Name");
                    facetCategory.SortOrder = new PivotFacetSortOrder(name);
                }
                else if (xmlReader.LocalName == "SortValue")
                {
                    if (facetCategory == null) continue;
                    if (facetCategory.SortOrder == null) continue;

                    facetCategory.SortOrder.AddValue(xmlReader.GetAttribute("Value"));
                }
            }

            return facetCategory;
        }

        private PivotItem ParseItem(XmlReader xmlReader)
        {
            PivotCollection cachedData = this.CachedCollectionData;

            PivotItem item = null;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element) continue;

                if (xmlReader.LocalName == "Item")
                {
                    String id = xmlReader.GetAttribute("Id");
                    item = new PivotItem(id, this);

                    String imagePath = xmlReader.GetAttribute("Img");
                    if (imagePath != null)
                    {
                        if (cachedData.ImageBase == null)
                        {
                            imagePath = new Uri(this.BasePathAsUri, imagePath).ToString();
                        }
                        item.Image = new PivotImage(imagePath);
                    }

                    item.Name = xmlReader.GetAttribute("Name");
                    item.Href = xmlReader.GetAttribute("Href");
                }
                else if (xmlReader.LocalName == "Description")
                {
                    item.Description = xmlReader.ReadElementContentAsString();
                }
                else if (xmlReader.LocalName == "Facet")
                {
                    this.ParseFacet(xmlReader.ReadSubtree(), item);
                }
                else if (xmlReader.LocalName == "Related")
                {
                    List<PivotLink> relatedLinks = this.ParseRelatedLinks(xmlReader.ReadSubtree());
                    item.RelatedLinks = relatedLinks;
                }
            }
            return item;
        }

        private void ParseFacet(XmlReader xmlReader, PivotItem item)
        {
            PivotCollection cachedData = this.CachedCollectionData;

            String facetCategoryName = null;
            PivotFacetType facetType = null;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element) continue;

                if (xmlReader.LocalName == "Facet")
                {
                    facetCategoryName = xmlReader.GetAttribute("Name");
                    PivotFacetCategory facetCategory = cachedData.FacetCategories[facetCategoryName];
                    facetType = facetCategory.Type;
                }
                else if ((facetType != null) && (xmlReader.LocalName == facetType.ToString()))
                {
                    if (facetType == PivotFacetType.Link)
                    {
                        PivotLink link = new PivotLink(xmlReader.GetAttribute("Name"), xmlReader.GetAttribute("Href"));
                        item.AddFacetValues(facetCategoryName, link);
                    }
                    else
                    {
                        String value = xmlReader.GetAttribute("Value");
                        item.AddFacetValues(facetCategoryName, facetType.ParseValue(value));
                    }
                }
            }
        }

        private List<PivotLink> ParseRelatedLinks(XmlReader xmlReader)
        {
            List<PivotLink> relatedLinks = new List<PivotLink>();
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element) continue;

                if (xmlReader.LocalName == "Link")
                {
                    PivotLink link = new PivotLink(xmlReader.GetAttribute("Name"), xmlReader.GetAttribute("Href"));
                    relatedLinks.Add(link);
                }
            }
            return relatedLinks;
        }

        private Uri BasePathAsUri
        {
            get
            {
                if (m_basePathAsUri == null)
                {
                    if (File.Exists(this.BasePath))
                    {
                        m_basePathAsUri = new Uri(Path.GetFullPath(this.BasePath));
                    }
                    else
                    {
                        m_basePathAsUri = new Uri(this.BasePath);
                    }
                }
                return m_basePathAsUri;
            }
        }

        private const String CollectionNamespace = "http://schemas.microsoft.com/collection/metadata/2009";

        private const String PivotNamespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009";

        private Uri m_basePathAsUri;
    }
}
