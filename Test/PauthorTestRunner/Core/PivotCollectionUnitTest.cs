//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Test.Core
{
    public class PivotCollectionUnitTest : PauthorUnitTest
    {
        public void TestSimpleCollection()
        {
            AssertNotNull(collection);

            AssertEqual("Letters", collection.FacetCategories[0].Name);
            AssertEqual("Subject", collection.FacetCategories[1].Name);
            AssertEqual(2, collection.FacetCategories.Count);

            AssertEqual("Alpha", collection.Items[0].Name);
            AssertEqual("Bravo", collection.Items[1].Name);
            AssertEqual("Charlie", collection.Items[2].Name);
            AssertEqual(3, collection.Items.Count);

            PivotItem alpha = collection.Items[0];
            AssertEqual("0", alpha.Id);
            AssertEqual("alpha alpha alpha alpha", alpha.Description);
            AssertEqual("http://www.alpha.com", alpha.Href);
            AssertEqual("Earth, Sky, Water, Explosion", alpha.GetAllFacetValuesAsString("Subject", ", "));
        }

        public void TestAddDuplicateFacetCategory()
        {
            try
            {
                collection.FacetCategories.Add(new PivotFacetCategory("Letters", PivotFacetType.String));
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("Cannot add a duplicate facet category name (Letters already exists)", e.Message);
            }
        }

        public void TestRemoveFacetCategoryInUse()
        {
            collection.FacetCategories.Remove(collection.FacetCategories["Letters"]);

            List<PivotFacetCategory> facetCategories = collection.FacetCategories.ToList();
            AssertEqual("Subject", facetCategories[0].Name);
            AssertEqual(1, facetCategories.Count);

            AssertEqual("Subject", collection.Items[0].FacetCategories.First());
            AssertEqual(1, collection.Items[0].FacetCategories.Count());
        }

        public void TestAddItemsWithUnsupportedFacetCategories()
        {
            try
            {
                collection.Items.Add(extraItem1);
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("Item Id 3 has an incompatible value (type: DateTime) for facet category Launch Date",
                    e.Message);
            }
        }

        public void TestCreatingNewFacetCategoriesWhileAddingItems()
        {
            collection.InferFacetCategories = true;
            collection.Items.Add(extraItem1);

            AssertEqual("Launch Date", collection.FacetCategories[2].Name);
            AssertEqual(3, collection.FacetCategories.Count);
        }

        public void TestAddItemsWithUnnormalizableFacetCategories()
        {
            try
            {
                collection.FacetCategories.Add(extraFacetCategory2);
                collection.Items.Add(extraItem1);
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("Item Id 3 has an incompatible value (type: DateTime) for facet category Launch Date",
                    e.Message);
            }
        }

        public void TestAddFacetValueNewFacetCategoryAlreadyInCollectionDoNotInferFacetCategories()
        {
            try
            {
                PivotItem item = collection.Items[1];
                item.AddFacetValues("alpha", "bravo");
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("Item Id 1 has an incompatible value (type: String) for facet category alpha", e.Message);
            }
        }

        public void TestAddFacetValueNewFacetCategoryAlreadyInCollectionInferFacetCategories()
        {
            collection.InferFacetCategories = true;
            PivotItem item = collection.Items[1];
            item.AddFacetValues("alpha", "bravo");

            AssertEqual("bravo", item.GetFacetValue("alpha"));
            AssertEqual(1, item.GetAllFacetValues("alpha").Count);
            AssertEqual(3, item.FacetCategories.Count);

            AssertEqual("alpha", collection.FacetCategories[2].Name);
            AssertEqual(PivotFacetType.String, collection.FacetCategories[2].Type);
            AssertEqual(3, collection.FacetCategories.Count);
        }

        public void TestAddFacetValueNewFacetCategoryAlreadyInCollectionInferFacetCategoriesIncompatibleType()
        {
            try
            {
                PivotItem item = collection.Items[1];
                item.AddFacetValues("Letters", "bravo");
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("Item Id 1 has an incompatible value (type: String) for facet category Letters", e.Message);
            }
        }

        private PivotCollection collection { get; set; }

        private PivotItem extraItem1 { get; set; }

        private PivotItem extraItem2 { get; set; }

        private PivotFacetCategory extraFacetCategory1 { get; set; }

        private PivotFacetCategory extraFacetCategory2 { get; set; }
    }
}
