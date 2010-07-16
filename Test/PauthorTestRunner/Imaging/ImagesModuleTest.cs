//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.LiveLabs.Pauthor.Imaging;

namespace Microsoft.LiveLabs.Pauthor.Test.Imaging
{
    public class ImagesModuleTest : PauthorUnitTest
    {
        public void TestCoordinateGeneration()
        {
            double x = 0.0, y = 0.0;

            for (int i = 0; i < 25; i++)
            {
                Images.GetTestCoordinate(i, out x, out y);
            }

            System.Console.WriteLine("Done");
        }
    }
}
