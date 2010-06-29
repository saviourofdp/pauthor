//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Streaming.OleDb
{
    /// <summary>
    /// CsvCollectionSource is a subclass of <see cref="OleDbCollectionSource"/> which specifically reads the CSV
    /// format.
    /// </summary>
    /// <remarks>
    /// As with the parent class, this class requires the use of the specific schema described by the <see
    /// cref="OleDbSchemaConstants"/> class. However, CSV doesn't naturally support a notion of multiple tables. OLE DB
    /// circumvents this problem by expecting a directory to contain a single CSV file for each separate table. This
    /// class provides a set of properties to allow you to independently specify the name of each CSV file for the three
    /// tables in the collection schema.
    /// </remarks>
    public class CsvCollectionSource : OleDbCollectionSource, ILocalCollectionSource
    {
        /// <summary>
        /// Creates a new CSV collection source and sets its <see cref="BasePath"/>.
        /// </summary>
        /// <param name="basePath">the path to the CSV file containing item data for the new collection source</param>
        public CsvCollectionSource(String basePath)
            : base(ConnectionStringTemplate)
        {
            this.BasePath = basePath;
        }

        /// <summary>
        /// The path to the CSV file containing item data for this collection.
        /// </summary>
        /// <remarks>
        /// When changed, this will automatically set a number of other properties:
        /// <list type="table">
        /// <item><term>BaseDirectory</term><description>to the parent directory of the given path</description></item>
        /// <item><term>ItemsFileName</term><description>to the file name portion of the given path</description></item>
        /// <item><term>CollectionFileName</term><description>to "<i>name</i>_collection.csv"</description></item>
        /// <item><term>FacetCategoriesFileName</term>
        /// <description>to "<i>name</i>_facetcategories.csv"</description></item>
        /// </list>
        /// <para>If so desired, you may later change any of those properties to a new value. The initial value for this
        /// property is set in the constructor.</para>
        /// </remarks>
        public String BasePath
        {
            get { return Path.Combine(this.BaseDirectory, this.ItemsFileName); }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BasePath");
                if (File.Exists(value) == false) throw new ArgumentException("BasePath does not exist: " + value);
                if (Path.GetFileNameWithoutExtension(value).Contains('.'))
                {
                    throw new ArgumentException(
                        "BasePath cannot contain '.' characters within the file name: " + value);
                }

                this.BaseDirectory = Directory.GetParent(value).FullName;
                this.ImageBaseDirectory = this.BaseDirectory;
                this.ConnectionString = String.Format(ConnectionStringTemplate, this.BaseDirectory);
                this.ItemsFileName = Path.GetFileName(value);

                String baseFileName = Path.GetFileNameWithoutExtension(value);
                this.CollectionFileName = baseFileName + "_collection.csv";
                this.FacetCategoriesFileName = baseFileName + "_facetcategories.csv";
            }
        }

        /// <summary>
        /// The path to the directory containing all the CSV files which make up this collection source.
        /// </summary>
        /// <remarks>
        /// The initial value for this property is set by the constructor.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        /// <exception cref="ArgumentException">if the given directory does not exist</exception>
        public String BaseDirectory
        {
            get { return m_baseDirectory; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BaseDirectory");
                if (Directory.Exists(value) == false)
                {
                    throw new ArgumentNullException("BaseDirectory does not exist: " + value);
                }
                m_baseDirectory = value;
            }
        }

        /// <summary>
        /// The name of the file within the <see cref="BaseDirectory"/> containing the top-level collection data.
        /// </summary>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        /// <exception cref="ArgumentException">if the given file does not exist</exception>
        public String CollectionFileName
        {
            get { return m_collectionFileName; }

            set
            {
                m_collectionFileName = value;
                this.CollectionDataQuery = (value == null) ? null : String.Format(CommandTemplate, value);
            }
        }

        /// <summary>
        /// The name of the file within the <see cref="BaseDirectory"/> containing the facet category data.
        /// </summary>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        /// <exception cref="ArgumentException">if the given file does not exist</exception>
        public String FacetCategoriesFileName
        {
            get { return m_facetCategoriesFileName; }

            set
            {
                m_facetCategoriesFileName = value;
                this.FacetCategoriesDataQuery = (value == null) ? null : String.Format(CommandTemplate, value);
            }
        }

        /// <summary>
        /// The name of the file within the <see cref="BaseDirectory"/> containing the items data.
        /// </summary>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        /// <exception cref="ArgumentException">if the given file does not exist</exception>
        public String ItemsFileName
        {
            get { return m_itemsFileName; }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("ItemFileName cannot be null or empty");
                }

                String itemsFilePath = Path.Combine(this.BaseDirectory, value);
                if (File.Exists(itemsFilePath) == false)
                {
                    throw new ArgumentException("ItemFileName does not exist: " + itemsFilePath);
                }
                m_itemsFileName = value;
                this.ItemsDataQuery = String.Format(CommandTemplate, m_itemsFileName);
            }
        }

        /// <summary>
        /// The user-facing title of this collection (See: <see cref="PivotCollection.Name"/>).
        /// </summary>
        /// <remarks>
        /// If the collection data query is null, then this will return the file name (without extension) of the items
        /// file.
        /// </remarks>
        public new String Name
        {
            get
            {
                if (this.CollectionDataQuery != null) return base.Name;
                return Path.GetFileNameWithoutExtension(m_itemsFileName);
            }
        }

        private const String ConnectionStringTemplate = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; " +
            "Extended Properties=\"Text;HDR=YES;FMT=Delimited\"";

        private const String CommandTemplate = "SELECT * FROM [{0}]";

        private String m_baseDirectory;

        private String m_collectionFileName;

        private String m_facetCategoriesFileName;

        private String m_itemsFileName;
    }
}
