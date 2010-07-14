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

namespace Microsoft.LiveLabs.Pauthor.Streaming.OleDb
{
    /// <summary>
    /// CsvCollectionTarget is a subclass of <see cref="OleDbCollectionTarget"/> which specifically writes to the CSV
    /// format.
    /// </summary>
    /// <remarks>
    /// As with the parent class, this class uses the specific schema described by the <see
    /// cref="OleDbSchemaConstants"/> class. However, since CSV doesn't naturally support a notion of multiple tables,
    /// this class will create a single CSV file for each separate table. This class provides a set of properties to
    /// allow you to independently specify the name of each CSV file for the three tables in the collection schema.
    /// </remarks>
    public class CsvCollectionTarget : OleDbCollectionTarget, ILocalCollectionTarget
    {
        /// <summary>
        /// Creates a new CSV collection target and sets its <see cref="BasePath"/>.
        /// </summary>
        /// <param name="basePath">the path to the CSV file into which the new target will write its item data</param>
        public CsvCollectionTarget(String basePath)
        {
            this.BasePath = basePath;
        }

        /// <summary>
        /// The path to the CSV file into which this collection target will write its item data.
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
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BasePath cannot be null");
                if (Path.GetFileNameWithoutExtension(value).Contains('.'))
                {
                    throw new ArgumentException(
                        "BasePath cannot contain '.' characters within the file name: " + value);
                }

                this.BaseDirectory = Directory.GetParent(value).FullName;
                this.ItemsFileName = Path.GetFileName(value);

                String baseFileName = Path.GetFileNameWithoutExtension(value);
                this.CollectionFileName = baseFileName + "_collection.csv";
                this.FacetCategoriesFileName = baseFileName + "_facetcategories.csv";
            }
        }

        /// <summary>
        /// The path to the directory into which this collection target will write all its CSV files.
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
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("BaseDirectory cannot be null");
                if (Directory.Exists(value) == false)
                {
                    Directory.CreateDirectory(value);
                }
                m_baseDirectory = value;
            }
        }

        /// <summary>
        /// A file name into which this collection target will write the top-level collection data.
        /// </summary>
        /// <remarks>
        /// The named file will be created in the <see cref="BaseDirectory"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        /// <exception cref="ArgumentException">if the given file does not exist</exception>
        public String CollectionFileName
        {
            get { return m_collectionFileName; }

            set { m_collectionFileName = value; }
        }

        /// <summary>
        /// A file name into which this collection target will write the facet category data.
        /// </summary>
        /// <remarks>
        /// The named file will be created in the <see cref="BaseDirectory"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        /// <exception cref="ArgumentException">if the given file does not exist</exception>
        public String FacetCategoriesFileName
        {
            get { return m_facetCategoriesFileName; }

            set { m_facetCategoriesFileName = value; }
        }

        /// <summary>
        /// A file name into which this collection target will write the item data.
        /// </summary>
        /// <remarks>
        /// The named file will be created in the <see cref="BaseDirectory"/>.
        /// </remarks>
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

                m_itemsFileName = value;
            }
        }

        /// <summary>
        /// Returns an appropriate connection string for creating the collection specified by this collection target.
        /// </summary>
        /// <returns>the connection string to use</returns>
        public override String GetConnectionString()
        {
            return String.Format(ConnectionStringTemplate, this.BaseDirectory);
        }

        /// <summary>
        /// Returns the <see cref="CollectionFileName"/> as the name of the collection table.
        /// </summary>
        /// <returns>the name of the collection table</returns>
        public override String GetCollectionTableName()
        {
            return this.CollectionFileName;
        }

        /// <summary>
        /// Returns the <see cref="FacetCategoriesFileName"/> as the name of the facet categories table.
        /// </summary>
        /// <returns>the name of the collection table</returns>
        public override String GetFacetCategoriesTableName()
        {
            return this.FacetCategoriesFileName;
        }

        /// <summary>
        /// Returns the <see cref="ItemsFileName"/> as the name of the items table.
        /// </summary>
        /// <returns>the name of the collection table</returns>
        public override String GetItemsTableName()
        {
            return this.ItemsFileName;
        }

        /// <summary>
        /// Writes the given collection to the set of CSV files specified by this collection target.
        /// </summary>
        /// <remarks>
        /// This method will first delete any existing CSV files, and then will write out the new collection data.
        /// </remarks>
        /// <param name="source">the collection source to write</param>
        public override void Write(IPivotCollectionSource source)
        {
            base.Write(source);

            String schemaFile = Path.Combine(this.BaseDirectory, "schema.ini");
            if (File.Exists(schemaFile))
            {
                File.Delete(schemaFile);
            }
        }

        private const String ConnectionStringTemplate = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; " +
            "Extended Properties=\"Text;HDR=YES;FMT=Delimited\"";

        private String m_baseDirectory;

        private String m_collectionFileName;

        private String m_facetCategoriesFileName;

        private String m_itemsFileName;
    }
}
