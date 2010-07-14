//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.IO;

using Microsoft.LiveLabs.Anise.API;
using Microsoft.LiveLabs.Pauthor.Crawling;

namespace Microsoft.LiveLabs.Pauthor.Test.Crawling
{
    public class UriUtilityUnitTests : AniseUnitTest
    {
        public void TestExpandUriWithAbsoluteLocalPath()
        {
            AssertEqual(@"c:\Windows", UriUtility.ExpandUri(@"c:\Windows"));
            AssertEqual(@"c:\Windows", UriUtility.ExpandUri("file:///c:/Windows"));
        }

        public void TestExpandUriWithRelativeLocalPath()
        {
            AssertMatches(@".*\\Test\\PauthorTestRunner\\bin\\.*\\PauthorTestRunner.exe",
                UriUtility.ExpandUri("PauthorTestRunner.exe"));
        }

        public void TestExpandUriWithAbsoluteUri()
        {
            AssertEqual("http://pauthor.codeplex.com/", UriUtility.ExpandUri("http://pauthor.codeplex.com"));
            AssertEqual("http://pauthor.codeplex.com/documentation",
                UriUtility.ExpandUri("http://pauthor.codeplex.com/documentation"));
        }

        public void TestExpandUriWithRelativeUri()
        {
            AssertEqual("foobar.html", UriUtility.ExpandUri("foobar.html"));
            AssertEqual("../../foobar.html", UriUtility.ExpandUri("../../foobar.html"));
            AssertEqual(@"..\..\foobar.html", UriUtility.ExpandUri(@"..\..\foobar.html"));
        }

        public void TestExpandRelativeUriWithTwoLocalPaths()
        {
            AssertEqual(Path.GetFullPath(@"..\..\Resources"),
                UriUtility.ExpandRelativeUri("PauthorTestRunner.exe", @"..\..\Resources"));
            AssertEqual(Path.GetFullPath(@"..\..\Resources\DeepZoom\sample_images"),
                UriUtility.ExpandRelativeUri(@"..\..\Resources\DeepZoom\sample.cxml", @"sample_images"));
            AssertEqual(Path.GetFullPath(@"..\..\Resources\Excel\sample.xlsx"),
                UriUtility.ExpandRelativeUri(@"..\..\Resources\DeepZoom", @"Excel\sample.xlsx"));
        }

        public void TestExpandRelativeUriWithAbsoluteAndRelativeLocalPaths()
        {
            String binDirectory = Path.GetFullPath(@"..\..\bin");

            AssertEqual(Path.GetFullPath(@"..\..\Resources\DeepZoom\sample.cxml"),
                UriUtility.ExpandRelativeUri(binDirectory, @"Resources\DeepZoom\sample.cxml"));
            AssertEqual(Path.GetFullPath(@"..\..\PauthorTestRunner.csproj"),
                UriUtility.ExpandRelativeUri(binDirectory, "PauthorTestRunner.csproj"));
        }

        public void TestExpandRelativeUriWithAbsoluteAndRelativeUris()
        {
            AssertEqual("http://pauthor.codeplex.com/license.html",
                UriUtility.ExpandRelativeUri("http://pauthor.codeplex.com/documentation", "license.html"));
            AssertEqual("http://pauthor.codeplex.com/documentation",
                UriUtility.ExpandRelativeUri("http://pauthor.codeplex.com/sample/foobar", "../documentation"));
        }

        public void TestCombineWithTwoRelativeLocalPaths()
        {
            AssertEqual(Path.GetFullPath(@"..\..\Resources\DeepZoom"),
                UriUtility.Combine(@"..\..\Resources", "DeepZoom"));
        }

        public void TestCombineWithAbsoluteAndRelativeLocalPaths()
        {
            String basePath = Path.GetFullPath(@"..\..\Resources");
            AssertEqual(basePath + @"\DeepZoom\sample.cxml", UriUtility.Combine(basePath, "DeepZoom/sample.cxml"));
        }

        public void TestCombineWithTwoRelativeUris()
        {
            try
            {
                UriUtility.Combine("path1/path2", "foobar.html");
            }
            catch (UriFormatException e)
            {
                AssertEqual("Invalid URI: The format of the URI could not be determined.", e.Message);
            }
        }

        public void TestCombineWithAbsoluteAndRelativeUris()
        {
            AssertEqual("http://pauthor.codeplex.com/documentation",
                UriUtility.Combine("http://pauthor.codeplex.com", "documentation"));
            AssertEqual("http://pauthor.codeplex.com/documentation/sample.html",
                UriUtility.Combine("http://pauthor.codeplex.com/documentation/foobar", "../sample.html"));
        }

        public void TestGetFileNameWithLocalPath()
        {
            AssertEqual("PauthorTestRunner.exe", UriUtility.GetFileName(@"PauthorTestRunner.exe"));
            AssertEqual("sample.cxml", UriUtility.GetFileName(@"..\..\Resources\DeepZoom\sample.cxml"));
            AssertEqual("PauthorTestRunner.exe", UriUtility.GetFileName(Path.GetFullPath("PauthorTestRunner.exe")));
        }

        public void TestGetFileNameWithUri()
        {
            AssertEqual("documentation", UriUtility.GetFileName("http://pauthor.codeplex.com/documentation"));
            AssertEqual("documentation", UriUtility.GetFileName("sample/foo/documentation"));
            AssertNull(UriUtility.GetFileName("http://pauthor.codeplex.com"));
        }
    }
}
