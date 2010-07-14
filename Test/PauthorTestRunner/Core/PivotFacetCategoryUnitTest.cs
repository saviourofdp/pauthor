//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Linq;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Test.Core
{
    public class PivotFacetCategoryUnitTest : PauthorUnitTest
    {
        public void TestFilterVisibleWhenShouldBe()
        {
            PivotFacetCategory category = new PivotFacetCategory("alpha", PivotFacetType.String);
            category.IsFilterVisible = true;

            category = new PivotFacetCategory("bravo", PivotFacetType.Number);
            category.IsFilterVisible = true;

            category = new PivotFacetCategory("charlie", PivotFacetType.DateTime);
            category.IsFilterVisible = true;
        }

        public void TestFilterNotVisbleWhenShouldNotBe()
        {
            try
            {
                PivotFacetCategory category = new PivotFacetCategory("bravo", PivotFacetType.LongString);
                category.IsFilterVisible = true;
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("A facet can only be filter visible if it is a DateTime, Number or String facet type " +
                    "(currently: LongString)", e.Message);
            }

            try
            {
                PivotFacetCategory category = new PivotFacetCategory("charlie", PivotFacetType.Link);
                category.IsFilterVisible = true;
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("A facet can only be filter visible if it is a DateTime, Number or String facet type " +
                    "(currently: Link)", e.Message);
            }
        }

        public void TestWordWheelVisibleWhenShouldBe()
        {
            PivotFacetCategory category = new PivotFacetCategory("alpha", PivotFacetType.String);
            category.IsWordWheelVisible = true;

            category = new PivotFacetCategory("bravo", PivotFacetType.LongString);
            category.IsWordWheelVisible = true;

            category = new PivotFacetCategory("charlie", PivotFacetType.Link);
            category.IsWordWheelVisible = true;
        }

        public void TestWordWheelNotVisibleWhenShouldNotBe()
        {
            try
            {
                PivotFacetCategory category = new PivotFacetCategory("bravo", PivotFacetType.Number);
                category.IsWordWheelVisible = true;
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("A facet can only be word wheel visible if it is a Link, LongString, or String facet " +
                    "type (currently: Number)", e.Message);
            }

            try
            {
                PivotFacetCategory category = new PivotFacetCategory("charlie", PivotFacetType.DateTime);
                category.IsWordWheelVisible = true;
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("A facet can only be word wheel visible if it is a Link, LongString, or String facet " +
                    "type (currently: DateTime)", e.Message);
            }
        }

        public void TestSetSortOrderNoValues()
        {
            subjectFacet.SortOrder = new PivotFacetSortOrder("alpha");
            AssertEqual(0, subjectFacet.SortOrder.Values.Count());
        }

        public void TestSetSortOrderMultipleValues()
        {
            PivotFacetSortOrder sortOrder = new PivotFacetSortOrder("alpha");
            sortOrder.AddValue("zulu");
            sortOrder.AddValue("yankee");
            sortOrder.AddValue("whiskey");

            subjectFacet.SortOrder = sortOrder;

            AssertEqual("zulu", subjectFacet.SortOrder.Values.ToArray()[0]);
            AssertEqual("yankee", subjectFacet.SortOrder.Values.ToArray()[1]);
            AssertEqual("whiskey", subjectFacet.SortOrder.Values.ToArray()[2]);
            AssertEqual(3, subjectFacet.SortOrder.Values.Count());
        }

        public void TestSetSortOrderDuplicateValues()
        {
            try
            {
                PivotFacetSortOrder sortOrder = new PivotFacetSortOrder("alpha");
                sortOrder.AddValue("zulu");
                sortOrder.AddValue("yankee");
                sortOrder.AddValue("whiskey");
                sortOrder.AddValue("alpha");
                sortOrder.AddValue("whiskey");
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("Cannot have duplicate sort order values: whiskey", e.Message);
            }
        }

        public void TestSetSortOrderWrongType()
        {
            try
            {
                lettersFacet.SortOrder = new PivotFacetSortOrder("alpha");
                Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                AssertEqual("A facet can only have a sort order if it is a String", e.Message);
            }
        }

        public PivotFacetCategory subjectFacet { get; set; }

        public PivotFacetCategory lettersFacet { get; set; }
    }
}
