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
using System.Text;

using Microsoft.LiveLabs.Pauthor.Imaging;

namespace Microsoft.LiveLabs.Pauthor.Test.Imaging
{
    public class ImagesModuleTest : PauthorUnitTest
    {
        private const double LowTolerance = 0.001;

        private const double HighTolerance = 0.01;

        private const double ColorTolerance = 0.10;

        public void TestSimpleIdenticalImagesLowAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");

            double similarity = Images.Similarity(imageA, imageA, 0.01, ColorTolerance);
            AssertWithinTolerance(1.0, LowTolerance, similarity);
        }

        public void TestSimpleIdenticalImagesHighAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");

            double similarity = Images.Similarity(imageA, imageA, 0.75, ColorTolerance);
            AssertWithinTolerance(1.0, 0.0001, similarity);
        }

        public void TestSameSimpleImageDifferentFormatLowAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\Blue.jpg");

            double similarity = Images.Similarity(imageA, imageB, 0.01, ColorTolerance);
            AssertWithinTolerance(1.0, LowTolerance, similarity);
        }

        public void TestSameSimpleImageDifferentFormatHighAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\Blue.jpg");

            double similarity = Images.Similarity(imageA, imageB, 0.75, ColorTolerance);
            AssertWithinTolerance(1.0, LowTolerance, similarity);
        }

        public void TestSameComplexImageLowAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\0.png");

            double similarity = Images.Similarity(imageA, imageA, 0.01, ColorTolerance);
            AssertWithinTolerance(1.0, LowTolerance, similarity);
        }

        public void TestSameComplexImageHighAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\0.png");

            double similarity = Images.Similarity(imageA, imageA, 0.75, ColorTolerance);
            AssertWithinTolerance(1.0, LowTolerance, similarity);
        }

        public void TestSameComplexImageDifferentFormatLowAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\0.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\0.jpg");

            double similarity = Images.Similarity(imageA, imageB, 0.01, ColorTolerance);
            AssertWithinTolerance(1.0, HighTolerance, similarity);
        }

        public void TestSameComplexImageDifferentFormatHighAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\0.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\0.jpg");

            double similarity = Images.Similarity(imageA, imageB, 0.75, ColorTolerance);
            AssertWithinTolerance(1.0, HighTolerance, similarity);
        }

        public void TestHalfSameSimpleImageLowAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\BlueWhiteColumns.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");

            double similarity = Images.Similarity(imageA, imageB, 0.01, ColorTolerance);
            AssertWithinTolerance(0.5, LowTolerance, similarity);
        }

        public void TestHalfSameSimpleImageHighAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\BlueWhiteColumns.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");

            double similarity = Images.Similarity(imageA, imageB, 0.75, ColorTolerance);
            AssertWithinTolerance(0.5, LowTolerance, similarity);
        }

        public void TestHalfSameComplexImageLowAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\0_small.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\0_smallWhiteRows.png");

            double similarity = Images.Similarity(imageA, imageB, 0.01, ColorTolerance);
            AssertWithinTolerance(0.5, LowTolerance, similarity);
        }

        public void TestHalfSameComplexImageHighAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\BlueWhiteColumns.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");

            double similarity = Images.Similarity(imageA, imageB, 0.75, ColorTolerance);
            AssertWithinTolerance(0.5, LowTolerance, similarity);
        }

        public void TestQuarterSameSimpleImageLowAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\BlueWhiteColumns.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\RedWhiteRows.png");

            double similarity = Images.Similarity(imageA, imageB, 0.01, ColorTolerance);
            AssertWithinTolerance(0.25, LowTolerance, similarity);
        }

        public void TestQuarterSameSimpleImageHighAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\BlueWhiteColumns.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\RedWhiteRows.png");

            double similarity = Images.Similarity(imageA, imageB, 0.75, ColorTolerance);
            AssertWithinTolerance(0.25, HighTolerance, similarity);
        }

        public void TestCompleteDifferentSimpleImageLowAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\Red.png");

            double similarity = Images.Similarity(imageA, imageB, 0.01, ColorTolerance);
            AssertWithinTolerance(0, LowTolerance, similarity);
        }

        public void TestCompletelyDifferentSimpleImageHighAccuracy()
        {
            String imageA = Path.Combine(this.ResourceDirectory, @"Images\Blue.png");
            String imageB = Path.Combine(this.ResourceDirectory, @"Images\Red.png");

            double similarity = Images.Similarity(imageA, imageB, 0.75, ColorTolerance);
            AssertWithinTolerance(0, LowTolerance, similarity);
        }
    }
}
