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
    /// OleDbCollectionTarget is a Pivot collection target which writes data to an OLE DB connection.
    /// </summary>
    /// <remarks>
    /// Since OLE DB is a generic protocol for communicating with tabular data sources, this class allows you to
    /// interoperate with a large number of different data providers, including: SQL Server, Microsoft Access, Microsoft
    /// Excel, CSV files, and any other format for which theere is an OLE DB driver.
    /// 
    /// <para>This class provides a number of properties which control the class's interaction with the underlying OLE
    /// DB data source. By configuring these properties, you may specifiy the driver, source, and table names necessary
    /// to write to any OLE DB data source. This namespace includes a few subclasses which provide default values
    /// for writing to Excel and CSV files.</para>
    /// 
    /// <para>This class (and its subclasses) write data using the same basic schema containing three tables:
    /// collections, facet categories, and items. When writing a collection, this class will first drop all three
    /// tables, and then re-create them. Then, it will populate the tables with data drawn from the given source
    /// collection.</para>
    /// </remarks>
    public abstract class OleDbCollectionTarget : IPivotCollectionTarget
    {
        /// <summary>
        /// Subclasses must implement this method to return the connection string for their OLE DB data source.
        /// </summary>
        /// <returns>the connection string to use</returns>
        public abstract String GetConnectionString();

        /// <summary>
        /// Subclasses must implement this method to return the table name for the collection data.
        /// </summary>
        /// <remarks>
        /// This must follow the appropriate naming conventions for OLE DB in general, and the specific data source in
        /// particular.
        /// </remarks>
        /// <returns>the name of the collection table</returns>
        public abstract String GetCollectionTableName();

        /// <summary>
        /// Subclasses must implement this method to return the table name for the facet category data.
        /// </summary>
        /// <remarks>
        /// This must follow the appropriate naming conventions for OLE DB in general, and the specific data source in
        /// particular.
        /// </remarks>
        /// <returns>the name of the facet categories table</returns>
        public abstract String GetFacetCategoriesTableName();

        /// <summary>
        /// Subclasses must implement this method to return the table name for the items data.
        /// </summary>
        /// <remarks>
        /// This must follow the appropriate naming conventions for OLE DB in general, and the specific data source in
        /// particular.
        /// </remarks>
        /// <returns>the name of the items table</returns>
        public abstract String GetItemsTableName();

        /// <summary>
        /// Writes the given collection to the underlying OLE DB data source.
        /// </summary>
        /// <remarks>
        /// This method will first drop the target tables and re-create them. Then, it will write out the give
        /// collection source's data.
        /// </remarks>
        /// <param name="source">the collection source to write</param>
        public virtual void Write(IPivotCollectionSource source)
        {
            using (OleDbConnection connection = new OleDbConnection(this.GetConnectionString()))
            {
                connection.Open();
                this.DropExistingTables(connection);
                this.CreateSchema(source, connection);
                this.WriteCollectionTable(source, connection);
                this.WriteFacetCategoriesTable(source, connection);
                this.WriteItemsTable(source, connection);
            }
        }

        /// <summary>
        /// Disposes of any resources used by this collection target.
        /// </summary>
        public void Dispose()
        {
            // Do nothing.
        }

        private void DropExistingTables(OleDbConnection connection)
        {
            String[] tableNames = new String[]
            {
                this.GetCollectionTableName(),
                this.GetFacetCategoriesTableName(),
                this.GetItemsTableName(),
            };

            DataTable schema = connection.GetOleDbSchemaTable(
                OleDbSchemaGuid.Tables, new Object[] { null, null, null, "TABLE" });
            foreach (DataRow row in schema.Rows)
            {
                String tableName = row["Table_Name"].ToString();
                if (tableNames.Contains(tableName))
                {
                    OleDbCommand dropCommand = new OleDbCommand("DROP TABLE [" + tableName + "]", connection);
                    dropCommand.ExecuteNonQuery();
                }
            }
        }

        private void CreateSchema(IPivotCollectionSource source, OleDbConnection connection)
        {
            List<String> columns = new List<String>();
            OleDbCommand command = new OleDbCommand();
            command.Connection = connection;

            columns.AddRange(OleDbSchemaConstants.Collection.AllColumns);
            command.CommandText = this.GenerateCreateTableStatement(this.GetCollectionTableName(), columns);
            command.ExecuteNonQuery();

            columns.Clear();
            columns.AddRange(OleDbSchemaConstants.FacetCategory.AllColumns);
            command.CommandText = this.GenerateCreateTableStatement(this.GetFacetCategoriesTableName(), columns);
            command.ExecuteNonQuery();

            columns.Clear();
            columns.AddRange(OleDbSchemaConstants.Item.AllColumns);
            columns.AddRange(source.FacetCategories.Select(facetCategory => facetCategory.Name));
            command.CommandText = this.GenerateCreateTableStatement(this.GetItemsTableName(), columns);
            command.ExecuteNonQuery();
        }

        private String GenerateCreateTableStatement(String tableName, IEnumerable<String> columns)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("CREATE TABLE [");
            builder.Append(tableName);
            builder.Append("] (");
            bool needsDelimiter = false;
            foreach (String column in columns)
            {
                if (needsDelimiter) builder.Append(", ");
                builder.Append("[");
                builder.Append(column);
                builder.Append("] MEMO");
                needsDelimiter = true;
            }
            builder.Append(")");
            return builder.ToString();
        }

        private void WriteCollectionTable(IPivotCollectionSource source, OleDbConnection connection)
        {
            String insertScript = this.GenerateInsertQuery(this.GetCollectionTableName(), 6);
            OleDbCommand command = new OleDbCommand(insertScript, connection);
            foreach (String column in OleDbSchemaConstants.Collection.AllColumns)
            {
                command.Parameters.Add(column, OleDbType.VarChar);
            }

            int index = 0;
            command.Parameters[index++].Value = source.Name;
            command.Parameters[index++].Value = (source.Icon != null) ? source.Icon.SourcePath : null;
            command.Parameters[index++].Value = (source.BrandImage != null) ? source.BrandImage.SourcePath : null;
            command.Parameters[index++].Value = source.AdditionalSearchText;
            command.Parameters[index++].Value = (source.Copyright == null) ? null : source.Copyright.Title;
            command.Parameters[index++].Value = (source.Copyright == null) ? null : source.Copyright.Url;

            this.AssignNulls(command, index);
            command.ExecuteNonQuery();
        }

        private void WriteFacetCategoriesTable(IPivotCollectionSource source, OleDbConnection connection)
        {
            String insertScript = this.GenerateInsertQuery(this.GetFacetCategoriesTableName(), 8);
            OleDbCommand command = new OleDbCommand(insertScript, connection);
            foreach (String column in OleDbSchemaConstants.FacetCategory.AllColumns)
            {
                command.Parameters.Add(column, OleDbType.VarChar);
            }

            foreach (PivotFacetCategory facetCategory in source.FacetCategories)
            {
                int index = 0;
                command.Parameters[index++].Value = facetCategory.Name;
                command.Parameters[index++].Value = facetCategory.Type.ToString();
                command.Parameters[index++].Value = facetCategory.Format;
                command.Parameters[index++].Value = facetCategory.IsFilterVisible.ToString().ToLowerInvariant();
                command.Parameters[index++].Value = facetCategory.IsMetaDataVisible.ToString().ToLowerInvariant();
                command.Parameters[index++].Value = facetCategory.IsWordWheelVisible.ToString().ToLowerInvariant();
                command.Parameters[index++].Value =
                    (facetCategory.SortOrder == null) ? null : facetCategory.SortOrder.Name;
                command.Parameters[index++].Value = this.GenerateSortValuesString(facetCategory);

                this.AssignNulls(command, index);
                command.ExecuteNonQuery();
            }
        }

        private void WriteItemsTable(IPivotCollectionSource source, OleDbConnection connection)
        {
            List<PivotFacetCategory> facetCategories = new List<PivotFacetCategory>(source.FacetCategories);
            String insertScript = this.GenerateInsertQuery(this.GetItemsTableName(),
                OleDbSchemaConstants.Item.AllColumns.Length + facetCategories.Count);
            OleDbCommand command = new OleDbCommand(insertScript, connection);
            foreach (String column in OleDbSchemaConstants.Item.AllColumns)
            {
                command.Parameters.Add(column, OleDbType.VarChar);
            }
            foreach (PivotFacetCategory facetCategory in facetCategories)
            {
                command.Parameters.Add(facetCategory.Name, OleDbType.VarChar);
            }

            foreach (PivotItem item in source.Items)
            {
                int index = 0;
                command.Parameters[index++].Value = item.Name;
                command.Parameters[index++].Value = (item.Image == null) ? null : item.Image.SourcePath;
                command.Parameters[index++].Value = item.Description;
                command.Parameters[index++].Value = item.Href;
                command.Parameters[index++].Value = this.GenerateRelatedLinksValue(item);
                foreach (PivotFacetCategory facetCategory in facetCategories)
                {
                    if (item.FacetCategories.Contains(facetCategory.Name)) 
                    {
                        command.Parameters[index].Value = item.GetAllFacetValuesAsString(
                            facetCategory.Name, OleDbSchemaConstants.FacetValueDelimiter);
                    }
                    else
                    {
                        command.Parameters[index].Value = null;
                    }
                    index++;
                }

                this.AssignNulls(command, index);
                command.ExecuteNonQuery();
            }
        }

        private String GenerateInsertQuery(String tableName, int parameterCount)
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append("INSERT INTO [");
            builder.Append(tableName);
            builder.Append("] VALUES (");
            for (int i = 0; i < parameterCount; i++)
            {
                builder.Append("?");
                if (i < parameterCount - 1) builder.Append(", ");
            }
            builder.Append(")");
            return builder.ToString();
        }

        private String GenerateSortValuesString(PivotFacetCategory facetCategory)
        {
            if (facetCategory.SortOrder == null) return null;
            String[] allValues = new String[facetCategory.SortOrder.Values.Count()];
            int index = 0;
            foreach (String value in facetCategory.SortOrder.Values)
            {
                allValues[index++] = value;
            }
            String facetValueString = String.Join(OleDbSchemaConstants.FacetValueDelimiter, allValues);
            return facetValueString;
        }

        private String GenerateRelatedLinksValue(PivotItem item)
        {
            if (item.HasRelatedLinks == false) return null;

            StringBuilder builder = new StringBuilder();
            bool needsDelimiter = false;
            foreach (PivotLink link in item.RelatedLinks)
            {
                if (needsDelimiter) builder.Append(OleDbSchemaConstants.FacetValueDelimiter);
                builder.Append(link.Title);
                builder.Append(OleDbSchemaConstants.LinkPartDelimiter);
                builder.Append(link.Url);
                needsDelimiter = true;
            }
            return builder.ToString();
        }

        private void AssignNulls(OleDbCommand command, int index)
        {
            while (index > 0)
            {
                index--;
                if (command.Parameters[index].Value == null)
                {
                    command.Parameters[index].Value = DBNull.Value;
                }
            }
        }
    }
}
