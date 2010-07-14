//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System.Linq;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Test.Core
{
    public class PivotItemUnitTest : PauthorUnitTest
    {
        public void TestAddFacetValueExistingFacetCategory()
        {
            emptyCollection.FacetCategories.Add(new PivotFacetCategory("bravo", PivotFacetType.String));

            PivotItem item = new PivotItem("alpha", emptyCollection);
            item.AddFacetValues("bravo", "charlie");
            item.AddFacetValues("bravo", "delta");

            AssertEqual("charlie", item.GetAllFacetValues("bravo")[0]);
            AssertEqual("delta", item.GetAllFacetValues("bravo")[1]);
            AssertEqual(2, item.GetAllFacetValues("bravo").Count);
            AssertEqual(1, item.FacetCategories.Count());
        }

        public void TestAddFacetValueExistingFacetCategoryAlreadyInCollection()
        {
            PivotItem item = sampleCollection1.Items[1];
            item.AddFacetValues("Subject", "New Car");

            AssertEqual("Vehicle", item.GetAllFacetValues("Subject")[0]);
            AssertEqual("New Car", item.GetAllFacetValues("Subject")[1]);
            AssertEqual(2, item.GetAllFacetValues("Subject").Count);
            AssertEqual(2, item.FacetCategories.Count);
        }

        public void TestAddFacetValueNewFacetCategory()
        {
            emptyCollection.FacetCategories.Add(new PivotFacetCategory("bravo", PivotFacetType.String));

            PivotItem item = new PivotItem("alpha", emptyCollection);
            item.AddFacetValues("bravo", "charlie");
            emptyCollection.Items.Add(item);

            item.AddFacetValues("delta", "echo");

            AssertEqual("charlie", item.GetAllFacetValues("bravo")[0]);
            AssertEqual(1, item.GetAllFacetValues("bravo").Count);
            AssertEqual("echo", item.GetAllFacetValues("delta")[0]);
            AssertEqual(1, item.GetAllFacetValues("delta").Count);
            AssertEqual(2, item.FacetCategories.Count);
        }

        public void TestSetRelatedLinks()
        {
            sampleItem1.RelatedLinks = new PivotLink[]
            {
                new PivotLink("alpha", "bravo"), new PivotLink("charlie", "delta")
            };

            AssertEqual("alpha", sampleItem1.RelatedLinks.ToArray()[0].Title);
            AssertEqual("bravo", sampleItem1.RelatedLinks.ToArray()[0].Url);
            AssertEqual("charlie", sampleItem1.RelatedLinks.ToArray()[1].Title);
            AssertEqual("delta", sampleItem1.RelatedLinks.ToArray()[1].Url);
            AssertEqual(2, sampleItem1.RelatedLinks.Count());
        }

        public void TestRelatedLinksClobberExisting()
        {
            sampleItem1.RelatedLinks = new PivotLink[]
            {
                new PivotLink("alpha", "bravo"), new PivotLink("charlie", "delta")
            };
            sampleItem1.RelatedLinks = new PivotLink[]
            {
                new PivotLink("echo", "foxtrot")
            };

            AssertEqual("echo", sampleItem1.RelatedLinks.ToArray()[0].Title);
            AssertEqual("foxtrot", sampleItem1.RelatedLinks.ToArray()[0].Url);
            AssertEqual(1, sampleItem1.RelatedLinks.Count());
        }

        public PivotCollection emptyCollection { get; set; }

        public PivotCollection sampleCollection1 { get; set; }

        public PivotItem sampleItem1 { get; set; }
    }
}
