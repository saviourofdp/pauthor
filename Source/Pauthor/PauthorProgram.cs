//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.LiveLabs.Anise.API;

using Microsoft.LiveLabs.Pauthor.Streaming;
using Microsoft.LiveLabs.Pauthor.Streaming.Filters;

namespace Microsoft.LiveLabs.Pauthor.CLI
{
    public class PauthorProgram : AniseEngineAware
    {
        public static void Main(String[] args)
        {
            try
            {
                AniseProgram program = new AniseProgram();
                program.LoadEmbeddedResource(typeof(PauthorProgram), "AniseConfig.adi");
                program.Run("pauthor-program", args);
            }
            catch (StopExecutionException e)
            {
                Environment.ExitCode = e.Success ? 0 : 1;
            }
            catch (Exception e)
            {
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                Log.Error("A problem occured while processing your collection: {0}", e);
            }
        }

        private static PauthorLog Log = PauthorLog.Global;

        public AniseEngine AniseEngine { get; set; }

        public String HelpText { get; set; }

        public void Run(List<List<String>> args)
        {
            try
            {
                try
                {
                    long start = DateTime.Now.Ticks;
                    this.ParseArgs(args);

                    if (m_currentSource == null)
                    {
                        this.Fail("No source was specified.");
                    }

                    if (m_currentTarget == null)
                    {
                        this.Fail("No target was specified.");
                    }

                    m_currentTarget.Write(m_currentSource);

                    long end = DateTime.Now.Ticks;
                    Log.Progress("Finished in {0:HH:mm:ss.fff}", new DateTime(end - start));
                    this.Exit(true);
                }
                catch (StopExecutionException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Log.Error("A problem occured while processing your collection: {0}", e);
                    this.Exit(false);
                }
            }
            catch (StopExecutionException e)
            {
                Environment.ExitCode = e.Success ? 0 : 1;
            }
        }

        public void ParseArgs(List<List<String>> args)
        {
            foreach (List<String> argGroup in args)
            {
                String argName = argGroup[0];

                if ((argName == "help") || (argName == "?"))
                {
                    this.PrintHelp();
                }
                else if (argName == "debug")
                {
                    if (Debugger.IsAttached == false) Debugger.Launch();
                }
                else if (argName == "source")
                {
                    if (argGroup.Count != 3) this.Fail("Expected 2 arguments for /source");

                    String name = argGroup[1] + "-source";
                    if (this.AniseEngine.ContainsObject(name) == false) this.Fail("Unknown source type: " + argGroup[1]);

                    m_currentSource = this.AniseEngine.GetObject<IPivotCollectionSource>(name);
                }
                else if (argName == "target")
                {
                    if (argGroup.Count != 3) this.Fail("Expected 2 arguments for /target");

                    String name = argGroup[1] + "-target";
                    if (this.AniseEngine.ContainsObject(name) == false) this.Fail("Unknown target type: " + argGroup[1]);

                    m_currentTarget = this.AniseEngine.GetObject<IPivotCollectionTarget>(name);
                }
                else
                {
                    String objectName = argName + "-filter";
                    if (this.AniseEngine.ContainsObject(objectName) == false)
                    {
                        Log.Error("Unsupported argument: " + argName);
                        this.Exit(false);
                    }

                    PivotCollectionSourceFilter filter =
                        this.AniseEngine.GetObject<PivotCollectionSourceFilter>(objectName);
                    filter.Source = m_currentSource;
                    m_currentSource = filter;
                }
            }
        }

        private void PrintHelp()
        {
            System.Console.Write(this.HelpText);

            throw new StopExecutionException(true);
        }

        private void Fail(String message)
        {
            System.Console.WriteLine();
            System.Console.WriteLine(message);
            System.Console.Write("Use /? for usage instructions");
            System.Console.WriteLine();

            throw new StopExecutionException(false);
        }

        private void Exit(bool success)
        {
            if (m_currentSource != null) m_currentSource.Dispose();
            if (m_currentTarget != null) m_currentTarget.Dispose();

            throw new StopExecutionException(success);
        }

        private IPivotCollectionSource m_currentSource;

        private IPivotCollectionTarget m_currentTarget;
    }

    internal class StopExecutionException : Exception
    {
        public bool Success { get; private set; }

        public StopExecutionException(bool success)
        {
            this.Success = success;
        }
    }
}
