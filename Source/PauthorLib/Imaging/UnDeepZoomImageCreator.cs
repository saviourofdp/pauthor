//
// Pauthor - An authoring library for Pivot collections
// http://pauthor.codeplex.com
//
// This source code is released under the Microsoft Code Sharing License.
// For full details, see: http://pauthor.codeplex.com/license
//

using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Xml.XPath;

using Microsoft.LiveLabs.Pauthor.Crawling;

namespace Microsoft.LiveLabs.Pauthor.Imaging
{
    /// <summary>
    /// UnDeepZoomImageCreator converts a DeepZoom image back into a plain bitmap image.
    /// </summary>
    /// <remarks>
    /// The end result contains the same image as was used to create the original DeepZoom image in a JPEG format at the
    /// highest quality.
    /// </remarks>
    public class UnDeepZoomImageCreator : ImageCreator
    {
        private static PauthorLog Log = PauthorLog.Global;

        /// <summary>
        /// Creates a new image creator.
        /// </summary>
        public UnDeepZoomImageCreator()
        {
            // Do nothing.
        }

        /// <summary>
        /// Converts a DeepZoom image back into an ordinary bitmap.
        /// </summary>
        /// <param name="dziPath">the path (relative or absolute) to the DZI file to convert</param>
        /// <returns>an absolute path to the resulting image within this image creator's working directory</returns>
        public String UnDeepZoomImage(String dziUri)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    String baseName = Path.GetFileNameWithoutExtension(UriUtility.GetFileName(dziUri));
                    String finalImagePath = Path.Combine(this.WorkingDirectory, baseName + StandardImageFormatExtension);
                    if (File.Exists(finalImagePath)) return finalImagePath;

                    String filesDirectoryUri = UriUtility.ExpandRelativeUri(dziUri, baseName + "_files");
                    String dziText = UriUtility.DownloadString(webClient, dziUri);

                    XPathHelper dzi = new XPathHelper(dziText);
                    String xmlns = this.DetermineNamespace(dziText);
                    if (xmlns != null)
                    {
                        dzi.AddNamespace("d", xmlns);
                    }

                    String format = dzi.FindString("//d:Image/@Format");
                    int tileSize = dzi.FindInt("//d:Image/@TileSize");
                    int overlap = dzi.FindInt("//d:Image/@Overlap");
                    int width = dzi.FindInt("//d:Size/@Width");
                    int height = dzi.FindInt("//d:Size/@Height");

                    int maxLevel = (int) Math.Ceiling(Math.Log(Math.Max(width, height), 2));

                    Bitmap finalImage = new Bitmap(width, height);
                    Graphics finalImageGraphics = Graphics.FromImage(finalImage);

                    int colCount = (int)Math.Ceiling((double)width / tileSize);
                    int rowCount = (int)Math.Ceiling((double)height / tileSize);
                    for (int row = 0; row < rowCount; row++)
                    {
                        for (int col = 0; col < colCount; col++)
                        {
                            String tileName = col + "_" + row + "." + format;
                            Uri tileUri = new Uri(filesDirectoryUri + "/" + maxLevel + "/" + tileName);
                            Stream imageStream = null;

                            try
                            {
                                imageStream = webClient.OpenRead(tileUri);
                                Bitmap tile = new Bitmap(imageStream);
                                float tileX = col * (tileSize);
                                float tileY = row * (tileSize);
                                finalImageGraphics.DrawImage(tile, tileX, tileY);
                            }
                            catch (WebException)
                            {   
                                Log.Warning("Could not find tile {0}, {1} for DZI {2}. " +
                                    "The image for this tile may be incomplete.", col, row, dziUri);
                                throw;
                            }
                            finally
                            {
                                if (imageStream != null) imageStream.Close();
                            }
                        }
                    }

                    finalImage.Save(finalImagePath, StandardImageFormat);
                    return finalImagePath;
                }
            }
            catch (XPathException e)
            {
                throw new PauthorException("Unable to decode DZI: " + dziUri, e);
            }
            catch (Exception e)
            {
                throw new PauthorException("Unable to un-deepzoom DZI: " + dziUri, e);
            }
        }

        private String DetermineNamespace(String xmlData)
        {
            String xmlnsTag = "xmlns=\"";
            int startIndex = xmlData.IndexOf(xmlnsTag);
            if (startIndex == -1) return null;

            startIndex += xmlnsTag.Length;
            int endIndex = xmlData.IndexOf('"', startIndex);
            if (endIndex == -1) return null;

            String xmlns = xmlData.Substring(startIndex, endIndex - startIndex);
            return xmlns;
        }
    }
}
