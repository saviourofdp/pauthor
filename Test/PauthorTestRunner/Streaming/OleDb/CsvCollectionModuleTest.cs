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
    public class CsvCollectionModuleTest : PauthorUnitTest
    {
        public void TestRoundTrip()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"CSV\sample.csv");
            String targetPath = Path.Combine(WorkingDirectory, "sample.csv");

            CsvCollectionSource source = new CsvCollectionSource(sourcePath);
            CsvCollectionTarget target = new CsvCollectionTarget(targetPath);

            target.Write(source);

            CsvCollectionSource targetAsSource = new CsvCollectionSource(targetPath);
            AssertCollectionsEqual(source, targetAsSource);
        }
    }
}
