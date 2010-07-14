//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;

using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Imaging;

namespace Microsoft.LiveLabs.Pauthor.Test.Imaging
{
    public class HtmlImageCreatorUnitTest : PauthorUnitTest
    {
        public void TestSimpleTemplateWithStandardElements()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{name};{href};{description}</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>Delta;http://www.delta.com;delta delta delta delta</body></html>", html);
        }

        public void TestTemplateWithMultipleOccurancesOfStandardElements()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{name};{name};{name};{name}</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>Delta;Delta;Delta;Delta</body></html>", html);
        }
        
        public void TestTemplateWithMixedStandardElementsAndSingleValueFacets()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{name};{letters}</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>Delta;5</body></html>", html);
        }

        public void TestTemplateWithIndexedTagsOutOfRange()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{subject:0};{subject:1};{subject:2};{subject:3}</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>Woman;Jewelry;;</body></html>", html);
        }

        public void TestTemplateWithIndexedTagMixedCase()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{subject:0};{Subject:1};{SUBJECT:2};{SuBjEcT:3}</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>Woman;Jewelry;;</body></html>", html);
        }

        public void TestTemplateWithJoinTagSingleValue()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{letters:join:, }</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>5</body></html>", html);
        }

        public void TestTemplateWithJoinTagMultipleValues()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{subject:join:, }</body></html>";

            String html = creator.InstantiateTemplate(sampleItem2);
            AssertEqual("<html><body>Earth, Sky, Water, Explosion</body></html>", html);
        }

        public void TestTemplateWithMixedCaseJoinTags()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{subject:join:, };{Subject:Join:, };" +
                "{SUBJECT:JOIN:, };{sUbJeCt:JoIn:, }</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>Woman, Jewelry;Woman, Jewelry;" +
                "Woman, Jewelry;Woman, Jewelry</body></html>", html);
        }

        public void TestTemplateWithMixedCaseStandardElements()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{name};{Name};{NAME};{NaMe}</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>Delta;Delta;Delta;Delta</body></html>", html);
        }

        public void TestTemplateWithMixedCaseFacets()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{letters};{Letters};{LETTERS};{LeTtErS}</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>5;5;5;5</body></html>", html);
        }

        public void TestTemplateWithFacetNamesWithSpaces()
        {
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.HtmlTemplate = "<html><body>{launch date}</body></html>";

            String html = creator.InstantiateTemplate(sampleItem1);
            AssertEqual("<html><body>2003-10-03T20:35:00</body></html>", html);
        }


        /// <summary>
        /// This is a tripwire test to alert us if someone changes the hardcoded default values. All other tests use
        /// these public constants directly
        /// </summary>
        public void TestDefaultValuesMatchSpec()
        {
            AssertEqual(5000, HtmlImageCreator.MaximumDimension);
            AssertEqual(1200, HtmlImageCreator.DefaultWidth);
            AssertEqual(1500, HtmlImageCreator.DefaultHeight);
        }

        /// <summary>
        /// This test will fail if the default values are changed
        /// </summary>
        public void TestDetermineSizeNotDefined()
        {
            int width = 0, height = 0;
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.ReadSizeFromTemplate("<html>{name}</html>", ref width, ref height);
            AssertEqual(0, width);
            AssertEqual(0, height);
        }

        public void TestDetermineSizeSimpleCase()
        {
            int width = 1000, height = 2000;
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.ReadSizeFromTemplate("<!-- size: 100, 200 --><html>{name}</html>", ref width, ref height);
            AssertEqual(100, width);
            AssertEqual(200, height);
        }

        public void TestDetermineSizeMixedCase()
        {
            int width = 1000, height = 2000;
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.ReadSizeFromTemplate("<!-- SiZe: 100, 200 --><html>{name}</html>", ref width, ref height);
            AssertEqual(100, width);
            AssertEqual(200, height);
        }

        public void TestDetermineSizeWithOddWhitespace()
        {
            int width = 1000, height = 2000;
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.ReadSizeFromTemplate("<!--     size:\n\n\t100,200\t--><html>{name}</html>", ref width, ref height);
            AssertEqual(100, width);
            AssertEqual(200, height);
        }

        public void TestDetermineSizeWithInvalidValue()
        {
            int width = 1000, height = 2000;
            HtmlImageCreator creator = new HtmlImageCreator();
            creator.ReadSizeFromTemplate("<!-- size: one hundred, 200 --><html>{name}</html>", ref width, ref height);
            AssertEqual(1000, width);
            AssertEqual(2000, height);
        }

        public PivotItem sampleItem1 { get; set; }

        public PivotItem sampleItem2 { get; set; }
    }
}
