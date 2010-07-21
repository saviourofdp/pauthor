//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Microsoft.LiveLabs.Pauthor.Imaging
{
    public static class Images
    {
        public static bool AreIdentical(String imagePathA, String imagePathB)
        {
            if (imagePathA == null) return false;
            if (imagePathB == null) return false;
            if (File.Exists(imagePathA) == false) return false;
            if (File.Exists(imagePathB) == false) return false;

            FileInfo fileInfoA = new FileInfo(imagePathA);
            FileInfo fileInfoB = new FileInfo(imagePathB);

            if (fileInfoA.Length != fileInfoB.Length) return false;

            using (FileStream fileStreamA = new FileStream(imagePathA, FileMode.Open))
            using (FileStream fileStreamB = new FileStream(imagePathB, FileMode.Open))
            using (BufferedStream streamA = new BufferedStream(fileStreamA))
            using (BufferedStream streamB = new BufferedStream(fileStreamB))
            {
                while (true)
                {
                    int byteA = streamA.ReadByte();
                    int byteB = streamB.ReadByte();
                    if (byteA != byteB) return false;
                    if (byteA == -1) break;
                }
            }
            return true;
        }

        public static double Similarity(String imagePathA, String imagePathB, double sample, double tolerance)
        {
            if (sample <= 0.0) throw new ArgumentException("Sample must be greather than 0: " + sample);
            if (sample > 1.0) throw new ArgumentException("Sample must be no greater than 1.0: " + sample);
            if (tolerance <= 0.0) throw new ArgumentException("Tolerance must be greather than 0: " + tolerance);
            if (tolerance > 1.0) throw new ArgumentException("Tolerance must be no greater than 1.0: " + tolerance);
            if (File.Exists(imagePathA) == false) throw new ArgumentException("Missing image: " + imagePathA);
            if (File.Exists(imagePathB) == false) throw new ArgumentException("Missing image: " + imagePathB);

            Bitmap bitmapA = new Bitmap(imagePathA);
            Bitmap bitmapB = new Bitmap(imagePathB);

            Bitmap output = new Bitmap(Math.Max(bitmapA.Width, bitmapB.Width), Math.Max(bitmapA.Height, bitmapB.Height));

            int maximumPixelCount = Math.Max(bitmapA.Height * bitmapA.Width, bitmapB.Height * bitmapB.Width);
            int desiredTestPixelCount = (int)(maximumPixelCount * sample);
            int testPixelCount = 0;
            for (int i = 0; i < LevelStartIndex.Length; i++)
            {
                testPixelCount = LevelStartIndex[i];
                if (testPixelCount > desiredTestPixelCount) break;
            }

            double colorTolerance = 255 * tolerance;
            double accuracyTotal = 0;
            for (int index = 0; index < testPixelCount; index++)
            {
                double xPercent, yPercent;
                GetTestCoordinate(index, out xPercent, out yPercent);

                Color pixelA = bitmapA.GetPixel((int) (bitmapA.Width * xPercent), (int) (bitmapA.Height * yPercent));
                Color pixelB = bitmapB.GetPixel((int) (bitmapB.Width * xPercent), (int) (bitmapB.Height * yPercent));

                bool redMatches = Math.Abs(pixelA.R - pixelB.R) < colorTolerance;
                bool greenMatches = Math.Abs(pixelA.G - pixelB.G) < colorTolerance;
                bool blueMatches = Math.Abs(pixelA.B - pixelB.B) < colorTolerance;
                bool pixelMatches = redMatches && greenMatches && blueMatches;
                accuracyTotal += pixelMatches ? 1 : 0;

                output.SetPixel((int) (output.Width * xPercent), (int) (output.Height * yPercent),
                    pixelMatches ? Color.Green : Color.Red);
            }

            String file = @"d:\codeplex\pauthor\test\pauthortestrunner\resources\images\output.png";
            if (File.Exists(file)) File.Delete(file);
            output.Save(file, ImageFormat.Png);

            return accuracyTotal / testPixelCount;
        }

        public static void GetTestCoordinate(int index, out double xPercent, out double yPercent)
        {
            double originX = 0, originY = 0;
            double widthPercent = 1.0, heightPercent = 1.0;

            index -= 1;
            if (index >= 0)
            {
                int finalLevel = 0;
                while (index > LevelStartIndex[finalLevel + 1])
                {
                    finalLevel++;
                }

                int levelSize = LevelStartIndex[finalLevel + 1] - LevelStartIndex[finalLevel];
                double levelPercent = ((double)index - LevelStartIndex[finalLevel]) / levelSize;

                for (int currentLevel = 0; currentLevel <= finalLevel; currentLevel++)
                {
                    int currentLevelSize = LevelStartIndex[currentLevel + 1] - LevelStartIndex[currentLevel];
                    int levelIndex = (int)(currentLevelSize * levelPercent) + LevelStartIndex[currentLevel];
                    int quadrant = levelIndex % 4;

                    if (quadrant == 0)
                    {
                        // Do nothing.
                    }
                    else if (quadrant == 1)
                    {
                        originX += widthPercent / 2.0;
                    }
                    else if (quadrant == 2)
                    {
                        originY += widthPercent / 2.0;
                    }
                    else if (quadrant == 3)
                    {
                        originX += widthPercent / 2.0;
                        originY += widthPercent / 2.0;
                    }
                    widthPercent *= 0.5;
                    heightPercent *= 0.5;
                }
            }

            xPercent = originX + (0.5 * widthPercent);
            yPercent = originY + (0.5 * heightPercent);
        }

        private static int[] LevelStartIndex;

        private const int MaxCachedLevels = 1000;

        static Images()
        {
            LevelStartIndex = new int[MaxCachedLevels];

            int index = 0;
            int nodesAtCurrentLevel = 4;
            for (int currentLevel = 0; currentLevel < MaxCachedLevels; currentLevel++)
            {
                LevelStartIndex[currentLevel] = index;
                index += nodesAtCurrentLevel;
                nodesAtCurrentLevel *= 4;
            }
        }
    }
}
