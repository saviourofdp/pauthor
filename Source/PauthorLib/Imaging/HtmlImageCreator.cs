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
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;

using Microsoft.LiveLabs.Pauthor.Core;

namespace Microsoft.LiveLabs.Pauthor.Imaging
{
    /// <summary>
    /// HtmlImageCreator creates bitmap images capturing a rendering of an HTML template.</summary>
    /// <remarks>The template is expected to contain various tags which are replaced with the appropriate values taken
    /// from a Pivot item. Once the template is "instantiated", it will be rendered using an in-memory IE7 engine. That
    /// rendered version will then be saved to an appropriate output stream.
    /// <para>The HTML template may be any valid HTML document which can be rendered in IE7. This can include CSS,
    /// JavaScript, or any other technology used to render web pages*. In order to determine the size at which the final
    /// image is drawn, each template may include an HTML comment specifying the size: <c>&lt;!-- size: <i>width</i>,
    /// <i>height</i> --&gt;</c>. If this comment is not included, then a default size will be used. In order to include
    /// item-specific data in the image, you can use the following tags in your template:</para>
    /// 
    /// <list type="table">
    /// <listheader><term>Syntax</term><description>Description</description></listheader>
    /// <item><term>{<i>facet</i>}</term>
    /// <description>inserts the first value of the named facet</description></item>
    /// <item><term>{<i>facet</i>:<i>n</i>}</term>
    /// <description>inserts the n-th value of the named facet</description></item>
    /// <item><term>{<i>facet</i>:join:<i>delimter</i>}</term>
    /// <description>inserts all of the named facet's values, delimited by the given string</description></item>
    /// </list>
    /// 
    /// <para><b>NOTE:</b> This class creates an instance of the IE7 rendering engine, and is therefore very memory
    /// intensive! Under low memory conditions, it is a known problem that images are sometimes not rendered as
    /// expected, so be sure to optimize memory use when using this class.</para>
    /// 
    /// <para>* bear in mind that the snapshot of the page is taken as soon as the document is finished rendering, so
    /// any animations or JavaScript which run after that time are likely not to be included.</para>
    /// </remarks>
    public class HtmlImageCreator : ImageCreator
    {
        /// <summary>
        /// The maximum width or height at which images may be rendered.
        /// </summary>
        public const int MaximumDimension = 5000;

        /// <summary>
        /// The default width at which images are rendered.
        /// </summary>
        public const int DefaultWidth = 1200;

        /// <summary>
        /// The default height at which images are rendered.
        /// </summary>
        public const int DefaultHeight = 1500;

        private static PauthorLog Log = PauthorLog.Global;

        /// <summary>
        /// Creates a new HTML image creator.
        /// </summary>
        public HtmlImageCreator()
        {
            m_lock = new Object();
            Thread workerThread = new Thread(new ThreadStart(RunWorkerThread));
            workerThread.IsBackground = true;
            workerThread.SetApartmentState(ApartmentState.STA);
            workerThread.Start();

            this.HtmlTemplate = "<html><body style=\"margin: 0px\">" +
                "<img src=\"{image}\" style=\"position:absolute;top:0;left:0;width:100%;height:100%\"/>" +
                "<p style=\"font-size:100px;color:white;background:grey;position:absolute;bottom:0;left:0;width:100%;" +
                "filter:alpha(opacity=50);padding:20px\">{name}</p></body></html>";
            this.Height = DefaultHeight;
            this.Width = DefaultWidth;
        }

        /// <summary>
        /// The HTML template used to create images.
        /// </summary>
        /// <remarks>
        /// This property may not be empty or null. When changing this property, the current values of
        /// <see cref="Height"/> and <see cref="Width"/> will be overridden if the new template provides an appropriate
        /// "size" comment.  The <see cref="HtmlTemplatePath"/> will also be set to null. By default, this property is
        /// set to a simple template which displays the item's current image overlaid with the item's title along the
        /// bottom edge.
        /// </remarks>
        public String HtmlTemplate
        {
            get { return m_htmlTemplate; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("HtmlTemplate cannot be null");
                m_htmlTemplate = value;
                m_htmlTemplatePath = null;

                this.ReadSizeFromTemplate(value, ref m_width, ref m_height);
            }
        }

        /// <summary>
        /// The path to an HTML file which should be used as the template for this image creator.
        /// </summary>
        /// <remarks>
        /// When setting this property, the specified file will be loaded, and the <see cref="HtmlTemplate"/> property
        /// set to its contents.
        /// </remarks>
        public String HtmlTemplatePath
        {
            get { return m_htmlTemplatePath; }

            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException("HtmlTemplatePath cannot be null");
                if (File.Exists(value) == false)
                {
                    throw new ArgumentException("HtmlTemplatePath does not exist: " + value);
                }

                this.HtmlTemplate = File.ReadAllText(value);
                m_htmlTemplatePath = value;
            }
        }

        /// <summary>
        /// The height (in pixels) at which to render images.
        /// </summary>
        /// <remarks>
        /// By default, this is set to 1500 pixels.
        /// </remarks>
        public int Height
        {
            get { return m_height; }

            set
            {
                if (value < 1) throw new ArgumentException("Height must be at least 1: " + value);
                if (value > MaximumDimension)
                {
                    throw new ArgumentException("Height must be less than " + MaximumDimension + ": " + value);
                }
                m_height = value;
            }
        }

        /// <summary>
        /// The width (in pixels) at which to render images.
        /// </summary>
        /// <remarks>
        /// By default, this is set to 1200 pixels.
        /// </remarks>
        public int Width
        {
            get { return m_width; }

            set
            {
                if (value < 1) throw new ArgumentException("Width must be at least 1: " + value);
                if (value > MaximumDimension)
                {
                    throw new ArgumentException("Width must be less than " + MaximumDimension + ": " + value);
                }
                m_width = value;
            }
        }

        /// <summary>
        /// Creates an image and assigns it to the given item.
        /// </summary>
        /// <remarks>
        /// If the item already had an image, it is replaced with the new image. The new image refers to a newly create
        /// image file stored in this image creator's working directory. If the <see
        /// cref="ImageCreator.ShouldDeleteWorkingDirectory"/> property is true, then the image will also be deleted as
        /// soon as this image creator is disposed, so it will be necessary to ensure the image is copied to a new
        /// directory (and the item updated) before that happens.
        /// </remarks>
        /// <param name="item">the item</param>
        public void AssignImage(PivotItem item)
        {
            String tempFile = Path.Combine(this.WorkingDirectory, item.Id + StandardImageFormatExtension);
            if (File.Exists(tempFile) == false)
            {
                using (FileStream fileStream = new FileStream(tempFile, FileMode.CreateNew, FileAccess.Write))
                {
                    if (this.CreateImage(item, fileStream))
                    {
                        item.Image = new PivotImage(tempFile);
                    }
                    else
                    {
                        Log.Warning("Could not create an image for {0} in within {1}ms", item.Name, RenderTimeout);
                    }
                }
            }
            else
            {
                item.Image = new PivotImage(tempFile);
            }
        }

        /// <summary>
        /// Creates a new image based upon a given item, and writes it to the provided output stream.
        /// </summary>
        /// <param name="item">the item upon which to base the image</param>
        /// <param name="outputStream">the stream to which the bitmap should be written</param>
        /// <returns>true if the image was successfully written</returns>
        public bool CreateImage(PivotItem item, Stream outputStream)
        {
            if (m_outputStream != null) throw new InvalidOperationException("Already creating an image");
            m_outputStream = outputStream;
            m_item = item;

            String html = this.InstantiateTemplate(item);
            String instanceDirectory = this.WorkingDirectory;
            if (this.HtmlTemplatePath != null)
            {
                instanceDirectory = Directory.GetParent(this.HtmlTemplatePath).FullName;
            }
            else
            {
                instanceDirectory = Path.GetTempPath();
            }
            String instanceFile = Path.Combine(instanceDirectory, Guid.NewGuid().ToString() + ".html");
            File.WriteAllText(instanceFile, html);

            bool createdImage = false;
            try
            {
                lock (m_lock)
                {
                    m_dispatcher.BeginInvoke((Action)delegate()
                    {
                        if (m_webBrowser != null)
                        {
                            m_webBrowser.Dispose();
                        }

                        m_webBrowser = new WebBrowser();
                        m_webBrowser.DocumentCompleted +=
                            new WebBrowserDocumentCompletedEventHandler(OnDocumentCompleted);
                        m_webBrowser.ScriptErrorsSuppressed = true;
                        m_webBrowser.ScrollBarsEnabled = false;

                        m_webBrowser.Height = m_height;
                        m_webBrowser.Width = m_width;
                        m_webBrowser.Url = new Uri(instanceFile);
                    });
                    createdImage = Monitor.Wait(m_lock, RenderTimeout);
                }
            }
            finally
            {
                File.Delete(instanceFile);
                m_outputStream = null;
                m_item = null;
            }

            return createdImage;
        }

        /// <summary>
        /// Disposes the various resources used to create images.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (m_dispatcher != null)
            {
                if (m_webBrowser != null)
                {
                    m_dispatcher.Invoke((Action)delegate()
                    {
                        m_webBrowser.Dispose();
                        m_webBrowser = null;
                    });
                }
                m_dispatcher.InvokeShutdown();
                m_dispatcher = null;
            }
        }

        /// <summary>
        /// Parses the text of an HTML template and returns the size specified (if any).
        /// </summary>
        /// <remarks>
        /// If a size comment was found, but the specified size could not be parsed, then warnings will be written to
        /// the Pauthor global log.
        /// </remarks>
        /// <param name="template">the HTML template to parse</param>
        /// <param name="width">updated with the specified width, if one was provided</param>
        /// <param name="height">updated with the specified height, if one was provided</param>
        public void ReadSizeFromTemplate(String template, ref int width, ref int height)
        {
            Match match = TemplateSizeRegex.Match(template);
            if (match.Success == false) return;

            if (Int32.TryParse(match.Groups[1].Value, out width) == false)
            {
                Log.Warning("HTML Template had an unexpected width: " + match.Groups[1].Value);
            }


            if (Int32.TryParse(match.Groups[2].Value, out height) == false)
            {
                Log.Warning("HTML Template had an unexpected height: " + match.Groups[2].Value);
            }
        }

        /// <summary>
        /// Creates a copy of this image creator's <see cref="HtmlTemplate"/> by replacing all the tags with appropriate
        /// values based upon a given image.
        /// </summary>
        /// <param name="item">the item containing the specific data to use</param>
        /// <returns>an HTML document with the given item's data</returns>
        public String InstantiateTemplate(PivotItem item)
        {
            String text = m_htmlTemplate;
            text = this.Replace(text, "{id}", item.Id);
            text = this.Replace(text, "{name}", item.Name ?? "");
            if (item.Image != null)
            {
                text = this.Replace(text, "{image}", item.Image.SourcePath ?? "");
            }
            text = this.Replace(text, "{href}", item.Href ?? "");
            text = this.Replace(text, "{description}", item.Description ?? "");

            foreach (String facetCategoryName in item.FacetCategories)
            {
                String replaceStringBase = "{" + facetCategoryName;

                String textValue = item.GetFacetValueAsString(facetCategoryName);
                text = this.Replace(text, replaceStringBase + "}", textValue);

                text = this.PerformJoinInstantiation(facetCategoryName, item, text);

                List<IComparable> values = new List<IComparable>(item.GetAllFacetValues(facetCategoryName));
                int index = 0;
                while (true)
                {
                    String replaceString = replaceStringBase + ":" + index + "}";
                    if (text.IndexOf(replaceString, StringComparison.InvariantCultureIgnoreCase) == -1) break;

                    textValue = "";
                    if (index < values.Count)
                    {
                        PivotFacetCategory facetCategory = item.CollectionDefinition.FacetCategories[facetCategoryName];
                        if (facetCategory.Format != null)
                        {
                            textValue = String.Format("{0:" + facetCategory.Format + "}", values[index]);
                        }
                        else
                        {
                            textValue = facetCategory.Type.FormatValue(values[index]);
                        }
                    }
                    text = this.Replace(text, replaceString, textValue);
                    index++;
                }
            }
            return text;
        }

        private String Replace(String source, String findText, String replaceText)
        {
            String result = source;
            while (true)
            {
                int index = result.IndexOf(findText, StringComparison.InvariantCultureIgnoreCase);
                if (index == -1) break;

                String foundText = result.Substring(index, findText.Length);
                result = result.Replace(foundText, replaceText);
            }
            return result;
        }

        private String PerformJoinInstantiation(String facetCategoryName, PivotItem item, String text)
        {
            String replaceStringBase = "{" + facetCategoryName + ":join:";
            int baseIndex = text.IndexOf(replaceStringBase, StringComparison.InvariantCultureIgnoreCase);
            if (baseIndex == -1) return text;

            int endIndex = text.IndexOf("}", baseIndex);
            int delimiterStartIndex = baseIndex + replaceStringBase.Length;
            String delimiter = text.Substring(delimiterStartIndex, endIndex - delimiterStartIndex);
            String replaceString = text.Substring(baseIndex, endIndex - baseIndex + 1);

            text = this.Replace(text, replaceString, item.GetAllFacetValuesAsString(facetCategoryName, delimiter));
            return text;
        }

        private void RunWorkerThread()
        {
            try
            {
                m_dispatcher = Dispatcher.CurrentDispatcher;
                Dispatcher.Run();
            }
            catch (Exception e)
            {
                Log.Error("Caught exception in rendering thread: {0}", e);
            }
        }

        private void OnDocumentCompleted(Object sender, WebBrowserDocumentCompletedEventArgs args)
        {
            try
            {
                Bitmap bitmap = new Bitmap(m_width, m_height);
                m_webBrowser.DrawToBitmap(bitmap, new Rectangle(
                    m_webBrowser.Location.X, m_webBrowser.Location.Y, m_webBrowser.Width, m_webBrowser.Height));

                ImageCodecInfo codecInfo = ImageCodecInfo.GetImageEncoders()[StandardImageEncoder];
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                bitmap.Save(m_outputStream, codecInfo, encoderParameters);
            }
            catch (Exception e)
            {
                Log.Error("Image rendering failed: {0}", e);
            }

            lock (m_lock) { Monitor.Pulse(m_lock); }
        }

        private static readonly Regex TemplateSizeRegex = new Regex(
            "<!--[\n\t ]*size:[\n\t ]*([0-9]+),[\n\t ]*([0-9]+)[\n\t ]*-->", RegexOptions.IgnoreCase);

        private const int RenderTimeout = 30000;

        private Object m_lock;

        private Dispatcher m_dispatcher;

        private WebBrowser m_webBrowser;

        private Stream m_outputStream;

        private PivotItem m_item;

        private String m_htmlTemplate;

        private String m_htmlTemplatePath;

        private int m_height;

        private int m_width;
    }
}
