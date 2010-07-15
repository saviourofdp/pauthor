//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.IO;
using System.Linq;
using System.Xml;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Streaming
{
    /// <summary>
    /// LocalCxmlCollectionTarget writes a given collection source to a CXML file on the local file system.
    /// </summary>
    /// <remarks>
    /// This class only writes the CXML file using the image references contained in the items given. If you would like
    /// to transform the collection (for example, by converting the images to DeepZoom images), look at the classes in
    /// the <see cref="Microsoft.LiveLabs.Pauthor.Streaming.Filters"/> namespace.
    /// </remarks>
    public class LocalCxmlCollectionTarget : ILocalCollectionTarget
    {
        private static PauthorLog Log = PauthorLog.Global;

        /// <summary>
        /// Creates a new CXML collection target given the path to where the new CXML file should be written.
        /// </summary>
        /// <remarks>
        /// If necessary, the directory(s) containing the given file will be created.
        /// </remarks>
        /// <param name="cxmlPath">the path to where the new CXML file should be written</param>
        /// <exception cref="ArgumentException">if a null or empty value is given</exception>
        public LocalCxmlCollectionTarget(String cxmlPath)
        {
            this.BasePath = cxmlPath;
            this.Indented = true;
        }

        /// <summary>
        /// The path where this collection target will write a new CXML file.
        /// </summary>
        /// <remarks>
        /// If necessary, the parent directory(s) will be created to contain the new file.
        /// </remarks>
        /// <exception cref="ArgumentException">if a null or empty value is given</exception>
        public String BasePath
        {
            get { return m_basePath; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BasePath cannot be null or empty");
                if (Directory.Exists(Directory.GetParent(value).FullName) == false)
                {
                    Directory.CreateDirectory(Directory.GetParent(value).FullName);
                }
                m_basePath = Path.ChangeExtension(value, "cxml");
            }
        }

        /// <summary>
        /// The XML stylesheet which should be specified in the output CXML.
        /// </summary>
        /// <remarks>
        /// This file may be used to format the CXML file in browsers other than Pivot. By default, this property is
        /// null.
        /// </remarks>
        public String Stylesheet
        {
            get { return m_stylesheet; }

            set
            {
                m_stylesheet = ((value != null) && (value.Length == 0)) ? null : value;
            }
        }

        /// <summary>
        /// Whether the output CXML should be indented.
        /// </summary>
        /// <remarks>
        /// If this is set to true, then each level in the CXML file will be written on a new line and intended by four
        /// spaces. Otherwise, the entire fill will be written without any extra newlines or whitespace. By default,
        /// this property is set to true.
        /// </remarks>
        public bool Indented
        {
            get { return m_indented; }

            set { m_indented = value; }
        }

        /// <summary>
        /// Writes the given collection to disk as a CXML file.
        /// </summary>
        /// <param name="source">the collection source to write</param>
        public void Write(IPivotCollectionSource source)
        {
            Log.Message("Writing Pivot collection to {0}", this.BasePath);
            using (FileStream fileStream = new FileStream(this.BasePath, FileMode.Create))
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = this.Indented;
                xmlWriterSettings.IndentChars = IndentChars;

                using (XmlWriter xmlWriter = XmlWriter.Create(fileStream, xmlWriterSettings))
                {
                    Log.Message("Writing header information");
                    xmlWriter.WriteProcessingInstruction("xml",
                        "version=\"1.0\" encoding=\"" + xmlWriter.Settings.Encoding.BodyName + "\"");
                    if (this.Stylesheet != null)
                    {
                        xmlWriter.WriteProcessingInstruction("xml-stylesheet",
                            "type=\"text/xsl\" href=\"" + this.Stylesheet + "\"");
                    }

                    xmlWriter.WriteStartElement("Collection", CollectionNamespace);
                    xmlWriter.WriteAttributeString("xmlns", "xsi", null, XmlSchemaInstanceNamespace);
                    xmlWriter.WriteAttributeString("xmlns", "xsd", null, XmlSchemaNamespace);
                    xmlWriter.WriteAttributeString("xmlns", "p", null, PivotNamespace);
                    this.WriteCollectionAttributes(xmlWriter, source);

                    if (source.FacetCategories.Count() > 0)
                    {
                        xmlWriter.WriteStartElement("FacetCategories");
                        foreach (PivotFacetCategory facetCategory in source.FacetCategories)
                        {
                            this.WriteFacetCategory(xmlWriter, facetCategory);
                        }
                        xmlWriter.WriteEndElement(); // FacetCategories
                    }

                    Log.Message("Writing items");
                    xmlWriter.WriteStartElement("Items");
                    if (source.ImageBase != null)
                    {
                        xmlWriter.WriteAttributeString("ImgBase", source.ImageBase);
                    }

                    int count = 0;
                    foreach (PivotItem item in source.Items)
                    {
                        if ((count > 0) && ((count % 10) == 0)) Log.Progress("Finished {0} items", count);
                        this.WriteItem(xmlWriter, item);
                        count++;
                    }
                    xmlWriter.WriteEndElement(); // Items

                    Log.Message("Writing footer");
                    if (source.Copyright != null)
                    {
                        xmlWriter.WriteStartElement("Extension");
                        xmlWriter.WriteStartElement("Copyright", PivotNamespace);
                        xmlWriter.WriteAttributeString("Name", source.Copyright.Title ?? "Copyright");
                        xmlWriter.WriteAttributeString("Href", source.Copyright.Url ?? "about:none");
                        xmlWriter.WriteEndElement(); // Extension
                        xmlWriter.WriteEndElement(); // Copyright
                    }
                    xmlWriter.WriteEndElement(); // Collection
                }
            }
        }

        /// <summary>
        /// Disposes of any resources used by this collection target.
        /// </summary>
        public void Dispose()
        {
            // Do nothing.
        }

        private void WriteCollectionAttributes(XmlWriter xmlWriter, IPivotCollectionSource source)
        {
            if (source.Name != null) 
            {
                xmlWriter.WriteAttributeString("Name", source.Name);
            }
            if (source.Icon != null) 
            {
                xmlWriter.WriteAttributeString("Icon", PivotNamespace, source.Icon.SourcePath);
            }
            if (source.BrandImage != null) 
            {
                xmlWriter.WriteAttributeString("BrandImage", PivotNamespace, source.BrandImage.SourcePath);
            }
            if (source.AdditionalSearchText != null)
            {
                xmlWriter.WriteAttributeString("AdditionalSearchText", PivotNamespace, source.AdditionalSearchText);
            }
            if (source.SchemaVersion != null)
            {
                xmlWriter.WriteAttributeString("SchemaVersion", source.SchemaVersion);
            }
        }

        private void WriteFacetCategory(XmlWriter xmlWriter, PivotFacetCategory facetCategory)
        {
            xmlWriter.WriteStartElement("FacetCategory");
            xmlWriter.WriteAttributeString("Name", facetCategory.Name);
            xmlWriter.WriteAttributeString("Type", facetCategory.Type.ToString());
            if (facetCategory.Format != null)
            {
                xmlWriter.WriteAttributeString("Format", facetCategory.Format);
            }
            xmlWriter.WriteAttributeString("IsFilterVisible", PivotNamespace,
                facetCategory.IsFilterVisible.ToString().ToLowerInvariant());
            xmlWriter.WriteAttributeString("IsMetaDataVisible", PivotNamespace,
                facetCategory.IsMetaDataVisible.ToString().ToLowerInvariant());
            xmlWriter.WriteAttributeString("IsWordWheelVisible", PivotNamespace,
                facetCategory.IsWordWheelVisible.ToString().ToLowerInvariant());
            if (facetCategory.SortOrder != null)
            {
                xmlWriter.WriteStartElement("Extension");
                xmlWriter.WriteStartElement("SortOrder", PivotNamespace);
                xmlWriter.WriteAttributeString("Name", facetCategory.SortOrder.Name);
                foreach (String sortValue in facetCategory.SortOrder.Values)
                {
                    xmlWriter.WriteStartElement("SortValue", PivotNamespace);
                    xmlWriter.WriteAttributeString("Value", sortValue);
                    xmlWriter.WriteEndElement(); // SortValue
                }
                xmlWriter.WriteEndElement(); // SortOrder
                xmlWriter.WriteEndElement(); // Extension
            }
            xmlWriter.WriteEndElement(); // FacetCategory
        }

        private void WriteItem(XmlWriter xmlWriter, PivotItem item)
        {
            xmlWriter.WriteStartElement("Item");
            xmlWriter.WriteAttributeString("Id", item.Id);
            if (item.Name != null) xmlWriter.WriteAttributeString("Name", item.Name);
            if (item.Image != null)
            {
                xmlWriter.WriteAttributeString("Img", this.NormalizeImagePath(item.Image.SourcePath));
            }
            else
            {
                Log.Warning("Item {0} ({1}) does not have an image", item.Id, item.Name);
            }
            if (item.Href != null) xmlWriter.WriteAttributeString("Href", item.Href);

            if (item.Description != null)
            {
                xmlWriter.WriteElementString("Description", item.Description);
            }
            if (item.FacetCategories.Count() > 0)
            {
                xmlWriter.WriteStartElement("Facets");
                foreach (String facetCategoryName in item.FacetCategories)
                {
                    xmlWriter.WriteStartElement("Facet");
                    this.WriteFacet(xmlWriter, facetCategoryName, item);
                    xmlWriter.WriteEndElement(); // Facet
                }
                xmlWriter.WriteEndElement(); // Facets
            }

            this.WriteRelatedLinks(xmlWriter, item);

            xmlWriter.WriteEndElement(); // Item
        }

        private String NormalizeImagePath(String imagePath)
        {
            String baseDirectoryPath = Directory.GetParent(this.BasePath).FullName;
            if (imagePath.StartsWith(baseDirectoryPath) == false) return imagePath;
            imagePath = imagePath.Substring(baseDirectoryPath.Length + 1);
            return imagePath;
        }

        private void WriteRelatedLinks(XmlWriter xmlWriter, PivotItem item)
        {
            if (item.HasRelatedLinks == false) return;
            xmlWriter.WriteStartElement("Extension");
            xmlWriter.WriteStartElement("Related", PivotNamespace);
            foreach (PivotLink link in item.RelatedLinks)
            {
                if (link.Url == null) continue;
                xmlWriter.WriteStartElement("Link", PivotNamespace);
                xmlWriter.WriteAttributeString("Name", link.Title ?? "Related Link");
                xmlWriter.WriteAttributeString("Href", link.Url);
                xmlWriter.WriteEndElement(); // Link
            }
            xmlWriter.WriteEndElement(); // Related
            xmlWriter.WriteEndElement(); // Extension
        }

        private void WriteFacet(XmlWriter xmlWriter, String facetCategoryName, PivotItem item)
        {
            PivotFacetCategory facetCategory = item.CollectionDefinition.FacetCategories[facetCategoryName];
            xmlWriter.WriteAttributeString("Name", facetCategoryName);
            foreach (IComparable value in item.GetAllFacetValues(facetCategoryName))
            {
                if (facetCategory.Type == PivotFacetType.Link)
                {
                    PivotLink linkValue = (PivotLink)value;
                    xmlWriter.WriteStartElement(facetCategory.Type.ToString());
                    if (linkValue.Title != null) xmlWriter.WriteAttributeString("Name", linkValue.Title);
                    if (linkValue.Url != null) xmlWriter.WriteAttributeString("Href", linkValue.Url);
                    xmlWriter.WriteEndElement(); // FacetType
                }
                else if (facetCategory.Type == PivotFacetType.DateTime)
                {
                    xmlWriter.WriteStartElement(facetCategory.Type.ToString());
                    xmlWriter.WriteAttributeString("Value", ((DateTime)value).ToString("s"));
                    xmlWriter.WriteEndElement(); // FacetType
                }
                else if (facetCategory.Type == PivotFacetType.Number)
                {
                    xmlWriter.WriteStartElement(facetCategory.Type.ToString());
                    xmlWriter.WriteAttributeString("Value", value.ToString());
                    xmlWriter.WriteEndElement(); // FacetType
                }
                else
                {
                    xmlWriter.WriteStartElement(facetCategory.Type.ToString());
                    xmlWriter.WriteAttributeString("Value", item.ConvertFacetValueToString(facetCategoryName, value));
                    xmlWriter.WriteEndElement(); // FacetType
                }
            }
        }

        private const String XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        private const String XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

        private const String CollectionNamespace = "http://schemas.microsoft.com/collection/metadata/2009";

        private const String PivotNamespace = "http://schemas.microsoft.com/livelabs/pivot/collection/2009";

        private const String IndentChars = "    ";

        private String m_basePath;

        private String m_stylesheet;

        private bool m_indented;
    }
}
