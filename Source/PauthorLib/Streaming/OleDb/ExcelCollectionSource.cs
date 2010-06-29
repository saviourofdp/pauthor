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
using Microsoft.LiveLabs.Pauthor.Imaging;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Streaming.OleDb
{
    /// <summary>
    /// ExcelCollectionSource is a subclass of <see cref="OleDbCollectionSource"/> which specifically reads the
    /// Microsoft Excel 2007 format.
    /// </summary>
    /// <remarks>
    /// As with the parent class, this class requires the use of the specific schema described by the <see
    /// cref="OleDbSchemaConstants"/> class. However, since Excel doesn't actually contain SQL tables, individual sheets
    /// within the Excel file are used as tables instead.
    /// </remarks>
    public class ExcelCollectionSource : OleDbCollectionSource, ILocalCollectionSource
    {
        /// <summary>
        /// Creates a new Excel collection source and sets its <see cref="BasePath"/>.
        /// </summary>
        /// <param name="basePath">the path to the Excel file containing the collection's data</param>
        public ExcelCollectionSource(String basePath)
            : base(String.Format(ConnectionStringTemplate, basePath))
        {
            this.BasePath = basePath;
        }

        /// <summary>
        /// The path to the Excel file containing the collection data.
        /// </summary>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        /// <exception cref="ArgumentException">
        /// if the given file does not exist, or if the file has a '.' in its name (not including the extension). This
        /// restriction is due to the facet that OLE DB treats Excel files as a database, and '.' is not a valid
        /// character in database names.
        /// </exception>
        public String BasePath
        {
            get { return m_basePath; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BasePath cannot be null or empty");
                if (File.Exists(value) == false) throw new ArgumentException("BasePath does not exist: " + value);
                if (Path.GetFileNameWithoutExtension(value).Contains('.'))
                {
                    throw new ArgumentException(
                        "BasePath cannot contain '.' characters within the file name: " + value);
                }

                m_basePath = value;
                m_baseDirectory = Directory.GetParent(this.BasePath).FullName;
                this.ImageBaseDirectory = m_baseDirectory;

                this.ConnectionString = String.Format(ConnectionStringTemplate, m_basePath);
                this.UpdateDataQueries();
            }
        }

        private void UpdateDataQueries()
        {
            String connectionString = String.Format(ConnectionStringTemplate, this.BasePath);
            using (OleDbConnection connection = new OleDbConnection(this.ConnectionString))
            {
                connection.Open();
                DataTable schema = connection.GetOleDbSchemaTable(
                    OleDbSchemaGuid.Tables, new Object[] { null, null, null, "TABLE" });
                String firstTableName = null;
                foreach (DataRow row in schema.Rows)
                {
                    String table = row["Table_Name"].ToString();
                    String sheet = table.ToLowerInvariant();
                    if (sheet == OleDbSchemaConstants.Table.Collection + "$")
                    {
                        this.CollectionDataQuery = String.Format(CommandTemplate, table);
                    }
                    if (sheet == OleDbSchemaConstants.Table.FacetCategories + "$")
                    {
                        this.FacetCategoriesDataQuery = String.Format(CommandTemplate, table);
                    }
                    if (sheet == OleDbSchemaConstants.Table.Items + "$")
                    {
                        this.ItemsDataQuery = String.Format(CommandTemplate, table);
                    }
                    if (firstTableName == null) firstTableName = table;
                }

                if (this.ItemsDataQuery == null)
                {
                    this.ItemsDataQuery = String.Format(CommandTemplate, firstTableName);
                }
            }
        }

        private const String ConnectionStringTemplate = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; " +
            "Extended Properties=\"Excel 12.0 Xml;HDR=YES;FMT=Delimited;IMEX=1\"";

        // This is the registry key which controls how many rows the ACE driver will read before deciding whether a
        // column is a "text" type (i.e., 255 characters maximum) or a "memo" type (i.e., unlimited characters). When
        // reading a column with more than 255 characters, it may be necessary to change the value of this registry key
        // to "0" so that the driver will scan all rows before deciding.
        private const String RegistryKey =
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Office\12.0\Access Connectivity Engine\Engines\Excel\TypeGuessRows";

        private const String CommandTemplate = "SELECT * FROM [{0}]";

        private String m_basePath;

        private String m_baseDirectory;
    }
}
