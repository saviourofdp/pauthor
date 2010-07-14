//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;

using Microsoft.LiveLabs.Anise.API;

namespace Microsoft.LiveLabs.Pauthor.Test
{
    public class PauthorTestRunner
    {
        public static void Main(String[] args)
        {
            AniseProgram program = new AniseProgram();
            program.LoadEmbeddedResource(typeof(PauthorTestRunner), "AniseConfig.adi");
            program.AniseEngine.RunAllTests(System.Console.Out);

            System.Console.Out.WriteLine("\nPress enter to finish");
            System.Console.In.ReadLine();
        }
    }
}
