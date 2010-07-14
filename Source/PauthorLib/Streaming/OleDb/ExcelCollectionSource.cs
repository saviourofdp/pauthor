//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Data;
using System.Data.OleDb;

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
    public class ExcelCollectionSource : OleDbCollectionSource
    {
        /// <summary>
        /// Creates a new Excel collection source and sets its <see cref="BasePath"/>.
        /// </summary>
        /// <param name="basePath">the path to the Excel file containing the collection's data</param>
        public ExcelCollectionSource(String basePath)
            : base(String.Format(ConnectionStringTemplate, basePath), basePath)
        {
            // Do nothing.
        }

        protected override void LoadHeaderData()
        {
            this.ConnectionString = String.Format(ConnectionStringTemplate, this.BasePath);
            this.UpdateDataQueries();

            base.LoadHeaderData();
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
    }
}
