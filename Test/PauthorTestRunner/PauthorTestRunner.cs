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
            if (args.Length == 0)
            {
                program.AniseEngine.RunAllTests(System.Console.Out);
            }
            else
            {
                foreach (String test in args)
                {
                    program.AniseEngine.RunTest(System.Console.Out, test);
                }
            }

            System.Console.Out.WriteLine("\nPress enter to finish");
            System.Console.In.ReadLine();
        }
    }
}
