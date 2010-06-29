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
using System.Xml.Linq;
using System.Xml.Schema;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// LocalCxmlCollectionSource provides a streaming collection read from a CXML file stored locally on disk.
    /// </summary>
    /// <remarks>
    /// The attributes, facet categories, and items are exactly as provided in the CXML file.
    /// </remarks>
    public class LocalCxmlCollectionSource : ILocalCollectionSource
    {
        /// <summary>
        /// Creates a new local CXML collection source given the path to a CXML file.
        /// </summary>
        /// <param name="basePath">the path to a CXML file</param>
        /// <exception cref="ArgumentException">if the given value is null or empty</exception>
        public LocalCxmlCollectionSource(String basePath)
        {
            this.BasePath = basePath;
        }

        /// <summary>
        /// The path on the local disk (relative or absolute) where this collection source's CXML file resides.
        /// </summary>
        /// <remarks>
        /// The initial value for this property is set by the constructor.
        /// </remarks>
        /// <exception cref="ArgumentException">if the given value is null or empty</exception>
        public String BasePath
        {
            get { return m_basePath; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BasePath cannot be null or empty");
                m_basePath = value;
                m_baseDirectory = Directory.GetParent(m_basePath).FullName;
            }
        }

        /// <summary>
        /// The user-facing title of this collection.
        /// </summary>
        /// <see cref="PivotCollection.Name"/>
        public String Name
        {
            get { return this.CachedCollectionData.Name; }
        }

        /// <summary>
        /// The favicon associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Icon"/>
        public PivotImage Icon
        {
            get { return this.CachedCollectionData.Icon; }
        }

        /// <summary>
        /// The branding image associated with this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.BrandImage"/>
        public PivotImage BrandImage
        {
            get { return this.CachedCollectionData.BrandImage; }
        }

        /// <summary>
        /// The additional text to be appended to an item's name when requesting search results from Bing.
        /// </summary>
        /// <seealso cref="PivotCollection.AdditionalSearchText"/>
        public String AdditionalSearchText
        {
            get { return this.CachedCollectionData.AdditionalSearchText; }
        }

        /// <summary>
        /// The version number of the Pivot schema used to represent this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.SchemaVersion"/>
        public String SchemaVersion
        {
            get { return this.CachedCollectionData.SchemaVersion; }
        }

        /// <summary>
        /// The copyright link for the content of this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Copyright"/>
        public PivotLink Copyright
        {
            get { return this.CachedCollectionData.Copyright; }
        }

        /// <summary>
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.ImageBase"/>
        public String ImageBase
        {
            get { return this.CachedCollectionData.ImageBase; }
        }

        /// <summary>
        /// The facet categories defined for this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.FacetCategories"/>
        public IReadablePivotList<String, PivotFacetCategory> FacetCategories
        {
            get { return this.CachedCollectionData.FacetCategories; }
        }

        /// <summary>
        /// An enumeration of all the items in this collection.
        /// </summary>
        /// <seealso cref="PivotCollection.Items"/>
        public IEnumerable<PivotItem> Items
        {
            get
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
        }

        /// <summary>
        /// Disposes of the resources used by this collection source.
        /// </summary>
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
                    m_cachedCollectionData = new PivotCollection();
                    this.LoadHeaderData();
                }
                return m_cachedCollectionData;
            }
        }

        private void LoadHeaderData()
        {
            using (FileStream fileStream = new FileStream(this.BasePath, FileMode.Open, FileAccess.Read))
            {
                using (XmlReader xmlReader = XmlReader.Create(fileStream))
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType != XmlNodeType.Element) continue;

                        if (xmlReader.LocalName == "Collection")
                        {
                            m_cachedCollectionData.Name = xmlReader.GetAttribute("Name");
                            m_cachedCollectionData.AdditionalSearchText = xmlReader.GetAttribute("AdditionalSearchText");
                            m_cachedCollectionData.SchemaVersion = xmlReader.GetAttribute("SchemaVersion");

                            String value = null;
                            if ((value = xmlReader.GetAttribute("Icon", PivotNamespace)) != null)
                            {
                                m_cachedCollectionData.Icon = new PivotImage(Path.Combine(m_baseDirectory, value));
                            }
                            if ((value = xmlReader.GetAttribute("BrandImage", PivotNamespace)) != null)
                            {
                                m_cachedCollectionData.BrandImage =
                                    new PivotImage(Path.Combine(m_baseDirectory, value));
                            }
                        }
                        else if (xmlReader.LocalName == "Items")
                        {
                            String imageBase = xmlReader.GetAttribute("ImgBase");
                            if (imageBase != null)
                            {
                                m_cachedCollectionData.ImageBase = Path.Combine(m_baseDirectory, imageBase);
                                xmlReader.Skip();
                            }
                        }
                        else if (xmlReader.LocalName == "Copyright")
                        {
                            m_cachedCollectionData.Copyright =
                                new PivotLink(xmlReader.GetAttribute("Name"), xmlReader.GetAttribute("Href"));
                        }
                        else if (xmlReader.LocalName == "FacetCategory")
                        {
                            PivotFacetCategory facetCategory = this.ParseFacetCategory(xmlReader.ReadSubtree());
                            m_cachedCollectionData.FacetCategories.Add(facetCategory);
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
                        if (m_cachedCollectionData.ImageBase == null)
                        {
                            imagePath = Path.Combine(m_baseDirectory, imagePath);
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
            String facetCategoryName = null;
            PivotFacetType facetType = null;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element) continue;

                if (xmlReader.LocalName == "Facet")
                {
                    facetCategoryName = xmlReader.GetAttribute("Name");
                    PivotFacetCategory facetCategory = m_cachedCollectionData.FacetCategories[facetCategoryName];
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

        private const String CollectionNamespace = "http://schemas.microsoft.com/collection/metadata/2009";

        private const String PivotNamespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009";

        private String m_basePath;

        private String m_baseDirectory;

        private PivotCollection m_cachedCollectionData;
    }
}
