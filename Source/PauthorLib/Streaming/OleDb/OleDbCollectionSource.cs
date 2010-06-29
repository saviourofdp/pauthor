//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Streaming.OleDb
{
    /// <summary>
    /// OleDbCollectionSource is a Pivot collection source should which draws data from and OLE DB connection.
    /// </summary>
    /// <remarks>
    /// Since OLE DB is a generic protocol for communicating with tabular data sources, this class allows you to
    /// interoperate with a large number of different data providers, including: SQL Server, Microsoft Access, Microsoft
    /// Excel, CSV files, and any other format for which there is an OLE DB driver.
    /// 
    /// <para>This class provides a number of properties which control the class's interaction with the underlying OLE
    /// DB data source. By configuring these properties, you may specifiy the driver, source, and queries necessary to
    /// access any OLE DB data source. This namespace includes a few subclasses which provide default values for
    /// accessing Excel and CSV data.</para>
    /// 
    /// <para>This class (and its subclasses) access the underlying data in two parts. First, the collection and facet
    /// category information if fetched and cached in memory. Then, each time the <see cref="Items"/> property is
    /// accessed, a new connection will be opened, and items will be fetched from the underlying data source one-by-one
    /// in accordance with the <see cref="IPivotCollectionSource"/> interface.</para>
    /// 
    /// <para>The underlying data is expected to be presented with the specific schema described by the
    /// <see cref="OleDbSchemaConstants"/> class. If the underlying data source does not match this schema, the various
    /// queries should be written to adapt it to the one expected by this class.</para>
    /// </remarks>
    public class OleDbCollectionSource : IPivotCollectionSource
    {
        /// <summary>
        /// Creates a new OLE DB collection source with a given connection string.
        /// </summary>
        /// <param name="connectionString">a string describing how to connect to the OLE DB data source</param>
        public OleDbCollectionSource(String connectionString)
        {
            this.ConnectionString = connectionString;
            this.ImageBaseDirectory = ".";
        }

        /// <summary>
        /// The string which defined which OLE DB driver to use, and instructions for how that driver should connect to
        /// the desired data source.
        /// </summary>
        /// <remarks>
        /// The initial value of this property is set by the constructor.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        public String ConnectionString
        {
            get { return m_connectionString; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("ConnectionString");
                m_connectionString = value;
            }
        }

        /// <summary>
        /// A SQL "select" statement which will retrieve the top-level collection data.
        /// </summary>
        /// <remarks>
        /// The query must specify a result set which matches the schema defined by <see cref="OleDbSchemaConstants"/>.
        /// If this property is null, then default values will be used for the top-level data. Setting this property to
        /// an empty string has the same effect as setting it to null. By default, this property is null.
        /// </remarks>
        public String CollectionDataQuery
        {
            get { return m_collectionDataQuery; }

            set { m_collectionDataQuery = (value == "") ? null : value; } 
        }

        /// <summary>
        /// An SQL "select" statement which will retrieve the facet category definitions.
        /// </summary>
        /// <remarks>
        /// The query must specify a result set which matches the schema defined by <see cref="OleDbSchemaConstants"/>.
        /// If this property is null, then facet categories will be inferred from the columns available from the items
        /// query. Setting this property to an empty string has the same effect as setting it to null. By default, this
        /// property is null.
        /// </remarks>
        public String FacetCategoriesDataQuery
        {
            get { return m_facetCategoriesDataQuery; }

            set { m_facetCategoriesDataQuery = (value == "") ? null : value; }
        }

        /// <summary>
        /// An SQL "select" statement which will retrieve the item data.
        /// </summary>
        /// <remarks>
        /// The query must specify a result set which matches the schema defined by <see cref="OleDbSchemaConstants"/>.
        /// If this property is null, then the first table found by examining the OLE DB schema will be assumed to
        /// contain items. By default, this property is null.
        /// 
        /// <para><b>NOTE:</b> For most data formats, assuming the first table contains item data is probably incorrect,
        /// so users of this class should nearly always provide a value for this property.</para>
        /// </remarks>
        public String ItemsDataQuery
        {
            get { return m_itemsDataQuery; }

            set { m_itemsDataQuery = (value == "") ? null : value; }
        }

        /// <summary>
        /// The base directory for images used by this collection source.
        /// </summary>
        /// <remarks>
        /// If any images in the source data use a have a relative source path, it will be resolved based upon this
        /// directory. By default, this property is set to the current working directory.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if given a null or empty value</exception>
        /// <exception cref="ArgumentException">if the given directory does not exist</exception>
        public String ImageBaseDirectory
        {
            get { return m_imageBaseDirectory; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("ImageBaseDirectory");
                if (Directory.Exists(value) == false)
                {
                    throw new ArgumentException("ImageBaseDirectory does not exist: " + value);
                }

                m_imageBaseDirectory = value;
            }
        }

        /// <summary>
        /// The user-facing title of this collection. See: <see cref="PivotCollection.Name"/>
        /// </summary>
        public String Name
        {
            get { return this.CachedCollectionData.Name; }
        }

        /// <summary>
        /// The favicon associated with this collection. See: <see cref="PivotCollection.Icon"/>
        /// </summary>
        public PivotImage Icon
        {
            get { return this.CachedCollectionData.Icon; }
        }

        /// <summary>
        /// The branding image associated with this collection. See: <see cref="PivotCollection.BrandImage"/>
        /// </summary>
        public PivotImage BrandImage
        {
            get { return this.CachedCollectionData.BrandImage; }
        }

        /// <summary>
        /// The additional text to be appended to an item's name when requesting search results from Bing. See:
        /// <see cref="PivotCollection.AdditionalSearchText"/>
        /// </summary>
        public String AdditionalSearchText
        {
            get { return this.CachedCollectionData.AdditionalSearchText; }
        }

        /// <summary>
        /// The version number of the Pivot schema used to represent this collection. See:
        /// <see cref="PivotCollection.SchemaVersion"/>
        /// </summary>
        public String SchemaVersion
        {
            get { return this.CachedCollectionData.SchemaVersion; }
        }

        /// <summary>
        /// The copyright link for the content of this collection. See: <see cref="PivotCollection.Copyright"/>
        /// </summary>
        public PivotLink Copyright
        {
            get { return this.CachedCollectionData.Copyright; }
        }

        /// <summary>
        /// The DeepZoom Collection (DZC file) which provides the imagery for this collection. See:
        /// <see cref="PivotCollection.ImageBase"/>
        /// </summary>
        public String ImageBase
        {
            get { return this.CachedCollectionData.ImageBase; }
        }

        /// <summary>
        /// The facet categories defined for this collection. See: <see cref="PivotCollection.FacetCategories"/>
        /// </summary>
        public IReadablePivotList<String, PivotFacetCategory> FacetCategories
        {
            get { return this.CachedCollectionData.FacetCategories; }
        }

        /// <summary>
        /// An enumeration of all the items in this collection. See: <see cref="PivotCollection.Items"/>
        /// </summary>
        public IEnumerable<PivotItem> Items
        {
            get
            {
                if (this.CachedCollectionData != null) { } // Ensure facet categories are loaded.

                using (OleDbConnection connection = new OleDbConnection(this.ConnectionString))
                {
                    connection.Open();

                    String itemsDataQuery = this.ItemsDataQuery;
                    if (itemsDataQuery == null) yield break;

                    OleDbCommand command = new OleDbCommand(itemsDataQuery, connection);
                    OleDbDataReader dataReader = command.ExecuteReader();
                    int itemCount = 0;
                    while (dataReader.Read())
                    {
                        PivotItem item = this.ReadItem(dataReader, itemCount);
                        yield return item;
                        itemCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Disposes of any resources used by this collection source.
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
                    using (OleDbConnection connection = new OleDbConnection(this.ConnectionString))
                    {
                        connection.Open();
                        this.LoadHeaderData(connection);
                    }
                }
                return m_cachedCollectionData;
            }
        }

        private void LoadHeaderData(OleDbConnection connection)
        {
            m_cachedCollectionData = new PivotCollection();
            m_facetCategoryMap = new Dictionary<String, PivotFacetCategory>();

            this.LoadCollectionData(connection);
            this.LoadFacetCategoriesData(connection);

            if (m_cachedCollectionData.FacetCategories.Count() == 0)
            {
                this.DeriveFacetCategoriesFromItems(connection);
            }
        }

        private void LoadCollectionData(OleDbConnection connection)
        {
            String collectionCommandString = this.CollectionDataQuery;
            if (collectionCommandString == null) return;

            OleDbCommand command = new OleDbCommand(collectionCommandString, connection);
            OleDbDataReader dataReader = command.ExecuteReader();

            if (dataReader.Read())
            {
                for (int column = 0; column < dataReader.FieldCount; column++)
                {
                    if (dataReader.IsDBNull(column)) continue;

                    String columnName = dataReader.GetName(column).ToLowerInvariant();
                    String value = dataReader.GetValue(column).ToString();

                    if (columnName == OleDbSchemaConstants.Collection.Name)
                    {
                        m_cachedCollectionData.Name = value;
                    }
                    else if (columnName == OleDbSchemaConstants.Collection.Icon)
                    {
                        String iconPath = Path.Combine(this.ImageBaseDirectory, value);
                        m_cachedCollectionData.Icon = new PivotImage(iconPath);
                    }
                    else if (columnName == OleDbSchemaConstants.Collection.BrandImage)
                    {
                        String brandImagePath = Path.Combine(this.ImageBaseDirectory, value);
                        m_cachedCollectionData.BrandImage = new PivotImage(brandImagePath);
                    }
                    else if (columnName == OleDbSchemaConstants.Collection.AdditionalSearchText)
                    {
                        m_cachedCollectionData.AdditionalSearchText = value;
                    }
                    else if (columnName == OleDbSchemaConstants.Collection.CopyrightTitle)
                    {
                        if (m_cachedCollectionData.Copyright == null)
                        {
                            m_cachedCollectionData.Copyright = new PivotLink(value, "about:none");
                        }
                        m_cachedCollectionData.Copyright.Title = value;
                    }
                    else if (columnName == OleDbSchemaConstants.Collection.CopyrightUrl)
                    {
                        if (m_cachedCollectionData.Copyright == null)
                        {
                            m_cachedCollectionData.Copyright = new PivotLink("Copyright", value);
                        }
                        m_cachedCollectionData.Copyright.Url = value;
                    }
                }
            }
        }

        private void LoadFacetCategoriesData(OleDbConnection connection)
        {
            String facetCategoriesDataQuery = this.FacetCategoriesDataQuery;
            if (facetCategoriesDataQuery == null) return;

            OleDbCommand command = new OleDbCommand(facetCategoriesDataQuery, connection);
            OleDbDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                PivotFacetCategory facetCategory = this.CreateFacetCategory(dataReader);

                for (int column = 0; column < dataReader.FieldCount; column++)
                {
                    if (dataReader.IsDBNull(column)) continue;

                    String columnName = dataReader.GetName(column).ToLowerInvariant();
                    String value = dataReader.GetValue(column).ToString();

                    if (columnName == OleDbSchemaConstants.FacetCategory.Format)
                    {
                        facetCategory.Format = value;
                    }
                    else if (columnName == OleDbSchemaConstants.FacetCategory.IsFilterVisible)
                    {
                        facetCategory.IsFilterVisible = Boolean.Parse(value.ToLowerInvariant());
                    }
                    else if (columnName == OleDbSchemaConstants.FacetCategory.IsMetadataVisible)
                    {
                        facetCategory.IsMetaDataVisible= Boolean.Parse(value.ToLowerInvariant());
                    }
                    else if (columnName == OleDbSchemaConstants.FacetCategory.IsWordWheelVisible)
                    {
                        facetCategory.IsWordWheelVisible = Boolean.Parse(value.ToLowerInvariant());
                    }
                    else if (columnName == OleDbSchemaConstants.FacetCategory.SortName)
                    {
                        PivotFacetSortOrder sortOrder = new PivotFacetSortOrder(value);
                        if (facetCategory.SortOrder != null)
                        {
                            sortOrder.AddAllValues(facetCategory.SortOrder.Values);
                        }
                        facetCategory.SortOrder = sortOrder;
                    }
                    else if (columnName == OleDbSchemaConstants.FacetCategory.SortValues)
                    {
                        if (facetCategory.SortOrder == null)
                        {
                            facetCategory.SortOrder = new PivotFacetSortOrder(facetCategory.Name);
                        }
                        facetCategory.SortOrder.AddAllValues(this.SplitJoinedStrings(value));
                    }
                }

                m_cachedCollectionData.FacetCategories.Add(facetCategory);
                m_facetCategoryMap.Add(facetCategory.Name.ToLowerInvariant(), facetCategory);
            }
        }

        private PivotFacetCategory CreateFacetCategory(OleDbDataReader dataReader)
        {
            String name = null;
            PivotFacetType type = null;

            for (int column = 0; column < dataReader.FieldCount; column++)
            {
                if (dataReader.IsDBNull(column)) continue;

                String columnName = dataReader.GetName(column).ToLowerInvariant();
                String value = dataReader.GetValue(column).ToString();

                if (columnName == OleDbSchemaConstants.FacetCategory.Name)
                {
                    name = value;
                }
                else if (columnName == OleDbSchemaConstants.FacetCategory.Type)
                {
                    type = PivotFacetType.Parse(value);
                }
            }
            if (name == null) throw new InvalidDataException("Facet Categories data set is missing a Name column");
            if (type == null) throw new InvalidDataException("Facet Categories data set is missing a Type column");
            return new PivotFacetCategory(name, type);
        }

        private void DeriveFacetCategoriesFromItems(OleDbConnection connection)
        {
            String itemsCommandString = this.ItemsDataQuery;
            if (itemsCommandString == null) return;

            OleDbCommand command = new OleDbCommand(itemsCommandString, connection);
            OleDbDataReader dataReader = command.ExecuteReader();
            for (int column = 0; column < dataReader.FieldCount; column++)
            {
                String name = dataReader.GetName(column).ToLowerInvariant();
                if (OleDbSchemaConstants.Item.AllColumns.Contains(name)) continue;

                PivotFacetCategory facetCategory = new PivotFacetCategory(name, PivotFacetType.String);
                m_cachedCollectionData.FacetCategories.Add(facetCategory);
                m_facetCategoryMap.Add(facetCategory.Name, facetCategory);
            }
        }

        private PivotItem ReadItem(OleDbDataReader dataReader, int rowId)
        {
            PivotItem item = new PivotItem(rowId.ToString(), this);

            for (int column = 0; column < dataReader.FieldCount; column++)
            {
                if (dataReader.IsDBNull(column)) continue;

                String columnName = dataReader.GetName(column).ToLowerInvariant();
                String value = dataReader.GetValue(column).ToString();
                if (String.IsNullOrEmpty(value)) continue;

                if (columnName == OleDbSchemaConstants.Item.Name)
                {
                    item.Name = value;
                }
                else if (columnName == OleDbSchemaConstants.Item.Image)
                {
                    item.Image = new PivotImage(Path.Combine(this.ImageBaseDirectory, value));
                }
                else if (columnName == OleDbSchemaConstants.Item.Description)
                {
                    item.Description = value;
                }
                else if (columnName == OleDbSchemaConstants.Item.Href)
                {
                    item.Href = value;
                }
                else if (columnName == OleDbSchemaConstants.Item.RelatedLinks)
                {
                    StringReader valueReader = new StringReader(value);
                    String singleValue = null;
                    while ((singleValue = valueReader.ReadLine()) != null)
                    {
                        String[] parts = singleValue.Split(
                            new String[] { OleDbSchemaConstants.LinkPartDelimiter }, StringSplitOptions.None);
                        if (parts.Length > 0)
                        {
                            String name = null, url = null;
                            if (parts.Length == 1)
                            {
                                url = parts[0];
                            }
                            else if (parts.Length >= 2)
                            {
                                name = parts[0];
                                url = parts[1];
                            }
                            item.AddRelatedLink(new PivotLink(name, url));
                        }
                    }
                }
                else
                {
                    PivotFacetCategory facetCategory = null;
                    foreach (PivotFacetCategory currentFacetCategory in m_facetCategoryMap.Values)
                    {
                        if (columnName == currentFacetCategory.Name.Replace('.', '#').ToLowerInvariant())
                        {
                            facetCategory = currentFacetCategory;
                            break;
                        }
                    }

                    if (facetCategory != null)
                    {
                        item.AddFacetValues(facetCategory.Name, this.SplitJoinedStrings(value).ToArray());
                    }
                }
            }

            return item;
        }

        private IEnumerable<Object> SplitJoinedValues(String joinedValue, PivotFacetType facetType)
        {
            foreach (String value in this.SplitJoinedStrings(joinedValue))
            {
                yield return facetType.ParseValue(value);
            }
        }

        private IEnumerable<String> SplitJoinedStrings(String joinedValue)
        {
            StringReader valueReader = new StringReader(joinedValue);
            String singleValue = null;
            while ((singleValue = valueReader.ReadLine()) != null)
            {
                if (String.IsNullOrEmpty(singleValue)) continue;
                yield return singleValue;
            }
        }

        private String m_connectionString;

        private String m_collectionDataQuery;

        private String m_facetCategoriesDataQuery;

        private String m_itemsDataQuery;

        private String m_imageBaseDirectory;

        private PivotCollection m_cachedCollectionData;

        private Dictionary<String, PivotFacetCategory> m_facetCategoryMap;
    }
}
