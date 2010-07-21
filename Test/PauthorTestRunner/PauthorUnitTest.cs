//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using Microsoft.LiveLabs.Anise.API;
using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Test
{
    public class PauthorUnitTest : AniseUnitTest
    {
        public const string NamespaceRemoteCollection = "http://schemas.microsoft.com/livelabs/pivot/collection/2009";

        public const string NamespaceImageCollection = "http://schemas.microsoft.com/collection/metadata/2009";

        public const string SchemaPathRemoteCollection = @"..\..\Resources\PivotRemoteCollection.xsd";

        public const string SchemaPathImageCollection = @"..\..\Resources\PivotImageCollectionMetadata.xsd";

        public const string WorkingDirectory = "working-directory";

        public String ResourceDirectory
        {
            get { return @"..\..\Resources"; }
        }

        public override void Setup()
        {
            base.Setup();

            m_log = new StringWriter();
            PauthorLog.Global.ConsoleStream = m_log;
            PauthorLog.Global.CurrentLevel = PauthorLog.Level.All;

            if (Directory.Exists(WorkingDirectory))
            {
                Directory.Delete(WorkingDirectory, true);
            }
            Directory.CreateDirectory(WorkingDirectory);
        }

        public override void TearDown()
        {
            base.TearDown();

            if (Directory.Exists(WorkingDirectory) == false)
            {
                Directory.Delete(WorkingDirectory, true);
            }

            this.Write(m_log.ToString());
            m_log = null;
        }

        /// <summary>
        /// Uses the XSD files from the Pivot sources to validate that a CXML file is valid.
        /// </summary>
        /// <param name="cxmlPath">Path to the CXML file</param>
        /// <returns>Validity of CXML</returns>
        public void AssertCxmlSchemaValid(string cxmlPath)
        {
            String message = "No error";

            XmlReader reader = null;
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();

                settings.ValidationType = ValidationType.Schema;
                settings.Schemas.Add(NamespaceRemoteCollection, SchemaPathRemoteCollection);
                settings.Schemas.Add(NamespaceImageCollection, SchemaPathImageCollection);

                // This action should throw an exception if the schema
                // is invalid.
                reader = XmlReader.Create(cxmlPath, settings);

                // However, just to be sure, we'll read the first element
                reader.Read();
            }
            catch (Exception e)
            {
                message = e.Message;
            }
            finally
            {
                AssertEqual("No error", message);
                if (reader != null) reader.Close();
            }
        }

        public void AssertCollectionsEqual(IPivotCollectionSource expected, IPivotCollectionSource actual)
        {
            if (actual == null || expected == null)
            {
                Fail("Neither parameter to AssertCollectionsEqual can be null");
            }

            AssertEqual(expected.Name, actual.Name);
            AssertEqual(expected.AdditionalSearchText, actual.AdditionalSearchText);

            AssertPivotImagesEqual(expected.BrandImage, actual.BrandImage);
            AssertPivotImagesEqual(expected.Icon, actual.Icon);
            AssertLinksEqual(expected.Copyright, actual.Copyright);

            AssertFacetCategoriesEqual(expected.FacetCategories, actual.FacetCategories);

            PivotItem[] expectedItems = expected.Items.ToArray<PivotItem>();
            PivotItem[] actualItems = actual.Items.ToArray<PivotItem>();

            for (int i = 0; i < actualItems.Count(); i++)
            {
                AssertItemsEqual(expectedItems[i], actualItems[i]);
            }

            AssertEqual(expectedItems.Length, actualItems.Length);
        }

        public void AssertFacetCategoriesEqual(IReadOnlyPivotList<String, PivotFacetCategory> expected,
            IReadOnlyPivotList<String, PivotFacetCategory> actual)
        {
            for (int i = 0; i < expected.Count; i++)
            {
                AssertEqual(expected[i].Name, actual[i].Name);
                AssertEqual(expected[i].Format, actual[i].Format);
                AssertEqual(expected[i].IsFilterVisible, actual[i].IsFilterVisible);
                AssertEqual(expected[i].IsMetaDataVisible, actual[i].IsMetaDataVisible);
                AssertEqual(expected[i].IsWordWheelVisible, actual[i].IsWordWheelVisible);
                AssertFacetSortOrdersEqual(expected[i].SortOrder, actual[i].SortOrder);
                AssertFacetTypesEqual(expected[i].Type, actual[i].Type);
            }

            AssertEqual(expected.Count, actual.Count);
        }

        public void AssertFacetSortOrdersEqual(PivotFacetSortOrder expected, PivotFacetSortOrder actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            AssertEqual(expected.Name, actual.Name);
            AssertEqual(String.Join(",", expected.Values.ToArray()), String.Join(",", actual.Values.ToArray()));
        }


        public void AssertFacetTypesEqual(PivotFacetType expected, PivotFacetType actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            AssertEqual(expected.GetType(), actual.GetType());
        }


        public void AssertItemsEqual(PivotItem expected, PivotItem actual)
        {
            AssertEqual(expected.Description, actual.Description);
            AssertEqual(expected.HasRelatedLinks, actual.HasRelatedLinks);
            AssertEqual(expected.Href, actual.Href);
            AssertEqual(expected.Name, actual.Name);

            AssertPivotImagesEqual(expected.Image, actual.Image);
            AssertAllLinksEqual(expected.RelatedLinks, actual.RelatedLinks);
            AssertEqual(String.Join(",", expected.FacetCategories.ToArray()),
                String.Join(",", actual.FacetCategories.ToArray()));

            foreach (string category in expected.FacetCategories)
            {
                AssertEqual(expected.GetAllFacetValuesAsString(category, Environment.NewLine),
                    actual.GetAllFacetValuesAsString(category, Environment.NewLine));
            }
        }


        public void AssertLinksEqual(PivotLink expected, PivotLink actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }
            else if (expected != null && actual != null)
            {
                AssertEqual(expected.Title, actual.Title);
                AssertEqual(expected.Url, actual.Url);
            }
            else
            {
                Fail("Link comparison failed: one is null the other isn't");
            }
        }

        public void AssertAllLinksEqual(IEnumerable<PivotLink> expectedLinks, IEnumerable<PivotLink> actualLinks)
        {
            var ordered1 = expectedLinks.ToArray();
            var ordered2 = actualLinks.ToArray();

            for (int i = 0; i < ordered1.Count(); i++)
            {
                AssertLinksEqual(ordered1[i], ordered2[i]);
            }

            AssertEqual(expectedLinks.Count(), actualLinks.Count());
        }


        public void AssertPivotImagesEqual(PivotImage expected, PivotImage actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            if (expected == null || actual == null)
            {
                Fail("One of the image paths is null, but the other isn't");
            }

            if (actual.IsLocalFile && expected.IsLocalFile)
            {
                FileInfo expectedFile = new FileInfo(expected.SourcePath);
                FileInfo actualFile = new FileInfo(actual.SourcePath);

                double difference = Math.Abs(expectedFile.Length - actualFile.Length);
                double differencePercent = difference / expectedFile.Length;
                AssertTrue(difference < expectedFile.Length * 0.01, String.Format("Images ({0} vs {1}) are " +
                    "substantially different in length ({2:0.00}%)", expected.SourcePath, actual.SourcePath,
                    differencePercent * 100));
            }
        }

        public void AssertFileExists(String pathInWorkingDirectory)
        {
            String fullPath = Path.Combine(WorkingDirectory, pathInWorkingDirectory);
            if (File.Exists(fullPath)) return;
            if (Directory.Exists(fullPath)) return;
            Fail("Could not find \"" + pathInWorkingDirectory + "\" in working directory");
        }

        public void AssertWithinTolerance(double expected, double tolerance, double actual)
        {
            AssertTrue(Math.Abs(expected - actual) <= tolerance,
                "Expected " + expected + " (+/- " + tolerance +") but found " + actual);
        }

        private StringWriter m_log;
    }
}
