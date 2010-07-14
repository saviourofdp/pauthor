//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.IO;

using Microsoft.LiveLabs.Pauthor.Streaming.OleDb;

namespace Microsoft.LiveLabs.Pauthor.Test.Streaming.OleDb
{
    public class ExcelCollectionModuleTest : PauthorUnitTest
    {
        public void TestRoundTrip()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"Excel\sample.xlsx");
            String targetPath = Path.Combine(WorkingDirectory, "sample.xlsx");

            ExcelCollectionSource source = new ExcelCollectionSource(sourcePath);
            ExcelCollectionTarget target = new ExcelCollectionTarget(targetPath);
            ExcelCollectionSource targetAsSource = new ExcelCollectionSource(targetPath);

            target.Write(source);

            AssertCollectionsEqual(source, targetAsSource);
        }
    }
}
