//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Test.Core
{
    public class PivotFacetTypeUnitTest : PauthorUnitTest
    {
        public void TestIdentifyString()
        {
            AssertEqual(PivotFacetType.String, PivotFacetType.IdentifyType("alpha"));
        }

        public void TestIdentifyLongString()
        {
            AssertEqual(PivotFacetType.LongString,
                PivotFacetType.IdentifyType("alpha bravo charlie delta echo"));
        }

        public void TestIdentifyNumber()
        {
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((byte)1));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((short)2));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((int)3));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((long)4));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((ushort)5));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((uint)6));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((ulong)7));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((float)8.8));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((double)9.9));
            AssertEqual(PivotFacetType.Number, PivotFacetType.IdentifyType((decimal)10.10));
        }

        public void TestIdentifyDateTime()
        {
            AssertEqual(PivotFacetType.DateTime, PivotFacetType.IdentifyType(new DateTime(1776, 07, 04)));
        }

        public void TestIdentifyLink()
        {
            AssertEqual(PivotFacetType.Link, PivotFacetType.IdentifyType(new PivotLink("alpha", "bravo")));
        }

        public void TestFormatString()
        {
            AssertEqual("alpha, bravo", PivotFacetType.String.FormatValue("alpha, bravo"));
        }

        public void TestFormatLongString()
        {
            AssertEqual("alpha: bravo, charlie, delta, echo",
                PivotFacetType.String.FormatValue("alpha: bravo, charlie, delta, echo"));
        }

        public void TestFormatNumber()
        {
            AssertEqual("1", PivotFacetType.Number.FormatValue((byte)1));
            AssertEqual("2", PivotFacetType.Number.FormatValue((short)2));
            AssertEqual("3", PivotFacetType.Number.FormatValue((int)3));
            AssertEqual("4", PivotFacetType.Number.FormatValue((long)4));
            AssertEqual("5", PivotFacetType.Number.FormatValue((ushort)5));
            AssertEqual("6", PivotFacetType.Number.FormatValue((uint)6));
            AssertEqual("7", PivotFacetType.Number.FormatValue((ulong)7));
            AssertEqual("8.8", PivotFacetType.Number.FormatValue((float)8.8));
            AssertEqual("9.9", PivotFacetType.Number.FormatValue((double)9.9));
            AssertEqual("10.1", PivotFacetType.Number.FormatValue((decimal)10.10));
        }

        public void TestFormatDateTime()
        {
            AssertEqual("1776-07-04T00:00:00",
                PivotFacetType.DateTime.FormatValue(new DateTime(1776, 07, 04)));
        }

        public void TestFormatLink()
        {
            AssertEqual("alpha||bravo", PivotFacetType.Link.FormatValue(new PivotLink("alpha", "bravo")));
        }

        public void TestParseString()
        {
            AssertEqual("alpha bravo", PivotFacetType.String.ParseValue("alpha bravo"));
        }

        public void TestParseLongString()
        {
            AssertEqual("alpha bravo charlie delta echo",
                PivotFacetType.LongString.ParseValue("alpha bravo charlie delta echo"));
        }

        public void TestParseNumber()
        {
            AssertEqual((long)1, (long)PivotFacetType.Number.ParseValue("1"));
            AssertEqual(2.5, PivotFacetType.Number.ParseValue("2.5"));
        }

        public void TestParseInvalidNumber()
        {
            try
            {
                PivotFacetType.Number.ParseValue("alpha");
                Fail("Expected exception");
            }
            catch (FormatException e)
            {
                AssertEqual("Input string was not in a correct format.", e.Message);
            }
        }

        public void TestParseDateTime()
        {
            AssertEqual(new DateTime(1776, 07, 04), PivotFacetType.DateTime.ParseValue("1776-07-04T00:00:00"));
        }

        public void TestParseInvalidDateTime()
        {
            try
            {
                PivotFacetType.DateTime.ParseValue("alpha");
            }
            catch (FormatException e)
            {
                AssertEqual("The string was not recognized as a valid DateTime. There is a unknown word starting at " +
                    "index 0.", e.Message);
            }
        }
    }
}
