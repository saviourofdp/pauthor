//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.IO;

using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Test.Streaming
{
    public class LocalCxmlModuleTest : PauthorUnitTest
    {
        public void TestSampleCollectionValidity()
        {
            AssertCxmlSchemaValid(Path.Combine(this.ResourceDirectory, @"DeepZoom\sample.cxml"));
        }

        public void TestRoundTrip()
        {
            String sourcePath = Path.Combine(this.ResourceDirectory, @"DeepZoom\sample.cxml");
            String targetPath = Path.Combine(WorkingDirectory, "sample.cxml");

            CxmlCollectionSource source = new CxmlCollectionSource(sourcePath);
            LocalCxmlCollectionTarget target = new LocalCxmlCollectionTarget(targetPath);
            CxmlCollectionSource targetAsSource = new CxmlCollectionSource(targetPath);

            target.Write(source);

            AssertCxmlSchemaValid(targetPath);
            AssertCollectionsEqual(source, targetAsSource);
        }
    }
}