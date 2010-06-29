//
// Pauthor - An authoring library for Pivot collections
// http://getpivot.com
//
// Copyright (c) 2010, by Microsoft Corporation
// All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.DeepZoomTools;
using Microsoft.LiveLabs.Pauthor;
using Microsoft.LiveLabs.Pauthor.Core;
using Microsoft.LiveLabs.Pauthor.Streaming;

namespace Microsoft.LiveLabs.Pauthor.Streaming.Filters
{
    /// <summary>
    /// DeepZoomTargetFilter alters a collection stream being written through it to include DeepZoom artifacts instead
    /// of raw images.
    /// </summary>
    /// <remarks>
    /// The DeepZoom artifacts are written to the file system adjcent to the base path of the underlying target. This
    /// target filter will create a DeepZoom collection (DZC) for the collection as a whole, and change the collection
    /// source's original <see cref="PivotCollection.ImageBase"/> to refer to the DZC file. This target filter will also
    /// alter the <see cref="PivotItem.Image"/> for each item to refer to the appropriate index in the DZC file.
    /// <para><b>NOTE:</b> This class can be <i>extremely</i> memory intensive for several reasons. First, the memory
    /// usage when creating DeepZoom artifacts inherently scales with the number of images in the collection. Second,
    /// this class attempts to speed up the process of creating DZIs for each item by running several conversion
    /// processes in parallel. Each of these also uses quite a bit of memory. If the application starts to run low on
    /// memory, the class will automatically start shutting down these background threads. If you would like to
    /// proactively control this memory usage, set the <see cref="ThreadCount"/> property.  Naturally, higher numbers
    /// complete the conversion process faster, while lower numbers use less memory.</para>
    /// </remarks>
    public class DeepZoomTargetFilter : PivotCollectionTargetFilter, ILocalCollectionTarget
    {
        /// <summary>
        /// Creates a new DeepZoom target filter using a stub collection target.
        /// </summary>
        /// <remarks>
        /// This stub should be replaced before actually using the new filter.
        /// </remarks>
        public DeepZoomTargetFilter()
            : this(NullCollectionTarget.Instance)
        {
            // Do nothing.
        }

        /// <summary>
        /// Creates a new DeepZoom target filter using the given delegate collection target.
        /// </summary>
        /// <param name="target">the delegate target to which a collection source will be written after it has been
        /// transformed to using DeepZoom artifacts for images</param>
        /// <exception cref="ArgumentNullException">if given a null value</exception>
        public DeepZoomTargetFilter(IPivotCollectionTarget target)
            : base(target)
        {
            m_sourceFilter = new DeepZoomSourceFilter(NullCollectionSource.Instance, this.BasePath);
        }

        /// <summary>
        /// The number of background DZI conversion threads used by this filter.
        /// </summary>
        /// <remarks>
        /// Setting this to a higher value will cause DZI creation to go faster (limited by I/O throughput). Setting
        /// this to a lower value will use less memory. By default, this property is set to five threads per processor.
        /// </remarks>
        public int ThreadCount
        {
            get { return m_sourceFilter.ThreadCount; }

            set { m_sourceFilter.ThreadCount = value; }
        }

        /// <summary>
        /// The base path of the underlying delegate collection target.
        /// </summary>
        /// <remarks>
        /// Changing this property will write through to the underlying delegate.
        /// </remarks>
        public String BasePath
        {
            get { return this.GetLocalTarget().BasePath; }

            set { this.GetLocalTarget().BasePath = value; }
        }

        /// <summary>
        /// Transforms the given collection to use DeepZoom artifacts for its images, and then writes the resulting
        /// collection to the underlying collection target delegate.
        /// </summary>
        /// <remarks>
        /// All of the DeepZoom artifacts will be written into a directory named "foo_deepzoom" where "foo" is the file
        /// name (without extension) of the collection target delegate's base path.
        /// </remarks>
        /// <param name="source">the collection source to write</param>
        public override void Write(IPivotCollectionSource source)
        {
            m_sourceFilter.Source = source;
            this.Target.Write(m_sourceFilter);
        }

        private ILocalCollectionTarget GetLocalTarget()
        {
            IPivotCollectionTarget deepestTarget = this.Target;
            while (deepestTarget is PivotCollectionTargetFilter)
            {
                deepestTarget = ((PivotCollectionTargetFilter)deepestTarget).Target;
            }
            if ((deepestTarget is ILocalCollectionTarget) == false)
            {
                throw new ArgumentException("Target chain must end with a ILocalCollectionTarget");
            }
            return (ILocalCollectionTarget) deepestTarget;
        }

        private DeepZoomSourceFilter m_sourceFilter;
    }

    internal class DeepZoomSourceFilter : PivotCollectionSourceFilter
    {
        public DeepZoomSourceFilter(IPivotCollectionSource source, String targetBasePath)
            : base(source)
        {
            String basePath = Directory.GetParent(targetBasePath).FullName;
            String baseFileName = Path.GetFileNameWithoutExtension(targetBasePath);
            String deepZoomDirectoryName = String.Format(DeepZoomDirectoryTemplate, baseFileName);
            m_dzcRelativePath = Path.Combine(deepZoomDirectoryName,
                String.Format(DzcFileNameTemplate, baseFileName));
            m_dzcPath = Path.Combine(basePath, m_dzcRelativePath);

            m_imageCreator = new ParallelDeepZoomCreator();
            m_imageCreator.OutputDirectory = Path.Combine(basePath, deepZoomDirectoryName);
        }

        public override String ImageBase
        {
            get { return m_dzcRelativePath; }
        }

        public override IEnumerable<PivotItem> Items
        {
            get
            {
                m_imageCreator.Reset();
                m_imageCreator.Start();

                foreach (PivotItem item in this.Source.Items)
                {
                    if (item.Image != null)
                    {
                        int index = m_imageCreator.Submit(item.Image.SourcePath);
                        item.Image.SourcePath = "#" + index;
                    }

                    yield return item;
                }

                m_imageCreator.Join();
                CollectionCreator collectionCreator = new CollectionCreator();
                collectionCreator.Create(m_imageCreator.DziPaths, m_dzcPath);
            }
        }

        public int ThreadCount
        {
            get { return m_imageCreator.ThreadCount; }

            set { m_imageCreator.ThreadCount = value; }
        }

        private const String DeepZoomDirectoryTemplate = "{0}_deepzoom";

        private const String DzcFileNameTemplate = "{0}.dzc";

        private String m_dzcPath;

        private String m_dzcRelativePath;

        private ParallelDeepZoomCreator m_imageCreator;
    }

    internal class ParallelDeepZoomCreator
    {
        private static readonly PauthorLog Log = PauthorLog.Global;

        internal ParallelDeepZoomCreator()
        {
            m_outputDirectory = ".";
            m_threadCount = Environment.ProcessorCount * 5;
            m_threadPool = new List<Thread>();
            m_dziPaths = new List<String>();
            m_workQueue = new Queue<String[]>();
            m_itemsRemaining = new int[] { 0 };
        }

        internal String OutputDirectory
        {
            get { return m_outputDirectory; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("OutputDirectory cannot be null");
                if (Directory.Exists(value) == false)
                {
                    Directory.CreateDirectory(value);
                }
                m_outputDirectory = value;
            }
        }

        internal List<String> DziPaths
        {
            get { return m_dziPaths; }
        }

        internal int ThreadCount
        {
            get { return m_threadCount; }

            set
            {
                if (value < 1) throw new ArgumentException("ThreadCount must be at least 1: " + value);
                m_threadCount = value;
            }
        }

        internal int Submit(String sourceImagePath)
        {
            String dziTargetPath = Path.GetFileNameWithoutExtension(sourceImagePath);
            dziTargetPath = dziTargetPath.Replace(' ', '_');
            dziTargetPath = dziTargetPath.Replace('{', '_');
            dziTargetPath = dziTargetPath.Replace('}', '_');
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
                        m_workQueue.Enqueue(new String[] { sourceImagePath, dziTargetPath });
                        m_itemsRemaining[0]++;
                    }
                    Monitor.Pulse(m_workQueue);
                }
            }

            return m_dziPaths.Count - 1;
        }

        internal void Start()
        {
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

        internal void Join()
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
        }

        internal void Reset()
        {
            this.Join();
            m_threadPool.Clear();
            m_dziPaths.Clear();
            m_workQueue.Clear();
            m_stopRequested = false;
            m_itemsRemaining[0] = 0;
        }

        internal void OnThreadRun()
        {
            while (m_stopRequested == false)
            {
                String[] currentJob = null;
                try
                {
                    lock (m_workQueue)
                    {
                        if (m_workQueue.Count == 0)
                        {
                            Monitor.Wait(m_workQueue);
                            continue;
                        }
                        currentJob = m_workQueue.Dequeue();
                    }

                    ImageCreator imageCreator = new ImageCreator();
                    imageCreator.Create(currentJob[0], currentJob[1]);
                }
                catch (OutOfMemoryException)
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

                    if (shouldResign) break;
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

        private const String DziFileNameTemplate = "{0}.dzi";

        private String m_outputDirectory;

        private int m_threadCount;

        private List<Thread> m_threadPool;

        private List<String> m_dziPaths;

        private Queue<String[]> m_workQueue;

        private int[] m_itemsRemaining;

        private volatile bool m_stopRequested;
    }
}
