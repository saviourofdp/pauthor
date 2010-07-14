//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.IO;

using Microsoft.LiveLabs.Pauthor.CLI;
using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Streaming;
using Microsoft.LiveLabs.Pauthor.Streaming.OleDb;

namespace Microsoft.LiveLabs.Pauthor.Test.CLI
{
    public class PauthorProgramModuleTest : PauthorUnitTest
    {
        public void TestCsvToCxml()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"CSV\sample.csv");
            String targetPath = Path.Combine(WorkingDirectory, "sample.cxml");

            PauthorProgram.Main(new String[] { "/source", "csv", sourcePath, "/target", "cxml", targetPath });

            IPivotCollectionSource source = new CsvCollectionSource(sourcePath);
            IPivotCollectionSource targetAsSource = new CxmlCollectionSource(targetPath);

            AssertCxmlSchemaValid(targetPath);
            AssertCollectionsEqual(source, targetAsSource);
            AssertFileExists(@"sample.cxml");
            AssertFileExists(@"sample_icon.ico");
            AssertFileExists(@"sample_images\0.png");
        }

        public void TestExcelToDeepZoom()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"Excel\sample.xlsx");
            String targetPath = Path.Combine(WorkingDirectory, "sample.cxml");

            PauthorProgram.Main(new String[] { "/source", "excel", sourcePath, "/target", "deepzoom", targetPath });

            IPivotCollectionSource source = new ExcelCollectionSource(sourcePath);
            IPivotCollectionSource targetAsSource = new CxmlCollectionSource(targetPath);
            AssertCxmlSchemaValid(targetPath);
            AssertCollectionsEqual(source, targetAsSource);

            AssertFileExists(@"sample.cxml");
            AssertFileExists(@"sample_icon.ico");
            AssertFileExists(@"sample_deepzoom\sample.dzc");
            AssertFileExists(@"sample_deepzoom\sample_files\0\0_0.png");
            AssertFileExists(@"sample_deepzoom\0.dzi");
            AssertFileExists(@"sample_deepzoom\0_files\0\0_0.png");
        }

        public void TestDeepZoomToCsv()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"DeepZoom\sample.cxml");
            String targetPath = Path.Combine(WorkingDirectory, "sample.csv");

            PauthorProgram.Main(new String[] { "/source", "deepzoom", sourcePath, "/target", "csv", targetPath });

            IPivotCollectionSource source = new CxmlCollectionSource(sourcePath);
            IPivotCollectionSource targetAsSource = new CsvCollectionSource(targetPath);
            AssertCollectionsEqual(source, targetAsSource);

            AssertFileExists(@"sample.csv");
            AssertFileExists(@"sample_collection.csv");
            AssertFileExists(@"sample_facetcategories.csv");
            AssertFileExists(@"sample_icon.ico");
            AssertFileExists(@"sample_images\0.png");
        }

        public void TestCxmlToExcel()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"CXML\sample.cxml");
            String targetPath = Path.Combine(WorkingDirectory, "sample.xlsx");

            PauthorProgram.Main(new String[] { "/source", "cxml", sourcePath, "/target", "excel", targetPath });

            IPivotCollectionSource source = new CxmlCollectionSource(sourcePath);
            IPivotCollectionSource targetAsSource = new ExcelCollectionSource(targetPath);
            AssertCollectionsEqual(source, targetAsSource);

            AssertFileExists(@"sample.xlsx");
            AssertFileExists(@"sample_icon.ico");
            AssertFileExists(@"sample_images\0.png");
        }

        public void TestCsvToCxmlWithTemplate()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"CSV\sample_missing_images.csv");
            String templatePath = Path.Combine(this.ResourceDirectory, @"CSV\template-1.htm");
            String targetPath = Path.Combine(WorkingDirectory, "sample.cxml");

            PauthorProgram.Main(new String[] {
                "/source", "csv", sourcePath, "/html-template", templatePath, "/target", "cxml", targetPath });

            AssertCxmlSchemaValid(targetPath);
            AssertFileExists(@"sample.cxml");
            AssertFileExists(@"sample_images\0.png");

            IPivotCollectionSource source = new CsvCollectionSource(sourcePath);
            IPivotCollectionSource targetAsSource = new CxmlCollectionSource(targetPath);
            PivotCollectionBuffer buffer = new PivotCollectionBuffer();
            buffer.Write(source);
            PivotCollection expected = buffer.Collection;
            buffer.Write(targetAsSource);
            PivotCollection actual = buffer.Collection;

            AssertEqual(expected.Name, actual.Name);
            AssertEqual(expected.Icon, actual.Icon);
            AssertEqual(expected.FacetCategories[0].Name, actual.FacetCategories[0].Name);
            AssertEqual(expected.FacetCategories.Count, actual.FacetCategories.Count);
            AssertEqual(expected.Items[0].Name, actual.Items[0].Name);
            AssertNotEqual(expected.Items[0].Image, actual.Items[0].Image);
        }

        public void TestChainedConversions()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"CSV\sample.csv");
            String step1Path = Path.Combine(WorkingDirectory, @"step1\sample.cxml");
            String step2Path = Path.Combine(WorkingDirectory, @"step2\sample.xlsx");
            String step3Path = Path.Combine(WorkingDirectory, @"step3\sample.cxml");
            String step4Path = Path.Combine(WorkingDirectory, @"step4\sample.csv");

            PauthorProgram.Main(new String[] { "/source", "csv", sourcePath, "/target", "cxml", step1Path });
            IPivotCollectionSource source = new CsvCollectionSource(sourcePath);
            IPivotCollectionSource targetAsSource = new CxmlCollectionSource(step1Path);
            AssertCxmlSchemaValid(step1Path);
            AssertCollectionsEqual(source, targetAsSource);

            PauthorProgram.Main(new String[] { "/source", "cxml", step1Path, "/target", "excel", step2Path });
            source = new CxmlCollectionSource(step1Path);
            targetAsSource = new ExcelCollectionSource(step2Path);
            AssertCollectionsEqual(source, targetAsSource);

            PauthorProgram.Main(new String[] { "/source", "excel", step2Path, "/target", "deepzoom", step3Path });
            source = new ExcelCollectionSource(step2Path);
            targetAsSource = new CxmlCollectionSource(step3Path);
            AssertCxmlSchemaValid(step3Path);
            AssertCollectionsEqual(source, targetAsSource);

            PauthorProgram.Main(new String[] { "/source", "deepzoom", step3Path, "/target", "csv", step4Path });
            source = new CxmlCollectionSource(step3Path);
            targetAsSource = new CsvCollectionSource(step4Path);
            AssertCollectionsEqual(source, targetAsSource);

            source = new CsvCollectionSource(sourcePath);
            targetAsSource = new CsvCollectionSource(step4Path);
            AssertCollectionsEqual(source, targetAsSource);
        }
    }
}
