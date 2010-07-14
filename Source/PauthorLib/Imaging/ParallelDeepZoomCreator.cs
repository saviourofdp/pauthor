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
using System.Threading;
using Microsoft.LiveLabs.Pauthor.Core;
using DZ = Microsoft.DeepZoomTools;

namespace Microsoft.LiveLabs.Pauthor.Imaging
{
    public class ParallelDeepZoomCreator
    {
        private static readonly PauthorLog Log = PauthorLog.Global;

        public ParallelDeepZoomCreator(String dzcPath)
        {
            m_threadCount = Environment.ProcessorCount * 5;
            m_threadPool = new List<Thread>();
            m_dziPaths = new List<String>();
            m_workQueue = new Queue<Object[]>();
            m_itemsRemaining = new int[] { 0 };

            this.DzcPath = dzcPath;
        }

        public String DzcPath
        {
            get { return m_dzcPath; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("OutputDirectory cannot be null");
                if (m_working) throw new InvalidOperationException("Cannot change DZC path while working");

                String outputDirectory = Directory.GetParent(value).ToString();
                if (Directory.Exists(outputDirectory) == false)
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                m_outputDirectory = outputDirectory;
                m_dzcPath = value;
            }
        }

        public int ThreadCount
        {
            get { return m_threadCount; }

            set
            {
                if (value < 1) throw new ArgumentException("ThreadCount must be at least 1: " + value);
                m_threadCount = value;
            }
        }

        public void Start()
        {
            m_working = true;
            lock (m_threadPool)
            {
                if (m_threadPool.Count > 0) return;

                m_stopRequested = false;
                while (m_threadPool.Count < m_threadCount)
                {
                    Thread newThread = new Thread(new ThreadStart(OnThreadRun));
                    newThread.Name = "ParallelDeepZoomCreator-" + m_threadPool.Count;
                    newThread.Start();

                    m_threadPool.Add(newThread);
                }
            }
        }

        public int Submit(PivotImage sourceImage)
        {
            String dziTargetPath = Path.GetFileNameWithoutExtension(sourceImage.SourcePath);
            dziTargetPath = dziTargetPath.Replace(' ', '_');
            dziTargetPath = dziTargetPath.Replace('{', '_');
            dziTargetPath = dziTargetPath.Replace('}', '_');
            dziTargetPath = dziTargetPath.Replace('[', '_');
            dziTargetPath = dziTargetPath.Replace(']', '_');
            dziTargetPath = String.Format(DziFileNameTemplate, dziTargetPath);
            dziTargetPath = Path.Combine(m_outputDirectory, dziTargetPath);

            int index = m_dziPaths.IndexOf(dziTargetPath);
            if (index != -1) return index;

            m_dziPaths.Add(dziTargetPath);

            if (File.Exists(dziTargetPath) == false)
            {
                lock (m_workQueue)
                {
                    lock (m_itemsRemaining)
                    {
                        m_workQueue.Enqueue(new Object[] { new PivotImage(sourceImage.SourcePath), dziTargetPath });
                        m_itemsRemaining[0]++;
                    }
                    Monitor.Pulse(m_workQueue);
                }
            }

            return m_dziPaths.Count - 1;
        }

        public void Join()
        {
            int lastReportedSize = int.MaxValue;
            while (true)
            {
                lock (m_itemsRemaining)
                {
                    if (m_itemsRemaining[0] == 0) break;

                    if ((lastReportedSize - m_itemsRemaining[0]) > 10)
                    {
                        Log.Progress("Finishing DeepZoom conversions ({0} remaining)", m_itemsRemaining[0]);
                        lastReportedSize = m_itemsRemaining[0];
                    }
                    Monitor.Wait(m_itemsRemaining);
                }
            }

            m_stopRequested = true;
            lock (m_workQueue) { Monitor.PulseAll(m_workQueue); }
            lock (m_threadPool)
            {
                lastReportedSize = int.MaxValue;
                while (m_threadPool.Count > 0)
                {
                    if ((lastReportedSize - m_threadPool.Count) > 10)
                    {
                        Log.Progress("Stopping threads ({0} remaining)", m_threadPool.Count);
                        lastReportedSize = m_threadPool.Count;
                    }
                    Monitor.Wait(m_threadPool);
                }
            }

            DZ.CollectionCreator collectionCreator = new DZ.CollectionCreator();
            collectionCreator.Create(m_dziPaths, m_dzcPath);

            m_working = false;
            m_threadPool.Clear();
            m_dziPaths.Clear();
            m_workQueue.Clear();
            m_stopRequested = false;
            m_itemsRemaining[0] = 0;
        }

        private void OnThreadRun()
        {
            while (m_stopRequested == false)
            {
                Object[] currentJob = null;
                try
                {
                    if (this.ExecuteNextJob(out currentJob) == false) continue;
                }
                catch (OutOfMemoryException)
                {
                    if (this.ShouldResign(currentJob)) break;
                }
                catch (Exception e)
                {
                    Log.Error("Unexpected exception while converting {0}: {1}", currentJob[0], e);
                }

                lock (m_itemsRemaining)
                {
                    m_itemsRemaining[0]--;
                    Monitor.Pulse(m_itemsRemaining);
                }
            }

            lock (m_threadPool)
            {
                m_threadPool.Remove(Thread.CurrentThread);
                Monitor.Pulse(m_threadPool);
            }
        }

        private bool ExecuteNextJob(out Object[] currentJob)
        {
            currentJob = null;

            lock (m_workQueue)
            {
                if (m_workQueue.Count == 0)
                {
                    Monitor.Wait(m_workQueue);
                    return false;
                }
                currentJob = m_workQueue.Dequeue();
            }

            PivotImage image = (PivotImage)currentJob[0];
            String targetDziPath = (String)currentJob[1];

            image.EnsureLocal();

            DZ.ImageCreator imageCreator = new DZ.ImageCreator();
            imageCreator.Create(image.SourcePath, targetDziPath);

            return true;
        }

        private bool ShouldResign(Object[] currentJob)
        {
            bool shouldResign = false;

            lock (m_threadPool)
            {
                if (m_threadPool.Count > 1)
                {
                    Log.Warning("Background thread resigning due to memory pressure. There are {0} " +
                        "threads remaining.", m_threadPool.Count - 1);
                    shouldResign = true;
                }
            }

            lock (m_workQueue)
            {
                m_workQueue.Enqueue(currentJob);
                Monitor.Pulse(m_workQueue);
            }

            return shouldResign;
        }

        private const String DziFileNameTemplate = "{0}.dzi";

        private String m_dzcPath;

        private String m_outputDirectory;

        private int m_threadCount;

        private List<Thread> m_threadPool;

        private List<String> m_dziPaths;

        private Queue<Object[]> m_workQueue;

        private int[] m_itemsRemaining;

        private volatile bool m_working;

        private volatile bool m_stopRequested;
    }
}
