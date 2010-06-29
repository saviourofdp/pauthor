using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Streaming.OleDb
{
    /// <summary>
    /// OleDbSchemaConstants defines all the table names, collum names and other strings expected in collections read
    /// from an OLE DB data source.
    /// </summary>
    public static class OleDbSchemaConstants
    {
        /// <summary>
        /// Table defines constants for all the standard table names used when reading from an OLE DB data source.
        /// </summary>
        public static class Table
        {
            /// <summary>
            /// The standard name of the "collection" table. See: <see cref="PivotCollection"/>
            /// </summary>
            public const String Collection = "collection";

            /// <summary>
            /// The standard name of the "facet categories" table. See: <see cref="PivotFacetCategory"/>
            /// </summary>
            public const String FacetCategories = "facet_categories";

            /// <summary>
            /// The standard name of the "items" table. See: <see cref="PivotItem"/>
            /// </summary>
            public const String Items = "items";
        }

        /// <summary>
        /// Collection defines constants for all the standard column names used in the collection table.
        /// </summary>
        public static class Collection
        {
            /// <summary>
            /// The standard name of the "name" column in the collection table. See: <see cref="PivotCollection.Name"/>
            /// </summary>
            public const String Name = "name";

            /// <summary>
            /// The standard name of the "icon" column in the collection table. See: <see cref="PivotCollection.Icon"/>
            /// </summary>
            public const String Icon = "icon";

            /// <summary>
            /// The standard name of the "brand image" column in the collection table. See:
            /// <see cref="PivotCollection.BrandImage"/>
            /// </summary>
            public const String BrandImage = "brand_image";

            /// <summary>
            /// The standard name of the "additional search text" column in the collection table. See:
            /// <see cref="PivotCollection.AdditionalSearchText"/>
            /// </summary>
            public const String AdditionalSearchText = "additional_search_text";

            /// <summary>
            /// The standard name of the column containing the title portion of the copyright link in the collection
            /// table. See: <see cref="PivotCollection.Copyright"/>
            /// </summary>
            public const String CopyrightTitle = "copyright_title";

            /// <summary>
            /// The standard name of the column containing the URL portion of the copyright link in the collection
            /// table. See: <see cref="PivotCollection.Copyright"/>
            /// </summary>
            public const String CopyrightUrl = "copyright_url";

            /// <summary>
            /// A list of all columns in the collection table.
            /// </summary>
            public static readonly String[] AllColumns = 
            {
                Name, Icon, BrandImage, AdditionalSearchText, CopyrightTitle, CopyrightUrl
            };
        }

        /// <summary>
        /// FacetCategory defines constants for all the standard column names used in the facet category table.
        /// </summary>
        public static class FacetCategory
        {
            /// <summary>
            /// The standard name of the "name" column in the facet categories table. See:
            /// <see cref="PivotFacetCategory.Name"/>
            /// </summary>
            public const String Name = "name";

            /// <summary>
            /// The standard name of the "type" column in the facet categories table. See:
            /// <see cref="PivotFacetCategory.Type"/>
            /// </summary>
            public const String Type = "type";

            /// <summary>
            /// The standard name of the "format" column in the facet categories table. See:
            /// <see cref="PivotFacetCategory.Format"/>
            /// </summary>
            public const String Format = "format";

            /// <summary>
            /// The standard name of the "is filter visible" column in the facet categories table. See:
            /// <see cref="PivotFacetCategory.IsFilterVisible"/>
            /// </summary>
            public const String IsFilterVisible = "is_filter_visible";

            /// <summary>
            /// The standard name of the "is metadata visible" column in the facet categories table. See:
            /// <see cref="PivotFacetCategory.IsMetaDataVisible"/>
            /// </summary>
            public const String IsMetadataVisible = "is_metadata_visible";

            /// <summary>
            /// The standard name of the "is word wheel visible" column in the facet categories table. See:
            /// <see cref="PivotFacetCategory.IsWordWheelVisible"/>
            /// </summary>
            public const String IsWordWheelVisible = "is_wordwheel_visible";

            /// <summary>
            /// The standard name of the "sort name" column in the facet categories table. See:
            /// <see cref="PivotFacetCategory.SortOrder"/>
            /// </summary>
            public const String SortName = "sort_name";

            /// <summary>
            /// The standard name of the "sort values" column in the facet categories table. See:
            /// <see cref="PivotFacetCategory.SortOrder"/>
            /// </summary>
            public const String SortValues = "sort_values";

            /// <summary>
            /// A list of all columns in the facet categories table.
            /// </summary>
            public static readonly String[] AllColumns = 
            {
                Name, Type, Format, IsFilterVisible, IsMetadataVisible, IsWordWheelVisible, SortName, SortValues
            };
        }

        /// <summary>
        /// Item defines constants for all the standard column names used in the item table.
        /// </summary>
        public static class Item
        {
            /// <summary>
            /// The standard name of the "name" column in the items table. See: <see cref="PivotItem.Name"/>
            /// </summary>
            public const String Name = "name";

            /// <summary>
            /// The standard name of the "image" column in the items table. See: <see cref="PivotItem.Image"/>
            /// </summary>
            public const String Image = "image";

            /// <summary>
            /// The standard name of the "description" column in the items table. See:
            /// <see cref="PivotItem.Description"/>
            /// </summary>
            public const String Description = "description";

            /// <summary>
            /// The standard name of the "href" column in the items table. See: <see cref="PivotItem.Href"/>
            /// </summary>
            public const String Href = "href";

            /// <summary>
            /// The standard name of the "related links" column in the items table. See:
            /// <see cref="PivotItem.RelatedLinks"/>
            /// </summary>
            public const String RelatedLinks = "related_links";

            /// <summary>
            /// A list of all columns in the facet categories table.
            /// </summary>
            public static readonly String[] AllColumns = 
            {
                Name, Image, Description, Href, RelatedLinks
            };
        }

        /// <summary>
        /// The delimiter used when writing a multi-valued facet into an OLE DB row.
        /// </summary>
        public const String FacetValueDelimiter = "\n";

        /// <summary>
        /// The delimiter used to separate the title and URL portions of a hyperlink.
        /// </summary>
        public const String LinkPartDelimiter = "||";
    }
}
