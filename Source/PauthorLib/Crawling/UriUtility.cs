//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.IO;
using System.Net;

namespace Microsoft.LiveLabs.Pauthor.Crawling
{
    public static class UriUtility
    {
        public static String ExpandUri(String path)
        {
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);

            if (uri.IsAbsoluteUri)
            {
                if (uri.IsFile) return uri.LocalPath;
                return uri.AbsoluteUri;
            }

            if (File.Exists(path) || Directory.Exists(path)) return Path.GetFullPath(path);

            return path;
        }

        public static String ExpandRelativeUri(String basePath, String relativePath)
        {
            Uri baseUri = new Uri(UriUtility.ExpandUri(basePath), UriKind.Absolute);
            Uri resultUri = new Uri(baseUri, relativePath);
            return UriUtility.ExpandUri(resultUri.ToString());
        }

        public static String Combine(String basePath, String relativePath)
        {
            Uri baseUri = new Uri(UriUtility.ExpandUri(basePath), UriKind.Absolute);
            Uri relativeUri = new Uri(relativePath, UriKind.RelativeOrAbsolute);

            String result = baseUri.IsAbsoluteUri ? baseUri.GetLeftPart(UriPartial.Query) : baseUri.ToString();
            result += result.EndsWith("/") ? "" : "/";
            result += relativeUri.IsAbsoluteUri ? relativeUri.PathAndQuery : relativeUri.ToString();

            return UriUtility.ExpandUri(result);
        }

        public static bool IsLocalFile(String path)
        {
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri) return uri.IsFile;
            if (File.Exists(path) || Directory.Exists(path)) return true;
            return false;
        }

        public static String GetFileName(String path)
        {
            if (UriUtility.IsLocalFile(path))
            {
                return Path.GetFileName(path);
            }
            else
            {
                Uri uri = new Uri(UriUtility.ExpandUri(path), UriKind.RelativeOrAbsolute);
                if (uri.IsAbsoluteUri == false) return Path.GetFileName(path);
                if (uri.Segments.Length <= 1) return null;

                return uri.Segments[uri.Segments.Length - 1];
            }
        }

        public static String DownloadString(WebClient webClient, String path)
        {
            if (UriUtility.IsLocalFile(path))
            {
                return File.ReadAllText(path);
            }
            else
            {
                return webClient.DownloadString(path);
            }
        }
    }
}
