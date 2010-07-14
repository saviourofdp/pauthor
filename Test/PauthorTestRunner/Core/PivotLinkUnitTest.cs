//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Test.Core
{
    public class PivotLinkUnitTest : PauthorUnitTest
    {
        public void TestParseCorrectValue()
        {
            PivotLink link = PivotLink.ParseValue("alpha||bravo");
            AssertEqual("alpha", link.Title);
            AssertEqual("bravo", link.Url);
        }

        public void TestParseMissingTitle()
        {
            PivotLink link = PivotLink.ParseValue("||bravo");
            AssertNull(link.Title);
            AssertEqual("bravo", link.Url);
        }

        public void TestParseMissingUrl()
        {
            PivotLink link = PivotLink.ParseValue("alpha||");
            AssertEqual("alpha", link.Title);
            AssertNull(link.Url);
        }

        public void TestParseMissingDelimiter()
        {
            PivotLink link = PivotLink.ParseValue("alpha bravo");
            AssertEqual("alpha bravo", link.Title);
            AssertNull(link.Url);
        }
    }
}
