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
    /// ExcelCollectionTarget is a subclass of <see cref="OleDbCollectionTarget"/> which specifically writes to the
    /// Microsoft Excel 2007 format.
    /// </summary>
    /// <remarks>
    /// As with the parent class, this class uses the specific schema described by the <see
    /// cref="OleDbSchemaConstants"/> class. However, since Excel doesn't naturally support a notion of multiple tables,
    /// this class will create a single sheet within the Excel file for each separate table.
    /// </remarks>
    public class ExcelCollectionTarget : OleDbCollectionTarget, ILocalCollectionTarget
    {
        /// <summary>
        /// Creates a new Excel collection target and sets its <see cref="BasePath"/>.
        /// </summary>
        /// <param name="basePath">the path to the Excel file which will contain the collection's data</param>
        public ExcelCollectionTarget(String basePath)
        {
            this.BasePath = basePath;
        }

        /// <summary>
        /// The path to the Excel file which will contain the collection data.
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
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BasePath cannot be null");
                if (Path.GetFileNameWithoutExtension(value).Contains('.'))
                {
                    throw new ArgumentException(
                        "BasePath cannot contain '.' characters within the file name: " + value);
                }

                if (Directory.GetParent(value).Exists == false)
                {
                    Directory.CreateDirectory(Directory.GetParent(value).FullName);
                }
                m_basePath = Path.ChangeExtension(value, "xlsx");
            }
        }

        /// <summary>
        /// Returns an appropriate connection string for creating the collection specified by this collection target.
        /// </summary>
        /// <returns>the connection string to use</returns>
        public override String GetConnectionString()
        {
            return String.Format(ConnectionStringTemplate, this.BasePath);
        }

        /// <summary>
        /// Returns <see cref="OleDbSchemaConstants.Table.Collection"/> as the name of the collection table.
        /// </summary>
        /// <returns>the name of the collection table</returns>
        public override String GetCollectionTableName()
        {
            return OleDbSchemaConstants.Table.Collection;
        }

        /// <summary>
        /// Returns <see cref="OleDbSchemaConstants.Table.FacetCategories"/> as the name of the facet categories table.
        /// </summary>
        /// <returns>the name of the collection table</returns>
        public override String GetFacetCategoriesTableName()
        {
            return OleDbSchemaConstants.Table.FacetCategories;
        }

        /// <summary>
        /// Returns <see cref="OleDbSchemaConstants.Table.Items"/> as the name of the items table.
        /// </summary>
        /// <returns>the name of the collection table</returns>
        public override String GetItemsTableName()
        {
            return OleDbSchemaConstants.Table.Items;
        }

        private const String ConnectionStringTemplate = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; " +
            "Extended Properties=\"Excel 12.0 Xml;HDR=YES\"";

        private String m_basePath;
    }
}