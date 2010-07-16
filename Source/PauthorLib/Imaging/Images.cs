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

        public static double Similarity(String imagePathA, String imagePathB, double accuracy)
        {
            Bitmap bitmapA = new Bitmap(imagePathA);
            Bitmap bitmapB = new Bitmap(imagePathB);

            return 0.0;
        }

        public static void GetTestCoordinate(int index, out double xPercent, out double yPercent)
        {
            System.Console.WriteLine("Log4(" + index + ") == " + Math.Log(index, 4));
            double originX = 0, originY = 0;
            double widthPercent = 1.0, heightPercent = 1.0;

            if (index > 0)
            {
                int level = (int) Math.Floor(Math.Log(index, 4));
            }

            //int level = (int) Math.Floor(Math.Log(index, 4));
            //int indexDrop = 1;

            //while (true)
            //{
            //    index -= indexDrop;
            //    if (index < 0) break;

            //    indexDrop = indexDrop * 4;
            //    int localIndex = index % 4;

            //    if (localIndex == 0)
            //    {
            //        // Do nothing. The origin is the same.
            //    }
            //    else if (localIndex == 1)
            //    {
            //        originX += widthPercent / 2.0;
            //    }
            //    else if (localIndex == 2)
            //    {
            //        originY += heightPercent / 2.0;
            //    }
            //    else if (localIndex == 3)
            //    {
            //        originX += widthPercent / 2.0;
            //        originY += heightPercent / 2.0;
            //    }
            //    widthPercent *= 0.5;
            //    heightPercent *= 0.5;
            //}

            xPercent = originX + (0.5 * widthPercent);
            yPercent = originY + (0.5 * heightPercent);
            System.Console.WriteLine(index + " => " + xPercent + ", " + yPercent);
        }
    }
}
