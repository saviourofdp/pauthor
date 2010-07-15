//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Linq;
using System.IO;

using Microsoft.LiveLabs.Pauthor.Core;
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

        public void TestBrokenRelatedLinks()
        {
            PivotCollection collection = new PivotCollection();
            collection.FacetCategories.Add(new PivotFacetCategory("alpha", PivotFacetType.String));

            PivotItem item = new PivotItem("0", collection);
            item.AddFacetValues("alpha", "alpha");
            item.AddRelatedLink(new PivotLink(null, "http://pauthor.codeplex.com"));
            collection.Items.Add(item);

            item = new PivotItem("1", collection);
            item.AddFacetValues("alpha", "bravo");
            item.AddRelatedLink(new PivotLink("charlie", null));
            collection.Items.Add(item);

            PivotCollectionBuffer buffer = new PivotCollectionBuffer(collection);
            String targetPath = Path.Combine(WorkingDirectory, "sample.cxml");
            LocalCxmlCollectionTarget target = new LocalCxmlCollectionTarget(targetPath);
            target.Write(buffer);

            AssertCxmlSchemaValid(targetPath);

            CxmlCollectionSource targetAsSource = new CxmlCollectionSource(targetPath);
            buffer.Write(targetAsSource);

            AssertEqual("Related Link", buffer.Collection.Items[0].RelatedLinks.First().Title);
            AssertEqual("http://pauthor.codeplex.com", buffer.Collection.Items[0].RelatedLinks.First().Url);
            AssertEqual(0, buffer.Collection.Items[1].RelatedLinks.Count());
        }
    }
}
